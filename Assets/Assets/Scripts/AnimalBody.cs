using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalBody : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnCollisionEnter(Collision collision)
     {
         if(collision.gameObject.name=="ground")
         {
            transform.GetComponent<Rigidbody>().isKinematic=true;
            transform.parent.GetComponent<AnimalMovement>().CollisionDetected(this);
         }
     }
}
