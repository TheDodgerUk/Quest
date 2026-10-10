using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HouseBuilder
{
    public class Scale
    {
        public ContentScaleMetaData ContentScaleMetaDataRef { get; set; }
        private Transform m_TransformToScale;
        private InteractionGameObject m_Interaction;

        public void Setup(InteractionGameObject inter, ContentScaleMetaData data, HouseBuilderConstants.ScaleData scaleData)
        {
            m_Interaction = inter;
            m_TransformToScale = m_Interaction.gameObject.transform.Find(GlobalConsts.RootModel).transform;
            ContentScaleMetaDataRef = data;
            SetScale(scaleData.m_Scale);
        }

        public void SetScale(float scale)
        {
            Vector3 objScale = Vector3.one * scale;
            m_TransformToScale.SetScale(objScale.x, objScale.y, objScale.z);
            ContentScaleMetaDataRef.SetScale(objScale);
        }

        private void SetScale(Vector3 objScale)
        {
            m_TransformToScale.SetScale(objScale.x, objScale.y, objScale.z);
        }

        public void ResetScale()
        {
            ContentScaleMetaDataRef.SetScale (Vector3.one);
            m_TransformToScale.SetScale(Vector3.one.x, Vector3.one.y, Vector3.one.z);
        }

        public Vector3 GetScale() { return ContentScaleMetaDataRef.GetObjectScale(); }
    }

}
