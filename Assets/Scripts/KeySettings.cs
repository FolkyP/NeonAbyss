using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KeySettings : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement;
    public Dashing dashing;
    public Overdrive overdrive;
    public Slider sensitivityInput;
    public Text sensitivityText;

    [Header("UI Inputs (Keybinds)")]
    public TMP_InputField forwardInput;
    public TMP_InputField backwardInput;
    public TMP_InputField leftInput;
    public TMP_InputField rightInput;
    public TMP_InputField jumpInput;
    public TMP_InputField sprintInput;
    public TMP_InputField dashInput;
    public TMP_InputField overInput;

    private TMP_InputField activeInputField = null;
    
    // Default bindings (canonical names)
    private readonly Dictionary<string, KeyCode> defaultKeys = new Dictionary<string, KeyCode>()
    {
        { "Forward", KeyCode.W },
        { "Backward", KeyCode.S },
        { "Left", KeyCode.A },
        { "Right", KeyCode.D },
        { "Jump", KeyCode.Space },
        { "Sprint", KeyCode.LeftShift },
        { "Dash", KeyCode.LeftControl },
        { "Overdrive", KeyCode.F }
    };

    // Working copy for UI edits (not yet applied/saved)
    private Dictionary<string, KeyCode> workingKeys = new Dictionary<string, KeyCode>();

    // Map input fields -> key name string ("Forward", "Backward", ...)
    private Dictionary<TMP_InputField, string> inputFieldMap = new Dictionary<TMP_InputField, string>();
    public void OnSensitivitySliderChanged(float value)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        PlayerPrefs.Save();

        sensitivityInput.value = value;
        sensitivityText.text = value.ToString("F2");
    }

    private void Start()
    {
        float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f);

        // Clamp to slider range
        savedSensitivity = Mathf.Clamp(savedSensitivity, sensitivityInput.minValue, sensitivityInput.maxValue);

        sensitivityInput.value = savedSensitivity;
        sensitivityText.text = savedSensitivity.ToString("F2");

        // Add listener for when slider changes
        sensitivityInput.onValueChanged.AddListener(OnSensitivitySliderChanged);

        // Setup input field map and listeners
        SetupInputField(forwardInput, "Forward");
        SetupInputField(backwardInput, "Backward");
        SetupInputField(leftInput, "Left");
        SetupInputField(rightInput, "Right");
        SetupInputField(jumpInput, "Jump");
        SetupInputField(sprintInput, "Sprint");
        SetupInputField(dashInput, "Dash");
        SetupInputField(overInput, "Overdrive");

        // Load saved keys into working copy (and apply to gameplay initially)
        LoadWorkingKeysFromPrefs();
        ApplyWorkingToPlayer(); // initial apply so gameplay uses saved keys
        UpdateKeyInputs();
    }

    private void SetupInputField(TMP_InputField input, string keyName)
    {
        if (input == null) return;
        inputFieldMap[input] = keyName;

        LockInputRect(input);

        input.text = ""; // clear initial
        input.onSelect.AddListener(_ => {
            StartRebind(input);
            // Lock rect again after TMP repositions it
            LockInputRect(input);
        });
        input.onDeselect.AddListener(_ => {
            if (activeInputField == input) activeInputField = null;
            UpdateKeyInputs();
            LockInputRect(input);
        });


        input.readOnly = false;
    }
    private void LockInputRect(TMP_InputField input)
    {
        if (input.textComponent == null) return;
        RectTransform rt = input.textComponent.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }

    private void OnGUI()
    {
        // Only handle KeyDown once per press
        if (activeInputField != null && Event.current.isKey && Event.current.type == EventType.KeyDown)
        {
            KeyCode newKey = Event.current.keyCode;

            // consume event so TMP/InputSystem doesn't also insert the character
            Event.current.Use();

            // Guard: ignore None or modifier-only if you want (optional)
            if (newKey == KeyCode.None)
            {
                FinishRebindWithMessage(activeInputField, "Invalid!");
                return;
            }

            // Determine which binding this input field is for
            if (!inputFieldMap.TryGetValue(activeInputField, out string keyName))
            {
                // safety: shouldn't happen, but make sure we clean up
                FinishRebind(activeInputField);
                return;
            }

            // prevent duplicates in the working copy
            if (IsKeyAlreadyBoundWorking(newKey))
            {
                Debug.LogWarning("Key already in use: " + newKey);
                FinishRebindWithMessage(activeInputField, "Duplicate!");
                return;
            }

            // Set new key in the working copy (do not save yet)
            workingKeys[keyName] = newKey;

            // Update UI to show the newly chosen key
            UpdateKeyInputs();

            // stop listening for key for this input field and defocus
            FinishRebind(activeInputField);
        }
    }

    private void StartRebind(TMP_InputField field)
    {
        // Clean up any previous active field
        if (activeInputField != null && activeInputField != field)
        {
            activeInputField.readOnly = false;
            activeInputField = null;
        }

        activeInputField = field;
        field.readOnly = true;
        field.text = "Press a key...";

        // DON'T call EventSystem.current.SetSelectedGameObject(field.gameObject) here.
        // The field was just selected (this method is called from OnSelect), so selecting again causes the error.
    }

    // Helper to end rebinding and defocus
    private void FinishRebind(TMP_InputField field)
    {
        if (field != null)
        {
            field.readOnly = false;
        }
        activeInputField = null;

        // defocus UI so input field stops receiving events
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        // Ensure UI text is centered after finishing
        if (field != null && field.textComponent != null)
            field.textComponent.alignment = TextAlignmentOptions.Center;
    }

    private void FinishRebindWithMessage(TMP_InputField field, string msg)
    {
        if (field != null)
        {
            field.text = msg;
            field.readOnly = false;
            if (field.textComponent != null)
                field.textComponent.alignment = TextAlignmentOptions.Center;
        }
        activeInputField = null;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private void UpdateKeyInputs()
    {
        // Use workingKeys to populate UI (so UI always reflects unsaved edits)
        forwardInput.text = workingKeys.ContainsKey("Forward") ? workingKeys["Forward"].ToString() : defaultKeys["Forward"].ToString();
        backwardInput.text = workingKeys.ContainsKey("Backward") ? workingKeys["Backward"].ToString() : defaultKeys["Backward"].ToString();
        leftInput.text = workingKeys.ContainsKey("Left") ? workingKeys["Left"].ToString() : defaultKeys["Left"].ToString();
        rightInput.text = workingKeys.ContainsKey("Right") ? workingKeys["Right"].ToString() : defaultKeys["Right"].ToString();
        jumpInput.text = workingKeys.ContainsKey("Jump") ? workingKeys["Jump"].ToString() : defaultKeys["Jump"].ToString();
        sprintInput.text = workingKeys.ContainsKey("Sprint") ? workingKeys["Sprint"].ToString() : defaultKeys["Sprint"].ToString();
        dashInput.text = workingKeys.ContainsKey("Dash") ? workingKeys["Dash"].ToString() : defaultKeys["Dash"].ToString();
        overInput.text = workingKeys.ContainsKey("Overdrive") ? workingKeys["Overdrive"].ToString() : defaultKeys["Overdrive"].ToString();
    }

    private bool IsKeyAlreadyBoundWorking(KeyCode key)
    {
        foreach (var kv in workingKeys)
        {
            if (kv.Value == key) return true;
        }
        return false;
    }

    #region Persistence & Loading

    // Save working keys to PlayerPrefs and apply to gameplay
    public void SaveChanges()
    {
        // Ensure all keys exist in workingKeys (defensive)
        EnsureWorkingHasAllKeys();

        // Write each working key to PlayerPrefs (use consistent pref names)
        PlayerPrefs.SetString("ForwardKey", workingKeys["Forward"].ToString());
        PlayerPrefs.SetString("BackwardKey", workingKeys["Backward"].ToString());
        PlayerPrefs.SetString("LeftKey", workingKeys["Left"].ToString());
        PlayerPrefs.SetString("RightKey", workingKeys["Right"].ToString());
        PlayerPrefs.SetString("JumpKey", workingKeys["Jump"].ToString());
        PlayerPrefs.SetString("SprintKey", workingKeys["Sprint"].ToString());
        PlayerPrefs.SetString("DashKey", workingKeys["Dash"].ToString());
        PlayerPrefs.SetString("OverdriveKey", workingKeys["Overdrive"].ToString());
        PlayerPrefs.Save();

        // Apply to actual gameplay
        ApplyWorkingToPlayer();
    }

    // Load from PlayerPrefs into workingKeys (doesn't apply to gameplay by itself)
    private void LoadWorkingKeysFromPrefs()
    {
        workingKeys.Clear();
        foreach (var kv in defaultKeys)
        {
            string pref = kv.Key + "Key";
            if (PlayerPrefs.HasKey(pref))
            {
                try
                {
                    KeyCode loaded = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(pref));
                    workingKeys[kv.Key] = loaded;
                }
                catch
                {
                    // if parsing fails, fallback to default
                    workingKeys[kv.Key] = kv.Value;
                }
            }
            else
            {
                workingKeys[kv.Key] = kv.Value;
            }
        }
    }

    // Apply workingKeys values to playerMovement/dashing (actually change gameplay)
    private void ApplyWorkingToPlayer()
    {
        EnsureWorkingHasAllKeys();

        if (playerMovement != null)
        {
            playerMovement.forwardKey = workingKeys["Forward"];
            playerMovement.backwardKey = workingKeys["Backward"];
            playerMovement.leftKey = workingKeys["Left"];
            playerMovement.rightKey = workingKeys["Right"];
            playerMovement.jumpKey = workingKeys["Jump"];
            playerMovement.sprintKey = workingKeys["Sprint"];
        }

        if (dashing != null)
        {
            dashing.dashKey = workingKeys["Dash"];
        }
        if (overdrive != null)
        {
            overdrive.overKey = workingKeys["Overdrive"];
        }
    }

    // ensure every key exists in workingKeys (prevent KeyNotFound)
    private void EnsureWorkingHasAllKeys()
    {
        foreach (var kv in defaultKeys)
        {
            if (!workingKeys.ContainsKey(kv.Key))
                workingKeys[kv.Key] = kv.Value;
        }
    }

    #endregion

    #region UI Button Handlers (Reset/Cancel)

    // Reset the working copy to defaults (does NOT save)
    public void ResetToDefaults()
    {
        // Clear any active rebind and make sure inputs are writable
        if (activeInputField != null)
        {
            activeInputField.readOnly = false;
            activeInputField = null;
        }
        // defocus any selected UI
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);

        // copy defaults into workingKeys
        workingKeys.Clear();
        foreach (var kv in defaultKeys) workingKeys[kv.Key] = kv.Value;
        OnSensitivitySliderChanged(1f);
        

   
        // update UI so user sees the defaults, but don't persist until SaveChanges()
        UpdateKeyInputs();
    }

    // Cancel/Return: discard unsaved working changes and reload saved values into workingKeys
    public void CancelChanges()
    {
        // Clear any active rebind and make sure inputs are writable
        if (activeInputField != null)
        {
            activeInputField.readOnly = false;
            activeInputField = null;
        }
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);

        // reload saved values
        LoadWorkingKeysFromPrefs();
        // update UI (and do not apply to gameplay)
        UpdateKeyInputs();
    }

    #endregion

   


}
