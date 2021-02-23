using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteStarfield : MonoBehaviour
{
	[SerializeField] GameObject[] starObjectsToGenerate;
	[SerializeField] ParticleSystem ParticleSystem;
	[SerializeField] ParticleSystem radarParticleSys;
	[SerializeField] AudioSource radarAudioSource;
	Transform tx;
	ParticleSystem.Particle[] points;
	ParticleSystem.Particle[] radarPoints;
	float[] pointSizes;
	float[] radarPointSizes;
	float[] starDistanceSqrs;
	bool[] isObject;
	bool[] isHostile;
	GameObject[] gameObjects;

	[SerializeField] int starsMax = 100;
	[SerializeField] Vector2 starSize = new Vector2(2.5f, 5);
	[SerializeField] Vector2 starDistance = new Vector2(1, 100);
	[SerializeField] float turnToObjectDistance = 10;
	[SerializeField] float maxRadarDistance = 5;
	[SerializeField] Color radarParticleColor;
	[SerializeField] Color radarParticleColorHostile;
	[SerializeField] float positionScaleMultiplier = 0.0002f;
	[SerializeField] float sizeScaleMultiplier = 0.002f;
	private float starDistanceSqr;
	private float maxRadarDistanceSqr;
	private float turnToObjectDistanceSqr;

	float percent;
	float distMag;
	PlanetScript planetScript;
	bool playRadarSound; // Plays when hostile planet is in range


	void Awake() {
		tx = transform;
		starDistanceSqr = starDistance.y * starDistance.y * 1.001f;
		maxRadarDistance = maxRadarDistance * maxRadarDistance;
		turnToObjectDistanceSqr = turnToObjectDistance * turnToObjectDistance;
	}


	void Update() {
		if (points == null)
			CreateStars();

		playRadarSound = false;
		for (int i = 0; i < starsMax; ++i) {

			distMag = (points[i].position - tx.position).sqrMagnitude;

			if (distMag > starDistanceSqr) {
				if (gameObjects[i] != null) {
					Destroy(gameObjects[i]);
					isObject[i] = false;
					gameObjects[i] = null;
				}
				points[i].position = Random.insideUnitSphere.normalized * starDistance.y + tx.position;
				points[i].startSize = Random.Range(starSize.x, starSize.y);
				pointSizes[i] = points[i].startSize;

				radarPoints[i].position = points[i].position * positionScaleMultiplier;
				radarPointSizes[i] = pointSizes[i] * sizeScaleMultiplier;
				radarPoints[i].startSize = radarPointSizes[i];

				generateGameObjectForParticle(i);
			}

			if (distMag > maxRadarDistance) {
				radarPoints[i].startSize = 0;
			} else {
				if (isHostile[i]) {
					playRadarSound = true;
				}
				radarPoints[i].position = (points[i].position - transform.position) * positionScaleMultiplier;
				radarPoints[i].startSize = radarPointSizes[i];
			}

			if (distMag <= turnToObjectDistanceSqr) {
				if (!isObject[i]) {
					points[i].startSize = 0;
					gameObjects[i].SetActive(true);
					isObject[i] = true;
				}
			} else {
				if (isObject[i]) {
					points[i].startSize = pointSizes[i];
					gameObjects[i].SetActive(false);
					isObject[i] = false;
				}
			}
		}

		// Radar Sound
		if (playRadarSound) {
			if (!radarAudioSource.isPlaying)
				radarAudioSource.Play();
		} else {
			if (radarAudioSource.isPlaying)
				radarAudioSource.Stop();
		}

		// Set Particles
		ParticleSystem.SetParticles(points, points.Length);
		radarParticleSys.SetParticles(radarPoints, radarPoints.Length);
	}


	private void CreateStars() {
		points = new ParticleSystem.Particle[starsMax];
		radarPoints = new ParticleSystem.Particle[starsMax];
		pointSizes = new float[starsMax];
		radarPointSizes = new float[starsMax];
		isObject = new bool[starsMax];
		isHostile = new bool[starsMax];
		gameObjects = new GameObject[starsMax];

		for (int i = 0; i < starsMax; i++) {
			points[i].position = Random.insideUnitSphere.normalized * (Random.Range(starDistance.x / starDistance.y, 1)) * starDistance.y + tx.position;
			//points[i].startColor = new Color(1, 1, 1, 1);
			points[i].startSize = Random.Range(starSize.x, starSize.y);
			pointSizes[i] = points[i].startSize;

			radarPointSizes[i] = pointSizes[i] * sizeScaleMultiplier;

			generateGameObjectForParticle(i);

		}
	}

	private void generateGameObjectForParticle(int i) {
		GameObject starObj = starObjectsToGenerate[Random.Range(0, starObjectsToGenerate.Length)];
		// GameObject starObj = starObjectsToGenerate[0];
		gameObjects[i] = Instantiate(starObj, points[i].position, Quaternion.Euler(Vector3.zero));
		DontDestroyOnLoad(gameObjects[i]);

		planetScript = gameObjects[i].GetComponent<PlanetScript>();
		planetScript.radius = pointSizes[i] * 0.15f;
		planetScript.transform.rotation = Quaternion.Euler(new Vector3(Random.Range(-180, 180), Random.Range(-180, 180), Random.Range(-180, 180)));

		// COLOR
		Color.RGBToHSV(planetScript.lightColor, out float h, out float s, out float v);
		if (s > .2f) s = .2f;
		points[i].startColor = Color.HSVToRGB(h, s, v);
		//radarPoints[i].startColor = points[i].startColor;

		// Radar points color
		isHostile[i] = planetScript.isHostile;
		if (planetScript.isHostile) {
			radarPoints[i].startColor = radarParticleColorHostile; // Different color for hostile planets
		} else {
			radarPoints[i].startColor = radarParticleColor;
		}

		gameObjects[i].SetActive(false);
	}

	// Cleans gameobjects from memory
	public void Clean() {
		foreach (GameObject go in gameObjects) {
			Destroy(go);
		}
	}

}
