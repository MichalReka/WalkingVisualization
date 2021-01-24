using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
public class VisualizationInputForm : MonoBehaviour
{
    bool ifCorrectForm = true;
    Text description;
    string selectedGameObjectName = "";
    void SetStartingInput(InputField inputText, dynamic savedInput)
    {
        if (savedInput != 0)
        {
            string convertedInput = Convert.ToString(savedInput);
            convertedInput = convertedInput.Replace(",", ".");
            inputText.text = convertedInput;
        }
    }
    void Start()
    {
        var dropdown = GameObject.Find("AnimalDropdown").GetComponent<Dropdown>();
        dropdown.value = PlayerPrefs.GetInt("animalPrefabDropdownValue", 0);
        SetStartingInput(GameObject.Find("PopulationSizeInput").GetComponent<InputField>(), PlayerPrefs.GetInt("populationSize", 0));
        SetStartingInput(GameObject.Find("PopulationPartSizeInput").GetComponent<InputField>(), PlayerPrefs.GetInt("populationPartSize", 0));
        SetStartingInput(GameObject.Find("StartingPositionInput").GetComponent<InputField>(), PlayerPrefs.GetFloat("startingPosition", 0));
        SetStartingInput(GameObject.Find("SpeedInput").GetComponent<InputField>(), PlayerPrefs.GetFloat("speed", 0));
        SetStartingInput(GameObject.Find("TimeImportanceInput").GetComponent<InputField>(), PlayerPrefs.GetFloat("timeBelowAveragePenalty", 0));
        SetStartingInput(GameObject.Find("WeightsMutationRateInput").GetComponent<InputField>(), PlayerPrefs.GetFloat("weightsMutationRate", 0));
        SetStartingInput(GameObject.Find("PhysicalMutationRateInput").GetComponent<InputField>(), PlayerPrefs.GetFloat("physicalMutationRate", 0));
        SetStartingInput(GameObject.Find("TournamentSizeInput").GetComponent<InputField>(), PlayerPrefs.GetFloat("tournamentSize", 0));
        SetStartingInput(GameObject.Find("CrossoverPercentInput").GetComponent<InputField>(), PlayerPrefs.GetFloat("crossoverPercent", 0));
        description = GameObject.Find("DescriptionText").GetComponent<Text>();
    }
    void Update()
    {
        if (EventSystem.current.currentSelectedGameObject)
        {
            if (selectedGameObjectName != EventSystem.current.currentSelectedGameObject.name)
            {
                selectedGameObjectName = EventSystem.current.currentSelectedGameObject.name;
                if(String.Equals(selectedGameObjectName, "PopulationSizeInput", StringComparison.OrdinalIgnoreCase))
                {
                    description.text = "Ilość osobników w populacji w algorytmie genetycznym.";
                }
                else if(String.Equals(selectedGameObjectName, "PopulationPartSizeInput", StringComparison.OrdinalIgnoreCase))
                {
                    description.text = "Ilość osobników pokazywanych jednocześnie podczas symulacji. Należy dobrać eksperymentalnie tą wartość patrząc na możliwości swojego urządzenia.";
                }
                else if(String.Equals(selectedGameObjectName, "StartingPositionInput", StringComparison.OrdinalIgnoreCase))
                {
                    description.text = "Pozycja, od której rozpoczynana jest gonitwa za każdym z osobników z osobna. Im większa wartość, tym dalej od pozycji początkowej.";
                }
                else if(String.Equals(selectedGameObjectName, "SpeedInput", StringComparison.OrdinalIgnoreCase))
                {
                    description.text = "Ilość pokonywanych metrów na sekunde podczas gonitwy osobników.";
                }
                else if(String.Equals(selectedGameObjectName, "TimeImportanceInput", StringComparison.OrdinalIgnoreCase))
                {
                    description.text = "Wartość zmniejszenia wyniku funkcji oceny dla osobników dających się złapać wcześniej niż inne lub szybciej upadających. Przy wartości 1 kara nie występuje, im mniejsza wartość, tym większa kara.";
                }
                else if(String.Equals(selectedGameObjectName, "WeightsMutationRateInput", StringComparison.OrdinalIgnoreCase))
                {
                    description.text = "Prawdopodobieństwo zajścia mutacji wag sieci neuronowej - 1 to 100%.";
                }
                else if(String.Equals(selectedGameObjectName, "PhysicalMutationRateInput", StringComparison.OrdinalIgnoreCase))
                {
                    description.text = "Prawdopodobieństwo zajścia mutacji zmieniającej fizyczne parametry modelu, przykładowo masę nogi - 1 to 100%.";
                }
                else if(String.Equals(selectedGameObjectName, "TournamentSizeInput", StringComparison.OrdinalIgnoreCase))
                {
                    description.text = "Maksymalny procent osobników zebranych do jednej tury podczas krzyżowania turniejowego - 1 to 100%.";
                }
                else if(String.Equals(selectedGameObjectName, "CrossoverPercentInput", StringComparison.OrdinalIgnoreCase))
                {
                    description.text = "Prawdopodobieńśtwo zajścia krzyżowania.";
                }
                else if(String.Equals(selectedGameObjectName, "MigrationToggle", StringComparison.OrdinalIgnoreCase))
                {
                    description.text = "Migracja polega na wczytywaniu do startowej populacji osobników zapisanych w bazie danych.";
                }
                else if(String.Equals(selectedGameObjectName, "AdaptationToggle", StringComparison.OrdinalIgnoreCase))
                {
                    description.text = "Współczynniki wejściowe będą automatycznie dostosywane w oparciu o obserwacje działania symulacji. Wpisane wartości są wartościami wejściowymi.";
                }
                else if(selectedGameObjectName.Contains("owad"))
                {
                    description.text = "Osobnik przypominający owada. Posiada 3 pary ruchomych w dwóch osiach nóg. Wszystkie nogi posiadają takie same osie zginania.";
                }
                else if(selectedGameObjectName.Contains("tetrapod1"))
                {
                    description.text = "Osobnik z dwoma parami dwuczłonowych nóg - tylne i przednie. Obie pary zginają się w jedną stronę.";
                }
                else if(selectedGameObjectName.Contains("tetrapod2"))
                {
                    description.text = "Osobnik z dwoma parami dwuczłonowych nóg - tylne i przednie. Obie pary zginają się w różne strony";
                }
                else if(selectedGameObjectName.Contains("tetrapod3"))
                {
                    description.text = "Osobnik z dwoma parami dwuczłonowych nóg - tylne i przednie. Obie pary zginają się w różne strony. Wysokość nóg obniżona o połowe względem osobnika tetrapod2.";
                }
                else
                {
                    description.text = "";
                }
            }
        }

    }
}
