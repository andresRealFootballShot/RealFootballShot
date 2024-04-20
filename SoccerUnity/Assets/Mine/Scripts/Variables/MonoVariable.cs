using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoVariable : MonoBehaviour
{
}
public class MonoVariable<T> : MonoVariable
{
    public string info;
    [SerializeField]
    T value;
    public delegate void VariableDelegate(T value);
    public event VariableDelegate VariableChanged;
    public virtual T Value { get { return value; } set { VariableChanged?.Invoke(value); this.value = value; } }
    public bool Equal(MonoVariable<T> other) { return Value.Equals(other.Value); }
    public override string ToString()
    {
        return Value.ToString();
    }
    public static List<MonoVariable<T>> Find(T t)
    {
        MonoVariable<T>[] list = FindObjectsOfType<MonoVariable<T>>();
        List<MonoVariable<T>> result=new List<MonoVariable<T>>();
        foreach (var item in list)
        {
            if (item.value.Equals(t))
            {
                result.Add(item);
            }
        }
        return result;
    }
    public static List<GameObject> FindGameObjects(T t)
    {
        MonoVariable<T>[] list = FindObjectsOfType<MonoVariable<T>>();
        List<GameObject> result = new List<GameObject>();
        foreach (var item in list)
        {
            if (item.value.Equals(t))
            {
                result.Add(item.gameObject);
            }
        }
        return result;
    }
    public static GameObject FindGameObject(T t)
    {
        MonoVariable<T>[] list = FindObjectsOfType<MonoVariable<T>>();
        foreach (var item in list)
        {
            if (item.value.Equals(t))
            {
                return item.gameObject;
            }
        }
        return null;
    }
}
