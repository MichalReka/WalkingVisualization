using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class PopulationUI : MonoBehaviour
{
    Text currGenNumber;
    Text catchedAnimalsNumber;
    Text currBestDistanceNumber;
    Text bestDistanceNumber;
    Text timeNumber;
    GameObject pauseLayer;
    // Start is called before the first frame update
    void Start()
    {
        pauseLayer=transform.Find("pauseLayer").gameObject;
        currGenNumber=transform.Find("currGenNumber").GetComponent<Text>();
        catchedAnimalsNumber=transform.Find("catchedAnimalsNumber").GetComponent<Text>();
        currBestDistanceNumber=transform.Find("currBestDistanceNumber").GetComponent<Text>();
        bestDistanceNumber=transform.Find("bestDistanceNumber").GetComponent<Text>();
        timeNumber=transform.Find("timeNumber").GetComponent<Text>();
    }
    public void UpdateUI(int currGen,int catchedAnimals, float currBestDistance,float bestDistance,float secondsPassed)
    {
        currGenNumber.text=currGen.ToString();
        catchedAnimalsNumber.text=catchedAnimals.ToString();
        currBestDistanceNumber.text=currBestDistance.ToString();
        bestDistanceNumber.text=bestDistance.ToString();
        TimeSpan time = TimeSpan.FromSeconds(secondsPassed);
        timeNumber.text=time.ToString(@"hh\:mm\:ss");
    }
    // Update is called once per frame
    void Update()
    {
        if(VisualizationBasics.ifPaused)
        {
            pauseLayer.SetActive(true);
        }
        else
        {
            pauseLayer.SetActive(false);
        }
    }
}
