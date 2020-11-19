using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Newtonsoft.Json;
public class AnimalData
{
    public bool isElite = false;
    public bool isMigrated = false;
    public static float multiplierRangeMin = 0.6f;
    public static float multiplierRangeMax = 1.4f;
    public static float massMin = 2.0f;
    public static float massMax = 10.0f;
    public static int velocityMin = 2500;
    public static int velocityMax = 6000;
    public AnimalBrain animalBrain;
    public Dictionary<int, float> partsMass;
    public Dictionary<int, System.Numerics.Vector3> partsScaleMultiplier;
    public Dictionary<int, System.Numerics.Vector3> limbsPositionMultiplier;
    public Dictionary<int, int> targetJointsVelocity;
    public AnimalData DeepCopy()
    {
        AnimalData copy = new AnimalData();
        copy.animalBrain = new AnimalBrain();
        copy.animalBrain.DeepCopyFrom(this.animalBrain);
        copy.isElite = true;
        copy.partsMass = new Dictionary<int, float>(this.partsMass);
        copy.partsScaleMultiplier = new Dictionary<int, System.Numerics.Vector3>(this.partsScaleMultiplier);
        copy.targetJointsVelocity = new Dictionary<int, int>(this.targetJointsVelocity);
        copy.limbsPositionMultiplier = new Dictionary<int, System.Numerics.Vector3>(this.limbsPositionMultiplier);
        return copy;
    }
    public void MutateData()
    {
        int dataToMutate = UnityEngine.Random.Range(0, 4);
        int[] keysArray;
        int index;
        switch (dataToMutate)
        {
            case 0:
                keysArray = partsMass.Keys.ToArray();
                index = UnityEngine.Random.Range(0, keysArray.Length);
                partsMass[keysArray[index]] = UnityEngine.Random.Range(massMin, massMax);
                break;
            case 1:
                keysArray = partsScaleMultiplier.Keys.ToArray();
                index = UnityEngine.Random.Range(0, keysArray.Length);
                partsScaleMultiplier[keysArray[index]] = new System.Numerics.Vector3(UnityEngine.Random.Range(multiplierRangeMin, multiplierRangeMax), 1, UnityEngine.Random.Range(multiplierRangeMin, multiplierRangeMax));
                break;
            case 2:
                keysArray = limbsPositionMultiplier.Keys.ToArray();
                index = UnityEngine.Random.Range(0, keysArray.Length);
                limbsPositionMultiplier[keysArray[index]] = new System.Numerics.Vector3(UnityEngine.Random.Range(multiplierRangeMin, multiplierRangeMax), 1, UnityEngine.Random.Range(multiplierRangeMin, multiplierRangeMax));
                break;
            case 3:
                keysArray = targetJointsVelocity.Keys.ToArray();
                index = UnityEngine.Random.Range(0, keysArray.Length);
                targetJointsVelocity[keysArray[index]] = UnityEngine.Random.Range(velocityMin, velocityMax);
                break;
        }
    }
    public string SerializeData()
    {
        animalBrain.PrepareToSerialize();
        string json = JsonConvert.SerializeObject(this,Formatting.Indented);    //bez formatowania by bylo ladnie - dwa razy mniej zajmuje
        return json;
    }

}