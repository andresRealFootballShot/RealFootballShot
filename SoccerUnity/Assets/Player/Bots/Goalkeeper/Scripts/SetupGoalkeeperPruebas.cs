using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupGoalkeeperPruebas : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        PublicGoalkeeperData myPublicPlayerData = GetComponent<PublicGoalkeeperData>();
        Area area = MyFunctions.GetComponentInChilds<Area>(gameObject, true);
        myPublicPlayerData.addMaximumJumpHeight(3.5f, area);
        myPublicPlayerData.addMaximumJumpHeight(2.5f, null);
        myPublicPlayerData.maxSpeedVar = new Variable<float>(5);
        PublicPlayerDataList.addPublicPlayerData(myPublicPlayerData);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
