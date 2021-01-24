using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
public class AnimalPresentation : MonoBehaviour
{
    // Start is called before the first frame update
    Animal selectedAnimal;
    AnimalPresentationUI _animalPresentationUIHandler;
    float time = 0;
    float speed = 0;
    float distance = 0;
    int batchIteration = 0; //kazdy osobnik podejmowal dzialanie 10 razy na sekunde, nie 50
    void Start()
    {

        string loadedAnimalJson = DatabaseHandler.ReturnAnimalDataJsonSingle(AnimalListItem.selectedAnimalIndex);
        var animalObject = Instantiate(Resources.Load("Prefabs/" + AnimalListItem.selectedAnimalPrefabName) as GameObject);
        var movingParts = animalObject.GetComponentsInChildren<JointHandler>();
        AnimalBrain.noMovingParts = movingParts.Length;
        AnimalBrain.outputSize = AnimalBrain.outputPerArm * AnimalBrain.noMovingParts;
        animalObject.transform.SetPositionAndRotation(new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
        selectedAnimal = animalObject.AddComponent<Animal>();
        selectedAnimal.ifSimulated = false;
        selectedAnimal.SetBody(false, false);
        AnimalData migratedData = JsonConvert.DeserializeObject<AnimalData>(loadedAnimalJson);
        migratedData.animalBrain.DeserializeWeights();
        selectedAnimal.SetAnimalData(migratedData);
        selectedAnimal.SetBodyPartsStartingX();
        _animalPresentationUIHandler = GameObject.Find("infoCanvas").GetComponent<AnimalPresentationUI>();
    }
    private void FixedUpdate()
    {
        batchIteration++;
        distance = selectedAnimal.GetAverageX();
        time = Time.fixedDeltaTime + time;
        speed = distance/time;
        if (batchIteration == Simulation.numberOfBatches)
        {
            selectedAnimal.UpdateInput();
            selectedAnimal.ComputeOutput();
            selectedAnimal.FinishJob();
            selectedAnimal.MoveLimbs();
            batchIteration = 0;
        }
        _animalPresentationUIHandler.UpdateUI(distance,speed,time);
    }

}
