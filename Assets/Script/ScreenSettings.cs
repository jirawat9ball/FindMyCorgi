using System;
using TMPro;
using UnityEngine;

public class ScreenSettings : MonoBehaviour
{
    public enum ScreenResolution { Fullscreen, Resolution3840x2160, Resolution1920x1080 }

    public ScreenResolution resolution = ScreenResolution.Fullscreen;
    public TextMeshProUGUI text;
    public int customWidth = 1920;
    public int customHeight = 1080;
    public bool fullscreen = true;
    public int targetFrameRate = 60;
    int index;
    void Start()
    {
        ApplyScreenSettings();
    }

    public void ApplyScreenSettings()
    {
        switch (resolution)
        {
            case ScreenResolution.Fullscreen:
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, fullscreen);
                break;
            case ScreenResolution.Resolution3840x2160:
                Screen.SetResolution(3840, 2160, fullscreen);
                break;
            case ScreenResolution.Resolution1920x1080:
                Screen.SetResolution(1920, 1080, fullscreen);
                break;
        }
        text.text = resolution.ToString();

        //Application.targetFrameRate = targetFrameRate;
    }
    public void SetScereen(int i) {
        index = index+i;
        if (index < 0) {
            index = 2;
        } else if (index >= 2) {
            index = 0;
        }
        resolution = (ScreenResolution)index;
        ApplyScreenSettings();
    }
    public void SetResolution(int width, int height, bool fullScreen)
    {
        Screen.SetResolution(width, height, fullScreen);
        fullscreen = fullScreen;
        customWidth = width;
        customHeight = height;
    }

    public void SetTargetFrameRate(int frameRate)
    {
        Application.targetFrameRate = frameRate;
        targetFrameRate = frameRate;
    }

    public void ToggleFullscreen()
    {
        fullscreen = !fullscreen;
        ApplyScreenSettings();
    }
}