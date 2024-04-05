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

            // we declare a hardcoded location for meta quest / in case of other devices with different path or without sdcard we catch error

            string obbFolderPath = "/sdcard/Android/obb/"+Application.identifier;
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
                // check obb folder existence
                if (Directory.Exists(obbFolderPath))
                {
                    string[] obbFiles = Directory.GetFiles(obbFolderPath);

                    foreach (string obbFile in obbFiles)
                    {
                        string fileName = Path.GetFileName(obbFile);
                        File.Move(obbFile, Path.Combine(destinationPath, fileName));
                    }
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
#endif
        }
        private string GetStoragePath()
        {
            string path = "/sdcard/Download/DomeViewer";

#if UNITY_ANDROID && !UNITY_EDITOR
#else
            path = Application.persistentDataPath + "/DomeViewer";
#endif
            return path;
        }
    }
}
