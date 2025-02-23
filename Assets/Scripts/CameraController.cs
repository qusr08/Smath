using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
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
		topBounds.position = new Vector3(0f, CameraHeight / 2f + 3f, 0f);
		rightBounds.position = new Vector3(CameraWidth / 2f + 3f, 0f, 0f);
		bottomBounds.position = new Vector3(0f, -CameraHeight / 2f - 3f, 0f);
		leftBounds.position = new Vector3(-CameraWidth / 2f - 3f, 0f, 0f);

		// Set the scales of the bounds
		topBounds.localScale = new Vector3(CameraWidth + 12f, 6f, 1f);
		rightBounds.localScale = new Vector3(6f, CameraHeight + 12f, 1f);
		bottomBounds.localScale = new Vector3(CameraWidth + 12f, 6f, 1f);
		leftBounds.localScale = new Vector3(6f, CameraHeight + 12f, 1f);
	}

	/// <summary>
	/// Shake the camera for a certain duration
	/// </summary>
	/// <param name="duration">The time in seconds for the camera to be shaking</param>
	public void ShakeCamera (float duration) {
		StopAllCoroutines( );
		StartCoroutine(IShakeCamera(duration));
	}
	
	private IEnumerator IShakeCamera (float duration) {
		// https://www.youtube.com/watch?v=lq7y0thMN1M&ab_channel=TheTrueDuck
		float elapsed = 0.0f;
		float currentMagnitude = 1f;

		while (elapsed < duration) {
			float x = (Random.value - 0.5f) * currentMagnitude;
			float y = (Random.value - 0.5f) * currentMagnitude;

			mainCamera.transform.localPosition = new Vector3(x, y, 0);

			elapsed += Time.deltaTime;
			currentMagnitude = (1 - (elapsed / duration)) * (1 - (elapsed / duration));

			yield return null;
		}

		mainCamera.transform.localPosition = Vector3.zero;
	}

}
