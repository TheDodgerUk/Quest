using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
public  class CursorState : Singleton<CursorState>
{
    public enum CursorStateEnum
    {
        None,
        Point,
        Hand,
        Orbit,
        Rotate,
    }
    Dictionary<CursorStateEnum, Texture2D> m_CursorDictionary = new Dictionary<CursorStateEnum, Texture2D>();
    private CursorStateEnum m_CurrentCursorState = CursorStateEnum.Point;
    private CursorStateEnum m_LoopCursorState = CursorStateEnum.Point;


    protected override void Construct()
    {
        m_CursorDictionary.Add(CursorStateEnum.Point, Resources.Load<Texture2D>("PointCursor"));
        m_CursorDictionary.Add(CursorStateEnum.Hand, Resources.Load<Texture2D>("HandCursor"));
        m_CursorDictionary.Add(CursorStateEnum.Orbit, Resources.Load<Texture2D>("OrbitCursor"));
        m_CursorDictionary.Add(CursorStateEnum.Rotate, Resources.Load<Texture2D>("RotateCursor"));

        foreach(var item in m_CursorDictionary)
        {
            if(item.Value == null)
            {
                Debug.LogError($"{item.Key} , the Texture could not be found");
            }
        }
        SetCursorState(CursorStateEnum.Point, true);
    }

    public void StartLoop()
    {
        m_LoopCursorState = CursorStateEnum.None;
    }

    public void EndLoop()
    {
        if (m_LoopCursorState == CursorState.CursorStateEnum.None)
        {
            SetCursorState(CursorState.CursorStateEnum.Point);
        }
    }

    public void SetCursorState(CursorStateEnum state, bool force = false)
    {
        m_LoopCursorState = state;
        if (m_CurrentCursorState != state || force == true)
        {
            m_CurrentCursorState = state;
            var item = m_CursorDictionary[m_CurrentCursorState];
            Cursor.SetCursor(item, Vector2.zero, CursorMode.ForceSoftware);
        }
    }

    public void SetCursorLockState()
    {
        if (m_CurrentCursorState == CursorStateEnum.Orbit)
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
        else
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }
}
