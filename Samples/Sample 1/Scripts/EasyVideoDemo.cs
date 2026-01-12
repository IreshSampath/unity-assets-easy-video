using GAG.EasyVideo;
using UnityEngine;

public class EasyVideoDemo : MonoBehaviour
{
    void OnEnable()
    {
        EasyVideoManager.CurrentVideoEnded += OnCurrentVideoEnded;
    }

    void OnDisable()
    {
        EasyVideoManager.CurrentVideoEnded -= OnCurrentVideoEnded;
    }

    void OnCurrentVideoEnded(int videoMode)
    {
        if (videoMode == 0)
        {
            Debug.Log("Current idle video ended.");
        }
        else
        {
            Debug.Log("Current action video ended.");
        }
    }

    public void PlayVideoByIndex(int index)
    {
        EasyVideoManager.RaiseActionVideoIndexChanged(index);
    }

    public void PlayVideoById(string id)
    {
        EasyVideoManager.RaiseActionVideoIdChanged(id);
    }
}
