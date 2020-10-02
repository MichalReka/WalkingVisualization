using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeOutHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var image=GetComponent<Image>();
        if(image)
        {
            IEnumerator couroutineHandler=FadeOut(image);
            StartCoroutine(couroutineHandler);
        }
        
    }

    //fade out i fade in po dodaniu do obiektu dzialaja tylko wtedy kiedy obiekt posiada tez component Image
    public static IEnumerator FadeOut(Image image)
    {
        var oldColor=image.color;
        var tempColor=image.color;
        for (float i = oldColor.a; i >= 0; i -= Time.unscaledDeltaTime*2)
        {
            // set color with i as alpha
            tempColor.a=i;
            image.color = tempColor;
            yield return null;
        } 
    }
}
