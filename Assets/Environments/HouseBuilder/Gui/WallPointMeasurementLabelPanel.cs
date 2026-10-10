using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HouseBuilder
{
    public class WallPointMeasurementLabelPanel : UserInterface
    {
        public enum MeasureType
        {
            Distance,
            Position,
        }

        public static WallPointMeasurementLabelPanel Instance;

        public static MeasureType measureTypeEnum = MeasureType.Distance;

        private UnityEngine.UI.Text m_LastCreatedMeasurementLabel;
        private UnityEngine.UI.Text m_MeasurementLabelPrefab;

        private PoolManagerLocalComponent<UnityEngine.UI.Text> m_LabelManager;


        private const float OFFSET = 35;

        public override void Initialise()
        {
            Instance = this;
            this.SetActive(true);
            SetTheChildrenCanvas();
            m_LabelManager = new PoolManagerLocalComponent<UnityEngine.UI.Text>(m_MeasurementLabelPrefab.gameObject, this.transform);
            this.GetComponentInChildren<CanvasGroup>().blocksRaycasts = false;
        }

        private void SetTheChildrenCanvas()
        {
            m_MeasurementLabelPrefab = transform.SearchComponent<UnityEngine.UI.Text>("MeasurementLabel");
            m_MeasurementLabelPrefab.SetActive(false);
        }

        public void ClearLabel()
        {
            m_LabelManager.DeSpawnAll();
        }

        public void CreateMeasurementLabel()
        {
            UnityEngine.UI.Text g = m_LabelManager.SpawnObject();
            g.gameObject.SetActive(true);
            g.transform.localScale = Vector3.one;
            g.transform.localPosition = Vector3.zero;
            g.color = Color.black;
            m_LastCreatedMeasurementLabel = g;
        }

        public void CalculateMeasurementLabelPosition(Vector3 startPoint, Vector3 endPoint)
        {
            CreateMeasurementLabel();
            Vector3 labelMeasurementPos = Vector3.Lerp(startPoint, endPoint, 0.5f);
            float distance = Vector3.Distance(startPoint, endPoint);
            float threshold = (startPoint.x - endPoint.x);

            Vector3 startPos = Camera.main.WorldToScreenPoint(startPoint);
            Vector3 endPos = Camera.main.WorldToScreenPoint(endPoint);

            float testangle = Vector3.Angle(Vector3.up, startPos - endPos);
            testangle -= 90;
            if (threshold > 0)
            {
                testangle = -testangle;
            }

            PlaceTheMeasurementLabel(labelMeasurementPos, distance, testangle);
        }

        private void PlaceTheMeasurementLabel(Vector3 mouseAsWorldPoint, float distance, float testangle)
        {
            mouseAsWorldPoint = Camera.main.WorldToScreenPoint(mouseAsWorldPoint);
            RectTransform recTransf = m_LastCreatedMeasurementLabel.GetComponent<RectTransform>();
            recTransf.position = mouseAsWorldPoint;
            recTransf.rotation = Quaternion.Euler(new Vector3(0, 0, testangle));
            recTransf.position += recTransf.up * OFFSET;
            SetLabelText(MeasurementManager.ConvertTheMeasure(distance));
        }

        public void FindThePosOfThePositionLabel(Vector3 startPoint)
        {
            CreateMeasurementLabel();
            PlaceThePositionLabel(startPoint);
        }

        private void PlaceThePositionLabel(Vector3 flagpos)
        {
            Vector3 mouseAsWorldPoint = Camera.main.WorldToScreenPoint(flagpos);
            RectTransform recTransf = m_LastCreatedMeasurementLabel.GetComponent<RectTransform>();
            recTransf.position = mouseAsWorldPoint;
            recTransf.rotation = Quaternion.identity;
            string xMeas = MeasurementManager.ConvertTheMeasure(flagpos.x);
            string zMeas = MeasurementManager.ConvertTheMeasure(flagpos.z);
            SetLabelText(xMeas + "," + zMeas);
        }

        private void SetLabelText(string measure)
        {
            m_LastCreatedMeasurementLabel.text = measure;
        }
    }
}