using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Burst;

namespace DOTS_ChaserDataCalculation
{
    public struct ChaserDataElement : IBufferElementData
    {
        public int chaserDataCalculationIndex;
        public int playerID;
        public Vector3 OptimalPoint;
        public float OptimalTime;
        public float OptimalTargetTime;
        public bool ReachTheTarget, CalculateOptimalPoint, CalculateOptimalPointAux;
        public bool thereIsClosestPoint;
        public Vector3 ClosestPoint { get; set; }
        public float differenceClosestTime;
        public Vector3 TargetPositionInClosestTime { get; set; }
        public float ClosestChaserTime { get; set; }
        public float ClosestTargetTime { get; set; }
        public bool thereIsIntercession { get; set; }
        public Vector3 Intercession { get; set; }
        public float TargetIntercessionTime { get; set; }
    }
    [BurstCompile]
    public struct AreaPlaneElement : IBufferElementData
    {
        public Plane plane1,plane2,plane3,plane4;

        public AreaPlaneElement(Plane plane1, Plane plane2, Plane plane3, Plane plane4)
        {
            this.plane1 = plane1;
            this.plane2 = plane2;
            this.plane3 = plane3;
            this.plane4 = plane4;
        }
        
        public bool GetLine(Vector3 pos0, Vector3 posf, out Line result)
        {
            result = new Line(pos0,posf);
            float currentLenght = Vector3.Distance(pos0, posf);
            Vector3 dir = posf - pos0;
            dir.Normalize();
            Ray ray = new Ray(pos0, dir);
            float lenght;
            bool isInside1, isInside2;
            isInside1 = plane1.GetSide(result.pos0);
            isInside2 = plane1.GetSide(result.posf);
            
            if (!isInside1 && !isInside2) return false;
            MyPlane.Raycast(ray, plane1, out lenght);
            if (lenght > 0)
            {
                
                Vector3 pos = pos0 + dir * lenght;
                
                if (!isInside2)
                {
                    float d = Vector3.Distance(pos, result.pos0);
                    if(d < currentLenght)
                    {
                        currentLenght = d;
                        result.posf = pos0 + dir * lenght;
                    }
                }
                else if(!isInside1)
                {
                    float d = Vector3.Distance(pos, result.posf);
                    if(d < currentLenght)
                    {
                        currentLenght = d;
                        result.pos0 = pos0 + dir * lenght;
                    }
                    //ray.origin = result.pos0;
                }
            }
            isInside1 = plane2.GetSide(result.pos0);
            isInside2 = plane2.GetSide(result.posf);
            
            if (!isInside1 && !isInside2) return false;
            MyPlane.Raycast(ray, plane2, out lenght);
            if (lenght > 0)
            {

                Vector3 pos = pos0 + dir * lenght;

                if (!isInside2)
                {
                    float d = Vector3.Distance(pos, result.pos0);
                    if (d < currentLenght)
                    {
                        currentLenght = d;
                        result.posf = pos0 + dir * lenght;
                    }
                }
                else if (!isInside1)
                {
                    float d = Vector3.Distance(pos, result.posf);
                    if (d < currentLenght)
                    {
                        currentLenght = d;
                        result.pos0 = pos0 + dir * lenght;
                    }
                    //ray.origin = result.pos0;
                }
            }
            
            isInside1 = plane3.GetSide(result.pos0);
            isInside2 = plane3.GetSide(result.posf);
            if (!isInside1 && !isInside2) return false;
            MyPlane.Raycast(ray, plane3, out lenght);
            if (lenght > 0)
            {

                Vector3 pos = pos0 + dir * lenght;

                if (!isInside2)
                {
                    float d = Vector3.Distance(pos, result.pos0);
                    if (d < currentLenght)
                    {
                        currentLenght = d;
                        result.posf = pos0 + dir * lenght;
                    }
                }
                else if (!isInside1)
                {
                    float d = Vector3.Distance(pos, result.posf);
                    if (d < currentLenght)
                    {
                        currentLenght = d;
                        result.pos0 = pos0 + dir * lenght;
                    }
                    //ray.origin = result.pos0;
                }
            }
            
            isInside1 = plane4.GetSide(result.pos0);
            isInside2 = plane4.GetSide(result.posf);
            if (!isInside1 && !isInside2) return false;
            MyPlane.Raycast(ray, plane4, out lenght);
            if (lenght > 0)
            {

                Vector3 pos = pos0 + dir * lenght;

                if (!isInside2)
                {
                    float d = Vector3.Distance(pos, result.pos0);
                    if (d < currentLenght)
                    {
                        currentLenght = d;
                        result.posf = pos0 + dir * lenght;
                    }
                }
                else if (!isInside1)
                {
                    float d = Vector3.Distance(pos, result.posf);
                    if (d < currentLenght)
                    {
                        currentLenght = d;
                        result.pos0 = pos0 + dir * lenght;
                    }
                    //ray.origin = result.pos0;
                }
            }
            return true;
        }
        [BurstCompile]
        public bool pointIsInside(Vector3 point)
        {
            if (!MyPlane.GetSide(plane1, point))
            {
                return false;
            }
            if (!MyPlane.GetSide(plane2, point))
            {
                return false;
            }
            if (!MyPlane.GetSide(plane3, point))
            {
                return false;
            }
            if (!MyPlane.GetSide(plane4, point))
            {
                return false;
            }
            return true;
        }
        [BurstCompile]
        public static bool pointIsInside(Vector3 point, in AreaPlaneElement plane)
        {
            if (!MyPlane.GetSide(plane.plane1, point))
            {
                return false;
            }
            if (!MyPlane.GetSide(plane.plane2, point))
            {
                return false;
            }
            if (!MyPlane.GetSide(plane.plane3, point))
            {
                return false;
            }
            if (!MyPlane.GetSide(plane.plane4, point))
            {
                return false;
            }
            return true;
        }
        [BurstCompile]
        public bool checkIntersecctionArea(ref SegmentedPathElement path,out Vector3 intercession, out float targetIntercessionTime)
        {
            bool thereIsIntercession = false;

            float currentLenght = Vector3.Distance(path.Pos0,path.Posf);
            Ray ray = new Ray(path.Pos0, path.V0);

            
            float lenght;
            if (MyPlane.Raycast(ray, plane1, out lenght))
            {
                if (lenght > 0 && lenght < currentLenght)
                {
                    currentLenght = lenght;
                    thereIsIntercession = true;
                }
            }
            if (MyPlane.Raycast(ray, plane2, out lenght))
            {
                if (lenght > 0 && lenght < currentLenght)
                {
                    currentLenght = lenght;
                    thereIsIntercession = true;
                }
            }
            if (MyPlane.Raycast(ray, plane3, out lenght))
            {
                if (lenght > 0 && lenght < currentLenght)
                {
                    currentLenght = lenght;
                    thereIsIntercession = true;
                }
            }
            if (MyPlane.Raycast(ray, plane4, out lenght))
            {
                if (lenght > 0 && lenght < currentLenght)
                {
                    currentLenght = lenght;
                    thereIsIntercession = true;
                }
            }
            intercession = ray.origin + ray.direction * currentLenght;
            targetIntercessionTime = path.t0 + currentLenght / path.V0Magnitude;
            return thereIsIntercession;
        }
   
    [BurstCompile]
    public bool GetPoint(Vector3 point,Vector3 pos0,out Vector3 result)
    {
        result = point;
        float currentLenght = Vector3.Distance(point, pos0);
        Vector3 dir = point - pos0;
        dir.Normalize();
        Ray ray = new Ray(pos0, dir);
        float lenght;
        MyPlane.Raycast(ray, plane1, out lenght);
        bool isInside1 = plane1.GetSide(point);
        bool isInside2 = plane1.GetSide(pos0);
        if (!isInside1 && !isInside2) return false;
        if (lenght > 0 && lenght < currentLenght && !isInside1)
        {
            currentLenght = lenght;
            result = pos0 + dir * currentLenght;
        }
        MyPlane.Raycast(ray, plane2, out lenght);
        isInside1 = plane2.GetSide(point);
        isInside2 = plane2.GetSide(pos0);
        if (!isInside1 && !isInside2) return false;
        if (lenght > 0 && lenght < currentLenght && !isInside1)
        {
            currentLenght = lenght;
            result = pos0 + dir * currentLenght;
        }
        MyPlane.Raycast(ray, plane3, out lenght);
        isInside1 = plane3.GetSide(point);
        isInside2 = plane3.GetSide(pos0);
        if (!isInside1 && !isInside2) return false;
        if (lenght > 0 && lenght < currentLenght && !isInside1)
        {
            currentLenght = lenght;
            result = pos0 + dir * currentLenght;
        }
        MyPlane.Raycast(ray, plane4, out lenght);
        isInside1 = plane4.GetSide(point);
        isInside2 = plane4.GetSide(pos0);
        if (!isInside1 && !isInside2) return false;
        if (lenght > 0 && lenght < currentLenght && !isInside1)
        {
            currentLenght = lenght;
            result = pos0 + dir * currentLenght;
        }
        return true;
    }
    
}
}
