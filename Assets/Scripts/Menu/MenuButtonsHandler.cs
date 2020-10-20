﻿using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Globalization;
using UnityEngine.SceneManagement;


public class MenuButtonsHandler : MonoBehaviour
{
    // Start is called before the first frame update

    GameObject mainMenuContainer;
    GameObject rootCanvas;
    Image fadeInOutImage;
    private void Start() {
        fadeInOutImage=GameObject.Find("FadeInOutImage").GetComponent<Image>();
        mainMenuContainer=GameObject.Find("MainMenu");
        rootCanvas=GameObject.Find("RootCanvas");
    }
    public void StartVisualization()
    {
        var animalDropdown=GameObject.Find("AnimalDropdown").GetComponent<Dropdown>();
        PopulationInputData.animalPrefabName=animalDropdown.options[animalDropdown.value].text;
        PopulationInputData.populationSize=Int32.Parse(GameObject.Find("PopulationSizeInput").GetComponent<InputField>().text);
        PopulationInputData.populationPartSize=Int32.Parse(GameObject.Find("PopulationPartSizeInput").GetComponent<InputField>().text);
        PopulationInputData.startingPosition=float.Parse(GameObject.Find("StartingPositionInput").GetComponent<InputField>().text, CultureInfo.InvariantCulture.NumberFormat);
        PopulationInputData.speed=float.Parse(GameObject.Find("SpeedInput").GetComponent<InputField>().text, CultureInfo.InvariantCulture.NumberFormat);
        PopulationInputData.timeBeingAliveImportance=float.Parse(GameObject.Find("TimeImportanceInput").GetComponent<InputField>().text, CultureInfo.InvariantCulture.NumberFormat);
        PopulationInputData.mutationRate=float.Parse(GameObject.Find("MutationRateInput").GetComponent<InputField>().text, CultureInfo.InvariantCulture.NumberFormat);
        PopulationInputData.maxPercentGenesToMutate=float.Parse(GameObject.Find("MaxGenesToMutateInput").GetComponent<InputField>().text, CultureInfo.InvariantCulture.NumberFormat);
        PlayerPrefs.SetInt("animalPrefabDropdownValue",animalDropdown.value);
        PlayerPrefs.SetInt("populationSize",PopulationInputData.populationSize);
        PlayerPrefs.SetInt("populationPartSize",PopulationInputData.populationPartSize);
        PlayerPrefs.SetFloat("startingPosition", PopulationInputData.startingPosition);
        PlayerPrefs.SetFloat("speed", PopulationInputData.speed);
        PlayerPrefs.SetFloat("timeBeingAliveImportance", PopulationInputData.timeBeingAliveImportance);
        PlayerPrefs.SetFloat("mutationRate", PopulationInputData.mutationRate);
        PlayerPrefs.SetFloat("maxPercentGenesToMutate", PopulationInputData.maxPercentGenesToMutate);
        var coroutineHandler=FadeInAndDo(()=>{VisualizationBasics.ResumeGame();SceneManager.LoadScene("walking");});
        StartCoroutine(coroutineHandler);
    }

    public void SwitchMenu(string subMenuName)
    {
        EventSystem.current.currentSelectedGameObject.GetComponent<Animator>().keepAnimatorControllerStateOnDisable=true;
        EventSystem.current.currentSelectedGameObject.GetComponent<Animator>().Play("Normal",-1);
        mainMenuContainer.SetActive(!mainMenuContainer.activeSelf);
        var subMenu=rootCanvas.transform.Find(subMenuName).gameObject;  //nie mozna bezposrednio GameObject.Find() - ta funkcja wyszukuje tlyko aktywne obikety
        subMenu.SetActive(!subMenu.activeSelf);
    }
    public void Quit()
    {
        var coroutineHandler=FadeInAndDo(()=>{Application.Quit();});
        StartCoroutine(coroutineHandler);
    }
    public void GoBackToMainMenu()
    {
        var coroutineHandler=FadeInAndDo(()=>{SceneManager.LoadScene("menu");});
        StartCoroutine(coroutineHandler);
    }
    IEnumerator FadeInAndDo(Action toDoAfterFade=null)  //uzycie action pozwala na zrobienie czegos po animacji
    {
        var coroutineHandler=FadeInHandler.FadeIn(fadeInOutImage);
        yield return StartCoroutine(coroutineHandler);
        toDoAfterFade.Invoke();
    }
    
    
    
}
