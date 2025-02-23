using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePoint : MonoBehaviour {
	[SerializeField] private SpringJoint2D springJoint2D;
	[SerializeField] private Camera mainCamera;
	[SerializeField] private PhysicsNumber _lockedPhysicsNumber;
	[Space]
	[SerializeField, Range(0.5f, 5f)] private float throwMultiplier = 3f;

	private Vector3 mouseWorldPositionFollower;
	private Vector3 mouseWorldVelocity;

	/// <summary>
	/// The physics number currently locked to this mouse point
	/// </summary>
	public PhysicsNumber LockedPhysicsNumber {
		get => _lockedPhysicsNumber;
		set {
			// Unconnect this mouse point from the previous locked physics number
			if (_lockedPhysicsNumber != null) {
				springJoint2D.connectedBody = null;
				_lockedPhysicsNumber.RigidBody2D.velocity = mouseWorldVelocity * throwMultiplier;
				_lockedPhysicsNumber.CanSmash = true;
			}

			_lockedPhysicsNumber = value;
			// hingeJoint2D.enabled = (_lockedPhysicsNumber != null);

			// Connect this mouse to the new locked physics number
			if (_lockedPhysicsNumber != null) {
				springJoint2D.autoConfigureConnectedAnchor = true;
				springJoint2D.autoConfigureDistance = true;
				springJoint2D.connectedBody = _lockedPhysicsNumber.RigidBody2D;
				springJoint2D.autoConfigureConnectedAnchor = false;
				springJoint2D.autoConfigureDistance = false;
			}
		}
	}

	private void Awake ( ) {
		mainCamera = Camera.main;
	}

	private void Update ( ) {
		// Calculate the current position of the mouse in world space
		Vector3 mouseWorldPosition = (Vector2) mainCamera.ScreenToWorldPoint(Input.mousePosition);

		// Update the mouse follower position
		mouseWorldPositionFollower = Vector3.SmoothDamp(mouseWorldPositionFollower, mouseWorldPosition, ref mouseWorldVelocity, 0.2f, Mathf.Infinity, Time.deltaTime);

		// Set this mouse point to the position of the mouse
		transform.position = mouseWorldPosition;
	}
}
