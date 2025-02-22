using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Operation {
	NONE, PLUS, MINUS, MULTIPLY, DIVIDE
}

public class PhysicsNumber : MonoBehaviour {
	[SerializeField] private MousePoint mousePoint;
	[Space]
	[SerializeField] private List<Sprite> numberSprites;
	[SerializeField] private List<Sprite> operationSprites;
	[SerializeField] private List<Color> operationColors;
	[Space]
	[SerializeField] private SpriteRenderer numberSpriteRenderer;
	[SerializeField] private PolygonCollider2D numberPolygonCollider;
	[SerializeField] private SpriteRenderer operationSpriteRenderer;
	[SerializeField] private PolygonCollider2D operationPolygonCollider;
	[SerializeField] private GameObject operationGameObject;
	[SerializeField] private Rigidbody2D _rigidBody2D;
	[Space]
	[SerializeField, Range(-1, 9)] private int _value;
	[SerializeField] private Operation _operation;
	[SerializeField, Range(0f, 1f)] private float operationGap;

	/// <summary>
	/// The rigidbody of this physics number
	/// </summary>
	public Rigidbody2D RigidBody2D => _rigidBody2D;

	/// <summary>
	/// The current value of this number
	/// </summary>
	public int Value {
		get => _value;
		set {
			// Make sure the value stays within the valid range
			_value = Mathf.Clamp(value, -1, 9);

			// Set the sprite of this number based on the new value
			numberSpriteRenderer.sprite = numberSprites[_value + 1];

			// Refresh the polygon collider since the sprite was updated
			// Only update the sprite and the collider if there is a number
			numberPolygonCollider.enabled = (_value >= 0);
			if (_value >= 0) {
				Destroy(numberPolygonCollider);
				numberPolygonCollider = gameObject.AddComponent<PolygonCollider2D>( );
			}

			UpdateOperationOffset( );
		}
	}

	/// <summary>
	/// The current operation applied to this number
	/// </summary>
	public Operation Operation {
		get => _operation;
		set {
			_operation = value;

			// Set the sprite of this operation based on the new value
			operationSpriteRenderer.sprite = operationSprites[(int) _operation];

			// Apply a color to the sprite based on the operation color
			operationSpriteRenderer.color = operationColors[(int) _operation];
			numberSpriteRenderer.color = operationColors[(int) _operation];

			// Refresh the polygon collider since the sprite was updated
			// Only do this if there is an operation
			operationPolygonCollider.enabled = (_operation != Operation.NONE);
			if (_operation != Operation.NONE) {
				Destroy(operationPolygonCollider);
				operationPolygonCollider = operationGameObject.AddComponent<PolygonCollider2D>( );

				UpdateOperationOffset( );
			}
		}
	}

	private void Awake ( ) {
		mousePoint = FindObjectOfType<MousePoint>( );
	}

	private void Start ( ) {
		Value = Value;
		Operation = Operation;
	}

	private void OnMouseDown ( ) {
		mousePoint.LockedPhysicsNumber = this;
	}

	private void OnMouseUp ( ) {
		mousePoint.LockedPhysicsNumber = null;
	}

	/// <summary>
	/// Update the offset of the operation object from the center of this game object
	/// </summary>
	private void UpdateOperationOffset ( ) {
		// If there is no operation, then do not adjust the offset of it because it does not matter
		if (Operation == Operation.NONE) {
			return;
		}

		// If there is no value, then the operation should be right in the center of the object
		if (Value == -1) {
			operationGameObject.transform.localPosition = Vector3.zero;
			return;
		}

		// Adjust the operation so it is next to the number
		// Some of the numbers are different widths
		float offsetX = operationSpriteRenderer.sprite.bounds.extents.x + numberSpriteRenderer.sprite.bounds.extents.x + operationGap;
		operationGameObject.transform.localPosition = new Vector3(-offsetX, 0, 0);
	}
}
