using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
namespace DOTS_ChaserDataCalculation
{
    public struct Line
    {
        public Vector3 pos0, posf;

        public Line(Vector3 pos0, Vector3 posf)
        {
            this.pos0 = pos0;
            this.posf = posf;
        }
    }
    [BurstCompile]
    public struct OptimalPointJob : IJobEntityBatch
    {
        //public ComponentTypeHandle<InputA> InputATypeHandle;
        public ComponentTypeHandle<PlayerDataComponent> playerDataHandle;
        [ReadOnly] public BufferTypeHandle<AreaPlaneElement> areaPlanesHandle;
        public BufferTypeHandle<SegmentedPathElement> segmentedPathsHandle;
        //[ReadOnly] public DynamicBuffer<AreaPlaneElement> areaPlanes;
        public EntityCommandBuffer.ParallelWriter ConcurrentCommands;
        
        public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
        {
            
            var playerDatas = batchInChunk.GetNativeArray(playerDataHandle);
            BufferAccessor<AreaPlaneElement> bufferAccessor = batchInChunk.GetBufferAccessor<AreaPlaneElement>(areaPlanesHandle);
            BufferAccessor<SegmentedPathElement> segmentedPathbufferAccessor = batchInChunk.GetBufferAccessor(segmentedPathsHandle);
            //NativeList<SegmentedPathElement> collectedSegmentedPaths = new NativeList<SegmentedPathElement>(Allocator.Temp);
            for (int i = 0; i < playerDatas.Length; i++)
            {
                PlayerDataComponent playerDataComponent = playerDatas[i];

                ChaserDataElement endChaserDataElement = new ChaserDataElement();
                endChaserDataElement.OptimalTime = Mathf.Infinity;
                endChaserDataElement.differenceClosestTime = Mathf.Infinity;
                endChaserDataElement.playerID = playerDatas[i].id;
                DynamicBuffer<AreaPlaneElement> areaPlaneElements = bufferAccessor[i];
                DynamicBuffer<SegmentedPathElement> segmentedPathElements = segmentedPathbufferAccessor[i];
                PlayerDataComponent playerData = playerDatas[i];
                bool thereIsChaserData = false;
                for (int j = 0; j < playerDataComponent.segmentedPathSize; j++)
                {
                    ChaserDataElement chaserDataElement = new ChaserDataElement();
                   //calculateChaserData(ref playerData, segmentedPathElements[j], areaPlaneElements.ToNativeArray(Allocator.Temp), ref chaserDataElement);
                    
                    if (chaserDataElement.ReachTheTarget && chaserDataElement.OptimalTime< endChaserDataElement.OptimalTime)
                    {
                        thereIsChaserData = true;
                        endChaserDataElement.ReachTheTarget = true;
                        endChaserDataElement.OptimalPoint = chaserDataElement.OptimalPoint;
                        endChaserDataElement.OptimalTime = chaserDataElement.OptimalTime;
                        endChaserDataElement.OptimalTargetTime = chaserDataElement.OptimalTargetTime;
                        endChaserDataElement.chaserDataCalculationIndex = j;
                        
                    }
                    if (chaserDataElement.thereIsClosestPoint && chaserDataElement.differenceClosestTime< endChaserDataElement.differenceClosestTime)
                    {
                        thereIsChaserData = true;
                        endChaserDataElement.thereIsClosestPoint = true;
                        endChaserDataElement.ClosestPoint = chaserDataElement.ClosestPoint;
                        endChaserDataElement.ClosestChaserTime = chaserDataElement.ClosestChaserTime;
                        endChaserDataElement.ClosestTargetTime = chaserDataElement.ClosestTargetTime;
                        endChaserDataElement.TargetPositionInClosestTime = chaserDataElement.TargetPositionInClosestTime;
                        endChaserDataElement.differenceClosestTime = chaserDataElement.differenceClosestTime;
                    }
                    if (chaserDataElement.thereIsIntercession && !endChaserDataElement.thereIsIntercession)
                    {
                        thereIsChaserData = true;
                        endChaserDataElement.thereIsIntercession = true;
                        endChaserDataElement.Intercession = chaserDataElement.Intercession;
                        endChaserDataElement.TargetIntercessionTime = chaserDataElement.TargetIntercessionTime;
                    }

                }
                playerDataComponent.segmentedPathSize = 0;
                playerDatas[i] = playerDataComponent;
                if (thereIsChaserData)
                {
                    //ConcurrentCommands.AppendToBuffer<ChaserDataElement>(batchIndex, playerDataComponent.chaserDataResultEntity, endChaserDataElement);
                    
                }
                //segmentedPathElements.Clear();
            }

        }
        [BurstCompile]
        public static bool calculateChaserData(ref PlayerDataComponent playerDataComponent,ref SegmentedPathElement segmentedPath ,ref AreaPlaneElement fullArea,ref ChaserDataElement chaserData)
        {
            Vector3 intercession;
            bool result;
            float targetIntercessionTime;
            bool thereIsIntercession = fullArea.checkIntersecctionArea(ref segmentedPath,out intercession, out targetIntercessionTime);
            
            float t = segmentedPath.t0;
            float t0 = t;
            if (thereIsIntercession)
            {
                //print("b " + thereIsIntercession);
                chaserData.Intercession = intercession;
                chaserData.thereIsIntercession = thereIsIntercession;
                chaserData.TargetIntercessionTime = targetIntercessionTime;
            }
            result = calculateClosestPoint(ref chaserData,playerDataComponent, segmentedPath,ref fullArea);
            
            Vector3 optimalPoint = segmentedPath.Pos0;
            //float optimalTime = Vector3.Distance(chaserPosition, newPath.Pos0) / chaserSpeed;
            
            float optimalTime = GetTimeToReachPointDOTS.getTimeToReachPosition(ref playerDataComponent,segmentedPath.Pos0);
            
            bool thereIsOptimalPoint = false;
            if (optimalTime <= t0)
            {
                //El chaser alcanza el objetivo antes de t0 (antes de que el balón llegue al inicio del segmento)

                if (fullArea.pointIsInside(optimalPoint) && optimalPoint.y <= playerDataComponent.maxJumpHeight)
                {
                    thereIsOptimalPoint = true;
                }
                    
                if (thereIsOptimalPoint)
                {
                    chaserData.ReachTheTarget = true;
                    chaserData.OptimalPoint = optimalPoint;
                    //chaserData.OptimalTime = optimalTime + segmentedPath.t0;
                    chaserData.OptimalTime = segmentedPath.t0;
                    chaserData.OptimalTargetTime = segmentedPath.t0;
                    result = true;
                }
            }
            if (!thereIsOptimalPoint)
            {
                float timeTargetResult;
                //Marker.Begin();
                if (checkHeight(ref playerDataComponent, segmentedPath, out optimalTime, out optimalPoint, out timeTargetResult))
                {
                    if (fullArea.pointIsInside(optimalPoint))
                    {
                        chaserData.ReachTheTarget = true;
                        chaserData.OptimalPoint = optimalPoint;
                        chaserData.OptimalTime = optimalTime;
                        chaserData.OptimalTargetTime = timeTargetResult;
                        result = true;
                    }
                }
                //Marker.End();
            }
            return result;
        }
        static bool checkHeight(ref PlayerDataComponent playerDataComponent, in SegmentedPathElement path, out float timeResult, out Vector3 pointResult, out float timeTargetResult)
        {
            //Comprueba que el chaser llega hasta optimalPoint.y
            timeResult = Mathf.Infinity;
            pointResult = Vector3.zero;
            //NativeList<float> times = new NativeList<float>(Allocator.Temp);
            Vector3 chaserPosition = playerDataComponent.position;
            float chaserSpeed = playerDataComponent.maxSpeed;
            float t0 = path.t0;
            //print("t0=" + t0);
            float heightTime;
            float lastHeightTime = Mathf.Infinity;
            Vector3 heightPoint = Vector3.positiveInfinity;
            timeTargetResult = Mathf.Infinity;
            float scope = playerDataComponent.scope;
            //scope = 0;
            MyFloatArray times = new MyFloatArray();
            if (playerDataComponent.useAccelerationGetTimeToReachPosition)
            {
                GetOptimalPointForReachTargetDOTS.getOptimalPointForReachTargetWhitAcceleration(path,ref playerDataComponent, t0, scope, 0.1f, ref times);
                
            }
            else
            {
                float t=-1;
                GetOptimalPointForReachTargetDOTS.getOptimalPointForReachTarget(path,playerDataComponent.position, chaserSpeed, t0, ref t);
                if(t!=-1)
                    times.Add(t);
            }

            /*for (int i = 0; i < times.Length; i++)
            {
                //print(times[i]);
                times[i] += path.t0;

            }*/
            float maximumHeight = playerDataComponent.maxJumpHeight;
                if ((path.Pos0.y < maximumHeight && path.Posf.y > maximumHeight) || (path.Pos0.y > maximumHeight && path.Posf.y < maximumHeight))
                {
                    //La máxima altura está en este path. Necesitamos saber si el chaser llega hasta el punto con la máxima altura ya que el teorema de cosenos no resuelve este problema.
                    //NativeList<float> list = new NativeList<float>(Allocator.Temp);
                    float timeReachY = -1;
                    timeToReachY(path,maximumHeight,ref timeReachY);
                    if (timeReachY!=-1)
                    {
                        Vector3 point = path.Pos0 + path.V0 * timeReachY;
                        float timeToReachPoint = Vector3.Distance(point, chaserPosition) / chaserSpeed;
                        float a = timeReachY + path.t0;
                        if (point.y <= maximumHeight && a < timeTargetResult && timeToReachPoint <=a)
                        {
                            lastHeightTime = timeToReachPoint;
                            timeTargetResult = a;
                            heightPoint = point;
                            times.Add(timeToReachPoint);
                        }
                    }
                    //list.Dispose();
            }
            bool r = false;
            for (int i = 0; i < times.lenght; i++)
            {
                float time = times.Get(i);
                if (time != lastHeightTime)
                {
                    Vector3 optimalPoint = path.Pos0 + path.V0 * (time - path.t0);
                    if (optimalPoint.y <= maximumHeight && time <= timeTargetResult)
                    {
                        timeTargetResult = time;
                        pointResult = optimalPoint;
                        timeResult = time;
                        r = true;
                    }

                }
                else
                {
                    pointResult = heightPoint;
                    timeResult = lastHeightTime;
                    r = true;
                }
            }
            return r;
        }
        static bool calculateClosestPoint(ref ChaserDataElement chaserData,in PlayerDataComponent playerDataComponent, SegmentedPathElement path, ref AreaPlaneElement fullArea)
        {
            Line line2;
            //print(chaserData.name + " | " + chaserData.closestArea);
            if (!fullArea.GetLine(path.Pos0, path.Posf, out line2))
            {
                return false;
            }
            Vector3 closestPoint = GetClosestPointOnFiniteLine(playerDataComponent.position, line2.pos0, line2.posf);
            Vector3 closestPoint2 = closestPoint;
            bool thereIsClosestPoint = false;
                //Buscamos el closestPoint teniendo en cuenta las alturas máximas
                if (closestPoint.y > playerDataComponent.maxJumpHeight)
                {
                    Plane plane = new Plane(playerDataComponent.fieldNormal, playerDataComponent.fieldPosition + playerDataComponent.fieldNormal * playerDataComponent.maxJumpHeight);
                    Vector3 direction = closestPoint - playerDataComponent.position;
                    Ray ray = new Ray(playerDataComponent.position, direction);
                    float lenght;
                    if (MyPlane.Raycast(ray,plane, out lenght))
                    {
                        //Punto del closestPoint en la altura máxima
                        Vector3 point = playerDataComponent.position + direction * lenght;

                        if (fullArea.pointIsInside(point))
                        {
                            if (Vector3.Distance(closestPoint, point) < 0.3f)
                            {
                                closestPoint2 = point;
                                thereIsClosestPoint = true;
                            }
                        }
                    }
                }
                else
                {
                    thereIsClosestPoint = true;
                }
            
            if (!thereIsClosestPoint)
            {
                return thereIsClosestPoint;
            }
            float closestTime = Vector3.Distance(closestPoint2, playerDataComponent.position) / playerDataComponent.maxSpeed;
            float chaserTimeFromPathT0 = closestTime - path.t0;
            Vector3 averageV = (path.V0 + path.Vf) * 0.5f;
            Vector3 targetPositionInClosestTime = path.Pos0 + averageV * chaserTimeFromPathT0;
            float timeTargetToReachClosestPoint;
            float v0magn = path.V0.magnitude;
            if (v0magn != 0)
            {
                timeTargetToReachClosestPoint = Vector3.Distance(closestPoint, path.Pos0) / v0magn;
                float timeDifference = chaserTimeFromPathT0 - timeTargetToReachClosestPoint;
                //print(targetTimeWithOffset + " | " + timeTargetToReachClosestPoint + " | " + timeDifference);
                //print(closestPointDistance + " "+chaserData.ClosestPointDistance+" "+ chaserData.TargetPositionInClosestTime + " " + chaserData.ClosestPoint);
                //Ray ray = new Ray(playerDataComponent.position, closestPoint2 - playerDataComponent.position);
                Vector3 closestPoint3;
                if (fullArea.GetPoint(closestPoint2, playerDataComponent.position, out closestPoint3))
                {
                    if (!chaserData.thereIsClosestPoint || (timeDifference < chaserData.differenceClosestTime))
                    {

                        chaserData.ClosestPoint = closestPoint3;
                        chaserData.ClosestChaserTime = closestTime;
                        chaserData.ClosestTargetTime = timeTargetToReachClosestPoint + path.t0;
                        chaserData.TargetPositionInClosestTime = targetPositionInClosestTime;
                        chaserData.differenceClosestTime = timeDifference;
                        chaserData.thereIsClosestPoint = true;
                        thereIsClosestPoint = true;
                    }
                }
            }
            else
            {
                chaserData.ClosestPoint = path.Pos0;
                chaserData.ClosestChaserTime = closestTime;
                chaserData.ClosestTargetTime = 0;
                chaserData.TargetPositionInClosestTime = path.Pos0;
                chaserData.differenceClosestTime = closestTime;
                chaserData.thereIsClosestPoint = true;
                thereIsClosestPoint = true;
            }
            return thereIsClosestPoint;
        }
        static bool timeToReachY(in SegmentedPathElement path,float y,ref float result)
        {
            result = -1;
            Plane plane = new Plane(Vector3.up, Vector3.up * y);
            Ray ray = new Ray(path.Pos0, path.V0);
            float lenght;
            if (MyPlane.Raycast(ray,plane, out lenght))
            {
                result = lenght / path.V0Magnitude;
                return true;
            }
            else
            {
                return false;
            }
        }
        static Vector3 GetClosestPointOnFiniteLine(Vector3 point, Vector3 line_start, Vector3 line_end)
        {
            if (line_end == Vector3.positiveInfinity || line_end == Vector3.negativeInfinity || Vector3IsNan(line_end))
            {
                return line_start;
            }
            Vector3 line_direction = line_end - line_start;
            float line_length = line_direction.magnitude;
            line_direction.Normalize();
            float project_length = Mathf.Clamp(Vector3.Dot(point - line_start, line_direction), 0f, line_length);
            return line_start + line_direction * project_length;
        }
        static bool Vector3IsNan(Vector3 vector3)
        {
            return float.IsNaN(vector3.x) || float.IsNaN(vector3.y) || float.IsNaN(vector3.z);
        }
        /*static bool GetLine(in AreaPlaneElement plane, Line line, out Line result)
        {
            //Si hay un punto dentro entonces el resultado es la linea entre el punto y la intersección y se devuelve verdadero
            //Si los dos están dentro el resultado el la line y se devuelve verdadero
            //Si los dos están fuera se devuelve falso
            bool pos0isInside = true;
            bool posfIsInside = true;
            result = new Line(line.pos0, line.posf);
            Ray ray = new Ray(line.posf, line.pos0 - line.posf);
            Vector3 intercession;
            if (!plane.GetPoint(line.pos0, ray, out intercession))
            {
                result.pos0 = intercession;
                pos0isInside = false;
            }
            ray = new Ray(line.pos0, line.posf - line.pos0);
            if (!GetPoint(planes,line.posf, ray, out intercession))
            {
                result.posf = intercession;
                posfIsInside = false;
            }
            return pos0isInside || posfIsInside;
        }
        public bool GetPoint(Vector3 point, Ray ray, out Vector3 result)
        {
            bool isInside = true;
            result = point;
            if (!plane1.GetSide(result))
            {
                //result = plane.ClosestPointOnPlane(result);
                isInside = false;
            }
            float lenghtRayOfInstersection = 0;
            if (MyPlane.Raycast(ray, plane1, out lenghtRayOfInstersection))
            {
                Vector3 intersection = ray.origin + ray.direction * lenghtRayOfInstersection;
                bool isInsideIntersection = true;
                for (int j = 0; j < planes.Length; j++)
                {
                    Plane plane2 = planes[j].plane1;
                    if (i != j)
                    {
                        if (!plane2.GetSide(intersection))
                        {
                            isInsideIntersection = false;
                        }
                    }
                }
                if (isInsideIntersection)
                {
                    result = intersection;
                }
            }



            if (isInside)
            {
                result = point;
            }
            return isInside;
        }
        [BurstCompile]
        public static bool checkIntersecctionArea([NoAlias] ref SegmentedPathElement path, [NoAlias] ref AreaPlaneElement fullArea, out Vector3 intercession, out float targetIntercessionTime)
        {
            intercession = Vector3.zero;
            targetIntercessionTime = 0;
            if (pointIsInside(path.Pos0,ref fullArea) && !pointIsInside(path.Posf,ref fullArea))
            {
                
                Ray ray = new Ray(path.Pos0, path.V0);
                Vector3 aux;
                bool thereIsIntercession = GetIntercession(ray,ref fullArea, out aux);
                //print("b "+ thereIsIntercession);
                intercession = aux;
                targetIntercessionTime = path.t0 + Vector3.Distance(aux, path.Pos0) / path.V0Magnitude;
                return thereIsIntercession;
            }
            return false;
        }

        [BurstCompile]
        static bool GetIntercession(in Ray ray, [NoAlias] ref NativeArray<AreaPlaneElement> planes, out Vector3 intercession)
        {
            intercession = Vector3.zero;
            for (int i = 0; i < planes.Length; i++)
            {
                Plane plane = planes[i].plane;
                float lenght;
                if (MyPlane.Raycast(ray,plane, out lenght))
                {
                    Vector3 intercessionAux = ray.origin + ray.direction * lenght;
                    bool intercessionIsInside = true;
                    for (int j= 0; j < planes.Length; j++)
                    {
                        Plane plane2 = planes[j].plane;
                        if (i!=j)
                        {
                            
                            if (!MyPlane.GetSide(ref plane2,intercessionAux))
                            {
                                intercessionIsInside = false;
                            }
                        }
                    }
                    if (intercessionIsInside)
                    {
                        intercession = intercessionAux;
                        return true;
                    }
                }
            }
            return false;
        }
        [BurstCompile]
        public static bool pointIsInside(Vector3 point, [NoAlias] ref NativeArray<AreaPlaneElement> planes)
        {
            bool isInside = true;
            for (int i = 0; i < planes.Length; i++)
            {
                Plane plane = planes[i].plane;
                
                if (!MyPlane.GetSide(ref plane,point))
                {
                    isInside = false;
                }
            }
            return isInside;
        }*/

    }
}
