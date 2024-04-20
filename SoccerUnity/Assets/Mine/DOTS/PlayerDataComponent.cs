using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

namespace DOTS_ChaserDataCalculation
{
    [System.Serializable]
    public struct PlayerDataGenericParams : ISharedComponentData
    {
        public float maxSpeed, acceleration, decceleration;
    }
    public struct PlayerDataComponent : IComponentData
    {
        public bool useAccelerationGetTimeToReachPosition;
        public int id;
        public Vector3 position,bodyY0Forward,normalizedBodyY0Forward,ForwardVelocity,normalizedForwardVelocity;
        public float maxSpeed, acceleration, decceleration;
        public float maxJumpHeight;
        public float scope;
        public float currentSpeed, minSpeedForRotate, maxAngleForRun, maxSpeedRotation, maxSpeedForReachBall;
        public float drag;
        public Vector3 fieldNormal,fieldPosition;
        public int segmentedPathSize;
        public float bodyBallRadio,height;
        public bool isGoalkeeper;
        public float normalMaximumJumpHeight, goalkeeperMaximumJumpHeight;
        public float maxReceiverHeight,maxKickForce;
        public bool has_OptimalPoint;
        public PlayerDataComponent(bool useAccelerationGetTimeToReachPosition, int id, Vector3 position, Vector3 bodyY0Forward, Vector3 forwardVelocity,Vector3 normalizedForwardVelocity, float maxSpeed, float acceleration, float decceleration, float maxJumpHeight, float scope, float currentSpeed, float minSpeedForRotate, float maxAngleForRun, float maxSpeedRotation, float maxSpeedForReachBall, float drag, Vector3 fieldNormal, Vector3 fieldPosition, int segmentedPathSize, float bodyBallRadio, float height, bool isGoalkeeper, float normalMaximumJumpHeight, float goalkeeperMaximumJumpHeight, float maxReceiverHeight,float maxKickForce) : this()
        {
            this.useAccelerationGetTimeToReachPosition = useAccelerationGetTimeToReachPosition;
            this.id = id;
            this.position = position;
            this.bodyY0Forward = bodyY0Forward;
            ForwardVelocity = forwardVelocity;
            this.normalizedForwardVelocity = normalizedForwardVelocity;
            this.maxSpeed = maxSpeed;
            this.acceleration = acceleration;
            this.decceleration = decceleration;
            this.maxJumpHeight = maxJumpHeight;
            this.scope = scope;
            this.currentSpeed = currentSpeed;
            this.minSpeedForRotate = minSpeedForRotate;
            this.maxAngleForRun = maxAngleForRun;
            this.maxSpeedRotation = maxSpeedRotation;
            this.maxSpeedForReachBall = maxSpeedForReachBall;
            this.drag = drag;
            this.fieldNormal = fieldNormal;
            this.fieldPosition = fieldPosition;
            this.segmentedPathSize = segmentedPathSize;
            this.bodyBallRadio = bodyBallRadio;
            this.height = height;
            this.isGoalkeeper = isGoalkeeper;
            this.normalMaximumJumpHeight = normalMaximumJumpHeight;
            this.goalkeeperMaximumJumpHeight = goalkeeperMaximumJumpHeight;
            this.maxReceiverHeight = maxReceiverHeight;
            this.maxKickForce = maxKickForce;
        }
    }
    public struct MyFloatArray
    {
        public float f1,f2,f3,f4,f5;
        public int lenght;
        public void Clear()
        {
            lenght = 0;
        }
        public float Get(int index)
        {
            if (index == 0) return f1;
            else if (index == 1) return f2;
            else if (index == 2) return f3;
            else if (index == 3) return f4;
            else return f5;
        }
        public void Add(float value)
        {
            if (lenght == 0) f1 = value;
            else if (lenght == 1) f2 = value;
            else if (lenght == 2) f3 = value;
            else if (lenght == 3) f4 = value;
            else if (lenght == 4) f5 = value;
            lenght++;
            lenght = lenght < 5 ? lenght : 0;
        }

    }
    public struct OptimalPointEntityElement : IBufferElementData
    {
        
        public Entity entity;
    }
    public struct OptimalPointListEntityElement : IBufferElementData
    {
        public int segmentedPathSize;
        public Entity entity;

        public OptimalPointListEntityElement(int segmentedPathSize, Entity entity)
        {
            this.segmentedPathSize = segmentedPathSize;
            this.entity = entity;
        }
    }
}
