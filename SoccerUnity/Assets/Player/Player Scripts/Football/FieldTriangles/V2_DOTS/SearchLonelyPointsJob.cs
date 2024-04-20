using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;

namespace FieldTriangleV2
{
    [BurstCompile]
    public struct SearchLonelyPointsJob : IJobEntityBatch
    {
        [ReadOnly]  public BufferTypeHandle<PointElement> pointsHandle;
        public BufferTypeHandle<EdgeElement> edgesHandle;
        public BufferTypeHandle<TriangleElement> trianglesHandle;
        public BufferTypeHandle<LonelyPointElement> lonelyPointsHandle;
        public ComponentTypeHandle<BufferSizeComponent> pointBufferSizeComponentHandle;
        //public EntityCommandBuffer.ParallelWriter ConcurrentCommands;
        [BurstCompile]
        public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
        {
            BufferAccessor<PointElement> pointBuffers = batchInChunk.GetBufferAccessor(pointsHandle);
            BufferAccessor<EdgeElement> edgeBuffers = batchInChunk.GetBufferAccessor(edgesHandle);
            BufferAccessor<TriangleElement> trianglesBuffer= batchInChunk.GetBufferAccessor(trianglesHandle);
            BufferAccessor<LonelyPointElement> lonelyPointsBuffer= batchInChunk.GetBufferAccessor(lonelyPointsHandle);
            NativeArray<BufferSizeComponent> pointBufferSizes = batchInChunk.GetNativeArray(pointBufferSizeComponentHandle);
            for (int i = 0; i < pointBuffers.Length; i++)
            {
                DynamicBuffer<PointElement> points = pointBuffers[i];
                DynamicBuffer<EdgeElement> edges = edgeBuffers[i];
                DynamicBuffer<TriangleElement> triangles = trianglesBuffer[i];
                DynamicBuffer<LonelyPointElement> lonelyPoints = lonelyPointsBuffer[i];
                BufferSizeComponent pointBufferSizeComponent = pointBufferSizes[i];
                if (pointBufferSizeComponent.pointSize > 2)
                {
                    int startEdgeCreationIndex = 3;
                    int startTriangleCreationIndex = 1;
                    createFirstTriangle(points, triangles, edges);
                    searchPointsPosition(points, pointBufferSizeComponent.pointSize, edges, triangles, ref startEdgeCreationIndex, ref startTriangleCreationIndex);
                    pointBufferSizeComponent.edgesResultSize = startEdgeCreationIndex;
                    pointBufferSizeComponent.trianglesResultSize = startTriangleCreationIndex;
                    int lonelyPointCreateIndex = 0;
                    createLonelyPointsOfTriangles(triangles, startTriangleCreationIndex,ref pointBufferSizeComponent, points,edges,lonelyPoints,ref lonelyPointCreateIndex);
                    createLonelyPointsOfEdges(points, lonelyPoints, edges, startEdgeCreationIndex, ref lonelyPointCreateIndex, pointBufferSizeComponent.lonelyPointDistance, pointBufferSizeComponent.maxLonelyPointsOfEdgeCount);
                    pointBufferSizeComponent.lonelyPointsResultSize = lonelyPointCreateIndex;
                    pointBufferSizes[i] = pointBufferSizeComponent;
                    //ConcurrentCommands.SetComponent<BufferSizeComponent>(batchIndex, pointBufferSizeComponent.enity, pointBufferSizeComponent);
                }
            }
        }
        
        void createLonelyPointsOfEdges(DynamicBuffer<PointElement> points,DynamicBuffer<LonelyPointElement> lonelyPointElements, DynamicBuffer<EdgeElement> edges,int edgeSize,ref int lonelyPointCreateIndex,float lonelyPointDistance,int maxCount)
        {
            for (int i = 0; i < edgeSize; i++)
            {
                if (!edges[i].isDisable)
                {
                    divideEdge(points[edges[i].pointIndex1].position, points[edges[i].pointIndex2].position, lonelyPointDistance, maxCount, lonelyPointElements, ref lonelyPointCreateIndex);
                }
            }
        }
        void createLonelyPointsOfTriangles(DynamicBuffer<TriangleElement> triangles,int triangleSize,ref BufferSizeComponent searchParams, DynamicBuffer<PointElement> points, DynamicBuffer<EdgeElement> edges, DynamicBuffer<LonelyPointElement> lonelyPoints, ref int lonelyPointCreateIndex)
        {
            for (int i = 0; i < triangleSize; i++)
            {
                TriangleElement triangle = triangles[i];
                float normalizedArea = getNormalizedArea(triangle,points);
                if (normalizedArea < searchParams.minNormalizedArea)
                {
                    int longestEdge = getLongestEdge(ref triangle, edges, points);
                    EdgeElement edge = edges[longestEdge];
                    disableEdgesOfTriangle(longestEdge, ref triangle,edges);
                    divideEdge(points[edge.pointIndex1].position, points[edge.pointIndex2].position, searchParams.lonelyPointDistance, searchParams.maxLonelyPointsOfEdgeCount, lonelyPoints, ref lonelyPointCreateIndex);
                }
                else
                {
                    if (!triangle.thereIsTrianglesInside)
                    {
                        Vector2 barycenter = getBarycenter(triangle, points);
                        if (checkPoints_Point_Distance(triangle, barycenter, searchParams.lonelyPointDistance, points))
                        {
                            if (lonelyPointCreateIndex >= lonelyPoints.Length)
                            {
                                return;
                            }
                            lonelyPoints[lonelyPointCreateIndex] = new LonelyPointElement(barycenter, lonelyPointCreateIndex, true);
                            lonelyPointCreateIndex++;
                        }

                        divideEdge(barycenter, points[triangle.pointIndex1].position, searchParams.lonelyPointDistance, searchParams.maxLonelyPointsOfEdgeCount, lonelyPoints, ref lonelyPointCreateIndex);
                        divideEdge(barycenter, points[triangle.pointIndex2].position, searchParams.lonelyPointDistance, searchParams.maxLonelyPointsOfEdgeCount, lonelyPoints, ref lonelyPointCreateIndex);
                        divideEdge(barycenter, points[triangle.pointIndex3].position, searchParams.lonelyPointDistance, searchParams.maxLonelyPointsOfEdgeCount, lonelyPoints, ref lonelyPointCreateIndex);
                    }
                }
                
            }
            /*if (checkPoints_Point_Distance(triangle, point2.position, lonelyPointDistance))
            {
                lonelyPoints.Add(new LonelyPoint(point2.position, point2.pointName));
            }*/
        }
        void disableEdgesOfTriangle(int longestEdgeIndex,ref TriangleElement triangle,DynamicBuffer<EdgeElement> edges)
        {
            if (longestEdgeIndex != triangle.edgeIndex1)
            {
                EdgeElement edge = edges[triangle.edgeIndex1];
                edge.isDisable = true;
                edges[triangle.edgeIndex1] = edge;
            }
            if (longestEdgeIndex != triangle.edgeIndex2)
            {
                EdgeElement edge = edges[triangle.edgeIndex2];
                edge.isDisable = true;
                edges[triangle.edgeIndex2] = edge;
            }
            if(longestEdgeIndex != triangle.edgeIndex3)
            {
                EdgeElement edge = edges[triangle.edgeIndex3];
                edge.isDisable = true;
                edges[triangle.edgeIndex3] = edge;
            }
        }
        void divideEdge(Vector2 point1, Vector2 point2, float lonelyPointDistance, int maxCount, DynamicBuffer<LonelyPointElement> lonelyPointElements,ref int lonelyPointCreateIndex)
        {
            Vector2 dir = point2 - point1;
            float distance = dir.magnitude;
            dir.Normalize();
            float d1 = distance / lonelyPointDistance;
            if (d1 < maxCount + 1)
            {
                int a = Mathf.FloorToInt(d1);
                if (a == 0)
                {
                    return;
                }
                float b = distance / a;
                for (int i = 1; i < a; i++)
                {
                    Vector2 pos = point1 + dir * b * i;
                    if (lonelyPointCreateIndex >= lonelyPointElements.Length)
                    {
                        return;
                    }
                    lonelyPointElements[lonelyPointCreateIndex] = new LonelyPointElement(pos,lonelyPointCreateIndex,false);
                    lonelyPointCreateIndex++;
                }
            }
            else
            {
                float a = distance / (maxCount + 1);
                for (int i = 1; i < maxCount + 1; i++)
                {
                    if (lonelyPointCreateIndex >= lonelyPointElements.Length)
                    {
                        return;
                    }
                    Vector2 pos = point1 + dir * a * i;
                    lonelyPointElements[lonelyPointCreateIndex] = new LonelyPointElement(pos, lonelyPointCreateIndex,false);
                    lonelyPointCreateIndex++;
                }
            }
        }
        Vector2 getBarycenter(TriangleElement triangleElement, DynamicBuffer<PointElement> points)
        {
            Vector2 result = Vector3.zero;
            result += points[triangleElement.pointIndex1].position;
            result += points[triangleElement.pointIndex2].position;
            result += points[triangleElement.pointIndex3].position;
            return result / 3;
        }
        bool checkPoints_Point_Distance(TriangleElement triangleElement, Vector2 pointPosition, float lonelyPointDistance, DynamicBuffer<PointElement> points)
        {
            bool add = true;
            if (Vector2.Distance(pointPosition, points[triangleElement.pointIndex1].position) < lonelyPointDistance)
            {
                add = false;
                return add;
            }
            if (Vector2.Distance(pointPosition, points[triangleElement.pointIndex2].position) < lonelyPointDistance)
            {
                add = false;
                return add;
            }
            if (Vector2.Distance(pointPosition, points[triangleElement.pointIndex3].position) < lonelyPointDistance)
            {
                add = false;
                return add;
            }
            return add;
        }
        void searchPointsPosition(DynamicBuffer<PointElement> points, int pointSize, DynamicBuffer<EdgeElement> edges, DynamicBuffer<TriangleElement> triangles,ref int startEdgeCreationIndex, ref int startTriangleCreationIndex)
        {
            
            for (int i = 3; i < pointSize; i++)
            {
                PointElement point = points[i];
                int nextEdgeIndex = 0;
                int nextTriangleIndex = 0;
                bool isInside;
                int count = 0;
                do
                {
                    TriangleElement triangle = triangles[nextTriangleIndex];
                    isInside = TriangleIsInside(triangle, ref point, points, edges, ref nextEdgeIndex,ref nextTriangleIndex);
                    if (isInside)
                    {
                        createTrianglesInsideTriangle(ref point, ref triangle,ref startTriangleCreationIndex,ref startEdgeCreationIndex,triangles,points,edges);
                    }
                    else
                    {
                        if (edges[nextEdgeIndex].isLimit)
                        {
                            TriangleElement nextLimitTriangle = triangles[nextTriangleIndex];
                            EdgeElement nextLimitEdge = edges[nextEdgeIndex];
                            createOutsideTriangle(ref point, ref nextLimitTriangle, ref nextLimitEdge,ref startTriangleCreationIndex,ref startEdgeCreationIndex,triangles,points,edges);
                            break;
                        }
                    }
                    count++;
                    if (count > 500)
                    {
                        Debug.LogWarning("searchPointPosition count>500");
                        break;
                    }
                } while (!isInside);
            }
        }
        void createFirstTriangle(DynamicBuffer<PointElement> points, DynamicBuffer<TriangleElement> triangles, DynamicBuffer<EdgeElement> edges)
        {
            PointElement point1 = points[0];
            PointElement point2 = points[1];
            PointElement point3 = points[2];
            EdgeElement edge1 = new EdgeElement(0, point1, point2, 0,false,true,1,2,0,1);
            
            int sign = getSign(ref point3, ref edge1, points);
            edge1.limitSign = -sign;
            EdgeElement edge2 = new EdgeElement(1, point1, point3, 0, false, true, 0,2, 0, 2);
            sign = getSign(ref point2, ref edge2, points);
            edge2.limitSign = -sign;
            
            EdgeElement edge3 = new EdgeElement(2, point2, point3, 0, false, true, 0, 1, 0, 3);
            sign = getSign(ref point1, ref edge3, points);
            edge3.limitSign = -sign;
            edges[0] = edge1;
            edges[1] = edge2;
            edges[2] = edge3;

            TriangleElement triangleElement = new TriangleElement(0,0,1,2,-edge1.limitSign, -edge2.limitSign, -edge3.limitSign,-1,-1,-1,0,1,2);
            triangles[0] = triangleElement;
        }
        void createOutsideTriangle(ref PointElement newPoint, ref TriangleElement limitTriangle, ref EdgeElement limitEdge, ref int startTriangleCreationIndex, ref int startEdgeCreationIndex,DynamicBuffer<TriangleElement> triangles, DynamicBuffer<PointElement> points, DynamicBuffer<EdgeElement> edges)
        {
            
            EdgeElement newEdge1 = new EdgeElement(startEdgeCreationIndex,points[limitEdge.pointIndex1], points[newPoint.index], startTriangleCreationIndex, false,true, limitEdge.edgeLimit1, startEdgeCreationIndex+1,0,2);
            PointElement p1 = points[limitEdge.pointIndex2];
            int sign1 = getSign(ref p1, newEdge1, points);
            newEdge1.limitSign = -sign1;
            
            startEdgeCreationIndex++;
            EdgeElement newEdge2 = new EdgeElement(startEdgeCreationIndex, points[limitEdge.pointIndex2], points[newPoint.index], startTriangleCreationIndex, false, true, limitEdge.edgeLimit2,newEdge1.index, 0,3);
            PointElement p2 = points[limitEdge.pointIndex1];
            int sign2 = getSign(ref p2, newEdge2, points);
            newEdge2.limitSign = -sign2;
            
            startEdgeCreationIndex++;
            TriangleElement newTriangle = new TriangleElement(startTriangleCreationIndex, limitEdge.index, newEdge1.index, newEdge2.index, limitEdge.limitSign,-newEdge1.limitSign,-newEdge2.limitSign, limitTriangle.index,-1,-1, limitEdge.pointIndex1,newPoint.index,limitEdge.pointIndex2);

            limitEdge.isLimit = false;
            EdgeElement limitEdge1 = edges[limitEdge.edgeLimit1];
            EdgeElement limitEdge2 = edges[limitEdge.edgeLimit2];
            setLimitOfPoint(limitEdge.pointIndex1, newEdge1.index, ref limitEdge1);
            setLimitOfPoint(limitEdge.pointIndex2, newEdge2.index, ref limitEdge2);
            edges[limitEdge.index] = limitEdge;
            edges[newEdge1.index] = newEdge1;
            edges[newEdge2.index] = newEdge2;
            edges[limitEdge1.index] = limitEdge1;
            edges[limitEdge2.index] = limitEdge2;

            startTriangleCreationIndex++;
            setTriangleIndex(limitEdge.nextTriangleEdgeIndex, newTriangle.index, ref limitTriangle);
            triangles[newTriangle.index] = newTriangle;
            triangles[limitTriangle.index] = limitTriangle;
            EdgeElement nextEdgeLimit = edges[limitEdge.edgeLimit1];
            EdgeElement firstPreviousEdge = newEdge2;
            EdgeElement previousEdge = newEdge1;
            int previousPointOfEdgeLimitIndex = limitEdge.pointIndex1;
            TriangleElement previousTriangle = newTriangle;
            int firstEdgeIndex = nextEdgeLimit.index;
            do
            {
                
                int otherPointIndex = getOtherPointIndexOfEdge(previousPointOfEdgeLimitIndex, nextEdgeLimit);
                bool isOutside = EdgeIsInside(nextEdgeLimit.limitSign, ref newPoint, nextEdgeLimit,points);
                if (isOutside)
                {
                    previousEdge.isLimit = false;
                    edges[previousEdge.index] = previousEdge;
                    nextEdgeLimit.isLimit = false;
                    edges[nextEdgeLimit.index] = nextEdgeLimit;
                    PointElement otherPoint = points[otherPointIndex];
                    int next2LimitEdge = getLimitEdgeWithPoint(ref otherPoint, ref nextEdgeLimit);
                    EdgeElement newEdge3 = new EdgeElement(startEdgeCreationIndex, points[otherPointIndex], points[newPoint.index], startTriangleCreationIndex, false, true, next2LimitEdge, firstPreviousEdge.index, 0,3);
                    PointElement proviousPoint = points[previousPointOfEdgeLimitIndex];
                    int sign3 = getSign(ref proviousPoint, newEdge3, points);
                    newEdge3.limitSign = -sign3;
                    edges[newEdge3.index] = newEdge3;
                    startEdgeCreationIndex++;

                    setLimitOfPoint(newPoint.index, newEdge3.index, ref firstPreviousEdge);
                    edges[firstPreviousEdge.index] = firstPreviousEdge;

                    EdgeElement edgeLimit1OfNextEdgeLimit = edges[getLimitEdgeWithPoint(ref otherPoint, ref nextEdgeLimit)];
                    setLimitOfPoint(otherPoint.index, newEdge3.index, ref edgeLimit1OfNextEdgeLimit);
                    edges[edgeLimit1OfNextEdgeLimit.index] = edgeLimit1OfNextEdgeLimit;

                    TriangleElement newTriangle2 = new TriangleElement(startTriangleCreationIndex, nextEdgeLimit.index, previousEdge.index, newEdge3.index, nextEdgeLimit.limitSign, previousEdge.limitSign, sign3, nextEdgeLimit.triangleLimitIndex, previousTriangle.index, -1, nextEdgeLimit.pointIndex1, nextEdgeLimit.pointIndex2, newPoint.index);
                    startTriangleCreationIndex++;
                    setTriangleIndex(previousEdge.nextTriangleEdgeIndex, newTriangle2.index, ref previousTriangle);
                    triangles[previousTriangle.index] = previousTriangle;
                    TriangleElement limitTriangle2 = triangles[nextEdgeLimit.triangleLimitIndex];
                    setTriangleIndexWithEdge(nextEdgeLimit.index, newTriangle2.index, ref limitTriangle2);
                    triangles[limitTriangle2.index] = limitTriangle2;
                    triangles[newTriangle2.index] = newTriangle2;

                    previousTriangle = newTriangle2;
                    previousEdge = newEdge3;
                    previousPointOfEdgeLimitIndex = otherPointIndex;
                    nextEdgeLimit = edges[getLimitEdgeWithPoint(ref otherPoint, ref nextEdgeLimit)];
                    if (firstEdgeIndex == nextEdgeLimit.index)
                    {
                        break;
                    }
                }
                else { break; }
            } while (true);

            nextEdgeLimit = edges[limitEdge.edgeLimit2];
            firstPreviousEdge = edges[previousEdge.index];
            EdgeElement previousEdge2 = edges[previousEdge.index];
            previousEdge = edges[newEdge2.index];
            previousPointOfEdgeLimitIndex = limitEdge.pointIndex2;
            previousTriangle = triangles[newTriangle.index];
            firstEdgeIndex = nextEdgeLimit.index;
            do
            {

                int otherPointIndex = getOtherPointIndexOfEdge(previousPointOfEdgeLimitIndex, nextEdgeLimit);
                bool isOutside = EdgeIsInside(nextEdgeLimit.limitSign, ref newPoint, nextEdgeLimit, points);
                if (isOutside)
                {

                    PointElement otherPoint = points[otherPointIndex];
                    
                    previousEdge.isLimit = false;
                    edges[previousEdge.index] = previousEdge;
                    nextEdgeLimit.isLimit = false;
                    edges[nextEdgeLimit.index] = nextEdgeLimit;
                    int next2LimitEdge = getLimitEdgeWithPoint(ref otherPoint, ref nextEdgeLimit);
                    EdgeElement newEdge3 = new EdgeElement(startEdgeCreationIndex, points[otherPointIndex], points[newPoint.index], startTriangleCreationIndex, false, true, next2LimitEdge, firstPreviousEdge.index, 0, 3);
                    PointElement proviousPoint = points[previousPointOfEdgeLimitIndex];
                    int sign3 = getSign(ref proviousPoint, newEdge3, points);
                    newEdge3.limitSign = -sign3;
                    edges[newEdge3.index] = newEdge3;
                    startEdgeCreationIndex++;

                    setLimitOfPoint(newPoint.index, newEdge3.index, ref firstPreviousEdge);
                    edges[firstPreviousEdge.index] = firstPreviousEdge;

                    EdgeElement edgeLimit1OfNextEdgeLimit = edges[getLimitEdgeWithPoint(ref otherPoint, ref nextEdgeLimit)];
                    setLimitOfPoint(otherPoint.index, newEdge3.index, ref edgeLimit1OfNextEdgeLimit);
                    edges[edgeLimit1OfNextEdgeLimit.index] = edgeLimit1OfNextEdgeLimit;

                    TriangleElement newTriangle2 = new TriangleElement(startTriangleCreationIndex, nextEdgeLimit.index, previousEdge.index, newEdge3.index, nextEdgeLimit.limitSign, previousEdge.limitSign, sign3, nextEdgeLimit.triangleLimitIndex, previousTriangle.index, -1, nextEdgeLimit.pointIndex1, nextEdgeLimit.pointIndex2, newPoint.index);
                    startTriangleCreationIndex++;
                    
                    
                    TriangleElement limitTriangle2 = triangles[nextEdgeLimit.triangleLimitIndex];
                    setTriangleIndexWithEdge(nextEdgeLimit.index, newTriangle2.index, ref limitTriangle2);
                    setTriangleIndex(previousEdge.nextTriangleEdgeIndex, newTriangle2.index, ref previousTriangle);
                    triangles[limitTriangle2.index] = limitTriangle2;
                    triangles[newTriangle2.index] = newTriangle2;
                    triangles[previousTriangle.index] = previousTriangle;
                    previousTriangle = newTriangle2;
                    previousEdge = newEdge3;
                    previousPointOfEdgeLimitIndex = otherPointIndex;
                    nextEdgeLimit = edgeLimit1OfNextEdgeLimit;

                    setLimitOfPoint(newPoint.index, newEdge3.index, ref previousEdge2);
                    edges[previousEdge2.index] = previousEdge2;

                    if(firstEdgeIndex== nextEdgeLimit.index)
                    {
                        break;
                    }
                }
                else { break; }
            } while (true);
        }
        void setLimitOfPoint(int pointIndex,int limitEdgeIndex, ref EdgeElement edge)
        {
            if (pointIndex == edge.pointIndex1)
            {
                edge.edgeLimit1 = limitEdgeIndex;
            }
            else if (pointIndex == edge.pointIndex2)
            {
                edge.edgeLimit2 = limitEdgeIndex;
            }
        }
        int getLimitEdgeWithPoint(PointElement point, ref EdgeElement edge)
        {
            if (point.index == edge.pointIndex1)
            {
                return edge.edgeLimit1;
            }
            else if (point.index == edge.pointIndex2)
            {
                return edge.edgeLimit2;
            }
            else
            {
                return -1;
            }
        }
        int getLimitEdgeWithPoint(ref PointElement point, ref EdgeElement edge)
        {
            if (point.index == edge.pointIndex1)
            {
                return edge.edgeLimit1;
            }
            else if(point.index == edge.pointIndex2)
            {
                return edge.edgeLimit2;
            }
            else
            {
                return -1;
            }
        }
        void setTriangleIndex(int index,int triangleIndex,ref TriangleElement triangleElement){
            if (index == 1)
            {
                triangleElement.triangleIndex1 = triangleIndex;
                return;
            }
            if (index == 2)
            {
                triangleElement.triangleIndex2 = triangleIndex;
                return;
            }
            if (index == 3)
            {
                triangleElement.triangleIndex3 = triangleIndex;
                return;
            }
        }
        void setTriangleIndexWithEdge(int edgeIndex, int triangleIndex, ref TriangleElement triangleElement)
        {
            if (edgeIndex == triangleElement.edgeIndex1)
            {
                triangleElement.triangleIndex1 = triangleIndex;
                return;
            }
            if (edgeIndex == triangleElement.edgeIndex2)
            {
                triangleElement.triangleIndex2 = triangleIndex;
                return;
            }
            if (edgeIndex == triangleElement.edgeIndex3)
            {
                triangleElement.triangleIndex3 = triangleIndex;
                return;
            }
        }
        void createTrianglesInsideTriangle(ref PointElement newPoint, ref TriangleElement triangle,ref int startTriangleCreationIndex,ref int startEdgeCreationIndex, DynamicBuffer<TriangleElement> triangles, DynamicBuffer<PointElement> points, DynamicBuffer<EdgeElement> edges)
        {
            int insideTriangleIndex = getInsideTriangle(ref triangle, ref newPoint, points, triangles, edges);
            TriangleElement insideTriangle = triangles[insideTriangleIndex];
            insideTriangle.thereIsTrianglesInside = true;
            int newEdgeIndex1 = startEdgeCreationIndex;
            EdgeElement newEdge1 = edges[newEdgeIndex1];
            newEdge1.index = newEdgeIndex1;
            startEdgeCreationIndex++;
            int newEdgeIndex2 = startEdgeCreationIndex;
            EdgeElement newEdge2 = edges[newEdgeIndex2];
            newEdge2.index = newEdgeIndex2;
            startEdgeCreationIndex++;
            int newTriangleIndex=-1;
            createEdgeOfTriangle(ref newPoint, insideTriangle.edgeIndex1, insideTriangle.edgeSign1, edges, points, triangles, ref startTriangleCreationIndex, ref newEdge1, ref newEdge2,ref newTriangleIndex, true,true, insideTriangle.triangleIndex1, startTriangleCreationIndex +1 , startTriangleCreationIndex + 2);
            insideTriangle.insideTriangleIndex1 = newTriangleIndex;
            int otherEdgeIndex, otherEdgeSign;
            getOtherEdgeWithPoint(edges[insideTriangle.edgeIndex1].pointIndex1, insideTriangle.edgeIndex1, ref insideTriangle,edges,out otherEdgeIndex,out otherEdgeSign);
            int newEdgeIndex3 = startEdgeCreationIndex;
            EdgeElement newEdge3 = edges[newEdgeIndex3];
            newEdge3.index = newEdgeIndex3;
            startEdgeCreationIndex++;
            createEdgeOfTriangle(ref newPoint, otherEdgeIndex, otherEdgeSign, edges, points, triangles, ref startTriangleCreationIndex, ref newEdge1, ref newEdge3, ref newTriangleIndex, false,true, insideTriangle.triangleIndex2, startTriangleCreationIndex - 1, startTriangleCreationIndex + 1);
            insideTriangle.insideTriangleIndex2 = newTriangleIndex;
            getOtherEdgeWithPoint(edges[insideTriangle.edgeIndex1].pointIndex2, insideTriangle.edgeIndex1, ref insideTriangle, edges, out otherEdgeIndex, out otherEdgeSign);
            createEdgeOfTriangle(ref newPoint, otherEdgeIndex, otherEdgeSign, edges, points, triangles, ref startTriangleCreationIndex, ref newEdge2, ref newEdge3, ref newTriangleIndex, false,false, insideTriangle.triangleIndex3, startTriangleCreationIndex - 1, startTriangleCreationIndex - 2);
            insideTriangle.insideTriangleIndex3 = newTriangleIndex;
            edges[newEdgeIndex1] = newEdge1;
            edges[newEdgeIndex2] = newEdge2;
            edges[newEdgeIndex3] = newEdge3;
            triangles[insideTriangleIndex] = insideTriangle;
        }
        void createEdgeOfTriangle(ref PointElement newPoint, int triangleEdgeIndex,int triangleEdgeSign, DynamicBuffer<EdgeElement> edges, DynamicBuffer<PointElement> points, DynamicBuffer<TriangleElement> triangles, ref int startTriangleCreationIndex,ref  EdgeElement newEdge1, ref EdgeElement newEdge2, ref int newTriangleIndex, bool updateEdge1,bool updateEdge2, int triangleIndex1, int triangleIndex2, int triangleIndex3)
        {
            if (updateEdge1)
            {
                newEdge1.pointIndex1 = edges[triangleEdgeIndex].pointIndex1;
                newEdge1.pointIndex2 = newPoint.index;
                newEdge1.perpendicular = getPerpendicular(ref newEdge1, points);
                newEdge1.isDeleted = false;
                newEdge1.isLimit = false;
                newEdge1.isDisable = false;
            }
            if (updateEdge2)
            {
                newEdge2.pointIndex1 = edges[triangleEdgeIndex].pointIndex2;
                newEdge2.pointIndex2 = newPoint.index;
                newEdge2.perpendicular = getPerpendicular(ref newEdge2, points);
                newEdge2.isDeleted = false;
                newEdge2.isLimit = false;
                newEdge2.isDisable = false;
            }

            PointElement p1 = points[newEdge2.pointIndex1];
            int sign1 = getSign(ref p1, newEdge1, points);
            PointElement p2 = points[newEdge1.pointIndex1];
            int sign2 = getSign(ref p2, newEdge2, points);
            newTriangleIndex = createTriangleInside(edges[triangleEdgeIndex], ref newEdge1, ref newEdge2, ref p1, ref p2, ref newPoint, triangleEdgeSign, sign1, sign2, triangles,ref startTriangleCreationIndex,triangleIndex1,triangleIndex2,triangleIndex3);
        }
        Vector2 getPerpendicular(ref EdgeElement edgeElement,DynamicBuffer<PointElement> points)
        {
            return Vector2.Perpendicular(points[edgeElement.pointIndex2].position - points[edgeElement.pointIndex1].position);
        }
        bool getOtherEdgeWithPoint(int pointIndex, int edgeIndex, ref TriangleElement triangle, DynamicBuffer<EdgeElement> edges, out int otherEdgeIndex, out int otherEdgeSign)
        {
            if(edgeContainsPoint(pointIndex, edges[triangle.edgeIndex1]) && triangle.edgeIndex1!= edgeIndex)
            {
                otherEdgeIndex = triangle.edgeIndex1;
                otherEdgeSign = triangle.edgeSign1;
                return true;
            }
            if (edgeContainsPoint(pointIndex, edges[triangle.edgeIndex2]) && triangle.edgeIndex2 != edgeIndex)
            {
                otherEdgeIndex = triangle.edgeIndex2;
                otherEdgeSign = triangle.edgeSign2;
                return true;
            }
            if (edgeContainsPoint(pointIndex, edges[triangle.edgeIndex3]) && triangle.edgeIndex3 != edgeIndex)
            {
                otherEdgeIndex = triangle.edgeIndex3;
                otherEdgeSign = triangle.edgeSign3;
                return true;
            }
            otherEdgeIndex = -1;
            otherEdgeSign = 0;
            return false;
        }
        bool edgeContainsPoint(int pointIndex,EdgeElement edgeElement)
        {
            if (edgeElement.pointIndex1 == pointIndex) return true;
            if (edgeElement.pointIndex2 == pointIndex) return true;
            return false;
        }
        int getOtherPointIndexOfEdge(int pointIndex, EdgeElement edgeElement)
        {
            if (edgeElement.pointIndex1 == pointIndex) return edgeElement.pointIndex2;
            else return edgeElement.pointIndex1;
        }
        int createTriangleInside(EdgeElement edge1, ref EdgeElement edge2,ref EdgeElement edge3,ref PointElement point1, ref PointElement point2, ref PointElement point3, int edgeSign1, int edgeSign2, int edgeSign3, DynamicBuffer<TriangleElement> triangles,ref int startTriangleCreationIndex,int triangleIndex1,int triangleIndex2,int triangleIndex3)
        {
            TriangleElement newTriangle = triangles[startTriangleCreationIndex];
            newTriangle.index = startTriangleCreationIndex;
            newTriangle.thereIsTrianglesInside = false;
            newTriangle.edgeIndex1 = edge1.index;
            newTriangle.edgeSign1 = edgeSign1;
            newTriangle.edgeIndex2 = edge2.index;
            newTriangle.edgeSign2 = edgeSign2;
            newTriangle.edgeIndex3 = edge3.index;
            newTriangle.edgeSign3 = edgeSign3;
            newTriangle.pointIndex1 = point1.index;
            newTriangle.pointIndex2 = point2.index;
            newTriangle.pointIndex3 = point3.index;
            newTriangle.triangleIndex1 = triangleIndex1;
            newTriangle.triangleIndex2 = triangleIndex2;
            newTriangle.triangleIndex3 = triangleIndex3;
            newTriangle.isDeleted = false;
            triangles[startTriangleCreationIndex] = newTriangle;
            int index = startTriangleCreationIndex;
            startTriangleCreationIndex++;
            return index;
        }
        bool TriangleIsInside(TriangleElement triangle, ref PointElement point, DynamicBuffer<PointElement> points, DynamicBuffer<EdgeElement> edges,ref int nextEdgeIndex, ref int nextTriangleIndex)
        {
            bool isInside1 = EdgeIsInside(triangle.edgeSign1, ref point,edges[triangle.edgeIndex1], points);
            if (!isInside1)
            {
                nextEdgeIndex = triangle.edgeIndex1;
                if (!edges[nextEdgeIndex].isLimit)
                {
                    nextTriangleIndex = triangle.triangleIndex1;
                }
                return false;
            }
            bool isInside2 = EdgeIsInside(triangle.edgeSign2, ref point,edges[triangle.edgeIndex2], points);
            if (!isInside2)
            {
                nextEdgeIndex = triangle.edgeIndex2;
                if (!edges[nextEdgeIndex].isLimit)
                {
                    nextTriangleIndex = triangle.triangleIndex2;
                }
                return false;
            }
            bool isInside3 = EdgeIsInside(triangle.edgeSign3, ref point,edges[triangle.edgeIndex3], points);
            if (!isInside3)
            {
                nextEdgeIndex = triangle.edgeIndex3;
                if (!edges[nextEdgeIndex].isLimit)
                {
                    nextTriangleIndex = triangle.triangleIndex3;
                }
                return false;
            }
            nextTriangleIndex = triangle.index;
            return true;
        }
        int getInsideTriangle(ref TriangleElement triangle, ref PointElement point, DynamicBuffer<PointElement> points, DynamicBuffer<TriangleElement> triangles, DynamicBuffer<EdgeElement> edges)
        {
            int result = triangle.index;
            TriangleElement testTriangle = triangle;
            int nextEdgeIndex = 0;
            int nextTriangleIndex=-1;
            int count = 0;
            if (!triangle.thereIsTrianglesInside) return result;
            result = triangle.insideTriangleIndex1;
            do
            {
                bool isInside = TriangleIsInside(triangles[result], ref point, points, edges, ref nextEdgeIndex, ref nextTriangleIndex);

                
                if (isInside && !triangles[result].thereIsTrianglesInside)
                {
                    return result;
                }
                else
                {
                    result = nextTriangleIndex;
                    testTriangle = triangles[nextTriangleIndex];
                }

                count++;
            } while (count<100);
            //Debug.LogWarning("getInsideTriangle error");
            return result;
        }
        bool EdgeIsInside(int sign,ref PointElement point,EdgeElement edge, DynamicBuffer<PointElement> points)
        {
            return sign == getSign(ref point, edge,points);
        }
        int getSign(ref PointElement point,ref EdgeElement edge, DynamicBuffer<PointElement> points)
        {
            Vector2 dir = point.position - points[edge.pointIndex1].position;
            int sign2 = (int)Mathf.Sign(Vector2.Dot(edge.perpendicular, dir));
            return sign2;
        }
        int getSign(ref PointElement point,EdgeElement edge, DynamicBuffer<PointElement> points)
        {
            Vector2 dir = point.position - points[edge.pointIndex1].position;
            int sign2 = (int)Mathf.Sign(Vector2.Dot(edge.perpendicular, dir));
            return sign2;
        }
        public static float getNormalizedArea(TriangleElement triangle, DynamicBuffer<PointElement> points)
        {
            float area = getArea(triangle,points);
            float perimeter = getPerimeter(triangle,points);
            return area / perimeter;
        }
        public static float getArea(TriangleElement triangle, DynamicBuffer<PointElement> points)
        {
            Vector2 p1 = points[triangle.pointIndex1].position;
            Vector2 p2 = points[triangle.pointIndex2].position;
            Vector2 p3 = points[triangle.pointIndex3].position;
            Vector2 v1 = p2 - p1;
            Vector2 v2 = p3 - p1;
            return Mathf.Abs(v1.magnitude*v2.magnitude*Mathf.Sin(Vector2.Angle(v1,v2)*Mathf.Deg2Rad)) / 2;
        }
        public static float getPerimeter(TriangleElement triangle, DynamicBuffer<PointElement> points)
        {
            Vector2 p1 = points[triangle.pointIndex1].position;
            Vector2 p2 = points[triangle.pointIndex2].position;
            Vector2 p3 = points[triangle.pointIndex3].position;
            float d1 = Vector2.Distance(p1,p2);
            float d2 = Vector2.Distance(p1,p3);
            float d3 = Vector2.Distance(p2,p3);
            return d1+d2+d3;
        }
        public static int getLongestEdge(ref TriangleElement triangle, DynamicBuffer<EdgeElement> edges, DynamicBuffer<PointElement> points)
        {
            float d1 = getEdgeLenght(edges[triangle.edgeIndex1], points);
            float d2 = getEdgeLenght(edges[triangle.edgeIndex2], points);
            float d3 = getEdgeLenght(edges[triangle.edgeIndex3], points);
            int result = triangle.edgeIndex1;
            float d = d1;
            if (d < d2)
            {
                result = triangle.edgeIndex2;
                d = d2;
            }
            if (d < d3)
            {
                result = triangle.edgeIndex3;
            }
            return result;
        }
        public static float getEdgeLenght(EdgeElement edge, DynamicBuffer<PointElement> points)
        {
            Vector2 p1 = points[edge.pointIndex1].position;
            Vector2 p2 = points[edge.pointIndex2].position;
            return Vector2.Distance(p1,p2);
        }
        public static float getBiggetsAngle(TriangleElement triangle, DynamicBuffer<PointElement> points)
        {
            Vector2 p1 = points[triangle.pointIndex1].position;
            Vector2 p2 = points[triangle.pointIndex2].position;
            Vector2 p3 = points[triangle.pointIndex3].position;
            Vector2 dir1 = p2 - p1;
            Vector2 dir2 = p3 - p1;
            Vector2 dir3 = p1 - p2;
            Vector2 dir4 = p3 - p2;
            Vector2 dir5 = p1 - p3;
            Vector2 dir6 = p2 - p3;
            float a1 = Vector2.Angle(dir1, dir2);
            float a2 = Vector2.Angle(dir3, dir4);
            float a3 = Vector2.Angle(dir5, dir6);
            float result = a1;
            if (result < a2)
            {
                result = a2;
            }
            if (result < a3)
            {
                result = a3;
            }
            return result;
        }
    }
}
