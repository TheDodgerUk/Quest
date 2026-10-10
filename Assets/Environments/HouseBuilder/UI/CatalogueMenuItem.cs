using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace HouseBuilder
{
    public class CatalogueMenuItem : CatalogueMenuObject
    {
        public void Initialise(CatalogueEntry entry, TaskAction task, Action onClick)
        {
            m_FilterList = new List<string>(entry.CatalogueFilters);
            m_Guid = entry.Guid;
            m_ShortName = entry.ShortName;
            gameObject.name = m_ShortName;
            m_Button = GetComponent<Button>();
            ButtonInactive buttonInactive = m_Button.gameObject.ForceComponent<ButtonInactive>();

            buttonInactive.OnClick.AddListener(() =>
            {
                Debug.LogError("buttonInactive");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.Beep();
#endif
            }); ;

            m_Button.onClick.AddListener(() => onClick());
            m_Icon = transform.Find("Icon").GetComponent<Image>();



            Core.AssetBundlesRef.CatalogueThumbnailAssetBundleRef.GetItem(Core.Mono, m_Guid, (texture) =>
            {
                task?.Increment();
                if (texture != null)
                {
                    Sprite sp = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                    m_Icon.sprite = sp;
                }
                else
                {
                    Debug.LogError($"Cant find Thumbnail for {entry.FullName}   {m_Guid}");
                }
            });


            GameObject colourableIcon = transform.Find("Icon/Colourable").gameObject;
            colourableIcon.SetActive(false);

            foreach (var item in entry.GetLoadedMetaData())
            {
                bool lightColourChange = (item.GetMetaDataType() == MetaDataType.ContentLight) && (((ContentLightMetaData)item).m_CanEdit);
                if (item.GetMetaDataType() == MetaDataType.ContentMaterialChange || lightColourChange)
                {
                    colourableIcon.SetActive(true);
                    break;
                }
            }


            GameObject modularIcon = transform.Find("Icon/Modular").gameObject;
            modularIcon.SetActive(false);

            foreach (var item in entry.GetLoadedMetaData())
            {
                if (item.GetMetaDataType() == MetaDataType.ContentAdaptive)
                {
                    modularIcon.SetActive(true);
                    break;
                }
            }


            m_Thumbnail = transform.Find("Label").gameObject;
            m_SelectedBorder = transform.Find("SelectedBorder").GetComponent<Image>();
            TextMeshProUGUI thumbnailLabel = transform.Find("Label/Text").GetComponent<TextMeshProUGUI>();

            m_SelectedBorder.color = NORMAL;
            thumbnailLabel.SetText(m_ShortName);
            ToggleThumbnail(false);
        }

        public void SetInteractive(bool inter) => m_Button.interactable = inter;
    }
}

