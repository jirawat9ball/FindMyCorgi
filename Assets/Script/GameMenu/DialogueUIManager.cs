using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUIManager : MonoBehaviour
{
    public TypewriterEffect typewriter;
    public AnimationCurve scaleCurve;
    [UnityEngine.Serialization.FormerlySerializedAs("parrent")]
    public GameObject parent;
    public Image portrait;
    
    [Header("Emotion Portraits")]
    public Sprite portraitNormal;
    public Sprite portraitHappy;
    public Sprite portraitCurious;

    public Image dialogueBox;
    public Image BG;
    string dialogText;
    bool ShowDialogIsDone;
    private Coroutine currentCoroutine;
    private void Update()
    {
        if (parent.activeSelf && Input.GetMouseButtonDown(0) && ShowDialogIsDone) {
            OnCloseDialog();
        }
    }
    public void OnShowDialog(string dialogKey)
    {
        if (parent.activeSelf) {
            OnCloseDialog();
            return;
        }
        parent.SetActive(true);
        // 🌟 ดึงข้อมูลจากไฟล์แปลภาษา
        dialogText = LanguageSettings.Instance.GetText(dialogKey);

        // 🌟 ค้นหาตัวลูกที่เป็นรูปหน้าจริงๆ (เพื่อไม่ให้กระทบกรอบรูปที่ตัวแม่)
        Image faceImage = portrait;
        if (portrait != null)
        {
            Transform photo = portrait.transform.Find("dialogue_portrait photo");
            if (photo != null) faceImage = photo.GetComponent<Image>();
            else if (portrait.transform.childCount > 0) faceImage = portrait.transform.GetChild(0).GetComponent<Image>();
        }

        // 🌟 ตรวจจับ Emotion Tag
        if (dialogText.Contains("(Happy)"))
        {
            if (portraitHappy != null && faceImage != null) faceImage.sprite = portraitHappy;
            dialogText = dialogText.Replace("(Happy)", "").Trim();
        }
        else if (dialogText.Contains("(Curious)"))
        {
            if (portraitCurious != null && faceImage != null) faceImage.sprite = portraitCurious;
            dialogText = dialogText.Replace("(Curious)", "").Trim();
        }
        else if (dialogText.Contains("(Normal)"))
        {
            if (portraitNormal != null && faceImage != null) faceImage.sprite = portraitNormal;
            dialogText = dialogText.Replace("(Normal)", "").Trim();
        }
        else
        {
            // ถ้าไม่มี Tag ก็ให้กลับไปเป็น Normal
            if (portraitNormal != null && faceImage != null) faceImage.sprite = portraitNormal;
        }

        ShowDialogIsDone = false;
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(SequenceShowDialog(dialogText));
    }
    public void OnCloseDialog()
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(SequenceCloseDialog());
    }
    IEnumerator SequenceShowDialog(string dialogText) {
        float duration = 0.25f;
        dialogueBox.fillAmount = 0;
        typewriter.ClearText();
        yield return StartCoroutine(portrait.transform.ScaleUp(Vector3.zero, Vector3.one, duration, scaleCurve));
        StartCoroutine(BG.Fade(0, 1, duration));
        yield return StartCoroutine(dialogueBox.TweenFillAmount(1, duration));
        typewriter.PlayText(dialogText);
        ShowDialogIsDone = true;
    }
    IEnumerator SequenceCloseDialog()
    {
        float duration = 0.25f;
        yield return StartCoroutine(portrait.transform.ScaleUp(Vector3.one , Vector3.zero, duration, scaleCurve));
        StartCoroutine(BG.Fade(1, 0, duration));
        yield return StartCoroutine(dialogueBox.TweenFillAmount(0, duration));
        parent.SetActive(false);
        if (UIManager.Instance != null) UIManager.Instance.OnNotificationClosed();
    }
    public void ChangeText(string newText)
    {
        if (typewriter != null)
        {
            typewriter.PlayText(newText);
        }
    }

    public void SkipDialog()
    {
        if (typewriter != null)
        {
            typewriter.SkipText();
            typewriter.textComponent.text = dialogText; // Shows the whole text.
        }
    }
}
