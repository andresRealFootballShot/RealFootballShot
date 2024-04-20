using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MyRaycastHit : MonoBehaviour
{
    public RaycastHit hit { get; set; }
    public bool isHitting { get; set; }
    public LayerMask layerMask;
    public new string tag = "All";
}
