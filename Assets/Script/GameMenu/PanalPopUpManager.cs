using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public enum TypePopUp {
    Inventory, Setting, Confirm, Howtoplay, GotItem
}
public static class TypePopUpExtensions
{
    public static TypePopUp ToTypePopUp(this int value)
    {
        if (Enum.IsDefined(typeof(TypePopUp), value))
        {
            return (TypePopUp)value;
        }
        else
        {
            return TypePopUp.Inventory; // Or throw an exception: throw new ArgumentException("Invalid int value for TypePopUp enum.");
        }
    }
}
public class PanalPopUpManager : MonoBehaviour
{
    public Image[] BG;
    public TextMeshProUGUI TxtConfirm;
    public Button YesButtonConfirm;
    public GotItem gotItem;
    [Header("ScaleUpOnStart")]
    public ScaleUpOnStart Inventory;
    public ScaleUpOnStart Setting;
    public ScaleUpOnStart Confirm;
    public ScaleUpOnStart Howtoplay;
    public ScaleUpOnStart GotItem;


    private Stack<ScaleUpOnStart> PopUplayer = new Stack<ScaleUpOnStart>();

    public void OpenMenuPopUp(int index)
    {
        TypePopUp typ = TypePopUpExtensions.ToTypePopUp(index);
        ShowPopUp(typ);
    }
    public void ShowPopUp(TypePopUp type) {
        Gamemanager.Instance.stateGame = StateGame.menu;
        StartCoroutine(ShowPopUpEnum(type));
    }
    public void ShowPopUpExit() {
        string t = "Are you sure you really want to exit the game";
        ShowPopUpConfirm(t, ()=> { 
            Application.Quit();
        });
    }
    public void ShowPopUpGotoCamp()
    {
        string t = "Are you sure you really want to go camp";
        ShowPopUpConfirm(t, () => {
            LoadSceneManager.Instance.UnloadCurrentScene(
                Gamemanager.Instance.GoHome
                );
        });
    }

    public void ShowPopUpResetGame()
    {
        string t = "Are you sure you want to reset game progress";
        ShowPopUpConfirm(t, () => {
            //reset save
            Gamemanager.Instance.ResetGame();
            LoadSceneManager.Instance.UnloadCurrentScene(
                Gamemanager.Instance.GoHome
                );
        });
    }
    public void ShowPopUpConfirm(string c, Action onClickYes)
    {
        TxtConfirm.text = c;
        StartCoroutine(ShowPopUpEnum(TypePopUp.Confirm));
        YesButtonConfirm.onClick.RemoveAllListeners();
        YesButtonConfirm.onClick.AddListener(() =>
        {
            onClickYes?.Invoke(); // Safely invoke the action
        });
    }
    public void ShowPopUpGotItem(KeyItem c)
    {
        gotItem.SetUpItem(c);
        StartCoroutine(ShowPopUpEnum(TypePopUp.GotItem));
    }
    public void ClosePopUp() {
        StartCoroutine(ClosePopUpEnum());
    }
    public void CloseAllPopUp()
    {
        while (PopUplayer.Count > 0) {
            StartCoroutine(ClosePopUpEnum());
        }
    }
    IEnumerator ShowPopUpEnum(TypePopUp type) {
        int index = PopUplayer.Count;
        BG[index].enabled = true;
        yield return StartCoroutine(BG[index].FadeIn(0.25f)); // Fade in over 1 second

        switch (type)
        {
            case TypePopUp.Inventory:
                PopUplayer.Push(Inventory);
                Inventory.gameObject.SetActive(true); break;
            case TypePopUp.Setting:
                PopUplayer.Push(Setting);
                Setting.gameObject.SetActive(true); break;
            case TypePopUp.Confirm:
                PopUplayer.Push(Confirm);
                Confirm.gameObject.SetActive(true); break;
            case TypePopUp.Howtoplay:
                PopUplayer.Push(Howtoplay);
                Howtoplay.gameObject.SetActive(true); break;
            case TypePopUp.GotItem:
                PopUplayer.Push(GotItem);
                GotItem.gameObject.SetActive(true); break;
            default:
                break;
        }
    }
    IEnumerator ClosePopUpEnum()
    {
        // 🌟 1. ดักจับเผื่อกรณี Stack ว่าง (กดปุ่มปิดรัวๆ) จะได้ไม่เกิด Error
        if (PopUplayer.Count == 0) yield break;

        ScaleUpOnStart closeLayer = PopUplayer.Pop();
        int index = PopUplayer.Count;
        index = Mathf.Clamp(index, 0, BG.Length - 1); // ป้องกัน index เกินขอบเขตของ BG Array

        Debug.Log($"🔒 ปิด PopUp: {closeLayer.gameObject.name}, Index: {index}, Stack Count: {PopUplayer.Count}");
        yield return StartCoroutine(closeLayer.ScaleDownRoutine());

        // 🌟 2. แก้บั๊กหลัก: เช็คก่อนว่า index มีอยู่จริงใน Array BG หรือไม่!
        if (BG != null && index >= 0 && index < BG.Length)
        {
            yield return StartCoroutine(BG[index].FadeOut(0.25f));
            BG[index].enabled = false;
        }
        else
        {
            Debug.LogWarning($"⚠️ ข้ามการปิด BG เพราะเปิด PopUp ซ้อนกันเกินจำนวน BG Array ที่ตั้งไว้ (Index: {index}, BG.Length: {(BG != null ? BG.Length : 0)})");
        }

        if (closeLayer != null)
        {
            closeLayer.gameObject.SetActive(false);
        }
    }
    void closeAllMenu() {
        Inventory.gameObject.SetActive(false);
        Setting.gameObject.SetActive(false); ;
        Confirm.gameObject.SetActive(false); ;
        Howtoplay.gameObject.SetActive(false);
        GotItem.gameObject.SetActive(false);
    }

  
}
