﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
public class GeneratePopulation : MonoBehaviour
{
    public int animalsObjectsCatched = 0;
    List<GameObject> animalsObjects;
    List<AnimalMovement> animals;
    public int currentPart = 0;
    public int populationSize = 100;
    public int populationPartSize = 50;
    public int maxGenNum = 5000;
    public float mutationRate = 0.05f;
    public int currentGen = 0;
    private GameObject bestSolution;
    public float bestDistance = 0;
    public float currBestDistance = 0;
    public string animalPrefabName = "animal0";
    public float startingPosition = 5.0f;
    public float speed = 2f;
    private int currAnimalIndex = 0;
    public static System.Random rnd = new System.Random();
    // Start is called before the first frame update
    public GeneticAlgorithm geneticAlgorithm;

    public int[] activeAnimalIndexes;
    void Start()
    {
        activeAnimalIndexes = new int[populationPartSize];
        animalsObjects = new List<GameObject>();
        animals = new List<AnimalMovement>();
        var tempObject = Instantiate(Resources.Load("Prefabs/" + animalPrefabName) as GameObject);
        Destroy(tempObject);
        createGeneration();
    }
    void createGeneration()
    {
        for (int i = 0; i < populationPartSize; i++)
        {
            if (currentGen > 0)
            {
                createAnimal(new Vector3(0, 0, 15 * i + transform.position.z), geneticAlgorithm.GetPopulationGenPool()[i]);
            }
            else
            {
                createAnimal(new Vector3(0, 0, 15 * i + transform.position.z));
            }
            activeAnimalIndexes[i] = i;
        }
    }
    void createAnimal(Vector3 position, List<HingeArmPart> individualChromosome = null)
    {
        var tempObject = Instantiate(Resources.Load("Prefabs/" + animalPrefabName) as GameObject);
        tempObject.transform.SetPositionAndRotation(position, new Quaternion(0, 0, 0, 0));
        tempObject.transform.SetParent(transform);
        var animalComponent = tempObject.AddComponent<AnimalMovement>();
        if (currentGen > 0)
        {
            animalComponent.setHingeParts(individualChromosome);
        }
        else
        {
            animalComponent.setRandomWeights();
        }
        animalComponent.speed = speed;
        animalComponent.currentX = -startingPosition;
        animalComponent.setBody();
        animalsObjects.Add(tempObject);
        animals.Add(animalComponent);
    }
    void trimGeneration()
    {
        for (int i = 0; i < populationSize; i++)
        {
            Destroy(animalsObjects[0]);
            animalsObjects.RemoveAt(0);
            animals.RemoveAt(0);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (currentGen <= maxGenNum)
        {
            if (animalsObjectsCatched == populationSize)
            {
                animalsObjectsCatched = 0;
                currentGen++;
                geneticAlgorithm = new GeneticAlgorithm(animalsObjects, mutationRate);
                currBestDistance = geneticAlgorithm.bestDistance;
                if (currBestDistance > bestDistance)
                {
                    bestDistance = geneticAlgorithm.bestDistance;
                }
                createGeneration();
                trimGeneration();
            }
            Parallel.For(0, populationPartSize, delegate (int i)
            {
                animals[activeAnimalIndexes[i]].Move();
            });
            for (int i = 0; i < populationPartSize; i++)
            {
                animals[activeAnimalIndexes[i]].UpdateInput();
                animals[activeAnimalIndexes[i]].chase();
                bool ifCatched = animals[activeAnimalIndexes[i]].ifCatched;
                if (ifCatched == true && animalsObjects[activeAnimalIndexes[i]].activeSelf)    //logike lapania zwierzeta implementuje w pliku animal movement
                {
                    animalsObjects[activeAnimalIndexes[i]].SetActive(false);   //ustawiam to zwierze jako nieaktywne
                    if (animalsObjects.Count != populationSize)
                    {
                        activeAnimalIndexes[i] = animalsObjects.Count;   //wprowadzam do tablicy indeksow nowe zwierze - bede sie do niego odwolywac przy nastepnym sprawdzeniu
                        if (currentGen > 0) //jesli zlapie zwierze, tworze nowe
                        {
                            createAnimal(new Vector3(0, 0, 15 * i + transform.position.z), geneticAlgorithm.GetPopulationGenPool()[activeAnimalIndexes[i]]);
                        }
                        else
                        {
                            createAnimal(new Vector3(0, 0, 15 * i + transform.position.z));
                        }
                    }
                    animalsObjectsCatched++;
                }
            }

        }
    }
}
