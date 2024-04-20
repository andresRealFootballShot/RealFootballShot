using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupModel : MonoBehaviour
{
    public List<SkinnedMeshRenderer> renderers;
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
        }
    }
    public void setEquipementColor()
    {
        
        if (equipement != null)
        {
            foreach (var item in renderers)
            {
                //item.sharedMaterial.color = equipement.mainColor;
                MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
                item.GetPropertyBlock(materialPropertyBlock);
                materialPropertyBlock.SetColor("_BaseColor", equipement.mainColor);
                item.SetPropertyBlock(materialPropertyBlock);
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
