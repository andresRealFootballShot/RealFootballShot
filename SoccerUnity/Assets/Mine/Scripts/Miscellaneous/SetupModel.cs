using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupModel : MonoBehaviour
{
    public List<SkinnedMeshRenderer> renderers;
    public TransparencyCtrl transparencyCtrl;
    Equipement equipement;
    public bool isLoaded;
    void Awake()
    {
        foreach (var item in renderers)
        {
            //item.material = Instantiate(item.sharedMaterial);
        }

        isLoaded = true;
    }
    public void setEquipementColor(Equipement equipement)
    {
        equipement.mainColorVar.VariableChanged += changeEquipementMainColor;
        this.equipement = equipement;
        
            foreach (var item in renderers)
            {
                //item.sharedMaterial.color = equipement.mainColor;
                MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
                item.GetPropertyBlock(materialPropertyBlock);
                materialPropertyBlock.SetColor("_BaseColor", equipement.mainColor);
                item.SetPropertyBlock(materialPropertyBlock);
            if (transparencyCtrl != null && transparencyCtrl.isTransparent)
            {
                transparencyCtrl.SetTransparency(item);
            }
        }
        
    }
    public void setEquipementColor()
    {
            foreach (var item in renderers)
            {
                //item.sharedMaterial.color = equipement.mainColor;
                MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
                item.GetPropertyBlock(materialPropertyBlock);
                materialPropertyBlock.SetColor("_BaseColor", equipement.mainColor);
                item.SetPropertyBlock(materialPropertyBlock);
                if (transparencyCtrl != null && transparencyCtrl.isTransparent)
                {
                    transparencyCtrl.SetTransparency(item);
                }
            }
        
    }
    void changeEquipementMainColor(Color color)
    {
        foreach (var item in renderers)
        {
            //item.material.color = color;
            MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
            item.GetPropertyBlock(materialPropertyBlock);
            materialPropertyBlock.SetColor("_BaseColor", color);
            item.SetPropertyBlock(materialPropertyBlock);
        }
    }
    
    /*
    public bool isLoaded;
    void Awake()
    {
        foreach (var item in renderers)
        {
            item.material = Instantiate(item.sharedMaterial);
        }
        isLoaded = true;
    }
    public void setEquipementColor(Equipement equipement)
    {
        equipement.mainColorVar.VariableChanged += changeEquipementMainColor;
        
        foreach (var item in renderers)
        {
            item.sharedMaterial.color = equipement.mainColor; 
        }
    }
    void changeEquipementMainColor(Color color)
    {
        foreach (var item in renderers)
        {
            item.material.color = color;
        }
    }*/
}
