using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

[CustomEditor(typeof(FootballPositionCtrl))]
public class FootballPositionCtrlEditor : Editor
{
    [System.Serializable]
    public class FieldPointList
    {
        public List<FieldPositionsData.Point2> list;
    }
    
    private void OnEnable()
    {
        var t = (target as FootballPositionCtrl);
        t.Load();
    }
   
    public override void OnInspectorGUI()
    {

        var t = (target as FootballPositionCtrl);
        base.DrawDefaultInspector();
        if (!t.debug) return;

        serializedObject.Update();
        t.playerPositionType = (PlayerPositionType)EditorGUILayout.EnumPopup("PlayerPosition", t.playerPositionType);
        t.lineupName = EditorGUILayout.TextField("Lineup Name", t.lineupName);
        t.pressureName = EditorGUILayout.TextField("Lineup Name", t.pressureName);
        t.playerSize = EditorGUILayout.IntField("Lineup Name", t.playerSize);
        LineupFieldPositionDatas lineupFieldPositionDatas;

        if(!t.getCurrentLineup(out lineupFieldPositionDatas)) return;

        PressureFieldPositionDatas pressureFieldPositionDatas;
        if (!t.getCurrentPressureFieldPositions(out pressureFieldPositionDatas)) return;
        
        FieldPositionsData fieldPositionParameters;
        if (!t.GetSelectedFieldPositionParameters(out fieldPositionParameters)) return;
        
        if (GUILayout.Button("Duplicate Player Points"))
        {
            PressureFieldPositionDatas currentFieldPositionList;
            if (t.getCurrentPressureFieldPositions(out currentFieldPositionList))
            {
                FieldPositionsData cloneFieldPositionParameters = fieldPositionParameters.Clone();
                currentFieldPositionList.FieldPositionDatas.Add(cloneFieldPositionParameters);
            }
        }
        EditorGUILayout.Space(10.0f);
        EditorGUILayout.LabelField("Selected Point", EditorStyles.boldLabel);
        //FieldPositionParameters.Point selectedPoint = fieldPositionParameters.selectedPoint;
        if (selectedPoint != null)
        {

            EditorGUILayout.LabelField(selectedPoint.info);
            if (true || fieldPositionParameters.points.Contains(selectedPoint))
            {
                
                selectedPoint.point = EditorGUILayout.Vector2Field("Point", selectedPoint.point);
                selectedPoint.value = EditorGUILayout.Vector2Field("Value", selectedPoint.value);
                selectedPoint.radio = EditorGUILayout.FloatField("Radio", selectedPoint.radio);
                
                EditorGUI.BeginChangeCheck();
                selectedPoint.enabled = EditorGUILayout.Toggle("enabled", selectedPoint.enabled);
                selectedPoint.useRadio = EditorGUILayout.Toggle("use radio", selectedPoint.useRadio);
                selectedPoint.snap = EditorGUILayout.Toggle("snap", selectedPoint.snap);
                selectedPoint.weight = EditorGUILayout.Slider("Weight", selectedPoint.weight, 0, 1);
                if (EditorGUI.EndChangeCheck())
                {
                    SceneView.RepaintAll();
                }
            }
        }
        else
        {
            if (fieldPositionParameters.points.Count > 0)
            {
                selectedPoint = fieldPositionParameters.points[0];
            }
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Point"))
        {
            float a = 0.03f;
            Vector2 random1 = new Vector2(UnityEngine.Random.Range(-a, a), UnityEngine.Random.Range(-a, a));
            for (int i = 0; i < t.playerSize; i++)
            {
                Vector2 random2 = new Vector2(UnityEngine.Random.Range(-a, a), UnityEngine.Random.Range(-a, a));

                FieldPositionsData.Point2 newPoint = new FieldPositionsData.Point2(t.newPointPosition + random1, t.newPointPosition + random2);
                pressureFieldPositionDatas.FieldPositionDatas[i].points.Add(newPoint);
                selectedPoint = newPoint;
                selectedPointIndex = pressureFieldPositionDatas.FieldPositionDatas[i].points.Count-1;
            }
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Duplicate Point"))
        {
            for (int i = 0; i < t.playerSize; i++)
            {
                Vector2 newValue = pressureFieldPositionDatas.FieldPositionDatas[i].points[selectedPointIndex].value;
                FieldPositionsData.Point2 newPoint = new FieldPositionsData.Point2(selectedPoint.point, newValue);
                pressureFieldPositionDatas.FieldPositionDatas[i].points.Add(newPoint);
                selectedPoint = newPoint;
                
            }
            selectedPointIndex = pressureFieldPositionDatas.FieldPositionDatas[0].points.Count - 1;
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Remove Point"))
        {
            if(selectedPoint!=null)
            {
                for (int i = 0; i < t.playerSize; i++)
                {
                    pressureFieldPositionDatas.FieldPositionDatas[i].points.RemoveAt(selectedPointIndex);
                }
                selectedPointIndex=0;
                selectedPoint = null;
                SceneView.RepaintAll();
            }
        }
        if (GUILayout.Button("Save"))
        {
            string jsonString = JsonUtility.ToJson(t.LineupFieldPositionList);
            File.WriteAllText(Application.dataPath + "/Player/Player Scripts/Football/FootballPosition/FieldPoints.json", jsonString);
        }
        if (GUILayout.Button("Load"))
        {
            string text = File.ReadAllText(Application.dataPath + "/Player/Player Scripts/Football/FootballPosition/FieldPoints.json");
            LineupFieldPositionDatasList LineupFieldPositionDatasList = JsonUtility.FromJson<LineupFieldPositionDatasList>(text);
            t.LineupFieldPositionList= LineupFieldPositionDatasList;
        }
        GUILayout.EndHorizontal();
        EditorGUILayout.Space(10.0f);
        EditorGUILayout.LabelField("Offside Line", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Offside Line"))
        {
            OffsideLine offsideLine = new OffsideLine(t.newPointPosition.y, t.newPointPosition.y-0.02f);
            t.AddOffsideLine(pressureFieldPositionDatas, offsideLine);
            selectedOffsideLine = offsideLine;
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Remove Offside Line"))
        {
            if (selectedOffsideLine != null)
            {
                pressureFieldPositionDatas.offsideLines.Remove(selectedOffsideLine);
                selectedOffsideLine = null;
                SceneView.RepaintAll();
            }
        }
        
        GUILayout.EndHorizontal();
        if (selectedOffsideLine != null)
        {
            EditorGUI.BeginChangeCheck();
            selectedOffsideLine.enabled = EditorGUILayout.Toggle("enabled", selectedOffsideLine.enabled);
            selectedOffsideLine.stop = EditorGUILayout.Toggle("stop", selectedOffsideLine.stop);
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }
        }

        EditorGUILayout.Space(10.0f);
        EditorGUILayout.LabelField("Offside Stop", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Offside Stop"))
        {
            OffsideStop offsideStop = new OffsideStop(t.newPointPosition, 0.1f);
            pressureFieldPositionDatas.offsideStops.Add(offsideStop);
            selectedOffsideStop = offsideStop;
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Remove Offside Stop"))
        {
            if (selectedOffsideStop != null)
            {
                pressureFieldPositionDatas.offsideStops.Remove(selectedOffsideStop);
                selectedOffsideStop = null;
                SceneView.RepaintAll();
            }
        }
        GUILayout.EndHorizontal();
        if (selectedOffsideStop != null)
        {
            EditorGUI.BeginChangeCheck();
            selectedOffsideStop.enabled = EditorGUILayout.Toggle("enabled", selectedOffsideStop.enabled);
            selectedOffsideStop.radio = EditorGUILayout.FloatField("radio", selectedOffsideStop.radio);
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }
        }
        //fieldPositionParameters.selectedPoint = selectedPoint;
        //t.selectedFieldPositionParameters = fieldPositionParameters;
        serializedObject.ApplyModifiedProperties();
    }

    FieldPositionsData.Point2 selectedPoint;
    FieldPositionsData.Point2 selectedValue;
    OffsideLine selectedOffsideLine;
    OffsideStop selectedOffsideStop;
    int selectedPointIndex;
    public void OnSceneGUI()
    {
        var t = (target as FootballPositionCtrl);
        if (!t.debug) return;
        PressureFieldPositionDatas PressureFieldPositionDatas;
        if (!t.getCurrentPressureFieldPositions(out PressureFieldPositionDatas)) return;

        FieldPositionsData fieldPositionParameters;
        if (!t.GetSelectedFieldPositionParameters(out fieldPositionParameters)) return;
       
        //FieldPositionParameters.Point selectedPoint = fieldPositionParameters.selectedPoint;
        float buttonSize = t.buttonSize;
        int i = 0;
        float a = 0.25f;
        GUIStyle style2 = new GUIStyle();
        foreach (var item in fieldPositionParameters.points)
        {
            Color color = Color.red;
            if (!item.enabled) color.a = a;
            Handles.color = color;
            if(!t.debugOnlyPlayerPositions)
            if (Handles.Button(t.getGlobalPosition(t.horizontalPositionType, item.point), Quaternion.identity, buttonSize, 1, Handles.SphereHandleCap))
            {
                selectedPoint = item;
                selectedPointIndex = i;
                selectedOffsideStop = null;
                Repaint();
            }
            color = Color.blue;
            if (!item.enabled) color.a = a;
           
            style2.fontSize = 14;
            color= Color.yellow;
            if (!item.enabled) color.a = a;
            style2.normal.textColor = color;
            item.info = "Point " + i;
            if (!t.debugOnlyPlayerPositions)
                if (t.debugText)
            Handles.Label(t.getGlobalPosition(t.horizontalPositionType, item.point) + Vector3.up * 1f, item.info, style2);
            
            i++;
        }
        i = 0;
        Handles.color = Color.blue;
        foreach (var FieldPositionData in PressureFieldPositionDatas.FieldPositionDatas)
        {
            if (t.debugAllValues)
            {
                int j = 0;
                foreach (var point in FieldPositionData.points)
                {
                    Color color = Color.blue;
                    if (!point.enabled) color.a = a;
                    Handles.color = color;
                    if (Handles.Button(t.getGlobalPosition(t.horizontalPositionType, point.value), Quaternion.identity, buttonSize, 1, Handles.SphereHandleCap))
                    {
                        selectedPoint = point;
                        selectedPointIndex = j;
                        selectedOffsideStop = null;
                        Repaint();
                    }

                    style2.fontSize = 14;
                    Color textColor = Color.yellow;
                    if (!point.enabled) textColor.a = a;
                    style2.normal.textColor = textColor;
                    point.info = FieldPositionData.playerPositionType.ToString() + " " + j;
                    if (t.debugText)
                        Handles.Label(t.getGlobalPosition(t.horizontalPositionType, point.value) + Vector3.up * 1f, point.info, style2);
                    j++;
                }
            }
            else
            {
                FieldPositionsData.Point2 point = FieldPositionData.points[selectedPointIndex];
                Color color = Color.blue;
                if (!point.enabled) color.a = a;
                Handles.color = color;
                if (!t.debugOnlyPlayerPositions)
                    if ( Handles.Button(t.getGlobalPosition(t.horizontalPositionType, point.value), Quaternion.identity, buttonSize, 1, Handles.SphereHandleCap))
                    {
                        selectedPoint = point;
                        selectedOffsideStop = null;
                        Repaint();
                    }

                style2.fontSize = 14;
                Color textColor = Color.yellow;
                if (!point.enabled) textColor.a = a;
                style2.normal.textColor = textColor;
                point.info = FieldPositionData.playerPositionType.ToString() + " " + selectedPointIndex;
                if (!t.debugOnlyPlayerPositions)
                    if (t.debugText)
                    Handles.Label(t.getGlobalPosition(t.horizontalPositionType, point.value) + Vector3.up * 1f, point.info, style2);

            }
            i++;
        }
        int k = 0;
        foreach (var offsideLine in PressureFieldPositionDatas.offsideLines)
        {
            Color color = Color.green;
            if (!offsideLine.enabled) color.a = a;
            Handles.color = color;
            Vector2 normalizedPos = new Vector2(0, offsideLine.yPos);
            if (!t.debugOnlyPlayerPositions)
                if (Handles.Button(t.getGlobalPosition(t.horizontalPositionType, normalizedPos), Quaternion.identity, buttonSize, 1, Handles.SphereHandleCap))
            {
                selectedOffsideLine = offsideLine;
                Repaint();
            }
            style2.normal.textColor = Color.yellow;
            if (!t.debugOnlyPlayerPositions)
                if (t.debugText)
                Handles.Label(t.getGlobalPosition(t.horizontalPositionType, normalizedPos) + Vector3.up * 1f, "Offside Line "+k, style2);
            k++;
        }
        if (selectedOffsideLine != null)
        {
            EditorGUI.BeginChangeCheck();
            Vector2 normalizedPos = new Vector2(0, selectedOffsideLine.yPos);
            if (!t.debugOnlyPlayerPositions)
            {
                Vector3 pos4 = Handles.PositionHandle(t.getGlobalPosition(t.horizontalPositionType, normalizedPos), Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    selectedOffsideLine.yPos = t.getNormalizedPosition(t.horizontalPositionType, pos4).y;
                }

                EditorGUI.BeginChangeCheck();
                Vector2 normalizedValue = new Vector2(0, selectedOffsideLine.yValue);
                Vector3 pos5 = Handles.PositionHandle(t.getGlobalPosition(t.horizontalPositionType, normalizedValue), Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    selectedOffsideLine.yValue = t.getNormalizedPosition(t.horizontalPositionType, pos5).y;
                }
                if (t.debugText)
                    Handles.Label(pos5 + Vector3.up * 0.5f, "Offside Value", style2);
            }
                
        }
        else
        {
            if (PressureFieldPositionDatas.offsideLines.Count > 0)
            {
                selectedOffsideLine = PressureFieldPositionDatas.offsideLines[0];
            }
            //Repaint();
        }
        Color color5 = new Color(1, 1, 0.5f);
        Handles.color = color5;
        GUIStyle style4 = new GUIStyle();
        style4.fontSize = 14;
        style4.normal.textColor = color5;
        float offsideWeight;
        float offsideLineValueY = t.GetOffsideLineGetValue(PressureFieldPositionDatas, t.normailizedBallPosition,out offsideWeight);
        Vector3 globalResultOffsideLineValue = t.getGlobalPosition(t.horizontalPositionType, new Vector2(0, offsideLineValueY));
        if (!t.debugOnlyPlayerPositions)
            if (t.debugText)
            Handles.Label(globalResultOffsideLineValue + Vector3.up * 1.5f, "Offside Line Result", style4);
        Handles.SphereHandleCap(0,
           globalResultOffsideLineValue,
           Quaternion.identity,
           0.5f,
           EventType.Repaint
       );

        k = 0;
        Color color2 = new Color(0.5f, 1, 0.5f,1);
        foreach (var offsideStop in PressureFieldPositionDatas.offsideStops)
        {
            if (!offsideStop.enabled) color2.a = a;
            Handles.color = color2;
            if (!t.debugOnlyPlayerPositions)
                if (Handles.Button(t.getGlobalPosition(t.horizontalPositionType, offsideStop.point), Quaternion.identity, buttonSize, 1, Handles.SphereHandleCap))
                {
                    selectedOffsideStop = offsideStop;
                    Repaint();
                }
            style2.normal.textColor = color2;
            if (!t.debugOnlyPlayerPositions)
                if (t.debugText)
                Handles.Label(t.getGlobalPosition(t.horizontalPositionType, offsideStop.point) + Vector3.up * 1f, "Offside Stop " + k, style2);
            k++;
        }

        if (selectedOffsideStop != null)
        {
            if (!selectedOffsideStop.enabled) color2.a = a;
            if (!t.debugOnlyPlayerPositions)
            {
                Handles.color = color2;
                EditorGUI.BeginChangeCheck();
                Vector2 normalizedPos = selectedOffsideStop.point;
                Vector3 globalPoint = t.getGlobalPosition(t.horizontalPositionType, normalizedPos);
                Vector3 pos4 = Handles.PositionHandle(globalPoint, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    selectedOffsideStop.point = t.getNormalizedPosition(t.horizontalPositionType, pos4);
                }

                EditorGUI.BeginChangeCheck();
                float scaleSize = 10;
                Vector3 scale = Handles.ScaleHandle(Vector3.one * (selectedOffsideStop.radio + 0.01f) * scaleSize, globalPoint + Vector3.up * 1, Quaternion.identity, 1);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Scaled ScaleAt Point");
                    selectedOffsideStop.radio = Mathf.Clamp(scale.x / scaleSize, 0, Mathf.Infinity);
                }
                Handles.color = color2;
                Handles.DrawWireDisc(globalPoint, new Vector3(0, 1, 0), selectedOffsideStop.radio * t.fieldWidth);
            }
                
        }




        EditorGUI.BeginChangeCheck();
        if (!t.debugOnlyPlayerPositions)
        {
            Vector3 pos1 = Handles.PositionHandle(t.getGlobalPosition(t.horizontalPositionType, t.newPointPosition), Quaternion.identity);
            GUIStyle style = new GUIStyle();
            style.fontSize = 14;
            style.normal.textColor = Color.yellow;
            if (t.debugText)
                Handles.Label(pos1 + Vector3.up * 0.5f, "New Point", style);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Move point");
                t.newPointPosition = t.getNormalizedPosition(t.horizontalPositionType, pos1);
                Repaint();
                //t.Update();
            }
        }
           


        EditorGUI.BeginChangeCheck();

            Vector3 pos3 = Handles.PositionHandle(t.getGlobalPosition(t.horizontalPositionType, t.normailizedBallPosition), Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                //Undo.RecordObject(target, "Move point");
                t.normailizedBallPosition = t.getNormalizedPosition(t.horizontalPositionType, pos3);
                //Repaint();
                //t.Update();
            }
            GUIStyle style3 = new GUIStyle();
            style3.fontSize = 14;
            style3.normal.textColor = Color.cyan;
            if (t.debugText)
                Handles.Label(pos3 + Vector3.up * 1.5f, "Ball position", style3);
            Handles.color = Color.cyan;
            Handles.DrawWireDisc(pos3 + Vector3.up * 0.25f, Vector3.up, 0.4f);
        
        
        Vector2 weightyValue = Vector2.zero;
        foreach (var FieldPositionData in PressureFieldPositionDatas.FieldPositionDatas)
        {
            Handles.color = Color.black;
            t.getWeightyValue4(t.normailizedBallPosition, FieldPositionData.points,offsideLineValueY,FieldPositionData.playerPositionType, offsideWeight, out weightyValue);
            
            Handles.SphereHandleCap(0, t.getGlobalPosition(t.horizontalPositionType, weightyValue) + Vector3.up * 0.25f, Quaternion.identity, 0.5f, EventType.Repaint);
            string info = FieldPositionData.playerPositionType.ToString();
            GUIStyle style = new GUIStyle();
            style.fontSize = 16;
            style.normal.textColor = Color.black;
            if (t.debugText)
                Handles.Label(t.getGlobalPosition(t.horizontalPositionType, weightyValue) + Vector3.up * 1f, info, style);

            if (t.debugSymmetricalPositions)
            {
                Vector2 weightValue2 = Vector2.zero;
                Vector2 symmetricalBallPosition = t.normailizedBallPosition;
                symmetricalBallPosition.x = 1 - symmetricalBallPosition.x;
                t.getWeightyValue4(symmetricalBallPosition, FieldPositionData.points, offsideLineValueY, FieldPositionData.playerPositionType, offsideWeight, out weightValue2);
                Handles.color = Color.grey;
                FieldPositionsData.HorizontalPositionType otherHorizontalPositionType = t.getOtherHorizontalPositionType(t.horizontalPositionType);
                Handles.SphereHandleCap(0, t.getGlobalPosition(otherHorizontalPositionType, weightValue2) + Vector3.up * 0.25f, Quaternion.identity, 0.5f, EventType.Repaint);
                info = FieldPositionData.playerPositionType.ToString();
                style.fontSize = 16;
                style.normal.textColor = Color.white;
                if (t.debugText)
                    Handles.Label(t.getGlobalPosition(otherHorizontalPositionType, weightValue2) + Vector3.up * 1f, info, style);
            }

        }
        if (selectedPoint != null && !t.debugOnlyPlayerPositions)
        {
            if (true || fieldPositionParameters.points.Contains(selectedPoint))
            {
                Vector3 selectedGlobalPoint = t.getGlobalPosition(t.horizontalPositionType, selectedPoint.point);
                Vector3 selectedGlobalValue = t.getGlobalPosition(t.horizontalPositionType, selectedPoint.value);
                EditorGUI.BeginChangeCheck();
                Vector3 pos2 = selectedGlobalPoint;
                
                Vector3 pos = Handles.PositionHandle(selectedGlobalPoint, Quaternion.identity);
                
                if (EditorGUI.EndChangeCheck())
                {

                    Undo.RecordObject(target, "Move point");
                    Vector2 normalizedPosition = t.getNormalizedPosition(t.horizontalPositionType, pos);
                    selectedPoint.point = normalizedPosition;
                    for ( i = 0; i < PressureFieldPositionDatas.FieldPositionDatas.Count; i++)
                    {
                        PressureFieldPositionDatas.FieldPositionDatas[i].points[selectedPointIndex].point = normalizedPosition;
                    }

                    Event e = Event.current;
                    if (e.alt)
                    {
                        foreach (var FieldPositionData in PressureFieldPositionDatas.FieldPositionDatas)
                        {
                            Vector3 globalPos = t.getGlobalPosition(t.horizontalPositionType, FieldPositionData.points[selectedPointIndex].value);
                            
                            Vector3 offset = globalPos + pos - pos2;
                            Vector2 offset2 = t.getNormalizedPosition(t.horizontalPositionType, offset);
                            FieldPositionData.points[selectedPointIndex].value = offset2;
                        }
                    }
                    Repaint();
                    //t.Update();
                }
                Handles.color = Color.blue;
                foreach (var FieldPositionData in PressureFieldPositionDatas.FieldPositionDatas)
                {
                    FieldPositionsData.Point2 point = FieldPositionData.points[selectedPointIndex];
                    if (!point.snap && point.enabled)
                        Handles.SphereHandleCap(0,
                           t.getGlobalPosition(t.horizontalPositionType, point.value),
                           Quaternion.identity,
                           buttonSize * 0.9f,
                           EventType.Repaint
                       );
                }
                EditorGUI.BeginChangeCheck();
                float scaleSize = 10;
                Vector3 scale = Handles.ScaleHandle(Vector3.one* (selectedPoint.radio+0.01f)* scaleSize, selectedGlobalPoint+Vector3.up*3, Quaternion.identity, 1);
                
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Scaled ScaleAt Point");
                    selectedPoint.radio = Mathf.Clamp(scale.x / scaleSize, 0, Mathf.Infinity);
                   
                    
                }
                Handles.color = Color.yellow;
                if(selectedPoint.useRadio && selectedPoint.enabled)
                    Handles.DrawWireDisc(selectedGlobalPoint, new Vector3(0, 1, 0), selectedPoint.radio* t.fieldWidth);
                
                if (!selectedPoint.snap && selectedPoint.enabled)
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3 pos5 = Handles.PositionHandle(selectedGlobalValue, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Move point");
                        selectedPoint.value = t.getNormalizedPosition(t.horizontalPositionType, pos5);
                        Repaint();
                        //t.Update();
                    }
                }
                
            }
        }
        else{
            if (fieldPositionParameters.points.Count > 0)
            {
                selectedPoint = fieldPositionParameters.points[0];
            }
            //Repaint();
        }

        if (t.debugRadios) DrawRaidos(fieldPositionParameters,t);
        //DrawPositions(fieldPositionParameters,t);
        //fieldPositionParameters.selectedPoint = selectedPoint;
        //t.selectedFieldPositionParameters = fieldPositionParameters;
    }
    void DrawPositions(FieldPositionsData fieldPositionParameters, FootballPositionCtrl t,float offsidePositionY,float offsideWeight)
    {
        if (!t.debug || !t.debugAllPositions) return;
        PressureFieldPositionDatas fieldPositionList;
        t.getCurrentPressureFieldPositions(out fieldPositionList);
        if (fieldPositionList == null) return;
        foreach (var fieldPositions in fieldPositionList.FieldPositionDatas)
        {
            if (fieldPositionParameters.playerPositionType.Equals(fieldPositions.playerPositionType)) continue;
                Vector2 weightyValue;
                t.getWeightyValue4(t.normailizedBallPosition, fieldPositions.points,offsidePositionY, fieldPositions.playerPositionType,offsideWeight, out weightyValue);
                Vector3 globalPosition = t.getGlobalPosition(t.horizontalPositionType, weightyValue);
                Handles.color = Color.grey;
                GUIStyle style3 = new GUIStyle();
                style3.fontSize = 14;
                style3.normal.textColor = Color.green;
                Handles.Label(globalPosition + Vector3.up * 1.5f, fieldPositions.playerPositionType.ToString(), style3);
                Handles.SphereHandleCap(0,
                   globalPosition,
                   Quaternion.identity,
                   0.5f,
                   EventType.Repaint
               );

            
        }
    }
    void DrawRaidos(FieldPositionsData fieldPositionParameters, FootballPositionCtrl t)
    {
        foreach (var point in fieldPositionParameters.points)
        {
            if (point.useRadio && point.enabled)
            {
                Handles.color = Color.yellow;
                Handles.DrawWireDisc(t.getGlobalPosition(t.horizontalPositionType, point.point), new Vector3(0, 1, 0), point.radio * t.fieldWidth);
            }
        }
        //Repaint();
    }
}
