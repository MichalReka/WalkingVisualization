using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public class AnimalMovement : MonoBehaviour
{
    public AnimalBrain animalBrain;
    private float startingBodyY;
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
    public void setNeuralNetwork(AnimalBrain neuralNetwork)
    {
        animalBrain=neuralNetwork;
    }
    public void setRandomWeights()
    {
        animalBrain=new AnimalBrain(orderedHingeParts.Length);
        animalBrain.setRandomWeights();
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
            animalBrain.setOutput();
        }
    }
    private float[] gatherInput()
    {
        float[] input = new float[animalBrain.noMovingParts*HingeArmPart.inputSize+animalBrain.bodyInput];
        var currIndex=0;
        Parallel.For(0, orderedHingeParts.Length, delegate (int i)
        {
            for(int j=0;j<orderedHingeParts[i].input.Count;j++)
            {
                input[currIndex]=orderedHingeParts[i].input[j];
                currIndex++;
            }
        });

        // for(int i=0;i<orderedHingeParts.Length;i++)
        // {
        //     for(int j=0;j<orderedHingeParts[i].input.Count;j++)
        //     {
        //         input[currIndex]=orderedHingeParts[i].input[j];
        //         currIndex++;
        //     }
        // }
        var bodyY = HingeArmPart.normalizeValue(body.position.y,startingBodyY*0.7f,startingBodyY*1.3f);
        input[currIndex]=bodyY;
        currIndex++;
        for (int i = 0; i < 3; i++)
        {
            input[currIndex]=HingeArmPart.normalizeValue(body.rotation[i],-360,360);
            currIndex++;
        }
        return input;
    }
    public void UpdateIO()
    {
        for (int i = 0; i < orderedHingeParts.Length; i++)
        {
            //orderedHingeParts[i].TranslateOutput();
            orderedHingeParts[i].setInput();
        }
        gatherInput();
        int currOutputIndex=0;
        for (int i = 0; i < orderedHingeParts.Length; i++)
        {
            
            List<float> partOutput=new List<float>();
            for(int j=0;j<HingeArmPart.outputSize;j++)
            {
                partOutput.Add(animalBrain.output[currOutputIndex]);
                currOutputIndex++;
            }
            orderedHingeParts[i].TranslateOutput(partOutput);
        }
    }
    public void setBody()
    {
        OrderAnimalChildren();
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
