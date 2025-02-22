using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Operation {
	NONE, PLUS, MINUS, MULTIPLY, DIVIDE
}

public class PhysicsNumber : MonoBehaviour {
	[SerializeField] private List<Sprite> numberSprites;
	[SerializeField] private List<Sprite> operationSprites;
	[SerializeField] private List<Color> operationColors;
	[Space]
	[SerializeField] private SpriteRenderer numberSpriteRenderer;
	[SerializeField] private PolygonCollider2D numberPolygonCollider;
	[SerializeField] private SpriteRenderer operationSpriteRenderer;
	[SerializeField] private PolygonCollider2D operationPolygonCollider;
	[SerializeField] private GameObject operationGameObject;
	[Space]
	[SerializeField, Range(0, 9)] private int _value;
	[SerializeField] private Operation _operation;
	[SerializeField, Range(0f, 1f)] private float operationGap;

	/// <summary>
	/// The current value of this number
	/// </summary>
	public int Value {
		get => _value;
		set {
			// Make sure the value stays within the valid range
			_value = Mathf.Clamp(value, 0, 9);

			// Set the sprite of this number based on the new value
			numberSpriteRenderer.sprite = numberSprites[_value];

			// Refresh the polygon collider since the sprite was updated
			Destroy(numberPolygonCollider);
			numberPolygonCollider = gameObject.AddComponent<PolygonCollider2D>( );

			// Adjust the operation so it is next to the number
			// Some of the numbers are different widths
			float offsetX = operationSpriteRenderer.sprite.bounds.extents.x + numberSpriteRenderer.sprite.bounds.extents.x + operationGap;
			operationGameObject.transform.position = new Vector3(transform.position.x - offsetX, transform.position.y, 0);
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
			operationPolygonCollider.enabled = false;
			if (_operation != Operation.NONE) {
				Destroy(operationPolygonCollider);
				operationPolygonCollider = operationGameObject.AddComponent<PolygonCollider2D>( );
			}
		}
	}

	private void Start ( ) {
		Value = Value;
		Operation = Operation;
	}
}
