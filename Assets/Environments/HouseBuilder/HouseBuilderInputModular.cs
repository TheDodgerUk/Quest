using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static HouseBuilder.HouseBuilderConstants;

namespace HouseBuilder
{
    public class HouseBuilderInputModular : HouseBuilderInput
    {
        public static readonly Vector3 CLONE_PLACEMENT = new Vector3(1000, 1000, 1000);

        protected override string ClassName => nameof(HouseBuilderInputModular);

        protected override float MANIPULATE_HELD_TIME => 0.1f;

        private ModularMenu m_ModularMenu;
        private Interaction m_Original;
        private Interaction m_Clone;
        private Vector3 m_CameraPosition = Vector3.zero;
        private Quaternion m_CameraRotation = Quaternion.identity;

        private ContentModularMetaData m_ContentModularMetaData;

        public override void Initialise()
        {
            HouseBuilderRoot.Instance.OnModularModeSelected += OnSelectedItem;
        }

        private void OnSelectedItem(Interaction obj)
        {
            m_ModularMenu = HBGUIManager.Instance.ModularMenuRef;
            m_Original = obj;
            LoadSaveHouseContentsData.GenerateCataloguePrefab(Core.Mono, 0, obj.CatalogueEntryRef, null, null, GenerateCatalogueType.Duplicate, null, (justAddedItem) =>
            {
                m_ModularMenu.SetVisible(true);
                m_Clone = justAddedItem.InteractionRef;
                HouseBuilderConstants.CopyData(m_Original.CatalogueGeneratedDataRef, m_Clone.CatalogueGeneratedDataRef);
                m_Clone.RootTransform.transform.position = CLONE_PLACEMENT;
                m_Clone.RootTransform.transform.position += CLONE_PLACEMENT - m_Clone.RealWorldBoundsCentre();
                m_CameraPosition = Camera.main.transform.position;
                m_CameraRotation = Camera.main.transform.rotation;
                CameraFocus();
                m_ContentModularMetaData = m_Clone.CatalogueEntryRef.GetMetaData<ContentModularMetaData>();
                m_ModularMenu.ShowList(m_Clone);
            });

        }


        public override void CleanUp()
        {
            m_ModularMenu = HBGUIManager.Instance.ModularMenuRef;
            m_ModularMenu.SetVisible(false);
            if (m_Original != null && m_Clone != null)
            {
                CopyModularData(m_Clone.CatalogueGeneratedDataRef, m_Original.CatalogueGeneratedDataRef);
                m_Original.ModularRef.Rebuild(m_Original.CatalogueGeneratedDataRef.m_JsonData.m_ModularData);
                Destroy(m_Clone.RootGameObject);
                m_Clone = null;
                Camera.main.transform.position = m_CameraPosition;
                Camera.main.transform.rotation = m_CameraRotation;
            }
        }

        void CameraFocus()
        {
            Vector3 pointOnside = m_Clone.RootTransform.position + new Vector3(m_Clone.RootTransform.localScale.x * 0.5f, 0.0f, m_Clone.RootTransform.localScale.z * 0.5f);
            float aspect = (float)Screen.width / (float)Screen.height;
            float maxDistance = (m_Clone.RootTransform.localScale.y * 0.5f) / Mathf.Tan(Mathf.Deg2Rad * (Camera.main.fieldOfView / aspect));
            Camera.main.transform.position = Vector3.MoveTowards(pointOnside, m_Clone.RootTransform.position + new Vector3(0,-1,0), -maxDistance);
            Camera.main.transform.LookAt(m_Clone.RootTransform.position);
        }

        private void CopyModularData(CatalogueGeneratedData source, CatalogueGeneratedData destination)
        {
            destination.m_JsonData.m_ModularData.m_SelectedChildIndexs.Clear();

            foreach (var item in source.CatalogueEntryRef.GetLoadedMetaData())
            {
                if (item.GetType() == typeof(ContentModularMetaData))
                {
                    if (destination.m_JsonData.m_ModularData == null)
                    {
                        destination.m_JsonData.m_ModularData = new ModularData();
                    }
                    var dataString = Json.FullSerialiser.WriteToText<ModularData>(source.m_JsonData.m_ModularData, true);
                    destination.m_JsonData.m_ModularData = Json.FullSerialiser.ReadFromText<ModularData>(dataString);
                }
            }
        }


        protected override void UpdateInputControlsFor_EarlyUpdate()
        {
        }

        protected override void UpdateInputControlsFor_Nothing()
        {
        }

        protected override void UpdateInputControlsFor_FigureOutCreatingOrManipulating()
        {
        }

        protected override void UpdateInputControlsFor_Creating()
        {
        }

        protected override void UpdateInputControlsFor_Manipulating()
        {
        }

        protected override void UpdateInputControlsFor_Add()
        {
        }

        protected override void UpdateInputControlsFor_ManipulatingOther()
        {
        }
    }
}
