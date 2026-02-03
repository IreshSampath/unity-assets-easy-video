using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace GAG.EasyVideo
{
    public class AndroidMediaStorageProvider : IStorageProvider
    {
        public string RootPath { get; private set; }
        public bool IsReady { get; private set; }

        const string RootFolderName = "EasyVideo";
        const string DefaultFolderName = "EasyVideo";

        public void Initialize(MonoBehaviour runner)
        {
            RootPath = Path.Combine(
                "/storage/emulated/0/Android/media",
                Application.identifier,
                RootFolderName
            );

            Debug.Log("[EasyVideo][Android] RootPath = " + RootPath);

            runner.StartCoroutine(EnsureInstalled());
        }

        IEnumerator EnsureInstalled()
        {
            if (!Directory.Exists(RootPath))
            {
                Directory.CreateDirectory(RootPath);
                Debug.Log("[EasyVideo][Android] Created RootPath");
                yield return CopyDefaultsFromStreamingAssets();
            }

            IsReady = true;
        }

        IEnumerator CopyDefaultsFromStreamingAssets()
        {
            string sourceRoot =
                Path.Combine(Application.streamingAssetsPath, DefaultFolderName);

            string indexPath = Path.Combine(sourceRoot, "index.txt");

            using var indexReq = UnityWebRequest.Get(indexPath);
            yield return indexReq.SendWebRequest();

            if (indexReq.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("[EasyVideo] Missing index.txt in StreamingAssets");
                yield break;
            }

            foreach (var line in indexReq.downloadHandler.text.Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string relative = line.Trim();
                string src = Path.Combine(sourceRoot, relative);
                string dst = Path.Combine(RootPath, relative);

                Directory.CreateDirectory(Path.GetDirectoryName(dst));

                using var fileReq = UnityWebRequest.Get(src);
                yield return fileReq.SendWebRequest();

                if (fileReq.result == UnityWebRequest.Result.Success)
                    File.WriteAllBytes(dst, fileReq.downloadHandler.data);
            }
        }

        public string GetVideoPath(string relativePath)
            => Path.Combine(RootPath, relativePath);

        public bool FileExists(string relativePath)
            => File.Exists(GetVideoPath(relativePath));
    }
}
