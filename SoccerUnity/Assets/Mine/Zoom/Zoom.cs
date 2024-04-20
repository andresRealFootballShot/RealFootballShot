using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Zoom : MonoBehaviour
{
    [SerializeField] private Material material;
    Camera camera;

    void Start()
    {
        camera = Camera.main;
        //material = image.material;
    }

    void Update()
    {
        Vector2 screenPixels = camera.WorldToScreenPoint(transform.position);
        screenPixels = new Vector2(screenPixels.x / Screen.width, screenPixels.y / Screen.height);
        material.SetVector("_ObjectScreenPosition", screenPixels);
    }
}
