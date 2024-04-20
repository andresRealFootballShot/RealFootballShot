using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptsPlayer : MonoBehaviour
{
    public ControllerAim controllerAim;
    public ControllerZoneAim controllerZoneAim;
    public ControllerDistance controllerDistance;
    public MyRaycastHit raycastAim;
    public MyRaycastHit raycastWall;
    public PlayTimeMenu menu;
    public MenuOptions menuOptions;
    public CameraRotation cameraRotation;
    public CameraPosition cameraPosition;
    public ArrowController arrowController;
    public MovimentValues movimentValues;
    public HudCtrl hudCtrl;
    public Touch touch;
    public TouchWithDirect touchWithDirect;
    public Shot shot;
    public ResistanceController resistanceController;
    public Velocity velocity;
    public TransparencyCtrl transparencyCtrl;
    private void Awake()
    {
        
    }
}
