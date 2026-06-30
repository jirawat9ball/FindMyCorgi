#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SetupDogInScene : Editor
{
    [MenuItem("CorgiTool/⭐ Setup Dogs in Scene")]
    public static void SetupDogs()
    {
        GameObject parentObj = Selection.activeGameObject;

        if (parentObj == null)
        {
            EditorUtility.DisplayDialog("แจ้งเตือน", "กรุณาคลิกเลือก GameObject หลัก (เช่น tibet-1_asset) ใน Hierarchy ก่อนครับ", "OK");
            return;
        }

        int dogCount = 0;
        int envInteractCount = 0;

        SceneHandle sceneScript = parentObj.GetComponent<SceneHandle>();
        if (sceneScript == null)
        {
            sceneScript = parentObj.AddComponent<SceneHandle>();
        }
        else
        {
            // 🌟 จุดที่เพิ่ม 1: แก้บั๊ก Inspector พัง (MissingReferenceException)
            // เคลียร์รายชื่อหมาเก่าทิ้งก่อนเริ่มหาใหม่ จะได้ไม่มีหมาผีค้างในสคริปต์
            Undo.RecordObject(sceneScript, "Clear Old Dog Lists");

            if (sceneScript.lostDogs != null) sceneScript.lostDogs.Clear();
            if (sceneScript.foundDogs != null) sceneScript.foundDogs.Clear();

            EditorUtility.SetDirty(sceneScript);
        }

        Transform[] allChildren = parentObj.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in allChildren)
        {
            if (child == null) continue;

            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr == null) continue;

            string objName = child.name;
            string upperName = objName.ToUpper();

            // ==========================================
            // 🌟 จุดที่เพิ่ม 2: ตั้งค่า Layer Order และดัน Z-Axis (กันคลิกทะลุ!)
            // ==========================================
            if (upperName.Contains("DOG"))
            {
                sr.sortingOrder = 0;
                child.localPosition = new Vector3(child.localPosition.x, child.localPosition.y, -1f);
                EditorUtility.SetDirty(sr);
                EditorUtility.SetDirty(child);
            }
            else if (upperName.Contains("SCN"))
            {
                sr.sortingOrder = -1;
                child.localPosition = new Vector3(child.localPosition.x, child.localPosition.y, 0f);
                EditorUtility.SetDirty(sr);
                EditorUtility.SetDirty(child);
            }
            else if (upperName.Contains("ENV"))
            {
                sr.sortingOrder = 1;
                child.localPosition = new Vector3(child.localPosition.x, child.localPosition.y, -2f);
                EditorUtility.SetDirty(sr);
                EditorUtility.SetDirty(child);
            }

            // ==========================================
            // 🌟 เซ็ตสคริปต์ ENV
            // ==========================================
            if (upperName.Contains("ENV"))
            {
                bool isInteractable = false;

                if (upperName.Contains("_MOVE"))
                {
                    // 🌟 เพิ่ม Collider ก่อนแอดสคริปต์
                    if (child.gameObject.GetComponent<BoxCollider2D>() == null) child.gameObject.AddComponent<BoxCollider2D>();

                    if (child.gameObject.GetComponent<MoveObject>() == null)
                        child.gameObject.AddComponent<MoveObject>();
                    isInteractable = true;
                }
                else if (upperName.Contains("_SLIDE"))
                {
                    // 🌟 เพิ่ม Collider ก่อนแอดสคริปต์
                    if (child.gameObject.GetComponent<BoxCollider2D>() == null) child.gameObject.AddComponent<BoxCollider2D>();

                    if (child.gameObject.GetComponent<SlideObject>() == null)
                        child.gameObject.AddComponent<SlideObject>();
                    isInteractable = true;
                }
                else if (upperName.Contains("_CLICK"))
                {
                    bool isDisappearMode = objName.EndsWith("_00");

                    int frameIndex = -1;
                    int lastUnderscore = objName.LastIndexOf('_');
                    string baseName = objName;

                    if (lastUnderscore > 0 && int.TryParse(objName.Substring(lastUnderscore + 1), out frameIndex))
                    {
                        baseName = objName.Substring(0, lastUnderscore);
                    }

                    bool isRootFrame = false;

                    if (isDisappearMode)
                    {
                        isRootFrame = true;
                    }
                    else if (frameIndex == -1 || frameIndex == 0)
                    {
                        isRootFrame = true;
                    }
                    else if (frameIndex == 1)
                    {
                        if (FindChildRecursive(parentObj.transform, baseName + "_0") == null &&
                            FindChildRecursive(parentObj.transform, baseName + "_00") == null)
                            isRootFrame = true;
                    }

                    if (isRootFrame)
                    {
                        // 🌟 เพิ่ม Collider ก่อนแอดสคริปต์ (แก้บั๊กของภาพนี้เลยครับ)
                        if (child.gameObject.GetComponent<BoxCollider2D>() == null) child.gameObject.AddComponent<BoxCollider2D>();

                        ClickObject clickObj = child.gameObject.GetComponent<ClickObject>();
                        if (clickObj == null) clickObj = child.gameObject.AddComponent<ClickObject>();
                        isInteractable = true;

                        // ดักเช็ค Null เพิ่มความปลอดภัยให้ Inspector
                        if (clickObj != null)
                        {
                            if (isDisappearMode)
                            {
                                Undo.RecordObject(clickObj, "Clear Array for Disappear Mode");
                                clickObj.spriteAfterClick = new Sprite[0];
                                clickObj.RequiredClick = 1;
                                EditorUtility.SetDirty(clickObj);
                            }
                            if (clickObj != null)
                            {
                                if (isDisappearMode)
                                {
                                    Undo.RecordObject(clickObj, "Clear Array for Disappear Mode");
                                    clickObj.spriteAfterClick = new Sprite[0];
                                    clickObj.RequiredClick = 1;
                                    EditorUtility.SetDirty(clickObj);
                                }
                                else
                                {
                                    // ==========================================
                                    // 🌟 ท่อนที่เปลี่ยนใหม่: เก็บเป็น GameObject และสั่ง SetActive(false)
                                    // ==========================================
                                    Undo.RecordObject(clickObj, "Set Array for Active Mode");
                                    List<GameObject> clickObjects = new List<GameObject>();
                                    int startSearch = frameIndex == -1 ? 0 : frameIndex + 1;

                                    for (int i = startSearch; i <= 20; i++)
                                    {
                                        string targetName = baseName + "_" + i;
                                        Transform foundObj = FindChildRecursive(parentObj.transform, targetName);

                                        if (foundObj != null)
                                        {
                                            // 🌟 สั่งปิดการแสดงผลไว้ก่อน และ "ไม่ลบ Object ทิ้ง"
                                            foundObj.gameObject.SetActive(false);
                                            clickObjects.Add(foundObj.gameObject);
                                        }
                                    }

                                    if (clickObjects.Count > 0)
                                    {
                                        // 💡 ถ้าในสคริปต์ ClickObject ของคุณมีตัวแปร Array แบบ GameObject ไว้รับค่า
                                        // เช่น public GameObject[] objectsToActive; 
                                        // เอาเครื่องหมาย // ด้านล่างออก แล้วแก้ชื่อตัวแปรให้ตรงกับในสคริปต์คุณได้เลยครับ:
                                        // clickObj.objectsToActive = clickObjects.ToArray();

                                        clickObj.RequiredClick = clickObjects.Count; // ยังใช้นับจำนวนคลิกอยู่
                                        EditorUtility.SetDirty(clickObj);
                                    }
                                    // ==========================================
                                }
                            }
                        }
                    }
                }

                if (isInteractable && child != null)
                {
                    // นับจำนวนชิ้นเฉยๆ ไม่ต้องแอด Collider ตรงนี้แล้ว
                    envInteractCount++;
                }
            }

            // ==========================================
            // 🌟 เซ็ตสคริปต์ DOG
            // ==========================================
            if (upperName.Contains("DOG") && objName.EndsWith("_H"))
            {
                BoxCollider2D collider = child.gameObject.GetComponent<BoxCollider2D>();
                if (collider == null) child.gameObject.AddComponent<BoxCollider2D>();

                Dog dogScript = child.gameObject.GetComponent<Dog>();
                if (dogScript == null) dogScript = child.gameObject.AddComponent<Dog>();

                Undo.RecordObject(dogScript, "Set Dog Script");

                if (sr != null)
                {
                    dogScript.spriteNotFound = sr.sprite;
                }

                Transform foundObj = null;

                foreach (Transform subChild in child)
                {
                    if (subChild.name.EndsWith("_B") || subChild.name.EndsWith("_F"))
                    {
                        foundObj = subChild;
                        break;
                    }
                }

                if (foundObj == null)
                {
                    string baseName = objName.Substring(0, objName.Length - 2);
                    foundObj = FindChildRecursive(parentObj.transform, baseName + "_F");
                }

                if (foundObj != null)
                {
                    SpriteRenderer foundRenderer = foundObj.GetComponent<SpriteRenderer>();
                    if (foundRenderer != null)
                    {
                        dogScript.spriteFound = foundRenderer.sprite;
                        DestroyImmediate(foundObj.gameObject);
                    }
                }
                else if (dogScript.spriteFound == null)
                {
                    if (sr != null)
                    {
                        dogScript.spriteFound = sr.sprite;
                    }
                }

                string baseIdName = objName.Substring(0, objName.Length - 2);
                dogScript.id = baseIdName.Replace("DOG-SPRITE_", "").Replace("DOG_", "");
                dogScript.startState = DogState.Visible;

                if (upperName.Contains("-S_") || upperName.Contains("_S_"))
                {
                    dogScript.isSpecial = true;
                }
                else
                {
                    dogScript.isSpecial = false;
                }

                EditorUtility.SetDirty(dogScript);
                dogCount++;
            }
        }

        EditorUtility.DisplayDialog("สำเร็จ!",
            $"เซ็ตติ้งด่านเสร็จสมบูรณ์ครับ!\n" +
            $"- จัด Layer Order และดัน Z-Axis กันคลิกทะลุ (SCN=0, DOG=-1, ENV=-2)\n" +
            $"- ติดตั้งสคริปต์ Dog: {dogCount} ตัว\n" +
            $"- ติดตั้งสคริปต์สิ่งกีดขวาง (ENV): {envInteractCount} ชิ้น",
            "เยี่ยมเลย!");
    }

    private static Transform FindChildRecursive(Transform parent, string nameToFind)
    {
        foreach (Transform child in parent)
        {
            if (child.name == nameToFind) return child;

            Transform result = FindChildRecursive(child, nameToFind);
            if (result != null) return result;
        }
        return null;
    }
}
#endif