using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HouseBuilder
{
    public class Adaptive
    {
        private const int START_INDEX = 1;

        public ContentAdaptiveMetaData ContentAdaptiveMetaDataRef { get; private set; }
        private InteractionGameObject m_Interaction;

        public int[] m_ScaleData = new int[Enum.GetNames(typeof(Axis)).Length];



        public void GetAmount1And2(out int amount1, out int amount2)
        {
            amount1 = 1;
            amount2 = 1;
            var axis = ContentAdaptiveMetaDataRef.UnPackAxisField();
            switch (axis[0])
            {
                case Axis.X:
                    amount1 = m_ScaleData[(int)Axis.X];
                    break;
                case Axis.Y:
                    amount1 = m_ScaleData[(int)Axis.Y];
                    break;
                case Axis.Z:
                    amount1 = m_ScaleData[(int)Axis.Z];
                    break;
                default:
                    break;
            }

            if (axis.Count == 2)
            {
                switch (axis[1])
                {
                    case Axis.X:
                        amount2 = m_ScaleData[(int)Axis.X];
                        break;
                    case Axis.Y:
                        amount2 = m_ScaleData[(int)Axis.Y];
                        break;
                    case Axis.Z:
                        amount2 = m_ScaleData[(int)Axis.Z];
                        break;
                    default:
                        break;
                }
            }
        }

        public void Setup(InteractionGameObject inter, ContentAdaptiveMetaData data, HouseBuilderConstants.AdaptiveData scaleData)
        {
            m_Interaction = inter;
            ContentAdaptiveMetaDataRef = data;
            Modulate(scaleData.m_MainItemScale[(int)Axis.X], scaleData.m_MainItemScale[(int)Axis.Y]);
            m_ScaleData[(int)Axis.X] = scaleData.m_MainItemScale[(int)Axis.X];
            m_ScaleData[(int)Axis.Y] = scaleData.m_MainItemScale[(int)Axis.Y];
            m_ScaleData[(int)Axis.Z] = scaleData.m_MainItemScale[(int)Axis.Z];
        }


        public void Modulate(int amount1,  int amount2)
        {
            amount1 = Mathf.Clamp(amount1, 1, amount1);
            amount2 = Mathf.Clamp(amount2, 1, amount2);

             
            var axis = ContentAdaptiveMetaDataRef.UnPackAxisField();

            switch (axis[0])
            {
                case Axis.X:
                    m_ScaleData[(int)Axis.X] = amount1;
                    break;
                case Axis.Y:
                    m_ScaleData[(int)Axis.Y] = amount1;
                    break;
                case Axis.Z:
                    m_ScaleData[(int)Axis.Z] = amount1;
                    break;
                default:
                    break;
            }


            if (axis.Count == 2)
            {
                switch (axis[1])
                {
                    case Axis.X:
                        m_ScaleData[(int)Axis.X] = amount2;
                        break;
                    case Axis.Y:
                        m_ScaleData[(int)Axis.Y] = amount2;
                        break;
                    case Axis.Z:
                        m_ScaleData[(int)Axis.Z] = amount2;
                        break;
                    default:
                        break;
                }
            }


            CollectData();
            DeleteOldDuplicatesPart();


            if (axis.Count == 1)
            {
                CreateNewOneSingles(amount1);
            }
            else
            {
                CreateNewOneDouble(amount1, amount2);
            }
        }


        private void CollectData()
        {
            StringToGameObject(ref ContentAdaptiveMetaDataRef.m_NonModularSingle.m_NonModuleCentre, m_Interaction.RootGameObject);
            StringToGameObject(ref ContentAdaptiveMetaDataRef.m_NonModularSingle.m_NonModuleMinus, m_Interaction.RootGameObject);
            StringToGameObject(ref ContentAdaptiveMetaDataRef.m_NonModularSingle.m_NonModulePlus, m_Interaction.RootGameObject);


            StringToGameObject(ref ContentAdaptiveMetaDataRef.m_NonModularDouble.m_NonModule_NW, m_Interaction.RootGameObject);
            StringToGameObject(ref ContentAdaptiveMetaDataRef.m_NonModularDouble.m_NonModule_N, m_Interaction.RootGameObject);
            StringToGameObject(ref ContentAdaptiveMetaDataRef.m_NonModularDouble.m_NonModule_NE, m_Interaction.RootGameObject);
            StringToGameObject(ref ContentAdaptiveMetaDataRef.m_NonModularDouble.m_NonModule_E, m_Interaction.RootGameObject);
            StringToGameObject(ref ContentAdaptiveMetaDataRef.m_NonModularDouble.m_NonModule_SE, m_Interaction.RootGameObject);
            StringToGameObject(ref ContentAdaptiveMetaDataRef.m_NonModularDouble.m_NonModule_S, m_Interaction.RootGameObject);
            StringToGameObject(ref ContentAdaptiveMetaDataRef.m_NonModularDouble.m_NonModule_SW, m_Interaction.RootGameObject);
            StringToGameObject(ref ContentAdaptiveMetaDataRef.m_NonModularDouble.m_NonModule_W, m_Interaction.RootGameObject);
            StringToGameObject(ref ContentAdaptiveMetaDataRef.m_NonModularDouble.m_NonModuleCentre, m_Interaction.RootGameObject);
        }

        private void DeleteOldDuplicatesPart()
        {
            if (ContentAdaptiveMetaDataRef.UnPackAxisField().Count == 1)
            {
                DeleteOldDuplicates(ContentAdaptiveMetaDataRef.m_NonModularSingle.m_NonModuleMinus.m_GameObjectName);
                DeleteOldDuplicates(ContentAdaptiveMetaDataRef.m_NonModularSingle.m_NonModulePlus.m_GameObjectName);
                DeleteOldDuplicates(ContentAdaptiveMetaDataRef.m_NonModularSingle.m_NonModuleCentre.m_GameObjectName);
            }
            else
            {
                DeleteOldDuplicates(ContentAdaptiveMetaDataRef.m_NonModularDouble.m_NonModule_NW.m_GameObjectName);
                DeleteOldDuplicates(ContentAdaptiveMetaDataRef.m_NonModularDouble.m_NonModule_N.m_GameObjectName);
                DeleteOldDuplicates(ContentAdaptiveMetaDataRef.m_NonModularDouble.m_NonModule_NE.m_GameObjectName);
                DeleteOldDuplicates(ContentAdaptiveMetaDataRef.m_NonModularDouble.m_NonModule_E.m_GameObjectName);
                DeleteOldDuplicates(ContentAdaptiveMetaDataRef.m_NonModularDouble.m_NonModule_SE.m_GameObjectName);
                DeleteOldDuplicates(ContentAdaptiveMetaDataRef.m_NonModularDouble.m_NonModule_S.m_GameObjectName);
                DeleteOldDuplicates(ContentAdaptiveMetaDataRef.m_NonModularDouble.m_NonModule_SW.m_GameObjectName);
                DeleteOldDuplicates(ContentAdaptiveMetaDataRef.m_NonModularDouble.m_NonModule_W.m_GameObjectName);
                DeleteOldDuplicates(ContentAdaptiveMetaDataRef.m_NonModularDouble.m_NonModuleCentre.m_GameObjectName);
            }

        }

        private void DeleteOldDuplicates(string name)
        {
            var duplicates = m_Interaction.transform.SearchListWithIncrementNumber(name);
            if (duplicates.Count > 1)
            {
                duplicates.RemoveAt(0); // this means it will not delete the fist one, which is not a duplicate
                m_Interaction.DeInitialiseOutline();

                foreach (var item in duplicates)
                {
                    UnityEngine.Object.Destroy(item.gameObject);
                }
            }
        }

        private GameObject InstantiateInternal(ContentAdaptiveMetaData.Modular mod, int index)
        {
            var item = UnityEngine.Object.Instantiate(mod.m_GameObject);
            item.name = mod.m_GameObject.name + index;
            item.transform.SetParent(mod.m_GameObject.transform.parent);
            item.transform.ClearLocals();
            return item;
        }



        #region Single

        private void CreateNewOneSingles(int amount)
        {
            SingleMoveEnds(amount);
            SingleCreateAndMoveMiddle(amount);
            m_Interaction.RecalculateCollider();
        }

        private void SingleMoveEnds(int amount)
        {
            Vector3 partSize = ContentAdaptiveMetaDataRef.m_NonModularSingle.m_NonModuleCentre.m_ModulePartSize;

            var AxisList = ContentAdaptiveMetaDataRef.UnPackAxisField();

            Vector3 newVector = partSize.CreateVector3Axis(AxisList[0]);
            Vector3 newVectorHalf = newVector * 0.5f;

            var moveAmount = amount - START_INDEX;

            ContentAdaptiveMetaDataRef.m_NonModularSingle.m_NonModulePlus.m_GameObject.transform.localPosition = ContentAdaptiveMetaDataRef.m_NonModularSingle.m_NonModulePlus.m_Offset + (moveAmount * newVectorHalf);
            ContentAdaptiveMetaDataRef.m_NonModularSingle.m_NonModuleMinus.m_GameObject.transform.localPosition = ContentAdaptiveMetaDataRef.m_NonModularSingle.m_NonModuleMinus.m_Offset - (moveAmount * newVectorHalf);
        }


        private void SingleCreateAndMoveMiddle(int amount)
        {
            var minus = ContentAdaptiveMetaDataRef.m_NonModularSingle.m_NonModuleMinus.m_GameObject.transform.localPosition;

            Vector3 partSize = ContentAdaptiveMetaDataRef.m_NonModularSingle.m_NonModuleCentre.m_ModulePartSize;
            Vector3 newVectorX = new Vector3(partSize.x, 0, 0);


            Vector3 startPartSize = ContentAdaptiveMetaDataRef.m_NonModularSingle.m_NonModuleMinus.m_Offset;

            var axis = ContentAdaptiveMetaDataRef.UnPackAxisField();
            Vector3 startNewVector = startPartSize.CreateVector3AxisAbs(axis[0]);

            int count = 0;
            for (int x = 0; x < amount; x++)
            {

                GameObject item = null;
                if (count != 0)
                {
                    item = InstantiateInternal(ContentAdaptiveMetaDataRef.m_NonModularSingle.m_NonModuleCentre, count - START_INDEX);
                }
                else
                {
                    item = ContentAdaptiveMetaDataRef.m_NonModularSingle.m_NonModuleCentre.m_GameObject;
                }
                item.transform.localPosition = minus;
                item.transform.localPosition += (x * (newVectorX)) + startNewVector;

                count++;
            }
        }

        #endregion Single

        #region double
        private void CreateNewOneDouble(int amount1, int amount2)
        {
            DoubleMoveEnds(amount1, amount2);
            DoubleMoveCorners();
            DoubleCreateAndMoveSides(amount1, amount2);
            DoubleCreateAndMoveMiddle(amount1, amount2);
            m_Interaction.RecalculateCollider();
 
        }

        private void DoubleMoveEnds(int amount1, int amount2)
        {
            ContentAdaptiveMetaData.ModAxisDouble doubleRef = ContentAdaptiveMetaDataRef.m_NonModularDouble;

            Vector3 partSize = doubleRef.m_NonModuleCentre.m_ModulePartSize;
            Vector3 newVector = Vector3.zero;
            Vector3 newVectorHalf = Vector3.zero;

            var axisList = ContentAdaptiveMetaDataRef.UnPackAxisField();

            int amount = 1;
            for(int i = 0; i < axisList.Count; i++)
            {
                Axis axis = axisList[i];
                if(i == 0)
                {
                    amount = amount1;
                }
                else
                {
                    amount = amount2;
                }

                newVector = partSize.CreateVector3Axis(axis);

                newVectorHalf = newVector * 0.5f;
                var moveAmount = amount - START_INDEX;

                Vector3 startPosition = Vector3.zero;
                switch (axis)
                {
                    case Axis.X:
                        if (doubleRef.m_NonModule_W.m_GameObject != null) doubleRef.m_NonModule_W.m_GameObject.transform.localPosition = doubleRef.m_NonModule_W.m_Offset + (moveAmount * newVectorHalf);
                        if (doubleRef.m_NonModule_E.m_GameObject != null) doubleRef.m_NonModule_E.m_GameObject.transform.localPosition = doubleRef.m_NonModule_E.m_Offset - (moveAmount * newVectorHalf);
                        if (doubleRef.m_NonModule_E.m_GameObject != null) startPosition = doubleRef.m_NonModule_E.m_GameObject.transform.localPosition;
                        break;

                    case Axis.Y:
                    case Axis.Z:
                        if (doubleRef.m_NonModule_N.m_GameObject != null) doubleRef.m_NonModule_N.m_GameObject.transform.localPosition = doubleRef.m_NonModule_N.m_Offset + (moveAmount * newVectorHalf);
                        if (doubleRef.m_NonModule_S.m_GameObject != null) doubleRef.m_NonModule_S.m_GameObject.transform.localPosition = doubleRef.m_NonModule_S.m_Offset - (moveAmount * newVectorHalf);
                        if (doubleRef.m_NonModule_S.m_GameObject != null) startPosition = doubleRef.m_NonModule_S.m_GameObject.transform.localPosition;
                        break;
                }
            }
        }

        private void DoubleMoveCorners()
        {
            ContentAdaptiveMetaData.ModAxisDouble doubleRef = ContentAdaptiveMetaDataRef.m_NonModularDouble;

            if (doubleRef.m_NonModule_N.m_GameObject != null)
            {
                Vector3 n = doubleRef.m_NonModule_N.m_GameObject.transform.localPosition;
                Vector3 s = doubleRef.m_NonModule_S.m_GameObject.transform.localPosition;
                Vector3 w = doubleRef.m_NonModule_W.m_GameObject.transform.localPosition;
                Vector3 e = doubleRef.m_NonModule_E.m_GameObject.transform.localPosition;

                doubleRef.m_NonModule_NE.m_GameObject.transform.localPosition = new Vector3(e.x, n.y, 0);
                doubleRef.m_NonModule_NW.m_GameObject.transform.localPosition = new Vector3(w.x, n.y, 0);
                doubleRef.m_NonModule_SE.m_GameObject.transform.localPosition = new Vector3(e.x, s.y, 0);
                doubleRef.m_NonModule_SW.m_GameObject.transform.localPosition = new Vector3(w.x, s.y, 0);
            }
        }

        private void DoubleCreateAndMoveSides(int amount1, int amount2)
        {
            ContentAdaptiveMetaData.ModAxisDouble doubleRef = ContentAdaptiveMetaDataRef.m_NonModularDouble;

            if (doubleRef.m_NonModule_N.m_GameObject == null)
            {
                return;
            }
            Vector3 n = doubleRef.m_NonModule_N.m_GameObject.transform.localPosition;
            Vector3 s = doubleRef.m_NonModule_S.m_GameObject.transform.localPosition;
            Vector3 w = doubleRef.m_NonModule_W.m_GameObject.transform.localPosition;
            Vector3 e = doubleRef.m_NonModule_E.m_GameObject.transform.localPosition;

            Vector3 ne = doubleRef.m_NonModule_NE.m_GameObject.transform.localPosition;
            Vector3 se = doubleRef.m_NonModule_SE.m_GameObject.transform.localPosition;
            Vector3 sw = doubleRef.m_NonModule_SW.m_GameObject.transform.localPosition;
            Vector3 nw = doubleRef.m_NonModule_NW.m_GameObject.transform.localPosition;

            Vector3 partSize = doubleRef.m_NonModuleCentre.m_ModulePartSize;
            var axisList = ContentAdaptiveMetaDataRef.UnPackAxisField();
            Vector3 newVector = partSize.CreateVector3Axis(axisList[0]);
          
            GameObject lastItem = null;

            for (int i = 0; i < amount1; i++)
            {
                GameObject item = null;
                if (i == 0)
                {
                    Vector3 startNewVector = doubleRef.m_NonModule_NE.m_Offset.CreateVector3AxisAbs(axisList[0]);
                    item = doubleRef.m_NonModule_N.m_GameObject;
                    item.transform.localPosition = ne;
                    item.transform.localPosition += startNewVector;
                }
                else
                {
                    item = InstantiateInternal(doubleRef.m_NonModule_N, i - START_INDEX);
                    item.transform.localPosition = lastItem.transform.localPosition + newVector;
                }
                lastItem = item;
            }

            for (int i = 0; i < amount1; i++)
            {
                GameObject item = null;
                if (i == 0)
                {
                    Vector3 startNewVector = doubleRef.m_NonModule_SE.m_Offset.CreateVector3AxisAbs(axisList[0]);
                    item = doubleRef.m_NonModule_S.m_GameObject;
                    item.transform.localPosition = se;
                    item.transform.localPosition += startNewVector;
                }
                else
                {
                    item = InstantiateInternal(doubleRef.m_NonModule_S, i - START_INDEX);
                    item.transform.localPosition = lastItem.transform.localPosition + newVector;
                }
                lastItem = item;
            }

            
            newVector = partSize.CreateVector3Axis(axisList[1]);
            for (int i = 0; i < amount2; i++)
            {
                GameObject item = null;
                if (i == 0)
                {
                    Vector3 startNewVector = doubleRef.m_NonModule_SE.m_Offset.CreateVector3AxisAbs(axisList[1]);
                    item = doubleRef.m_NonModule_E.m_GameObject;
                    item.transform.localPosition = se;
                    item.transform.localPosition += startNewVector;
                }
                else
                {
                    item = InstantiateInternal(doubleRef.m_NonModule_E, i - START_INDEX);
                    item.transform.localPosition = lastItem.transform.localPosition + newVector;
                }
                lastItem = item;
            }

            for (int i = 0; i < amount2; i++)
            {
                GameObject item = null;
                if (i == 0)
                {
                    Vector3 startNewVector = doubleRef.m_NonModule_SW.m_Offset.CreateVector3AxisAbs(axisList[1]);
                    item = doubleRef.m_NonModule_W.m_GameObject;
                    item.transform.localPosition = sw;
                    item.transform.localPosition += startNewVector;
                }
                else
                {
                    item = InstantiateInternal(doubleRef.m_NonModule_W, i - START_INDEX);
                    item.transform.localPosition = lastItem.transform.localPosition + newVector;
                }
                lastItem = item;
            }

        }


        private void DoubleCreateAndMoveMiddle(int amount1, int amount2)
        {
            ContentAdaptiveMetaData.ModAxisDouble doubleRef = ContentAdaptiveMetaDataRef.m_NonModularDouble;
            var axisList = ContentAdaptiveMetaDataRef.UnPackAxisField();


            Vector3 ne = Vector3.zero;
            Vector3 se = Vector3.zero;
            Vector3 sw = Vector3.zero;

            if (doubleRef.m_NonModule_NE.m_GameObject != null)
            {
                ne = doubleRef.m_NonModule_NE.m_GameObject.transform.localPosition;
                se = doubleRef.m_NonModule_SE.m_GameObject.transform.localPosition;
                sw = doubleRef.m_NonModule_SW.m_GameObject.transform.localPosition;
            }
            else
            {
                float axis1 = (doubleRef.m_NonModuleCentre.m_ModulePartSize.GetAxis(axisList[0]) * (amount1 - 1)) / 2;
                float axis2 = (doubleRef.m_NonModuleCentre.m_ModulePartSize.GetAxis(axisList[1]) * (amount2 - 1)) / 2;

                se = se.SetAxis(axisList[0], -axis1);
                se = se.SetAxis(axisList[1], -axis2);
            }


            Vector3 partSize = doubleRef.m_NonModuleCentre.m_ModulePartSize;
            Vector3 newVector1 = partSize.CreateVector3Axis(axisList[0]);
            Vector3 newVector2 = partSize.CreateVector3Axis(axisList[1]);


            Vector3 startPartSize = doubleRef.m_NonModule_SW.m_Offset;
            Vector3 startNewVector1 = startPartSize.CreateVector3AxisAbs(axisList[0]);
            Vector3 startNewVector2 = startPartSize.CreateVector3AxisAbs(axisList[1]);

            int count = 0;
            for (int index1 = 0; index1 < amount1; index1++)
            {
                for (int index2 = 0; index2 < amount2; index2++)
                {
                    GameObject item = null;
                    if (count != 0)
                    {
                        item = InstantiateInternal(doubleRef.m_NonModuleCentre, count - START_INDEX);
                    }
                    else
                    {
                        item = doubleRef.m_NonModuleCentre.m_GameObject;
                    }
                    item.transform.localPosition = se;
                    item.transform.localPosition += (index2 * (newVector2)) + startNewVector2;
                    item.transform.localPosition += (index1 * (newVector1)) + startNewVector1;

                    count++;
                }
            }

        }

        #endregion double

        private void StringToGameObject(ref ContentAdaptiveMetaData.Modular obj, UnityEngine.GameObject root)
        {
            if (root == null)
            {
                UnityEngine.Debug.LogError($"PrefabManager.LoadPrefab came back as null");
                obj = null;
            }
            if (string.IsNullOrEmpty(obj.m_GameObjectName) == false)
            {
                UnityEngine.Transform trans = root.transform.Search(obj.m_GameObjectName);
                if (trans != null)
                {
                    obj.m_GameObject = trans.gameObject;
                    return;
                }
            }
        }


    }
}

