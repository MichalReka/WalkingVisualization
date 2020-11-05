using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading;
using System.Threading.Tasks;
public struct PopulationInputData
{
    public static int populationSize;
    public static int populationPartSize;
    public static float weightsMutationRate;
    public static string animalPrefabName;
    public static float startingPosition;
    public static float speed;
    public static float physicalMutationRate;
    public static float timeBeingAliveImportance;
}
public class GeneratePopulation : MonoBehaviour
{
    private int animalsObjectsCatched = 0;
    List<GameObject> animalsObjects;
    List<AnimalMovement> animals;
    public int currentGen = 0;
    public float bestDistance = 0;
    public float bestFitness = 0;
    public float currBestDistance = 0;
    public float currBestFitness = 0;
    public int populationSize;
    public int populationPartSize;
    public float mutationRate;
    public string animalPrefabName;
    public float startingPosition;
    public float speed;
    public float physicalMutationRate;
    public float timeBeingAliveImportance;
    public List<float> bestDistances;
    public List<float> bestFitnesses;
    private GeneticAlgorithm geneticAlgorithm;
    private int[] activeAnimalIndexes;
    private bool _newGenerationMove;
    PopulationUI populationUIhandler;
    void Start()
    {
        populationSize=PopulationInputData.populationSize;
        populationPartSize=PopulationInputData.populationPartSize;
        mutationRate=PopulationInputData.weightsMutationRate;
        animalPrefabName=PopulationInputData.animalPrefabName;
        startingPosition=PopulationInputData.startingPosition;
        speed=PopulationInputData.speed;
        physicalMutationRate=PopulationInputData.physicalMutationRate;
        timeBeingAliveImportance=PopulationInputData.timeBeingAliveImportance;
        _newGenerationMove = true;
        bestDistances = new List<float>();
        activeAnimalIndexes = new int[populationPartSize];
        animalsObjects = new List<GameObject>();
        animals = new List<AnimalMovement>();
        var tempObject = Instantiate(Resources.Load("Prefabs/" + animalPrefabName) as GameObject);
        var movingParts = tempObject.GetComponentsInChildren<JointHandler>();
        AnimalBrain.noMovingParts = movingParts.Length;
        AnimalBrain.outputSize  = AnimalBrain.outputPerArm * AnimalBrain.noMovingParts + AnimalBrain.armsToMoveCount;
        Destroy(tempObject);
        CreateGeneration();
        populationUIhandler = transform.Find("infoCanvas").GetComponent<PopulationUI>();
    }
    void CreateGeneration()
    {
        for (int i = 0; i < populationPartSize; i++)
        {
            if (currentGen > 0)
            {

                CreateAnimal(new Vector3(0, 0, 15 * i + transform.position.z), geneticAlgorithm.GetPopulationGenPool()[i]);
            }
            else
            {
                CreateAnimal(new Vector3(0, 0, 15 * i + transform.position.z));
            }
            activeAnimalIndexes[i] = i;
        }
    }
    
    void CreateAnimal(Vector3 position, AnimalData individualData = null)
    {
        var tempObject = Instantiate(Resources.Load("Prefabs/" + animalPrefabName) as GameObject);
        tempObject.transform.SetPositionAndRotation(position, new Quaternion(0, 0, 0, 0));
        tempObject.transform.SetParent(transform);
        var animalComponent = tempObject.AddComponent<AnimalMovement>();
        animalComponent.speed = speed;
        animalComponent.timeBeingAliveImportance = timeBeingAliveImportance;
        animalComponent.currentX = -startingPosition;
        if (currentGen > 0)
        {
            animalComponent.SetBody(individualData.animalBrain.isElite);
            animalComponent.SetAnimalData(individualData);
        }
        else
        {
            animalComponent.SetBody(false);
            animalComponent.SetRandomData();
        }
        animalComponent.SetBodyPartsStartingX();
        animalsObjects.Add(tempObject);
        animals.Add(animalComponent);
    }
    void trimGeneration()
    {
        for (int i = 0; i < populationSize; i++)
        {
            animals[0].Destroy();
            Destroy(animalsObjects[0]);
            animalsObjects.RemoveAt(0);
            animals.RemoveAt(0);
        }
    }
    private void GenerateJson()
    {
        string json = JsonUtility.ToJson(this);
        System.IO.File.WriteAllText(DatabaseHandler.jsonPath, json);
    }
    public void AddDataToTable()
    {
        GenerateJson();
        DatabaseHandler.AddDataToTable();
    }

    public void GeneratePdfDataPresentation()
    {
        GenerateJson();
        //uruchamianie skryptu python
        string strCmdText;
        //strCmdText = "/C py ./presentData.py&pause";
        strCmdText = "/C py ./presentData.py";
        System.Diagnostics.Process.Start("CMD.exe", strCmdText);
    }
    // Update is called once per frame
    IEnumerator CreateNewGeneration()
    {
        yield return new WaitForSeconds(0.5f);
        _newGenerationMove = true;
    }
    void FixedUpdate()
    {
        if (!VisualizationBasics.ifPaused)
        {
            if (animalsObjectsCatched == populationSize)
            {
                _newGenerationMove = false;
                currentGen++;
                geneticAlgorithm = new GeneticAlgorithm(animals, mutationRate, physicalMutationRate);
                currBestDistance = geneticAlgorithm.bestDistance;
                currBestFitness = geneticAlgorithm.bestFitness;
                bestDistances.Add(currBestDistance);
                bestFitnesses.Add(currBestFitness);
                if (currBestDistance > bestDistance)
                {
                    bestDistance = currBestDistance;
                }
                if (currBestFitness > bestFitness)
                {
                    bestFitness = currBestFitness;
                }
                CreateGeneration();
                trimGeneration();
                animalsObjectsCatched = 0;
                StartCoroutine(CreateNewGeneration());
            }
            else if (_newGenerationMove)
            {
                for (int i = 0; i < populationPartSize; i++)     
                {
                    animals[activeAnimalIndexes[i]].StartArmsToMoveJob();
                }
                for (int i = 0; i < populationPartSize; i++)     
                {
                    animals[activeAnimalIndexes[i]].FinishJob();
                    animals[activeAnimalIndexes[i]].StartMovementJob();
                }
                for (int i = 0; i < populationPartSize; i++)
                {
                    animals[activeAnimalIndexes[i]].FinishJob();
                    animals[activeAnimalIndexes[i]].UpdateIO();
                    animals[activeAnimalIndexes[i]].Chase();
                    bool ifCatched = animals[activeAnimalIndexes[i]].ifCatched;
                    if (ifCatched == true && animalsObjects[activeAnimalIndexes[i]].activeSelf)    //logike lapania zwierzeta implementuje w pliku animal movement
                    {
                        animalsObjectsCatched++;  
                        animalsObjects[activeAnimalIndexes[i]].SetActive(false);   //ustawiam to zwierze jako nieaktywne
                        if (animalsObjects.Count != populationSize)
                        {
                            activeAnimalIndexes[i] = animalsObjects.Count;   //wprowadzam do tablicy indeksow nowe zwierze - bede sie do niego odwolywac przy nastepnym sprawdzeniu
                            if (currentGen > 0) //jesli zlapie zwierze, tworze nowe
                            {
                                CreateAnimal(new Vector3(0, 0, 15 * i + transform.position.z), geneticAlgorithm.GetPopulationGenPool()[activeAnimalIndexes[i]]);
                            }
                            else
                            {
                                CreateAnimal(new Vector3(0, 0, 15 * i + transform.position.z));
                            }
                        }
                    }
                }
            }
            populationUIhandler.UpdateUI(currentGen, animalsObjectsCatched, currBestDistance, bestDistance);
        }
    }

}
