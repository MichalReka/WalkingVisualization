using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

public class HingeArmPart : MonoBehaviour
{
    public int maximumMotorForce;
    public int maximumVelocityLimit;
    HingeJoint hinge;
    //prosta implementacja sieci neuronowej
    public List<float> input;
    public static int outputSize=2;
    public static int inputSize=2;

    float startingBodyY;
    //jako input przyjmuje - pozycje, obrot, przyspieszenie, przyspieszenie katowe czesci ktora sie porusza
    //jako output motor, limit, velocity
    // Start is called before the first frame update
    Transform animalBody;
    Rigidbody rigidBody;

    public void initArm()
    {
        rigidBody = GetComponent<Rigidbody>();
        animalBody = transform.parent.gameObject.transform.Find("body");
        startingBodyY = animalBody.transform.position.y;
        hinge = GetComponent<HingeJoint>();
        input = new List<float>();
        setInput();
        
    }
    void Start()
    {

    }
    public static float normalizeValue(float value, float min, float max) //wartosci od -1 do 1 - normalizacja potrzebna dla zwiekszenia wydajnosci
    {
        float targetMin=-1;
        float targetMax=1;
        float translatedValue=(targetMax-targetMin)/(max-min)*(value-max)+targetMax;
        return translatedValue;
    }
    public List<float> setInput()
    {
        input.Clear();
        input.Add(normalizeValue(transform.rotation.x,-360,360));
        input.Add(normalizeValue(transform.rotation.z,-360,360));
        // for (int i = 0; i < 3; i++)
        // {
        //     input.Add(normalizeValue(transform.rotation[i],-360,360));  //nie powinna nastapic wieksza rotacja
        // }
        // for (int i = 0; i < 3; i++)
        // {
        //     input.Add(rigidBody.velocity[i]);
        // }
        return input;
    }
    float translateToValue(float min, float max, float output)   //tlumacze output na wartosci
    {
        var y = (max + min) / 2.0f;
        var x = max - y;
        var value = output * x + y;
        return value;
    }
    //,int limitMin,int limitMax
    public void TranslateOutput(List<float> output)
    {
        // if (output[3] > output[2])
        // {
        //     var temp = output[3];
        //     output[3] = output[2];
        //     output[2] = temp;
        // }
        var motor = hinge.motor;
        motor.force = translateToValue(0, maximumMotorForce, output[0]);
        motor.targetVelocity = translateToValue(-maximumVelocityLimit, maximumVelocityLimit, output[1]);
        JointLimits limits = hinge.limits;
        // float limit=translateToValue(-60,60, output[2]);
        // if(limit<0)
        // {
        //     limits.max=0;
        //     limits.min=limit;
        // }
        // else
        // {
        //     limits.max=limit;
        //     limits.min=0;
        // }
        // limits.max=limitMax;
        // limits.min=limitMin;
        hinge.useLimits = true;
        hinge.useMotor = true;
        hinge.motor = motor;
        hinge.limits = limits;
    }
    // Update is called once per frame
    void Update()
    {

    }
}