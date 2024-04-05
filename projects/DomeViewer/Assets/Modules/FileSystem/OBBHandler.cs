using UnityEngine;

namespace pfc.Fulldome
{

    public class OBBHandler : MonoBehaviour
    {   

        // Mediafiles need to be in this location on Android (via sideload or via deployment with Meta Quest Developer Hub):
        // sdcard/Android/oob/com.example.myapp/ as *.jpg / *.png / *.mp4

        void Start()
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            // Sourcepath OBB-Folder
            string obbFolderPath = "/sdcard/Android/obb/"+Application.identifier;

            // Targetpath on Android Device
            string destinationPath = "/sdcard/Download/DomeViewer";

            try
                {
                    System.IO.Directory.CreateDirectory(GetStoragePath());
                }
            catch (System.Exception e)
                {
                    Debug.LogError(e.Message);
                }
            
            try
            {
                // checkk obb folder existence
                if (Directory.Exists(obbFolderPath))
                {
                    // list all files in obb folder
                    string[] obbFiles = Directory.GetFiles(obbFolderPath);

                    // move all files
                    foreach (string obbFile in obbFiles)
                    {
                        // get filename
                        string fileName = Path.GetFileName(obbFile);

                        // move current file to target location
                        File.Move(obbFile, Path.Combine(destinationPath, fileName));
                    }

                    Debug.Log("All files are moved successfully.");
                }
                else
                {
                    Debug.LogError("OBB-Folder not found");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error moving OBB-Files: " + e.Message);
            }
            #else
            Debug.Log("Move obb files skipped");
            #endif
        }
        private string GetStoragePath()
            {
                string path = "/sdcard/Download/DomeViewer";


    #if UNITY_ANDROID && !UNITY_EDITOR
            // AndroidJavaClass Environment = new AndroidJavaClass("android.os.Environment");
            // AndroidJavaObject directory = Environment.CallStatic<AndroidJavaObject>("getExternalStoragePublicDirectory", "Pictures");
            // path = directory.Call<string>("getAbsolutePath");
            // path = System.IO.Path.Combine(path, "Oculus");
            // path = System.IO.Path.Combine(path, "Gallery");
    #else
                path = Application.persistentDataPath + "/DomeViewer";
    #endif

                return path;
            }
    }
}
