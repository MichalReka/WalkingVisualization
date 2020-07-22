using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class AnimalBrain
{
    public List<int> limitMax;
    public List<int> limitMin;
    private int outputPart = 0;
    public int bodyInput = 4;
    public static int noMovingParts;
    public int hiddenLayerSize;
    public List<List<float>> inputSynapsesWeights;
    public List<List<float>> outputSynapsesWeights;
    public List<float> hiddenLayerBias;
    public float[] output;  //dzielnik czworki
    public float[] input;
    int maxLimitBorder=55;
    int minLimitBorder=10;
    // public AnimalBrain()
    // {
    //     hiddenLayerSize = input.Count * 2 / 3 + outputSize;
    //     if (ifSetoToRandom)
    //     {
    //         setRandomWeights();
    //     }
    // }
    public AnimalBrain()
    {
        this.hiddenLayerSize = ((noMovingParts * HingeArmPart.inputSize) + bodyInput) * 2 / 3 + noMovingParts * HingeArmPart.outputSize;
        output = new float[noMovingParts * HingeArmPart.outputSize];
        input = new float[(noMovingParts * HingeArmPart.inputSize) + bodyInput];
        inputSynapsesWeights = new List<List<float>>();
        outputSynapsesWeights = new List<List<float>>();
        hiddenLayerBias = new List<float>(hiddenLayerSize);
        limitMin=new List<int>();
        limitMax=new List<int>();
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
        limitMax=new List<int>(source.limitMax);
        limitMin=new List<int>(source.limitMin);
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
        hiddenLayerBias = new List<float>(source.hiddenLayerBias);
    }
    public void mixWeights(AnimalBrain parent, float chance)    //jaka jest szansa na zmiane genow
    {
        for(int i=0;i<noMovingParts;i++)
        {
            if (roll(chance))
            {
                limitMin[i] = parent.limitMin[i];
                limitMax[i] = parent.limitMax[i];
            }
        }
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
        //zmieniam tylko jedna wartosc - mutacje maja niewielki zasieg, lecz duzy wplyw na wynik
        int synapseGroup = Random.Range(0, 4); //wybieram czy ma byc zmieniony bias, synapsy input czy synapsy output
        switch (synapseGroup)
        {
            case 0:
                hiddenLayerBias[Random.Range(0,hiddenLayerBias.Count)] = Random.Range(-1.0f, 1.0f);
                break;
            case 1:
                inputSynapsesWeights[Random.Range(0,inputSynapsesWeights.Count)][Random.Range(0,inputSynapsesWeights[0].Count)] = Random.Range(-1.0f, 1.0f);
                break;
            case 2:
                outputSynapsesWeights[Random.Range(0,outputSynapsesWeights.Count)][Random.Range(0,outputSynapsesWeights[0].Count)] = Random.Range(-1.0f, 1.0f);
                break;
            case 3:
                int hingeDirection=Random.Range(0,2);
                int randomMovingPart=Random.Range(0,noMovingParts);
                if(hingeDirection==0)
                {
                    limitMin[randomMovingPart]=-maxLimitBorder;
                    limitMax[randomMovingPart]=minLimitBorder;
                }
                else
                {
                    limitMin[randomMovingPart]=-minLimitBorder;
                    limitMax[randomMovingPart]=maxLimitBorder;
                }
                break;
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
        for(int i=0;i<noMovingParts;i++)
        {
            int hingeDirection=Random.Range(0,2);
            if(hingeDirection==0)
            {
                limitMin.Add(-maxLimitBorder);
                limitMax.Add(minLimitBorder);
            }
            else
            {
                limitMin.Add(-minLimitBorder);
                limitMax.Add(maxLimitBorder);
            }
        }
        for (int i = 0; i < hiddenLayerSize; i++)
        {
            hiddenLayerBias.Add(Random.Range(-1.0f, 1.0f));
            List<float> neuronInputWeights = new List<float>();
            List<float> neuronOutputWeights = new List<float>();
            for (int j = 0; j < input.Length; j++)
            {
                neuronInputWeights.Add(Random.Range(-1.0f, 1.0f));
            }
            for (int j = 0; j < output.Length; j++)
            {
                neuronOutputWeights.Add(Random.Range(-1.0f, 1.0f));
            }
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
        outputPart = endPart;
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
