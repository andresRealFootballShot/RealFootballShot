using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseTypeMatch : MonoBehaviour
{
    public GameObject chooseTypeMatchMenu,searchingMenu;
    public FindRandomMatch findRandomMatch;
    void Start()
    {
        
    }

    public void startFindRandomMatch(string typeMatch)
    {
        FindRandomMatch.typeNormalMatch = MyFunctions.parseEnum<TypeNormalMatch>(typeMatch);
        chooseTypeMatchMenu.SetActive(false);
        searchingMenu.SetActive(true);
        findRandomMatch.find();
    }
}
