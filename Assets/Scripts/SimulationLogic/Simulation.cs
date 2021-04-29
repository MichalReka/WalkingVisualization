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
    public static bool forceSimilarPartsSynchronization = false;
}
public class Simulation : MonoBehaviour
{
    public static GameObject modelAnimal;
    public float secondsPassed = 0.0f;
    public List<AnimalData> migratedAnimals;
    public string bestAnimalDataJson;
    AnimalData bestAnimalData;
    private int _animalsObjectsCatched = 0;
    List<GameObject> _animalsObjects;
    List<Animal> _animals;
    public int currentGen = 0;
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
    private int[] _animalIndexesToMove;
    private bool _newGenerationMove;
    PopulationUI _populationUIhandler;
    int _migratedAnimalsLeftToPick = 0;
    int _migratedAnimalIndexToPick = 0;
    int _animalDivision = 1;
    float _chanceToMigrate = 0;
    int _iterationsToResetMGenes;
    List<float> _currentMGenes;
    List<int> __animalsToActivate;
    public static int numberOfBatches = 5;        //Fixed Update - 50 fps - w 5 klatkach wszystkie sieci neuronowe juz obliczyly wartosci - 10 razy na sekunde wszystkie osobniki moga zmienic kierunek chodzenia

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
        __animalsToActivate = new List<int>();
        bestDistances = new List<float>();
        bestFitnesses = new List<float>();
        averageDistances = new List<float>();
        averageFitnesses = new List<float>();
        _animalIndexesToMove = new int[populationPartSize];
        _animalsObjects = new List<GameObject>();
        _animals = new List<Animal>();
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
        _geneticAlgorithm = new GeneticAlgorithm(_animals);
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
            _animalsObjects[i].SetActive(true);
            __animalsToActivate.RemoveAt(0);
            _animalIndexesToMove[i] = i;
        }
    }
    void CreateGeneration()
    {
        for (int i = 0; i < populationSize; i++)
        {
            CreateAnimal(new Vector3(0, 0, 15 * (i % populationPartSize) + transform.position.z));
            __animalsToActivate.Add(i);
        }
        EnableGenerationFirstBatch();
    }

    void CreateAnimal(Vector3 position, AnimalData individualData = null)
    {
        var tempObject = Instantiate(Resources.Load("Prefabs/" + animalPrefabName) as GameObject);
        tempObject.transform.SetPositionAndRotation(position, new Quaternion(0, 0, 0, 0));
        tempObject.transform.SetParent(transform);
        var animalComponent = tempObject.AddComponent<Animal>();
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
        _animalsObjects.Add(tempObject);
        _animals.Add(animalComponent);
        tempObject.SetActive(false);
        _animalDivision++;
    }
    void ResetGeneration(List<AnimalData> populationGenPool)
    {
        for (int i = 0; i < populationSize; i++)
        {
            _animals[i].ResetAnimal(-startingPosition);
            _animals[i].SetAnimalData(populationGenPool[i]);
            _animals[i].SetBodyPartsStartingX();
            __animalsToActivate.Add(i);
            // _animals[0].Destroy();
            // Destroy(_animalsObjects[0]);
            // _animalsObjects.RemoveAt(0);
            // _animals.RemoveAt(0);
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
        int currentBatch = 0;
        int batchesLeftToCompute = numberOfBatches-1;
        int batchSize = populationPartSize / numberOfBatches;
        List<int> indexesToSkip = new List<int>();
        while (true)
        {
            if (_animalsObjectsCatched < populationSize && !VisualizationBasics.ifPaused)
            {
                int iterationStart = batchSize * currentBatch;
                int iterationEnd = populationPartSize - (batchesLeftToCompute * batchSize);
                // for (int i = iterationStart; i < iterationEnd; i++)
                // {
                //     _animals[_animalIndexesToMove[i]].SelectLimbsToChangeState();
                // }
                for (int i = iterationStart; i < iterationEnd; i++)
                {
                    // _animals[_animalIndexesToMove[i]].FinishJob();
                    if (_animalsObjects[_animalIndexesToMove[i]].activeSelf)
                    {
                        _animals[_animalIndexesToMove[i]].UpdateInput();
                        _animals[_animalIndexesToMove[i]].ComputeOutput();
                    }
                }
                for (int i = iterationStart; i < iterationEnd; i++)
                {
                    if (_animalsObjects[_animalIndexesToMove[i]].activeSelf)
                    {
                        _animals[_animalIndexesToMove[i]].FinishJob();
                        _animals[_animalIndexesToMove[i]].MoveLimbs();
                        bool ifCatched = _animals[_animalIndexesToMove[i]].ifCatched;
                        if (ifCatched == true)    //logike lapania zwierzeta implementuje w pliku animal movement
                        {
                            _animalsObjectsCatched++;
                            _animalsObjects[_animalIndexesToMove[i]].SetActive(false);   //ustawiam to zwierze jako nieaktywne
                            if (__animalsToActivate.Count != 0)
                            {
                                _animalsObjects[__animalsToActivate[0]].transform.position = _animalsObjects[_animalIndexesToMove[i]].transform.position;
                                _animalIndexesToMove[i] = __animalsToActivate[0];   //wprowadzam do tablicy indeksow nowe zwierze - bede sie do niego odwolywac przy nastepnym sprawdzeniu
                                __animalsToActivate.RemoveAt(0);
                                _animalsObjects[_animalIndexesToMove[i]].SetActive(true);
                            }
                        }
                    }
                    _populationUIhandler.UpdateUI(currentGen, _animalsObjectsCatched, currBestDistance, bestDistance, secondsPassed);
                }
                currentBatch++;
                batchesLeftToCompute--;
                if (batchesLeftToCompute < 0)
                {
                    currentBatch = 0;
                    batchesLeftToCompute = numberOfBatches-1;
                    indexesToSkip.Clear();
                }
            }
            else
            {
                currentBatch = 0;
                batchesLeftToCompute = numberOfBatches-1;
            }
            yield return new WaitForFixedUpdate();
        }
    }
    void FixedUpdate()
    {
        if (!VisualizationBasics.ifPaused)
        {
            secondsPassed = secondsPassed + Time.fixedDeltaTime;
            if (_animalsObjectsCatched == populationSize)
            {
                if (currentGen % _iterationsToResetMGenes == 0)
                {
                    _currentMGenes = new List<float>();
                }
                _newGenerationMove = false;
                currentGen++;
                _geneticAlgorithm.RunAlgorithm();
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
                _animalsObjectsCatched = 0;
                // StartCoroutine(CreateNewGeneration());
            }
            _populationUIhandler.UpdateUI(currentGen, _animalsObjectsCatched, currBestDistance, bestDistance, secondsPassed);
        }
    }

}
