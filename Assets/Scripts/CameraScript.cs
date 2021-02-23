using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Threading.Tasks;

public class CameraScript : MonoBehaviour
{
	Camera hudCamera;
	Camera renderCamera;
	AnimatorScript hudCameraAnimator;
	RenderTexture hudCameraTexture;
	RenderTexture spaceCameraTexture;
	[SerializeField] Image renderCameraPanel;
	Material renderCameraMaterial;


	new Camera camera;
	new Rigidbody rigidbody;
	AudioListener audioListener;
	NoiseTest scriptNoiseTest;
	AudioScript audioScript;

	[SerializeField] int desiredFrameRate = 60;
	[SerializeField] float timeToFadeIn = 2f;
	[SerializeField] AudioClip soundStartGame;

	[SerializeField] AudioSource audioSourceEffects;
	[SerializeField] AudioSource audioSourceBackgroundMusic;
	[SerializeField] AudioClip musicBackground;
	[SerializeField] AudioClip soundStarshipMotor;
	[SerializeField] AudioClip soundStarshipTurboStart;
	[SerializeField] AudioClip soundStarshipTurboStop;
	[SerializeField] AnimationCurve speedToVolume = AnimationCurve.Linear(0, 0, 1, 0);
	[SerializeField] AnimationCurve speedToMusicVolume = AnimationCurve.Linear(0, 0, 1, 0);
	[SerializeField] AnimationCurve speedToOffsetMultiplier = AnimationCurve.Linear(0, 0, 1, 0);
	[SerializeField] AnimationCurve speedToTextureZOffset = AnimationCurve.Linear(0, 0, 1, 0);
	[SerializeField] AnimationCurve speedToFieldOfView = AnimationCurve.Linear(0, 0, 1, 0);

	[SerializeField] private float baseAcceleration = 0.5f;
	[SerializeField] private float baseSpeed = 0.5f;
	[SerializeField] private float movementAcceleration = 1.5f;
	[SerializeField] private float movementSpeed = 1f;
	[SerializeField] private float turboAcceleration = 2f;
	[SerializeField] private float turboSpeed = 1.2f;
	[SerializeField] private float shakeIntensity = 1f;

	enum MovementType {
		Base,
		Movement,
		HyperDrive,
		Brake,
	}
	MovementType movementType;
	MovementType lastMovementType;

	[SerializeField] float speedH = 2.0f;
	[SerializeField] float speedV = 2.0f;
	//[Range(0.0f, 1.0f)]
	//[SerializeField] public float glidePercentage = 0.2f;


	[SerializeField] GameObject laser;
	[SerializeField] float laserSpeed = 100f;
	[SerializeField] float laserLoadTime = 1f;
	[SerializeField] AudioClip shootLaserSound;


	// Other
	float yaw = 0.0f;
	float pitch = 0.0f;
	float rotationSpeedMultiplier = 1;
	float targetSpeed = 0.0f;
	float acceleration = 0.0f;
	bool isShaking = false;
	Vector3 lastShakePos = Vector3.zero;
	Vector3 localVel = Vector3.zero;
	Vector3 shakePos = Vector3.zero;
	Vector3 lastHudCameraRotation = Vector3.zero;
	float laserLoadTimer = 0f;

	static readonly string RULE_REASON_LOADING = "loading";
	static readonly string RULE_REASON_HYPERDRIVE = "hyperdrive";

	Rule ruleRotating = new Rule(RULE_REASON_LOADING);
	Rule ruleMovement = new Rule(RULE_REASON_LOADING);
	Rule ruleShooting = new Rule(RULE_REASON_LOADING);
	Rule ruleReloading = new Rule(RULE_REASON_LOADING);



	void Awake()
    {
		audioListener = GetComponent<AudioListener>();
		audioListener.enabled = false;
		camera = transform.Find("SpaceCamera").GetComponent<Camera>();
		hudCamera = GameObject.FindWithTag("HUD_Camera").GetComponent<Camera>();
		hudCameraAnimator = hudCamera.gameObject.GetComponent<AnimatorScript>();
		renderCameraPanel = GameObject.FindWithTag("RenderCameraImage").GetComponent<Image>();
		renderCamera = Camera.main;

		rigidbody = gameObject.GetComponent<Rigidbody>();
		audioScript = gameObject.GetComponent<AudioScript>();
		scriptNoiseTest = gameObject.GetComponent<NoiseTest>();

		Application.targetFrameRate = desiredFrameRate;

		renderCameraMaterial = renderCameraPanel.material;
		hudCameraTexture = new RenderTexture(1920, 1080, 0, RenderTextureFormat.Default, 0);
		spaceCameraTexture = new RenderTexture(1920, 1080, 0, RenderTextureFormat.Default, 0);
		hudCameraTexture.antiAliasing = 2;
		spaceCameraTexture.antiAliasing = 2;
		hudCamera.targetTexture = hudCameraTexture;
		camera.targetTexture = spaceCameraTexture;

		if (GameObject.FindWithTag("MainMenuCamera") != null) {
			//renderCameraMaterial.SetFloat("_Fade", 0);
		} else {
			StartGame();
		}
	}


	public async void StartGame() {
		audioListener.enabled = true;
		await Task.Delay(System.TimeSpan.FromSeconds(1f));
		//audioSourceEffects.PlayOneShot(soundStartGame);

		audioSourceBackgroundMusic.clip = musicBackground;
		audioSourceBackgroundMusic.loop = true;
		audioSourceBackgroundMusic.Play();

		hudCameraAnimator.FadeOut();
		ruleRotating.Allow(RULE_REASON_LOADING);
		ruleMovement.Allow(RULE_REASON_LOADING);

		await Task.Delay(System.TimeSpan.FromSeconds(1.4f));
		ruleShooting.Allow(RULE_REASON_LOADING);
		ruleReloading.Allow(RULE_REASON_LOADING);
	}


	private void OnApplicationQuit() {
		if (renderCameraMaterial) {
			renderCameraMaterial.SetTexture("_SpaceTexture", null);
			renderCameraMaterial.SetTexture("_HUDTexture", null);
			renderCameraMaterial.SetFloat("_Fade", 1);
		}
	}

	void Update() {
		if (Input.GetKey(KeyCode.Escape)) {
			Application.Quit();
		}
	}


	void FixedUpdate() {

		#region [ ---- Game Start ---- ]

		//if (gameLoaded && fadeInTimer == null) {
		//	if (Input.anyKeyDown) {
		//		fadeInTimer = timeToFadeIn;
		//		audioSourceEffects.PlayOneShot(soundStartGame);
		//		audioSourceBackgroundMusic.clip = musicBackground;
		//		audioSourceBackgroundMusic.loop = true;
		//		audioSourceBackgroundMusic.Play();
		//		audioScript.Play(soundStarshipMotor, true);
		//		allowMovement = true;
		//		allowShooting = true;
		//	}
		//} else if (fadeInTimer != null && fadeInTimer > 0) {
		//	fadeInTimer -= Time.deltaTime;
		//	float fade = Mathf.Clamp01(1f - (float)fadeInTimer / timeToFadeIn);
		//	audioSourceBackgroundMusic.volume = fade;
		//	renderCameraMaterial.SetFloat("_Fade", fade);
		//}

		#endregion


		#region [ ---- Render To Main Camera ---- ]

		renderCameraMaterial.SetTexture("_SpaceTexture", spaceCameraTexture);
		renderCameraMaterial.SetTexture("_HUDTexture", hudCameraTexture);

		#endregion


		#region [ ---- Rotation ---- ]

		// Add HUD camera rotation
		transform.eulerAngles -= lastHudCameraRotation;
		transform.eulerAngles += hudCamera.transform.localEulerAngles;
		lastHudCameraRotation = hudCamera.transform.localEulerAngles;

		// Mouse movement -> Rotation
		if (ruleRotating.IsAllowed) {
			yaw = speedH * Mathf.Clamp(Input.GetAxis("Mouse X"), -4, 4);
			pitch = -speedV * Mathf.Clamp(Input.GetAxis("Mouse Y"), -4, 4);

			/*transform.Rotate(pitch, yaw, 0, Space.Self);*/

			rigidbody.AddTorque(transform.up * yaw * rotationSpeedMultiplier);
			rigidbody.AddTorque(-transform.right * pitch * rotationSpeedMultiplier);

			if (Input.GetKey(KeyCode.A)) {
				rigidbody.AddTorque(transform.forward * 2.2f * rotationSpeedMultiplier);
			} else if (Input.GetKey(KeyCode.D)) {
				rigidbody.AddTorque(-transform.forward * 2.2f * rotationSpeedMultiplier);
			}

			scriptNoiseTest.rotation = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, -transform.eulerAngles.z);
		}

		#endregion


		#region [ ---- Movement ---- ]
		if (ruleMovement.IsAllowed) {
			if (Input.GetAxis("Vertical") > 0) {
				if (Input.GetKey(KeyCode.LeftShift)) {
					movementType = MovementType.HyperDrive;
					rotationSpeedMultiplier = 0.25f;
				} else {
					movementType = MovementType.Movement;
				}
			} else if (Input.GetAxis("Vertical") < 0) {
				movementType = MovementType.Brake;
			} else {
				movementType = MovementType.Base;
			}
			if (movementType != lastMovementType) {
				if (movementType == MovementType.HyperDrive) {
					ruleShooting.DontAllow(RULE_REASON_HYPERDRIVE);
					audioScript.Play(new List<AudioClip> {
						soundStarshipTurboStart,
						soundStarshipMotor
					}, true);
				} else if (lastMovementType == MovementType.HyperDrive) {
					ruleShooting.Allow(RULE_REASON_HYPERDRIVE);
					rotationSpeedMultiplier = 1f;
					audioSourceEffects.PlayOneShot(soundStarshipTurboStop);
					audioScript.Play(new List<AudioClip> { soundStarshipMotor }, true);
				}
			}
			lastMovementType = movementType;

			switch (movementType) {
				case MovementType.Base:
					targetSpeed = baseSpeed;
					acceleration = baseAcceleration;
					break;
				case MovementType.Movement:
					targetSpeed = movementSpeed;
					acceleration = movementAcceleration;
					break;
				case MovementType.HyperDrive:
					targetSpeed = turboSpeed;
					acceleration = turboAcceleration;
					break;
			}

			localVel = transform.InverseTransformDirection(rigidbody.velocity);

			// - Brake -
			if (movementType == MovementType.Brake && localVel.z > baseSpeed) {
				rigidbody.AddForce(-localVel.z * 2f * transform.forward.normalized);
			}
			// - Accelerate -
			else if (localVel.z < targetSpeed) {

				if (localVel.x > 0 || localVel.x < 0) {
					rigidbody.AddForce(-localVel.x * 2f * transform.right.normalized);
				}
				if (localVel.y > 0 || localVel.y < 0) {
					rigidbody.AddForce(-localVel.y * 2f * transform.up.normalized);
				}


				var force = acceleration * transform.forward.normalized;
				//+ Input.GetAxis("Horizontal") * movementSpeed * transform.right.normalized;

				rigidbody.AddForce(force);
			}

			if (localVel.z > targetSpeed) {
				if (movementType != MovementType.HyperDrive && targetSpeed < rigidbody.velocity.magnitude * 1.0526f) {
					rigidbody.velocity = Vector3.ClampMagnitude(rigidbody.velocity, rigidbody.velocity.magnitude * (1f - 0.995f * Time.deltaTime));
				} else {
					rigidbody.velocity = Vector3.ClampMagnitude(rigidbody.velocity, rigidbody.velocity.magnitude - localVel.z + targetSpeed);
				}
			}

			scriptNoiseTest.offset = new Vector3(rigidbody.position.x, rigidbody.position.y, -rigidbody.position.z) / 10f;// * speedToOffsetMultiplier.Evaluate(localVel.magnitude);

			//localVel = transform.InverseTransformDirection(rigidbody.velocity);
		}

		#endregion


		#region [ ---- Shooting ---- ]

		if (ruleReloading.IsAllowed && laserLoadTimer > 0) {
			laserLoadTimer = Mathf.Max(0, laserLoadTimer - Time.deltaTime);
		}

		if (ruleShooting.IsAllowed && Input.GetMouseButton(0) && laserLoadTimer <= 0) {
			GameObject laser = Instantiate(this.laser, transform.position + transform.forward * 1f + transform.up * -1f, Quaternion.Euler(transform.forward));
			audioSourceEffects.PlayOneShot(shootLaserSound);
			laser.GetComponent<Rigidbody>().velocity = transform.forward * laserSpeed + rigidbody.velocity;
			laserLoadTimer = laserLoadTime;
		}

		#endregion


		#region [ ---- Shaking ---- ]

		isShaking = (localVel.magnitude > 100f);

		if (isShaking) {
			shakePos = new Vector3(
				Random.Range(-shakeIntensity, shakeIntensity),
				Random.Range(-shakeIntensity, shakeIntensity),
				0
			);
			shakePos = Vector3.Lerp(lastShakePos, shakePos, Time.deltaTime * 0.02f);
		} else {
			shakePos = Vector3.Lerp(lastShakePos, Vector3.zero, Time.deltaTime * 0.1f);
		}
		lastShakePos = shakePos;

		#endregion


		#region [ ---- Camera Field Of View / Texture Offset ---- ]

		// Zoom out in relation to speed
		//scriptNoiseTest.textureOffset = new Vector3(shakePos.x, shakePos.y, 0.1f);
		scriptNoiseTest.textureOffset = new Vector3(shakePos.x, shakePos.y, speedToTextureZOffset.Evaluate(localVel.magnitude));
		//camera.transform.localPosition = new Vector3(shakePos.x * 20, shakePos.y * 20, 0);
		//camera.fieldOfView = 70f;
		camera.fieldOfView = speedToFieldOfView.Evaluate(localVel.magnitude);
		//camera.fieldOfView = scriptNoiseTest.textureOffset.z / 0.15f * 60;

		#endregion


		#region [ ---- Volume ---- ]

		audioScript.audioSource.volume = speedToVolume.Evaluate(localVel.magnitude);
		audioSourceBackgroundMusic.volume = speedToMusicVolume.Evaluate(localVel.magnitude);

		#endregion

	}


}
