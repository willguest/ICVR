using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// A simple script that demonstrates how to connect the DoubleClick function, which 
/// is usable by any interactable, to play and pause either audio or video streams.
/// Place this script as a component alongside whichever collider you wish to interact with.
/// </summary>
public class MediaPlayerBehaviour : MonoBehaviour
{
    public void OnDoubleClick()
    {
        if (GetComponent<AudioSource>()) ToggleAudio();
        if (GetComponent<VideoPlayer>()) ToggleVideo();
    }

    public void ToggleAudio()
    {
        if (!gameObject.GetComponent<AudioSource>().isPlaying)
        {
            gameObject.GetComponent<AudioSource>().Play();
        }
        else 
        {
            gameObject.GetComponent<AudioSource>().Pause();
        }
    }

    public void ToggleVideo()
    {
        VideoPlayer vp = gameObject.GetComponent<VideoPlayer>();

        if (vp != null && vp.isPrepared)
        {
            if (vp.isPlaying)
            {
                vp.Pause();
            }
            else
            {
                vp.Play();
            }
        }
        else
        {
            // do nothing, the video is not ready yet
        }
    }
}
