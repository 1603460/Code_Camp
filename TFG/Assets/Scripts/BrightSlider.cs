using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BrightSlider : MonoBehaviour
{
    public Slider slider;
    public float sliderValue;
    public Image brightPanel;
    public Image brightnessImage;
    public Sprite fullBright;
    public Sprite halfBright;
    public Sprite minBright;
    // Start is called before the first frame update
    void Start()
    {
        slider.value = PlayerPrefs.GetFloat("brightness", 0.0f);
        brightPanel.color = new Color(brightPanel.color.r, brightPanel.color.g, brightPanel.color.b, slider.value);
        ReviseBright();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeSlider(float valor)
    {
        sliderValue = valor;
        PlayerPrefs.SetFloat("brightness", sliderValue);
        brightPanel.color = new Color(brightPanel.color.r, brightPanel.color.g, brightPanel.color.b, slider.value);
        ReviseBright();
    }

    public void ReviseBright()
    {
        if(sliderValue < 0.5)
        {
            brightnessImage.sprite = fullBright;
        }
        else
        {
            if(sliderValue > 0.88)
            {
                brightnessImage.sprite = minBright;
            }
            else
            {
                brightnessImage.sprite = halfBright;
            }
        }
    }
}
