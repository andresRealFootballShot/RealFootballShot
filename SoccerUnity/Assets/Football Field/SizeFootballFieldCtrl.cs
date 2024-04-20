using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeFootballFieldCtrl : MonoBehaviour
{
    public Transform parent;
    static Dictionary<SizeFootballFieldID, Transform> midfields = new Dictionary<SizeFootballFieldID, Transform>();
    static string Midfield = "Midfield";
    private void Awake()
    {
        //sideOfFields.Add(SideOfFieldID.One,new SideOfFieldData(sideOfFieldOne));
        //sideOfFields.Add(SideOfFieldID.Two, new SideOfFieldData(sideOfFieldOne));
        midfields.Clear();
        getData();
    }
    void getData()
    {
        List<SizeFootballField> sizeFootballFields = MyFunctions.GetComponentsInChilds<SizeFootballField>(parent.gameObject, true,true);
        foreach (var sizeFootballField in sizeFootballFields)
        {
            Transform midfield = sizeFootballField.transform.Find(Midfield);
            midfields.Add(sizeFootballField.Value, midfield);
        }
    }
    public static Transform getMidField(SizeFootballFieldID sizeFootballFieldID)
    {
        return midfields[sizeFootballFieldID];
    }
    public static Transform getMidField()
    {
        return midfields[TypeMatch.SizeFootballField];
    }
}
