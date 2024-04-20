using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootballFieldComponents
{
    public PhysicMaterial footballFieldPhysicMaterial;
    public Area fullFieldArea;
    public List<SideOfField> sideOfFields;
    public List<CornerComponents> cornersComponents;
    public List<PlaneWithLimits> sidelines = new List<PlaneWithLimits>();
    public Vector3 position { get => transform.position; set => transform.position = value; }
    public Vector3 normal { get => transform.up.normalized; set => transform.up = value; }
    public Transform transform;
    public Vector3 center { get => transform.position; }
    public float fieldLenght { get; set; }
    public float fieldWidth { get; set; }
}
