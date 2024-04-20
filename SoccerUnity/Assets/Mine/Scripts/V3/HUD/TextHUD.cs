using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class TextHUD : MonoBehaviour
{
    private Variable<string> VariableString;
    private Variable<float> VariableFloat;
    private Variable<Color> VariableColor;
    string floatFormat;
    public TextMeshProUGUI text;
    public void SetVariable(Variable<string> variable)
    {
        if (VariableString != null)
        {
            VariableString.VariableChanged -= OnValueChanged;
        }
        VariableString = variable;
        VariableString.VariableChanged += OnValueChanged;
        text.text = VariableString.Value;
    }
    public void SetVariable(Variable<float> variable,string floatFormat)
    {
        this.floatFormat = floatFormat;
        if (VariableString != null)
        {
            VariableString.VariableChanged -= OnValueChanged;
        }
        VariableFloat = variable;
        VariableFloat.VariableChanged += OnValueChanged;
        text.text = VariableFloat.Value.ToString(floatFormat);
    }
    void OnValueChanged(string value)
    {
        text.text = value;
    }
    void OnValueChanged(float value)
    {
        text.text = value.ToString(floatFormat);
    }
    public void SetVariableColor(Variable<Color> variableColor)
    {
        if (VariableColor != null)
        {
            VariableColor.VariableChanged -= OnColorValueChanged;
        }
        VariableColor = variableColor;
        VariableColor.VariableChanged += OnColorValueChanged;
        text.color = variableColor.Value;
    }
    void OnColorValueChanged(Color value)
    {
        text.color = value;
    }
}
