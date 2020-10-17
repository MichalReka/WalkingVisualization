using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using JacksonDunstan.NativeCollections;
public class AnimalBrain : MonoBehaviour
{
    public bool isElite = false;
    private const int bodyInput = 1;
    public static int noMovingParts;
    public int hiddenLayerSize;
    public NativeArray2D<float> inputSynapsesWeights;
    public NativeArray2D<float> outputSynapsesWeights;
    public NativeArray2D<float> hiddenLayerSynapsesWeights;
    public NativeArray<float> hiddenLayerBias;
    public NativeArray<float> secondHiddenLayerBias;
    public NativeArray<float> output;
    public NativeArray<float> input;

    public bool ifFirstOutput = true;
    private int _numberOfWeights;
    private float _maxPercentGenesToMutate;
    public AnimalBrain(float maxPercentGenesToMutate)
    {
        output = new NativeArray<float>(1 + JointHandler.outputSize, Allocator.Persistent);
        input = new NativeArray<float>((noMovingParts * JointHandler.inputSize) + bodyInput + output.Length - 1, Allocator.Persistent);
        hiddenLayerSize = input.Length *4/3;
        inputSynapsesWeights = new NativeArray2D<float>(hiddenLayerSize, input.Length, Allocator.Persistent);
        outputSynapsesWeights = new NativeArray2D<float>(hiddenLayerSize, output.Length, Allocator.Persistent);
        hiddenLayerSynapsesWeights = new NativeArray2D<float>(hiddenLayerSize, hiddenLayerSize, Allocator.Persistent);
        hiddenLayerBias = new NativeArray<float>(hiddenLayerSize, Allocator.Persistent);
        secondHiddenLayerBias = new NativeArray<float>(hiddenLayerSize, Allocator.Persistent);
        _numberOfWeights=input.Length*hiddenLayerSize+hiddenLayerSize*hiddenLayerSize+hiddenLayerSize*output.Length+2*hiddenLayerSize;
        this._maxPercentGenesToMutate=maxPercentGenesToMutate;
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
        // inputSynapsesWeights = source.inputSynapsesWeights.Clone() as float[,];
        // outputSynapsesWeights = source.outputSynapsesWeights.Clone() as float[,];
        // hiddenLayerSynapsesWeights = source.hiddenLayerSynapsesWeights.Clone() as float[,];
        // secondHiddenLayerBias = source.secondHiddenLayerBias.Clone() as float[];
        // hiddenLayerBias = source.hiddenLayerBias.Clone() as float[];
        inputSynapsesWeights.CopyFrom(source.inputSynapsesWeights);
        outputSynapsesWeights.CopyFrom(source.outputSynapsesWeights);
        secondHiddenLayerBias.CopyFrom(source.secondHiddenLayerBias);
        hiddenLayerSynapsesWeights.CopyFrom(source.hiddenLayerSynapsesWeights);
        hiddenLayerBias.CopyFrom(source.hiddenLayerBias);
    }
    public void mixWeights(AnimalBrain parent, float chance)    //jaka jest szansa na zmiane genow
    {

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
                    hiddenLayerSynapsesWeights[i, j] = parent.hiddenLayerSynapsesWeights[i, j];
                }
            }
            for (int j = 0; j < input.Length; j++)
            {
                if (roll(chance))
                {
                    inputSynapsesWeights[i, j] = parent.inputSynapsesWeights[i, j];
                }
            }
            for (int j = 0; j < output.Length; j++)
            {
                if (roll(chance))
                {
                    outputSynapsesWeights[i, j] = parent.outputSynapsesWeights[i, j];
                }
            }
        }
    }
    public void mutateWeights() //mutacja zakonczona dla dwoch warstw
    {
        int maxMutations = Mathf.RoundToInt(_numberOfWeights*_maxPercentGenesToMutate);
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
                    outputSynapsesWeights[Random.Range(0, hiddenLayerSize), Random.Range(0, output.Length)] = Random.Range(-1.0f, 1.0f);
                    break;
                case 3:
                    secondHiddenLayerBias[Random.Range(0, secondHiddenLayerBias.Length)] = Random.Range(-1.0f, 1.0f);
                    break;
                case 4:
                    hiddenLayerSynapsesWeights[Random.Range(0, hiddenLayerSize), Random.Range(0, hiddenLayerSize)] = Random.Range(-1.0f, 1.0f);
                    break;
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

        for (int i = 0; i < hiddenLayerSize; i++)
        {
            hiddenLayerBias[i] = Random.Range(-1.0f, 1.0f);
            secondHiddenLayerBias[i] = Random.Range(-1.0f, 1.0f);
            for (int j = 0; j < input.Length; j++)
            {
                inputSynapsesWeights[i, j] = Random.Range(-1.0f, 1.0f);
            }
            for (int j = 0; j < output.Length; j++)
            {
                outputSynapsesWeights[i, j] = Random.Range(-1.0f, 1.0f);
            }
            for (int j = 0; j < hiddenLayerSize; j++)
            {
                hiddenLayerSynapsesWeights[i, j] = Random.Range(-1.0f, 1.0f);
            }
        }
    }
    [BurstCompile]
    struct ComputeOutputJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<float> hiddenLayerBias;
        [ReadOnly]
        public NativeArray<float> secondHiddenLayerBias;
        [ReadOnly]
        public NativeArray<float> input;
        [ReadOnly]
        public NativeArray2D<float> inputSynapsesWeights;
        [ReadOnly]
        public NativeArray2D<float> hiddenLayerSynapsesWeights;
        [ReadOnly]
        public NativeArray2D<float> outputSynapsesWeights;
        public NativeArray<float> tempOutputValue;
        public int vectorSize;
        public int outputIndex;
        public void Execute(int i)
        {
            float secondNeuronCalculatedWeight = 0;
            for (int neuronIndex = 0; neuronIndex < hiddenLayerBias.Length; neuronIndex++)
            {
                float neuronCalculatedWeight = 0;
                for (int synapseIndex = 0; synapseIndex < input.Length; synapseIndex++)
                {
                    neuronCalculatedWeight += input[synapseIndex] * inputSynapsesWeights[neuronIndex,synapseIndex];
                }
                neuronCalculatedWeight += hiddenLayerBias[neuronIndex];
                secondNeuronCalculatedWeight += neuronCalculatedWeight * hiddenLayerSynapsesWeights[i,neuronIndex];
            }
            secondNeuronCalculatedWeight += secondHiddenLayerBias[i];
            tempOutputValue[i] = secondNeuronCalculatedWeight * outputSynapsesWeights[i,outputIndex];

            // float neuronCalculatedWeight = 0;
            // for (int synapseIndex = 0; synapseIndex < input.Length; synapseIndex++)
            // {
            //     neuronCalculatedWeight += input[synapseIndex] * inputSynapsesWeights[i, synapseIndex];
            // }
            // neuronCalculatedWeight += hiddenLayerBias[i];
            // tempOutputValue[i] = neuronCalculatedWeight * outputSynapsesWeights[i, outputIndex];

        }
    }
    public void setOutput()
    {

        // float tempOutputValue = 0;
        // float secondNeuronCalculatedWeight;
        int vectorSize = 4; //Vector4
        // float[] neuronsCalculatedWeightsVector = new float[vectorSize];
        //var accVector = System.Numerics.Vector3.Zero;
        // int synapseIndex;
        // int neuronVectorIndex = 0;
        JobHandle[] jobsHandler = new JobHandle[output.Length];
        for (int outputIndex = 0; outputIndex < output.Length; outputIndex++)   //pierwszy element w tablicy to ktora noga ejst wybrana
        {
            // Set up the job data

            // for (int secondNeuronIndex = 0; secondNeuronIndex < hiddenLayerSize; secondNeuronIndex++)
            // {
            //     secondNeuronCalculatedWeight = 0;
            //     for (int neuronIndex = 0; neuronIndex < hiddenLayerBias.Length; neuronIndex++)
            //     {
            //         neuronsCalculatedWeightsVector[neuronVectorIndex] = 0;
            //         //takie podejscie daje 6-7 fps przy update - SIMD mniej obciaza procesor
            //         for (synapseIndex = 0; synapseIndex <= input.Length - vectorSize; synapseIndex += vectorSize)
            //         {
            //             var inputVector = new System.Numerics.Vector4(input[synapseIndex], input[synapseIndex + 1], input[synapseIndex + 2], input[synapseIndex + 3]);
            //             var weightsVector = new System.Numerics.Vector4(inputSynapsesWeights[neuronIndex, synapseIndex], inputSynapsesWeights[neuronIndex, synapseIndex + 1], inputSynapsesWeights[neuronIndex, synapseIndex + 2], inputSynapsesWeights[neuronIndex, synapseIndex + 3]);
            //             neuronsCalculatedWeightsVector[neuronVectorIndex] += System.Numerics.Vector4.Dot(inputVector, weightsVector);
            //         }
            //         for (; synapseIndex < input.Length; synapseIndex++)
            //         {
            //             neuronsCalculatedWeightsVector[neuronVectorIndex] += input[synapseIndex] * inputSynapsesWeights[neuronIndex, synapseIndex];
            //         }
            //         neuronsCalculatedWeightsVector[neuronVectorIndex] += hiddenLayerBias[neuronIndex];
            //         if (neuronVectorIndex == (vectorSize - 1))
            //         {
            //             var hiddenLayer = new System.Numerics.Vector4(hiddenLayerSynapsesWeights[secondNeuronIndex, neuronIndex], hiddenLayerSynapsesWeights[secondNeuronIndex, neuronIndex - 1], hiddenLayerSynapsesWeights[secondNeuronIndex, neuronIndex - 2], hiddenLayerSynapsesWeights[secondNeuronIndex, neuronIndex - 3]);
            //             var neuronWeightsVector = new System.Numerics.Vector4(neuronsCalculatedWeightsVector[neuronVectorIndex], neuronsCalculatedWeightsVector[neuronVectorIndex - 1], neuronsCalculatedWeightsVector[neuronVectorIndex - 2], neuronsCalculatedWeightsVector[neuronVectorIndex - 3]);
            //             secondNeuronCalculatedWeight += System.Numerics.Vector4.Dot(hiddenLayer, neuronWeightsVector);
            //             neuronVectorIndex = 0;
            //         }
            //         else
            //         {
            //             neuronVectorIndex++;
            //         }
            //     }
            //     neuronVectorIndex = 0;
            //     int j = 0;
            //     for (int i = neuronVectorIndex; i >= 0; i--)
            //     {
            //         secondNeuronCalculatedWeight += neuronsCalculatedWeightsVector[j] * hiddenLayerSynapsesWeights[secondNeuronIndex, neuronVectorIndex - i];
            //         j++;
            //     }
            //     secondNeuronCalculatedWeight += secondHiddenLayerBias[secondNeuronIndex];
            //     tempOutputValue = tempOutputValue + secondNeuronCalculatedWeight * outputSynapsesWeights[secondNeuronIndex, outputIndex];
            // }
            // tempOutputValue = sigmoid(tempOutputValue);
            // output[outputIndex] = tempOutputValue;
            NativeArray<float> outputArr = new NativeArray<float>(hiddenLayerSize, Allocator.TempJob);
            ComputeOutputJob outputJob = new ComputeOutputJob();
            outputJob.hiddenLayerBias = hiddenLayerBias;
            outputJob.secondHiddenLayerBias = secondHiddenLayerBias;
            outputJob.input = input;
            outputJob.inputSynapsesWeights = inputSynapsesWeights;
            outputJob.hiddenLayerSynapsesWeights = hiddenLayerSynapsesWeights;
            outputJob.outputSynapsesWeights = outputSynapsesWeights;
            outputJob.tempOutputValue = outputArr;
            outputJob.vectorSize = vectorSize;
            outputJob.outputIndex = outputIndex;
            jobsHandler[outputIndex] = outputJob.Schedule(hiddenLayerSize, 20);
            jobsHandler[outputIndex].Complete();
            output[outputIndex] = 0;
            for (int i = 0; i < hiddenLayerSize; i++)
            {
                output[outputIndex] += outputArr[i];
            }
            outputArr.Dispose();
            output[outputIndex] = sigmoid(output[outputIndex]);
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
    }

    public void Dispose()
    {
        output.Dispose();
        input.Dispose();
        inputSynapsesWeights.Dispose();
        outputSynapsesWeights.Dispose();
        hiddenLayerSynapsesWeights.Dispose();
        hiddenLayerBias.Dispose();
        secondHiddenLayerBias.Dispose();
    }
}
