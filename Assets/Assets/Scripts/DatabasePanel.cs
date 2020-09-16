using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DatabasePanel : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        List<List<string>> dataFromTable=DatabaseHandler.ReturnDataFromTable();
        var itemTemplate=GameObject.Find("SampleRow");
        var itemParent=GameObject.Find("Content").transform;
        Debug.Log(dataFromTable.Count);
        foreach(var row in dataFromTable)
        {
            var newRow=Instantiate(itemTemplate,itemParent);
            var textCells=newRow.GetComponentsInChildren<Text>();
            for(int i=0;i<textCells.Length;i++)
            {
                textCells[i].text=row[i];
            }
        }
        itemTemplate.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
