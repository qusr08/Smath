using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePoint : MonoBehaviour {
	[SerializeField] private HingeJoint2D hingeJoint2D;
	[SerializeField] private Camera mainCamera;
	[SerializeField] private PhysicsNumber _lockedPhysicsNumber;

	// private Vector3 lastMouseWorldPosition;
	// private Vector3 mouseWorldVelocity;

	/// <summary>
	/// The mouse's position in world space
	/// </summary>
	public Vector3 MouseWorldPosition {
		get {
			Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
			return new Vector3(mouseWorldPosition.x, mouseWorldPosition.y, 0);
		}
	}

	/// <summary>
	/// The physics number currently locked to this mouse point
	/// </summary>
	public PhysicsNumber LockedPhysicsNumber {
		get => _lockedPhysicsNumber;
		set {
			// Unconnect this mouse point from the previous locked physics number
			if (_lockedPhysicsNumber != null) {
				/// TO DO: Need to include the velocity into the calculation
				// _lockedPhysicsNumber.RigidBody2D.velocity = mouseWorldVelocity;
				hingeJoint2D.connectedBody = null;
			}

			_lockedPhysicsNumber = value;
			// hingeJoint2D.enabled = (_lockedPhysicsNumber != null);

			// Connect this mouse to the new locked physics number
			if (_lockedPhysicsNumber != null) {
				hingeJoint2D.connectedBody = _lockedPhysicsNumber.RigidBody2D;
				/// TO DO: Need to fix to incorperate rotation into the calculation
				// hingeJoint2D.connectedAnchor = MouseWorldPosition - _lockedPhysicsNumber.transform.position;
			}
		}
	}

	private void Awake ( ) {
		mainCamera = Camera.main;
		// lastMouseWorldPosition = MouseWorldPosition;
	}

	private void Update ( ) {
		// Set this mouse point to the position of the mouse
		Vector3 currentMousePosition = MouseWorldPosition;
		transform.position = currentMousePosition;

		// mouseWorldVelocity = currentMousePosition - lastMouseWorldPosition;
		// lastMouseWorldPosition = currentMousePosition;
	}
}
