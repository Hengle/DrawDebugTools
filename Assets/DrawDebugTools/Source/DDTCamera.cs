using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DDTCamera : MonoBehaviour
{
    #region ========== Variables ==========
    private DrawDebugTools      m_DrawDebugTools;
    private bool                m_IsActive = true;

    #endregion
    #region ========== Functions ==========
    void Start()
    {        
    }
    private void OnPostRender()
    {
        if (m_IsActive)
            m_DrawDebugTools.Draw();
    }

    public void SetDrawDebugTools(DrawDebugTools Ddt)
    {
        m_DrawDebugTools = Ddt;
    }

    public void SetActive(bool Active)
    {
        m_IsActive = Active;
    }
    #endregion
}
