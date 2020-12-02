using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GeneticAlgorithm
{
    bool _ifAdaptive = false;
    private List<AnimalMovement> _currentGeneration;   //list of animals
    private AnimalData[] _populationGenPool;
    private float _globalMutationRate;
    private float _globalPhyscialMutationRate;
    private float _timeBelowAveragePenalty;
    private float _averageTimeBeingAlive;
    private int _globalNumberOfElites;
    public float bestDistance { get; private set; }
    public float bestFitness { get; private set; }
    private float _tournamentPartMaxSize;
    public float elitionismPercent = 0.01f;
    List<float> distancesList;
    List<float> fitnessList;
    List<int> elitesIndexes;
    List<float> _syncErrors;
    private float[] _bodyPartsStartingPositions;
    const int minFitness = -999;
    public AnimalData bestAnimalData;
    int _matingMaxIterations;
    float _crossoverChance;
    public float fitnessAverage;
    public float distanceAverage;
    float _previousFitnessAverage;
    int _numberOfWeightsMutations;
    int _generationsToConsider;
    List<float> _fitnessesToConsider;
    // float _syncErrorAverage;
    public GeneticAlgorithm(List<AnimalMovement> gen)
    {
        _ifAdaptive = PopulationInputData.adaptationEnabled;
        _tournamentPartMaxSize = PopulationInputData.tournamentSize;
        _crossoverChance = PopulationInputData.crossoverPercent;
        _averageTimeBeingAlive = 0;
        _globalMutationRate = PopulationInputData.weightsMutationRate;
        _globalPhyscialMutationRate = PopulationInputData.physicalMutationRate;
        _timeBelowAveragePenalty = PopulationInputData.timeBelowAveragePenalty;
        fitnessAverage = 0;
        _currentGeneration = gen;
        _populationGenPool = new AnimalData[_currentGeneration.Count()];
        _globalNumberOfElites = (int)Mathf.Ceil(_currentGeneration.Count() * elitionismPercent);
        _matingMaxIterations = (int)Mathf.Ceil(gen.Count * 0.1f);
        _numberOfWeightsMutations = 1;
        _generationsToConsider = 3;
        _fitnessesToConsider = new List<float>();
    }

    public List<AnimalData> GetPopulationGenPool()
    {
        return _populationGenPool.ToList();
    }
    float CalculateFitness(AnimalMovement individual, float distance)
    {
        // var individualBody = individual.transform.Find("body").gameObject;
        float fitness = distance;
        if (individual.timeBeingAlive < _averageTimeBeingAlive)
        {
            fitness = fitness * _timeBelowAveragePenalty;
        }
        // if (PopulationInputData.forceSimilarPartsSynchronization)
        // {
        //     // if(_syncErrorAverage<individual.syncError)
        //     // {
        //     //     fitness = fitness * 0.9f;
        //     // }
        // }
        // else
        // {
        //     if (individual.averageVelocity > _averageVelocity)
        //     {
        //         fitness = fitness * (2-_timeBelowAveragePenalty);
        //     }
        // }
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
        return distances.Average();
    }
    private void SetDistancesList()
    {
        distancesList = new List<float>();
        for (int i = 0; i < _currentGeneration.Count; i++)
        {
            distancesList.Add(CalculateDistance(_currentGeneration[i])); //tutaj bede trzymac wagi
        }
        distanceAverage = distancesList.Average();
    }
    private void SetFitnessList()
    {
        fitnessList = new List<float>();
        _previousFitnessAverage = fitnessAverage;
        for (int i = 0; i < _currentGeneration.Count; i++)
        {
            fitnessList.Add(CalculateFitness(_currentGeneration[i], distancesList[i])); //tutaj bede trzymac wagi
        }
        fitnessAverage = fitnessList.Average();
    }
    private void SetElitiesIndexesList()
    {
        elitesIndexes = new List<int>();
        List<float> tempFitnessList = new List<float>(fitnessList); //kopiowanie jest niezbedne, lista bedzie modyfikowana
        for (int i = 0; i < _globalNumberOfElites; i++)
        {
            int nextEliteIndex = tempFitnessList.IndexOf(tempFitnessList.Max());
            elitesIndexes.Add(nextEliteIndex);
            tempFitnessList[nextEliteIndex] = minFitness;
        }
    }
    private void SetBestAnimalData()
    {
        int index = fitnessList.IndexOf(fitnessList.Max());
        bestAnimalData = _currentGeneration[index].animalData;
        bestDistance = fitnessList[index];
        bestFitness = distancesList[index];
    }
    private void CalculateAliveTimeAverage()
    {
        for (int i = 0; i < _currentGeneration.Count; i++)
        {
            _averageTimeBeingAlive += _currentGeneration[i].timeBeingAlive;
        }
        _averageTimeBeingAlive = _averageTimeBeingAlive / _currentGeneration.Count;
    }
    public void AdjustMutationChance(AnimalData animalData, float fitness)
    {
        animalData.weightsMutationRate = animalData.weightsMutationRate * fitnessAverage / fitness;
        animalData.physicalMutationRate = animalData.physicalMutationRate * fitnessAverage / fitness;
    }
    // private float CalculateIndAverageSyncError(AnimalMovement individual)
    // {
    //     List<JointHandler> jointsToChoose = new List<JointHandler>(individual.orderedMovingParts);
    //     float syncError = 0;
    //     while (jointsToChoose.Count > 0)
    //     {
    //         int pairIndex = jointsToChoose[0].jointPairIndex;
    //         for (int secondIndex = 1; secondIndex < jointsToChoose.Count; secondIndex++)
    //         {
    //             if (jointsToChoose[secondIndex].jointPairIndex == pairIndex)
    //             {
    //                 syncError += Mathf.Abs(jointsToChoose[secondIndex].averageVelocityOutput - jointsToChoose[0].averageVelocityOutput);
    //                 jointsToChoose.RemoveAt(secondIndex);
    //                 jointsToChoose.RemoveAt(0);
    //                 break;
    //             }
    //         }
    //     }
    //     individual.syncError = syncError;
    //     return syncError;
    // }
    // private void CalculateGenAverageSyncError()
    // {
    //     _syncErrorAverage = 0;
    //     foreach (AnimalMovement individual in _currentGeneration)
    //     {
    //         _syncErrorAverage+=CalculateIndAverageSyncError(individual);
    //     }
    // }
    void AdjustCrossoverPercent()
    {
        if (_previousFitnessAverage > 0)
        {
            // _crossoverChance = _crossoverChance * _previousFitnessAverage / fitnessAverage;
            // if (_crossoverChance > 0.5f)   //bezpiecznik aby dziwne krzyzowki nie powstaly
            // {
            //     _crossoverChance = 0.5f;
            // }
            if (_crossoverChance < 0.5f)
            {
                _crossoverChance = _crossoverChance + _crossoverChance * 0.02f;
            }
        }
    }
    void AdjustSelectionPressure()
    {
        if (fitnessAverage > _previousFitnessAverage)
        {
            if (_tournamentPartMaxSize < 0.5f)
            {
                _tournamentPartMaxSize = _tournamentPartMaxSize + _tournamentPartMaxSize * 0.02f;
            }
        }
    }
    void AdjustMutationsIterationNumber()
    {
        if (_fitnessesToConsider.Count == _generationsToConsider)
        {
            if (Mathf.RoundToInt(bestFitness) > Mathf.RoundToInt(_fitnessesToConsider.Average()))
            {
                if (_numberOfWeightsMutations > 1)
                {
                    _numberOfWeightsMutations = _numberOfWeightsMutations - (int)Mathf.Ceil(_numberOfWeightsMutations * 0.5f);
                }
            }
            else
            {
                _numberOfWeightsMutations = _numberOfWeightsMutations + (int)Mathf.Ceil(_numberOfWeightsMutations * 0.5f);
            }
            _fitnessesToConsider.Clear();
        }
        else
        {
            _fitnessesToConsider.Add(bestFitness);
        }
    }
    public void AlgorithmStart()
    {
        SetDistancesList();
        // if (PopulationInputData.forceSimilarPartsSynchronization)
        // {
        //     // CalculateGenAverageSyncError();
        // }
        CalculateAliveTimeAverage();
        SetFitnessList();
        SetBestAnimalData();
        SetElitiesIndexesList();
        if (_ifAdaptive)
        {
            AdjustCrossoverPercent();
            AdjustSelectionPressure();
            AdjustMutationsIterationNumber();
        }

        var bestFitnessIndex = fitnessList.IndexOf(bestFitness);
        for (int i = 0; i < _currentGeneration.Count; i++)
        {
            if (_ifAdaptive)
            {
                AdjustMutationChance(_currentGeneration[i].animalData, fitnessList[i]);
            }
            if (elitesIndexes.Contains(i))    //najlepszy zostaje
            {
                _populationGenPool[i] = _currentGeneration[i].animalData;
            }
            else
            {
                _populationGenPool[i] = Mate();
            }
        }
    }

    private int ChooseParent()  //wybieram losowo osobnikow do konkursu - indexy nie moga sie powtarzac
    {
        //jak galapagos
        //dobieranie "wagami" prowadzilo do szybkiej stagnacji
        // int parentIndex = Random.Range(0, _currentGeneration.Count);
        // float worstFitness = fitnessList.Min();
        // float bestFitness = fitnessList.Max();
        // float randomFitnessNumber = Random.Range(worstFitness, bestFitness);
        // while (fitnessList[parentIndex] < randomFitnessNumber)
        // {
        //     parentIndex = Random.Range(0, _currentGeneration.Count);
        // }
        //tournament - nie wybieram przedzialu osobnikow - przez to moga tworzyc sie "obszary" w liscie osobnikow, gdzie podobne osobniki sa krzyzowane z podobnymi
        //wtedy bardzo szybko dane 
        int count = Random.Range(1, (int)Mathf.Ceil((fitnessList.Count) * _tournamentPartMaxSize));    //ilosc osobnikow w jednym konkursie bedzie definiowana w menu
        Dictionary<int, float> fitnessesForTournament = new Dictionary<int, float>();
        int randomIndex;
        for (int i = 0; i < count; i++)
        {
            randomIndex = Random.Range(0, fitnessList.Count);
            while (fitnessesForTournament.ContainsKey(randomIndex))
            {
                randomIndex = Random.Range(0, fitnessList.Count);
            }
            fitnessesForTournament.Add(randomIndex, fitnessList[randomIndex]);
        }
        KeyValuePair<int, float> tournamentWinner = fitnessesForTournament.First();
        foreach (KeyValuePair<int, float> contester in fitnessesForTournament)
        {
            if (contester.Value > tournamentWinner.Value)
            {
                tournamentWinner = contester;
            }
        }
        // float parentFitness = fitnessList.GetRange(beginning, count).Max();
        return tournamentWinner.Key;
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
        float parent1MGene = _currentGeneration[parent1Index].animalData.animalBrain.mGene;
        int parent2Index = ChooseParent();
        float chance;
        int currentIteration = 0;
        while (true)
        {
            parent2Index = ChooseParent();
            float parent2MGene = _currentGeneration[parent2Index].animalData.animalBrain.mGene; //jeden z rodzicow przekazuje mGene
            currentIteration++;
            if (parent1MGene != parent2MGene)  //jesli to ten sam index to taki sam mGene
            {
                break;
            }
            else if (currentIteration >= _matingMaxIterations)
            {
                if (parent1Index != parent2Index)
                {
                    break;
                }
            }
        }
        AnimalBrain childBrain = new AnimalBrain();
        // float mixChance;
        childBrain.DeepCopyFrom(_currentGeneration[parent1Index].animalData.animalBrain);
        // mixChance = Random.Range(0.0f, 75.0f);
        childData.partsMass = MixDictionaries<float>(_currentGeneration[parent1Index].animalData.partsMass, _currentGeneration[parent2Index].animalData.partsMass, _crossoverChance);
        childData.partsScaleMultiplier = MixDictionaries<System.Numerics.Vector3>(_currentGeneration[parent1Index].animalData.partsScaleMultiplier, _currentGeneration[parent2Index].animalData.partsScaleMultiplier, _crossoverChance);
        childData.targetJointsVelocity = MixDictionaries<int>(_currentGeneration[parent1Index].animalData.targetJointsVelocity, _currentGeneration[parent2Index].animalData.targetJointsVelocity, _crossoverChance);
        childData.limbsPositionMultiplier = MixDictionaries<System.Numerics.Vector3>(_currentGeneration[parent1Index].animalData.limbsPositionMultiplier, _currentGeneration[parent2Index].animalData.limbsPositionMultiplier, _crossoverChance);
        childBrain.mixWeights(_currentGeneration[parent2Index].animalData.animalBrain, _crossoverChance);
        float mutationRate;
        float physicalMutationRate;
        if (_ifAdaptive)
        {
            physicalMutationRate = childData.physicalMutationRate;
            mutationRate = childData.weightsMutationRate;
        }
        else
        {
            physicalMutationRate = _globalPhyscialMutationRate;
            mutationRate = _globalMutationRate;
        }
        for (int i = 0; i < _numberOfWeightsMutations; i++)
        {
            chance = Random.Range(0.0f, 1.0f);
            if (chance < mutationRate) //obsluga mutacji - mutacja obejmuje zmiane indexu genu z losowym innym genem
            {
                childBrain.MutateWeights();
            }
        }
        chance = Random.Range(0.0f, 1.0f);  //szanse na mutacje wag i mutacje fizycznych wlasciwosci sa inne
        if (chance < physicalMutationRate)
        {
            childData.MutateData();
        }
        childData.animalBrain = childBrain;
        return childData;
    }
}

