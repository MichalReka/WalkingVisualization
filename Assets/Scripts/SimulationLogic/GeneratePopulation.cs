using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
public struct PopulationInputData
{
    public static bool migrationEnabled;
    public static bool adaptationEnabled;
    public static int populationSize;
    public static int populationPartSize;
    public static float weightsMutationRate;
    public static string animalPrefabName;
    public static float startingPosition;
    public static float speed;
    public static float physicalMutationRate;
    public static float timeBelowAveragePenalty;
    public static float tournamentSize;
    public static float crossoverPercent;
    public static bool forceSimilarPartsSynchronization=false;
}
public class GeneratePopulation : MonoBehaviour
{
    public static GameObject modelAnimal;
    public float secondsPassed = 0.0f;
    public List<AnimalData> migratedAnimals;
    public string bestAnimalDataJson;
    AnimalData bestAnimalData;
    private int animalsObjectsCatched = 0;
    List<GameObject> animalsObjects;
    List<AnimalMovement> animals;
    public int currentGen = 0;
    public bool forceSynchronization = false;
    public float bestDistance = 0;
    public float bestFitness = 0;
    public float currBestDistance = 0;
    public float currBestFitness = 0;
    public float currAverageDistance = 0;
    public float currAverageFitness = 0;
    public int populationSize;
    public int populationPartSize;
    public string animalPrefabName;
    public float startingPosition;
    public float speed;
    public float mutationRate;
    public float physicalMutationRate;
    public float timeBelowAveragePenalty;
    public List<float> bestDistances;
    public List<float> bestFitnesses;
    public List<float> averageDistances;
    public List<float> averageFitnesses;
    private GeneticAlgorithm _geneticAlgorithm;
    private int[] _activeAnimalIndexes;
    private bool _newGenerationMove;
    PopulationUI _populationUIhandler;
    int _migratedAnimalsLeftToPick = 0;
    int _migratedAnimalIndexToPick = 0;
    int _animalDivision = 1;
    float _chanceToMigrate = 0;
    int _iterationsToResetMGenes;
    List<float> _currentMGenes;
    List<int> animalsToActivate;

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
        animalsToActivate = new List<int>();
        bestDistances = new List<float>();
        bestFitnesses = new List<float>();
        averageDistances = new List<float>();
        averageFitnesses = new List<float>();
        _activeAnimalIndexes = new int[populationPartSize];
        animalsObjects = new List<GameObject>();
        animals = new List<AnimalMovement>();
        modelAnimal = Instantiate(Resources.Load("Prefabs/" + animalPrefabName) as GameObject);
        var movingParts = modelAnimal.GetComponentsInChildren<JointHandler>();
        AnimalBrain.noMovingParts = movingParts.Length;
        // AnimalBrain.outputSize = AnimalBrain.outputPerArm * AnimalBrain.noMovingParts + AnimalBrain.armsToMoveCount;
        AnimalBrain.outputSize = AnimalBrain.outputPerArm * AnimalBrain.noMovingParts;
        _iterationsToResetMGenes = (int)Mathf.Ceil(Mathf.Log(populationSize, 2));
        modelAnimal.SetActive(false);
        if (PopulationInputData.migrationEnabled)
        {
            LoadMigratedIndividuals();
            _migratedAnimalsLeftToPick = migratedAnimals.Count;
            _chanceToMigrate = Random.Range(0.0f, 100.0f);
        }
        CreateGeneration();
        _geneticAlgorithm = new GeneticAlgorithm(animals);
        _populationUIhandler = transform.Find("infoCanvas").GetComponent<PopulationUI>();
        StartCoroutine("MoveGenerationBatch");
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
    void EnableGenerationFirstBatch()
    {
        for (int i = 0; i < populationPartSize; i++)
        {
            animalsObjects[i].SetActive(true);
            animalsToActivate.RemoveAt(0);
            _activeAnimalIndexes[i] = i;
        }
    }
    void CreateGeneration()
    {
        for (int i = 0; i < populationSize; i++)
        {
            CreateAnimal(new Vector3(0, 0, 15 * (i % populationPartSize) + transform.position.z));
            animalsToActivate.Add(i);
        }
        EnableGenerationFirstBatch();
    }

    void CreateAnimal(Vector3 position, AnimalData individualData = null)
    {
        var tempObject = Instantiate(Resources.Load("Prefabs/" + animalPrefabName) as GameObject);
        tempObject.transform.SetPositionAndRotation(position, new Quaternion(0, 0, 0, 0));
        tempObject.transform.SetParent(transform);
        var animalComponent = tempObject.AddComponent<AnimalMovement>();
        animalComponent.speed = speed;
        animalComponent.currentX = -startingPosition;
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

        if (currentGen % _iterationsToResetMGenes == 0)
        {
            float randomGene = Random.value;
            while (_currentMGenes.Contains(randomGene))
            {
                randomGene = Random.value;
            }
            animalComponent.animalData.animalBrain.mGene = randomGene;
        }
        animalComponent.SetBodyPartsStartingX();
        animalsObjects.Add(tempObject);
        animals.Add(animalComponent);
        tempObject.SetActive(false);
        _animalDivision++;
    }
    void ResetGeneration(List<AnimalData> populationGenPool)
    {
        for (int i = 0; i < populationSize; i++)
        {
            animals[i].ResetAnimal(-startingPosition);
            animals[i].SetAnimalData(populationGenPool[i]);
            animals[i].SetBodyPartsStartingX();
            animalsToActivate.Add(i);
            // animals[0].Destroy();
            // Destroy(animalsObjects[0]);
            // animalsObjects.RemoveAt(0);
            // animals.RemoveAt(0);
        }
        EnableGenerationFirstBatch();
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
        yield return new WaitForSeconds(1f);
        _newGenerationMove = true;
    }
    IEnumerator MoveGenerationBatch()
    {
        int numberOfBatches = 5;        //Fixed Update - 50 fps - w 5 klatkach wszystkie sieci neuronowe juz obliczyly wartosci - 10 razy na sekunde wszystkie osobniki moga zmienic kierunek chodzenia
        int currentBatch = 0;
        int batchesLeftToCompute = 4;
        int batchSize = populationPartSize / numberOfBatches;
        while (true)
        {
            if (animalsObjectsCatched < populationSize && !VisualizationBasics.ifPaused)
            {
                int iterationStart = batchSize * currentBatch;
                int iterationEnd = populationPartSize - (batchesLeftToCompute * batchSize);
                // for (int i = iterationStart; i < iterationEnd; i++)
                // {
                //     animals[_activeAnimalIndexes[i]].SelectLimbsToChangeState();
                // }
                for (int i = iterationStart; i < iterationEnd; i++)
                {
                    // animals[_activeAnimalIndexes[i]].FinishJob();
                    animals[_activeAnimalIndexes[i]].MoveSelectedLimbs();
                }
                for (int i = iterationStart; i < iterationEnd; i++)
                {
                    animals[_activeAnimalIndexes[i]].FinishJob();
                    animals[_activeAnimalIndexes[i]].UpdateIO();
                    //animals[_activeAnimalIndexes[i]].Chase();
                    bool ifCatched = animals[_activeAnimalIndexes[i]].ifCatched;
                    if (ifCatched == true && animalsObjects[_activeAnimalIndexes[i]].activeSelf)    //logike lapania zwierzeta implementuje w pliku animal movement
                    {
                        animalsObjectsCatched++;
                        animalsObjects[_activeAnimalIndexes[i]].SetActive(false);   //ustawiam to zwierze jako nieaktywne
                        if (animalsToActivate.Count != 0)
                        {
                            animalsObjects[animalsToActivate[0]].transform.position = animalsObjects[_activeAnimalIndexes[i]].transform.position;
                            _activeAnimalIndexes[i] = animalsToActivate[0];   //wprowadzam do tablicy indeksow nowe zwierze - bede sie do niego odwolywac przy nastepnym sprawdzeniu
                            animalsToActivate.RemoveAt(0);
                            animalsObjects[_activeAnimalIndexes[i]].SetActive(true);
                            // if (currentGen > 0) //jesli zlapie zwierze, tworze nowe
                            // {
                            //     CreateAnimal(new Vector3(0, 0, 15 * i + transform.position.z), _geneticAlgorithm.GetPopulationGenPool()[_activeAnimalIndexes[i]]);
                            // }
                            // else
                            // {
                            //     CreateAnimal(new Vector3(0, 0, 15 * i + transform.position.z));
                            // }
                        }
                        _populationUIhandler.UpdateUI(currentGen, animalsObjectsCatched, currBestDistance, bestDistance);
                    }
                }
                currentBatch++;
                batchesLeftToCompute--;
                if (batchesLeftToCompute < 0)
                {
                    currentBatch = 0;
                    batchesLeftToCompute = 4;
                }
            }
            else
            {
                currentBatch = 0;
                batchesLeftToCompute = 4;
            }
            yield return new WaitForFixedUpdate();
        }
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
                _geneticAlgorithm.AlgorithmStart();
                currBestDistance = _geneticAlgorithm.bestDistance;
                currBestFitness = _geneticAlgorithm.bestFitness;
                currAverageDistance = _geneticAlgorithm.distanceAverage;
                currAverageFitness = _geneticAlgorithm.fitnessAverage;
                bestDistances.Add(currBestDistance);
                bestFitnesses.Add(currBestFitness);
                averageDistances.Add(currAverageDistance);
                averageFitnesses.Add(currAverageFitness);
                if (currBestDistance > bestDistance)
                {
                    bestDistance = currBestDistance;
                }
                if (currBestFitness > bestFitness)
                {
                    bestFitness = currBestFitness;
                    bestAnimalData = _geneticAlgorithm.bestAnimalData.DeepCopy();
                }
                ResetGeneration(_geneticAlgorithm.GetPopulationGenPool());
                // CreateGeneration();
                animalsObjectsCatched = 0;
                _populationUIhandler.UpdateUI(currentGen, animalsObjectsCatched, currBestDistance, bestDistance);
                // StartCoroutine(CreateNewGeneration());
            }
            // else //if (_newGenerationMove)
            // {

            // }
        }
    }

}
