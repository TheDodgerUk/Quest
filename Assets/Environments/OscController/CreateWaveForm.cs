using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateWaveForm
{
    public class WavePoolObject
    {
        public RawImage m_RawImage;
        public RectTransform m_RectTransform;
    }

    public static WavePoolObject CreateNewWave(GameObject prefab)
    {
        WavePoolObject newWave = new WavePoolObject();
        newWave.m_RawImage = prefab.GetComponent<RawImage>();
        newWave.m_RectTransform = prefab.GetComponent<RectTransform>();
        return newWave;
    }

    public static WavePoolObject onSpawn(WavePoolObject item)
    {
        item.m_RawImage.gameObject.SetActive(true);
        return item;
    }
    public static WavePoolObject onDeSpawn(WavePoolObject item)
    {
        item.m_RawImage.gameObject.SetActive(true);
        return item;
    }


    private AudioClip m_AudioClip;
    private float[] m_AllSamples;
    private float[] m_SelectedSamples;
    private readonly Color BLANK_COLOUR = new Color(1f, 1f, 1f, 1f);

    public  void CreateAndAssignSplitSeconds(List<WavePoolObject> waves, int secondsToCreate, int ChannelIndex, AudioClip clip, Color color, Color background, Vector2 paintWaveToThisSize, Action<bool> isBeingCreated, Action finished)
    {      
        if (clip == null)
        {
            waves.ForEach(e => e.m_RawImage.SetActive(false));
            return;
        }
        if (m_AudioClip == null || m_AudioClip != clip)
        {
            m_AudioClip = clip;
            isBeingCreated?.Invoke(true);
            Core.Mono.StartCoroutine(InternalCreateAndAssignSplitSeconds(waves, secondsToCreate, ChannelIndex, clip, color, background, paintWaveToThisSize, finished));
        }
        else
        {
            isBeingCreated?.Invoke(false);
            finished?.Invoke();
        }
    }

    private  IEnumerator InternalCreateAndAssignSplitSeconds(List<WavePoolObject> waves, int secondsToCreate, int ChannelIndex, AudioClip clip, Color color, Color background, Vector2 paintWaveToThisSize, Action finished)
    {
        yield return null;

        waves.ForEach(e => e.m_RawImage.SetActive(false));
        m_AllSamples = new float[clip.samples * clip.channels];
        clip.GetData(m_AllSamples, 0);

        m_SelectedSamples = GetChannelData(ChannelIndex, clip);

        RectTransform rect = waves[0].m_RectTransform;

        int numberToMake = (int)((clip.length) / secondsToCreate);
        numberToMake++; // add one to make sure
        yield return null;
        for (int i = 0; i < numberToMake; i++)
        {
            var wave = waves[i];
            wave.m_RawImage.SetActive(true);
            rect = wave.m_RectTransform;
            Vector2 oldSize = rect.sizeDelta;
            rect.sizeDelta = paintWaveToThisSize;

            wave.m_RawImage.color = BLANK_COLOUR;
            var texture2D = PaintWaveformSpectrum(ref m_SelectedSamples, clip, i * secondsToCreate, secondsToCreate, (int)rect.sizeDelta.x, (int)rect.sizeDelta.y, color, background);
            wave.m_RawImage.texture = texture2D;
            rect.sizeDelta = oldSize;
        }
        finished?.Invoke();
    }

    private  float[] GetChannelData(int ChannelIndex, AudioClip clip)
    {
        // loop though with offset for choice channel
        List<float> currentChannelSamples = new List<float>();
        for (int i = ChannelIndex; i < m_AllSamples.Length;)
        {
            currentChannelSamples.Add(m_AllSamples[i]);
            i += clip.channels;
        }
        float[] samples = currentChannelSamples.ToArray();
        return samples;
    }

    public  void CreateAndAssignPerSecond(ref float[] samplesTotal, AudioClip clip, Image image, int startSecond, Color color, Color background)
    {
        RectTransform rect = image.GetComponent<RectTransform>();
        image.color = BLANK_COLOUR;
        var texture2D = PaintWaveformSpectrum(ref samplesTotal, clip, startSecond, secondsToCreate: 1,(int)rect.sizeDelta.x, (int)rect.sizeDelta.y, color, background);
        if(texture2D != null)
        {
            var createdSprite = Sprite.Create((Texture2D)texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
            image.sprite = createdSprite;
        }
        else
        {
            image.SetActive(false);
        }

    }
    private Texture2D PaintWaveformSpectrum(ref float[] samplesTotal, AudioClip audio, int startSecond, int secondsToCreate, int width, int height, Color col, Color background)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        float[] waveform = new float[width];

        int packSize = ((audio.frequency / width)* secondsToCreate) + 1;
        int s = 0;

        int start = (audio.frequency * startSecond);
        int end = (audio.frequency * (startSecond + secondsToCreate));
        for (int i = start ; i < end; i += packSize)
        {
            if (i < samplesTotal.Length)
            {
                if (waveform.Length > s)
                {
                    waveform[s] = Mathf.Abs(samplesTotal[i]);
                    s++;
                }
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tex.SetPixel(x, y, background);
            }
        }

        for (int x = 0; x < waveform.Length; x++)
        {
            for (int y = 0; y <= waveform[x] * ((float)height * .75f); y++)
            {
                tex.SetPixel(x, (height / 2) + y, col);
                tex.SetPixel(x, (height / 2) - y, col);
            }
        }

        tex.Apply();

        return tex;

    }
}
