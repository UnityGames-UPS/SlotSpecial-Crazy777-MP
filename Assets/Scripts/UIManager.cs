using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class UIManager : MonoBehaviour
{
    #region VARIABLE_AND_OBJECT_DECLARATIONS

    [Header("Button References")]
    [SerializeField] private List<UiButton> uiButton = new List<UiButton>();
    private Dictionary<string, Button> uiButtonDictionary = new Dictionary<string, Button>();

    [Header("InputField References")]
    [SerializeField] private List<UiInputField> uiInputField = new List<UiInputField>();
    private Dictionary<string, TMP_InputField> uiInputFieldDictionary = new Dictionary<string, TMP_InputField>();

    [Header("Text References")]
    [SerializeField] private List<UiText> uiText = new List<UiText>();
    private Dictionary<string, TMP_Text> uiTextDictionary = new Dictionary<string, TMP_Text>();

    [Header("Sprite References")]
    [SerializeField] private List<UiSprite> uiSprite = new List<UiSprite>();
    private Dictionary<string, Sprite> uiSpriteDictionary = new Dictionary<string, Sprite>();

    [Header("GameObjects References")]
    [SerializeField] private List<UiGameObject> uiGameObject = new List<UiGameObject>();
    private Dictionary<string, GameObject> uiGameObjectDictionary = new Dictionary<string, GameObject>();
    public TMP_Text respinsRangeText;

    #endregion

    private void OnEnable()
    {
        UpdateButtonDictionary();
        UpdateInputFieldDictionary();
        UpdateTextDictionary();
        UpdateSpriteDictionary();
        UpdateGameObjectDictionary();
    }

    #region OBJECT_METHODS

    private void UpdateButtonDictionary()
    {
        uiButtonDictionary.Clear();
        foreach (UiButton uiReference in uiButton)
        {
            if (uiReference.button != null && !uiButtonDictionary.ContainsKey(uiReference.key))
            {
                uiButtonDictionary.Add(uiReference.key, uiReference.button);
            }
        }
    }

    private void UpdateInputFieldDictionary()
    {
        uiInputFieldDictionary.Clear();
        foreach (UiInputField uiReference in uiInputField)
        {
            if (uiReference.inputField != null && !uiInputFieldDictionary.ContainsKey(uiReference.key))
            {
                uiInputFieldDictionary.Add(uiReference.key, uiReference.inputField);
            }
        }
    }

    private void UpdateTextDictionary()
    {
        uiTextDictionary.Clear();
        foreach (UiText uiReference in uiText)
        {
            if (uiReference.text != null && !uiTextDictionary.ContainsKey(uiReference.key))
            {
                uiTextDictionary.Add(uiReference.key, uiReference.text);
            }
        }
    }

    private void UpdateSpriteDictionary()
    {
        uiSpriteDictionary.Clear();
        foreach (UiSprite uiReference in uiSprite)
        {
            if (uiReference.Sprite != null && !uiSpriteDictionary.ContainsKey(uiReference.key))
            {
                uiSpriteDictionary.Add(uiReference.key, uiReference.Sprite);
            }
        }
    }

    private void UpdateGameObjectDictionary()
    {
        uiGameObjectDictionary.Clear();
        foreach (UiGameObject uiReference in uiGameObject)
        {
            if (uiReference.gameObject != null && !uiGameObjectDictionary.ContainsKey(uiReference.key))
            {
                uiGameObjectDictionary.Add(uiReference.key, uiReference.gameObject);
            }
        }
    }

    #endregion

    #region BUTTON_CALLBACKS

    internal Button GetButton(string key)
    {
        //Debug.Log(string.Concat("<color=yellow><b>", key, "</b></color>"));
        if (uiButtonDictionary.ContainsKey(key))
        {
            return uiButtonDictionary[key];
        }
        return null;
    }
    internal TMP_InputField GetInputField(string key)
    {
        //Debug.Log(string.Concat("<color=yellow><b>", key, "</b></color>"));
        if (uiInputFieldDictionary.ContainsKey(key))
        {
            return uiInputFieldDictionary[key];
        }
        return null;
    }
    // internal void ReconnectionPopup()
    // {
    //     if (uiGameObjectDictionary.ContainsKey("Reconnecting_Popup"))
    //     {
    //         uiGameObjectDictionary["Reconnecting_Popup"].SetActive(true);
    //     }
    // }
    // internal void DisconnectionPopup()
    // {
    //     if (uiGameObjectDictionary.ContainsKey("Disconnection_Popup"))
    //     {

    //         uiGameObjectDictionary["Disconnection_Popup"].SetActive(true);
    //     }

    // }
    // internal void CheckAndClosePopups()
    // {
    //     if (uiGameObjectDictionary["Reconnecting_Popup"].activeInHierarchy)
    //     {
    //         ClosePopup(ReconnectPopup_Object);
    //     }
    //     if (DisconnectPopup_Object.activeInHierarchy)
    //     {
    //         ClosePopup(DisconnectPopup_Object);
    //     }
    // }



    internal TMP_Text GetText(string key)
    {
        //Debug.Log(string.Concat("<color=yellow><b>", key, "</b></color>"));
        if (uiTextDictionary.ContainsKey(key))
        {
            return uiTextDictionary[key];
        }
        return null;
    }

    internal Sprite GetSprite(string key)
    {
        if (uiSpriteDictionary.ContainsKey(key))
        {
            return uiSpriteDictionary[key];
        }
        return null;
    }

    internal GameObject GetGameObject(string key)
    {
        //Debug.Log(string.Concat("<color=yellow><b>", key, "</b></color>"));
        if (uiGameObjectDictionary.ContainsKey(key))
        {
            return uiGameObjectDictionary[key];
        }
        return null;
    }
    #endregion
}

#region STRUCTURES

[System.Serializable]
public struct UiButton
{
    public string key;
    public Button button;
}

[System.Serializable]
public struct UiInputField
{
    public string key;
    public TMP_InputField inputField;
}

[System.Serializable]
public struct UiText
{
    public string key;
    public TMP_Text text;
}

[System.Serializable]
public struct UiSprite
{
    public string key;
    public Sprite Sprite;
}

[System.Serializable]
public struct UiGameObject
{
    public string key;
    public GameObject gameObject;
}

#endregion