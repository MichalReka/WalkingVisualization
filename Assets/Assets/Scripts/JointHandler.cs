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
    public static int outputSize=1;
    public static int inputSize=1;
    public MoveableAxis axisToMove;
    float startingBodyY;
    //jako input przyjmuje - pozycje, obrot, przyspieszenie, przyspieszenie katowe czesci ktora sie porusza
    //jako output motor, limit, velocity
    // Start is called before the first frame update
    Transform animalBody;
    Rigidbody rigidBody;
    // ConfigurableJoint bodyLimits;
    Quaternion startingRotation;
    Quaternion startingLocalRotation;
    public void initArm()
    {
        rigidBody = GetComponent<Rigidbody>();
        animalBody = transform.parent.gameObject.transform.Find("body");
        // bodyLimits=animalBody.GetComponent<ConfigurableJoint>();
        startingBodyY = animalBody.transform.position.y;
        joint = GetComponent<ConfigurableJoint>();
        input = new float[inputSize];
        startingRotation=transform.rotation;
        startingLocalRotation=transform.localRotation;
        setInput();
        
    }
    // //https://gist.github.com/mstevenson/4958837#file-configurablejointextensions-cs
    // ConfigurableJoint SetTargetRotationInternal (ConfigurableJoint joint, Quaternion targetRotation, Quaternion startRotation)
    //  {
    //      // Calculate the rotation expressed by the joint's axis and secondary axis
    //      var right = joint.axis;
    //      var forward = Vector3.Cross (joint.axis, joint.secondaryAxis).normalized;
    //      var up = Vector3.Cross (forward, right).normalized;
         
    //     Quaternion worldToJointSpace = Quaternion.LookRotation (forward, up);
         
    //      // Transform into world space
    //     Quaternion resultRotation = Quaternion.Inverse (worldToJointSpace);
         
    //      // Counter-rotate and apply the new local rotation.
    //      // Joint space is the inverse of world space, so we need to invert our value
    //     resultRotation *= Quaternion.Inverse (targetRotation) * startRotation; 
    //      // Transform back into joint space
    //     resultRotation *= worldToJointSpace;
         
    //      // Set target rotation to our newly calculated rotation
    //      joint.targetRotation = resultRotation;
    //      return joint;
    //  }
    //end of code snippet

    public static float normalizeValue(float value, float min, float max) //wartosci od -1 do 1 - normalizacja potrzebna dla zwiekszenia wydajnosci
    {
        float targetMin=-1;
        float targetMax=1;
        float translatedValue=(targetMax-targetMin)/(max-min)*(value-max)+targetMax;
        return translatedValue;   
    }
    public float[] setInput()
    {
        if(axisToMove==MoveableAxis.x)
        {
            input[0]=(normalizeValue(transform.rotation.x,startingRotation.x+joint.lowAngularXLimit.limit,startingRotation.x+joint.highAngularXLimit.limit));
        }
        else if(axisToMove==MoveableAxis.y)
        {
            input[0]=(normalizeValue(transform.rotation.y,startingRotation.y-joint.angularYLimit.limit,startingRotation.x+joint.angularYLimit.limit));
        }
        else
        {
            input[0]=(normalizeValue(transform.rotation.z,startingRotation.z-joint.angularZLimit.limit,startingRotation.z+joint.angularZLimit.limit));
        }
        // input.Add(normalizeValue(transform.rotation.z,startingRotation.z-20,startingRotation.z+20));    
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
        Quaternion targetRotation=joint.targetRotation;
        var velocityVector=joint.targetAngularVelocity;
        var targetRotationVector=joint.targetRotation;
        // float targetRotationValue;
        // var jointSlerpDrive = joint.slerpDrive;
        velocityVector[(int)axisToMove] = translateToValue(-targetVelocity, targetVelocity, output[0]);
        // if(axisToMove==MoveableAxis.x)
        // {
        //     targetRotationValue=translateToValue(joint.lowAngularXLimit.limit, joint.highAngularXLimit.limit, output[1]);
        // }
        // else if(axisToMove==MoveableAxis.y)
        // {
        //     targetRotationValue=translateToValue(-joint.angularYLimit.limit, joint.angularYLimit.limit, output[1]);
        // }
        // else
        // {
        //     targetRotationValue=translateToValue(-joint.angularZLimit.limit, joint.angularZLimit.limit, output[1]);
        // }
        // if(velocityVector[(int)axisToMove]<0)
        // {
        //     targetRotationValue=-Mathf.Abs(targetRotationValue);
        // }
        // else
        // {
        //     targetRotationValue=Mathf.Abs(targetRotationValue);
        // }
        // targetRotation[(int)axisToMove]=targetRotationValue;
        // joint=SetTargetRotationInternal(joint,targetRotation,startingLocalRotation);
        joint.targetAngularVelocity=velocityVector;
        // joint.slerpDrive=jointSlerpDrive;
    }
    // Update is called once per frame
 
}