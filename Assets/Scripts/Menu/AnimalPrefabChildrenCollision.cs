using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalPrefabChildrenCollision : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (transform.parent.GetComponent<AnimalPrefabCollision>().ifScaled == false)
        {
            transform.parent.GetComponent<AnimalPrefabCollision>().ifScaled = true;
        }

    }
}
