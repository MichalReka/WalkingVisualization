using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading;
using System.Threading.Tasks;
public class GeneratePopulation : MonoBehaviour
{
    private int animalsObjectsCatched=0;
    List<GameObject> animalsObjects;
    List<AnimalMovement> animals;
    public int currentGen=0;
    public float bestDistance=0;
    public float bestFitness=0;
    public float currBestDistance=0;
    public float currBestFitness=0;
    public static int populationSize;
    public static int populationPartSize;
    public static float mutationRate;
    public static string animalPrefabName;
    public static float startingPosition;
    public static float speed;
    public static float maxPercentGenesToMutate;
    public static float timeBeingAliveImportance;
    public List<float> bestDistances;
    public List<float> bestFitnesses;
    private GeneticAlgorithm geneticAlgorithm;
    private int[] activeAnimalIndexes;
    private bool _newGenerationMove;
    PopulationUI populationUIhandler;
    void Start()
    {
        _newGenerationMove=true;
        bestDistances = new List<float>();
        activeAnimalIndexes = new int[populationPartSize];
        animalsObjects = new List<GameObject>();
        animals = new List<AnimalMovement>();
        var tempObject = Instantiate(Resources.Load("Prefabs/" + animalPrefabName) as GameObject);
        var movingParts = tempObject.GetComponentsInChildren<JointHandler>();
        AnimalBrain.noMovingParts = movingParts.Length;
        Destroy(tempObject);
        createGeneration();
        populationUIhandler = transform.Find("infoCanvas").GetComponent<PopulationUI>();
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
    void createAnimal(Vector3 position, AnimalBrain individualBrain = null)
    {
        var tempObject = Instantiate(Resources.Load("Prefabs/" + animalPrefabName) as GameObject);
        tempObject.transform.SetPositionAndRotation(position, new Quaternion(0, 0, 0, 0));
        tempObject.transform.SetParent(transform);
        var animalComponent = tempObject.AddComponent<AnimalMovement>();
        animalComponent.speed = speed;
        animalComponent.timeBeingAliveImportance = timeBeingAliveImportance;
        animalComponent.currentX = -startingPosition;
        animalComponent.maxPercentGenesToMutate=maxPercentGenesToMutate;
        if (currentGen > 0)
        {
            animalComponent.setBody(individualBrain.isElite);
            animalComponent.setNeuralNetwork(individualBrain);
        }
        else
        {
            animalComponent.setBody(false);
            animalComponent.setRandomWeights();
        }

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
    public void ComeBackToMainMenu()
    {
        //var coroutineHandler=FadeInAndDo(()=>{SceneManager.LoadScene("walking");});
        //StartCoroutine(coroutineHandler);
    }
    IEnumerator FadeInAndDo(Action toDoAfterFade=null)  //uzycie action pozwala na zrobienie czegos po animacji
    {
        var coroutineHandler=FadeOutHandler.FadeOut(GameObject.Find("FindInOutImage").GetComponent<Image>());
        yield return StartCoroutine(coroutineHandler);
        toDoAfterFade.Invoke();
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
        _newGenerationMove=true;   
    }
    void FixedUpdate()
    {
        if (!VisualizationBasics.ifPaused)
        {
            if (animalsObjectsCatched == populationSize)
            {
                _newGenerationMove=false;
                currentGen++;
                geneticAlgorithm = new GeneticAlgorithm(animals, mutationRate,maxPercentGenesToMutate);
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
                createGeneration();
                trimGeneration();
                animalsObjectsCatched = 0;
                StartCoroutine(CreateNewGeneration());
            }
            else if(_newGenerationMove)
            {
                // Parallel.For(0, populationPartSize, delegate (int i)
                // {
                //     animals[activeAnimalIndexes[i]].Move();
                // });
                for (int i = 0; i < populationPartSize; i++)
                {
                    animals[activeAnimalIndexes[i]].Move();
                    animals[activeAnimalIndexes[i]].UpdateIO();
                    animals[activeAnimalIndexes[i]].chase();
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
                                createAnimal(new Vector3(0, 0, 15 * i + transform.position.z), geneticAlgorithm.GetPopulationGenPool()[activeAnimalIndexes[i]]);
                            }
                            else
                            {
                                createAnimal(new Vector3(0, 0, 15 * i + transform.position.z));
                            }
                        }
                    }
                }
            }
            populationUIhandler.UpdateUI(currentGen, animalsObjectsCatched, currBestDistance, bestDistance);
        }
    }

}
