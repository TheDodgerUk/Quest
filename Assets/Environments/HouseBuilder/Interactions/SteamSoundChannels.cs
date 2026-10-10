using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// got details from here
//https://forum.unity.com/threads/how-to-play-sound-on-left-or-right-speaker-only.289112/
public class SteamSoundChannels : MonoBehaviour
{
    public enum PlayChannel
    {
        Left,
        Right,
        Both,
    }

    public PlayChannel m_PlayChannel = PlayChannel.Both;

    // Start is called before the first frame update
    void OnAudioFilterRead(float[] data, int channels)
    {
        switch (m_PlayChannel)
        {
            case PlayChannel.Left:
                for (int i = 0; i < data.Length; i += channels)
                {
                    data[i+1] = 0; // mute right channel
                }
                break;
            case PlayChannel.Right:
                for (int i = 0; i < data.Length; i += channels)
                {
                    data[i] = 0; // mute left channel
                }
                break;
            case PlayChannel.Both:
                break;
            default:
                break;
        }

    }
}
