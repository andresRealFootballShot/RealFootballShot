using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvisibleCircularWall : MonoBehaviour
{
    List<PublicPlayerData> players;
    Transform targetTransform;
    float minDistance;
    void Start()
    {
        MatchComponents.rulesComponents.invisibleCircularWall = this;
        Disable();
    }
    public void setParameters(List<PublicPlayerData> players,Transform targetTransform,float minDistance)
    {
        this.players = players;
        this.targetTransform = targetTransform;
        this.minDistance = minDistance;
    }
    public void Enable()
    {
        enabled = true;
    }
    public void Disable()
    {
        
        enabled = false;
    }
    void Update()
    {
        foreach (var player in players)
        {
            float distance = Vector3.Distance(player.bodyTransform.position, targetTransform.position);
            if (distance < minDistance)
            {
                distance = Mathf.Clamp(distance, minDistance, distance);
                Vector3 direction = player.bodyTransform.position - targetTransform.position;
                if (direction.y < 0 || true)
                {
                    direction = MyFunctions.setYToVector3(direction, 0);
                }
                player.bodyTransform.position = MyFunctions.setYToVector3(targetTransform.position, player.bodyTransform.position.y) + direction.normalized * distance;
            }
        }
    }
}
