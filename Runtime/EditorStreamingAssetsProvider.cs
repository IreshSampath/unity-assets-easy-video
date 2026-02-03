using System.IO;
using UnityEngine;

namespace GAG.EasyVideo
{
    public class EditorStreamingAssetsProvider : IStorageProvider
    {
        public string RootPath { get; private set; }
        public bool IsReady { get; private set; }

        public void Initialize(MonoBehaviour runner)
        {
            RootPath = Path.Combine(
                Application.streamingAssetsPath,
                "EasyVideo"
            );

            IsReady = true;
        }

        public string GetVideoPath(string relativePath)
            => Path.Combine(RootPath, relativePath);

        public bool FileExists(string relativePath)
            => File.Exists(GetVideoPath(relativePath));
    }
}
