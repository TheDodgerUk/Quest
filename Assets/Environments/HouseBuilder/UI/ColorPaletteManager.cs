using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HouseBuilder
{
    public class ColorPaletteManager : MonoBehaviour
    {
        private const int COLOR_GRADIENT_BOX_HEIGHT = 180;
        private const int COLOR_GRADIENT_BOX_WIDTH = 109;

        private const float BOX_SLIDER_MAX_VALUE = 0.999f;
        private const float BOX_SLIDER_MIN_VALUE = 0.001f;

        private ColorPickerManager m_ColorPickerManager;
        private GameObject m_ColorPaletteHandle;
        private BoxSlider m_BoxSlider;
        private Texture2D m_ColorGradientTexture;
        private RawImage m_Background;
        private Color m_SelectedColor;
        
        public void Initialise(ColorPickerManager colorPickerManager, GraphicRaycaster raycaster)
        {
            m_ColorPickerManager = colorPickerManager;
            m_ColorGradientTexture = new Texture2D(COLOR_GRADIENT_BOX_WIDTH, COLOR_GRADIENT_BOX_HEIGHT);
            var boxSlider = this.transform.Find("ColorBox").transform.Find("BoxSlider").gameObject;
            m_ColorPaletteHandle = boxSlider.transform.Find("Handle Slide Area").transform.Find("Handle").gameObject;
            m_Background = boxSlider.GetComponent<RawImage>();
            m_BoxSlider = boxSlider.ForceComponent<BoxSlider>();
            m_BoxSlider.maxValue = BOX_SLIDER_MAX_VALUE;
            m_BoxSlider.minValue = BOX_SLIDER_MIN_VALUE;
            m_BoxSlider.handleRect = m_ColorPaletteHandle.GetComponent<RectTransform>();
            m_BoxSlider.onValueChanged.AddListener((float a,float b ) => { ChangeObjectColor(); });
            m_BoxSlider.OnEndDrag.AddListener( ()=> { OnMouseRelease(); });
        }

        private void OnMouseRelease()
        {
            HouseBuilderRoot.Instance.RecorderRef.Record();
        }

        private void ChangeObjectColor()
        {
            float currentX = m_BoxSlider.value * COLOR_GRADIENT_BOX_WIDTH;
            float currentY = m_BoxSlider.valueY * COLOR_GRADIENT_BOX_HEIGHT;
            m_SelectedColor = m_ColorGradientTexture.GetPixel((int)currentX, (int)currentY);
            m_ColorPickerManager.ChangeSelectedColorViaColorPalette(m_SelectedColor);
        }

        public void UpdateGradientTexture(Color color)
        {
            Color lerpedColorOne;
            Color lerpedColorTwo;
            Texture2D newTexture = new Texture2D(COLOR_GRADIENT_BOX_WIDTH, COLOR_GRADIENT_BOX_HEIGHT);

            for (int y = COLOR_GRADIENT_BOX_HEIGHT; y >= 0; --y)
            {
                float t = (float)y / COLOR_GRADIENT_BOX_HEIGHT;
                lerpedColorOne = Color.Lerp(Color.white, color, t);
                for (int x = COLOR_GRADIENT_BOX_WIDTH; x >= 0; --x)
                {
                    float v = (float)x / COLOR_GRADIENT_BOX_WIDTH;
                    lerpedColorTwo = Color.Lerp(Color.black, lerpedColorOne , v);
                    newTexture.SetPixel(x, y, lerpedColorTwo);
                }
            }
            newTexture.Apply();
            m_ColorGradientTexture = newTexture;
            m_Background.texture = m_ColorGradientTexture;
            ChangeObjectColor();
        }
    }
}