using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowJointRotation : MonoBehaviour
{
    // Start is called before the first frame update
    ConfigurableJoint joint;
    void Start()
    {
        joint=transform.GetComponent<ConfigurableJoint>();
    }
    public Quaternion getJointRotation()
    {
        //https://answers.unity.com/questions/1694646/how-to-get-the-rotation-of-configurable-joint.html
        return (Quaternion.Inverse(joint.connectedBody.transform.rotation) * joint.transform.rotation);
    }
    // Update is called once per frame
    void Update()
    {
        Debug.Log("x:"+JointHandler.AdjustRotation(transform.localEulerAngles.x)+"y:"+JointHandler.AdjustRotation(transform.localEulerAngles.y)+"z:"+JointHandler.AdjustRotation(transform.localEulerAngles.z));
    }
}
