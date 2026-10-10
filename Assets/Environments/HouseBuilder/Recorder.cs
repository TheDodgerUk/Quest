using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HouseBuilder.HouseBuilderConstants;

namespace HouseBuilder
{
    public class Recorder
    {
        private uint m_TotalTimesRecorded = 0;
        private const int AMOUNT_TO_RECORD = 50;

        public class CollectionData
        {
            public HouseContentsData m_HouseContentsData;
            public FloorCollectionData m_FloorCollectionData;
            public GeneralData m_GeneralData;
        }

        private List<CollectionData> m_CollectionData = new List<CollectionData>();
        private int m_CurrentIndex = 0;

        public bool RecordEnabled { get; set; }

        public Recorder(){ }

        public void Record(bool forceRecord = false)
        {
            if(RecordEnabled == false)
            {
                return;
            }
            HouseBuilderRoot.Instance.m_LoadSaveHouseContentsData.CollectData(HouseBuilderRoot.Instance.InputContents.ModeContainerRef.GetList, out HouseContentsData houseContentsData);
            HouseBuilderRoot.Instance.m_LoadSaveHouseWallsData.CollectData(HouseBuilderRoot.Instance.InputFloorPlan.ModeContainerRef.GetList, out FloorCollectionData floorCollectionData);
            HouseBuilderRoot.Instance.m_LoadSaveGeneralData.CollectData(out GeneralData data);

            bool recordNewData = IsNewDataToRecord(ref houseContentsData, ref floorCollectionData, ref data);
            if (m_CollectionData.Count == 0 || recordNewData == true || forceRecord == true)
            {
                FloorAndSkyboxManager.Instance.UpdateReflectionProbe();
                CollectionData newCollectionData = new CollectionData();
                newCollectionData.m_HouseContentsData = houseContentsData;
                newCollectionData.m_FloorCollectionData = floorCollectionData;
                newCollectionData.m_GeneralData = data;
                m_CollectionData.Add(newCollectionData);

                // for memory
                if (m_CollectionData.Count > AMOUNT_TO_RECORD)
                {
                    m_CollectionData.RemoveAt(0);
                }

                m_CurrentIndex = m_CollectionData.Count - 1;
                m_TotalTimesRecorded++;
                Debug.Log($"Current record place: {m_CollectionData.Count}, total times data has been recorded {m_TotalTimesRecorded}");
                PositionGrid.Instance.RegenerateHeight(true);
            }
        }

        private bool IsNewDataToRecord(ref HouseContentsData houseClone, ref FloorCollectionData floorClone, ref GeneralData generalClone)
        {
            string other1, other2, other3;
            CopyData(ref houseClone, ref floorClone, ref generalClone, out other1, out other2, out other3);

            var combinedHash = GlobalConsts.GetChecksumFromString(other1+ other2/* + other3*/); // not others3 as that wont effect sound
            SteamAudio.SteamAudioManager.GetSingleton().CurrentHashCode = combinedHash;

            if (m_CollectionData.Count == 0)
            {
                return false;
            }

            var lastEntry = m_CollectionData[m_CurrentIndex];

            var main1 = Json.FullSerialiser.WriteToText<HouseContentsData>(lastEntry.m_HouseContentsData, true);
            if (main1 != other1)
            {
                return true;
            }

            var main2 = Json.FullSerialiser.WriteToText<FloorCollectionData>(lastEntry.m_FloorCollectionData, true);
            if (main2 != other2)
            {
                return true;
            }

            var main3 = Json.FullSerialiser.WriteToText<GeneralData>(lastEntry.m_GeneralData, true);
            if (main3 != other3)
            {
                return true;
            }

            return false;
        }

        private static void CopyData(ref HouseContentsData houseClone, ref FloorCollectionData floorClone, ref GeneralData generalClone, out string other1, out string other2, out string other3)
        {
            var jsonSystem = Json.FullSerialiser;

            other1 = jsonSystem.WriteToText<HouseContentsData>(houseClone, true);
            other2 = jsonSystem.WriteToText<FloorCollectionData>(floorClone, true);
            other3 = jsonSystem.WriteToText<GeneralData>(generalClone, true);

            houseClone = new HouseContentsData();
            houseClone = jsonSystem.ReadFromText<HouseContentsData>(other1);

            floorClone = new FloorCollectionData();
            floorClone = jsonSystem.ReadFromText<FloorCollectionData>(other2);

            generalClone = new GeneralData();
            generalClone = jsonSystem.ReadFromText<GeneralData>(other3);
        }

        public void StepBack(Action callback)
        {
            if (m_CurrentIndex > 0)
            {
                m_CurrentIndex--;
                var collected = m_CollectionData[m_CurrentIndex];
                HouseBuilderRoot.Instance.Load(collected.m_HouseContentsData, collected.m_FloorCollectionData, collected.m_GeneralData, () =>
                {
                    callback?.Invoke();
                });
            }
            else
            {
                callback?.Invoke();
            }
        }

        public void StepForward(Action callback)
        {
            if (m_CurrentIndex < m_CollectionData.LastIndex())
            {
                m_CurrentIndex++;
                var collected = m_CollectionData[m_CurrentIndex];
                HouseBuilderRoot.Instance.Load(collected.m_HouseContentsData, collected.m_FloorCollectionData, collected.m_GeneralData, () =>
                {
                    callback?.Invoke();
                });
            }
            else
            {
                callback?.Invoke();
            }
        }

    }
}
