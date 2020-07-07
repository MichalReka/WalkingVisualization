using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public struct AnimalBrain
{
    public List<List<float>> inputSynapsesWeights;
    public List<List<float>> outputSynapsesWeights;
    public List<float> hiddenLayerBias;
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
    AnimalBrain animalBrain;
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
        hiddenLayerSize = input.Count * 2 / 3 + outputSize;
        inputSynapsesWeights = new List<List<float>>();
        outputSynapsesWeights = new List<List<float>>();
        hiddenLayerBias = new List<float>(hiddenLayerSize);
        for (int i = 0; i < hiddenLayerSize; i++)
        {
            hiddenLayerBias.Add(System.Convert.ToSingle(GeneratePopulation.rnd.NextDouble() * 2.0 - 1.0));
            List<float> neuronInputWeights = new List<float>();
            List<float> neuronOutputWeights = new List<float>();
            for (int j = 0; j < input.Count; j++)
            {
                neuronInputWeights.Add(System.Convert.ToSingle(GeneratePopulation.rnd.NextDouble() * 2.0 - 1.0));
            }
            for (int j = 0; j < outputSize; j++)
            {
                neuronOutputWeights.Add(System.Convert.ToSingle(GeneratePopulation.rnd.NextDouble() * 2.0 - 1.0));
            }
            inputSynapsesWeights.Add(neuronInputWeights);
            outputSynapsesWeights.Add(neuronOutputWeights);
        }
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
    public void UpdateInput()
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
        else if (body.transform.position.y < 0)   //jak spadnie w nieznane
        {
            body.transform.position = new Vector3(-999, -999, -999);
            ifCatched = true;
        }
    }
    void Update()
    {

    }
}
