using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GeneticAlgorithm
{
    private List<AnimalMovement> currentGeneration;   //list of animals
    private AnimalBrain[] populationGenPool;
    private List<AnimalBrain> parentsList;
    private float mutationRate;
    private int numberOfElites;
    public float bestDistance { get; private set; }
    private float elitionismPercent = 0.05f;
    List<float> fitnessList;
    List<int> elitesIndexes;
    const int minFitness = -999;
    float penaltyForCrash=1;
    public GeneticAlgorithm(List<AnimalMovement> gen, float mutationRate)
    {
        currentGeneration = gen;
        this.mutationRate = mutationRate;
        populationGenPool = new AnimalBrain[currentGeneration.Count()];
        numberOfElites = (int)Mathf.Ceil(currentGeneration.Count() * elitionismPercent);
        AlgorithmStart();
    }
    float calculateFitness(AnimalMovement individual)
    {
        // var individualBody = individual.transform.Find("body").gameObject;
        // sprawdzam wszystkie czesci ciala, dziele przez ilosc czesci ciala, tak otrzymuje jak daleko doszly i fitness (koniec z wyrzucaniem body d przodu)
        int bodyPartsCount = individual.transform.childCount;
        float fitness = 0;
        for (int i = 0; i < bodyPartsCount; i++)
        {
            Transform child = individual.transform.GetChild(i);
            fitness=fitness+child.transform.position.x;
        }
        fitness=fitness/bodyPartsCount;
        //fitness=fitness*individual.averageBodyY;
        fitness=fitness+individual.timeBeingAlive;
        if(individual.ifCrashed==true)
        {
            fitness=fitness*penaltyForCrash;   //jesli upadnie, nieznacznie zmniejszam fitness
        }
        return fitness;
    }
    public List<HingeArmPart> OrderAnimalChildren(GameObject obj)
    {
        HingeArmPart[] temp = obj.GetComponentsInChildren<HingeArmPart>();
        HingeArmPart[] orderedHingeParts = new HingeArmPart[temp.Length];
        int index = 0;
        int noOfChildren = obj.transform.childCount;
        for (int i = 0; i < noOfChildren; i++)
        {
            HingeArmPart childComponent = obj.transform.GetChild(i).GetComponent<HingeArmPart>();
            if (childComponent != null)
            {
                orderedHingeParts[index] = childComponent;
                index++;
            }
        }
        return orderedHingeParts.ToList();
    }
    public List<AnimalBrain> GetPopulationGenPool()
    {
        return populationGenPool.ToList();
    }
    private void SetFitnessList()
    {
        fitnessList = new List<float>();
        for (int i = 0; i < currentGeneration.Count; i++)
        {
            fitnessList.Add(calculateFitness(currentGeneration[i])); //tutaj bede trzymac wagi
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
        SetFitnessList();
        SetElitiesIndexesList();
        bestDistance = fitnessList.Max();
        var bestDistanceIndex = fitnessList.IndexOf(bestDistance);
        if(currentGeneration[bestDistanceIndex].ifCrashed)      //wlasciwe pokazywanie w GUI
        {
            bestDistance=bestDistance/penaltyForCrash;
        }
        bestDistance=bestDistance-currentGeneration[bestDistanceIndex].timeBeingAlive;
        
        //bestDistance=bestDistance/currentGeneration[bestDistanceIndex].averageBodyY;    //wlasciwe pokazywanie w GUI
        for (int i = 0; i < currentGeneration.Count; i++)
        {
            if (elitesIndexes.Contains(i))    //najlepszy zostaje
            {
                populationGenPool[i] = new AnimalBrain();
                populationGenPool[i].deepCopy(currentGeneration[i].animalBrain);
            }
            else
            {
                populationGenPool[i] = Mate();
            }
        }
    }
    private int ChooseParent()
    {
        int parentIndex = Random.Range(0, currentGeneration.Count);
        float worstFitness = fitnessList.Min();
        float bestFitness = fitnessList.Max();
        float randomFitnessNumber = Random.Range(worstFitness, bestFitness);
        while (fitnessList[parentIndex] < randomFitnessNumber)
        {
            parentIndex = Random.Range(0, currentGeneration.Count);
        }
        return parentIndex;
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
        AnimalBrain child = new AnimalBrain();
        float mixChance;
        child.deepCopy(currentGeneration[parent1Index].animalBrain);
        mixChance = Random.Range(0.0f, 100.0f);
        child.mixWeights(currentGeneration[parent2Index].animalBrain, mixChance);
        float chance = Random.Range(0.0f, 1.0f);
        if (chance < mutationRate) //obsluga mutacji - mutacja obejmuje zmiane indexu genu z losowym innym genem
        {
            child.mutateWeights();
        }
        return child;
    }

}

