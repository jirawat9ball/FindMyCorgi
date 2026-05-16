using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

public class PrefabLevelPlacer : EditorWindow
{
    private string folderPath = "Assets/Prefabs/Level_Jordan";
    private float gridSpacing = 2.0f;
    private int maxColumns = 10;

    private Transform targetParent;

    [MenuItem("CorgiTool/🗺️ Level Prefab Placer")]
    public static void ShowWindow()
    {
        GetWindow<PrefabLevelPlacer>("Level Placer");
    }

    private void OnGUI()
    {
        GUILayout.Label("เครื่องมือเท Prefab ลงฉาก (จัดเรียงเป็น Grid)", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        folderPath = EditorGUILayout.TextField("ดึง Prefab จากโฟลเดอร์", folderPath);

        EditorGUILayout.Space();
        gridSpacing = EditorGUILayout.FloatField("ระยะห่างระหว่างตัว", gridSpacing);
        maxColumns = EditorGUILayout.IntField("จำนวนสูงสุดต่อแถว", maxColumns);

        EditorGUILayout.Space();

        GUILayout.Label("การจัดกลุ่มใน Hierarchy:", EditorStyles.boldLabel);
        targetParent = (Transform)EditorGUILayout.ObjectField("ลาก Parent มาใส่ (เช่น scene_jordan-1)", targetParent, typeof(Transform), true);

        EditorGUILayout.Space();
        GUILayout.Label("เลือกประเภทที่ต้องการวางลงฉาก:", EditorStyles.boldLabel);

        if (GUILayout.Button("🐶 1. วางเฉพาะสุนัข (DOG_)", GUILayout.Height(35)))
        {
            SpawnPrefabsIntoScene(new string[] { "DOG_" }, "Dogs_Only");
        }

        if (GUILayout.Button("🪨 2. วางเฉพาะสิ่งของ (ENV_)", GUILayout.Height(35)))
        {
            SpawnPrefabsIntoScene(new string[] { "ENV_" }, "Environment_Only");
        }

        EditorGUILayout.Space();

        GUI.backgroundColor = new Color(0.8f, 1f, 0.8f);
        if (GUILayout.Button("📦 3. วางทั้งหมด (DOG_ และ ENV_)", GUILayout.Height(45)))
        {
            SpawnPrefabsIntoScene(new string[] { "DOG_", "ENV_" }, "ALL_Prefabs");
        }
        GUI.backgroundColor = Color.white;
    }

    private void SpawnPrefabsIntoScene(string[] prefixes, string containerName)
    {
        // 1. สร้าง Main Container (โฟลเดอร์แม่)
        GameObject mainParent = new GameObject($"[ {containerName} ]");

        if (targetParent != null)
        {
            mainParent.transform.SetParent(targetParent);
            mainParent.transform.localPosition = Vector3.zero;
        }
        else
        {
            Vector3 startPos = SceneView.lastActiveSceneView ? SceneView.lastActiveSceneView.pivot : Vector3.zero;
            mainParent.transform.position = startPos;
        }

        int col = 0;
        int row = 0;
        int successCount = 0;

        // 2. วนลูปตามประเภท Prefix ที่เรากด (เช่น หา DOG_ ก่อน แล้วค่อยหา ENV_)
        foreach (string prefix in prefixes)
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
            List<GameObject> prefabsToSpawn = new List<GameObject>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab != null && prefab.name.StartsWith(prefix))
                {
                    prefabsToSpawn.Add(prefab);
                }
            }

            // ถ้าในหมวดนี้ไม่มี Prefab เลย ให้ข้ามไปหมวดต่อไป
            if (prefabsToSpawn.Count == 0) continue;

            prefabsToSpawn = prefabsToSpawn.OrderBy(p => ExtractNumberFromName(p.name)).ThenBy(p => p.name).ToList();

            // 3. สร้าง Sub-Container (โฟลเดอร์ย่อย) ตามชื่อ Prefix เช่น [ DOG_Group ]
            GameObject subParent = new GameObject($"[ {prefix}Group ]");
            subParent.transform.SetParent(mainParent.transform);
            subParent.transform.localPosition = Vector3.zero;

            // 4. เท Prefab ลงในโฟลเดอร์ย่อย
            foreach (GameObject prefab in prefabsToSpawn)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.transform.SetParent(subParent.transform);

                // คำนวณ Grid ให้เรียงต่อกันไปเรื่อยๆ แม้จะอยู่คนละโฟลเดอร์ย่อย
                Vector3 offset = new Vector3(col * gridSpacing, -row * gridSpacing, 0);
                instance.transform.localPosition = offset;

                col++;
                if (col >= maxColumns)
                {
                    col = 0;
                    row++;
                }
                successCount++;
            }
        }

        // เช็คเผื่อกรณีที่หา Prefab ไม่เจอเลยสักหมวด
        if (successCount == 0)
        {
            string prefixList = string.Join(" หรือ ", prefixes);
            Debug.LogWarning($"⚠️ ไม่พบ Prefab ที่ขึ้นต้นด้วย '{prefixList}' ในโฟลเดอร์ '{folderPath}' ครับ");
            DestroyImmediate(mainParent); // ลบ Main Container ทิ้งไปเลยถ้าไม่มีของ
            return;
        }

        Selection.activeGameObject = mainParent;
        SceneView.FrameLastActiveSceneView();

        Debug.Log($"✅ นำ Prefab ลงฉากสำเร็จ {successCount} ชิ้น! (จัดกลุ่มย่อยเรียบร้อย)");
    }

    private int ExtractNumberFromName(string name)
    {
        System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(name, @"\d+$");
        if (match.Success && int.TryParse(match.Value, out int number))
        {
            return number;
        }
        return 0;
    }
}