using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGizmo : MonoBehaviour
{
    public Color color;
    public Color selectionColor;
    public float size;
    public bool active=true;
    void OnDrawGizmos()
    {
        if (active)
        {
            Gizmos.color = color;
            //Gizmos.DrawSphere(transform.position, sizeGizmo);
            Gizmos.DrawSphere(transform.position, size);
            Gizmos.color = Color.yellow;
        }
    }
    #if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (active)
        {
            if (UnityEditor.Selection.activeGameObject == gameObject)
            {
                Gizmos.color = selectionColor;
                Gizmos.DrawSphere(transform.position, size);
            }
        }
    }
#endif
}
