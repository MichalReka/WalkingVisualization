using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using JacksonDunstan.NativeCollections;
using static System.Math;
using Unity.Collections.LowLevel.Unsafe;
[System.Serializable]
public struct SerializableWeightsData   //NetiveArray2D sa serializowane do jednowymiaorwoej tablicy dlatego konieczne jest rzutowanie na zwykle tablice
{
    public float[,] inputSynapsesWeights;
    public float[,] outputSynapsesWeights;
    public float[,] hiddenLayerSynapsesWeights;
    public float[] hiddenLayerBias;
    public float[] secondHiddenLayerBias;
}
[System.Serializable]
public class AnimalBrain
{
    public float mGene;
    public SerializableWeightsData serializableWeightsData;
    public static int armsToMoveCount = 2;
    public static int outputPerArm = 1;
    public static int outputSize;
    [System.NonSerialized]
    private const int bodyInput = 4;
    public static int noMovingParts;
    [System.NonSerialized]
    public int hiddenLayerSize;
    private NativeArray2D<float> inputSynapsesWeights;
    private NativeArray2D<float> outputSynapsesWeights;
    private NativeArray2D<float> hiddenLayerSynapsesWeights;
    private NativeArray<float> hiddenLayerBias;
    private NativeArray<float> secondHiddenLayerBias;
    [System.NonSerialized]
    public NativeArray<float> output;
    [System.NonSerialized]
    public NativeArray<float> input;
    private JobHandle _jobHandler;
    public AnimalBrain()
    {
        output = new NativeArray<float>(outputSize, Allocator.Persistent);
        input = new NativeArray<float>((noMovingParts * JointHandler.inputSize) + bodyInput, Allocator.Persistent);
        hiddenLayerSize = input.Length * 4 / 3;
        inputSynapsesWeights = new NativeArray2D<float>(hiddenLayerSize, input.Length, Allocator.Persistent);
        outputSynapsesWeights = new NativeArray2D<float>(hiddenLayerSize, output.Length, Allocator.Persistent);
        hiddenLayerSynapsesWeights = new NativeArray2D<float>(hiddenLayerSize, hiddenLayerSize, Allocator.Persistent);
        hiddenLayerBias = new NativeArray<float>(hiddenLayerSize, Allocator.Persistent);
        secondHiddenLayerBias = new NativeArray<float>(hiddenLayerSize, Allocator.Persistent);
        _jobHandler = new JobHandle();
    }
    bool roll(float chance)
    {
        float localchance = Random.Range(0.0f, 1.0f);
        if (chance > localchance)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void DeepCopyFrom(AnimalBrain source)
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
        mGene = parent.mGene;
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
    public void MutateWeights() //mutacja zakonczona dla dwoch warstw
    {
        // int maxMutations = Mathf.RoundToInt(_numberOfWeights * _maxPercentGenesToMutate);
        // List<int> mutationIndexes = new List<int>();
        // int mutationsToOccur = Random.Range(0, maxMutations); //wybieram ile mutacji zajdzie
        // for (int i = 0; i < mutationsToOccur; i++)
        // {
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
        // }

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
    public void PrepareToSerialize()
    {
        serializableWeightsData = new SerializableWeightsData();
        serializableWeightsData.inputSynapsesWeights=inputSynapsesWeights.ToArray();
        serializableWeightsData.outputSynapsesWeights=outputSynapsesWeights.ToArray();
        serializableWeightsData.hiddenLayerSynapsesWeights=hiddenLayerSynapsesWeights.ToArray();
        serializableWeightsData.hiddenLayerBias=hiddenLayerBias.ToArray();
        serializableWeightsData.secondHiddenLayerBias=secondHiddenLayerBias.ToArray();
    }
    public void DeserializeWeights()
    {
        inputSynapsesWeights.CopyFrom(serializableWeightsData.inputSynapsesWeights);
        outputSynapsesWeights.CopyFrom(serializableWeightsData.outputSynapsesWeights);
        hiddenLayerSynapsesWeights.CopyFrom(serializableWeightsData.hiddenLayerSynapsesWeights);
        hiddenLayerBias.CopyFrom(serializableWeightsData.hiddenLayerBias);
        secondHiddenLayerBias.CopyFrom(serializableWeightsData.secondHiddenLayerBias);
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
    struct ComputeOutputJob : IJob
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
        public NativeArray<int> outputIndexesToCompute;
        public void Execute()
        {
            for (int i = 0; i < outputIndexesToCompute.Length; i++)
            {
                for (int secondNeuronIndex = 0; secondNeuronIndex < hiddenLayerBias.Length; secondNeuronIndex++)
                {
                    float secondNeuronCalculatedWeight = 0;
                    for (int neuronIndex = 0; neuronIndex < hiddenLayerBias.Length; neuronIndex++)
                    {
                        float neuronCalculatedWeight = 0;
                        for (int synapseIndex = 0; synapseIndex < input.Length; synapseIndex++)
                        {
                            neuronCalculatedWeight += input[synapseIndex] * inputSynapsesWeights[neuronIndex, synapseIndex];
                        }
                        neuronCalculatedWeight += hiddenLayerBias[neuronIndex];
                        secondNeuronCalculatedWeight += neuronCalculatedWeight * hiddenLayerSynapsesWeights[secondNeuronIndex, neuronIndex];
                    }
                    secondNeuronCalculatedWeight += secondHiddenLayerBias[secondNeuronIndex];
                    tempOutputValue[outputIndexesToCompute[i]] += secondNeuronCalculatedWeight * outputSynapsesWeights[secondNeuronIndex, outputIndexesToCompute[i]];
                }
                //sigmoid
                tempOutputValue[outputIndexesToCompute[i]] = (float)(1 / (Exp(-tempOutputValue[outputIndexesToCompute[i]]) + 1)) * 2 - 1;  // wartosci od -1 do 1
            }
        }
    }
    public void SetOutput(NativeArray<int> outputIndexesToCompute)
    {
        ComputeOutputJob outputJob = new ComputeOutputJob();
        outputJob.hiddenLayerBias = hiddenLayerBias;
        outputJob.secondHiddenLayerBias = secondHiddenLayerBias;
        outputJob.input = input;
        outputJob.inputSynapsesWeights = inputSynapsesWeights;
        outputJob.hiddenLayerSynapsesWeights = hiddenLayerSynapsesWeights;
        outputJob.outputSynapsesWeights = outputSynapsesWeights;
        outputJob.tempOutputValue = output;
        outputJob.outputIndexesToCompute=outputIndexesToCompute;
        _jobHandler=outputJob.Schedule();
    }
    public void FinishJob()
    {
        _jobHandler.Complete();
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
