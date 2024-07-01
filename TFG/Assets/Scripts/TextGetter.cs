using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TextGetter : MonoBehaviour
{
    public TMP_InputField inputField;
    public Button updateButton;
    private string inputText;

    void Start()
    {
        // Asignar el listener al botón
        updateButton.onClick.AddListener(UpdateText);
    }

    public void UpdateText()
    {
        // Obtener el texto del input field y almacenarlo en la variable inputText
        inputText = inputField.text;
        Debug.Log("Texto ingresado: " + inputText);
    }

    // Método para obtener el texto en cualquier momento
    public string GetInputText()
    {
        return inputField.text;
    }
}
