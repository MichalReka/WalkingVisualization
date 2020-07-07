using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HingeArmPart : MonoBehaviour
{
    public float bodyY;
    public float motorForce;
    public float targetVelocity;
    HingeJoint hinge;
    //prosta implementacja sieci neuronowej
    public List<List<float>> inputSynapsesWeights;
    public List<List<float>> outputSynapsesWeights;
    public List<float> hiddenLayerBias;
    static int outputSize=4;
    public float[] output = new float[4]; //velocity,motor,min.limit,max.limit
    public List<float> input;
    public int hiddenLayerSize;
    float startingBodyY;
    Quaternion startingRotation;
    Quaternion startingBodyRotation;
    //jako input przyjmuje - pozycje, obrot, przyspieszenie, przyspieszenie katowe czesci ktora sie porusza
    //jako output motor, limit, velocity
    // Start is called before the first frame update
    Transform animalBody;
    Rigidbody rigidBody;
    Quaternion bodyRotation;
    public bool ifSetoToRandom = false;

    public void initArm()
    {
        rigidBody = GetComponent<Rigidbody>();
        animalBody = transform.parent.gameObject.transform.Find("body");
        startingBodyY = animalBody.transform.position.y;
        startingRotation = transform.rotation;
        startingBodyRotation = animalBody.transform.rotation;
        hinge = GetComponent<HingeJoint>();
        input = new List<float>();
        setInput();
        hiddenLayerSize = input.Count * 2 / 3 + outputSize;
        if (ifSetoToRandom)
        {
            setRandomWeights();
        }
    }
    void Start()
    {

    }
    public void deepCopy(HingeArmPart source)
    {
        for (int i = 0; i < inputSynapsesWeights.Count; i++)
        {
            for (int j = 0; j < inputSynapsesWeights[i].Count; j++)
            {

                inputSynapsesWeights[i][j] = source.inputSynapsesWeights[i][j];

            }
        }
        for (int i = 0; i < outputSynapsesWeights.Count; i++)
        {
            for (int j = 0; j < outputSynapsesWeights[i].Count; j++)
            {

                outputSynapsesWeights[i][j] = source.outputSynapsesWeights[i][j];

            }
        }
        for (int i = 0; i < hiddenLayerBias.Count; i++)
        {

            hiddenLayerBias[i] = source.hiddenLayerBias[i];

        }
    }
    float translateToValue(float min, float max, float output)   //tlumacze output na wartosci
    {
        var y = (max + min) / 2.0f;
        var x = max - y;
        var value = output * x + y;
        return value;
    }
    static float sigmoid(float value)
    {
        float translatedValue = (Mathf.Exp(value) / (Mathf.Exp(value) + 1)) * 2 - 1;  // wartosci od -1 do 1
        return translatedValue;
    }
    public void setInput()
    {
        input.Clear();
        for (int i = 0; i < 3; i++)
        {
            input.Add((transform.rotation[i] - startingRotation[i]) * Mathf.Deg2Rad);
        }
        for (int i = 0; i < 3; i++)
        {
            input.Add(rigidBody.velocity[i]);
        }
        for (int i = 0; i < 3; i++)
        {
            input.Add(rigidBody.angularVelocity[i]);
        }
        bodyY = animalBody.transform.position.y;
        input.Add(bodyY);
        for (int i = 0; i < 3; i++)
        {
            input.Add((startingBodyRotation[i] - animalBody.transform.rotation[i]) * Mathf.Deg2Rad);
        }
    }

    public void setRandomWeights()
    {
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
    }
    bool roll(float chance)
    {
        float localchance = Random.Range(0.0f, 100.0f);
        if (chance > localchance)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void mixWeights(HingeArmPart parent, float chance)    //jaka jest szansa na zmiane genow
    {
        for (int i = 0; i < hiddenLayerSize; i++)
        {
            if (roll(chance))
            {
                hiddenLayerBias[i] = parent.hiddenLayerBias[i];
            }
            for (int j = 0; j < input.Count; j++)
            {
                if (roll(chance))
                {
                    inputSynapsesWeights[i][j] = parent.inputSynapsesWeights[i][j];
                }
            }
            for (int j = 0; j < outputSize; j++)
            {
                if (roll(chance))
                {
                    outputSynapsesWeights[i][j] = parent.outputSynapsesWeights[i][j];
                }
            }
        }
    }
    public void mutateWeights()    //jaka jest szansa na zmiane genow
    {
        float mutationChance = Random.Range(0.0f, 100.0f);
        for (int i = 0; i < hiddenLayerSize; i++)
        {
            mutationChance = Random.Range(0.0f, 100.0f);
            if (roll(mutationChance))
            {
                hiddenLayerBias[i] = Random.Range(-1.0f, 1.0f);
            }
            for (int j = 0; j < input.Count; j++)
            {
                mutationChance = Random.Range(0.0f, 100.0f);
                if (roll(mutationChance))
                {
                    inputSynapsesWeights[i][j] = Random.Range(-1.0f, 1.0f);
                }
            }
            for (int j = 0; j < outputSize; j++)
            {
                mutationChance = Random.Range(0.0f, 100.0f);
                if (roll(mutationChance))
                {
                    outputSynapsesWeights[i][j] = Random.Range(-1.0f, 1.0f);
                }
            }
        }
    }
    public void setOutput()
    {
        float tempOutputValue = 0;
        float neuronsCalculatedWeight;
        for (int outputIndex = 0; outputIndex < outputSize; outputIndex++)
        {
            for (int neuronIndex = 0; neuronIndex < hiddenLayerSize; neuronIndex++)
            {
                neuronsCalculatedWeight = 0;
                for (int synapseIndex = 0; synapseIndex < inputSynapsesWeights[neuronIndex].Count; synapseIndex++)
                {
                    neuronsCalculatedWeight += sigmoid(input[synapseIndex]) * inputSynapsesWeights[neuronIndex][synapseIndex];
                }
                neuronsCalculatedWeight = sigmoid(neuronsCalculatedWeight);
                neuronsCalculatedWeight += hiddenLayerBias[neuronIndex];
                tempOutputValue = tempOutputValue + neuronsCalculatedWeight * outputSynapsesWeights[neuronIndex][outputIndex];
            }
            tempOutputValue = sigmoid(tempOutputValue);
            output[outputIndex] = tempOutputValue;
        }
    }
    public void TranslateOutput()
    {
        if (output[3] > output[2])
        {
            var temp = output[3];
            output[3] = output[2];
            output[2] = temp;
        }
        var motor = hinge.motor;
        motor.force = translateToValue(0, 600, output[0]);
        motorForce = motor.force;
        motor.targetVelocity = translateToValue(-3000, 3000, output[1]);
        targetVelocity = motor.targetVelocity;
        JointLimits limits = hinge.limits;
        limits.max = translateToValue(-101, 101, output[2]);
        limits.min = translateToValue(-101, 101, output[3]);
        hinge.useMotor = true;
        limits.bounciness = 0;
        limits.bounceMinVelocity = 0;
        hinge.useLimits = true;
        hinge.motor = motor;
        hinge.limits = limits;
    }
    // Update is called once per frame
    void Update()
    {

    }
}