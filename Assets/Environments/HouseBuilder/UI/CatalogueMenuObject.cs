using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HouseBuilder
{
    public class CatalogueMenuObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        protected List<string> m_FilterList;
        protected string m_Guid;
        protected string m_ShortName;
        public Button m_Button;
        protected GameObject m_Thumbnail;
        protected Image m_SelectedBorder;
        protected Image m_Icon;
        public bool m_IsActive;


        public Vector2 IconSize => new Vector2(m_Icon.sprite.rect.width, m_Icon.sprite.rect.height);
        public bool IsLandscape => (m_Icon.sprite.rect.width > m_Icon.sprite.rect.height);

        protected readonly Color NORMAL = new Color(1f, 1f, 1f, 0f);
        protected readonly Color HIGHLIGHT = new Color(1f, 1f, 1f, 1f);
        protected const float BORDER_TIME = 0.4f;

        public void SetActive(bool state)
        {
            gameObject.SetActive(state);
            m_IsActive = state;
        }

        protected virtual void ToggleThumbnail(bool state)
        {
            m_Thumbnail.SetActive(state);
        }

        public bool MatchFilters(List<string> fullFilters)
        {
            foreach (string filter in fullFilters)
            {
                if (false == MatchSingleFilter(filter))
                {
                    return false;
                }
            }

            return true;
        }

        public bool MatchSingleFilter(string fullFilter)
        {
            try
            {
                foreach (string str in m_FilterList)
                {
                    if (str.ToLower().Equals(fullFilter.ToLower()))
                    {
                        return true;
                    }
                }
            }
            catch(Exception e)
            {
                Debug.LogError($" {this.name}  error found  {e.Message}", this);
            }
            return false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ToggleThumbnail(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToggleThumbnail(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            m_SelectedBorder.color = HIGHLIGHT;

            this.WaitFor(BORDER_TIME, () => { m_SelectedBorder.color = NORMAL; });
        }

        protected void AssignMaterialNameTolabel(TextMeshProUGUI thumbnailLabel)
        {
            // will change       Floor_ConcreteRough_Pattern1_Mat_Scale4    to    ConcreteRough Pattern1 Mat
            var splitList = m_ShortName.Split('_').ToList();
            if (splitList.Count > 1)
            {
                splitList.RemoveAt(0);
                if (splitList.Last().StartsWith(HouseBuilderConstants.TEXTURE_SCALE, StringComparison.CurrentCultureIgnoreCase) == true)
                {
                    if (splitList.Count > 0)
                    {
                        splitList.RemoveAt(splitList.Count - 1);
                    }
                }
                string name = "";
                foreach (var item in splitList)
                {
                    name += $"{item} ";
                }
                thumbnailLabel.SetText(name);
            }
            else
            {
                thumbnailLabel.SetText(m_ShortName);
            }
        }
    }
}