using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBounds : MonoBehaviour {
	[SerializeField] private Transform topBounds;
	[SerializeField] private Transform rightBounds;
	[SerializeField] private Transform bottomBounds;
	[SerializeField] private Transform leftBounds;

	private void Start ( ) {
		// Calculate the width and height of the camera
		float cameraHeight = Camera.main.orthographicSize * 2f;
		float cameraWidth = cameraHeight * Camera.main.aspect;

		// Set the positions of the bounds
		topBounds.position = new Vector3(0f, cameraHeight / 2f + 0.5f, 0f);
		rightBounds.position = new Vector3(cameraWidth / 2f + 0.5f, 0f, 0f);
		bottomBounds.position = new Vector3(0f, -cameraHeight / 2f - 0.5f, 0f);
		leftBounds.position = new Vector3(-cameraWidth / 2f - 0.5f, 0f, 0f);

		// Set the scales of the bounds
		topBounds.localScale = new Vector3(cameraWidth, 1f, 1f);
		rightBounds.localScale = new Vector3(1f, cameraHeight, 1f);
		bottomBounds.localScale = new Vector3(cameraWidth, 1f, 1f);
		leftBounds.localScale = new Vector3(1f, cameraHeight, 1f);
	}
}
