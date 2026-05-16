using UnityEngine;
using System.Collections;
using TMPro;

public class TypewriterEffect : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public float delay = 0.1f; // Delay between characters

    public void PlayText(string textToType)
    {
        StartCoroutine(TypeText(textToType));
    }
    public void ClearText() {
        textComponent.text = "";
    }
    IEnumerator TypeText(string textToType)
    {
        textComponent.text = ""; // Clear existing text

        for (int i = 0; i < textToType.Length; i++)
        {
            textComponent.text += textToType[i];
            yield return new WaitForSeconds(delay);
        }
    }

    public void SkipText()
    {
        StopAllCoroutines();
        // If you want to show the entire text immediately, you can do this:
        // textComponent.text = currentText;
    }
}