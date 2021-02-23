using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowScale : MonoBehaviour
{

	[SerializeField] GameObject objectToFollow;


    void Update()
    {
		transform.localScale = objectToFollow.transform.localScale;
    }
}
