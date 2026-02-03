using System.Collections;
using System.IO;
using UnityEngine;

namespace EasyVideo
{
    /// <summary>
    /// Resolves external video storage paths and ensures
    /// default videos are installed only once.
    /// </summary>
    public class EasyVideoExternalStorageProvider : MonoBehaviour
    {
        public static EasyVideoExternalStorageProvider Instance { get; private set; }

        [Header("Root Folder Name")]
        [SerializeField] string _rootFolderName = "EasyVideo";

        [Header("Default Content Folder (inside StreamingAssets)")]
        [SerializeField] string _defaultContentFolder = "EasyVideo";

        public string RootPath { get; private set; }

        public bool IsReady { get; private set; }

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        IEnumerator Start()
        {
            ResolveRootPath();
            yield return EnsureInstalled();
            IsReady = true;
        }

        // --------------------------------------------------------------------

        void ResolveRootPath()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
    RootPath = Path.Combine(
        "/storage/emulated/0/Android/media",
        Application.identifier,
        _rootFolderName
    );
#elif UNITY_EDITOR
            RootPath = Path.Combine(
                Application.streamingAssetsPath,
                _rootFolderName
            );
#elif UNITY_STANDALONE
    RootPath = Path.Combine(
        Application.dataPath,
        "..",
        "_ExternalVideos",
        _rootFolderName
    );
#else
    RootPath = Path.Combine(
        Application.persistentDataPath,
        _rootFolderName
    );
#endif

            //#if UNITY_ANDROID && !UNITY_EDITOR
            //            RootPath = Path.Combine(
            //                "/storage/emulated/0/Android/media",
            //                Application.identifier,
            //                _rootFolderName
            //            );
            //#elif UNITY_STANDALONE || UNITY_EDITOR
            //            RootPath = Path.Combine(
            //                Application.dataPath,
            //                "..",
            //                "_ExternalVideos",
            //                _rootFolderName
            //            );
            //#else
            //            RootPath = Path.Combine(
            //                Application.persistentDataPath,
            //                _rootFolderName
            //            );
            //#endif
        }

        // --------------------------------------------------------------------

        IEnumerator EnsureInstalled()
        {
            if (Directory.Exists(RootPath))
                yield break;

            Debug.Log($"[EasyVideo] Installing default videos to:\n{RootPath}");

            Directory.CreateDirectory(RootPath);

            string sourceRoot = Path.Combine(
                Application.streamingAssetsPath,
                _defaultContentFolder
            );

#if UNITY_ANDROID && !UNITY_EDITOR
            yield return CopyStreamingAssetsAndroid(sourceRoot, RootPath);
#else
            CopyDirectoryRecursive(sourceRoot, RootPath);
#endif
        }

        // --------------------------------------------------------------------
        // ANDROID STREAMING ASSETS COPY
        // --------------------------------------------------------------------

#if UNITY_ANDROID && !UNITY_EDITOR
        IEnumerator CopyStreamingAssetsAndroid(string sourceRoot, string targetRoot)
        {
            string indexFile = Path.Combine(sourceRoot, "index.txt");

            using (UnityWebRequest indexReq = UnityWebRequest.Get(indexFile))
            {
                yield return indexReq.SendWebRequest();

                if (indexReq.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("[EasyVideo] index.txt missing in StreamingAssets");
                    yield break;
                }

                string[] files = indexReq.downloadHandler.text.Split('\n');

                foreach (string file in files)
                {
                    if (string.IsNullOrWhiteSpace(file))
                        continue;

                    string src = Path.Combine(sourceRoot, file.Trim());
                    string dst = Path.Combine(targetRoot, file.Trim());

                    Directory.CreateDirectory(Path.GetDirectoryName(dst));

                    using (UnityWebRequest fileReq = UnityWebRequest.Get(src))
                    {
                        yield return fileReq.SendWebRequest();

                        if (fileReq.result == UnityWebRequest.Result.Success)
                        {
                            File.WriteAllBytes(dst, fileReq.downloadHandler.data);
                        }
                        else
                        {
                            Debug.LogError($"[EasyVideo] Failed to copy {file}");
                        }
                    }
                }
            }
        }
#endif

        // --------------------------------------------------------------------
        // DESKTOP COPY
        // --------------------------------------------------------------------

        void CopyDirectoryRecursive(string source, string target)
        {
            Directory.CreateDirectory(target);

            foreach (var file in Directory.GetFiles(source))
            {
                string dst = Path.Combine(target, Path.GetFileName(file));
                File.Copy(file, dst, true);
            }

            foreach (var dir in Directory.GetDirectories(source))
            {
                CopyDirectoryRecursive(
                    dir,
                    Path.Combine(target, Path.GetFileName(dir))
                );
            }
        }

        // --------------------------------------------------------------------
        // PUBLIC API
        // --------------------------------------------------------------------

        public string GetVideoPath(string relativePath)
        {
            return Path.Combine(RootPath, relativePath);
        }

        public bool VideoExists(string relativePath)
        {
            return File.Exists(GetVideoPath(relativePath));
        }
    }
}
