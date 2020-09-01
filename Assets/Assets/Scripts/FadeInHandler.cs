using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeInHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var image=GetComponent<Image>();
        if(image)
        {
            IEnumerator couroutineHandler=FadeIn(image);
            StartCoroutine(couroutineHandler);
        }
    }

    // Update is called once per frame
    public static IEnumerator FadeIn(Image image)
    {
        var oldColor=image.color;
        var tempColor=image.color;
        for (float i = oldColor.a; i <= 1; i += Time.deltaTime*2)
        {
            // set color with i as alpha
            tempColor.a=i;
            image.color = tempColor;
            yield return null;
        } 
    }
}
