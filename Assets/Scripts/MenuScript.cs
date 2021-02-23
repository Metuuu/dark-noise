using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class MenuScript : MonoBehaviour
{
	AudioSource audioSource;
	[SerializeField] AudioClip menuMusic;
	[SerializeField] AudioClip screenOnSound;
	[SerializeField] AudioClip screenOffSound;
	[SerializeField] AudioClip clickSound;
	[SerializeField] AudioSource audioSourceEffects;
	bool intro;
	Material planetMaterial;
	bool startPressed;
	bool quitPressed;
	bool pressAnyKeyToContinue;

	[SerializeField] AnimatorScript animatorIntroMusic;
	[SerializeField] AnimatorScript animatorIntroLogo;
	[SerializeField] AnimatorScript animatorScreenEffect;
	[SerializeField] AnimatorScript animatorMusic;
	[SerializeField] AnimatorScript animatorLoading;
	[SerializeField] AnimatorScript animatorGuide;
	[SerializeField] AnimatorScript animatorMenuItems;
	[SerializeField] AnimatorScript animatorPlanet;
	[SerializeField] AnimatorScript animatorPressAnyKeyToContinue;


	public void Start() {
		audioSource = GetComponent<AudioSource>();
		StartIntro();
	}

	public void FixedUpdate() {

		// Intro
		if (intro && Input.anyKeyDown) {
			EndIntro();
		}

		// "Press any key to continue" -> Start Game
		if (pressAnyKeyToContinue && Input.anyKeyDown) {
			pressAnyKeyToContinue = false;
			audioSourceEffects.PlayOneShot(clickSound);
			animatorPressAnyKeyToContinue.FadeOut();
			animatorMusic.FadeOut();
			animatorGuide.FadeOut(() => {
				GameObject.FindWithTag("Player").GetComponent<CameraScript>().StartGame();
				SceneManager.UnloadSceneAsync("MainMenu");
			});
		}

	}

	public async void StartIntro() {
		intro = true;
		await Task.Delay(TimeSpan.FromSeconds(2f));
		animatorIntroLogo.FadeIn();
		await Task.Delay(TimeSpan.FromSeconds(3.5f));
		animatorIntroLogo.FadeOut();
		await Task.Delay(TimeSpan.FromSeconds(1f));
		if (intro) {
			EndIntro();
		}
	}

	public void EndIntro() {
		intro = false;
		animatorIntroMusic.FadeOut();
		animatorIntroLogo.gameObject.SetActive(false);
		OpenMenu();
	}

	public async void OpenMenu() {
		await Task.Delay(TimeSpan.FromSeconds(0.5f));
		audioSourceEffects.PlayOneShot(screenOnSound);
		animatorScreenEffect.FadeIn(async () => {
			await Task.Delay(TimeSpan.FromSeconds(0.5f));
			//animatorMusic.FadeIn();
			animatorPlanet.FadeIn();
			animatorMenuItems.FadeIn();
			audioSource.clip = menuMusic;
			audioSource.loop = true;
			audioSource.Play();
		});
	}

	public void StartGame() {
		if (startPressed || quitPressed) return;
		startPressed = true;
		audioSourceEffects.PlayOneShot(clickSound);

		animatorMenuItems.FadeOut();
		animatorPlanet.FadeOut(async () => {
			animatorLoading.FadeIn();
			await Task.Delay(TimeSpan.FromSeconds(1f));
			animatorGuide.FadeIn();
			await Task.Delay(TimeSpan.FromSeconds(1.1f));
			SceneManager.LoadSceneAsync("NoiseWorld", LoadSceneMode.Additive);
			await Task.Delay(TimeSpan.FromSeconds(1f));
			pressAnyKeyToContinue = true;
			animatorLoading.FadeOut();
			animatorPressAnyKeyToContinue.FadeIn();
		});
	}

	public void Quit() {
		if (startPressed || quitPressed) return;
		quitPressed = true;
		animatorMusic.fadeOutSpeedMultiplier = 3;
		animatorMusic.FadeOut();
		audioSourceEffects.PlayOneShot(screenOffSound);
		animatorScreenEffect.FadeOut(async () => {
			await Task.Delay(TimeSpan.FromSeconds(0.5f));
			Application.Quit();
		});
	}


	Color SetAlpha(Color color, float alpha) {
		return new Color(color.r, color.g, color.b, alpha);
	}

}
