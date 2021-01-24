using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class DatabasePanel : MonoBehaviour
{

    public static Button showIndButton;
    void Start()
    {
        List<List<string>> dataFromTable=DatabaseHandler.ReturnDataFromTable();
        var itemTemplate=GameObject.Find("SampleRow");
        showIndButton = GameObject.Find("ShowIndButton").GetComponent<Button>();
        var itemParent=GameObject.Find("Content").transform;
        foreach(var row in dataFromTable)
        {
            var newRow=Instantiate(itemTemplate,itemParent);
            var textCells=newRow.GetComponentsInChildren<Text>();
            for(int i=0;i<textCells.Length;i++)
            {
                if(textCells[i].text=="time")
                {
                    TimeSpan time = TimeSpan.FromSeconds(Convert.ToDouble(row[i]));
                    textCells[i].text=time.ToString(@"hh\:mm\:ss");
                }
                else
                {
                    textCells[i].text=row[i];
                }
            }
        }
        showIndButton.interactable=false;
        itemTemplate.SetActive(false);
    }
    public static void ActivateButton()
    {
        showIndButton.interactable=true;
    }
    public static void DeactivateButton()
    {
        showIndButton.interactable=false;
    }


    // Update is called once per frame

}
