using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalBodyPart : MonoBehaviour
{
    public int limbIndex;
    public int partIndex;
    public bool ifCollisionKills = true;
    public bool ifPositionCanBeChanged = true;
    public bool ifScalable = false;
    public bool isMoveable = false;
    public Vector3 scaleMultiplier;
    public Vector3 positionMultiplier;
    public float mass;
    Vector3 _startingPosition;
    Vector3 _startingScale;
    Rigidbody bodyData;
    AnimalMovement animalMovement;
    JointHandler[] jointHandler;
    private void Awake()
    {
        bodyData = transform.GetComponent<Rigidbody>();
        if (isMoveable)
        {
            jointHandler = transform.GetComponents<JointHandler>();
        }
        _startingScale = transform.localScale;
        _startingPosition = transform.localPosition;
    }
    private void Start()
    {
        animalMovement = transform.parent.GetComponent<AnimalMovement>();
    }
    enum MoveablePositions
    {
        X = 0, Z = 1, XZ = 2
    }
    // Start is called before the first frame update
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "ground" && ifCollisionKills)
        {
            bodyData.isKinematic = true;
            animalMovement.CollisionDetected();
        }
    }
    public void SetMass(float mass)
    {
        bodyData.mass = mass;
        this.mass = mass;
    }
    public void SetScale(Vector3 scaleMultiplier)
    {
        this.scaleMultiplier = scaleMultiplier;
        transform.localScale = new Vector3(scaleMultiplier.x * _startingScale.x, _startingScale.y, scaleMultiplier.z * _startingScale.z);
    }
    public void SetMaximumVelocity(int velocity)
    {
        for (int i = 0; i < jointHandler.Length; i++)
        {
            jointHandler[i].targetVelocity = velocity;
        }
    }
    public void SetPosition(Vector3 positionMultiplier)
    {

        this.positionMultiplier = positionMultiplier;
        transform.localPosition = new Vector3(positionMultiplier.x * _startingPosition.x, _startingPosition.y, positionMultiplier.z * _startingPosition.z);

    }
}
