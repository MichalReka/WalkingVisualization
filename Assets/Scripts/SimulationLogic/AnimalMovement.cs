using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
public class AnimalMovement : MonoBehaviour
{
    public AnimalData animalData;
    public float[] bodyPartsStartingX;
    Transform[] children;
    Rigidbody[] rigidbodiesChildren;
    private float startingBodyY;
    public float averageBodyY;
    public JointHandler[] orderedMovingParts;
    public AnimalBodyPart[] orderedBodyParts;
    int _framesPassed = 0;
    public bool ifCatched = false;
    // public bool ifCrashed = false;
    public float currentX = 0;
    public float speed;
    public float timeBeingAlive = 0;
    public float averageVelocity = 0;
    Transform body;
    Rigidbody bodyRigibody;
    // Transform leftFoot;
    // Transform rightFoot;
    // float startingRightFootPosition;
    // float startingLeftFootPosition;
    Vector3 startingBodyRotation;
    List<int> limbsIndexes;
    List<int> similarBodyPartsIndexes;
    Dictionary<int, List<AnimalBodyPart>> limbsDictionary;
    Dictionary<int, List<AnimalBodyPart>> similarBodyPartsDictionary;
    List<int> armsToMove = new List<int>();
    NativeArray<int> outputIndexesToCompute;
    bool ifStartedToMove = false;
    void DisableJoints()
    {
        var jointTogglers = GetComponentsInChildren<JointToggler>();
        foreach (JointToggler jointToggler in jointTogglers)
        {
            jointToggler.enabled = false;
        }
    }
    void EnableJoints()
    {
        var jointTogglers = GetComponentsInChildren<JointToggler>();
        foreach (JointToggler jointToggler in jointTogglers)
        {
            jointToggler.enabled = true;
        }
    }
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
    public void SetAnimalData(AnimalData newAnimalData)
    {
        animalData = newAnimalData;
        // animalData.animalBrain = newAnimalData.animalBrain;
        // animalData.partsMass = newAnimalData.partsMass;
        // animalData.partsScaleMultiplier = newAnimalData.partsScaleMultiplier;
        // animalData.targetJointsVelocity = newAnimalData.targetJointsVelocity;
        // animalData.limbsPositionMultiplier = newAnimalData.limbsPositionMultiplier;
        for (int i = 0; i < similarBodyPartsIndexes.Count; i++)
        {
            foreach (AnimalBodyPart bodyPart in similarBodyPartsDictionary[i])
            {
                bodyPart.SetMass(newAnimalData.partsMass[i]);
                if (bodyPart.ifScalable)
                {
                    bodyPart.SetScale(newAnimalData.partsScaleMultiplier[i]);
                }
                if (bodyPart.isMoveable)
                {
                    bodyPart.SetMaximumVelocity(newAnimalData.targetJointsVelocity[i]);
                }
            }
        }
        DisableJoints();
        for (int i = 0; i < limbsIndexes.Count; i++)
        {
            foreach (AnimalBodyPart bodyPart in limbsDictionary[i])
            {
                if (bodyPart.ifPositionCanBeChanged)
                {
                    bodyPart.SetPosition(newAnimalData.limbsPositionMultiplier[i]);
                }
            }
        }
        EnableJoints();

    }
    public void SetRandomData()
    {
        animalData = new AnimalData();
        animalData.partsMass = new Dictionary<int, float>();
        animalData.partsScaleMultiplier = new Dictionary<int, System.Numerics.Vector3>();
        animalData.targetJointsVelocity = new Dictionary<int, int>();
        animalData.limbsPositionMultiplier = new Dictionary<int, System.Numerics.Vector3>();
        animalData.animalBrain = new AnimalBrain();
        animalData.animalBrain.setRandomWeights();
        animalData.weightsMutationRate=PopulationInputData.weightsMutationRate;
        animalData.physicalMutationRate=PopulationInputData.physicalMutationRate;
        float multiplierRangeMin = AnimalData.multiplierRangeMin;
        float multiplierRangeMax = AnimalData.multiplierRangeMax;
        RandomizePhysicalData(ref animalData);
        DisableJoints();
        for (int i = 0; i < limbsIndexes.Count; i++)
        {
            System.Numerics.Vector3 randomMultiplier = new System.Numerics.Vector3(Random.Range(multiplierRangeMin, multiplierRangeMax), 1, Random.Range(multiplierRangeMin, multiplierRangeMax));
            foreach (AnimalBodyPart bodyPart in limbsDictionary[i])
            {
                if (bodyPart.ifPositionCanBeChanged)
                {
                    bodyPart.SetPosition(randomMultiplier);
                }
            }
            animalData.limbsPositionMultiplier[i] = randomMultiplier;
        }
        EnableJoints();
    }
    public void RandomizePhysicalData(ref AnimalData animalData)
    {
        for (int i = 0; i < similarBodyPartsIndexes.Count; i++)
        {
            float mass = Random.Range(AnimalData.massMin, AnimalData.massMax);
            float xScale = Random.Range(AnimalData.multiplierRangeMin, AnimalData.multiplierRangeMax);
            float zScale = Random.Range(AnimalData.multiplierRangeMin, AnimalData.multiplierRangeMax);
            int maximumVelocity = Random.Range(AnimalData.velocityMin, AnimalData.velocityMax);
            foreach (AnimalBodyPart bodyPart in similarBodyPartsDictionary[i])
            {
                bodyPart.SetMass(mass);
                if (bodyPart.ifScalable)
                {
                    bodyPart.SetScale(new System.Numerics.Vector3(xScale, 1, zScale));
                }
                if (bodyPart.isMoveable)
                {
                    bodyPart.SetMaximumVelocity(maximumVelocity);
                }
            }
            animalData.partsMass[i] = mass;
            animalData.partsScaleMultiplier[i] = new System.Numerics.Vector3(xScale, 1, zScale);
            animalData.targetJointsVelocity[i] = maximumVelocity;
        }

    }
    public void CollisionDetected()
    {
        ifCatched = true;
        // ifCrashed = true;
    }
    public void SelectLimbsToChangeState()
    {
        outputIndexesToCompute = new NativeArray<int>(AnimalBrain.armsToMoveCount, Allocator.TempJob);
        for (int i = 0; i < outputIndexesToCompute.Length; i++)
        {
            outputIndexesToCompute[i] = i;
        }
        animalData.animalBrain.SetOutput(outputIndexesToCompute);
    }
    void TranslateLimbsToMove()
    {
        int newArmIndex;
        armsToMove.Clear();
        for (int i = 0; i < AnimalBrain.armsToMoveCount; i++)
        {
            newArmIndex = (int)Mathf.Floor(JointHandler.translateToValue(0, orderedMovingParts.Length, animalData.animalBrain.output[i]));
            if (newArmIndex == orderedMovingParts.Length)
            {
                newArmIndex -= 1;
            }
            if (!armsToMove.Contains(newArmIndex))
            {
                armsToMove.Add(newArmIndex);
            }
        }
    }
    public void MoveSelectedLimbs()
    {
        if (!ifStartedToMove)
        {
            ifStartedToMove = true;
        }
        TranslateLimbsToMove();
        outputIndexesToCompute = new NativeArray<int>(armsToMove.Count * AnimalBrain.outputPerArm, Allocator.TempJob);
        int arrIndex = 0;
        for (int i = 0; i < armsToMove.Count; i++)
        {
            int startingIndex = AnimalBrain.armsToMoveCount + armsToMove[i] * AnimalBrain.outputPerArm;
            int endingIndex = startingIndex + AnimalBrain.outputPerArm;
            for (int j = startingIndex; j < endingIndex; j++)
            {
                outputIndexesToCompute[arrIndex] = j;
                arrIndex++;
            }
        }
        animalData.animalBrain.SetOutput(outputIndexesToCompute);
    }
    public void ResetAnimal(float startingX)
    {
        for (int i = 0; i < orderedBodyParts.Length; i++)
        {
            orderedBodyParts[i].ResetBodyPart();
        }
        currentX = startingX;
        timeBeingAlive = 0;
        ifCatched = false;
        ifStartedToMove = false;
    }
    public void FinishJob()
    {
        animalData.animalBrain.FinishJob();
        outputIndexesToCompute.Dispose();
    }
    private void GatherInput(NativeArray<float> input)
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
        int angleDeviation = 15;
        input[currIndex] = JointHandler.normalizeValue(JointHandler.AdjustRotation(body.localEulerAngles.x), startingBodyRotation.x - angleDeviation, startingBodyRotation.x + angleDeviation);
        currIndex++;
        input[currIndex] = JointHandler.normalizeValue(JointHandler.AdjustRotation(body.localEulerAngles.y), startingBodyRotation.y - angleDeviation, startingBodyRotation.y + angleDeviation);
        currIndex++;
        input[currIndex] = JointHandler.normalizeValue(JointHandler.AdjustRotation(body.localEulerAngles.z), startingBodyRotation.z - angleDeviation, startingBodyRotation.z + angleDeviation);
        currIndex++;
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
        GatherInput(animalData.animalBrain.input);
        for (int i = 0; i < armsToMove.Count; i++)
        {
            int startingIndex = AnimalBrain.armsToMoveCount + armsToMove[i] * AnimalBrain.outputPerArm;
            int endingIndex = startingIndex + AnimalBrain.outputPerArm;
            List<float> partOutput = new List<float>();
            for (int j = startingIndex; j < endingIndex; j++)
            {
                partOutput.Add(animalData.animalBrain.output[j]);
            }
            orderedMovingParts[armsToMove[i]].TranslateOutput(partOutput);
        }

    }
    void SetIndexesLists()
    {
        limbsIndexes = new List<int>();
        similarBodyPartsIndexes = new List<int>();
        foreach (AnimalBodyPart bodyPart in orderedBodyParts)
        {
            if (!limbsIndexes.Contains(bodyPart.limbIndex))
            {
                limbsIndexes.Add(bodyPart.limbIndex);
            }
            if (!similarBodyPartsIndexes.Contains(bodyPart.partIndex))
            {
                similarBodyPartsIndexes.Add(bodyPart.partIndex);
            }
        }
    }
    void SetDictionaries()
    {
        limbsDictionary = new Dictionary<int, List<AnimalBodyPart>>();
        similarBodyPartsDictionary = new Dictionary<int, List<AnimalBodyPart>>();
        List<AnimalBodyPart> tempList = new List<AnimalBodyPart>(orderedBodyParts);
        for (int i = 0; i < limbsIndexes.Count; i++)
        {
            List<AnimalBodyPart> partsWithIndex = new List<AnimalBodyPart>();
            for (int j = 0; j < tempList.Count; j++)
            {
                if (limbsIndexes[i] == tempList[j].limbIndex)
                {
                    partsWithIndex.Add(tempList[j]);
                    tempList.RemoveAt(j);
                    j--;
                }
            }
            limbsDictionary.Add(i, partsWithIndex);
        }
        tempList = new List<AnimalBodyPart>(orderedBodyParts);
        for (int i = 0; i < similarBodyPartsIndexes.Count; i++)
        {
            List<AnimalBodyPart> partsWithIndex = new List<AnimalBodyPart>();
            for (int j = 0; j < tempList.Count; j++)
            {
                if (similarBodyPartsIndexes[i] == tempList[j].partIndex)
                {
                    partsWithIndex.Add(tempList[j]);
                    tempList.RemoveAt(j);
                    j--;
                }
            }
            similarBodyPartsDictionary.Add(i, partsWithIndex);
        }
    }
    public void SetBody(bool isElite, bool isMigrated)
    {
        OrderAnimalChildren<JointHandler>(ref orderedMovingParts);
        body = transform.Find("body");
        if (isElite)
        {
            body.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        }
        if (isMigrated)
        {
            body.GetComponent<Renderer>().material.SetColor("_Color", Color.yellow);
        }
        OrderAnimalChildren<AnimalBodyPart>(ref orderedBodyParts);
        SetIndexesLists();
        SetDictionaries();
        startingBodyY = body.transform.position.y;
        startingBodyRotation = body.transform.localEulerAngles;
        bodyRigibody = body.GetComponent<Rigidbody>();
        averageBodyY = 0;
        for (int i = 0; i < orderedMovingParts.Length; i++)
        {
            orderedMovingParts[i].initArm();
        }
        OrderAnimalChildren<Transform>(ref children);
        OrderAnimalChildren<Rigidbody>(ref rigidbodiesChildren);
    }
    public void SetBodyPartsStartingX()
    {
        bodyPartsStartingX = new float[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            bodyPartsStartingX[i] = transform.GetChild(i).transform.localPosition.x;
        }
    }
    float GetSmallestX()
    {
        float smallestX = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            float x = children[i].localPosition.x - bodyPartsStartingX[i];
            if (x < smallestX)
            {
                smallestX = x;
            }
        }
        return smallestX;
    }
    void CalculateVelocity()
    {
        float averageFrameVelocity = 0;
        foreach (Rigidbody child in rigidbodiesChildren)
        {
            averageFrameVelocity += child.velocity.x;
        }
        averageFrameVelocity = averageFrameVelocity / rigidbodiesChildren.Length;
        averageVelocity += averageFrameVelocity;
    }
    void FixedUpdate()
    {
        // if (body.transform.position.y > startingBodyY)
        // {
        //     averageBodyY += startingBodyY;
        // }
        // else
        // {
        //     averageBodyY += body.transform.position.y;
        // }
        // framesPassed++;
        // var bodyX = body.transform.position.x;
        // var leftFootX = leftFoot.transform.position.x - startingLeftFootPosition;   // inaczej stopy maja za duzy udzial - zwierzeta je wyrzucaja do przodu i sie blokuja
        // var rightFootX = rightFoot.transform.position.x - startingRightFootPosition;
        if (ifStartedToMove)
        {
            _framesPassed++;
            currentX += Time.fixedDeltaTime * speed;
            timeBeingAlive = timeBeingAlive + Time.fixedDeltaTime;
            float smallestX = GetSmallestX();
            CalculateVelocity();
            if (currentX > smallestX)   //jak zostanie zlapane
            {
                ifCatched = true;
            }
            else if (body.transform.position.y > startingBodyY * 2)   //jak poleci w nieznane
            {
                ifCatched = true;
            }
            else if (body.transform.position.y < 0)   //jak spadnie w nieznane
            {
                ifCatched = true;
            }
            else if (body.transform.rotation.z < -120)  //jesli zachce robic fikolki sobie
            {
                ifCatched = true;
            }
            if (ifCatched)
            {
                averageVelocity = averageVelocity / _framesPassed;
                // averageBodyY = averageBodyY / timeBeingAlive;
            }
        }

    }
    public void Destroy()
    {
        animalData.animalBrain.Dispose();
    }
}
