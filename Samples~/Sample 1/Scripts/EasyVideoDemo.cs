using GAG.EasyVideo;
using UnityEngine;

public class EasyVideoDemo : MonoBehaviour
{
    [Header("Target Screen")]
    [SerializeField] int _screenIndex = 0;
    
    [Header("Resolution")]
    [SerializeField] EasyVideoResolutionHandler _resolutionHandler;
    
    // -------------------------------------------------
    // Resolution
    // -------------------------------------------------
    public void SetAutoResolution()
    {
        _resolutionHandler.SetResolutionAuto();
    }

    public void ReduceResolution()
    {
        _resolutionHandler.ReduceResolution(0.5f); // low-end tablets
    }

    public void SetManualResolution(int width, int height)
    {
        _resolutionHandler.SetResolutionManual(1280, 720);
    }
    // -------------------------------------------------
    // ACTIONS
    // -------------------------------------------------

    public void PlayActionById(string id)
    {
        if (EasyVideoManager.Instance == null)
            return;

        EasyVideoManager.Instance.PlayActionById(id, _screenIndex);
    }

    public void PlayActionByIndex(int index)
    {
        if (EasyVideoManager.Instance == null)
            return;

        // Simple helper for demo usage
        var actionId = $"v{index + 1}";
        EasyVideoManager.Instance.PlayActionById(actionId, _screenIndex);
    }

    // -------------------------------------------------
    // IDLE
    // -------------------------------------------------

    public void PlayIdle()
    {
        if (EasyVideoManager.Instance == null)
            return;

        EasyVideoManager.Instance.PlayIdleOnAllScreens();
    }
}