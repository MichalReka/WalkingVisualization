using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class AnimalBrain 
{
    private int outputPart=0;
    public int bodyInput=4;
    public int noMovingParts;
    public int hiddenLayerSize;
    const int valuesPerMovingPart=4;
    public List<List<float>> inputSynapsesWeights;
    public List<List<float>> outputSynapsesWeights;
    public List<float> hiddenLayerBias;
    public float[] output;  //dzielnik czworki
    public float[] input;
    // public AnimalBrain()
    // {
    //     hiddenLayerSize = input.Count * 2 / 3 + outputSize;
    //     if (ifSetoToRandom)
    //     {
    //         setRandomWeights();
    //     }
    // }
    public AnimalBrain(int noMovingParts)
    {
        
        this.noMovingParts=noMovingParts;
        this.hiddenLayerSize=((noMovingParts*HingeArmPart.inputSize)+bodyInput)*2/3+noMovingParts*HingeArmPart.outputSize;
        output=new float[noMovingParts*HingeArmPart.outputSize];
        input=new float[(noMovingParts*HingeArmPart.inputSize)+bodyInput];
        inputSynapsesWeights = new List<List<float>>();
        outputSynapsesWeights = new List<List<float>>();
        hiddenLayerBias = new List<float>(hiddenLayerSize);
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
    public void deepCopy(AnimalBrain source)
    {
        for (int i = 0; i < source.inputSynapsesWeights.Count; i++)
        {
            List<float> tempInputList=new List<float>();
            for (int j = 0; j < source.inputSynapsesWeights[i].Count; j++)
            {
                tempInputList.Add(source.inputSynapsesWeights[i][j]);
            }
            inputSynapsesWeights.Add(tempInputList);
        }
        for (int i = 0; i < source.outputSynapsesWeights.Count; i++)
        {
            List<float> tempOutputList=new List<float>();
            for (int j = 0; j < source.outputSynapsesWeights[i].Count; j++)
            {
                tempOutputList.Add(source.outputSynapsesWeights[i][j]);
            }
            outputSynapsesWeights.Add(tempOutputList);
        }
        List<float> tempBiasList=new List<float>();
        for (int i = 0; i < source.hiddenLayerSize; i++)
        {
            tempBiasList.Add(source.hiddenLayerBias[i]);
        }
        hiddenLayerBias=tempBiasList;
    }
    public void mixWeights(AnimalBrain parent, float chance)    //jaka jest szansa na zmiane genow
    {
        for (int i = 0; i < hiddenLayerSize; i++)
        {
            if (roll(chance))
            {
                hiddenLayerBias[i] = parent.hiddenLayerBias[i];
            }
            for (int j = 0; j < input.Length; j++)
            {
                if (roll(chance))
                {
                    inputSynapsesWeights[i][j] = parent.inputSynapsesWeights[i][j];
                }
            }
            for (int j = 0; j < output.Length; j++)
            {
                if (roll(chance))
                {
                    outputSynapsesWeights[i][j] = parent.outputSynapsesWeights[i][j];
                }
            }
        }
    }
    public void mutateWeights()    
    {
        float mutationChance;
        for (int i = 0; i < hiddenLayerSize; i++)
        {
            mutationChance = Random.Range(0.0f, 100.0f);
            if (roll(mutationChance))
            {
                hiddenLayerBias[i] = Random.Range(-1.0f, 1.0f);
            }
            for (int j = 0; j < input.Length; j++)
            {
                mutationChance = Random.Range(0.0f, 100.0f);
                if (roll(mutationChance))
                {
                    inputSynapsesWeights[i][j] = Random.Range(-1.0f, 1.0f);
                }
            }
            for (int j = 0; j < output.Length; j++)
            {
                mutationChance = Random.Range(0.0f, 100.0f);
                if (roll(mutationChance))
                {
                    outputSynapsesWeights[i][j] = Random.Range(-1.0f, 1.0f);
                }
            }
        }
    }
    float sigmoid(float value)
    {
        float translatedValue = (1 / (Mathf.Exp(-value) + 1)) * 2 - 1;  // wartosci od -1 do 1
        return translatedValue;
    }
    
    public void setRandomWeights()
    {

        
        for (int i = 0; i < hiddenLayerSize; i++)
        {
            hiddenLayerBias.Add(System.Convert.ToSingle(GeneratePopulation.rnd.NextDouble() * 2.0 - 1.0));
            List<float> neuronInputWeights = new List<float>();
            List<float> neuronOutputWeights = new List<float>();
            for (int j = 0; j < input.Length; j++)
            {
                neuronInputWeights.Add(System.Convert.ToSingle(GeneratePopulation.rnd.NextDouble() * 2.0 - 1.0));
            }
            for (int j = 0; j < output.Length; j++)
            {
                neuronOutputWeights.Add(System.Convert.ToSingle(GeneratePopulation.rnd.NextDouble() * 2.0 - 1.0));
            }
            inputSynapsesWeights.Add(neuronInputWeights);
            outputSynapsesWeights.Add(neuronOutputWeights);
        }
    }
    
    public void setOutput()
    {
        if(outputPart==output.Length)
        {
            outputPart=0;
        }
        int endPart=outputPart+HingeArmPart.outputSize;
        float tempOutputValue = 0;
        float neuronsCalculatedWeight;
        //Parallel.For(outputPart, endPart, delegate (int outputIndex)
        for (int outputIndex = outputPart; outputIndex < endPart; outputIndex++)
        {
            for (int neuronIndex = 0; neuronIndex < hiddenLayerBias.Count; neuronIndex++)
            {
                neuronsCalculatedWeight = 0;
                for (int synapseIndex = 0; synapseIndex < inputSynapsesWeights[neuronIndex].Count; synapseIndex++)
                {
                    //neuronsCalculatedWeight += sigmoid(input[synapseIndex]) * inputSynapsesWeights[neuronIndex][synapseIndex];
                    neuronsCalculatedWeight += input[synapseIndex] * inputSynapsesWeights[neuronIndex][synapseIndex];
                }
                //neuronsCalculatedWeight = sigmoid(neuronsCalculatedWeight);
                neuronsCalculatedWeight += hiddenLayerBias[neuronIndex];
                tempOutputValue = tempOutputValue + neuronsCalculatedWeight * outputSynapsesWeights[neuronIndex][outputIndex];
            }
            tempOutputValue = sigmoid(tempOutputValue);
            output[outputIndex] = tempOutputValue;
        }
        //);
        outputPart=endPart;
        // for (int outputIndex = 0; outputIndex < output.Length; outputIndex++)
        // {
        //     for (int neuronIndex = 0; neuronIndex < hiddenLayerBias.Count  ; neuronIndex++)
        //     {
        //         neuronsCalculatedWeight = 0;
        //         for (int synapseIndex = 0; synapseIndex < inputSynapsesWeights[neuronIndex].Count; synapseIndex++)
        //         {
        //             neuronsCalculatedWeight += sigmoid(input[synapseIndex]) * inputSynapsesWeights[neuronIndex][synapseIndex];
        //         }
        //         neuronsCalculatedWeight = sigmoid(neuronsCalculatedWeight);
        //         neuronsCalculatedWeight += hiddenLayerBias[neuronIndex];
        //         tempOutputValue = tempOutputValue + neuronsCalculatedWeight * outputSynapsesWeights[neuronIndex][outputIndex];
        //     }
        //     tempOutputValue = sigmoid(tempOutputValue);
        //     output[outputIndex] = tempOutputValue;
        // }
    }
}
