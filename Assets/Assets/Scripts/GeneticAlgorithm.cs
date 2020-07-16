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
    private int chromosomeLength;
    private float mutationRate;
    private int numberOfParents;
    public float bestDistance{get;private set;}
    public GeneticAlgorithm(List<AnimalMovement> gen, float mutationRate)
    {
        currentGeneration = gen;
        this.mutationRate = mutationRate;
        var tempPartContainer=gen[0].GetComponentsInChildren<HingeArmPart>();
        chromosomeLength=tempPartContainer.Length;
        populationGenPool = new AnimalBrain[currentGeneration.Count()];
        numberOfParents=1+currentGeneration.Count()/30; //na 30 osobnikow kolejny rodzic
        AlgorithmStart();
    }
    float calculateFitness(AnimalMovement individual)
    {
        //chwilowo tylko cztery nogi
        //chwilowo tylko ustawiam jak daleko zaszedl osobnik - jesli jego tulow jest powyzej (na razie stalej - pozniej ustawie niezaleznie od gatunku) wartosci, mnoze 1.2
        var individualBody=individual.transform.Find("body").gameObject;
        var xPosition = individualBody.transform.position.x;
        var fitness=xPosition;
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
    private void AlgorithmStart()
    {
        const int minFitness = -999;
        float currentBestValue=0;
        int maxValueIndex = 0;
        List<float> fitnessList=new List<float>(currentGeneration.Count());
        for (int i=0;i<currentGeneration.Count();i++) fitnessList.Add(0);
        AnimalBrain currentBest; //najlepszy zostaje
        List<AnimalBrain> betaParents = new List<AnimalBrain>();
        for(int i=0;i<currentGeneration.Count;i++)
        {
            fitnessList[i] = calculateFitness(currentGeneration[i]); //tutaj bede trzymac wagi
            if(fitnessList[i]>currentBestValue) //nie musze dwa razy przechodzic przez cala liste
            {
                currentBestValue=fitnessList[i];
                maxValueIndex=i;
            }
        }
        bestDistance=currentBestValue;
        fitnessList[maxValueIndex] = minFitness;    //zamieniam najmniejsza w najwieksza - uzyskam tak prosto liste reszty 
        for(int i=0;i<numberOfParents;i++)
        {
            var parentFitness = fitnessList.Max();
            betaParents.Add(currentGeneration[fitnessList.IndexOf(parentFitness)].animalBrain);
            fitnessList[fitnessList.IndexOf(parentFitness)]=fitnessList.Min();
        }
        fitnessList[maxValueIndex] = currentBestValue;
        currentBest = currentGeneration[maxValueIndex].animalBrain;
        for (int i = 0; i < currentGeneration.Count; i++)
        {
            if(i==maxValueIndex)    //najlepszy zostaje
            {
                populationGenPool[i]=currentBest;
  
            }
            else
            {
                var chance=Random.Range(0,numberOfParents);
                populationGenPool[i] = Mate(currentBest, betaParents[chance],i);
            }
        }
    }
    //rozmnazanie - osobnik alfa i lista osobnikow beta, osobnik alfa na pewno przekaze czesc genow, ilosc osobnikow beta 
    public AnimalBrain Mate(AnimalBrain alphaParent, AnimalBrain betaParent,int generationIndex)  //trzeba przerobobic to by synapsy sie zmienialy, nie cale "nogi"
    {
        //nie tworzyc obiektow - stworzyc liste genow 
        //krzyzowanie polega na kopiowaniu genow z rodzica 1
        //losowanie szansy od 0 do 100 przekazania genow 2 rodzica
        //zmianie genow kopii 1 rodzica jesli lokalna szansa jest mniejsza od szansy 2 rodzica
        List<int> genesFromParent1 = new List<int>();
        List<int> genesFromParent2 = new List  <int>();
        AnimalBrain child=new AnimalBrain(alphaParent.noMovingParts);
        float mixChance;
        child.deepCopy(alphaParent);
        mixChance=Random.Range(0.0f,100.0f);
        child.mixWeights(betaParent,mixChance);
        float chance = Random.Range(0.0f,100.0f);
        chance = chance/100;
        if (chance < mutationRate) //obsluga mutacji - mutacja obejmuje zmiane indexu genu z losowym innym genem
        {
            child.mutateWeights();
        }
        return child;
    }
    
}

            