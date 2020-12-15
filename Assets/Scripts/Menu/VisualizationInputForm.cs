using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class VisualizationInputForm : MonoBehaviour
{
    bool ifCorrectForm=true;
    void SetStartingInput(InputField inputText,dynamic savedInput)
    {
        if(savedInput!=0)
        {
            string convertedInput=Convert.ToString(savedInput);
            convertedInput=convertedInput.Replace(",",".");
            inputText.text=convertedInput;
        }
    }
    void Start()
    {
        var dropdown=GameObject.Find("AnimalDropdown").GetComponent<Dropdown>();
        dropdown.value=PlayerPrefs.GetInt("animalPrefabDropdownValue",0);
        SetStartingInput(GameObject.Find("PopulationSizeInput").GetComponent<InputField>(),PlayerPrefs.GetInt("populationSize",0));
        SetStartingInput(GameObject.Find("PopulationPartSizeInput").GetComponent<InputField>(),PlayerPrefs.GetInt("populationPartSize",0));
        SetStartingInput(GameObject.Find("StartingPositionInput").GetComponent<InputField>(),PlayerPrefs.GetFloat("startingPosition",0));
        SetStartingInput(GameObject.Find("SpeedInput").GetComponent<InputField>(),PlayerPrefs.GetFloat("speed",0));
        SetStartingInput(GameObject.Find("TimeImportanceInput").GetComponent<InputField>(),PlayerPrefs.GetFloat("timeBelowAveragePenalty",0));
        SetStartingInput(GameObject.Find("WeightsMutationRateInput").GetComponent<InputField>(),PlayerPrefs.GetFloat("weightsMutationRate",0));
        SetStartingInput(GameObject.Find("PhysicalMutationRateInput").GetComponent<InputField>(),PlayerPrefs.GetFloat("physicalMutationRate",0));
        SetStartingInput(GameObject.Find("TournamentSizeInput").GetComponent<InputField>(),PlayerPrefs.GetFloat("tournamentSize",0));
        SetStartingInput(GameObject.Find("CrossoverPercentInput").GetComponent<InputField>(),PlayerPrefs.GetFloat("crossoverPercent",0));

        // GameObject.Find("PopulationSizeInput").GetComponent<InputField>()=PlayerPrefs.GetInt("populationSize").ToString();
        // GameObject.Find("PopulationPartSizeInput").GetComponent<InputField>()=PlayerPrefs.GetFloat("populationPartSize").ToString();
        // GameObject.Find("StartingPositionInput").GetComponent<InputField>()=PlayerPrefs.GetFloat("startingPosition").ToString();
        // GameObject.Find("SpeedInput").GetComponent<InputField>()=PlayerPrefs.GetFloat("speed").ToString();
        // GameObject.Find("TimeImportanceInput").GetComponent<InputField>()=PlayerPrefs.GetFloat("timeBeingAliveImportance").ToString();
        // GameObject.Find("MutationRateInput").GetComponent<InputField>()=PlayerPrefs.GetFloat("mutationRate").ToString();
        // GameObject.Find("MaxGenesToMutateInput").GetComponent<InputField>()=PlayerPrefs.GetFloat("maxPercentGenesToMutate").ToString();
    }
}
