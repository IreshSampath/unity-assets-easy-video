using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

namespace GAG.EasyVideo
{
    public class EasyVideoManager : MonoBehaviour
    {
        public static EasyVideoManager Instance;

        public static event Action<int> ActionVideoIndexChanged;
        public static void RaiseActionVideoIndexChanged(int index)
        { ActionVideoIndexChanged?.Invoke(index); }

        public static event Action<string> ActionVideoIdChanged;
        public static void RaiseActionVideoIdChanged(string id)
        { ActionVideoIdChanged?.Invoke(id); }

        public static event Action<int> CurrentVideoEnded;
        public static void RaiseCurrentVideoEnded(int videoType)
        { CurrentVideoEnded?.Invoke(videoType); }

        [Header("Config")]
        [SerializeField] string _configFile = "EasyVideo/video_config.json";
        VideoConfig _config;

        [Header("Video Transition")]
        [SerializeField] Animator _transition;
        [SerializeField] float _fadeDuration = 0.5f;

        [Header("Video Player")]
        [SerializeField] VideoPlayer _videoPlayer;

        [Header("Video Resolution")]
        [SerializeField] RenderTexture _videoRenderTexture;
        [SerializeField] TMP_InputField _textureWidth;
        [SerializeField] TMP_InputField _textureHeight;
        Coroutine _rotationRoutine;
        int _lastScreenWidth;
        int _lastScreenHeight;

        [Header("Folders (inside StreamingAssets)")]
        [SerializeField] string _idleFolder = "";
        [SerializeField] string _actionFolder = "";

        [Header("Action Videos")]
        [SerializeField] List<EasyVideoModel> _actionVideos = new();

        List<string> _idleVideoPaths = new();
        int _idleIndex;
        [SerializeField] bool _isIdleLoop = true;
        [SerializeField] bool _isGoIdle = true;
        bool _isIdleMode = true;



        void OnEnable()
        {
            ActionVideoIndexChanged += OnActionVideoIndexChanged;
            ActionVideoIdChanged += OnActionVideoIdChanged;
        }

        void OnDisable()
        {
            ActionVideoIndexChanged += OnActionVideoIndexChanged;
            ActionVideoIdChanged -= OnActionVideoIdChanged;
        }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            LoadConfig();
            LoadIdleVideos();

            _videoPlayer.loopPointReached += OnVideoFinished;
        }

        void Start()
        {
            // Prevents the screen from dimming or sleeping
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;

            SetResolutionAuto();
            SyncInputFieldsWithScreen();
            PlayIdle();
        }

        void LoadConfig()
        {
            string path = Path.Combine(Application.streamingAssetsPath, _configFile);

            if (!File.Exists(path))
            {
                Debug.LogError("❌ Video config not found: " + path);
                return;
            }

            string json = File.ReadAllText(path);

            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("❌ Video config is empty!");
                return;
            }

            _config = JsonUtility.FromJson<VideoConfig>(json);

            if (_config == null)
            {
                Debug.LogError("❌ Failed to parse VideoConfig JSON");
                Debug.Log(json);
                return;
            }

            if (_config.idle == null || _config.actions == null)
            {
                Debug.LogError("❌ Config sections missing (idle/actions)");
                return;
            }

            // ✅ SAFE TO APPLY
            _idleFolder = _config.idle.folder;
            _isIdleLoop = _config.idle.loop;
            _isGoIdle = _config.idle.returnAfterAction;

            _actionFolder = _config.actions.folder;
            _actionVideos = _config.actions.videos;

            Debug.Log("✅ Video config loaded successfully");
        }

        void Update()
        {
            HandleAutoRotation();
        }

        // ------------------------------------
        // LOAD IDLE VIDEOS
        // ------------------------------------
        void LoadIdleVideos()
        {
            _idleVideoPaths.Clear();

            string idlePath = Path.Combine(Application.streamingAssetsPath, _idleFolder);

            if (!Directory.Exists(idlePath))
            {
                Debug.LogError("Idle folder not found: " + idlePath);
                return;
            }

            var files = Directory.GetFiles(idlePath);
            foreach (var file in files)
            {
                if (file.EndsWith(".mp4") || file.EndsWith(".webm"))
                    _idleVideoPaths.Add(file);
            }
        }

        // ------------------------------------
        // IDLE LOOP
        // ------------------------------------
        void PlayIdle()
        {
            if (_idleVideoPaths.Count == 0) return;

            _isIdleMode = true;
            _idleIndex = 0;

            //AppManager.RaiseIdleChanged(_idleIndex);
            StartCoroutine(PlayUrlDelayed(_idleVideoPaths[_idleIndex], false));
        }

        void PlayNextIdle()
        {
            _idleIndex = (_idleIndex + 1) % _idleVideoPaths.Count;
            //AppManager.RaiseIdleChanged(_idleIndex);
            StartCoroutine(PlayUrlDelayed(_idleVideoPaths[_idleIndex], false));
        }

        void OnVideoFinished(VideoPlayer vp)
        {
            // ShowPanel(0);
            if (_isIdleMode)
            {
                RaiseCurrentVideoEnded(0);

                if (_isIdleLoop)
                {
                    PlayNextIdle();
                }
            }
            else
            {
                RaiseCurrentVideoEnded(1);   
                if (_isGoIdle)
                    ReturnToIdle();
            }
        }

        // ------------------------------------
        // ACTION VIDEOS (BUTTON)
        // ------------------------------------
        public void PlayActionByIndex(int index)
        {
            if (index < 0 || index >= _actionVideos.Count) return;

            _isIdleMode = false;

            var entry = _actionVideos[index];
            string path = BuildActionPath(entry.FileName);

            StartCoroutine(PlayUrlDelayed(path, entry.Loop));
        }

        public void PlayActionById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogWarning("PlayActionById called with empty id");
                return;
            }

            id = id.Trim();

            var entry = _actionVideos.Find(v => v != null && v.Id == id);

            if (entry == null)
            {
                Debug.LogWarning($"Video ID not found: '{id}' (count: {_actionVideos.Count})");
                // Debug all loaded IDs
                for (int i = 0; i < _actionVideos.Count; i++)
                    Debug.Log($"[{i}] Id='{_actionVideos[i]?.Id}' File='{_actionVideos[i]?.FileName}'");
                return;
            }

            _isIdleMode = false;
            StartCoroutine(PlayUrlDelayed(BuildActionPath(entry.FileName), entry.Loop));
        }

        public void ReturnToIdle()
        {
            _videoPlayer.Stop();
            PlayIdle();
        }

        // ------------------------------------
        // CORE PLAY
        // ------------------------------------
        IEnumerator PlayUrlDelayed(string path, bool loop)
        {
            ShowTransition(0);

            yield return new WaitForSeconds(_fadeDuration);

            PlayUrl(path, loop);
        }

        void PlayUrl(string path, bool loop)
        {
            _videoPlayer.Stop();
            _videoPlayer.source = VideoSource.Url;
            _videoPlayer.url = path;
            _videoPlayer.isLooping = loop;
            _videoPlayer.Play();
        }

        string BuildActionPath(string fileName)
        {
            return Path.Combine(Application.streamingAssetsPath, _actionFolder, fileName);
        }

        void ShowTransition(int panelIndex)
        {
            _transition.Play("Transition Animation");
        }

        // ------------------------------------
        // VIDEO RESOLUTION
        // ------------------------------------

        void HandleAutoRotation()
        {
            if (Screen.width == _lastScreenWidth &&
                Screen.height == _lastScreenHeight)
                return;

            if (_rotationRoutine != null)
                StopCoroutine(_rotationRoutine);

            _rotationRoutine = StartCoroutine(ApplyRotationDelayed());
        }

        IEnumerator ApplyRotationDelayed()
        {
            yield return new WaitForSeconds(0.2f);

            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;

            SetResolutionAuto();
        }

        public void SetResolutionManual()
        {
            if (!int.TryParse(_textureWidth.text, out int width)) return;
            if (!int.TryParse(_textureHeight.text, out int height)) return;

            ApplyRenderTextureResolution(width, height);
        }

        public void SetResolutionAuto()
        {
            int width = Screen.width;
            int height = Screen.height;

            ApplyRenderTextureResolution(width, height);
        }

        void ApplyRenderTextureResolution(int width, int height)
        {
            if (_videoRenderTexture == null)
            {
                Debug.LogError("RenderTexture is missing!");
                return;
            }

            // Stop video before changing RT
            _videoPlayer.Pause();

            // Release old RT
            _videoRenderTexture.Release();

            _videoRenderTexture.width = width;
            _videoRenderTexture.height = height;

            _videoRenderTexture.Create();

            // Reassign to VideoPlayer (important!)
            _videoPlayer.targetTexture = _videoRenderTexture;

            _videoPlayer.Play();

            Debug.Log($"RenderTexture resized to {width} x {height}");
            SyncInputFieldsWithScreen();
        }


        public void SyncInputFieldsWithScreen()
        {
            _textureWidth.text = Screen.width.ToString();
            _textureHeight.text = Screen.height.ToString();
        }



        public void ReduceResolution()
        {
            //For low-end devices
            int width = Screen.width / 2;
            int height = Screen.height / 2;
        }

        void OnActionVideoIndexChanged(int index)
        {
           PlayActionByIndex(index);
        }

        void OnActionVideoIdChanged(string id)
        {
            PlayActionById(id);
        }
    }
}