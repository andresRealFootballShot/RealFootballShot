using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
namespace NextMove_Algorithm
{
    public class InterestingPoint
    {

    }
    public class Node
    {
        public float t;
        public NativeArray<PlayerData> playerDatas;
    }
    public class InitialNextMoveCreator
    {
        public List<ChaserData> sortedChaserDatas= new List<ChaserData>();
        
        public void Start()
        {
            MatchEvents.publicPlayerDataOfAddedPlayerToTeamIsAvailable.AddListener(publicPlayerDataIsAdded);
        }


        void publicPlayerDataIsAdded(PlayerAddedToTeamEventArgs playerAddedToTeamEventArgs)
        {
            ChaserData chaserData;
            if (playerAddedToTeamEventArgs.publicPlayerData.getFirstChaserData(out chaserData))
            {
                sortedChaserDatas.Add(chaserData);
            }
        }
        public void Update()
        {
            getArrival_OrderList();
        }
        public void getArrival_OrderList()
        {
            sortedChaserDatas.Sort(ChaserData.CompareBySelectedOptimalTime);
            
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                int i = 0;
                foreach (var chaserData in sortedChaserDatas)
                {
                    Vector3 globalPoint;
                    if(chaserData.getGlobalOptimalPoint(out globalPoint))
                    {

                        Handles.Label(globalPoint+ Vector3.up*2,i.ToString());
                        //Handles.Label(segmentedPathElement.Pos0 + Vector3.up * 0.5f, segmentedPathElement.Pos0.ToString("f2") );

                        Gizmos.DrawSphere(globalPoint, 0.1f);
                    }
                    i++;
                }
            }
        }
#endif
    }
}
