using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HouseBuilder
{
    public class ShowRoomSettingsMenu : MonoBehaviour
    {
        public enum CameraType
        {
            Test,
            Real,
        }
        //private CurveLineRenderer m_CurveLineRenderer;
        //public List<CurveLineRenderer.SplineData> m_SplineData = new List<CurveLineRenderer.SplineData>();

        private int m_SplineIndex = 0;

        private string CAMERA_MODEL = "CameraModelRoot";



        private CanvasGroup m_Group;
        private bool m_IsGroupActive;

        private Button m_BtnPlay;
        private Button m_BtnPlayPath;
        private Button m_BtnReset;


        private Button m_SplineNext;
        private Button m_SplinePrev;
        private Button m_SplineMove;
        private Button m_SplineRotate;
        private Button m_SplineCreate;
        private Button m_SplineDelete;
        private Button m_SplineTest;

        private Text m_TxtNumber;
        private InputField m_InputTime;
        private InputField m_WaitTime;

        private GameObject m_CameraModel;

        private IEnumerator m_CameraMovement;
        private CameraType m_IsCameraMovementType = CameraType.Real;
        private Vector3 m_CameraPosition;
        private Quaternion m_CameraRotation;

        private void OnEnvironmentLoadingComplete()
        {
            Core.Environment.OnEnvironmentLoadingComplete -= OnEnvironmentLoadingComplete;
            m_CameraModel = CachedObjects.Instance.InstantiateCachedGameObject(CAMERA_MODEL);
            m_CameraModel.SetActive(false);

            GameObject line = new GameObject("m_CurveLineRenderer");
            //m_CurveLineRenderer = line.AddComponent<CurveLineRenderer>();
            //UpdateSplineVisuals();
        }

        private void OnDisable()
        {
            if (m_CameraModel != null)
            {
                m_CameraModel.SetActive(false);
                HouseBuilderRoot.Instance.TransformGizmoRef.ClearTargets();
            }
        }

        internal void Initialise()
        {
            m_Group = transform.GetComponent<CanvasGroup>(); //hidden by default
            m_IsGroupActive = false;


            m_BtnPlay = transform.SearchComponent<Button>("PlayButton");
            m_BtnPlay.onClick.AddListener(PlayButtonPressed);

            m_BtnPlayPath = transform.SearchComponent<Button>("PlayPathButton");
            m_BtnPlayPath.onClick.AddListener(PlayPathButtonPressed);

            m_BtnReset = transform.SearchComponent<Button>("ResetButton");
            m_BtnReset.onClick.AddListener(ResetButtonPressed);

            m_SplineTest = transform.SearchComponent<Button>("Test");
            //m_SplineTest.onClick.AddListener(SplineTest);

            ////////
            m_SplineNext = transform.SearchComponent<Button>("Next");
            //m_SplineNext.onClick.AddListener(SplineNext);

            m_SplinePrev = transform.SearchComponent<Button>("Prev");
            //m_SplinePrev.onClick.AddListener(SplinePrev);

            m_SplineMove = transform.SearchComponent<Button>("Move");
            m_SplineMove.onClick.AddListener(SplineMove);

            m_SplineRotate = transform.SearchComponent<Button>("Pivot");
            m_SplineRotate.onClick.AddListener(SplineRotate);

            m_SplineCreate = transform.SearchComponent<Button>("Create");
            ///m_SplineCreate.onClick.AddListener(SplineCreate);

            m_SplineDelete = transform.SearchComponent<Button>("Delete");
            //m_SplineDelete.onClick.AddListener(SplineDelete);

            m_TxtNumber = transform.SearchComponent<Text>("NumberText");
            m_TxtNumber.text = "-";

            //////m_InputTime = transform.SearchComponent<InputField>("Time");
            //////m_InputTime.onValueChanged.AddListener((data) => { SaveToSpline(m_SplineData[m_SplineIndex]); });

            //////m_WaitTime = transform.SearchComponent<InputField>("Wait");
            //////m_WaitTime.onValueChanged.AddListener((data) => { SaveToSpline(m_SplineData[m_SplineIndex]); });

            Core.Environment.OnEnvironmentLoadingComplete += OnEnvironmentLoadingComplete;
        }


        public void CancelCameraMovement()
        {
            if (m_CameraMovement != null)
            {
                StopCoroutine(m_CameraMovement);
                m_CameraMovement = null;

                if (m_IsCameraMovementType == CameraType.Real)
                {
                    m_CameraModel.SetActive(true);
                    Camera.main.transform.position = m_CameraPosition;
                    Camera.main.transform.rotation = m_CameraRotation;
                   // m_CurveLineRenderer.enabled = true;
                }
            }
        }
        ////private void SplineDelete()
        ////{
        ////    m_SplineData.RemoveAt(m_SplineIndex);
        ////    if (m_SplineData.Count > 0)
        ////    {
        ////        m_SplineIndex = m_SplineIndex.Wrap(m_SplineData);
        ////        AttachToSpline(m_SplineData[m_SplineIndex]);
        ////    }
        ////    UpdateSplineVisuals();
        ////}

        ////private void SplineCreate()
        ////{
        ////    CurveLineRenderer.SplineData node = new CurveLineRenderer.SplineData();
        ////    if (m_SplineData.Count > 0)
        ////    {
        ////        node.Position = m_SplineData.Last().Position;
        ////        node.FacingVector = m_SplineData.Last().FacingVector;
        ////        node.Position += node.FacingVector * 2;
        ////        SaveToSpline(m_SplineData[m_SplineIndex]);
        ////    }

        ////    m_SplineIndex = m_SplineData.Count;
        ////    m_SplineData.Add(node);
        ////    AttachToSpline(node);
        ////}


        private void SplineRotate()
        {
            HouseBuilderRoot.Instance.TransformGizmoRef.transformType = RuntimeGizmos.TransformType.Rotate;
        }
        private void SplineMove()
        {
            HouseBuilderRoot.Instance.TransformGizmoRef.transformType = RuntimeGizmos.TransformType.Move;
        }

        //private void SplinePrev()
        //{
        //    SaveToSpline(m_SplineData[m_SplineIndex]);
        //    m_SplineIndex = m_SplineIndex.WrapDecrement(m_SplineData);
        //    AttachToSpline(m_SplineData[m_SplineIndex]);
        //}

        //private void SplineNext()
        //{
        //    SaveToSpline(m_SplineData[m_SplineIndex]);
        //    m_SplineIndex = m_SplineIndex.WrapIncrement(m_SplineData);
        //    AttachToSpline(m_SplineData[m_SplineIndex]);
        //}

        //private void SplineTest()
        //{
        //    m_IsCameraMovementType = CameraType.Test;
        //    m_CameraMovement = CameraMovement(m_CameraModel);
        //    StartCoroutine(m_CameraMovement);
        //}

        //private void AttachToSpline(CurveLineRenderer.SplineData data)
        //{
        //    m_TxtNumber.text = m_SplineIndex.ToString();
        //    m_InputTime.SetTextWithoutNotify(data.Time.ToString("0.##"));
        //    m_WaitTime.SetTextWithoutNotify(data.Wait.ToString("0.##"));
        //    m_CameraModel.transform.position = data.Position;
        //    m_CameraModel.transform.forward = data.FacingVector;
        //    m_CameraModel.SetActive(true);

        //    Camera.main.gameObject.MoveToLookAtRealive(m_CameraModel);
        //    HouseBuilderRoot.Instance.TransformGizmoRef.ClearAndAddTarget(m_CameraModel.transform);
        //    HouseBuilderRoot.Instance.TransformGizmoRef.transformType = RuntimeGizmos.TransformType.Move;
        //    UpdateSplineVisuals();
        //}

        //private void SaveToSpline(CurveLineRenderer.SplineData data)
        //{
        //    data.Position = m_CameraModel.transform.position;
        //    data.FacingVector = m_CameraModel.transform.forward;
        //    data.Time = float.Parse(m_InputTime.text);
        //    data.Wait = float.Parse(m_WaitTime.text);
        //}

        //private void UpdateSplineVisuals()
        //{
        //    if (m_CurveLineRenderer == null)
        //    {
        //        return;
        //    }
        //    if (m_CurveLineRenderer.m_RunUpdate == true)
        //    {
        //        m_SplineNext.interactable = (m_SplineData.Count > 1);
        //        m_SplinePrev.interactable = (m_SplineData.Count > 1);
        //        m_SplineDelete.interactable = (m_SplineData.Count > 0);
        //        m_SplineTest.interactable = (m_SplineData.Count > 1);
        //        m_BtnPlayPath.interactable = (m_SplineData.Count > 1);
        //        m_InputTime.interactable = (m_SplineData.Count > 1);
        //        m_WaitTime.interactable = (m_SplineData.Count > 1);
        //    }
        //    else
        //    {
        //        m_SplineNext.interactable = false;
        //        m_SplinePrev.interactable = false;
        //        m_SplineDelete.interactable = false;
        //        m_SplineTest.interactable = false;
        //        m_BtnPlayPath.interactable = false;
        //        m_InputTime.interactable = false;
        //        m_WaitTime.interactable = false;
        //    }


        //    if (m_SplineData.Count > 0)
        //    {
        //        m_InputTime.interactable = ((m_SplineIndex + 1) < m_SplineData.Count);
        //    }
        //    if (m_SplineData.Count == 0)
        //    {
        //        HouseBuilderRoot.Instance.TransformGizmoRef.ClearTargets();
        //    }
        //    if (m_CameraMovement == null)
        //    {
        //        m_CurveLineRenderer.m_RawControlPoints = m_SplineData;
        //    }
        //    if (m_SplineData.Count == 0)
        //    {
        //        m_TxtNumber.text = "-";
        //        m_CameraModel.SetActive(false);
        //    }
        //}

        //public void ManualUpdate()
        //{
        //    UpdateSplineVisuals();
        //    if (m_CurveLineRenderer != null && m_CameraMovement == null && m_CurveLineRenderer.m_RunUpdate == true)
        //    {
        //        if (m_SplineData.Count > 0)
        //        {
        //            m_SplineData[m_SplineIndex].Position = m_CameraModel.transform.position;
        //            m_SplineData[m_SplineIndex].FacingVector = m_CameraModel.transform.forward;

        //        }
        //    }
        //}
        private void PlayButtonPressed()
        {
            HBGUIManager.Instance.m_ViewMode.SetVisible(false);
            HBGUIManager.Instance.CameraWidgetRef.SetActive(false);
            //SetVisible(false);
            HouseBuilderRoot.Instance.PlayAllSoundAndLighting(0);
            HouseBuilderRoot.Instance.TransformGizmoRef.ClearTargets();

        }

        private void PlayPathButtonPressed()
        {
            PlayButtonPressed();

            m_IsCameraMovementType = CameraType.Real;
            m_CameraPosition = Camera.main.transform.position;
            m_CameraRotation = Camera.main.transform.rotation;
           // m_CameraMovement = CameraMovement(Camera.main.gameObject);
           // StartCoroutine(m_CameraMovement);
        }

        ////private IEnumerator CameraMovement(GameObject camera)
        ////{
        ////    if (m_IsCameraMovementType == CameraType.Real)
        ////    {
        ////        HouseBuilderRoot.Instance.TransformGizmoRef.ClearTargets();
        ////        m_CameraModel.SetActive(false);
        ////        m_CurveLineRenderer.enabled = false;
        ////    }

        ////    m_CurveLineRenderer.m_RunUpdate = false;


        ////    // collect Vector3Curve curvePos
        ////    var pos = m_CurveLineRenderer.GetPositionPoints();
        ////    var positions = pos.SplitList(m_CurveLineRenderer.SegmentCount);
        ////    Vector3Curve curvePos = new Vector3Curve();
        ////    for (int i = 0; i < positions.Count; i++)
        ////    {
        ////        if (i == 0)  // its missing the start position
        ////        {
        ////            positions[i].Insert(0, m_SplineData[0].Position);
        ////        }
        ////        curvePos.Add(positions[i], m_SplineData[i].Time);
        ////    }


        ////    // collect Vector3Curve curveRot
        ////    var face = m_CurveLineRenderer.GetFacingVectorPoints();
        ////    var faces = face.SplitList(m_CurveLineRenderer.SegmentCount);
        ////    Vector3Curve curveRot = new Vector3Curve();
        ////    for (int i = 0; i < faces.Count; i++)
        ////    {
        ////        if (i == 0) // its missing the start facing
        ////        {
        ////            faces[i].Insert(0, m_SplineData[0].FacingVector);
        ////        }
        ////        curveRot.Add(faces[i], m_SplineData[i].Time);
        ////    }



        ////    int index = 0;
        ////    float timer = 0f;
        ////    float totalTime = 0f;
        ////    float maxTime = 0f;
        ////    bool compleated = false;

        ////    for (int i = 0; i < m_SplineData.Count - 1; i++)
        ////    {
        ////        maxTime += m_SplineData[i].Time;
        ////    }


        ////    while (index < m_SplineData.Count - 1)
        ////    {
        ////        timer = 0f;
        ////        totalTime = 0f;

        ////        for (int i = 0; i < index; i++)
        ////        {
        ////            timer += m_SplineData[i].Time;
        ////        }


        ////        for (int i = 0; i < index + 1; i++)
        ////        {
        ////            totalTime += m_SplineData[i].Time;
        ////        }



        ////        compleated = false;
        ////        StartCoroutine(PartMove(camera, curvePos, curveRot, index, timer, totalTime, maxTime, () =>
        ////        {
        ////            compleated = true;
        ////        }));
        ////        yield return new WaitUntil(() => compleated == true);
        ////        index++;
        ////    }


        ////    if (m_CameraMovement != null)
        ////    {
        ////        StopCoroutine(m_CameraMovement);
        ////        m_CameraMovement = null;
        ////        m_CurveLineRenderer.m_RunUpdate = true;

        ////        if (m_SplineData.Count != 0)
        ////        {
        ////            m_CameraModel.transform.forward = m_SplineData[m_SplineIndex].FacingVector;
        ////            m_CameraModel.transform.position = m_SplineData[m_SplineIndex].Position;
        ////        }
        ////    }
        ////}


        ////private IEnumerator PartMove(GameObject camera, Vector3Curve curvePos, Vector3Curve curveRot, int index, float timer, float totalTime, float maxTime, Action callback)
        ////{
        ////    float waitTimer = 0f;
        ////    while (waitTimer < m_SplineData[index].Wait)
        ////    {
        ////        waitTimer += Time.deltaTime;
        ////    }

        ////    while (timer < totalTime)
        ////    {

        ////        yield return new WaitForEndOfFrame();
        ////        timer += (Time.deltaTime / m_SplineData[index].Time);

        ////        camera.transform.position = curvePos.InterpPercentage(timer / maxTime);
        ////        camera.transform.forward = curveRot.InterpPercentage(timer / maxTime);
        ////    }
        ////    callback?.Invoke();
        ////}

        private void ResetButtonPressed()
        {
            HouseBuilderRoot.Instance.ResetAllSoundAndLighting();
        }

        ////public void SetVisible(bool state)
        ////{
        ////    m_IsGroupActive = state;
        ////    m_Group.VisibleAndInteractive(state);
        ////    if (m_CameraModel != null)
        ////    {
        ////        m_CameraModel.SetActive(state);
        ////        m_CurveLineRenderer.enabled = state;
        ////        m_CurveLineRenderer.m_RunUpdate = state;

        ////        if (m_SplineData.Count != 0)
        ////        {
        ////            m_CameraModel.transform.forward = m_SplineData[m_SplineIndex].FacingVector;
        ////            m_CameraModel.transform.position = m_SplineData[m_SplineIndex].Position;
        ////        }
        ////    }
        ////    if (m_CameraMovement != null)
        ////    {
        ////        StopCoroutine(m_CameraMovement);
        ////        m_CameraMovement = null;
        ////        m_CurveLineRenderer.m_RunUpdate = state;

        ////        if (m_IsCameraMovementType == CameraType.Real)
        ////        {
        ////            Camera.main.transform.position = m_CameraPosition;
        ////            Camera.main.transform.rotation = m_CameraRotation;
        ////        }
        ////    }

        ////}
    }
}