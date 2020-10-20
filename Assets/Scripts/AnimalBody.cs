using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalBody : MonoBehaviour
{
    public int limbIndex;
    public int partIndex;
    public bool ifCollisionKills = true;
    public bool ifPositionCanBeChanged = true;
    public Vector3 scaleMultiplier;
    public Vector3 positionMultiplier;
    public float mass;
    Vector3 _startingPosition;
    Vector3 _startingScale;
    Rigidbody bodyData;

    enum MoveablePositions
    {
        X=0,Z=1,XZ=2
    }
    // Start is called before the first frame update
    void OnCollisionEnter(Collision collision)
     {
         if(collision.gameObject.name=="ground"&&ifCollisionKills)
         {
            transform.GetComponent<Rigidbody>().isKinematic=true;
            transform.parent.GetComponent<AnimalMovement>().CollisionDetected();
         }
     }
}
