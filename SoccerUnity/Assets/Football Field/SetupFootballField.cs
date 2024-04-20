using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupFootballField : MonoBehaviour,ILoad
{
    public PhysicMaterial physicMaterial;
    public Transform footballFieldTransform;
    public Transform fullFieldAreas;
    public Transform sideLinesRoot;
    [HideInInspector]
    public Area fullFieldArea;
    public static int _loadLevel = MatchEvents.staticLoadLevel + 1;
    public int loadLevel { get => _loadLevel; set => _loadLevel = value; }
    EventTrigger trigger = new EventTrigger();
    EventTrigger sideOfFieldAreTrigger;
    public void Load(int level)
    {
        if (loadLevel == level)
        {
            Load();
        }
    }
    void Update()
    {
        //print(MatchComponents.footballFieldComponents.fullFieldArea.PointIsInside(MatchComponents.componentsBall.rigBall.position));
    }
    void Load()
    {
        MatchComponents.footballField = new FootballFieldComponents();
        trigger.addTrigger(MatchEvents.setMainBall, true, 1, true);
        trigger.addTrigger(MatchEvents.typeMatchSetuped, true, 1, true);
        trigger.addTrigger(MatchEvents.sizeFootballFieldChanged, true, 1, true);
        trigger.addFunction(setupFoootballField);
        trigger.endLoadTrigger();
    }
    void setupFoootballField()
    {
        MatchComponents.footballField.footballFieldPhysicMaterial = physicMaterial;
        MatchComponents.footballField.transform = footballFieldTransform;
        waitSideOfFieldAreLoaded();
        loadFullFieldArea();
        loadSidelines();
    }
    void waitSideOfFieldAreLoaded()
    {
        sideOfFieldAreTrigger = new EventTrigger();
        MatchComponents.footballField.sideOfFields = SideOfFieldCtrl.getSideOfFieldsOfSizeFootballField(TypeMatch.SizeFootballField);
        foreach (var sideOfField in MatchComponents.footballField.sideOfFields)
        {
            if (sideOfField.enabled)
            {
                sideOfFieldAreTrigger.addTrigger(sideOfField.isLoadedEvent, false, 1, true);
            }
        }
        sideOfFieldAreTrigger.addFunction(setSideOfFieldsComponents);
        sideOfFieldAreTrigger.endLoadTrigger();
    }

    void setSideOfFieldsComponents()
    {
        MatchComponents.footballField.sideOfFields = SideOfFieldCtrl.getSideOfFieldsOfSizeFootballField(TypeMatch.SizeFootballField);
        MatchComponents.footballField.cornersComponents = new List<CornerComponents>();
        foreach (var sideOfField in MatchComponents.footballField.sideOfFields)
        {
           MatchComponents.footballField.cornersComponents.AddRange(sideOfField.corners);
           MatchComponents.footballField.fieldLenght +=sideOfField.sideOfFieldLenght;
           MatchComponents.footballField.fieldWidth =sideOfField.sideOfFieldWidth;
        }
        MatchEvents.footballFieldLoaded.Invoke();
    }
    public void loadFieldDimensions(List<SideOfField> list)
    {
        MatchComponents.footballField.fieldLenght = 0;
        foreach (var sideOfField in list)
        {
            MatchComponents.footballField.fieldLenght += sideOfField.sideOfFieldLenght;
            MatchComponents.footballField.fieldWidth = sideOfField.sideOfFieldWidth;
        }
    }
    void loadSidelines()
    {
        SizeFootballField[] sizeFootballSidelines = sideLinesRoot.GetComponentsInChildren<SizeFootballField>();
        float ballRadio = MatchComponents.ballComponents.radio;
        //MatchComponents.footballField.sidelines.Clear();
        foreach (var sizeFootballFieldArea in sizeFootballSidelines)
        {
            
            if (sizeFootballFieldArea.Value == TypeMatch.SizeFootballField)
            {
                PlaneWithLimits[] planeWithLimitsList = sizeFootballFieldArea.gameObject.GetComponentsInChildren<PlaneWithLimits>();
                foreach (var planeWithLimits in planeWithLimitsList)
                {
                    Vector3 offset = new Vector3(0, 0, -ballRadio);
                    planeWithLimits.buildPlanes(offset);
                    MatchComponents.footballField.sidelines.Add(planeWithLimits);
                }
            }
        }
        if (MatchComponents.footballField.fullFieldArea == null)
        {
            Debug.LogError("Falta fullFieldArea en el campo " + TypeMatch.SizeFootballField.ToString());
        }
    }
    void loadFullFieldArea()
    {
        SizeFootballField[] sizeFootballFieldAreas = fullFieldAreas.GetComponentsInChildren<SizeFootballField>();
        float ballRadio = MatchComponents.ballComponents.radio;
        foreach (var sizeFootballFieldArea in sizeFootballFieldAreas)
        {
            if (sizeFootballFieldArea.Value==TypeMatch.SizeFootballField)
            {
                Area area = sizeFootballFieldArea.gameObject.GetComponent<Area>();
                if (area != null)
                {
                    Vector3 offset = new Vector3(-ballRadio, 0, -ballRadio);
                    area.buildArea(offset);
                    MatchComponents.footballField.fullFieldArea = area;
                    break;
                }
            }
        }
        if (MatchComponents.footballField.fullFieldArea == null)
        {
            Debug.LogError("Falta fullFieldArea en el campo "+ TypeMatch.SizeFootballField.ToString());
        }
    }
}
