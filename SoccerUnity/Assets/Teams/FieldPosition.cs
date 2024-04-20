using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldPosition : MonoBehaviour
{
    //public TypeFieldPosition.Type typeFieldPosition;
    public Vector3 initPosition { get {return transform.localPosition;}}
    public Vector3 eulerAngleRotation { get { return transform.eulerAngles; } }
}
