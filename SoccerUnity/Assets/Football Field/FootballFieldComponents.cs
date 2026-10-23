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

    public Vector2 getBallNormalizedPosition(Team team)
    {
        FieldPositionsData.HorizontalPositionType horizontalPositionType = FieldPositionsData.HorizontalPositionType.Right;
        Vector3 position = MatchComponents.ballPosition;
        SideOfField sideOfField = team.SideOfField;
        float verticalBallDistance = sideOfField.backPlane.GetDistanceToPoint(position) / fieldLenght;
        //verticalBallDistance = Mathf.Clamp01(verticalBallDistance);
        float horizontalBallDistance;
        if (horizontalPositionType.Equals(FieldPositionsData.HorizontalPositionType.Right))
        {
            horizontalBallDistance = sideOfField.rightPlane.GetDistanceToPoint(position) / fieldWidth;
        }
        else
        {
            horizontalBallDistance = sideOfField.leftPlane.GetDistanceToPoint(position) / fieldWidth;
        }
        //horizontalBallDistance = Mathf.Clamp01(horizontalBallDistance);

        return new Vector2(horizontalBallDistance, verticalBallDistance);
    }
    public float getBallNormalizedPositionY(Team team)
    {
        return getBallNormalizedPosition(team).y;
    }
    public float getBallNormalizedPositionX(Team team)
    {
        return getBallNormalizedPosition(team).x;
    }
}
