using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    private Transform LookAtTargetTransform;
    private Vector3 lookPos;
    public float damping = 0.6f;

    public void Start()
    {
        LookAtTargetTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;
        if (LookAtTargetTransform != null)
        {
            lookPos = new Vector3(transform.position.x, LookAtTargetTransform.position.y, transform.position.z);
            var targetDirection = lookPos - LookAtTargetTransform.position;
            var rotation = Quaternion.LookRotation(targetDirection, transform.up);
            transform.rotation = rotation;
        }
    }

    void Update()
    {
        if (LookAtTargetTransform != null)
        {
            lookPos = new Vector3(transform.position.x, LookAtTargetTransform.position.y, transform.position.z);
            var targetDirection = lookPos - LookAtTargetTransform.position;
            //transform.forward = targetDirection;

            var rotation = Quaternion.LookRotation(targetDirection, transform.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * damping);
        }
        else
        {
            LookAtTargetTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;
        }
    }
}
