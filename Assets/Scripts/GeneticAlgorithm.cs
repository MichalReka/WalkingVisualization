﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GeneticAlgorithm
{
    private List<AnimalMovement> _currentGeneration;   //list of animals
    private AnimalBrain[] _populationGenPool;
    private float _mutationRate;
    private int numberOfElites;
    public float bestDistance { get; private set; }
    public float bestFitness { get; private set; }
    private float elitionismPercent = 0.1f;
    List<float> distancesList;
    List<float> fitnessList;
    List<int> elitesIndexes;
    public float _maxPercentGenesToMutate;
    const int minFitness = -999;
    // float penaltyForCrash = 1;
    public GeneticAlgorithm(List<AnimalMovement> gen, float _mutationRate,float maxPercentGenesToMutate)
    {
        _currentGeneration = gen;
        this._mutationRate = _mutationRate;
        this._maxPercentGenesToMutate=maxPercentGenesToMutate;
        _populationGenPool = new AnimalBrain[_currentGeneration.Count()];
        numberOfElites = (int)Mathf.Ceil(_currentGeneration.Count() * elitionismPercent);
        AlgorithmStart();
    }
    public List<AnimalBrain> GetPopulationGenPool()
    {
        return _populationGenPool.ToList();
    }
    float CalculateFitness(AnimalMovement individual,float distance)
    {
        // var individualBody = individual.transform.Find("body").gameObject;
        // sprawdzam wszystkie czesci ciala, dziele przez ilosc czesci ciala, tak otrzymuje jak daleko doszly i fitness (koniec z wyrzucaniem body d przodu)
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
        float distance = 0;
        for (int i = 0; i < bodyPartsCount; i++)
        {
            Transform child = individual.transform.GetChild(i);
            distance = distance + child.transform.position.x;
        }
        distance = distance / bodyPartsCount;
        return distance;
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
            fitnessList.Add(CalculateFitness(_currentGeneration[i],distancesList[i])); //tutaj bede trzymac wagi
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
    private void AlgorithmStart()
    {
        SetDistancesList();
        SetFitnessList();
        SetElitiesIndexesList();
        bestDistance = distancesList.Max();
        bestFitness = fitnessList.Max();
        var bestFitnessIndex = fitnessList.IndexOf(bestFitness);
        for (int i = 0; i < _currentGeneration.Count; i++)
        {
            if (elitesIndexes.Contains(i))    //najlepszy zostaje
            {
                _populationGenPool[i] = new AnimalBrain(_maxPercentGenesToMutate);
                _populationGenPool[i].deepCopy(_currentGeneration[i].animalBrain);
                _populationGenPool[i].isElite=true;
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
        int border=Random.Range(0,fitnessList.Count);
        int count=Random.Range(1,fitnessList.Count-border);
        float parentFitness=fitnessList.GetRange(border,count).Max();
        return fitnessList.IndexOf(parentFitness);
    }
    public AnimalBrain Mate()
    {
        //krzyzowanie polega na "konkursie" wynialajacym dwoch rodzicow na jednego osobnika potomnego
        //osobniki o wiekszym fitness maja wieksza szanse na zostanie wybranym do krzyzowania
        //losowana jest wartosc pomiedzy minimalna wartoscia z fitness list a maksymalna wartoscia z fitness list
        //losowany index osobnika dopoki wylosowana wczesniej wartosc nie jest mniejsza od wartosci fitness danego indexu
        //losowani dwaj rodzice, rozmnazanie jak wczesniej
        int parent1Index = ChooseParent();
        int parent2Index = ChooseParent();
        while(parent2Index==parent1Index)
        {
            parent2Index = ChooseParent();
        }
        AnimalBrain child = new AnimalBrain(_maxPercentGenesToMutate);
        float mixChance;
        child.deepCopy(_currentGeneration[parent1Index].animalBrain);
        mixChance = Random.Range(0.0f, 100.0f);
        child.mixWeights(_currentGeneration[parent2Index].animalBrain, mixChance);
        float chance = Random.Range(0.0f, 1.0f);
        if (chance < _mutationRate) //obsluga mutacji - mutacja obejmuje zmiane indexu genu z losowym innym genem
        {
            child.mutateWeights();
        }
        return child;
    }

}

