using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PopulationUI : MonoBehaviour
{
    Text currGenNumber;
    Text catchedAnimalsNumber;
    Text currBestDistanceNumber;
    Text bestDistanceNumber;
    GameObject pauseLayer;
    // Start is called before the first frame update
    void Start()
    {
        pauseLayer=transform.Find("pauseLayer").gameObject;
        currGenNumber=transform.Find("currGenNumber").GetComponent<Text>();
        catchedAnimalsNumber=transform.Find("catchedAnimalsNumber").GetComponent<Text>();
        currBestDistanceNumber=transform.Find("currBestDistanceNumber").GetComponent<Text>();
        bestDistanceNumber=transform.Find("bestDistanceNumber").GetComponent<Text>();
    }
    public void UpdateUI(int currGen,int catchedAnimals, float currBestDistance,float bestDistance)
    {
        currGenNumber.text=currGen.ToString();
        catchedAnimalsNumber.text=catchedAnimals.ToString();
        currBestDistanceNumber.text=currBestDistance.ToString();
        bestDistanceNumber.text=bestDistance.ToString();
    }
    // Update is called once per frame
    void Update()
    {
        if(visualizationBasics.ifPaused)
        {
            pauseLayer.SetActive(true);
        }
        else
        {
            pauseLayer.SetActive(false);
        }
    }
}
