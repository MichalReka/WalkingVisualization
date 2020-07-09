using System.Collections;
using System.Collections.Generic;
using UnityEngine;
struct AnimalBrain{
    const int valuesPerMovingPart=4;
    public List<List<float>> inputSynapsesWeights;
    public List<List<float>> outputSynapsesWeights;
    public List<float> hiddenLayerBias;
    public List<float> output;  //dzielnik czworki
}
public class AnimalMovement : MonoBehaviour
{
    private float startingBodyY;
    private HingeArmPart[] injectedHingeParts;
    public HingeArmPart[] orderedHingeParts;
    public bool ifCatched = false;
    public float currentX;
    public float speed;
    Transform body;
    public void OrderAnimalChildren()
    {
        HingeArmPart[] temp = GetComponentsInChildren<HingeArmPart>();
        orderedHingeParts = new HingeArmPart[temp.Length];
        int index = 0;
        int noOfChildren = transform.childCount;
        for (int i = 0; i < noOfChildren; i++)
        {
            HingeArmPart childComponent = transform.GetChild(i).GetComponent<HingeArmPart>();
            if (childComponent != null)
            {
                orderedHingeParts[index] = childComponent;
                index++;
            }

        }
    }
    public void setHingeParts(List<HingeArmPart> hingeParts)
    {
        this.injectedHingeParts = hingeParts.ToArray();
        OrderAnimalChildren();
        var i = 0;
        foreach (HingeArmPart part in orderedHingeParts)
        {
            part.inputSynapsesWeights = injectedHingeParts[i].inputSynapsesWeights;
            part.outputSynapsesWeights = injectedHingeParts[i].outputSynapsesWeights;
            part.hiddenLayerBias = injectedHingeParts[i].hiddenLayerBias;
            i++;
        }
    }
    public void setRandomWeights()
    {
        OrderAnimalChildren();
        foreach (HingeArmPart part in orderedHingeParts)
        {
            part.ifSetoToRandom = true;
        }
    }
    void Start()
    {
        
    }
    public void CollisionDetected()
    {
        var joints = GetComponentsInChildren<HingeJoint>();
        foreach (var hJoint in joints)
        {
            var motor = hJoint.motor;
            motor.force = 0;
            motor.targetVelocity = 0;
            hJoint.motor = motor;
            hJoint.useMotor = false;
        }
        ifCatched = true;
    }
    // Update is called once per frame
    public void Move()
    {
        for (int i = 0; i < orderedHingeParts.Length; i++)
        {
            orderedHingeParts[i].setOutput();
        }
    }
    public void UpdateIO()
    {
        for (int i = 0; i < orderedHingeParts.Length; i++)
        {
            orderedHingeParts[i].TranslateOutput();
            orderedHingeParts[i].setInput();
        }
    }
    public void setBody()
    {
        body = transform.Find("body");
        startingBodyY = body.transform.position.y;
        for (int i = 0; i < orderedHingeParts.Length; i++)
        {
            orderedHingeParts[i].initArm();
        }
    }
    public void chase()
    {
        currentX += Time.deltaTime * speed;
        if (currentX > body.transform.position.x)
        {
            ifCatched = true;
        }
        else if (body.transform.position.y > startingBodyY * 1.5)   //jak poleci w nieznane
        {
            body.transform.position = new Vector3(-999, -999, -999);
            ifCatched = true;
        }
        else if (body.transform.position.y<0)   //jak spadnie w nieznane
        {
            body.transform.position = new Vector3(-999, -999, -999);
            ifCatched = true;
        }
    }
    void Update()
    {

    }
}
