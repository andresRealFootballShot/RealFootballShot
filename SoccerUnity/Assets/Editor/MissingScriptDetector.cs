using UnityEngine;
using UnityEditor;

public class MissingScriptDetector
{
    [MenuItem("Tools/Buscar Componentes Faltantes")]
    public static void FindMissingScripts()
    {
        int missingCount = 0;
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();

        foreach (GameObject go in allObjects)
        {
            Component[] components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    Debug.LogWarning($"GameObject '{go.name}' en la escena tiene un componente faltante", go);
                    missingCount++;
                }
            }
        }

        Debug.Log($"Búsqueda completada. Total de componentes faltantes: {missingCount}");
    }
}
