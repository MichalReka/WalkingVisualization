using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class AnimalPresentationUI : MonoBehaviour
{
    // Start is called before the first frame update
    Text distanceNumber;
    Text speedNumber;
    Text timeNumber;

    GameObject pauseLayer;
    // Start is called before the first frame update
    void Start()
    {
        pauseLayer=transform.Find("pauseLayer").gameObject;
        distanceNumber=transform.Find("distanceNumber").GetComponent<Text>();
        speedNumber=transform.Find("speedNumber").GetComponent<Text>();
        timeNumber=transform.Find("timeNumber").GetComponent<Text>();
    }
    public void UpdateUI(float distance,float speed, float time)
    {
        distanceNumber.text=distance.ToString();
        speedNumber.text=speed.ToString();
        TimeSpan timeSpan = TimeSpan.FromSeconds(time);
        timeNumber.text=timeSpan.ToString(@"hh\:mm\:ss");
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
