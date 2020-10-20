using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

public class JointHandler : MonoBehaviour
{
    public enum MoveableAxis{
        x=0,y=1,z=2
    }
    // public int maxForce;
    public int targetVelocity;
    ConfigurableJoint joint;
    //prosta implementacja sieci neuronowej
    public float[] input;
    public static int inputSize=3;
    public MoveableAxis axisToMove;
    float startingBodyY;
    //jako input przyjmuje - pozycje, obrot, przyspieszenie, przyspieszenie katowe czesci ktora sie porusza
    //jako output motor, limit, velocity
    // Start is called before the first frame update
    Transform animalBody;
    Rigidbody rigidBody;
    // ConfigurableJoint bodyLimits;
    Vector3 startingRotation;
    float maxMoveTime = 1f;
    public float moveTimeLeft = -1f;
    public void initArm()
    {
        rigidBody = GetComponent<Rigidbody>();
        animalBody = transform.parent.gameObject.transform.Find("body");
        startingBodyY = animalBody.transform.position.y;
        startingRotation=transform.localEulerAngles;
        joint = GetComponent<ConfigurableJoint>();
        input = new float[inputSize];
        setInput();
    }


    public static float normalizeValue(float value, float min, float max) //wartosci od -1 do 1 - normalizacja potrzebna dla zwiekszenia wydajnosci
    {
        float targetMin=-1;
        float targetMax=1;
        float translatedValue=(targetMax-targetMin)/(max-min)*(value-max)+targetMax;
        return translatedValue;   
    }
    float AdjustRotation(float rotationAngle)
    {
        if(rotationAngle>180)
        {
            return rotationAngle-360;
        }
        else
        {
            return rotationAngle;
        }
    }
    public float[] setInput()
    {
        if(axisToMove==MoveableAxis.x)
        {
            input[0]=normalizeValue(AdjustRotation(transform.localEulerAngles.x),startingRotation.x+joint.lowAngularXLimit.limit,startingRotation.x+joint.highAngularXLimit.limit);
            input[1]=normalizeValue(joint.targetAngularVelocity.x,-targetVelocity,targetVelocity);
        }
        else if(axisToMove==MoveableAxis.y)
        {
            input[0]=normalizeValue(AdjustRotation(transform.localEulerAngles.y),startingRotation.y-joint.angularYLimit.limit,startingRotation.x+joint.angularYLimit.limit);
            input[1]=normalizeValue(joint.targetAngularVelocity.y,-targetVelocity,targetVelocity);

        }
        else
        {
            input[0]=normalizeValue(AdjustRotation(transform.localEulerAngles.z),startingRotation.z-joint.angularZLimit.limit,startingRotation.z+joint.angularZLimit.limit);
            input[1]=normalizeValue(joint.targetAngularVelocity.z,-targetVelocity,targetVelocity);
        }
        input[2]=moveTimeLeft;
        return input;
    }
    public static float translateToValue(float min, float max, float output)   //tlumacze output na wartosci
    {
        var y = (max + min) / 2.0f;
        var x = max - y;
        var value = output * x + y;
        return value;
    }
    //,int limitMin,int limitMax
    public void TranslateOutput(List<float> output)
    {
        var velocityVector=joint.targetAngularVelocity;
        moveTimeLeft = output[0];
        velocityVector[(int)axisToMove] = translateToValue(-targetVelocity, targetVelocity, output[1]);
        joint.targetAngularVelocity=velocityVector;
    }

    private void FixedUpdate() {
        // if(moveTimeLeft>0)
        // {
        //     moveTimeLeft-=Time.fixedDeltaTime;
        // }
        if(moveTimeLeft<=0)
        {        
            var velocityVector=joint.targetAngularVelocity;
            velocityVector[(int)axisToMove]=0;
            joint.targetAngularVelocity=velocityVector;
        }
    } 
}