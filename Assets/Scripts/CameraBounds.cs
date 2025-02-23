using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBounds : MonoBehaviour {
	[SerializeField] private Transform topBounds;
	[SerializeField] private Transform rightBounds;
	[SerializeField] private Transform bottomBounds;
	[SerializeField] private Transform leftBounds;
	[SerializeField] private Camera mainCamera;

	/// <summary>
	/// The current height of the camera in world space
	/// </summary>
	public float CameraHeight => mainCamera.orthographicSize * 2f;

	/// <summary>
	/// The current width of the camera in world space
	/// </summary>
	public float CameraWidth => CameraHeight * mainCamera.aspect;

	private void Start ( ) {
		// Set the positions of the bounds
		topBounds.position = new Vector3(0f, CameraHeight / 2f + 0.5f, 0f);
		rightBounds.position = new Vector3(CameraWidth / 2f + 0.5f, 0f, 0f);
		bottomBounds.position = new Vector3(0f, -CameraHeight / 2f - 0.5f, 0f);
		leftBounds.position = new Vector3(-CameraWidth / 2f - 0.5f, 0f, 0f);

		// Set the scales of the bounds
		topBounds.localScale = new Vector3(CameraWidth, 1f, 1f);
		rightBounds.localScale = new Vector3(1f, CameraHeight, 1f);
		bottomBounds.localScale = new Vector3(CameraWidth, 1f, 1f);
		leftBounds.localScale = new Vector3(1f, CameraHeight, 1f);
	}
}
