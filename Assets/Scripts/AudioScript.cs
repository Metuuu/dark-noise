using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioScript : MonoBehaviour {

	public AudioSource audioSource;
	List<AudioClip> audioClips = new List<AudioClip>();
	bool loopLast;
	private bool startedPlaying;


	private void Awake() {
		audioSource = gameObject.GetComponent<AudioSource>();
	}


	void FixedUpdate() {

		if (audioClips.Count != 0) {
			if (!audioSource.isPlaying) {

				if (!startedPlaying) {
					startedPlaying = true;
				} else {
					audioClips.RemoveAt(0);
				}

				if (audioClips.Count != 0) {
					audioSource.clip = audioClips[0];
					if (audioClips.Count == 1) {
						audioSource.loop = loopLast;
					} else {
						audioSource.loop = false;
					}
					audioSource.Play();
				}

			}
		}
		
	}

	public void Play(AudioClip audioClip, bool loop = false) {
		audioSource.Stop();
		startedPlaying = false;
		loopLast = loop;
		audioClips = new List<AudioClip> { audioClip };
	}

	public void Play(List<AudioClip> audioClips, bool loopLast = false) {
		audioSource.Stop();
		startedPlaying = false;
		this.loopLast = loopLast;
		this.audioClips = audioClips;
	}

	public void Stop() {
		audioSource.Stop();
		startedPlaying = false;
		audioClips = new List<AudioClip>();
	}

}

