using UnityEngine;
using UnityEngine.Video;

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
            //Debug.Log("playing audio");
            gameObject.GetComponent<AudioSource>().Play();
        }
        else 
        {
            //Debug.Log("pausing audio");
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
                //Debug.Log("pausing video");
                vp.Pause();
            }
            else
            {
                //Debug.Log("playing video");
                vp.Play();
            }
        }
        else
        {
            // something that involves waiting
            //Debug.Log("cannot play video");
        }
    }
}
