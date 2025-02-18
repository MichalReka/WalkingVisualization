﻿using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

public class JointHandler : MonoBehaviour
{
    public bool hasLimits = false;
    public enum MoveableAxis
    {
        x = 0, y = 1, z = 2
    }
    public float targetVelocity;
    ConfigurableJoint joint;
    public float[] input;
    public static int inputSize = 2;
    public MoveableAxis localRotationAxis;
    public MoveableAxis jointAxis;
    public bool checkingRotation;
    // public int jointPairIndex;
    Vector3 startingRotation;
    // public float averageVelocityOutput = 0;
    public List<float> rotationsInTimeSpan;
    float _lowLimit;
    float _highLimit;
    float _minimumRotationChange;
    public float max;
    public float min;
    public void initArm()
    {
        startingRotation = transform.localEulerAngles;
        joint = GetComponent<ConfigurableJoint>();
        if (hasLimits)
        {
            if (jointAxis == MoveableAxis.x)
            {
                _lowLimit = joint.lowAngularXLimit.limit;
                _highLimit =  joint.highAngularXLimit.limit;
            }
            else if (jointAxis == MoveableAxis.y)
            {
                _lowLimit = -joint.angularYLimit.limit;
                _highLimit = joint.angularYLimit.limit;
            }
            else
            {
                _lowLimit = -joint.angularZLimit.limit;
                _highLimit = joint.angularZLimit.limit;
            }
            input = new float[inputSize];
        }
        else
        {
            input = new float[inputSize - 1];
        }
        if (PopulationInputData.forceSimilarPartsSynchronization)
        {
            rotationsInTimeSpan = new List<float>();
            _minimumRotationChange = (Mathf.Abs(_lowLimit) + Mathf.Abs(_highLimit)) * 0.4f;
        }
        SetInput();
    }
    

    public static float NormalizeValue(float value, float sourceMin, float sourceMax)
    {
        float targetMin = -1;
        float targetMax = 1;
        float normalizedValue = (targetMax - targetMin) * ((value-sourceMin)/(sourceMax-sourceMin))+targetMin;
        return normalizedValue;
    }
    public static float AdjustRotation(float rotationAngle)
    {
        if (rotationAngle > 180)
        {
            return rotationAngle - 360;
        }
        else
        {
            return rotationAngle;
        }
    }
    public Quaternion GetJointRotation()
    {
        //https://answers.unity.com/questions/1694646/how-to-get-the-rotation-of-configurable-joint.html
        return (Quaternion.Inverse(joint.connectedBody.transform.rotation) * joint.transform.rotation);
    }
    public float[] SetInput()
    {
        int currentIndex = 0;
        if (hasLimits)
        {
            input[currentIndex] = NormalizeValue(-AdjustRotation(GetJointRotation().eulerAngles[(int)localRotationAxis]), _lowLimit, _highLimit);
            currentIndex++;
        }
        input[currentIndex] = NormalizeValue(joint.targetAngularVelocity[(int)jointAxis], -targetVelocity, targetVelocity);
        currentIndex++;
        return input;
    }
    public static float translateToValue(float min, float max, float output)   //tlumacze output na wartosci
    {
        var y = (max + min) / 2.0f;
        var x = max - y;
        var value = output * x + y;
        return value;
    }
    public void TranslateOutput(List<float> output)
    {
        var velocityVector = joint.targetAngularVelocity;
        velocityVector[(int)jointAxis] = translateToValue(-targetVelocity, targetVelocity, output[0]);
        joint.targetAngularVelocity = velocityVector;
    }
    public void ResetJoint()
    {
        if (PopulationInputData.forceSimilarPartsSynchronization)
        {
            rotationsInTimeSpan.Clear();
        }
        joint.targetAngularVelocity = new Vector3(0, 0, 0);
    }
    public void AddCurrentRotation()
    {
        rotationsInTimeSpan.Add(AdjustRotation(transform.localEulerAngles[(int)localRotationAxis]));
        max = rotationsInTimeSpan.Max();
        min = rotationsInTimeSpan.Min();
    }
    public void ClearRotations()
    {
        rotationsInTimeSpan.Clear();
    }
    public bool RotationsMinChangeFulfilled()
    {
        // if ((rotationsInTimeSpan.Max() - rotationsInTimeSpan.Min()) > _minimumRotationChange)
        // {
        // }
        // else
        // {
        // }
        return true;
    }
}