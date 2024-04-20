using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void emptyDelegate();
public delegate void genericDelegate<T>(T value);
public delegate bool boolDelegate();
public delegate void stateDelegate(ObsoleteState state);
public delegate void calculoDelegate(Calculo calculo);
public delegate void stringDelegate(string value);
public delegate bool conditionDelegate();
public delegate void floatDelegate(float value);
public delegate float getTimeToReachPointDelegate(Vector3 value,float scope);
public delegate void gameObjectDelegate(GameObject value);
public delegate void chaserDataResultDelegate(List<ChaserData> result);