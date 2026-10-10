using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HouseBuilder
{
    public class HueManager : MonoBehaviour
    {
        private Slider m_HueSlider;
        private Texture2D m_HueBackGroundImage;

        private float m_HueBackgroundImageWidth;
        private float m_HueBackgroundImageHeight;

        private ColorPickerManager m_ColorPicker;

        public void Initialise(ColorPickerManager colorPicker)
        {
            m_ColorPicker = colorPicker;
            SliderSetup();
            StoreTexture();
            UpdatePaletteColor();
        }

        private void StoreTexture()
        {
            m_HueBackGroundImage = (Texture2D)this.transform.Find("Background").GetComponent<RawImage>().texture;
            m_HueBackgroundImageWidth = m_HueBackGroundImage.width;
            m_HueBackgroundImageHeight = m_HueBackGroundImage.height;
        }

        private void UpdatePaletteColor()
        {
            float currentY = (1.0f -m_HueSlider.value) * (m_HueBackgroundImageHeight - 1); // the array goes from 0 to m_HueBackgroundImageWidth-1
            Color color = m_HueBackGroundImage.GetPixel((int)(m_HueBackgroundImageWidth / 2), (int)currentY);
            m_ColorPicker.UpdateColorPalette(color);
        }

        private void SliderSetup()
        {
            m_HueSlider = this.transform.GetComponent<Slider>();
            m_HueSlider.onValueChanged.AddListener(delegate { UpdatePaletteColor(); });
        }
    }
}
