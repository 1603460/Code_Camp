using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundSlider : MonoBehaviour
{
    public Slider slider;
    public float sliderValue;
    public Image muteImage;
    public Sprite fullSound;
    public Sprite halfSound;
    public Sprite muteSound;
    // Start is called before the first frame update
    void Start()
    {
        slider.value = PlayerPrefs.GetFloat("volume", 0.9f);
        AudioListener.volume = slider.value;
        ReviseMuted();
    }

    public void ChangeSlider(float valor)
    {
        sliderValue = valor;
        PlayerPrefs.SetFloat("volume", sliderValue);
        AudioListener.volume = slider.value;
        ReviseMuted();
    }

    public void ReviseMuted()
    {
        if (sliderValue == 1)
        {
            muteImage.sprite = fullSound;
        }
        if (sliderValue == 0)
        {
            muteImage.sprite = muteSound;
        }
        else
        {
            if(sliderValue <= 0.5)
            {
                muteImage.sprite = halfSound;
            }
            else
            {
                muteImage.sprite = fullSound;
            }
        }
    }
}
