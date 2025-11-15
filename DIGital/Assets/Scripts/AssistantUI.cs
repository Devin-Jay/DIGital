using UnityEngine;
using UnityEngine.UI;

public class AssistantUI : MonoBehaviour
{
    public GameObject ui;
    public InputField input;

    public void ToggleAssistant()
    {
        ui.SetActive(!ui.activeSelf);
    }

    public void OnSubmit()
    {
        Debug.Log(input.text);
        input.text = "";
    }
}
