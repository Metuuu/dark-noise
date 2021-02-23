using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObject : MonoBehaviour
{

    Transform myTransform;
    public Transform target;


    void Start() {
        myTransform = transform;
    }

    void LateUpdate() {
        myTransform.position = target.position;
    }


}
