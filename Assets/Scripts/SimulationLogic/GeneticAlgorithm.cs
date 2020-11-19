using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GeneticAlgorithm
{
    private List<AnimalMovement> _currentGeneration;   //list of animals
    private AnimalData[] _populationGenPool;
    private float _mutationRate;
    private float _physcialMutationRate;
    private int numberOfElites;
    public float bestDistance { get; private set; }
    public float bestFitness { get; private set; }
    private float elitionismPercent = 0.1f;
    List<float> distancesList;
    List<float> fitnessList;
    List<int> elitesIndexes;
    private float[] _bodyPartsStartingPositions;
    const int minFitness = -999;
    public AnimalData bestAnimalData; 
    public GeneticAlgorithm(List<AnimalMovement> gen, float _mutationRate, float _physcialMutationRate)
    {
        _currentGeneration = gen;
        this._mutationRate = _mutationRate;
        this._physcialMutationRate = _physcialMutationRate;
        _populationGenPool = new AnimalData[_currentGeneration.Count()];
        numberOfElites = (int)Mathf.Ceil(_currentGeneration.Count() * elitionismPercent);
        AlgorithmStart();
    }
    public List<AnimalData> GetPopulationGenPool()
    {
        return _populationGenPool.ToList();
    }
    float CalculateFitness(AnimalMovement individual, float distance)
    {
        // var individualBody = individual.transform.Find("body").gameObject;
        float fitness = distance;
        fitness = fitness + individual.timeBeingAlive;
        // fitness = fitness * individual.averageBodyY;
        // if (individual.ifCrashed == true)
        // {
        //     fitness = fitness * penaltyForCrash;   //jesli upadnie, nieznacznie zmniejszam fitness
        // }
        return fitness;
    }

        
    
    float CalculateDistance(AnimalMovement individual)
    {
        int bodyPartsCount = individual.transform.childCount;
        float[] distances = new float[bodyPartsCount];
        // for (int i = 0; i < bodyPartsCount; i++)        // sprawdzam wszystkie czesci ciala, dziele przez ilosc czesci ciala, tak otrzymuje jak daleko doszly i fitness (koniec z wyrzucaniem body d przodu)
        // {
        //     Transform child = individual.transform.GetChild(i);
        //     distance = distance + child.transform.position.x - individual.bodyPartsStartingX[i];       //osobniki z nogami bardziej umiejscowionymi z przodu nie moga byc faworyzowane
        // }
        // distance = distance / bodyPartsCount;
        //dystans to najmniejszy pokonany dystans ze wszystkich czesci ciala - dzieki temu osobnniki nie wyrzucaja sie calym cialem do przodu
        for (int i = 0; i < bodyPartsCount; i++)
        {
            Transform child = individual.transform.GetChild(i);
            distances[i] = child.transform.position.x - individual.bodyPartsStartingX[i];       //osobniki z nogami bardziej umiejscowionymi z przodu nie moga byc faworyzowane
        }
        return distances.Min();
    }
    private void SetDistancesList()
    {
        distancesList = new List<float>();
        for (int i = 0; i < _currentGeneration.Count; i++)
        {
            distancesList.Add(CalculateDistance(_currentGeneration[i])); //tutaj bede trzymac wagi
        }
    }
    private void SetFitnessList()
    {
        fitnessList = new List<float>();
        for (int i = 0; i < _currentGeneration.Count; i++)
        {
            fitnessList.Add(CalculateFitness(_currentGeneration[i], distancesList[i])); //tutaj bede trzymac wagi
        }
    }
    private void SetElitiesIndexesList()
    {
        elitesIndexes = new List<int>();
        List<float> tempFitnessList = new List<float>(fitnessList); //kopiowanie jest niezbedne, lista bedzie modyfikowana
        for (int i = 0; i < numberOfElites; i++)
        {
            int nextEliteIndex = tempFitnessList.IndexOf(tempFitnessList.Max());
            elitesIndexes.Add(nextEliteIndex);
            tempFitnessList[nextEliteIndex] = minFitness;
        }
    }
    private void SetBestAnimalData()
    {
        int index=fitnessList.IndexOf(fitnessList.Max());
        bestAnimalData = _currentGeneration[index].animalData;
        bestDistance=fitnessList[index];
        bestFitness=distancesList[index];

    }
    private void AlgorithmStart()
    {
        SetDistancesList();
        SetFitnessList();
        SetBestAnimalData();
        SetElitiesIndexesList();
        var bestFitnessIndex = fitnessList.IndexOf(bestFitness);
        for (int i = 0; i < _currentGeneration.Count; i++)
        {
            if (elitesIndexes.Contains(i))    //najlepszy zostaje
            {
                _populationGenPool[i] = _currentGeneration[i].animalData.DeepCopy();
            }
            else
            {
                _populationGenPool[i] = Mate();
            }
        }
    }

    private int ChooseParent()
    {
        // int parentIndex = Random.Range(0, _currentGeneration.Count);
        // float worstFitness = fitnessList.Min();
        // float bestFitness = fitnessList.Max();
        // float randomFitnessNumber = Random.Range(worstFitness, bestFitness);
        // while (fitnessList[parentIndex] < randomFitnessNumber)
        // {
        //     parentIndex = Random.Range(0, _currentGeneration.Count);
        // }
        //tournament
        int border = Random.Range(0, fitnessList.Count);
        int count = Random.Range(1, fitnessList.Count - border);
        float parentFitness = fitnessList.GetRange(border, count).Max();
        return fitnessList.IndexOf(parentFitness);
    }
    public Dictionary<int, T> MixDictionaries<T>(Dictionary<int, T> parent1, Dictionary<int, T> parent2, float mixChance)
    {
        Dictionary<int, T> child = new Dictionary<int, T>(parent1);
        foreach (KeyValuePair<int, T> part in parent2)
        {
            if (mixChance >= Random.Range(0.0f, 100.0f))
            {
                child[part.Key] = part.Value;
            }
        }
        return child;
    }

    public AnimalData Mate()
    {
        //krzyzowanie polega na "konkursie" wynialajacym dwoch rodzicow na jednego osobnika potomnego
        //osobniki o wiekszym fitness maja wieksza szanse na zostanie wybranym do krzyzowania
        //losowana jest wartosc pomiedzy minimalna wartoscia z fitness list a maksymalna wartoscia z fitness list
        //losowany index osobnika dopoki wylosowana wczesniej wartosc nie jest mniejsza od wartosci fitness danego indexu
        //losowani dwaj rodzice, rozmnazanie jak wczesniej
        AnimalData childData = new AnimalData();
        int parent1Index = ChooseParent();
        int parent2Index = ChooseParent();
        while (parent2Index == parent1Index)
        {
            parent2Index = ChooseParent();
        }
        AnimalBrain childBrain = new AnimalBrain();
        float mixChance;
        childBrain.DeepCopyFrom(_currentGeneration[parent1Index].animalData.animalBrain);
        mixChance = Random.Range(0.0f, 100.0f);
        childData.partsMass = MixDictionaries<float>(_currentGeneration[parent1Index].animalData.partsMass, _currentGeneration[parent2Index].animalData.partsMass, mixChance);
        childData.partsScaleMultiplier = MixDictionaries<System.Numerics.Vector3>(_currentGeneration[parent1Index].animalData.partsScaleMultiplier, _currentGeneration[parent2Index].animalData.partsScaleMultiplier, mixChance);
        childData.targetJointsVelocity = MixDictionaries<int>(_currentGeneration[parent1Index].animalData.targetJointsVelocity, _currentGeneration[parent2Index].animalData.targetJointsVelocity, mixChance);
        childData.limbsPositionMultiplier = MixDictionaries<System.Numerics.Vector3>(_currentGeneration[parent1Index].animalData.limbsPositionMultiplier, _currentGeneration[parent2Index].animalData.limbsPositionMultiplier, mixChance);
        childBrain.mixWeights(_currentGeneration[parent2Index].animalData.animalBrain, mixChance);
        float chance = Random.Range(0.0f, 1.0f);
        if (chance < _mutationRate) //obsluga mutacji - mutacja obejmuje zmiane indexu genu z losowym innym genem
        {
            childBrain.mutateWeights();
        }
        chance = Random.Range(0.0f, 1.0f);  //szanse na mutacje wag i mutacje fizycznych wlasciwosci sa inne
        if (chance < _physcialMutationRate)
        {
            childData.MutateData();
        }
        childData.animalBrain = childBrain;
        return childData;
    }

}

