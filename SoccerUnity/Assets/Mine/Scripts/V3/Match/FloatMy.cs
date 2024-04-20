using UnityEngine;

public class FloatMy : MonoBehaviour
{
    public delegate void FloatDelegate(float value);
    public string info;
    public float initialValue;
    float value;
    public event FloatDelegate Event;
    public virtual float Value
    {
        get { return value; }
        set { this.value = value; Event?.Invoke(value); }
    }
    
}
