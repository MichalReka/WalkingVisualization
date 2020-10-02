using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public class AnimalMovement : MonoBehaviour
{
    public AnimalBrain animalBrain;
    private float startingBodyY;
    private int framesPassed;
    public float averageBodyY;
    public JointHandler[] orderedMovingParts;
    public bool ifCatched = false;
    public bool ifCrashed = false;
    public float currentX = 0;
    public float speed;
    public float timeBeingAlive = 0;
    public float timeBeingAliveImportance = 0.1f;
    Transform body;
    Rigidbody bodyRigibody;
    Transform leftFoot;
    Transform rightFoot;
    float startingRightFootPosition;
    float startingLeftFootPosition;
    Quaternion startingBodyRotation;
    ConfigurableJoint bodyLimits;

    public void OrderAnimalChildren()
    {
        JointHandler[] temp = GetComponentsInChildren<JointHandler>();
        orderedMovingParts = new JointHandler[temp.Length];
        int index = 0;
        int noOfChildren = transform.childCount;
        for (int i = 0; i < noOfChildren; i++)
        {
            JointHandler[] childComponent = transform.GetChild(i).GetComponents<JointHandler>();
            if (childComponent != null)
            {
                for (int j = 0; j < childComponent.Length; j++)
                {
                    orderedMovingParts[index] = childComponent[j];
                    index++;
                }
            }
        }
    }
    public void setNeuralNetwork(AnimalBrain neuralNetwork)
    {
        animalBrain = neuralNetwork;
    }
    public void setRandomWeights()
    {
        animalBrain = new AnimalBrain();
        animalBrain.setRandomWeights();
    }
    void Start()
    {

    }
    public void CollisionDetected()
    {
        ifCatched = true;
        ifCrashed = true;
    }
    // Update is called once per frame
    public void Move()
    {
        for (int i = 0; i < orderedMovingParts.Length; i++)
        {
            animalBrain.setOutput();
        }
    }
    private float[] gatherInput()
    {
        float[] input = new float[AnimalBrain.noMovingParts * JointHandler.inputSize + AnimalBrain.bodyInput + animalBrain.output.Length];
        var currIndex = 0;
        for (int i = 0; i < orderedMovingParts.Length; i++)
        {
            for (int j = 0; j < orderedMovingParts[i].input.Length; j++)
            {
                input[currIndex] = orderedMovingParts[i].input[j];
                currIndex++;
            }
        }
        var bodyY = JointHandler.normalizeValue(body.position.y, startingBodyY * 0.7f, startingBodyY * 1.3f);
        input[currIndex] = bodyY;
        currIndex++;
        //rotacje
        // input[currIndex] = JointHandler.normalizeValue(body.rotation.x, startingBodyRotation.x + bodyLimits.lowAngularXLimit.limit, startingBodyRotation.x + bodyLimits.highAngularXLimit.limit);
        // currIndex++;
        // input[currIndex] = JointHandler.normalizeValue(body.rotation.y, startingBodyRotation.y - bodyLimits.angularYLimit.limit, startingBodyRotation.y + bodyLimits.angularYLimit.limit);
        // currIndex++;
        // input[currIndex] = JointHandler.normalizeValue(body.rotation.z, startingBodyRotation.z - bodyLimits.angularZLimit.limit, startingBodyRotation.z + bodyLimits.angularZLimit.limit);
        // currIndex++;
        // int rotationBorder = 20; //im mniejsze tym czulsze
        // input[currIndex] = JointHandler.normalizeValue(body.rotation.x, startingBodyRotation.x - rotationBorder, startingBodyRotation.x + rotationBorder);
        // currIndex++;
        // input[currIndex] = JointHandler.normalizeValue(body.rotation.y, startingBodyRotation.y - rotationBorder, startingBodyRotation.y + rotationBorder);
        // currIndex++;
        // input[currIndex] = JointHandler.normalizeValue(body.rotation.z, startingBodyRotation.z - rotationBorder, startingBodyRotation.z + rotationBorder);
        // currIndex++;
        //predkosc
        input[currIndex] = bodyRigibody.velocity.x;
        currIndex++;
        input[currIndex] = bodyRigibody.velocity.y;
        currIndex++;
        if (!animalBrain.ifFirstOutput)
        {
            for (int i = 0; i < animalBrain.output.Length; i++)
            {
                input[currIndex] = animalBrain.output[i];
                currIndex++;
            }
        }
        else
        {
            for (int i = 0; i < animalBrain.output.Length; i++)
            {
                input[currIndex] = 0;
                currIndex++;
            }
            animalBrain.ifFirstOutput = false;
        }
        return input;
    }
    public void UpdateIO()
    {
        for (int i = 0; i < orderedMovingParts.Length; i++)
        {
            orderedMovingParts[i].setInput();
        }
        animalBrain.input = gatherInput();
        float max = AnimalBrain.noMovingParts - 1;
        float y = (0 + max) / 2.0f;   //czesc kodu z translate value
        float x = max - y;
        float value = animalBrain.output[0] * x + y;
        int armToMove = (int)Mathf.Round(value);
        List<float> partOutput = new List<float>();
        for (int j = 1; j <= JointHandler.outputSize; j++)
        {
            partOutput.Add(animalBrain.output[j]);
        }
        orderedMovingParts[armToMove].TranslateOutput(partOutput);
    }
    public void setBody(bool isElite)
    {
        OrderAnimalChildren();
        body = transform.Find("body");
        if (isElite)
        {
            body.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        }
        rightFoot = transform.Find("RightBackFoot");
        leftFoot = transform.Find("LeftBackFoot");
        startingBodyY = body.transform.position.y;
        startingBodyRotation = body.transform.rotation;
        startingLeftFootPosition = leftFoot.transform.position.x;
        startingRightFootPosition = rightFoot.transform.position.x;
        bodyRigibody = body.GetComponent<Rigidbody>();
        // bodyLimits = body.GetComponent<ConfigurableJoint>();
        averageBodyY = 0;
        for (int i = 0; i < orderedMovingParts.Length; i++)
        {
            orderedMovingParts[i].initArm();
        }
    }
    public void chase()
    {

        currentX += Time.deltaTime * speed;
        timeBeingAlive += Time.deltaTime * timeBeingAliveImportance;
        averageBodyY += body.transform.position.y;
        framesPassed++;
        var bodyX = body.transform.position.x;
        var leftFootX = leftFoot.transform.position.x - startingLeftFootPosition;   // inaczej stopy maja za duzy udzial - zwierzeta je wyrzucaja do przodu i sie blokuja
        var rightFootX = rightFoot.transform.position.x - startingRightFootPosition;
        if (currentX > bodyX || currentX > rightFootX || currentX > leftFootX)   //jak zostanie zlapane
        {
            ifCatched = true;
        }
        else if (body.transform.position.y > startingBodyY * 2)   //jak poleci w nieznane
        {
            body.transform.position = new Vector3(-999, -999, -999);
            ifCatched = true;
        }
        else if (body.transform.position.y < 0)   //jak spadnie w nieznane
        {
            body.transform.position = new Vector3(-999, -999, -999);
            ifCatched = true;
        }
        // else if(body.transform.rotation.z<-120)  //jesli zachce robic fikolki sobie
        // {
        //     ifCatched = true;
        // }
        if (ifCatched)
        {
            averageBodyY = averageBodyY / framesPassed;
        }
    }

}
