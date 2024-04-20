using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipement : MonoBehaviour
{
    public Color mainColor;
    public Variable<Color> mainColorVar = new Variable<Color>();

    public void Load()
    {
        mainColorVar.Value = mainColor;
    }
}
