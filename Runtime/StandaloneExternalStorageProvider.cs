using GAG.EasyVideo;
using System.IO;
using UnityEngine;

namespace GAG.EasyVideo
{
    public class StandaloneExternalStorageProvider : IStorageProvider
    {
        //Windows / macOS / Linux
        public string RootPath { get; private set; }
        public bool IsReady { get; private set; }

        const string RootFolderName = "EasyVideo";
        const string DefaultFolderName = "EasyVideo";

        public void Initialize(MonoBehaviour runner)
        {
            RootPath = Path.Combine(
                Application.dataPath,
                "..",
                "_ExternalVideos",
                RootFolderName
            );

            if (!Directory.Exists(RootPath))
            {
                CopyDefaults();
            }

            IsReady = true;
        }

        void CopyDefaults()
        {
            string source =
                Path.Combine(Application.streamingAssetsPath, DefaultFolderName);

            CopyRecursive(source, RootPath);
        }

        void CopyRecursive(string src, string dst)
        {
            Directory.CreateDirectory(dst);

            foreach (var file in Directory.GetFiles(src))
                File.Copy(file, Path.Combine(dst, Path.GetFileName(file)), true);

            foreach (var dir in Directory.GetDirectories(src))
                CopyRecursive(dir, Path.Combine(dst, Path.GetFileName(dir)));
        }

        public string GetVideoPath(string relativePath)
            => Path.Combine(RootPath, relativePath);

        public bool FileExists(string relativePath)
            => File.Exists(GetVideoPath(relativePath));
    }
}
