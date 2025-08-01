using UnityEditor;
using UnityEngine;
using System.IO;

public class CheckPrefabsForIssues
{
    [MenuItem("Tools/Scan Prefabs for Errors")]
    public static void ScanPrefabs()
    {
        string[] prefabPaths = AssetDatabase.GetAllAssetPaths();
        int totalChecked = 0;
        int totalErrors = 0;

        foreach (string path in prefabPaths)
        {
            if (!path.EndsWith(".prefab")) continue;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            totalChecked++;

            if (prefab == null)
            {
                Debug.LogError($"❌ Prefab corrupt or failed to load: {path}");
                totalErrors++;
                continue;
            }

            Component[] components = prefab.GetComponentsInChildren<Component>(true);
            foreach (Component comp in components)
            {
                if (comp == null)
                {
                    Debug.LogError($"⚠️ Missing component in prefab: {path}");
                    totalErrors++;
                    continue;
                }

                SerializedObject so = new SerializedObject(comp);
                SerializedProperty prop = so.GetIterator();

                while (prop.NextVisible(true))
                {
                    if (prop.propertyType == SerializedPropertyType.ObjectReference && prop.objectReferenceValue == null && prop.name != "m_Script")
                    {
                        Debug.LogWarning($"🟡 Null reference in {comp.GetType().Name} on prefab: {path}, field: {prop.displayName}");
                    }
                }
            }
        }

        Debug.Log($"✅ Finished scanning {totalChecked} prefabs. Total issues found: {totalErrors}");
    }
}
