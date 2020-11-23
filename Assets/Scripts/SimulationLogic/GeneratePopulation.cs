using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
public struct PopulationInputData
{
    public static bool migrationEnabled;
    public static int populationSize;
    public static int populationPartSize;
    public static float weightsMutationRate;
    public static string animalPrefabName;
    public static float startingPosition;
    public static float speed;
    public static float physicalMutationRate;
    public static float timeBelowAveragePenalty;
}
public class GeneratePopulation : MonoBehaviour
{

    public float secondsPassed = 0.0f;
    public List<AnimalData> migratedAnimals;
    public string bestAnimalDataJson;
    AnimalData bestAnimalData;
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
    public float timeBelowAveragePenalty;
    public List<float> bestDistances;
    public List<float> bestFitnesses;
    private GeneticAlgorithm geneticAlgorithm;
    private int[] activeAnimalIndexes;
    private bool _newGenerationMove;
    PopulationUI populationUIhandler;
    int _migratedAnimalsLeftToPick = 0;
    int _migratedAnimalIndexToPick = 0;
    int _animalDivision = 1;
    float _chanceToMigrate = 0;
    int _iterationsToResetMGenes;
    List<float> _currentMGenes;
    void Start()
    {
        populationSize = PopulationInputData.populationSize;
        populationPartSize = PopulationInputData.populationPartSize;
        mutationRate = PopulationInputData.weightsMutationRate;
        animalPrefabName = PopulationInputData.animalPrefabName;
        startingPosition = PopulationInputData.startingPosition;
        speed = PopulationInputData.speed;
        physicalMutationRate = PopulationInputData.physicalMutationRate;
        timeBelowAveragePenalty = PopulationInputData.timeBelowAveragePenalty;
        _newGenerationMove = true;
        _currentMGenes = new List<float>();
        bestDistances = new List<float>();
        activeAnimalIndexes = new int[populationPartSize];
        animalsObjects = new List<GameObject>();
        animals = new List<AnimalMovement>();
        var tempObject = Instantiate(Resources.Load("Prefabs/" + animalPrefabName) as GameObject);
        var movingParts = tempObject.GetComponentsInChildren<JointHandler>();
        AnimalBrain.noMovingParts = movingParts.Length;
        AnimalBrain.outputSize = AnimalBrain.outputPerArm * AnimalBrain.noMovingParts + AnimalBrain.armsToMoveCount;
        _iterationsToResetMGenes = (int)Mathf.Ceil(Mathf.Log(populationSize, 2));
        Destroy(tempObject);
        if (PopulationInputData.migrationEnabled)
        {
            LoadMigratedIndividuals();
            _migratedAnimalsLeftToPick = migratedAnimals.Count;
            _chanceToMigrate = Random.Range(0.0f, 100.0f);
        }
        CreateGeneration();
        populationUIhandler = transform.Find("infoCanvas").GetComponent<PopulationUI>();
    }
    void LoadMigratedIndividuals()
    {
        List<string> migratedAnimalDataJsons = DatabaseHandler.ReturnAnimalDataJsonArray(animalPrefabName);
        migratedAnimals = new List<AnimalData>();
        if (migratedAnimalDataJsons.Count > 0)
        {
            foreach (var json in migratedAnimalDataJsons)
            {
                AnimalData migratedData = JsonConvert.DeserializeObject<AnimalData>(json);
                migratedData.animalBrain.DeserializeWeights();
                migratedAnimals.Add(migratedData);
            }
        }
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
        animalComponent.currentX = -startingPosition;
        if (currentGen > 0)
        {
            animalComponent.SetBody(individualData.isElite, false);
            animalComponent.SetAnimalData(individualData);
        }
        else
        {
            if (_migratedAnimalsLeftToPick > 0)
            {
                float chanceForRandomData = 100.0f - ((float)_animalDivision) / ((float)populationSize / (float)migratedAnimals.Count) * 100.0f;
                if (_chanceToMigrate > chanceForRandomData)
                {
                    animalComponent.SetBody(false, true);
                    _chanceToMigrate = Random.Range(0.0f, 100.0f);
                    animalComponent.SetAnimalData(migratedAnimals[_migratedAnimalIndexToPick]);
                    _animalDivision = 1;
                    _migratedAnimalsLeftToPick--;
                    _migratedAnimalIndexToPick++;
                }
                else
                {
                    animalComponent.SetBody(false, false);
                    animalComponent.SetRandomData();
                }
            }
            else
            {
                animalComponent.SetBody(false, false);
                animalComponent.SetRandomData();
            }
        }
        if (currentGen % _iterationsToResetMGenes == 0)
        {
            float randomGene = Random.value;
            while(_currentMGenes.Contains(randomGene))
            {
                randomGene = Random.value;
            }
            animalComponent.animalData.animalBrain.mGene = randomGene;
        }
        animalComponent.SetBodyPartsStartingX();
        animalsObjects.Add(tempObject);
        animals.Add(animalComponent);
        _animalDivision++;
    }
    void TrimGeneration()
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
        bestAnimalDataJson = bestAnimalData.SerializeData();
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
            secondsPassed = secondsPassed + Time.fixedDeltaTime;
            if (animalsObjectsCatched == populationSize)
            {
                if (currentGen % _iterationsToResetMGenes == 0)
                {
                    _currentMGenes = new List<float>();
                }
                _newGenerationMove = false;
                currentGen++;
                geneticAlgorithm = new GeneticAlgorithm(animals);
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
                    bestAnimalData = geneticAlgorithm.bestAnimalData.DeepCopy();
                }
                CreateGeneration();
                TrimGeneration();
                animalsObjectsCatched = 0;
                StartCoroutine(CreateNewGeneration());
            }
            else if (_newGenerationMove)
            {
                for (int i = 0; i < populationPartSize; i++)
                {
                    animals[activeAnimalIndexes[i]].SelectLimbsToChangeState();
                }
                for (int i = 0; i < populationPartSize; i++)
                {
                    animals[activeAnimalIndexes[i]].FinishJob();
                    animals[activeAnimalIndexes[i]].MoveSelectedLimbs();
                }
                // for (int i = 0; i < populationPartSize; i++)     
                // {
                //     animals[activeAnimalIndexes[i]].FinishJob();
                //     animals[activeAnimalIndexes[i]].MoveSelectedLimbs();
                // }
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
