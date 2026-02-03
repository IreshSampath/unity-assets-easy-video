using System;
using TMPro;
using UnityEngine;

public class EasyVideoUIManager : MonoBehaviour
{
    [SerializeField] EasyVideoResolutionHandler _resolutionHandler;
    [SerializeField] EasyVideoFitMode _fitMode = EasyVideoFitMode.Fit_Inside;
    [SerializeField] TMP_Dropdown _dropdown;

    [SerializeField] private TMP_InputField _width;
    [SerializeField] private TMP_InputField _height;
    
    void Awake()
    {
        LoadFitMode();
        PopulateDropdown();
        _dropdown.onValueChanged.AddListener(OnDropdownChanged);
    }

    void OnEnable()
    {
        _resolutionHandler.OnResolutionChanged += UpdateResolutionUI;
    }
    
    void OnDisable()
    {
        _resolutionHandler.OnResolutionChanged -= UpdateResolutionUI;
    }
    
    void UpdateResolutionUI(int width, int height)
    {
        _width.text = width.ToString();
        _height.text = height.ToString();
    }
    void LoadFitMode()
    {
        _fitMode = _resolutionHandler.LoadFitMode();
        _resolutionHandler.SetFitMode(_fitMode);
        
    }
    public void ShowResolution()
    {
        _width.text = Screen.width.ToString();
        _height.text = Screen.height.ToString();
    }
    public void SetManualResolution()
    {
        int width = int.Parse(_width.text);
        int height = int.Parse(_height.text);
        _resolutionHandler.SetResolutionManual(width, height);
    }
    
    void PopulateDropdown()
    {
        _dropdown.ClearOptions();

        var enumNames = Enum.GetNames(typeof(EasyVideoFitMode));
        var options = new System.Collections.Generic.List<string>();

        foreach (var name in enumNames)
        {
            options.Add(name.Replace("_", " "));
        }

        _dropdown.AddOptions(options);

        // Set current value
        _dropdown.value = (int)_fitMode;
        _dropdown.RefreshShownValue();
    }
    
    void OnDropdownChanged(int index)
    {
        ApplyFitMode((EasyVideoFitMode)index);
    }
    void ApplyFitMode(EasyVideoFitMode mode)
    {
        _resolutionHandler.SetFitMode(mode);
    }
}
