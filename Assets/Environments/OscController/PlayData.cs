using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static OscController.DataContainer;
using static OscController.DataContainer.OscGroupData;
using static OscController.SequenceRunTimePanel;
using static UnityEngine.UI.Extensions.UILineRenderer;

namespace OscController
{

    public class PlayData
    {
        private PlayDataMono m_PlayDataMono;

        public PlayData(PlayDataMono playDataMono)
        {
            m_PlayDataMono = playDataMono;
            m_PlayDataMono.Initialise();
        }

        public bool IsPlayTimeLine() => m_PlayDataMono.IsPlayTimeLine();
        public void GetPlayTimeLine(Action<float> currentTime) => m_PlayDataMono.m_TimeDataTime += currentTime;
        public void PlayTimeLine() => m_PlayDataMono.PlayTimeLine();
        public void PlayTimeOnTimeLine(float time) => m_PlayDataMono.InternalPlayTimeLineValue(time, sendSingleItems: true);
        public void PlayTimeLineFastUntil(float time) => m_PlayDataMono.PlayTimeLineFastUntil(time);
        public void ResetTimeLine() => m_PlayDataMono.ResetTimeLine();
        public void StopTimeLine() => m_PlayDataMono.StopTimeLine();
        public void ClearAllDmx() => m_PlayDataMono.ClearAllDmx();

        public void ResetSequence() => m_PlayDataMono.ResetSequence();
        public void PlayCurrentSequence(Action<float> timeCallBack) => m_PlayDataMono.PlayCurrentSequence(timeCallBack);
        public void PlaySequence(SequenceRunTimeClass runTimeSequenceClass, Action<float> timeCallBack, Action callbackFinished) => m_PlayDataMono.PlaySequence(runTimeSequenceClass, timeCallBack, callbackFinished);
        public void PlaySequenceRepeat(SequenceRunTimeClass runTimeSequenceClass, Action partStart, Action finished) => m_PlayDataMono.PlaySequenceRepeat(runTimeSequenceClass, partStart, finished);

        public void StopSequenceRepeat(SequenceRunTimeClass runTimeSequenceClass) => m_PlayDataMono.StopSequenceRepeat(runTimeSequenceClass);
        public void ForceStopSequenceRepeat(OscSequenceData sequence, Action<float> timeCallBack, Action<SequenceRunTimeClass> runTimeSequenceClass) => m_PlayDataMono.ForceStopSequenceRepeat(sequence, timeCallBack, runTimeSequenceClass);

        public byte GetDmxValue(float timer, FixtureSliderData currentSliderData) => m_PlayDataMono.GetDmxValue(timer, currentSliderData);
    }

    public class PlayDataMono : MonoBehaviour
    {
        private List<byte> m_FullDmxData = new List<byte>();
        private IEnumerator m_PlayCoroutine;
        public Action<float> m_TimeDataTime;

        private class CoroutineData
        {
            public IEnumerator m_IEnumerator;
            public bool m_Loop = true;
            public SequenceRunTimeClass m_RunTimeSequenceClass;
        }

        private Dictionary<OscSequenceData, CoroutineData> m_SeperateCoroutine = new Dictionary<OscSequenceData, CoroutineData>();

        public void Initialise()
        {
            m_FullDmxData.Resize(OscControllerRoot.MAX_DMX_INDEX);
        }


        public void ResetSequence()
        {
            var sequence = DataContainer.Instance.CurrentSequenceData;
            if (m_PlayCoroutine != null)
            {
                Core.Mono.StopCoroutine(m_PlayCoroutine);
                m_PlayCoroutine = null;
            }
            InternalPlayAnimation(null, null, 0, sequence, ref m_FullDmxData, true);

        }

        public void PlayCurrentSequence(Action<float> timeCallback)
        {
            if (m_PlayCoroutine == null)
            {
                m_PlayCoroutine = InternalPlayCurrentSequence(timeCallback);
                Core.Mono.StartCoroutine(m_PlayCoroutine);
            }
        }

        public void PlaySequence(SequenceRunTimeClass runTimeSequenceClass, Action<float> timeCallback, Action callbackFinished)
        {
            Core.Mono.StartCoroutine(InternalPlaySequence(runTimeSequenceClass, timeCallback, callbackFinished));
        }

        public void PlaySequenceRepeat(SequenceRunTimeClass runTimeSequenceClass, Action partStart, Action finished)
        {
            if(m_SeperateCoroutine.ContainsKey(runTimeSequenceClass.m_OscSequenceData) == false)
            {
  
                CoroutineData newData = new CoroutineData();
                newData.m_Loop = true;
                newData.m_RunTimeSequenceClass = runTimeSequenceClass;
                newData.m_IEnumerator = InternalPlaySequenceRepeat(runTimeSequenceClass, newData, partStart, () =>
                {
                    m_SeperateCoroutine.Remove(runTimeSequenceClass.m_OscSequenceData);
                    finished?.Invoke();
                });
                m_SeperateCoroutine.Add(runTimeSequenceClass.m_OscSequenceData, newData);
                StartCoroutine(newData.m_IEnumerator);
                
            }            
        }

        public void StopSequenceRepeat(SequenceRunTimeClass runTimeSequenceClass)
        {
            if (m_SeperateCoroutine.ContainsKey(runTimeSequenceClass.m_OscSequenceData) == true)
            {
                m_SeperateCoroutine[runTimeSequenceClass.m_OscSequenceData].m_Loop = false;
            }
        }

        public void ForceStopSequenceRepeat(OscSequenceData sequence, Action<float> timeCallback, Action<SequenceRunTimeClass> runTimeSequenceClass)
        {
            SequenceRunTimeClass newData = null;
            if (m_SeperateCoroutine.ContainsKey(sequence) == true)
            {
                StopCoroutine(m_SeperateCoroutine[sequence].m_IEnumerator);
                m_SeperateCoroutine[sequence].m_Loop = false;
                newData = m_SeperateCoroutine[sequence].m_RunTimeSequenceClass;
            }
            m_SeperateCoroutine.Remove(sequence);
            runTimeSequenceClass?.Invoke(newData);
        }

        

        public void ResetTimeLine()
        {
            StopTimeLine();
            OscControllerRoot.Instance.PlayDataRef.ClearAllDmx();
            OscControllerRoot.Instance.TimeLinePanelRef.SetTimeLineValue(0f);
        }

        public void StopTimeLine()
        {
            if (m_PlayCoroutine != null)
            {
                Core.Mono.StopCoroutine(m_PlayCoroutine);
                m_PlayCoroutine = null;
            }
        }

        public void PlayTimeLineFastUntil(float untilTime)
        {
            if (m_PlayCoroutine != null)
            {
                Core.Mono.StopCoroutine(m_PlayCoroutine);
                m_PlayCoroutine = null;
            }

            m_PlayCoroutine = InternalPlayTimeLineFastUntil(untilTime);
            Core.Mono.StartCoroutine(m_PlayCoroutine);
        }

        public bool IsPlayTimeLine() => (m_PlayCoroutine != null);
        public void PlayTimeLine()
        {
            if (m_PlayCoroutine == null)
            {
                m_PlayCoroutine = InternalPlayTimeLine();
                Core.Mono.StartCoroutine(m_PlayCoroutine);
            }
        }

        private IEnumerator InternalPlayCurrentSequence(Action<float> timeCallback)
        {
            var sequence = DataContainer.Instance.CurrentSequenceData;
            yield return null;
            float time = 0;
            while (time < sequence.m_DurationTime)
            {
                InternalPlaySequenceValue(sequence, time);
                timeCallback?.Invoke(time);
                yield return new WaitForEndOfFrame();
                time += Time.deltaTime;

            }
            Core.Mono.StopCoroutine(m_PlayCoroutine);
            m_PlayCoroutine = null;
        }

        private IEnumerator InternalPlaySequence(SequenceRunTimeClass sequenceRunTimeClassOscSequenceData, Action<float> timeCallback, Action callbackFinished)
        {
            yield return null;
            float time = 0;
            while (time < sequenceRunTimeClassOscSequenceData.m_OscSequenceData.m_DurationTime)
            {
                InternalPlaySequenceValue(sequenceRunTimeClassOscSequenceData.m_OscSequenceData, time);
                timeCallback?.Invoke(time);
                yield return new WaitForEndOfFrame();
                time += (Time.deltaTime * sequenceRunTimeClassOscSequenceData.m_SequenceRunTimeGroupClass.m_GroupSpeedMultiplier);
            }

            // play the last part
            time = sequenceRunTimeClassOscSequenceData.m_OscSequenceData.m_DurationTime;
            InternalPlaySequenceValue(sequenceRunTimeClassOscSequenceData.m_OscSequenceData, time);
            timeCallback?.Invoke(time);


            callbackFinished?.Invoke();
        }

        private IEnumerator InternalPlaySequenceRepeat(SequenceRunTimeClass sequenceRunTimeClassOscSequenceData, CoroutineData newData, Action partStart, Action finished)
        {
            yield return null;           
            while (newData.m_Loop)
            {
                partStart?.Invoke();
                float time = 0;
                while (time < sequenceRunTimeClassOscSequenceData.m_OscSequenceData.m_DurationTime)
                {
                    InternalPlaySequenceValue(sequenceRunTimeClassOscSequenceData.m_OscSequenceData, time);
                    yield return new WaitForEndOfFrame();
                    time += (Time.deltaTime * sequenceRunTimeClassOscSequenceData.m_SequenceRunTimeGroupClass.m_GroupSpeedMultiplier);
                }
                time = sequenceRunTimeClassOscSequenceData.m_OscSequenceData.m_DurationTime;
                InternalPlaySequenceValue(sequenceRunTimeClassOscSequenceData.m_OscSequenceData, time);
            }
            finished?.Invoke();
        }




        private void InternalPlaySequenceValue(OscSequenceData sequence, float time)
        {
            InternalPlayAnimation(null, null, time, sequence, ref m_FullDmxData, sendSingleItems: true);
        }

        private IEnumerator InternalPlayTimeLine()
        {
            float value = OscControllerRoot.Instance.TimeLinePanelRef.GetTimeLineValue();
            yield return null;
            while (value < TimeLinePanel.MAX_SECONDS)
            {
                OscControllerRoot.Instance.TimeLinePanelRef.SetTimeLineValue(value);
                InternalPlayTimeLineValue(value, sendSingleItems :true);
                yield return new WaitForEndOfFrame();
                value += Time.deltaTime;
                m_TimeDataTime?.Invoke(value);
            }
            Core.Mono.StopCoroutine(m_PlayCoroutine);
            m_PlayCoroutine = null;
        }

        private IEnumerator InternalPlayTimeLineFastUntil(float endPoint)
        {
            for(int i = 0; i < m_FullDmxData.Count; i++)
            {
                m_FullDmxData[i] = 0;
            }
            float value = 0f;
            yield return null;
            while (value < endPoint)
            {
                InternalPlayTimeLineValue(value, sendSingleItems : false);
                value += (Time.deltaTime);
            }
            Core.Mono.StopCoroutine(m_PlayCoroutine);

            for (int i = 0; i < OscControllerRoot.MAX_DMX_VALUE; i++)
            {
                var dmxIndex = i + 1;
                PhotonDMX.Instance.SendDmxData(dmxIndex, (byte)m_FullDmxData[dmxIndex]);
            }

            m_PlayCoroutine = null;
        }

        public void InternalPlayTimeLineValue(float time, bool sendSingleItems)
        {
            foreach (var item in DataContainer.Instance.TimeLineDataRef)
            {
                if (item.m_IsSelected == true)
                {
                    foreach (var group in item.m_GroupLinkageData)
                    {
                        if ((time >= (group.m_StartTime - group.m_Sequence.m_LerpTime)) && (time <= (group.m_StartTime + group.m_Sequence.m_DurationTime)))
                        {
                            InternalPlayAnimation(item, group, time, group.m_Sequence, ref m_FullDmxData, sendSingleItems);
                        }
                    }
                }
            }
        }

        private void InternalPlayAnimation(OscGroupData groupData, GroupLinkageData linkageData, float time, OscSequenceData sequenceData, ref List<byte> fullData, bool sendSingleItems)
        {
            bool isLerping = false;
            float lerpNormPercentage = 0;
            float curveTime = time;
            if (linkageData != null)
            {
                curveTime = time - linkageData.m_StartTime;
                isLerping = ((time >= (linkageData.m_StartTime)) == false);
                float dif = time - (linkageData.m_StartTime - sequenceData.m_LerpTime);
                dif = Mathf.Abs(dif);
                if(isLerping == true)
                {
                    lerpNormPercentage = dif / sequenceData.m_LerpTime;
                    Debug.LogError(lerpNormPercentage);
                }
            }

          
            for (int j = 0; j < sequenceData.m_SliderData.Count; j++)
            {
                var currentSliderData = sequenceData.m_SliderData[j];
                
                int fullDmxIndex = currentSliderData.m_FixtureData.m_DmxStartIndex + currentSliderData.m_ChannelIndex;
                if (sequenceData.m_OptionsName != OscControllerRoot.OPTIONS_ALL && sequenceData.m_OptionsType == OscControllerRoot.OPTIONS_SINGLE)
                {
                    if (groupData != null)
                    {
                        fullDmxIndex = groupData.m_OptionsDmxStart + currentSliderData.m_ChannelIndex;
                    }

                }

                byte dmxValue = currentSliderData.m_DmxValue;
                if (currentSliderData.m_UseAnimationCurve == true)
                {
                    dmxValue = GetDmxValue(curveTime, currentSliderData);
                }
                if(isLerping == true)
                {
                    dmxValue = (byte)Mathf.Lerp((float)fullData[fullDmxIndex], (float)dmxValue, lerpNormPercentage);
                }

                if (sendSingleItems == true)
                {
                    if (fullData[fullDmxIndex] != dmxValue)
                    {
                        fullData[fullDmxIndex] = dmxValue;

                        SliderPanel sliderPanelRef = OscControllerRoot.Instance.SliderPanelRef;
                        var slider = sliderPanelRef.FindSliderPrefab(fullDmxIndex);

                        sliderPanelRef.DisplayVisualsAndSendSlideData(dmxValue, fullDmxIndex, slider);
                    }
                }
                else
                {
                    fullData[fullDmxIndex] = dmxValue;
                }
            }

        }

        public void ClearAllDmx()
        {

            for (int i = 1; i < m_FullDmxData.Count; i++)
            {
                var dmxIndex = i;
                m_FullDmxData[dmxIndex] = 0;
                if (PhotonDMX.Instance != null)
                {
                    PhotonDMX.Instance.SendDmxData(dmxIndex, (byte)m_FullDmxData[dmxIndex]);
                }
                
            }
        }

        public byte GetDmxValue(float timer, FixtureSliderData currentSliderData)
        {
            switch (currentSliderData.m_BezierType)
            {

                case BezierType.AnimationCurve:
                    return (byte)currentSliderData.m_AnimationCurveRepeated.Evaluate(timer);
                    break;
                case BezierType.Straight:
                    var frames = currentSliderData.m_AnimationCurveRepeated.keys;
                    int startIndex = 0;
                    for (int i = 0; i < frames.Length; i++)
                    {
                        if (frames[i].time <= timer)
                        {
                            startIndex = i;
                        }
                    }

                    if (startIndex != (frames.Length - 1)) // if not at end
                    {
                        float lerpPosition = Mathf.InverseLerp(frames[startIndex].time, frames[startIndex + 1].time, timer);
                        return (byte)Mathf.Lerp(frames[startIndex].value, frames[startIndex + 1].value, lerpPosition);
                    }
                    else
                    {
                        return (byte)frames[startIndex].value;
                    }
                    break;
                case BezierType.Square:
                    //Debug.LogError("Square");
                    var framesSquare = currentSliderData.m_AnimationCurveRepeated.keys;
                    int startIndexSquare = 0;
                    for (int i = 0; i < framesSquare.Length-1; i++) // not the last one 
                    {
                        if (framesSquare[i].time <= timer)
                        {
                            startIndexSquare = i;
                        }
                    }
                    return (byte)framesSquare[startIndexSquare].value;
                    break;
                default:
                    break;
            }
            return 0;
        }

    }
}
