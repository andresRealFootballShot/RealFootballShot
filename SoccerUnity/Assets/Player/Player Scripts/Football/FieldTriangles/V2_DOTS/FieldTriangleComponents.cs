using System.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using System;
using System.Collections.Generic;

namespace FieldTriangleV2
{
    public struct PointElement : IBufferElementData
    {
        public Vector2 position;
        public bool isDeleted;
        public int index;

        public PointElement(Vector2 position, bool isDeleted, int index)
        {
            this.position = position;
            this.isDeleted = isDeleted;
            this.index = index;
        }
    }
    public struct LonelyPointElement : IBufferElementData
    {
        public Vector2 position;
        public int index;
        public bool isBarycenter;
        public LonelyPointElement(Vector2 position, int index,bool isBarycenter)
        {
            this.position = position;
            this.index = index;
            this.isBarycenter = isBarycenter;
        }
    }
    public struct LonelyPointElement2 : IBufferElementData
    {
        public Vector2 position;
        public int index;
        public bool straightReachBall, parabolicReachBall;
        public float weight;
        public int order;
        public LonelyPointElement2(Vector2 position, int index)
        {
            this.position = position;
            this.index = index;
            straightReachBall = false;
            parabolicReachBall = false;
            weight = Mathf.Infinity;
            order = -1;
        }
        public LonelyPointElement2(LonelyPointElement lonelyPointElement)
        {
            this.position = lonelyPointElement.position;
            this.index = lonelyPointElement.index;
            straightReachBall = false;
            parabolicReachBall = false;
            weight = Mathf.Infinity;
            order = -1;
        }
    }
    public struct BufferSizeComponent : IComponentData
    {
        public Entity enity;
        public int pointSize;
        public int trianglesResultSize;
        public int edgesResultSize;
        public int lonelyPointsResultSize;
        public int lonelyPointDistance;
        public int maxLonelyPointsOfEdgeCount;
        public float minNormalizedArea;
        public BufferSizeComponent(int pointSize,int lonelyPointDistance,int maxLonelyPointsOfEdgeCount, float minNormalizedArea, Entity enity) : this()
        {
            this.pointSize = pointSize;
            this.enity = enity;
            this.lonelyPointDistance = lonelyPointDistance;
            this.maxLonelyPointsOfEdgeCount = maxLonelyPointsOfEdgeCount;
            this.minNormalizedArea = minNormalizedArea;
        }
    }
    public struct EdgeLimitElement : IBufferElementData
    {
        public int edgeIndex;
    }
    public struct EdgeElement : IBufferElementData
    {
        public int index;
        public int pointIndex1, pointIndex2;
        public Vector2 perpendicular;
        public bool isDeleted;
        public bool isLimit;
        public int edgeLimit1, edgeLimit2;
        public int triangleLimitIndex;
        public int nextTriangleEdgeIndex;
        public int limitSign;
        public bool isDisable;
        public EdgeElement(int index,PointElement point1, PointElement point2, int triangleLimitIndex, bool isDeleted, bool isLimit, int edgeLimit1, int edgeLimit2, int limitSign,int nextTriangleEdgeIndex)
        {
            pointIndex1 = point1.index;
            pointIndex2 = point2.index;
            perpendicular = Vector2.Perpendicular(point2.position - point1.position);
            this.isDeleted = isDeleted;
            this.isLimit = isLimit;
            this.edgeLimit1 = edgeLimit1;
            this.edgeLimit2 = edgeLimit2;
            this.limitSign = limitSign;
            this.index = index;
            this.triangleLimitIndex = triangleLimitIndex;
            this.nextTriangleEdgeIndex = nextTriangleEdgeIndex;
            isDisable = false;
        }
    }
    public struct TriangleElement : IBufferElementData
    {
        public int index;
        public int edgeIndex1, edgeIndex2, edgeIndex3;
        public int edgeSign1, edgeSign2, edgeSign3;
        public int pointIndex1, pointIndex2, pointIndex3;
        public int triangleIndex1, triangleIndex2, triangleIndex3;
        public bool thereIsTrianglesInside;
        public int insideTriangleIndex1, insideTriangleIndex2, insideTriangleIndex3;
        public bool isDeleted;

        public TriangleElement(int index, int edgeIndex1, int edgeIndex2, int edgeIndex3, int edgeSign1, int edgeSign2, int edgeSign3, int triangleIndex1, int triangleIndex2, int triangleIndex3, int pointIndex1, int pointIndex2, int pointIndex3)
        {
            this.index = index;
            this.edgeIndex1 = edgeIndex1;
            this.edgeIndex2 = edgeIndex2;
            this.edgeIndex3 = edgeIndex3;
            this.edgeSign1 = edgeSign1;
            this.edgeSign2 = edgeSign2;
            this.edgeSign3 = edgeSign3;
            this.triangleIndex1 = triangleIndex1;
            this.triangleIndex2 = triangleIndex2;
            this.triangleIndex3 = triangleIndex3;
            this.pointIndex1 = pointIndex1;
            this.pointIndex2 = pointIndex2;
            this.pointIndex3 = pointIndex3;
            insideTriangleIndex1 = -1;
            insideTriangleIndex2 = -1;
            insideTriangleIndex3 = -1;
            isDeleted = false;
            thereIsTrianglesInside = false;
        }
    }
}
