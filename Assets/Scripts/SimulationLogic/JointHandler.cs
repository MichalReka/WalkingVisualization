using System.Collections;
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
    public int targetVelocity;
    ConfigurableJoint joint;
    public float[] input;
    public static int inputSize = 2;
    public MoveableAxis axisToMove;

    Vector3 startingRotation;
    public float isMoving = -1f;
    public void initArm()
    {
        startingRotation = transform.localEulerAngles;
        joint = GetComponent<ConfigurableJoint>();
        if (hasLimits)
        {
            input = new float[inputSize];
        }
        else
        {
            input = new float[inputSize - 1];
        }
        setInput();
    }


    public static float normalizeValue(float value, float min, float max) //wartosci od -1 do 1 - normalizacja potrzebna dla zwiekszenia wydajnosci
    {
        float targetMin = -1;
        float targetMax = 1;
        float translatedValue = (targetMax - targetMin) / (max - min) * (value - max) + targetMax;
        return translatedValue;
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
    public float[] setInput()
    {
        int currentIndex = 0;
        if (axisToMove == MoveableAxis.x)
        {
            if (hasLimits)
            {
                input[currentIndex] = normalizeValue(AdjustRotation(transform.localEulerAngles.x), startingRotation.x + joint.lowAngularXLimit.limit, startingRotation.x + joint.highAngularXLimit.limit);
                currentIndex++;
            }
            input[currentIndex] = normalizeValue(joint.targetAngularVelocity.x, -targetVelocity, targetVelocity);
            currentIndex++;
        }
        else if (axisToMove == MoveableAxis.y)
        {
            if (hasLimits)
            {
                input[currentIndex] = normalizeValue(AdjustRotation(transform.localEulerAngles.y), startingRotation.y - joint.angularYLimit.limit, startingRotation.y + joint.angularYLimit.limit);
                currentIndex++;
            }
            input[currentIndex] = normalizeValue(joint.targetAngularVelocity.y, -targetVelocity, targetVelocity);
            currentIndex++;
        }
        else
        {
            if (hasLimits)
            {
                input[currentIndex] = normalizeValue(AdjustRotation(transform.localEulerAngles.z), startingRotation.z - joint.angularZLimit.limit, startingRotation.z + joint.angularZLimit.limit);
                currentIndex++;
            }
            input[currentIndex] = normalizeValue(joint.targetAngularVelocity.z, -targetVelocity, targetVelocity);
            currentIndex++;
        }
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
        // int velocityBorderDivision = 5;
        var velocityVector = joint.targetAngularVelocity;
        // isMoving = output[0];
        // if (isMoving < 0)
        // {
        //         velocityVector[(int)axisToMove] = 0;
        // }
        // else
        // {
            // float absVelocity = translateToValue(-targetVelocity, targetVelocity, Mathf.Abs(output[1]) * 2 - 1);    //abs*2-1 poniewaz np 0.2 i -0.2 maja miec te same predkosci
            velocityVector[(int)axisToMove] = translateToValue(-targetVelocity, targetVelocity, output[0]);    //abs*2-1 poniewaz np 0.2 i -0.2 maja miec te same predkosci
            // if (output[1] < 0)
            // {
            //     velocityVector[(int)axisToMove] = -absVelocity;
            // }
            // else
            // {
            //     velocityVector[(int)axisToMove] = absVelocity;
            // }
        // }

        // velocityVector[(int)axisToMove] = translateToValue(-targetVelocity, targetVelocity, output[1]);  //taki zapis zmniejsza ruchliwosc osobnikow
        joint.targetAngularVelocity = velocityVector;
    }
    public void ResetJoint()
    {
        joint.targetAngularVelocity = new Vector3(0,0,0);
    }

}