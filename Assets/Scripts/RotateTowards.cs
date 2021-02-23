using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTowards : MonoBehaviour {

    public GameObject target;
    public bool inverse;

	Vector3 rot;
    Vector3 normalized;


    void Update() {
		if (target) Rotate();
	}


	void Rotate() {

		if (!inverse) {
			transform.LookAt(target.transform);
		} else {
			transform.rotation = Quaternion.LookRotation(transform.position - target.transform.position);
			//	rot = transform.rotation.eulerAngles;
			//	normalized = transform.rotation.eulerAngles.normalized;
			//	rot = new Vector3(rot.x + 180 * normalized.x, rot.y + 180 * normalized.y, rot.z + 180 * normalized.z);
			//	transform.rotation = Quaternion.Euler(rot);
		}
	}

}
