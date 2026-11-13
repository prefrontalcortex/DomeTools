using System;
using UnityEngine;

namespace pfc.Fulldome
{
    /// <summary>
    /// Bridge to the Android system file picker (see DomeViewerFilePicker.java).
    /// The user picks one file, no storage permission is needed.
    /// </summary>
    public static class AndroidFilePicker
    {
        public static bool IsSupported =>
#if UNITY_ANDROID && !UNITY_EDITOR
            true;
#else
            false;
#endif

        private const string JavaClass = "com.prefrontalcortex.domeviewer.DomeViewerFilePicker";
        private static Receiver receiver;
        private static Action<string, string> onDone;
        private static Action<string> onFailed;

        private static Receiver GetReceiver()
        {
            if (receiver) return receiver;
            var go = new GameObject("AndroidFilePicker (callbacks)");
            UnityEngine.Object.DontDestroyOnLoad(go);
            receiver = go.AddComponent<Receiver>();
            return receiver;
        }

        /// <summary>Opens the picker. onPicked gets (content uri, display name); onFailed gets null when cancelled.</summary>
        public static void Pick(string[] mimeTypes, Action<string, string> onPicked, Action<string> onFailed)
        {
            // Wrapped explicitly: a bare string[] would be spread into the params array.
            Call("pick", onPicked, onFailed, new object[] {mimeTypes});
        }

        /// <summary>Copies a picked file into the app cache. onCopied gets (file path, display name).</summary>
        public static void CopyToCache(string uri, string name, Action<string, string> onCopied, Action<string> onFailed)
        {
            Call("copyToCache", onCopied, onFailed, new object[] {uri, name});
        }

        /// <summary>
        /// Streams a picked file from a local http server (no copy) and returns its url, or null.
        /// </summary>
        public static string Serve(string uri, string name)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                using var picker = new AndroidJavaClass(JavaClass);
                return picker.CallStatic<string>("serve", activity, uri, name);
            }
            catch (Exception e)
            {
                Debug.LogError($"AndroidFilePicker.serve failed: {e}");
                return null;
            }
#else
            return null;
#endif
        }

        // method(activity, [uri, name | mimeTypes], receiver, callback)
        private static void Call(string method, Action<string, string> done, Action<string> failed, object[] args)
        {
            onDone = done;
            onFailed = failed;
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                var target = GetReceiver().gameObject.name;
                using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                using var picker = new AndroidJavaClass(JavaClass);
                if (method == "pick")
                    picker.CallStatic(method, activity, target, nameof(Receiver.OnResult), (string[]) args[0]);
                else
                    picker.CallStatic(method, activity, (string) args[0], (string) args[1], target, nameof(Receiver.OnResult));
            }
            catch (Exception e)
            {
                Debug.LogError($"AndroidFilePicker.{method} failed: {e}");
                onDone = null;
                onFailed = null;
                failed?.Invoke(e.Message);
            }
#else
            failed?.Invoke("The file picker is only available on Android.");
#endif
        }

        private class Receiver : MonoBehaviour
        {
            // "ok|<value>|<display name>", "cancel|" or "error|<reason>"
            public void OnResult(string message)
            {
                var parts = message.Split(new[] {'|'}, 3);
                var done = onDone;
                var failed = onFailed;
                onDone = null;
                onFailed = null;
                if (parts[0] == "ok" && parts.Length == 3) done?.Invoke(parts[1], parts[2]);
                else failed?.Invoke(parts[0] == "cancel" ? null : parts.Length > 1 ? parts[1] : message);
            }
        }
    }
}
