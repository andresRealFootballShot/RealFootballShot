using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Velocity : MonoBehaviour
{
    /*public float velocity
    {
        get { return _value; }
        set { _value = value; if(stringMy!=null)stringMy.Value = value.ToString("0.0"); }
    }*/
    public ComponentsPlayer componentsPlayer;
    public TextHUD textHUD;
    public Vector3 previousPos;
    //public StringMy stringMy;

    public float velocityNormalized() => componentsPlayer.scriptsPlayer.movimentValues.velocityObsolete.Value / componentsPlayer.scriptsPlayer.movimentValues.maxSpeed.Value;
    void Start()
    {
        previousPos = componentsPlayer.transBody.position;
        
    }
    //No cambiar a update.En update se calcula mal la velocidad
    void FixedUpdate()
    {
        //calculateVelocity(Time.fixedDeltaTime);
    }
    void calculateVelocity(float delta)
    {
        float distanceMoved = Vector3.Distance(componentsPlayer.transBody.position, previousPos) / delta;
        Vector3 dir = previousPos - componentsPlayer.transBody.position;
        //print((dir / delta).magnitude);
        componentsPlayer.playerComponents.playerData.Velocity= dir/delta;
        textHUD.text.text = componentsPlayer.playerComponents.playerData.Speed.ToString("f1");
        previousPos = componentsPlayer.transBody.position;
    }
}
