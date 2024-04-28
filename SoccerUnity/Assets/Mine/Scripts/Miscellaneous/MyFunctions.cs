using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
public static class MyFunctions
{
    public static bool SolveQuadratic(float a, float b, float c, out float result1, out float result2)
    {
        float sqrtpart = (b * b) - (4 * a * c);

        float x, x1, x2;

        if (sqrtpart > 0)
        {
            x1 = (-b + Mathf.Sqrt(sqrtpart)) / (2 * a);
            x2 = (-b - Mathf.Sqrt(sqrtpart)) / (2 * a);
            if (x1 > x2)
            {
                result1 = x2;
                result2 = x1;
            }
            else
            {
                result1 = x1;
                result2 = x2;
            }
            return true;
        }
        else if (sqrtpart < 0)
        {
            result1 = Mathf.NegativeInfinity;
            result2 = Mathf.NegativeInfinity;
            return false;
        }
        else
        {
            x = (-b + Mathf.Sqrt(sqrtpart)) / (2 * a);
            result1 = x;
            result2 = x;
            return true;
        }

    }
    public static void RemoveListener(emptyDelegate listeners, emptyDelegate targetListener)
    {
        if (listeners != null)
        {
            foreach (var listener in listeners.GetInvocationList())
            {
                if (listener.Equals(targetListener))
                {
                    listeners -= targetListener;
                }
            }
        }
    }
    public static List<T> DictionaryValuesToList<U, T>(Dictionary<U, T>.ValueCollection values)
    {
        List<T> list = new List<T>();
        list.AddRange(values);
        return list;
    }
    public static bool getOptimalIntersection(Vector3 chaserPosition, float chaserSpeed, Vector3 targetPosition, Vector3 targetVelocity, out Vector3 result)
    {

        Vector3 vectorFromRunner = chaserPosition - targetPosition;
        float distanceToRunner = vectorFromRunner.magnitude;
        // Now set up the quadratic formula coefficients
        float a = chaserSpeed * chaserSpeed - targetVelocity.magnitude * targetVelocity.magnitude;
        float b = 2 * Vector3.Dot(vectorFromRunner, targetVelocity);
        float c = -distanceToRunner * distanceToRunner;
        float time1, time2, time;
        if (SolveQuadratic(a, b, c, out time1, out time2))
        {
            bool time1Valide = true, time2Valide = true;
            time = 0;
            if (time1 == Mathf.Infinity || float.IsNaN(time1))
            {
                time1Valide = false;
                time = time2;
            }
            if (time2 == Mathf.Infinity || float.IsNaN(time2))
            {
                if (time1Valide)
                {
                    time = time1;
                }
                time2Valide = false;
            }

            if (time1 < 0 && time2 < 0)
            {
                result = Vector2.zero;
                return false;
            }
            if (time1Valide && time2Valide)
            {
                if (time1 > 0 && time2 > 0)
                {
                    time = Mathf.Min(time1, time2);
                }
                else
                {
                    time = Mathf.Max(time1, time2);
                }
            }
            else if (!time1Valide && !time2Valide)
            {
                result = Vector3.zero;
                return false;
            }
            result = targetPosition + targetVelocity * time;
            return true;
        }
        else
        {
            result = Vector2.zero;
            return false;
        }
    }
    public static bool GetClosestPointOnFiniteLine(Vector3 point, Vector3 line_start, Vector3 line_end, out Vector3 result)
    {
        if (line_end == Vector3.positiveInfinity || line_end == Vector3.negativeInfinity || Vector3IsNan(line_end))
        {
            result = Vector3.zero;
            return false;
        }
        Vector3 line_direction = line_end - line_start;
        float line_length = line_direction.magnitude;
        line_direction.Normalize();
        float project_length = Vector3.Dot(point - line_start, line_direction);
        if (project_length >= 0 && project_length <= line_length)
        {
            result = line_start + line_direction * project_length;
            return true;
        }
        else
        {
            result = Vector3.zero;
            return false;
        }
    }
    public static bool Line_LineIntersection(out Vector3 intersection,
        Vector3 linePoint1, Vector3 lineDirection1,
        Vector3 linePoint2, Vector3 lineDirection2)
    {

        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineDirection1, lineDirection2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineDirection2);
        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        //is coplanar, and not parallel
        if (Mathf.Abs(planarFactor) < 0.0001f
                && crossVec1and2.sqrMagnitude > 0.0001f)
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineDirection1 * s);
            return true;
        }
        else
        {
            intersection = Vector3.zero;
            return false;
        }
    }
    public static bool FasterLineSegmentIntersection(Vector2 line1point1, Vector2 line1point2, Vector2 line2point1, Vector2 line2point2)
    {

        Vector2 a = line1point2 - line1point1;
        Vector2 b = line2point1 - line2point2;
        Vector2 c = line1point1 - line2point1;

        float alphaNumerator = b.y * c.x - b.x * c.y;
        float betaNumerator = a.x * c.y - a.y * c.x;
        float denominator = a.y * b.x - a.x * b.y;

        if (denominator == 0)
        {
            return false;
        }
        else if (denominator > 0)
        {
            if (alphaNumerator < 0 || alphaNumerator > denominator || betaNumerator < 0 || betaNumerator > denominator)
            {
                return false;
            }
        }
        else if (alphaNumerator > 0 || alphaNumerator < denominator || betaNumerator > 0 || betaNumerator < denominator)
        {
            return false;
        }
        return true;
    }
    public static Vector3 GetClosestPointOnFiniteLine(Vector3 point, Vector3 line_start, Vector3 line_end)
    {
        if (line_end == Vector3.positiveInfinity || line_end == Vector3.negativeInfinity || Vector3IsNan(line_end))
        {
            return line_start;
        }
        Vector3 line_direction = line_end - line_start;
        float line_length = line_direction.magnitude;
        line_direction.Normalize();
        float project_length = Mathf.Clamp(Vector3.Dot(point - line_start, line_direction), 0f, line_length);
        return line_start + line_direction * project_length;
    }
    public static float GetClosestLenghtOnFiniteLine(Vector3 point, Vector3 line_start, Vector3 line_end)
    {
        if (line_end == Vector3.positiveInfinity || line_end == Vector3.negativeInfinity || Vector3IsNan(line_end))
        {
            return 0;
        }
        Vector3 line_direction = line_end - line_start;
        float line_length = line_direction.magnitude;
        line_direction.Normalize();
        float project_length = Mathf.Clamp(Vector3.Dot(point - line_start, line_direction), 0f, line_length);
        return project_length;
    }
    public static float DistancePointAndFiniteLine(Vector3 point, Vector3 line_start, Vector3 line_end)
    {
        Vector3 line_direction = line_end - line_start;
        float line_length = line_direction.magnitude;
        line_direction.Normalize();
        float project_length = Mathf.Clamp(Vector3.Dot(point - line_start, line_direction), 0f, line_length);
        return Vector3.Distance(point, line_start + line_direction * project_length);
    }
    public static Vector3 GetClosestPointOnLineWithStartPointAndInfiniteLenght(Vector3 point, Vector3 line_start, Vector3 dir)
    {
        dir.Normalize();
        float project_length = Mathf.Clamp(Vector3.Dot(point - line_start, dir), 0f, Mathf.Infinity);
        return line_start + dir * project_length;
    }
    public static Vector3 GetClosestPointOnInfiniteLine(Vector3 point, Vector3 line_start, Vector3 dirLine)
    {
        return line_start + Vector3.Project(point - line_start, dirLine);
    }
    public static List<GameObject> GetChilds(GameObject parent, bool includeParent)
    {
        List<GameObject> list = new List<GameObject>();
        if (includeParent)
        {
            list.Add(parent.gameObject);
        }
        foreach (Transform child in parent.transform)
        {
            if (!includeParent)
            {
                list.Add(parent.gameObject);
            }
            GetChilds(child.gameObject, includeParent);
        }
        return list;
    }
    public static List<GameObject> GetChildsWithComponent<T>(GameObject parent, bool includeParent, bool onlyActives)
    {
        List<GameObject> list = new List<GameObject>();
        if (includeParent)
        {
            if (parent.gameObject.GetComponent<T>() != null)
            {
                if (onlyActives)
                {
                    if (parent.gameObject.activeInHierarchy)
                    {
                        list.Add(parent.gameObject);
                    }
                }
                else
                {
                    list.Add(parent.gameObject);
                }
            }
        }
        foreach (Transform child in parent.transform)
        {
            if (!includeParent)
            {
                if (child.gameObject.GetComponent<T>() != null)
                {
                    if (onlyActives)
                    {
                        if (child.gameObject.activeInHierarchy)
                        {
                            list.Add(child.gameObject);
                        }
                    }
                    else
                    {
                        list.Add(child.gameObject);
                    }
                }
            }
            list.AddRange(GetChildsWithComponent<T>(child.gameObject, includeParent, onlyActives));
        }
        return list;
    }
    public static List<T> GetComponentsInChilds<T>(GameObject parent, bool includeParent, bool onlyActives)
    {
        List<T> list = new List<T>();
        if (includeParent)
        {
            if (parent.gameObject.GetComponent<T>() != null)
            {
                if (onlyActives)
                {
                    if (parent.gameObject.activeInHierarchy)
                    {
                        T[] components = parent.GetComponents<T>();
                        list.AddRange(components);
                    }
                }
                else
                {
                    T[] components = parent.GetComponents<T>();
                    list.AddRange(components);
                }
            }
        }
        foreach (Transform child in parent.transform)
        {
            if (!includeParent)
            {
                if (child.gameObject.GetComponent<T>() != null)
                {
                    if (onlyActives)
                    {
                        if (child.gameObject.activeInHierarchy)
                        {
                            T[] components = child.GetComponents<T>();
                            list.AddRange(components);
                        }
                    }
                    else
                    {
                        T[] components = child.GetComponents<T>();
                        list.AddRange(components);
                    }
                }
            }
            list.AddRange(GetComponentsInChilds<T>(child.gameObject, includeParent, onlyActives));
        }
        return list;
    }
    public static T GetComponentInChilds<T>(GameObject parent, bool includeParent) where T : Component
    {
        if (includeParent)
        {
            if (parent.gameObject.GetComponent<T>() != null)
            {
                return parent.gameObject.GetComponent<T>();
            }
        }
        foreach (Transform child in parent.transform)
        {
            if (!includeParent)
            {
                if (child.gameObject.GetComponent<T>() != null)
                {
                    return parent.gameObject.GetComponent<T>();
                }
            }
            T t = GetComponentInChilds<T>(child.gameObject, includeParent);
            if (t != null)
            {
                return t;
            }
        }
        return null;
    }
    public static GameObject FindChildContainsName(GameObject parent, string name, bool includeParent)
    {
        if (includeParent)
        {
            if (parent.name.Equals(name))
            {
                return parent;
            }
        }
        foreach (Transform child in parent.transform)
        {
            if (!includeParent)
            {
                if (child.name.Equals(name))
                {
                    return child.gameObject;
                }
            }
            GameObject g = FindChildContainsName(child.gameObject, name, includeParent);
            if (g != null)
            {
                return g;
            }
        }
        return null;
    }
    public static bool GetKeyByValue<T, W>(this Dictionary<T, W> dict, W val, out T result)
    {
        result = default;
        bool valueIsContained = false;
        foreach (KeyValuePair<T, W> pair in dict)
        {
            if (EqualityComparer<W>.Default.Equals(pair.Value, val))
            {
                result = pair.Key;
                valueIsContained = true;
                break;
            }
        }
        return valueIsContained;
    }
    public static GameObject GetGameObjectWithValue<T>(GameObject parent, T value)
    {
        List<MonoVariable<T>> list = MyFunctions.GetComponentsInChilds<MonoVariable<T>>(parent, true, true);
        foreach (var item in list)
        {
            if (item.Value.Equals(value))
            {
                return item.gameObject;
            }
        }
        return null;
    }
    public static IRulesSettingsType<T> GetSettingsWithSettingsValue<T>(GameObject parent, T value)
    {
        List<IRulesSettingsType<T>> list = MyFunctions.GetComponentsInChilds<IRulesSettingsType<T>>(parent, true, true);
        foreach (var item in list)
        {
            if (item.settingsType.Equals(value))
            {
                return item;
            }
        }
        return null;
    }
    public static GameObject GetGameObjectWithValue<T>(T value)
    {
        MonoVariable<T>[] list = GameObject.FindObjectsOfType<MonoVariable<T>>();
        foreach (var item in list)
        {
            if (item.Value.Equals(value))
            {
                return item.gameObject;
            }
        }
        return null;
    }
    public static void SetStateGameObjectWithValue<T>(GameObject parent, T value)
    {
        List<MonoVariable<T>> list = MyFunctions.GetComponentsInChilds<MonoVariable<T>>(parent, true, true);
        foreach (var item in list)
        {
            item.gameObject.SetActive(item.Value.Equals(value));
        }
    }
    public static void SetStateGameObjectWithValue<T>(T value)
    {
        MonoVariable<T>[] list = GameObject.FindObjectsOfType<MonoVariable<T>>();
        foreach (var item in list)
        {
            item.gameObject.SetActive(item.Value.Equals(value));
        }
    }
    public static T parseEnum<T>(string value)
    {
        return (T)System.Enum.Parse(typeof(T), value);
    }
    public static List<T> EnumToList<T>()
    {
        List<T> list = Enum.GetValues(typeof(T)).Cast<T>().ToList();
        return list;
    }
    public static T RandomEnum<T>()
    {
        List<T> list = EnumToList<T>();
        return list[UnityEngine.Random.Range(0, list.Count)];
    }
    public static bool Vector3IsNan(Vector3 vector3)
    {
        return float.IsNaN(vector3.x) || float.IsNaN(vector3.y) || float.IsNaN(vector3.z);
    }
    public static bool floatIsNanOrInfinity(float value)
    {
        return float.IsNaN(value) || float.IsInfinity(value);
    }
    public static bool floatIsNan(float value)
    {
        return float.IsNaN(value);
    }
    public static Vector3 setYToVector3(Vector3 vector3, float y)
    {
        return new Vector3(vector3.x, y, vector3.z);
    }
    public static Vector3 setY0ToVector3(Vector3 vector3)
    {
        return new Vector3(vector3.x, 0, vector3.z);
    }
    public static Vector3 setYToVector3WithClamp(Vector3 vector3, float y, float min, float max)
    {
        return new Vector3(vector3.x, Mathf.Clamp(y, min, max), vector3.z);
    }
    public static Vector3 setRandomRotateAxisY(Vector3 vector3, float range)
    {
        float angle = UnityEngine.Random.Range(-range, range);
        return Quaternion.AngleAxis(angle, Vector3.up) * vector3;
    }
}
