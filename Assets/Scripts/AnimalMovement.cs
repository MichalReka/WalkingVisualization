using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
public class AnimalMovement : MonoBehaviour
{
    public AnimalBrain animalBrain;
    private float startingBodyY;
    private int framesPassed;
    public float averageBodyY;
    public JointHandler[] orderedMovingParts;
    public AnimalBody[] orderedBodyParts;

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
    AnimalBody[] bodyPartsData;
    List<int> limbsIndexes;
    List<int> similarBodyPartsIndexes;
    Dictionary<int,AnimalBody[]> limbsDictionary;
    Dictionary<int,AnimalBody[]> similarBodyPartsDictionary;
    public float maxPercentGenesToMutate;
    public void OrderAnimalChildren<T>(ref T[] orderedContainer)
    {
        T[] temp = GetComponentsInChildren<T>();
        orderedContainer = new T[temp.Length];
        int index = 0;
        int noOfChildren = transform.childCount;
        for (int i = 0; i < noOfChildren; i++)
        {
            T[] childComponent = transform.GetChild(i).GetComponents<T>();
            if (childComponent != null)
            {
                for (int j = 0; j < childComponent.Length; j++)
                {
                    orderedContainer[index] = childComponent[j];
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
        animalBrain = new AnimalBrain(maxPercentGenesToMutate);
        
        animalBrain.setRandomWeights();
    }

    public void CollisionDetected()
    {
        ifCatched = true;
        ifCrashed = true;
    }
    // Update is called once per frame
    public void StartJob()
    {
        animalBrain.SetOutput();
    }
    public void FinishJob()
    {
        animalBrain.FinishJob();
    }
    private void gatherInput(NativeArray<float> input)
    {
        var currIndex = 0;
        for (int i = 0; i < orderedMovingParts.Length; i++)
        {
            for (int j = 0; j < orderedMovingParts[i].input.Length; j++)
            {
                input[currIndex] = orderedMovingParts[i].input[j];
                currIndex++;
            }
        }
        var bodyY = JointHandler.normalizeValue(body.position.y, startingBodyY * 0.8f, startingBodyY * 1.2f);
        input[currIndex] = bodyY;
        currIndex++;
        // input[currIndex] = bodyRigibody.velocity.x;
        // currIndex++;
        // input[currIndex] = bodyRigibody.velocity.y;
        // currIndex++;
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
        // if (!animalBrain.ifFirstOutput)
        // {
        //     for (int i = 0; i < animalBrain.output.Length-1; i++)
        //     {
        //         input[currIndex] = animalBrain.output[i];
        //         currIndex++;
        //     }
        // }
        // else
        // {
        //     for (int i = 0; i < animalBrain.output.Length-1; i++)
        //     {
        //         input[currIndex] = 0;
        //         currIndex++;
        //     }
        //     animalBrain.ifFirstOutput = false;
        // }
    }
    public void UpdateIO()
    {
        for (int i = 0; i < orderedMovingParts.Length; i++)
        {
            orderedMovingParts[i].setInput();
        }
        gatherInput(animalBrain.input);
        
        int[] armsToMove = new int[AnimalBrain.armsToMove];
        int outputIndexesForArm = (AnimalBrain.outputSize - armsToMove.Length) / armsToMove.Length;
        for (int i = 0; i < armsToMove.Length; i++)
        {
            armsToMove[i] = (int)Mathf.Floor(JointHandler.translateToValue(0, orderedMovingParts.Length, animalBrain.output[i]));
            if(armsToMove[i]==orderedMovingParts.Length)
            {
                armsToMove[i]-=1;
            }
            List<float> partOutput = new List<float>();
            for (int j = armsToMove.Length + (i * outputIndexesForArm); j <= armsToMove.Length + (i * outputIndexesForArm) + outputIndexesForArm; j++)
            {
                partOutput.Add(animalBrain.output[j]);
            }
            orderedMovingParts[armsToMove[i]].TranslateOutput(partOutput);
        }

    }
    void SetIndexesLists()
    {
        foreach(AnimalBody bodyPart in orderedBodyParts)
        {
            if(!limbsIndexes.Contains(bodyPart.limbIndex))
            {
                limbsIndexes.Add(bodyPart.limbIndex);
            }
            if(!similarBodyPartsIndexes.Contains(bodyPart.partIndex))
            {
                similarBodyPartsIndexes.Add(bodyPart.partIndex);
            }
        }
    }
    void SetLimbsAndSimilarParts()
    {
        
    }
    public void setBody(bool isElite)
    {
        OrderAnimalChildren<JointHandler>(ref orderedMovingParts);
        body = transform.Find("body");
        if (isElite)
        {
            body.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        }
        OrderAnimalChildren<AnimalBody>(ref orderedBodyParts);
        SetIndexesLists();
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

        currentX += Time.fixedDeltaTime * speed;
        timeBeingAlive += Time.fixedDeltaTime * timeBeingAliveImportance;
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
    public void Destroy()
    {
        animalBrain.Dispose();
    }
}
