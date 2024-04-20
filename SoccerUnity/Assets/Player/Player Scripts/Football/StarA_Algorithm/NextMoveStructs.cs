using FieldTriangleSpace;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NextMove_Algorithm
{
    public struct PlayerData
    {
        public bool useAccelerationGetTimeToReachPosition;
        public int id;
        public Vector3 position, bodyY0Forward, normalizedBodyY0Forward, ForwardVelocity, normalizedForwardVelocity;
        public float maxSpeed, acceleration, decceleration;
        public float maxJumpHeight;
        public float scope;
        public float currentSpeed, minSpeedForRotate, maxAngleForRun, maxSpeedRotation, maxSpeedForReachBall;
        public float drag;
        public Vector3 fieldNormal, fieldPosition;

        public PlayerData(bool useAccelerationGetTimeToReachPosition, int id, Vector3 position, Vector3 bodyY0Forward, Vector3 forwardVelocity, Vector3 normalizedForwardVelocity, float maxSpeed, float acceleration, float decceleration, float maxJumpHeight, float scope, float currentSpeed, float minSpeedForRotate, float maxAngleForRun, float maxSpeedRotation, float maxSpeedForReachBall, float drag, Vector3 fieldNormal, Vector3 fieldPosition) : this()
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
        }
    }
}
