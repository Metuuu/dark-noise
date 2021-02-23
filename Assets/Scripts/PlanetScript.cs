using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetScript : MonoBehaviour
{
	GameObject player;
	GameObject lighting;
	Light lightDirectional;
	Light lightSpotInverse;

	public float radius;
	public Color lightColor;
	[SerializeField] bool canBeHostile;
	[SerializeField] float chanceToBeHostile = 0.5f;
	[SerializeField] float chanceToHaveDustRing = 0.5f;
	[SerializeField] float chanceToHaveRingMeteorites = 0.5f; // Meteorites only exist with dust ring
	[SerializeField] float spotlightBrightness = 1f;
	[SerializeField] float spotLightDistance = 10f;
	[SerializeField] float spotLightInverseAngleMultiplier = 1f;
	[SerializeField] AnimationCurve spotLightBrightnessMultiplierFromDistancePercentage;

	[HideInInspector] public bool isHostile;
	bool hasDustRing;
	bool hasRingMeteorites;
	float distance;
	float maxSpotlightDistance;
	float maxInverseSpotlightDistance;
	bool somethingBetween;
	float spotlightBrightnessMultiplier;



	void Awake()
    {
		//radius = Random.Range(30, 200);
		player = GameObject.FindWithTag("Player");
		lighting = transform.Find("Lighting").gameObject;
		lightSpotInverse = lighting.transform.Find("LightSpotInverse").GetComponent<Light>();

		// lightSpot = lighting.transform.Find("LightSpot").GetComponent<Light>();
		lightDirectional = lighting.transform.Find("LightDirectional").GetComponent<Light>();
		lightDirectional.color = lightColor;
		lightSpotInverse.color = lightColor;

		lighting.GetComponent<FollowObject>().target = player.transform;
		lightDirectional.GetComponent<RotateTowards>().target = transform.Find("Sphere").gameObject;
		lightSpotInverse.GetComponent<RotateTowards>().target = transform.Find("Sphere").gameObject;;

		RotateTowards rt = GetComponent<RotateTowards>();
		if (rt) rt.target = player;

		lightDirectional.transform.localPosition = new Vector3(0, 0, (-spotLightDistance / radius));

		Transform outerGas = transform.Find("OuterGas");
		if (outerGas) outerGas.GetComponent<RotateTowards>().target = player;

		isHostile = (canBeHostile && Random.value <= chanceToBeHostile);
		if (chanceToHaveDustRing > 0 && Random.value <= chanceToHaveDustRing) {
			hasDustRing = true;
			hasRingMeteorites = (chanceToHaveRingMeteorites > 0 && Random.value <= chanceToHaveRingMeteorites);
		}

		if (hasDustRing) transform.Find("Ring")?.gameObject.SetActive(true);
		if (hasRingMeteorites) transform.Find("Rocks")?.gameObject.SetActive(true);
	}

	private void Start() {
		transform.localScale = new Vector3(radius * 2, radius * 2, radius * 2);
	}

	void Update()
    {
		transform.localScale = new Vector3(radius * 2, radius * 2, radius * 2);
		lightDirectional.transform.localPosition = new Vector3(0, 0, (-spotLightDistance / radius));

		distance = Vector3.Distance(player.transform.position, transform.position);

		somethingBetween = false;//Physics.Raycast(player.transform.position, (transform.position - player.transform.position), distance - radius * 2.1f);

		maxInverseSpotlightDistance = radius * 100f;
		maxSpotlightDistance = radius * 200f;

		spotlightBrightnessMultiplier = spotLightBrightnessMultiplierFromDistancePercentage.Evaluate(1f - (maxSpotlightDistance - distance) / maxSpotlightDistance);


		if ((distance > maxInverseSpotlightDistance) || somethingBetween) {
			//Debug.Log("Hit");
			lightSpotInverse.enabled = false;
			spotlightBrightnessMultiplier *= 0.75f;
		} else {
			//Debug.Log("Not Hit");
			lightSpotInverse.enabled = true;
			lightSpotInverse.spotAngle = Mathf.Atan(radius * 9f / (distance - radius * 2)) * Mathf.Rad2Deg * spotLightInverseAngleMultiplier;
		}

		if (distance > maxSpotlightDistance) {
			if (lightDirectional.isActiveAndEnabled) lighting.SetActive(false);
		} else if (!lightDirectional.isActiveAndEnabled) {
			lighting.SetActive(true);
		}

		lightDirectional.intensity = spotlightBrightness * spotlightBrightnessMultiplier;

	}
}
