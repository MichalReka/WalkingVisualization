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
    public System.Numerics.Vector3 scaleMultiplier;
    public System.Numerics.Vector3 positionMultiplier;
    System.Numerics.Vector3 _startingPosition;
    System.Numerics.Vector3 _startingScale;
    Vector3 _startingRotation;
    Rigidbody bodyData;
    AnimalMovement animalMovement;
    JointHandler[] jointHandlers;
    private void Awake()
    {
        bodyData = transform.GetComponent<Rigidbody>();
        if (isMoveable)
        {
            jointHandlers = transform.GetComponents<JointHandler>();
        }
        _startingScale = ConvertVector(transform.localScale);
        _startingPosition = ConvertVector(transform.localPosition);
        _startingRotation = transform.localEulerAngles;
    }
    private System.Numerics.Vector3 ConvertVector(Vector3 unitySourceVector)
    {
        System.Numerics.Vector3 newVector = new System.Numerics.Vector3();
        newVector.X=unitySourceVector.x;
        newVector.Y=unitySourceVector.y;
        newVector.Z=unitySourceVector.z;
        return newVector;
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
    }
    public void SetScale(System.Numerics.Vector3 scaleMultiplier)
    {
        this.scaleMultiplier = scaleMultiplier;
        transform.localScale = new Vector3(scaleMultiplier.X * _startingScale.X, _startingScale.Y, scaleMultiplier.Z * _startingScale.Z);
    }
    public void SetMaximumVelocity(int velocity)
    {
        for (int i = 0; i < jointHandlers.Length; i++)
        {
            jointHandlers[i].targetVelocity = velocity;
        }
    }
    public void SetPosition(System.Numerics.Vector3 positionMultiplier)
    {
        this.positionMultiplier = positionMultiplier;
        transform.localPosition = new Vector3(positionMultiplier.X * _startingPosition.X, _startingPosition.Y, positionMultiplier.Z * _startingPosition.Z);
    }
    public void ResetBodyPart()
    {
        transform.localScale = new Vector3(_startingScale.X,_startingScale.Y,_startingScale.Z);
        transform.localPosition = new Vector3(_startingPosition.X,_startingPosition.Y,_startingPosition.Z);
        transform.localEulerAngles = _startingRotation;
        bodyData.angularVelocity=Vector3.zero;
        bodyData.velocity = Vector3.zero;
        bodyData.isKinematic = false;
        
        if (isMoveable)
        {
            foreach(JointHandler joint in jointHandlers)
            {
                joint.ResetJoint();
            }
        }
    }
}
