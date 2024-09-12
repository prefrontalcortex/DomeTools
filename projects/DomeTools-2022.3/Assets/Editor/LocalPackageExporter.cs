using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace pfc.DomeTools
{
    public static class LocalPackageExporter
    {
        private static string PackageName = "com.pfc.dome-tools";

        [MenuItem("Dome Tools/Development/Export Local Package")]
        private static void ExportMenu()
        {
            ExportLocalPackage();
        }
        
        private static string GetRootPath()
        {
            return Path.GetFullPath(Path.Combine("Assets", "..", "..", "DomeTools-2022.3-Immutable", "Packages"));
        }
        
        private static async void ExportLocalPackage()
        {
            var rootPath = GetRootPath();
            var packageFolder = Path.GetFullPath("Packages/" + PackageName);
            
            // Rename Samples to Samples~ in packageFolder
            var samplesFolder = Path.Combine(packageFolder, "Samples");
            var samplesFolderNew = Path.Combine(packageFolder, "Samples~");
            var tempMetaFileLocation = Path.GetTempFileName();
            if (Directory.Exists(samplesFolder))
            {
                if (Directory.Exists(samplesFolderNew)) Directory.Delete(samplesFolderNew, true);
                Directory.Move(samplesFolder, samplesFolderNew);
                File.Move(samplesFolder + ".meta", tempMetaFileLocation);
            }
            
            var targetFolder = Path.GetFullPath(rootPath);
            Debug.Log("Packing " + packageFolder + " → " + targetFolder);
            var packRequest = Client.Pack(packageFolder, targetFolder);
            var prog = Progress.Start("Exporting Dome Tools Package", "Please wait...", Progress.Options.Indefinite);
            while (!packRequest.IsCompleted)
            {
                Progress.Report(prog, .5f);
                await Task.Yield();
            }
            if (packRequest.Status == StatusCode.Success)
            {
                Progress.Finish(prog, Progress.Status.Succeeded);
                var tarball = packRequest.Result.tarballPath;
                var target = Path.GetDirectoryName(tarball) + "/pfc-dome-tools.tgz";
                if (File.Exists(target)) File.Delete(target);
                File.Move(tarball, target);
                var fileSize = File.Exists(target) ? new FileInfo(target).Length : 0;
                var fileSizeMb = $"{fileSize / 1024f / 1024f:F1} MB";
                Debug.Log($"Success → {target} ({fileSizeMb})");
                EditorUtility.RevealInFinder(target); 
            }
            else
            {
                Progress.Finish(prog, Progress.Status.Failed);
                Debug.LogError("Failed to pack " + packageFolder + " → " + targetFolder);
            }
            
            // Rename Samples~ back to Samples in packageFolder
            if (Directory.Exists(samplesFolderNew))
            {
                if (Directory.Exists(samplesFolder)) Directory.Delete(samplesFolder, true);
                Directory.Move(samplesFolderNew, samplesFolder);
                File.Move(tempMetaFileLocation, samplesFolder + ".meta");
            }
        }
    }
}