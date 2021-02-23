using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarScript : MonoBehaviour
{

	void FixedUpdate()
    {
		transform.rotation = Quaternion.identity; // Don't rotatate at all
	}

}
 