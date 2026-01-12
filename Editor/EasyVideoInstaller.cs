#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class EasyVideoInstaller
{
    const string PackageName = "com.ireshsampath.unity-assets.easy-video";
    const string SampleRelativePath = "Samples~/Sample 1/StreamingAssets/EasyVideo";
    const string TargetRelativePath = "Assets/StreamingAssets/EasyVideo";

    [MenuItem("Tools/GAG/EasyVideo/Install Sample StreamingAssets")]
    public static void InstallSampleStreamingAssets()
    {
        var source = Path.GetFullPath($"Packages/{PackageName}/{SampleRelativePath}");
        var target = Path.GetFullPath(TargetRelativePath);

        if (!Directory.Exists(source))
        {
            EditorUtility.DisplayDialog("EasyVideo",
                "Sample StreamingAssets not found.\n\nMake sure the sample exists in:\n" + source,
                "OK");
            return;
        }

        if (Directory.Exists(target))
        {
            bool overwrite = EditorUtility.DisplayDialog("EasyVideo",
                "Target already exists:\n" + target + "\n\nOverwrite it?",
                "Overwrite", "Cancel");

            if (!overwrite) return;

            Directory.Delete(target, true);
        }

        CopyDirectory(source, target);

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("EasyVideo", "Installed sample files to:\n" + target, "OK");
    }

    static void CopyDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var dest = Path.Combine(destDir, Path.GetFileName(file));
            File.Copy(file, dest, true);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            var dest = Path.Combine(destDir, Path.GetFileName(dir));
            CopyDirectory(dir, dest);
        }
    }
}
#endif
