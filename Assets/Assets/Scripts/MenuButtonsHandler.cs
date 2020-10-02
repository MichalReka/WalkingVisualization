using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.SceneManagement;


public class MenuButtonsHandler : MonoBehaviour
{
    // Start is called before the first frame update

    GameObject mainMenuContainer;
    GameObject rootCanvas;
    Image fadeInOutImage;
    private void Start() {
        var fadeInOutImageObject=GameObject.Find("FadeInOutImage");
        fadeInOutImage=fadeInOutImageObject.GetComponent<Image>();
        mainMenuContainer=GameObject.Find("MainMenu");
        rootCanvas=GameObject.Find("RootCanvas");
    }
    public void StartVisualization()
    {
        var coroutineHandler=FadeInAndDo(()=>{SceneManager.LoadScene("walking");});
        StartCoroutine(coroutineHandler);
    }
    public void SwitchMenu(string subMenuName)
    {
        mainMenuContainer.SetActive(!mainMenuContainer.activeSelf);
        var subMenu=rootCanvas.transform.Find(subMenuName).gameObject;  //nie mozna bezposrednio GameObject.Find() - ta funkcja wyszukuje tlyko aktywne obikety
        subMenu.SetActive(!subMenu.activeSelf);
    }
    public void Quit()
    {
        var coroutineHandler=FadeInAndDo(()=>{Application.Quit();});
        StartCoroutine(coroutineHandler);
    }
    IEnumerator FadeInAndDo(Action toDoAfterFade=null)  //uzycie action pozwala na zrobienie czegos po animacji
    {
        var coroutineHandler=FadeInHandler.FadeIn(fadeInOutImage);
        yield return StartCoroutine(coroutineHandler);
        toDoAfterFade.Invoke();
    }
    
    
}
