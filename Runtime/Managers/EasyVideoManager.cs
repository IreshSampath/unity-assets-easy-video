using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GAG.EasyVideo
{
    public class EasyVideoManager : MonoBehaviour
    {
        public static EasyVideoManager Instance;

        [SerializeField] List<EasyVideoPlayerHandler> _players;

        bool _idleAllowTransition;
        bool _actionAllowTransition;
        bool _isIdleMode = true;

        VideoConfig _config;
        IStorageProvider _storage;

        string _idleFolder;
        string _actionFolder;

        readonly List<string> _idleVideoPaths = new();
        int _idleIndex;

        readonly List<EasyVideoModel> _actionVideos = new();
        readonly Dictionary<string, EasyVideoModel> _actionById = new();

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

        void Start()
        {
            StartCoroutine(BootSequence());
        }

        IEnumerator BootSequence()
        {
            _storage = EasyVideoStorageFactory.Create();
            _storage.Initialize(this);

            yield return new WaitUntil(() => _storage.IsReady);

            if (!LoadConfig())
                yield break;

            LoadIdleVideos();
            BuildActionLookup();

            PlayIdle();
        }

        // --------------------------------------------------------------------
        // IDLE
        // --------------------------------------------------------------------
        
        public void PlayIdleOnAllScreens()
        {
            PlayIdle();
        }
        
        void PlayIdle()
        {
            if (_idleVideoPaths.Count == 0)
                return;

            _isIdleMode = true;
            _idleIndex = Mathf.Clamp(_idleIndex, 0, _idleVideoPaths.Count - 1);

            PlayOnAllPlayers(
                _idleVideoPaths[_idleIndex],
                _config.idle.loop
            );
        }

        void PlayOnAllPlayers(string path, bool loop)
        {
            foreach (var player in _players)
            {
                StartCoroutine(
                    player.Play(
                        path,
                        loop,
                        ShouldUseTransition()
                    )
                );
            }
        }

        // --------------------------------------------------------------------
        // ACTIONS
        // --------------------------------------------------------------------
        public void PlayActionById(string id, int screenIndex)
        {
            if (!_actionById.TryGetValue(id, out var entry))
            {
                Debug.LogWarning("[EasyVideo] Action ID not found: " + id);
                return;
            }

            if (screenIndex < 0 || screenIndex >= _players.Count)
            {
                Debug.LogWarning("[EasyVideo] Invalid screen index: " + screenIndex);
                return;
            }

            _isIdleMode = false;

            string path = BuildActionPath(entry.FileName);

            StartCoroutine(
                _players[screenIndex].Play(
                    path,
                    entry.Loop,
                    _actionAllowTransition
                )
            );
        }

        // --------------------------------------------------------------------
        // CONFIG
        // --------------------------------------------------------------------
        bool LoadConfig()
        {
            string configPath = _storage.GetVideoPath("video_config.json");
            if (!File.Exists(configPath))
                return false;

            _config = JsonUtility.FromJson<VideoConfig>(File.ReadAllText(configPath));

            _idleFolder = _config.idle.folder;
            _actionFolder = _config.actions.folder;

            _idleAllowTransition = _config.idle.allowTransition;
            _actionAllowTransition = _config.actions.allowTransition;

            _actionVideos.Clear();
            _actionVideos.AddRange(_config.actions.videos);

            return true;
        }

        void LoadIdleVideos()
        {
            _idleVideoPaths.Clear();

            string fullPath = _storage.GetVideoPath(_idleFolder);
            if (!Directory.Exists(fullPath))
                return;

            foreach (var file in Directory.GetFiles(fullPath))
            {
                if (file.EndsWith(".mp4") || file.EndsWith(".webm"))
                    _idleVideoPaths.Add(file);
            }

            _idleVideoPaths.Sort();
        }

        void BuildActionLookup()
        {
            _actionById.Clear();

            foreach (var v in _actionVideos)
            {
                if (!string.IsNullOrEmpty(v.Id))
                    _actionById[v.Id] = v;
            }
        }

        string BuildActionPath(string fileName)
        {
            return _storage.GetVideoPath(
                Path.Combine(_actionFolder, fileName)
            );
        }

        bool ShouldUseTransition()
        {
            return _isIdleMode
                ? _idleAllowTransition
                : _actionAllowTransition;
        }
    }
}
