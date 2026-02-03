using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class EasyVideoResolutionHandler : MonoBehaviour
{
    [SerializeField] VideoPlayer _videoPlayer;
    [SerializeField] RenderTexture _videoRenderTexture;
    const string FIT_MODE_KEY = "fit_mode";
    
    int _lastScreenWidth;
    int _lastScreenHeight;
    Coroutine _rotationRoutine;

    
    public Action<int, int> OnResolutionChanged;
    
    void OnEnable()
    {
        CacheScreenSize();

        // ✅ Apply saved fit mode
        ApplyFitMode(LoadFitMode());
    }


    void Update()
    {
        if (!isActiveAndEnabled)
            return;

        HandleAutoRotation();
    }

    // ------------------------------------
    // AUTO ROTATION / RESOLUTION
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
        // Debounce (important on Android)
        yield return new WaitForSeconds(0.2f);

        CacheScreenSize();
        SetResolutionAuto();
    }

    void CacheScreenSize()
    {
        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;
    }

    void ApplyFitMode(EasyVideoFitMode mode)
    {
        switch (mode)
        {
            case EasyVideoFitMode.Fit_Inside:
                _videoPlayer.aspectRatio =
                    VideoAspectRatio.FitInside;
                break;

            case EasyVideoFitMode.Fill_Outside:
                _videoPlayer.aspectRatio =
                    VideoAspectRatio.FitOutside;
                break;

            case EasyVideoFitMode.Stretch:
                _videoPlayer.aspectRatio =
                    VideoAspectRatio.Stretch;
                break;
        }
        SaveFitMode(mode);
    }

    void SaveFitMode(EasyVideoFitMode mode)
    {
        PlayerPrefs.SetInt(FIT_MODE_KEY, (int)mode);
        PlayerPrefs.Save();
    }
    
    // ------------------------------------
    // PUBLIC API
    // ------------------------------------

    public EasyVideoFitMode LoadFitMode()
    {
        EasyVideoFitMode _fitMode;
        if (PlayerPrefs.HasKey(FIT_MODE_KEY))
            _fitMode = (EasyVideoFitMode)PlayerPrefs.GetInt(FIT_MODE_KEY);
        else
            _fitMode = EasyVideoFitMode.Fit_Inside;

        return _fitMode;
    }
    
    public void SetResolutionAuto()
    {
        ApplyRenderTextureResolution(Screen.width, Screen.height);
    }

    public void SetFitMode(EasyVideoFitMode mode)
    {
        ApplyFitMode(mode);     
    }
    
    public void SetResolutionManual(int width, int height)
    {
        ApplyRenderTextureResolution(width, height);
    }

    public void ReduceResolution(float scale = 0.5f)
    {
        int width = Mathf.RoundToInt(Screen.width * scale);
        int height = Mathf.RoundToInt(Screen.height * scale);

        ApplyRenderTextureResolution(width, height);
    }

    // ------------------------------------
    // CORE LOGIC
    // ------------------------------------

    void ApplyRenderTextureResolution(int width, int height)
    {
        if (width <= 0 || height <= 0)
            return;

        if (_videoRenderTexture == null || _videoPlayer == null)
            return;

        if (_videoRenderTexture.width == width &&
            _videoRenderTexture.height == height)
            return;

        bool wasPlaying = _videoPlayer.isPlaying;

        if (wasPlaying)
            _videoPlayer.Pause();

        _videoRenderTexture.Release();
        _videoRenderTexture.width = width;
        _videoRenderTexture.height = height;
        _videoRenderTexture.Create();

        _videoPlayer.targetTexture = _videoRenderTexture;

        if (wasPlaying)
            _videoPlayer.Play();

        // ✅ Notify UI
        OnResolutionChanged?.Invoke(width, height);

        Debug.Log($"🎥 RenderTexture resized → {width} x {height}");
    }

    // ------------------------------------
    // CLEANUP
    // ------------------------------------

    public void Release()
    {
        if (_videoRenderTexture != null)
            _videoRenderTexture.Release();
    }

    void OnDisable()
    {
        Release();
    }
}
