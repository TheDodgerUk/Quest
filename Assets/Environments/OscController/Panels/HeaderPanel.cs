using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OscController
{
    public class HeaderPanel : MonoBehaviour
    {
        private Text m_FileName;
        public void Initialise()
        {
            m_FileName = this.transform.SearchComponent<Text>("Current FileText");
            m_FileName.text = "Non loaded!"; 
        }

        public void SetCurrentFileName(string name)
        {
            if(m_FileName != null)
            {
                m_FileName.text = name;
            }
        }
    }
}
