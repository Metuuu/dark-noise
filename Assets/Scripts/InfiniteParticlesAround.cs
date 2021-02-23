//Source: http://www.blenderfreak.com/blog/post/flythrough-camera-smooth-movement-unity3d/
// + Customized By Metu

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteParticlesAround : MonoBehaviour
{
	[SerializeField] private ParticleSystem ParticleSystem;
	private Transform tx;
	private ParticleSystem.Particle[] points;
	private float[] pointSizes;
	private float[] particleDistanceSqrs;

	[SerializeField] private bool rotation = false;
	[SerializeField] private int particlesMax = 100;
	[SerializeField] private Vector2 particleSize = new Vector2(0.5f, 1);
	[SerializeField] private Vector2 particleDistance = new Vector2(1, 10);
	[SerializeField] private Vector2 particleClipDistance = new Vector2(0.75f, 1);
	private float particleDistanceSqr;
	private float particleClipDistanceXSqr;
	private float particleClipDistanceSqr;

	float percent;
	float distMag;


	void Start() {
		tx = transform;
		particleDistanceSqr = particleDistance.y * particleDistance.y * 1.001f;
		particleClipDistanceXSqr = particleClipDistance.x * particleClipDistance.x;
		particleClipDistanceSqr = particleClipDistance.y * particleClipDistance.y;
	}


	void Update() {
		if (points == null)
			CreateStars();

		for (int i = 0; i < particlesMax; ++i) {

			distMag = (points[i].position - tx.position).sqrMagnitude;

			if (distMag > particleDistanceSqr) {
				//Debug.Log(distMag+ " > "+particleDistanceSqr);
				//var asdf = new Vector3(Random.Range(-90, 90), Random.Range(-90, 90), 0);
				//points[i].position = Quaternion.Euler(new Vector3(Random.Range(-90, 90), Random.Range(-90, 90), 0)) * new Vector3(1, 1, 1) * particleDistance.y + tx.position;
				points[i].position = Random.insideUnitSphere.normalized * particleDistance.y + tx.position;
				points[i].startSize = Random.Range(particleSize.x, particleSize.y);
				pointSizes[i] = points[i].startSize;
			}

			if ((distMag - particleClipDistanceXSqr) <= particleClipDistanceSqr) {
				percent = (distMag - particleClipDistanceXSqr) / particleClipDistanceSqr;
				//Debug.Log(percent);
				points[i].startColor = new Color(1, 1, 1, percent);
				points[i].startSize = Mathf.Clamp01(percent) * pointSizes[i];
				//if (percent < 0.01f) {
				//	points[i].position = Random.insideUnitSphere.normalized * particleDistance.y + tx.position;
				//}
			}
		}

		ParticleSystem.SetParticles(points, points.Length);
	}


	private void CreateStars() {
		points = new ParticleSystem.Particle[particlesMax];
		pointSizes = new float[particlesMax];

		for (int i = 0; i < particlesMax; i++) {
			points[i].position = Random.insideUnitSphere.normalized * (Random.Range(particleDistance.x / particleDistance.y, 1)) * particleDistance.y + tx.position;
			points[i].startColor = new Color(1, 1, 1, 1);
			points[i].startSize = Random.Range(particleSize.x, particleSize.y);
			if (rotation) {
				ParticleSystem.GetComponent<ParticleSystemRenderer>().alignment = ParticleSystemRenderSpace.World;
				points[i].rotation = Random.Range(-180f, 180f);
				//points[i].rotation3D = new Vector3(Random.Range(-180f, 180f), Random.Range(-180f, 180f), Random.Range(-180f, 180f));
			}
			pointSizes[i] = points[i].startSize;
		}
	}

}
