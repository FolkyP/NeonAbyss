using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsResolution : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    private Resolution[] resolutions;
    private List<Resolution> filteredResolutions;
    private int currentResolutionIndex = 0;

   

    void Start()
    {
        resolutions = Screen.resolutions;
        filteredResolutions = new List<Resolution>();

        resolutionDropdown.ClearOptions();

        int currentRefreshRate = Screen.currentResolution.refreshRate;

        // Filter only resolutions that match current refresh rate
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].refreshRate == currentRefreshRate)
            {
                filteredResolutions.Add(resolutions[i]);
            }
        }

        // Sort by width (desc), then height (desc)
        filteredResolutions.Sort((a, b) =>
        {
            if (a.width != b.width)
                return b.width.CompareTo(a.width);
            else
                return b.height.CompareTo(a.height);
        });

        // Take only up to 8 resolutions
        if (filteredResolutions.Count > 8)
            filteredResolutions = filteredResolutions.GetRange(0, 8);

        List<string> options = new List<string>();
        for (int i = 0; i < filteredResolutions.Count; i++)
        {
            string resolutionOption = filteredResolutions[i].width + "x" + filteredResolutions[i].height;
            options.Add(resolutionOption);

            if (filteredResolutions[i].width == Screen.width &&
                filteredResolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        // Add listener so it applies immediately
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = filteredResolutions[resolutionIndex];

        // Always force apply
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
        Debug.Log($"Resolution set: {resolution.width}x{resolution.height}, mode: {Screen.fullScreenMode}");
    }
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen; // toggles fullscreen on/off
        Debug.Log($"Fullscreen set: {isFullscreen}");
    }
}
