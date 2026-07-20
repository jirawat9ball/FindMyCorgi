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
        textComponent.maxVisibleCharacters = 0;
    }
    IEnumerator TypeText(string textToType)
    {
        // กำหนดข้อความไปก่อน แล้วปรับให้ซ่อนไว้ (0)
        textComponent.text = textToType; 
        textComponent.maxVisibleCharacters = 0;
        
        // รอ 1 เฟรมเพื่อให้ TextMeshPro รีเฟรชตัวเอง (Canvas อัปเดต)
        yield return null;
        
        // พอมันอัปเดตแล้ว เราถึงจะดึงจำนวนตัวอักษรที่ถูกต้องมาได้
        int totalVisibleCharacters = textComponent.textInfo.characterCount;
        
        // กรณีถ้ามี Error ดึงค่าไม่ได้ ให้ใช้ความยาวทั้งหมดเผื่อไว้
        if (totalVisibleCharacters == 0 && textToType.Length > 0)
        {
            totalVisibleCharacters = textToType.Length;
        }

        for (int i = 0; i <= totalVisibleCharacters; i++)
        {
            textComponent.maxVisibleCharacters = i;
            yield return new WaitForSeconds(delay);
        }
    }

    public void SkipText()
    {
        StopAllCoroutines();
    }
}