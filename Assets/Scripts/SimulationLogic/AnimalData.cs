using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class AnimalData
{
    public static float multiplierRangeMin = 0.6f;
    public static float multiplierRangeMax = 1.4f;
    public static float massMin = 2.0f;
    public static float massMax = 10.0f;
    public static int velocityMin = 2000;
    public static int velocityMax = 5500;
    public AnimalMovement animalMovement;
    public AnimalBrain animalBrain;
    public Dictionary<int, float> partsMass;
    public Dictionary<int, Vector3> partsScaleMultiplier;
    public Dictionary<int, Vector3> limbsPositionMultiplier;
    public Dictionary<int, int> targetJointsVelocity;
    public void MutateData()
    {
        int dataToMutate = Random.Range(0, 4);
        int[] keysArray;
        int index;
        switch (dataToMutate)
        {
            case 0:
                keysArray = partsMass.Keys.ToArray();
                index = Random.Range(0, keysArray.Length);
                partsMass[keysArray[index]]=Random.Range(massMin,massMax);
                break;
            case 1:
                keysArray = partsScaleMultiplier.Keys.ToArray();
                index = Random.Range(0, keysArray.Length);
                partsScaleMultiplier[keysArray[index]]=new Vector3(Random.Range(multiplierRangeMin,multiplierRangeMax),1,Random.Range(multiplierRangeMin,multiplierRangeMax));
                break;
            case 2:
                keysArray = limbsPositionMultiplier.Keys.ToArray();
                index = Random.Range(0, keysArray.Length);
                limbsPositionMultiplier[keysArray[index]]=new Vector3(Random.Range(multiplierRangeMin,multiplierRangeMax),1,Random.Range(multiplierRangeMin,multiplierRangeMax));
                break;
            case 3:
                keysArray = targetJointsVelocity.Keys.ToArray();
                index = Random.Range(0, keysArray.Length);
                targetJointsVelocity[keysArray[index]]=Random.Range(velocityMin,velocityMax);
                break;
        }
    }

}