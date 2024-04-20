using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public enum MyColor
{
    Red,
    Blue,
    White,
    Green,
    Yellow
}
public class MyDebug : MonoBehaviour
{
    public List<Color> colors;
    public List<MyColor> nameColors;
    public Dictionary<MyColor, Color> colorsDictionary = new Dictionary<MyColor, Color>();
    public GameObject textPrefab;
    public Transform debugTransform;
    public Canvas canvas;
    public KeyCode keyCode;
    public BoolVar allowShowDebug;
    string currentActor;
    
    private void Awake()
    {
        for (int i = 0; i < colors.Count; i++)
        {
            colorsDictionary.Add(nameColors[i], colors[i]);
        }
    }
    private void Update()
    {
        if (allowShowDebug.Value)
        {
            if (Input.GetKeyDown(keyCode))
            {
                canvas.enabled = !canvas.enabled;
                if (canvas.enabled)
                {
                    CursorCtrl.notifyShowMenu();
                }
                else
                {
                    CursorCtrl.notifyHideMenu();
                }
            }
        }
    }
    public void print(string text,bool debug)
    {
        if (allowShowDebug.Value && debug)
        {
            GameObject newText = Instantiate(textPrefab, debugTransform);
            TMP_Text Text = newText.GetComponent<TMP_Text>();
            Text.text = text;
        }
    }
    public void print(string text, Color color, bool debug)
    {
        if (allowShowDebug.Value && debug)
        {
            GameObject newText = Instantiate(textPrefab, debugTransform);
            TMP_Text Text = newText.GetComponent<TMP_Text>();
            Text.text += text + "\n";
            Text.color = color;
        }
    }
    public void print(string text)
    {
        if (allowShowDebug.Value)
        {
            GameObject newText = Instantiate(textPrefab, debugTransform);
            TMP_Text Text = newText.GetComponent<TMP_Text>();
            Text.text = text;
        }
    }
    public void print(string text,Color color)
    {
        if (allowShowDebug.Value)
        {
            GameObject newText = Instantiate(textPrefab, debugTransform);
            TMP_Text Text = newText.GetComponent<TMP_Text>();
            Text.text += text + "\n";
            Text.color = color;
        }
    }
    public void print(string text, Color color,string actor)
    {
        if (allowShowDebug.Value)
        {
            if (currentActor == actor.ToString())
            {
                GameObject newText = Instantiate(textPrefab, debugTransform);
                TMP_Text Text = newText.GetComponent<TMP_Text>();
                Text.text = text;
                Text.color = color;
            }
            else
            {
                GameObject newText = Instantiate(textPrefab, debugTransform);
                TMP_Text Text = newText.GetComponent<TMP_Text>();
                Text.text = "Actor " + actor;
                Text.color = Color.cyan;
                newText = Instantiate(textPrefab, debugTransform);
                Text = newText.GetComponent<TMP_Text>();
                Text.text = text;
                Text.color = color;
            }
            currentActor = actor.ToString();
        }
    }
    public void print(string text, MyColor color, string actor)
    {
        if (allowShowDebug.Value)
        {
            if (currentActor == actor.ToString())
            {
                GameObject newText = Instantiate(textPrefab, debugTransform);
                TMP_Text Text = newText.GetComponent<TMP_Text>();
                Text.text = text;
                Text.color = colorsDictionary[color];
            }
            else
            {
                GameObject newText = Instantiate(textPrefab, debugTransform);
                TMP_Text Text = newText.GetComponent<TMP_Text>();
                Text.text = "Actor " + actor;
                Text.color = Color.cyan;
                newText = Instantiate(textPrefab, debugTransform);
                Text = newText.GetComponent<TMP_Text>();
                Text.text = text;
                Text.color = colorsDictionary[color];
            }
            currentActor = actor.ToString();
        }
    }
    public void print(string text, MyColor color)
    {
        if (allowShowDebug.Value)
        {
            GameObject newText = Instantiate(textPrefab, debugTransform);
            TMP_Text Text = newText.GetComponent<TMP_Text>();
            Text.text = text;
            Text.color = colorsDictionary[color];
        }
    }
}
