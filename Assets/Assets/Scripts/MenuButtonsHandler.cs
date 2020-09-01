using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.SceneManagement;


public class MenuButtonsHandler : MonoBehaviour
{
    // Start is called before the first frame update

    Image fadeInOutImage;
    private void Start() {
        var fadeInOutImageObject=GameObject.Find("FadeInOutImage");
        fadeInOutImage=fadeInOutImageObject.GetComponent<Image>();
    }
    public void startVisualization()
    {
        StartCoroutine("fadeInAndMoveToScene");
    }
    IEnumerator fadeInAndMoveToScene()
    {
        var coroutineHandler=FadeInHandler.FadeIn(fadeInOutImage);
        yield return StartCoroutine(coroutineHandler);
        SceneManager.LoadScene("walking");
    }
}
