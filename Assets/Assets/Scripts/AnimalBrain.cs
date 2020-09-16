using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class AnimalBrain
{
    // public List<int> limitMax;
    // public List<int> limitMin;
    private int outputPart = 0;
    public int bodyInput = 4;
    public static int noMovingParts;
    public int hiddenLayerSize;
    public List<List<float>> inputSynapsesWeights;
    public List<List<float>> outputSynapsesWeights;
    public List<List<float>> hiddenLayerSynapsesWeights;
    public List<float> hiddenLayerBias;
    public List<float> secondHiddenLayerBias;
    public float[] output;  //dzielnik czworki
    public float[] input;
    // int maxLimitBorder = 70;
    // int minLimitBorder = 0;

    public AnimalBrain()
    {
        this.hiddenLayerSize = ((noMovingParts * HingeArmPart.inputSize) + bodyInput) * 2 / 3 + noMovingParts * HingeArmPart.outputSize;
        output = new float[noMovingParts * HingeArmPart.outputSize];
        input = new float[(noMovingParts * HingeArmPart.inputSize) + bodyInput];
        inputSynapsesWeights = new List<List<float>>();
        outputSynapsesWeights = new List<List<float>>();
        hiddenLayerBias = new List<float>();
        secondHiddenLayerBias = new List<float>();
        hiddenLayerSynapsesWeights = new List<List<float>>();
        // limitMin = new List<int>();
        // limitMax = new List<int>();
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
        for (int i = 0; i < source.inputSynapsesWeights.Count; i++)
        {
            List<float> tempInputList = new List<float>();
            for (int j = 0; j < source.inputSynapsesWeights[i].Count; j++)
            {
                tempInputList.Add(source.inputSynapsesWeights[i][j]);
            }
            inputSynapsesWeights.Add(tempInputList);
        }
        for (int i = 0; i < source.outputSynapsesWeights.Count; i++)
        {
            List<float> tempOutputList = new List<float>();
            for (int j = 0; j < source.outputSynapsesWeights[i].Count; j++)
            {
                tempOutputList.Add(source.outputSynapsesWeights[i][j]);
            }
            outputSynapsesWeights.Add(tempOutputList);
        }
        for (int i = 0; i < source.hiddenLayerSynapsesWeights.Count; i++)
        {
            hiddenLayerSynapsesWeights.Add(new List<float>(source.hiddenLayerSynapsesWeights[i]));
        }
        secondHiddenLayerBias = new List<float>(source.secondHiddenLayerBias);
        hiddenLayerBias = new List<float>(source.hiddenLayerBias);
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
                    hiddenLayerSynapsesWeights[i][j] = parent.hiddenLayerSynapsesWeights[i][j];
                }
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
                    hiddenLayerBias[Random.Range(0, hiddenLayerBias.Count)] = Random.Range(-1.0f, 1.0f);
                    break;
                case 1:
                    inputSynapsesWeights[Random.Range(0, inputSynapsesWeights.Count)][Random.Range(0, inputSynapsesWeights[0].Count)] = Random.Range(-1.0f, 1.0f);
                    break;
                case 2:
                    outputSynapsesWeights[Random.Range(0, outputSynapsesWeights.Count)][Random.Range(0, outputSynapsesWeights[0].Count)] = Random.Range(-1.0f, 1.0f);
                    break;
                case 3:
                    secondHiddenLayerBias[Random.Range(0, secondHiddenLayerBias.Count)] = Random.Range(-1.0f, 1.0f);
                    break;
                case 4:
                    hiddenLayerSynapsesWeights[Random.Range(0, hiddenLayerSynapsesWeights.Count)][Random.Range(0, hiddenLayerSynapsesWeights[0].Count)] = Random.Range(-1.0f, 1.0f);
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
        //             inputSynapsesWeights[i][j] = Random.Range(-1.0f, 1.0f);
        //         }
        //     }
        //     for (int j = 0; j < output.Length; j++)
        //     {
        //         mutationChance = Random.Range(0.0f, 100.0f);
        //         if (roll(mutationChance))
        //         {
        //             outputSynapsesWeights[i][j] = Random.Range(-1.0f, 1.0f);
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
            hiddenLayerBias.Add(Random.Range(-1.0f, 1.0f));
            secondHiddenLayerBias.Add(Random.Range(-1.0f, 1.0f));
            List<float> neuronInputWeights = new List<float>();
            List<float> neuronOutputWeights = new List<float>();
            List<float> neuronHiddenLayerWeights = new List<float>();
            for (int j = 0; j < input.Length; j++)
            {
                neuronInputWeights.Add(Random.Range(-1.0f, 1.0f));
            }
            for (int j = 0; j < output.Length; j++)
            {
                neuronOutputWeights.Add(Random.Range(-1.0f, 1.0f));
            }
            for (int j = 0; j < hiddenLayerSize; j++)
            {
                neuronHiddenLayerWeights.Add(Random.Range(-1.0f, 1.0f));
            }
            hiddenLayerSynapsesWeights.Add(neuronHiddenLayerWeights);
            inputSynapsesWeights.Add(neuronInputWeights);
            outputSynapsesWeights.Add(neuronOutputWeights);
        }
    }

    public void setOutput()
    {
        if (outputPart == output.Length)
        {
            outputPart = 0;
        }
        int endPart = outputPart + HingeArmPart.outputSize;
        float tempOutputValue = 0;
        float neuronCalculatedWeight;
        float secondNeuronCalculatedWeight;
        //Parallel.For(outputPart, endPart, delegate (int outputIndex)
        for (int outputIndex = outputPart; outputIndex < endPart; outputIndex++)
        {
            for (int secondNeuronIndex = 0; secondNeuronIndex < hiddenLayerBias.Count; secondNeuronIndex++)
            {
                secondNeuronCalculatedWeight = 0;
                for (int neuronIndex = 0; neuronIndex < hiddenLayerBias.Count; neuronIndex++)
                {
                    neuronCalculatedWeight = 0;
                    for (int synapseIndex = 0; synapseIndex < inputSynapsesWeights[neuronIndex].Count; synapseIndex++)
                    {
                        //neuronCalculatedWeight += sigmoid(input[synapseIndex]) * inputSynapsesWeights[neuronIndex][synapseIndex];
                        neuronCalculatedWeight += input[synapseIndex] * inputSynapsesWeights[neuronIndex][synapseIndex];
                    }
                    //neuronCalculatedWeight = sigmoid(neuronCalculatedWeight);
                    neuronCalculatedWeight += hiddenLayerBias[neuronIndex];
                    secondNeuronCalculatedWeight += neuronCalculatedWeight * hiddenLayerSynapsesWeights[secondNeuronIndex][neuronIndex];
                }
                secondNeuronCalculatedWeight += secondHiddenLayerBias[secondNeuronIndex];
                tempOutputValue = tempOutputValue + secondNeuronCalculatedWeight * outputSynapsesWeights[secondNeuronIndex][outputIndex];
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
        outputPart = endPart;
        
    }
}
