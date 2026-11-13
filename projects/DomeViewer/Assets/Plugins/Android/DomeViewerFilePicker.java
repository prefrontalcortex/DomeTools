package com.prefrontalcortex.domeviewer;

import android.app.Activity;
import android.app.Fragment;
import android.content.Intent;
import android.database.Cursor;
import android.net.Uri;
import android.os.Bundle;
import android.os.ParcelFileDescriptor;
import android.provider.OpenableColumns;
import android.util.Log;

import com.unity3d.player.UnityPlayer;

import android.content.Context;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.net.InetAddress;
import java.net.ServerSocket;
import java.net.Socket;
import java.nio.charset.StandardCharsets;
import java.util.Locale;
import java.util.UUID;

/**
 * Opens the Android system file picker (Storage Access Framework) and reports the chosen
 * file back to Unity. Needs no storage permission: the user grants access to the one file
 * they pick.
 *
 * Messages sent to Unity are "ok|<value>|<display name>", "cancel|" or "error|<reason>".
 */
public class DomeViewerFilePicker extends Fragment {
    private static final String TAG = "DomeViewerFilePicker";
    private static final int REQUEST_PICK = 4711;

    private String receiver;
    private String method;
    private String[] mimeTypes;

    public static void pick(final Activity activity, final String receiver, final String method,
                            final String[] mimeTypes) {
        activity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                DomeViewerFilePicker fragment = new DomeViewerFilePicker();
                fragment.receiver = receiver;
                fragment.method = method;
                fragment.mimeTypes = mimeTypes;
                activity.getFragmentManager().beginTransaction()
                        .add(fragment, TAG).commitAllowingStateLoss();
            }
        });
    }

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        if (receiver == null) {
            // Recreated by the system without its arguments: nothing to report to.
            removeSelf();
            return;
        }
        Intent intent = new Intent(Intent.ACTION_OPEN_DOCUMENT);
        intent.addCategory(Intent.CATEGORY_OPENABLE);
        intent.setType("*/*");
        if (mimeTypes != null && mimeTypes.length > 0)
            intent.putExtra(Intent.EXTRA_MIME_TYPES, mimeTypes);
        try {
            startActivityForResult(intent, REQUEST_PICK);
        } catch (Exception e) {
            Log.e(TAG, "Could not open the file picker", e);
            send("error|" + e);
            removeSelf();
        }
    }

    @Override
    public void onActivityResult(int requestCode, int resultCode, Intent data) {
        if (requestCode != REQUEST_PICK) return;
        if (resultCode != Activity.RESULT_OK || data == null || data.getData() == null) {
            send("cancel|");
        } else {
            Uri uri = data.getData();
            try {
                getActivity().getContentResolver().takePersistableUriPermission(
                        uri, Intent.FLAG_GRANT_READ_URI_PERMISSION);
            } catch (Exception e) {
                Log.w(TAG, "No persistable permission for " + uri + ": " + e);
            }
            send("ok|" + uri + "|" + displayName(getActivity(), uri));
        }
        removeSelf();
    }

    private void removeSelf() {
        if (getActivity() == null) return;
        getActivity().getFragmentManager().beginTransaction().remove(this).commitAllowingStateLoss();
    }

    private void send(String message) {
        UnityPlayer.UnitySendMessage(receiver, method, message);
    }

    private static String displayName(Activity activity, Uri uri) {
        try (Cursor cursor = activity.getContentResolver().query(
                uri, new String[]{OpenableColumns.DISPLAY_NAME}, null, null, null)) {
            if (cursor != null && cursor.moveToFirst() && !cursor.isNull(0)) return cursor.getString(0);
        } catch (Exception e) {
            Log.w(TAG, "No display name for " + uri + ": " + e);
        }
        String last = uri.getLastPathSegment();
        return last != null ? last : "file";
    }

    // --- Local streaming -------------------------------------------------------------------
    // Unity's VideoPlayer cannot read content:// uris. Instead of copying a multi-gigabyte
    // file, a tiny HTTP server on 127.0.0.1 streams the picked file with range support.
    // A random token in the path keeps other apps on the device from reading it.

    private static ServerSocket server;
    private static volatile Uri servedUri;
    private static volatile String servedToken;
    private static volatile long servedSize;
    private static volatile String servedType;

    /** Returns an http://127.0.0.1 url that streams the picked file, or null on failure. */
    public static synchronized String serve(Activity activity, String uri, String name) {
        try {
            final Context context = activity.getApplicationContext();
            Uri parsed = Uri.parse(uri);
            long size;
            try (ParcelFileDescriptor pfd = context.getContentResolver().openFileDescriptor(parsed, "r")) {
                if (pfd == null) throw new java.io.IOException("cannot open " + uri);
                size = pfd.getStatSize();
            }
            if (size <= 0) throw new java.io.IOException("unknown size for " + uri);

            if (server == null) {
                final ServerSocket socket = new ServerSocket(0, 8, InetAddress.getByName("127.0.0.1"));
                server = socket;
                Thread acceptor = new Thread(new Runnable() {
                    @Override
                    public void run() {
                        while (!socket.isClosed()) {
                            try {
                                final Socket client = socket.accept();
                                Thread worker = new Thread(new Runnable() {
                                    @Override
                                    public void run() {
                                        handle(context, client);
                                    }
                                }, "DomeViewerStream");
                                worker.setDaemon(true);
                                worker.start();
                            } catch (Exception e) {
                                Log.w(TAG, "accept failed: " + e);
                            }
                        }
                    }
                }, "DomeViewerStreamServer");
                acceptor.setDaemon(true);
                acceptor.start();
            }

            String lower = name.toLowerCase(Locale.ROOT);
            String ext = lower.contains(".") ? lower.substring(lower.lastIndexOf('.')) : ".mp4";
            servedType = ext.equals(".mov") ? "video/quicktime" : "video/mp4";
            servedSize = size;
            servedUri = parsed;
            servedToken = UUID.randomUUID().toString();
            String url = "http://127.0.0.1:" + server.getLocalPort() + "/" + servedToken + "/video" + ext;
            return url;
        } catch (Exception e) {
            Log.e(TAG, "Could not serve " + uri, e);
            return null;
        }
    }

    private static void handle(Context context, Socket client) {
        try (Socket socket = client) {
            BufferedReader reader = new BufferedReader(
                    new InputStreamReader(socket.getInputStream(), StandardCharsets.ISO_8859_1));
            String requestLine = reader.readLine();
            if (requestLine == null) return;
            String range = null;
            String line;
            while ((line = reader.readLine()) != null && !line.isEmpty()) {
                if (line.regionMatches(true, 0, "Range:", 0, 6)) range = line.substring(6).trim();
            }
            String[] request = requestLine.split(" ");
            OutputStream out = socket.getOutputStream();
            Uri uri = servedUri;
            long size = servedSize;
            if (request.length < 2 || uri == null || !request[1].startsWith("/" + servedToken + "/")) {
                out.write("HTTP/1.1 403 Forbidden\r\nContent-Length: 0\r\nConnection: close\r\n\r\n"
                        .getBytes(StandardCharsets.ISO_8859_1));
                return;
            }

            long start = 0;
            long end = size - 1;
            boolean partial = false;
            if (range != null && range.startsWith("bytes=")) {
                String[] bounds = range.substring(6).split("-", 2);
                try {
                    if (!bounds[0].isEmpty()) start = Long.parseLong(bounds[0].trim());
                    if (bounds.length > 1 && !bounds[1].trim().isEmpty())
                        end = Math.min(end, Long.parseLong(bounds[1].trim()));
                    partial = true;
                } catch (NumberFormatException ignored) {
                    start = 0;
                    end = size - 1;
                }
            }
            if (start > end || start >= size) {
                out.write(("HTTP/1.1 416 Range Not Satisfiable\r\nContent-Range: bytes */" + size
                        + "\r\nContent-Length: 0\r\nConnection: close\r\n\r\n").getBytes(StandardCharsets.ISO_8859_1));
                return;
            }

            long length = end - start + 1;
            StringBuilder header = new StringBuilder();
            header.append(partial ? "HTTP/1.1 206 Partial Content\r\n" : "HTTP/1.1 200 OK\r\n");
            header.append("Content-Type: ").append(servedType).append("\r\n");
            header.append("Accept-Ranges: bytes\r\n");
            header.append("Content-Length: ").append(length).append("\r\n");
            if (partial)
                header.append("Content-Range: bytes ").append(start).append('-').append(end)
                        .append('/').append(size).append("\r\n");
            header.append("Connection: close\r\n\r\n");
            out.write(header.toString().getBytes(StandardCharsets.ISO_8859_1));
            if ("HEAD".equals(request[0])) return;

            try (ParcelFileDescriptor pfd = context.getContentResolver().openFileDescriptor(uri, "r");
                 FileInputStream in = new FileInputStream(pfd.getFileDescriptor())) {
                in.getChannel().position(start);
                byte[] buffer = new byte[256 * 1024];
                long remaining = length;
                while (remaining > 0) {
                    int read = in.read(buffer, 0, (int) Math.min(buffer.length, remaining));
                    if (read <= 0) break;
                    out.write(buffer, 0, read);
                    remaining -= read;
                }
            }
        } catch (Exception e) {
            // The player closes connections whenever it seeks: that is expected.
            Log.d(TAG, "stream connection ended: " + e);
        }
    }

    /** Copies the picked file into the app's cache (replacing earlier copies) on a background thread. */
    public static void copyToCache(final Activity activity, final String uri, final String name,
                                   final String receiver, final String method) {
        new Thread(new Runnable() {
            @Override
            public void run() {
                try {
                    File dir = new File(activity.getExternalCacheDir(), "picked");
                    File[] old = dir.listFiles();
                    if (old != null) for (File f : old) f.delete();
                    dir.mkdirs();
                    File target = new File(dir, new File(name).getName());
                    try (InputStream in = activity.getContentResolver().openInputStream(Uri.parse(uri));
                         OutputStream out = new FileOutputStream(target)) {
                        if (in == null) throw new java.io.IOException("no input stream");
                        byte[] buffer = new byte[1 << 20];
                        int read;
                        while ((read = in.read(buffer)) > 0) out.write(buffer, 0, read);
                    }
                    UnityPlayer.UnitySendMessage(receiver, method, "ok|" + target.getAbsolutePath() + "|" + name);
                } catch (Exception e) {
                    Log.e(TAG, "Could not copy " + uri, e);
                    UnityPlayer.UnitySendMessage(receiver, method, "error|" + e);
                }
            }
        }, "DomeViewerFileCopy").start();
    }
}
