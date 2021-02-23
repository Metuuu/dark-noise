using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class UIButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	Text childText;
	string originalText;

    void Start()
    {
		childText = gameObject.GetComponentInChildren<Text>();
		originalText = childText.text;
	}


	public void OnPointerEnter(PointerEventData eventData) {
		childText.text = "> " + originalText;
	}

	public void OnPointerExit(PointerEventData eventData) {
		childText.text = originalText;
	}
}
