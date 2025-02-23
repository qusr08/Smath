using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler {
	[SerializeField] private RectTransform rectTransform;
	[SerializeField] private bool _isHidden;
	[Space]
	[SerializeField] private UnityEvent OnDown;

	public bool IsHidden {
		get => _isHidden;
		set {
			_isHidden = value;

			// When this button is either hidden or unhidden, change its position
			if (_isHidden) {
				Vector3 hiddenPosition = new Vector3(rectTransform.anchoredPosition.x, 500f, 0f);
				rectTransform.anchoredPosition = hiddenPosition;
				toPosition = hiddenPosition;
			} else {
				Vector3 unhiddenPosition = new Vector3(rectTransform.anchoredPosition.x, 0f, 0f);
				rectTransform.anchoredPosition = unhiddenPosition;
				toPosition = unhiddenPosition;
			}
		}
	}

	private Vector3 toPosition;
	private Vector3 toVelocity;

	private void Awake ( ) {
		toPosition = new Vector3(rectTransform.anchoredPosition.x, 0f, 0f);
	}

	private void Update ( ) {
		// Smoothly move the button when the pointer is over it
		rectTransform.anchoredPosition = Vector3.SmoothDamp(rectTransform.anchoredPosition, toPosition, ref toVelocity, 0.15f);
	}

	public void OnPointerEnter (PointerEventData eventData) {
		if (!IsHidden) {
			toPosition = new Vector3(rectTransform.anchoredPosition.x, -50f, 0f);
		}
	}

	public void OnPointerExit (PointerEventData eventData) {
		if (!IsHidden) {
			toPosition = new Vector3(rectTransform.anchoredPosition.x, 0f, 0f);
		}
	}

	public void OnPointerDown (PointerEventData eventData) {
		if (!IsHidden) {
			OnDown?.Invoke( );
		}
	}
}
