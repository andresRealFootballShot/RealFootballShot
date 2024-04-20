using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransparencyCtrl : MonoBehaviour
{
    public Material matTransparent;
    public SetupModel setupModel;
    public bool enable=true;
    List<Material> list= new List<Material>();
    bool computer=true;
    Color transparencyColor;
    void Start()
    {
        transparencyColor = matTransparent.color;
    }

    public void GetListMaterials(Transform trans)
    {

        foreach (Transform child in trans)
        {
            SkinnedMeshRenderer skinnedMeshRenderer = child.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                list.Add(skinnedMeshRenderer.material);
            }
        }
    }
    public void SetTransparency(Transform trans)
    {
        if (enable&&computer)
        {
            foreach (Transform child in trans)
            {
                SkinnedMeshRenderer skinnedMeshRenderer = child.GetComponent<SkinnedMeshRenderer>();
                if (skinnedMeshRenderer != null)
                {
                    skinnedMeshRenderer.material = matTransparent;
                    setMaterialPropertyColor(skinnedMeshRenderer, transparencyColor);
                }
            }
            computer = false;
        }
    }
    void setMaterialPropertyColor(Renderer renderer,Color color)
    {
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(materialPropertyBlock);
        materialPropertyBlock.SetColor("_BaseColor", color);
        renderer.SetPropertyBlock(materialPropertyBlock);
    }
    public void SetOpaque(Transform trans)
    {
        
        int i = 0;
        if (enable && !computer)
        {
            foreach (Transform child in trans)
            {
                SkinnedMeshRenderer skinnedMeshRenderer = child.GetComponent<SkinnedMeshRenderer>();
                if (skinnedMeshRenderer != null)
                {
                    skinnedMeshRenderer.material = list[i];
                    i++;
                }
            }
            if (setupModel != null)
            {
              setupModel.setEquipementColor();
            }
            computer = true;
        }
    }
}
