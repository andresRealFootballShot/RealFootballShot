using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class SetGizmosCtrl : MonoBehaviour
{
    public List<string> gizmosNames;
    public List<Color> colors;
    public Color selectectionColor;
    public bool active;
    public float size=0.1f;
    void OnDrawGizmos()
    {
        MyGizmo[] gizmos = transform.GetComponentsInChildren<MyGizmo>();
        if (active)
        {
            foreach (var gizmo in gizmos)
            {
                gizmo.active = true;
                gizmo.size = size;
                for (int i = 0; i < gizmosNames.Count; i++)
                {
                    if (gizmo.name.Equals(gizmosNames[i]))
                    {
                        if (i < colors.Count)
                        {
                            gizmo.color = colors[i];
                        }
                        gizmo.selectionColor = selectectionColor;
                    }
                }
            }
        }
        else
        {
            foreach (var gizmo in gizmos)
            {
                gizmo.active = false;
                gizmo.size = size;
            }
        }
    }
}
