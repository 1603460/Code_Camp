using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FullScreen : MonoBehaviour
{
    public TMP_Dropdown resolutionDrop;
    Resolution[] resolutions;

    // Start is called before the first frame update
    void Start()
    {
        reviseResolution();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void activateFullScreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }

    public void reviseResolution()
    {
        resolutions = Screen.resolutions;
        resolutionDrop.ClearOptions();
        List<string> options = new List<string>();
        int actualRes = 0;

        for(int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if(Screen.fullScreen && resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                actualRes = i;
            }
        }
        resolutionDrop.AddOptions(options);
        resolutionDrop.value = actualRes;
        resolutionDrop.RefreshShownValue();

        resolutionDrop.value = PlayerPrefs.GetInt("ResNum", 0);
    }

    public void changeResolution(int resIndex)
    {
        PlayerPrefs.SetInt("ResNum", resolutionDrop.value);

        Resolution res = resolutions[resIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }
}
