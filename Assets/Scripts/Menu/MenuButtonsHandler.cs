using System.Collections;
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

    AsyncOperation loadingOperation;
    GameObject mainMenuContainer;
    GameObject rootCanvas;
    GameObject loadingObject;
    Slider progressBar;
    Image fadeInOutImage;
    private void Start()
    {
        if (GameObject.Find("FadeInOutImage"))
        {
            fadeInOutImage = GameObject.Find("FadeInOutImage").GetComponent<Image>();
        }
        loadingObject = GameObject.Find("Loading");
        mainMenuContainer = GameObject.Find("MainMenu");
        rootCanvas = GameObject.Find("RootCanvas");
        if (loadingObject)
        {
            progressBar = loadingObject.GetComponentInChildren<Slider>();
            loadingObject.SetActive(false);
        }
    }
    public void StartVisualization()
    {
        var animalDropdown = GameObject.Find("AnimalDropdown").GetComponent<Dropdown>();
        PopulationInputData.animalPrefabName = animalDropdown.options[animalDropdown.value].text;
        PopulationInputData.populationSize = Int32.Parse(GameObject.Find("PopulationSizeInput").GetComponent<InputField>().text);
        PopulationInputData.populationPartSize = Int32.Parse(GameObject.Find("PopulationPartSizeInput").GetComponent<InputField>().text);
        PopulationInputData.startingPosition = float.Parse(GameObject.Find("StartingPositionInput").GetComponent<InputField>().text, CultureInfo.InvariantCulture.NumberFormat);
        PopulationInputData.speed = float.Parse(GameObject.Find("SpeedInput").GetComponent<InputField>().text, CultureInfo.InvariantCulture.NumberFormat);
        PopulationInputData.timeBelowAveragePenalty = float.Parse(GameObject.Find("TimeImportanceInput").GetComponent<InputField>().text, CultureInfo.InvariantCulture.NumberFormat);
        PopulationInputData.weightsMutationRate = float.Parse(GameObject.Find("WeightsMutationRateInput").GetComponent<InputField>().text, CultureInfo.InvariantCulture.NumberFormat);
        PopulationInputData.physicalMutationRate = float.Parse(GameObject.Find("PhysicalMutationRateInput").GetComponent<InputField>().text, CultureInfo.InvariantCulture.NumberFormat);
        PopulationInputData.migrationEnabled = GameObject.Find("MigrationToggle").GetComponent<Toggle>().isOn;
        PopulationInputData.tournamentSize = float.Parse(GameObject.Find("TournamentSizeInput").GetComponent<InputField>().text, CultureInfo.InvariantCulture.NumberFormat);
        PopulationInputData.crossoverPercent = float.Parse(GameObject.Find("CrossoverPercentInput").GetComponent<InputField>().text, CultureInfo.InvariantCulture.NumberFormat);
        PopulationInputData.adaptationEnabled = GameObject.Find("AdaptationToggle").GetComponent<Toggle>().isOn;
        PlayerPrefs.SetInt("animalPrefabDropdownValue", animalDropdown.value);
        PlayerPrefs.SetInt("populationSize", PopulationInputData.populationSize);
        PlayerPrefs.SetInt("populationPartSize", PopulationInputData.populationPartSize);
        PlayerPrefs.SetFloat("startingPosition", PopulationInputData.startingPosition);
        PlayerPrefs.SetFloat("speed", PopulationInputData.speed);
        PlayerPrefs.SetFloat("timeBelowAveragePenalty", PopulationInputData.timeBelowAveragePenalty);
        PlayerPrefs.SetFloat("weightsMutationRate", PopulationInputData.weightsMutationRate);
        PlayerPrefs.SetFloat("physicalMutationRate", PopulationInputData.physicalMutationRate);
        PlayerPrefs.SetFloat("tournamentSize", PopulationInputData.tournamentSize);
        PlayerPrefs.SetFloat("crossoverPercent", PopulationInputData.crossoverPercent);
        animalDropdown.gameObject.SetActive(false);
        var coroutineHandler = FadeInAndDo(() =>
        {
            loadingObject.SetActive(true);
            Application.backgroundLoadingPriority = ThreadPriority.Normal;
            StartCoroutine("LoadScene");
        });
        StartCoroutine(coroutineHandler);
    }

    public void SwitchMenu(string subMenuName)
    {
        EventSystem.current.currentSelectedGameObject.GetComponent<Animator>().keepAnimatorControllerStateOnDisable = true;
        EventSystem.current.currentSelectedGameObject.GetComponent<Animator>().Play("Normal", -1);
        mainMenuContainer.SetActive(!mainMenuContainer.activeSelf);
        var subMenu = rootCanvas.transform.Find(subMenuName).gameObject;  //nie mozna bezposrednio GameObject.Find() - ta funkcja wyszukuje tlyko aktywne obikety
        subMenu.SetActive(!subMenu.activeSelf);
    }
    public void Quit()
    {
        var coroutineHandler = FadeInAndDo(() => { Time.timeScale = 1; ; Application.Quit(); });
        StartCoroutine(coroutineHandler);
    }
    public void GoBackToMainMenu()
    {
        var coroutineHandler = FadeInAndDo(() => { Time.timeScale = 1; SceneManager.LoadScene("menu"); });
        StartCoroutine(coroutineHandler);
    }
    IEnumerator FadeInAndDo(Action toDoAfterFade = null)  //uzycie action pozwala na zrobienie czegos po animacji
    {
        var coroutineHandler = FadeInHandler.FadeIn(fadeInOutImage);
        yield return StartCoroutine(coroutineHandler);
        toDoAfterFade.Invoke();
    }
    IEnumerator LoadScene()  //uzycie action pozwala na zrobienie czegos po animacji
    {

        loadingOperation = SceneManager.LoadSceneAsync("Simulation");
        VisualizationBasics.ResumeGame();
        while (!loadingOperation.isDone)
        {
            Debug.Log(Mathf.Clamp01(loadingOperation.progress / 0.9f));
            yield return null;
        }
    }
    private void FixedUpdate()
    {
        if (loadingObject)
        {
            if (loadingObject.activeSelf)
            {
                Debug.Log(Mathf.Clamp01(loadingOperation.progress / 0.9f));
                progressBar.value = Mathf.Clamp01(loadingOperation.progress / 0.9f);
            }
        }
    }
}
