using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HouseBuilder
{
    /// <summary>
    /// This Class should be called MeasurementLabelManager!
    /// </summary>
    public class MeasurementLabelPanel : UserInterface
    {
        public enum MeasureType
        {
            Distance,
            Position,
        }

        [SerializeField]
        private Vector3 m_Scale = new Vector3(0.005f, 0.005f, 0.005f);

        [SerializeField]
        private Vector2 m_LineWidth = new Vector2(0.025f, 0.025f);

        [SerializeField]
        private float m_MaxLineWidth = 0.05f;

        public static MeasurementLabelPanel Instance;

        public static MeasureType measureTypeEnum = MeasureType.Distance;

        private UnityEngine.UI.Text m_MeasurementLabelPrefab;
        private LineRenderer m_LineRendererPrefab;
        private PoolManagerLocalComponent<UnityEngine.UI.Text> m_LabelManager;  
        private PoolManagerLocalComponent<LineRenderer> m_LineRendererManager;

        public override void Initialise()
        {
            Instance = this;
            this.SetActive(true);
            SetTheChildrenCanvas();
            m_LabelManager = new PoolManagerLocalComponent<UnityEngine.UI.Text>(m_MeasurementLabelPrefab.gameObject, this.transform);
            this.GetComponentInChildren<CanvasGroup>().blocksRaycasts = false;

            m_LineRendererManager = new PoolManagerLocalComponent<LineRenderer>(m_LineRendererPrefab.gameObject, this.transform);
        }


        private void SetTheChildrenCanvas()
        {
            m_MeasurementLabelPrefab = transform.SearchComponent<UnityEngine.UI.Text>("MeasurementLabel");
            m_MeasurementLabelPrefab.SetActive(false);
            m_LineRendererPrefab = GetComponentInChildren<LineRenderer>();
            m_LineRendererPrefab.SetActive(false);

            m_LineRendererPrefab.textureMode = LineTextureMode.Tile;
            m_LineRendererPrefab.alignment = LineAlignment.TransformZ;
            m_LineRendererPrefab.useWorldSpace = true;
        }


        public void ClearLabel()
        {
            m_LabelManager.DeSpawnAll();
            m_LineRendererManager.DeSpawnAll();
        }


        public void PlaceTheMeasurementText(float amount, Vector3 placePoint, Transform trans, Vector3 lineFacing, float offset, bool useDot)
        {
            UnityEngine.UI.Text textItem = m_LabelManager.SpawnObject();

            offset += 0.002f; // 0.002 = 2mm    minor movement in front , to text will be in front of any lines drawn 
            textItem.color = Color.black;
            textItem.fontStyle = FontStyle.Bold;

            textItem.gameObject.SetActive(true);
            textItem.transform.localScale = m_Scale * Vector3.Distance(trans.position, Camera.main.transform.position);
            textItem.transform.position = placePoint;

            if (useDot)
            {
                Vector3 toCamera = trans.position - Camera.main.transform.position;
                float dot = Vector3.Dot(toCamera, trans.forward);

                if (dot < 0)
                {
                    textItem.transform.LookAt((textItem.transform.position + -lineFacing), Vector3.up);
                    textItem.transform.position += (trans.forward * offset);
                }
                else
                {
                    textItem.transform.LookAt((textItem.transform.position + lineFacing), Vector3.up);
                    textItem.transform.position += (-trans.forward * offset);
                }

            }
            else
            {
                // face camera
                textItem.transform.rotation = Camera.main.transform.rotation;
                textItem.transform.position += (-trans.forward * offset);
            }
            textItem.text = MeasurementManager.ConvertTheMeasure(amount);
        }


        public void PlaceTheMeasurementTextCutOut(Vector3 wall, Vector3 wallNext, Vector3 cutoutSide1, Vector3 cutoutSide2, Transform trans, Vector3 lineFacing, float offset)
        {
            wall.y = cutoutSide1.y;
            wallNext.y = cutoutSide1.y;


            Vector3 downSmallAmount = Vector3.zero;

            float distance1Side1 = Vector3.Distance(wall, cutoutSide1);
            float distance2Side1 = Vector3.Distance(wallNext, cutoutSide1);
            float distanceSide1 = Mathf.Min(distance1Side1, distance2Side1);
            if (distance1Side1 < distance2Side1)
            {
                //Debug.Log("cutoutSide1 1:  " + distanceSide1);
                Vector3 newPos = cutoutSide1 - (trans.transform.right * 0.2f) + downSmallAmount;
                PlaceTheMeasurementText(distanceSide1, newPos, trans, lineFacing, offset, true);
            }
            else
            {
                //Debug.Log("cutoutSide1 2:  " + distanceSide1);
                Vector3 newPos = cutoutSide1 - (trans.transform.right * 0.2f) + downSmallAmount;
                PlaceTheMeasurementText(distanceSide1, newPos, trans, lineFacing, offset, true);
            }


            float distance1Side2 = Vector3.Distance(wall, cutoutSide2);
            float distance2Side2 = Vector3.Distance(wallNext, cutoutSide2);
            float distanceSide2 = Mathf.Min(distance1Side2, distance2Side2);
            if (distance1Side2 > distance2Side2)
            {
                //Debug.Log("cutoutSide2 1:  " + distanceSide2);
                Vector3 newPos = cutoutSide2 + (trans.transform.right * 0.2f) + downSmallAmount;
                PlaceTheMeasurementText(distanceSide2, newPos, trans, lineFacing, offset, true);
            }
            else
            {
                //Debug.Log("cutoutSide2 2:  " + distanceSide2);
                Vector3 newPos = cutoutSide2 + (trans.transform.right * 0.2f) + downSmallAmount;
                PlaceTheMeasurementText(distanceSide2, newPos, trans, lineFacing, offset, true);
            }
        }


        public void PlaceLineRender(Vector3 start, Vector3 end, Transform trans, Vector3 lineFacing, float forwardOffset, bool useDot)
        {
            if (Camera.main != null)
            {
                LineRenderer lineRendererItem = m_LineRendererManager.SpawnObject();
                lineRendererItem.gameObject.SetActive(true);
                lineRendererItem.gameObject.transform.position = start;
                lineRendererItem.positionCount = 2;

                lineRendererItem.startWidth = m_LineWidth.x * Vector3.Distance(trans.position, Camera.main.transform.position);
                lineRendererItem.endWidth = m_LineWidth.y * Vector3.Distance(trans.position, Camera.main.transform.position);

                lineRendererItem.startWidth = lineRendererItem.startWidth.Clamp(0, m_MaxLineWidth);
                lineRendererItem.endWidth = lineRendererItem.endWidth.Clamp(0, m_MaxLineWidth);

                lineRendererItem.transform.forward = -lineFacing;
                lineRendererItem.transform.localScale = Vector3.one;
                lineRendererItem.transform.position += (trans.forward * forwardOffset);
                lineRendererItem.SetPosition(0, start + (trans.forward * forwardOffset));
                lineRendererItem.SetPosition(1, end + (trans.forward * forwardOffset));


                if (useDot == true)
                {
                    Vector3 toCamera = trans.position - Camera.main.transform.position;
                    float dot = Vector3.Dot(toCamera, trans.forward);
                    if (dot < 0)
                    {
                        lineRendererItem.transform.LookAt((lineRendererItem.transform.position + -lineFacing), Vector3.up);
                        lineRendererItem.transform.position += (trans.forward * forwardOffset);

                        lineRendererItem.SetPosition(0, start + (trans.forward * forwardOffset));
                        lineRendererItem.SetPosition(1, end + (trans.forward * forwardOffset));
                    }
                    else
                    {
                        lineRendererItem.transform.LookAt((lineRendererItem.transform.position + lineFacing), Vector3.up);
                        lineRendererItem.transform.position += (-trans.forward * forwardOffset);
                        lineRendererItem.SetPosition(0, start + (-trans.forward * forwardOffset));
                        lineRendererItem.SetPosition(1, end + (-trans.forward * forwardOffset));
                    }
                }
            }
        }


        public override void OpenInstant()
        {
            base.OpenInstant();
            if (m_PanelGroupLayout != null)
            {
                m_PanelGroupLayout.interactable = false;
                m_PanelGroupLayout.blocksRaycasts = false;
            }
        }


        public override void CloseInstant()
        {
            base.CloseInstant();
            if (m_PanelGroupLayout != null)
            {
                m_PanelGroupLayout.interactable = false;
                m_PanelGroupLayout.blocksRaycasts = false;
            }
        }
    }
}
