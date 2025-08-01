using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

public class AnimationCurveChecker : EditorWindow
{
    [MenuItem("Tools/AnimationCurve Checker")]
    public static void ShowWindow()
    {
        GetWindow<AnimationCurveChecker>("AnimationCurve Checker");
    }

    private Vector2 scroll;
    private List<string> issues = new List<string>();

    void OnGUI()
    {
        if (GUILayout.Button("Escanear proyecto"))
        {
            ScanProject();
        }

        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach (var issue in issues)
        {
            EditorGUILayout.HelpBox(issue, MessageType.Warning);
        }
        EditorGUILayout.EndScrollView();
    }

    void ScanProject()
    {
        issues.Clear();

        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();

        foreach (string path in allAssetPaths)
        {
            Object obj = AssetDatabase.LoadMainAssetAtPath(path);

            if (obj == null || obj is MonoScript) continue;

            Component[] components = null;

            if (obj is GameObject go)
            {
                components = go.GetComponentsInChildren<Component>(true);
            }
            else if (obj is ScriptableObject so)
            {
                CheckObjectForCurves(so, path);
                continue;
            }

            if (components != null)
            {
                foreach (var comp in components)
                {
                    if (comp == null) continue;
                    CheckObjectForCurves(comp, path + " → " + comp.GetType().Name);
                }
            }
        }

        Debug.Log($"Escaneo completo. {issues.Count} posibles problemas encontrados.");
    }

    void CheckObjectForCurves(Object obj, string path)
    {
        var fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var field in fields)
        {
            if (field.FieldType == typeof(AnimationCurve))
            {
                var value = field.GetValue(obj) as AnimationCurve;
                if (value == null)
                {
                    issues.Add($"{path} → Campo '{field.Name}' es NULL");
                }
                else if (value.keys.Length == 0)
                {
                    issues.Add($"{path} → Campo '{field.Name}' está vacío (sin keyframes)");
                }
            }
        }
    }
}