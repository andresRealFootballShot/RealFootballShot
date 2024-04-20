using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PublicFieldPlayerData : PublicPlayerData
{
    public Variable<float> maximumJumpForceVar = new Variable<float>();
    public new Rigidbody rigidbody;
    public Animator animator;
    public float maximumJumpForce { get { return maximumJumpForceVar.Value; } set { maximumJumpForceVar.Value = value; maximumJumpHeights = addMaximumJumpHeight(); } }
    SortedList<float, Area> addMaximumJumpHeight()
    {
        SortedList<float, Area> list = new SortedList<float, Area>();
        Vector3 headPosition = animator.GetBoneTransform(HumanBodyBones.Head).TransformPoint(Vector3.up * 0.15f);
        float height = Vector3.Distance(headPosition, bodyTransform.position);
        height += ParabolaWithDrag.getMaximumY(maximumJumpForceVar.Value, rigidbody.drag, 9.81f);
        movimentValues.NormalMaximumJumpHeight = height;
        list.Add(height,null);
        return list;
    }
}
