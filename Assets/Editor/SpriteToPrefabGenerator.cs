using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class SpriteToPrefabGenerator : EditorWindow
{
    public enum PrefabCategory
    {
        InteractableDog,     // 🐶 หมาที่กดคลิกหาได้ (Prefix: DOG_)
        Obstacle_ENV,        // 🪨 สิ่งกีดขวาง (Prefix: ENV_)
        TargetDog_TARGET     // 🎯 หมาเป้าหมาย (Prefix: TARGET_)
    }

    private PrefabCategory selectedCategory = PrefabCategory.InteractableDog;
    private string saveFolderPath = "Assets/Prefabs/Level_Jordan";

    private int sortingOrder = 1;

    private Texture2D spriteSheetA;
    private Texture2D spriteSheetB;
    private Texture2D standardSpriteSheet;

    [MenuItem("CorgiTool/⭐ Sprite To Prefab Generator")]
    public static void ShowWindow()
    {
        GetWindow<SpriteToPrefabGenerator>("Prefab Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("เครื่องมือสร้าง Prefab สุนัข & สิ่งกีดขวาง", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        selectedCategory = (PrefabCategory)EditorGUILayout.EnumPopup("ประเภท Prefab", selectedCategory);
        saveFolderPath = EditorGUILayout.TextField("เซฟไว้ที่โฟลเดอร์", saveFolderPath);
        sortingOrder = EditorGUILayout.IntField("Order in Layer", sortingOrder);

        EditorGUILayout.Space();

        if (selectedCategory == PrefabCategory.InteractableDog)
        {
            EditorGUILayout.HelpBox("ลากภาพ Sprite Sheet ต้นฉบับมาใส่ได้เลยครับ (ระบบจะดึงตัวย่อ 3 ตัวแรก และตัวเลขมาทำ ID ให้ เช่น JOR-1-1)", MessageType.Info);
            spriteSheetA = (Texture2D)EditorGUILayout.ObjectField("แผ่น A (ยังหาไม่เจอ)", spriteSheetA, typeof(Texture2D), false);
            spriteSheetB = (Texture2D)EditorGUILayout.ObjectField("แผ่น B (หาเจอแล้ว)", spriteSheetB, typeof(Texture2D), false);
        }
        else
        {
            EditorGUILayout.HelpBox("ลากภาพ Sprite Sheet ต้นฉบับมาใส่ในช่องด้านล่างได้เลยครับ", MessageType.Info);
            standardSpriteSheet = (Texture2D)EditorGUILayout.ObjectField("แผ่นภาพ Sprite Sheet", standardSpriteSheet, typeof(Texture2D), false);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("🚀 สร้าง Prefab ทันที!", GUILayout.Height(40)))
        {
            GeneratePrefabs();
        }
    }

    private void GeneratePrefabs()
    {
        CreateFolderIfNotExists(saveFolderPath);

        Sprite[] spritesA = null;
        Sprite[] spritesB = null;

        if (selectedCategory == PrefabCategory.InteractableDog)
        {
            if (spriteSheetA == null || spriteSheetB == null) return;
            spritesA = GetSpritesFromTexture(spriteSheetA);
            spritesB = GetSpritesFromTexture(spriteSheetB);
        }
        else
        {
            if (standardSpriteSheet == null) return;
            spritesA = GetSpritesFromTexture(standardSpriteSheet);
        }

        if (spritesA == null || spritesA.Length == 0) return;

        int successCount = 0;

        for (int i = 0; i < spritesA.Length; i++)
        {
            Sprite spriteA = spritesA[i];

            string prefix = "";
            switch (selectedCategory)
            {
                case PrefabCategory.InteractableDog: prefix = "DOG_"; break;
                case PrefabCategory.Obstacle_ENV: prefix = "ENV_"; break;
                case PrefabCategory.TargetDog_TARGET: prefix = "TARGET_"; break;
            }

            string cleanName = spriteA.name.Replace("_A", "").Replace("-A", "");
            string prefabName = prefix + cleanName;
            string fullPath = saveFolderPath + "/" + prefabName + ".prefab";

            GameObject go = new GameObject(prefabName);
            go.layer = LayerMask.NameToLayer("Default");

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = spriteA;
            sr.sortingOrder = 1;

            if (selectedCategory == PrefabCategory.InteractableDog)
            {
                Dog dogScript = go.AddComponent<Dog>();

                dogScript.id = GenerateIDFromName(spriteA.name, i);
                dogScript.spriteNotFound = spriteA;

                Sprite spriteB = null;
                string suffix = spriteA.name.Replace(spriteSheetA.name, "");
                string expectedNameB = spriteSheetB.name + suffix;

                // 🌟 ค้นหาภาพ B ที่ชื่อตรงกันเป๊ะๆ
                spriteB = spritesB.FirstOrDefault(b => b.name == expectedNameB);

                // 🌟 ลบโค้ดที่ให้จับคู่ตามลำดับทิ้งไปแล้ว ถ้าหาไม่เจอ spriteB จะเป็น null และถูกปล่อยว่างไว้
                if (spriteB != null)
                {
                    dogScript.spriteFound = spriteB;
                }
                else
                {
                    Debug.LogWarning($"⚠️ ไม่พบภาพ B ที่ชื่อตรงกับ '{expectedNameB}' สำหรับ Prefab '{prefabName}' (ปล่อยช่อง Sprite Found ว่างไว้)");
                }
            }
            else if (selectedCategory == PrefabCategory.Obstacle_ENV)
            {
                go.AddComponent<BoxCollider2D>();
                sr.sortingOrder = 2;

            }
            else if (selectedCategory == PrefabCategory.TargetDog_TARGET)
            {
                go.AddComponent<BoxCollider2D>();
            }

            PrefabUtility.SaveAsPrefabAsset(go, fullPath);
            DestroyImmediate(go);
            successCount++;
        }

        Debug.Log($"✅ สร้าง Prefab อัตโนมัติและเซ็ต ID สำเร็จ {successCount} ชิ้น!");
        AssetDatabase.Refresh();
    }

    private Sprite[] GetSpritesFromTexture(Texture2D texture)
    {
        string path = AssetDatabase.GetAssetPath(texture);
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray();
    }

    private string GenerateIDFromName(string name, int loopIndex)
    {
        string shortCode = ExtractThreeLetterPrefix(name);
        MatchCollection matches = Regex.Matches(name, @"\d+");

        if (matches.Count >= 2)
        {
            int firstNum = int.Parse(matches[0].Value);
            int lastNum = int.Parse(matches[matches.Count - 1].Value);
            int finalItemNum = (lastNum > 0) ? lastNum : (loopIndex + 1);
            return $"{shortCode}-{firstNum}-{finalItemNum}";
        }
        else if (matches.Count == 1)
        {
            int itemNum = int.Parse(matches[0].Value);
            int finalItemNum = (itemNum > 0) ? itemNum : (loopIndex + 1);
            return $"{shortCode}-{finalItemNum}";
        }
        else
        {
            return $"{shortCode}-{loopIndex + 1}";
        }
    }

    private string ExtractThreeLetterPrefix(string name)
    {
        string lettersOnly = new string(name.Where(char.IsLetter).ToArray());
        if (lettersOnly.Length >= 3)
        {
            return lettersOnly.Substring(0, 3).ToUpper();
        }
        else if (lettersOnly.Length > 0)
        {
            return lettersOnly.ToUpper();
        }
        return "DOG";
    }

    private void CreateFolderIfNotExists(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string[] folders = path.Split('/');
            string currentPath = folders[0];
            for (int i = 1; i < folders.Length; i++)
            {
                if (!AssetDatabase.IsValidFolder(currentPath + "/" + folders[i]))
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                currentPath += "/" + folders[i];
            }
        }
    }
}