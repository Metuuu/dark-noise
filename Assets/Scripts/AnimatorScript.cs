using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animation))]
public class AnimatorScript : MonoBehaviour
{
	//public Animation animation;
	public Action fadeInCallback;
	public Action fadeOutCallback;

	//enum Animation {
	//	FadingIn,
	//	Looping,
	//	FadingOut,
	//}

	enum AnimationState {
		Waiting = 0,
		FadingIn = 1,
		LoopingAnimationState = 2,
		FadingOut = 3,
	}

	new Animation animation;
	AnimationState state;

	public bool playOnEnable;
	public AnimationClip fadeInAnimation;
	public bool startsIn;
	public AnimationClip loopingAnimation;
	public bool useReversedInForOutAnim;
	public AnimationClip fadeOutAnimation;
	public bool disableOnFadeOut;

	public float fadeInSpeedMultiplier = 1;
	public float loopingSpeedMultiplier = 1;
	public float fadeOutSpeedMultiplier = 1;


	void Awake() {
		animation = GetComponent<Animation>();

		#region [ ---- Add animations ---- ]
		if (loopingAnimation != null) {
			loopingAnimation.legacy = true;
			animation.AddClip(loopingAnimation, "LoopingAnimation");
		}
		fadeInAnimation.legacy = true;
		animation.AddClip(fadeInAnimation, "FadeInAnimation");
		if (fadeOutAnimation) {
			fadeOutAnimation.legacy = true;
			animation.AddClip(fadeOutAnimation, "FadeOutAnimation");
		}
		#endregion

	}


	private void OnEnable() {
		if (playOnEnable) {
			if (startsIn) {
				if (loopingAnimation != null) {
					animation.Play("LoopingAnimation");
					animation["LoopingAnimation"].speed = 1 * loopingSpeedMultiplier;
				}
				state = AnimationState.LoopingAnimationState;
			} else {
				state = AnimationState.FadingIn;
				animation.Play("FadeInAnimation");
				animation["FadeInAnimation"].speed = 1 * fadeInSpeedMultiplier;
			}
		} if (!playOnEnable) {
			if (startsIn) {
				if (loopingAnimation != null) {
					animation.Play("LoopingAnimation");
					animation["LoopingAnimation"].time = 0;
					animation["LoopingAnimation"].speed = 0;
				} else {
					animation.Play("FadeInAnimation");
					animation["FadeInAnimation"].time = animation["FadeInAnimation"].length;
				}
			} else if (fadeOutAnimation != null) {
				animation.Play("FadeOutAnimation");
				animation["FadeOutAnimation"].time = animation["FadeOutAnimation"].length;
			} else if (fadeInAnimation != null) {
				animation.Play("FadeInAnimation");
				animation["FadeInAnimation"].time = 0;
				animation["FadeInAnimation"].speed = 0;
			}
			state = AnimationState.Waiting;
		}
	}


	public void PlayLoopingAnimation() {
		animation.Play("LoopingAnimation");
		animation["LoopingAnimation"].speed = 1 * loopingSpeedMultiplier;
		state = AnimationState.LoopingAnimationState;
	}

	public void FadeIn(Action callback = null) {
		if (callback != null) {
			fadeInCallback = callback;
		}
		if (!isActiveAndEnabled) {
			gameObject.SetActive(true);
			if (playOnEnable) {
				return;
			}
		}
		animation.Play("FadeInAnimation");
		animation["FadeInAnimation"].speed = 1 * fadeInSpeedMultiplier;
		state = AnimationState.FadingIn;
	}

	public void FadeOut(Action callback = null) {
		if (useReversedInForOutAnim) {
			animation.Play("FadeInAnimation");
			animation["FadeInAnimation"].time = animation["FadeInAnimation"].length;
			animation["FadeInAnimation"].speed = -1 * fadeOutSpeedMultiplier;
			state = AnimationState.FadingOut;
			if (callback != null) {
				fadeOutCallback = callback;
			}
		} else if (fadeOutAnimation != null) {
			animation.Play("FadeOutAnimation");
			animation["FadeOutAnimation"].speed = 1 * fadeOutSpeedMultiplier;
			state = AnimationState.FadingOut;
			if (callback != null) {
				fadeOutCallback = callback;
			}
		}
	}

	private void FixedUpdate() {
		if (!animation.isPlaying) {

			if (state == AnimationState.FadingIn) {
				if (loopingAnimation != null) {
					animation.Play("LoopingAnimation");
					animation["LoopingAnimation"].speed = 1 * loopingSpeedMultiplier;
				}
				state = AnimationState.LoopingAnimationState;
				fadeInCallback?.Invoke();
				fadeInCallback = null;
			}

			if (state == AnimationState.LoopingAnimationState) {
				if (loopingAnimation != null) {
					animation.Play("LoopingAnimation");
					animation["LoopingAnimation"].speed = 1 * loopingSpeedMultiplier;
				} else {
					state = AnimationState.Waiting;
				}
			}

			if (state == AnimationState.FadingOut) {
				fadeOutCallback?.Invoke();
				fadeOutCallback = null;
				state = AnimationState.Waiting;
				if (disableOnFadeOut) {
					gameObject.SetActive(false);
				}
			}

		}
	}

}
