using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
public class AnimalBrain
{
    // public List<int> limitMax;
    // public List<int> limitMin;
    // private int outputPart = 0;
    public bool isElite = false;
    public const int bodyInput = 3;
    public static int noMovingParts;
    public int hiddenLayerSize;
    public float[,] inputSynapsesWeights;
    public float[,] outputSynapsesWeights;
    public float[,] hiddenLayerSynapsesWeights;
    public float[] hiddenLayerBias;
    public float[] secondHiddenLayerBias;
    public float[] output;  //dzielnik czworki
    public float[] input;
    // int maxLimitBorder = 70;
    // int minLimitBorder = 0;
    public bool ifFirstOutput = true;
    public AnimalBrain()  
    {
        this.hiddenLayerSize = ((noMovingParts * JointHandler.inputSize) + bodyInput) * 2 / 3 + noMovingParts * JointHandler.outputSize;
        output = new float[1 + JointHandler.outputSize];
        input = new float[(noMovingParts * JointHandler.inputSize) + bodyInput + output.Length];
        inputSynapsesWeights = new float[hiddenLayerSize, input.Length];
        outputSynapsesWeights = new float[hiddenLayerSize, output.Length];
        hiddenLayerBias = new float[hiddenLayerSize];
        secondHiddenLayerBias = new float[hiddenLayerSize];
        hiddenLayerSynapsesWeights = new float[hiddenLayerSize, hiddenLayerSize];
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
        // limitMax = new List<int>(source.limitMax);
        // limitMin = new List<int>(source.limitMin);
        inputSynapsesWeights=source.inputSynapsesWeights.Clone() as float[,];
        outputSynapsesWeights=source.outputSynapsesWeights.Clone() as float[,];
        hiddenLayerSynapsesWeights=source.hiddenLayerSynapsesWeights.Clone() as float[,];
        secondHiddenLayerBias = source.secondHiddenLayerBias.Clone() as float[];
        hiddenLayerBias = source.hiddenLayerBias.Clone() as float[];
    }
    public void mixWeights(AnimalBrain parent, float chance)    //jaka jest szansa na zmiane genow
    {
        // for (int i = 0; i < noMovingParts; i++)
        // {
        //     if (roll(chance))
        //     {
        //         limitMin[i] = parent.limitMin[i];
        //         limitMax[i] = parent.limitMax[i];
        //     }
        // }
        for (int i = 0; i < hiddenLayerSize; i++)
        {
            if (roll(chance))
            {
                hiddenLayerBias[i] = parent.hiddenLayerBias[i];
            }
            if (roll(chance))
            {
                secondHiddenLayerBias[i] = parent.secondHiddenLayerBias[i];
            }
            for (int j = 0; j < hiddenLayerSize; j++)
            {
                if (roll(chance))
                {
                    hiddenLayerSynapsesWeights[i,j] = parent.hiddenLayerSynapsesWeights[i,j];
                }
            }
            for (int j = 0; j < input.Length; j++)
            {
                if (roll(chance))
                {
                    inputSynapsesWeights[i,j] = parent.inputSynapsesWeights[i,j];
                }
            }
            for (int j = 0; j < output.Length; j++)
            {
                if (roll(chance))
                {
                    outputSynapsesWeights[i,j] = parent.outputSynapsesWeights[i,j];
                }
            }
        }
    }
    public void mutateWeights() //mutacja zakonczona dla dwoch warstw
    {
        int maxMutations = 5;
        List<int> mutationIndexes = new List<int>();
        int mutationsToOccur = Random.Range(0, maxMutations); //wybieram ile mutacji zajdzie
        for (int i = 0; i < mutationsToOccur; i++)
        {
            int synapseGroup = Random.Range(0, 5); //wybieram czy ma byc zmieniony bias, synapsy input czy synapsy output
            switch (synapseGroup)
            {
                case 0:
                    hiddenLayerBias[Random.Range(0, hiddenLayerSize)] = Random.Range(-1.0f, 1.0f);
                    break;
                case 1:
                    inputSynapsesWeights[Random.Range(0, hiddenLayerSize), Random.Range(0, input.Length)] = Random.Range(-1.0f, 1.0f);
                    break;
                case 2:
                    outputSynapsesWeights[Random.Range(0, hiddenLayerSize),Random.Range(0, output.Length)] = Random.Range(-1.0f, 1.0f);
                    break;
                case 3:
                    secondHiddenLayerBias[Random.Range(0, secondHiddenLayerBias.Length)] = Random.Range(-1.0f, 1.0f);
                    break;
                case 4:
                    hiddenLayerSynapsesWeights[Random.Range(0, hiddenLayerSize),Random.Range(0, hiddenLayerSize)] = Random.Range(-1.0f, 1.0f);
                    break;
                    // case 5:
                    //     int hingeDirection = Random.Range(0, 3);    //3 rozne tryby zginania 
                    //     int randomMovingPart = Random.Range(0, noMovingParts);
                    //     if (hingeDirection == 0)
                    //     {
                    //         limitMin[randomMovingPart] = -maxLimitBorder;
                    //         limitMax[randomMovingPart] = minLimitBorder;
                    //     }
                    //     else if (hingeDirection == 1)
                    //     {
                    //         limitMin[randomMovingPart] = -minLimitBorder;
                    //         limitMax[randomMovingPart] = maxLimitBorder;
                    //     }
                    //     else
                    //     {
                    //         limitMin[randomMovingPart] = -maxLimitBorder;
                    //         limitMax[randomMovingPart] = maxLimitBorder;
                    //     }
                    //     break;
            }
        }

        //sposob losowego doboru ilosci synaps z kazdej grupy nie dowiodl swojej skutecznosci
        //mutowane osobniki byly w wiekszosci wadliwe i odpadaly - zbyt duzo losowych wartosci
        //mozna bylo zaobserwowac szybki wzrost odleglosci w pierwszych paru generacjach
        //po ktorym nastepowala stagnacja
        //co po eksperymentach na algorytmie genetycznym w innym programie
        //dowodzi brak prawidlowego dzialania mutacji
        //po prostu wybrane zostaly najlepsze osobniki z puli genowej danej na poczatku
        //mutacja nei pracowala wlasciwie
        // for (int i = 0; i < hiddenLayerSize; i++)
        // {
        //     mutationChance = Random.Range(0.0f, 100.0f);
        //     if (roll(mutationChance))
        //     {
        //         hiddenLayerBias[i] = Random.Range(-1.0f, 1.0f);
        //     }
        //     for (int j = 0; j < input.Length; j++)
        //     {
        //         mutationChance = Random.Range(0.0f, 100.0f);
        //         if (roll(mutationChance))
        //         {
        //             inputSynapsesWeights[i,j] = Random.Range(-1.0f, 1.0f);
        //         }
        //     }
        //     for (int j = 0; j < output.Length; j++)
        //     {
        //         mutationChance = Random.Range(0.0f, 100.0f);
        //         if (roll(mutationChance))
        //         {
        //             outputSynapsesWeights[i,j] = Random.Range(-1.0f, 1.0f);
        //         }
        //     }
        // }
    }
    float sigmoid(float value)
    {
        float translatedValue = (1 / (Mathf.Exp(-value) + 1)) * 2 - 1;  // wartosci od -1 do 1
        return translatedValue;
    }

    public void setRandomWeights()
    {
        // for (int i = 0; i < noMovingParts; i++)
        // {
        //     int hingeDirection = Random.Range(0, 3); //3 rozne tryby zginania 
        //     if (hingeDirection == 0)
        //     {
        //         limitMin.Add(-maxLimitBorder);
        //         limitMax.Add(minLimitBorder);
        //     }
        //     else if (hingeDirection == 1)
        //     {
        //         limitMin.Add(-minLimitBorder);
        //         limitMax.Add(maxLimitBorder);
        //     }
        //     else
        //     {
        //         limitMin.Add(-maxLimitBorder);
        //         limitMax.Add(maxLimitBorder);
        //     }
        // }
        for (int i = 0; i < hiddenLayerSize; i++)
        {
            hiddenLayerBias[i]=Random.Range(-1.0f, 1.0f);
            secondHiddenLayerBias[i]=Random.Range(-1.0f, 1.0f);
            for (int j = 0; j < input.Length; j++)
            {
                inputSynapsesWeights[i,j]=Random.Range(-1.0f, 1.0f);
            }
            for (int j = 0; j < output.Length; j++)
            {
                outputSynapsesWeights[i,j]=Random.Range(-1.0f, 1.0f);
            }
            for (int j = 0; j < hiddenLayerSize; j++)
            {
                hiddenLayerSynapsesWeights[i,j]=Random.Range(-1.0f, 1.0f);
            }
        }
    }
    public void setOutput()
    {

        float tempOutputValue = 0;
        float secondNeuronCalculatedWeight;
        int vectorSize = 4; //Vector4
        float[] neuronsCalculatedWeightsVector = new float[vectorSize];
        //var accVector = System.Numerics.Vector3.Zero;
        int synapseIndex;
        int neuronVectorIndex = 0;
        for (int outputIndex = 0; outputIndex < output.Length; outputIndex++)   //pierwszy element w tablicy to ktora noga ejst wybrana
        {
            for (int secondNeuronIndex = 0; secondNeuronIndex < hiddenLayerSize; secondNeuronIndex++)
            {
                secondNeuronCalculatedWeight = 0;
                // for (int neuronIndex = 0; neuronIndex < hiddenLayerBias.Count; neuronIndex++)
                // {
                //     neuronCalculatedWeight = 0;
                //     for (int synapseIndex = 0; synapseIndex < inputSynapsesWeights[neuronIndex].Count; synapseIndex++)
                //     {
                //         //neuronCalculatedWeight += sigmoid(input[synapseIndex]) * inputSynapsesWeights[neuronIndex,synapseIndex];
                //         neuronCalculatedWeight += input[synapseIndex] * inputSynapsesWeights[neuronIndex][synapseIndex];
                //     }
                //     //neuronCalculatedWeight = sigmoid(neuronCalculatedWeight);
                //     neuronCalculatedWeight += hiddenLayerBias[neuronIndex];
                //     secondNeuronCalculatedWeight += neuronCalculatedWeight * hiddenLayerSynapsesWeights[secondNeuronIndex][neuronIndex];
                // }
                for (int neuronIndex = 0; neuronIndex < hiddenLayerBias.Length; neuronIndex++)
                {
                    neuronsCalculatedWeightsVector[neuronVectorIndex] = 0;
                    //takie podejscie daje 6-7 fps przy update - SIMD mniej obciaza procesor
                    for (synapseIndex = 0; synapseIndex <= input.Length - vectorSize; synapseIndex += vectorSize)
                    {
                        var inputVector = new System.Numerics.Vector4(input[synapseIndex], input[synapseIndex + 1], input[synapseIndex + 2], input[synapseIndex + 3]);
                        var weightsVector = new System.Numerics.Vector4(inputSynapsesWeights[neuronIndex, synapseIndex], inputSynapsesWeights[neuronIndex,synapseIndex + 1], inputSynapsesWeights[neuronIndex,synapseIndex + 2], inputSynapsesWeights[neuronIndex,synapseIndex + 3]);
                        neuronsCalculatedWeightsVector[neuronVectorIndex] += System.Numerics.Vector4.Dot(inputVector, weightsVector);
                    }
                    for (; synapseIndex < input.Length; synapseIndex++)
                    {
                        neuronsCalculatedWeightsVector[neuronVectorIndex] += input[synapseIndex] * inputSynapsesWeights[neuronIndex, synapseIndex];
                    }
                    // for (int synapseIndex = 0; synapseIndex < inputSynapsesWeights[neuronIndex].Count; synapseIndex++)
                    // {

                    //     //neuronCalculatedWeight += sigmoid(input[synapseIndex]) * inputSynapsesWeights[neuronIndex][synapseIndex];
                    //     neuronCalculatedWeight += input[synapseIndex] * inputSynapsesWeights[neuronIndex][synapseIndex];
                    // }
                    //neuronCalculatedWeight = sigmoid(neuronCalculatedWeight);
                    neuronsCalculatedWeightsVector[neuronVectorIndex] += hiddenLayerBias[neuronIndex];
                    if (neuronVectorIndex == (vectorSize - 1))
                    {
                        var hiddenLayer = new System.Numerics.Vector4(hiddenLayerSynapsesWeights[secondNeuronIndex,neuronIndex], hiddenLayerSynapsesWeights[secondNeuronIndex,neuronIndex - 1], hiddenLayerSynapsesWeights[secondNeuronIndex,neuronIndex - 2], hiddenLayerSynapsesWeights[secondNeuronIndex,neuronIndex - 3]);
                        var neuronWeightsVector = new System.Numerics.Vector4(neuronsCalculatedWeightsVector[neuronVectorIndex], neuronsCalculatedWeightsVector[neuronVectorIndex - 1], neuronsCalculatedWeightsVector[neuronVectorIndex - 2], neuronsCalculatedWeightsVector[neuronVectorIndex - 3]);
                        secondNeuronCalculatedWeight += System.Numerics.Vector4.Dot(hiddenLayer, neuronWeightsVector);
                        neuronVectorIndex = 0;
                    }
                    else
                    {
                        neuronVectorIndex++;
                    }
                    // secondNeuronCalculatedWeight += neuronCalculatedWeight * hiddenLayerSynapsesWeights[secondNeuronIndex][neuronIndex];
                }
                neuronVectorIndex=0;
                int j = 0;
                for (int i = neuronVectorIndex; i >= 0; i--)
                {
                    secondNeuronCalculatedWeight += neuronsCalculatedWeightsVector[j] * hiddenLayerSynapsesWeights[secondNeuronIndex,neuronVectorIndex - i];
                    j++;
                }
                secondNeuronCalculatedWeight += secondHiddenLayerBias[secondNeuronIndex];
                tempOutputValue = tempOutputValue + secondNeuronCalculatedWeight * outputSynapsesWeights[secondNeuronIndex,outputIndex];
            }

            tempOutputValue = sigmoid(tempOutputValue);
            output[outputIndex] = tempOutputValue;
        }
        //);
        //jedna
        // for (int outputIndex = 0; outputIndex < output.Length; outputIndex++)
        // {
        //     for (int neuronIndex = 0; neuronIndex < hiddenLayerBias.Count  ; neuronIndex++)
        //     {
        //         neuronCalculatedWeight = 0;
        //         for (int synapseIndex = 0; synapseIndex < inputSynapsesWeights[neuronIndex].Count; synapseIndex++)
        //         {
        //             neuronCalculatedWeight += sigmoid(input[synapseIndex]) * inputSynapsesWeights[neuronIndex][synapseIndex];
        //         }
        //         neuronCalculatedWeight = sigmoid(neuronCalculatedWeight);
        //         neuronCalculatedWeight += hiddenLayerBias[neuronIndex];
        //         tempOutputValue = tempOutputValue + neuronCalculatedWeight * outputSynapsesWeights[neuronIndex][outputIndex];
        //     }
        //     tempOutputValue = sigmoid(tempOutputValue);
        //     output[outputIndex] = tempOutputValue;
        // }
        // outputPart = endPart;

    }
}
