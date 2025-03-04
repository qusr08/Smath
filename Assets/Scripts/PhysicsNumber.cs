using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Operation {
	NONE, PLUS, MINUS, MULTIPLY, DIVIDE
}

public class PhysicsNumber : MonoBehaviour {
	[SerializeField] private MousePoint mousePoint;
	[SerializeField] private GameManager gameManager;
	[SerializeField] private MenuManager menuManager;
	[Space]
	[SerializeField] private List<Sprite> numberSprites;
	[SerializeField] private List<Sprite> operationSprites;
	[SerializeField] private List<Color> operationColors;
	[Space]
	[SerializeField] private Rigidbody2D _rigidBody2D;
	[SerializeField] private List<SpriteRenderer> digitSpriteRenderers;
	[SerializeField] private List<PolygonCollider2D> digitPolygonColliders;
	[SerializeField] private List<GameObject> digitGameObjects;
	[SerializeField] private SpriteRenderer operationSpriteRenderer;
	[SerializeField] private PolygonCollider2D operationPolygonCollider;
	[SerializeField] private GameObject operationGameObject;
	[Space]
	[SerializeField, Range(0, 999)] private int _value;
	[SerializeField] private Operation _operation;
	public bool CanSmash;
	// public string CalculationPath = "";
	[SerializeField, Range(0f, 1f)] private float digitGap = 0.2f;
	[SerializeField, Range(0f, 10f)] private float smashSpeed = 4f;

	/// <summary>
	/// The rigidbody of this physics number
	/// </summary>
	public Rigidbody2D RigidBody2D => _rigidBody2D;

	/// <summary>
	/// The operation sprite renderer of this physics number
	/// </summary>
	public SpriteRenderer OperationSpriteRenderer { get => operationSpriteRenderer; private set => operationSpriteRenderer = value; }

	/// <summary>
	/// The current value of this number
	/// </summary>
	public int Value {
		get => _value;
		set {
			// If the previous value was 0, then set the calculation path to whatever the new number is
			// if (_value == 0 && value != 0) {
				// CalculationPath = $"{value}";
			// }

			// Make sure the value stays within the valid range
			_value = Mathf.Clamp(value, 0, 999);

			// Update the sprite renderers for each place of the number
			int digitIndex = 0;
			bool hasFoundValue = false;
			for (int place = 100; place >= 1; place /= 10) {
				// If the value has a digit at the current place number, then set the sprite of the number
				// This will make sure the value is placed at the right spot within the digit sprite renderers
				if (hasFoundValue || (_value / place) % 10 > 0) {
					// Set the sprite of this number based on the new value
					digitSpriteRenderers[digitIndex].enabled = true;
					digitSpriteRenderers[digitIndex].sprite = numberSprites[(_value / place) % 10];

					// Refresh the polygon collider since the sprite was updated
					// Only update the sprite and the collider if there is a number
					Destroy(digitPolygonColliders[digitIndex]);
					digitPolygonColliders[digitIndex] = digitGameObjects[digitIndex].AddComponent<PolygonCollider2D>( );

					digitIndex++;
					hasFoundValue = true;
				}
			}

			// Disable the rest of the sprite renderers
			for (; digitIndex < 3; digitIndex++) {
				// Set the sprite of this number based on the new value
				digitSpriteRenderers[digitIndex].sprite = numberSprites[0];

				// Make it so the polygon collider is no longer enabled because it is not used
				digitSpriteRenderers[digitIndex].enabled = false;
				digitPolygonColliders[digitIndex].enabled = false;
			}

			// Update the offset of all the sprite renderers
			UpdateOffsets( );
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
			foreach (SpriteRenderer digitSpriteRenderer in digitSpriteRenderers) {
				digitSpriteRenderer.color = operationColors[(int) _operation];
			}

			// Refresh the polygon collider since the sprite was updated
			// Only do this if there is an operation
			if (_operation != Operation.NONE) {
				operationSpriteRenderer.enabled = true;

				Destroy(operationPolygonCollider);
				operationPolygonCollider = operationGameObject.AddComponent<PolygonCollider2D>( );
			} else {
				operationSpriteRenderer.enabled = false;
				operationPolygonCollider.enabled = false;
			}

			// Update the offset of all the sprite renderers
			UpdateOffsets( );
		}
	}

	private void Awake ( ) {
		mousePoint = FindObjectOfType<MousePoint>( );
		gameManager = FindObjectOfType<GameManager>( );
		menuManager = FindObjectOfType<MenuManager>( );
	}

	private void Start ( ) {
		Operation = Operation;
		Value = Value;
	}

	private void OnCollisionEnter2D (Collision2D collision) {
		// If the game is not currently being played right now, then do not collide
		if (menuManager.MenuState != MenuState.GAME) {
			return;
		}

		// If this physics number was not just thrown by the player, it cannot smash into anything
		if (!CanSmash) {
			return;
		}

		// Since this shape has collided with something, it can no longer smash into anything else afterwards
		CanSmash = false;

		// If this physics number speed is moving too slow to smash, then do nothing
		if (RigidBody2D.velocity.magnitude < smashSpeed) {
			return;
		}

		// If the collision was with another physics number, then try to merge the two of them together
		// If the collision was with the bounds of the screen, try to break this number apart
		if ((LayerMask.GetMask("Physics Number") & (1 << collision.gameObject.layer)) > 0) {
			gameManager.MergePhysicsNumbers(this, collision.transform.GetComponent<PhysicsNumber>( ));
		} else if ((LayerMask.GetMask("Bounds") & (1 << collision.gameObject.layer)) > 0) {
			// We don't want splitting to make the random puzzles really easy
			// If we used set puzzles then we could add splitting back in
			// Need to fix that somehow
			gameManager.SplitPhysicsNumber(this);
		}
	}

	private void OnMouseDown ( ) {
		// If the game is not currently being played right now, then do not collide
		if (menuManager.MenuState != MenuState.GAME) {
			return;
		}

		mousePoint.LockedPhysicsNumber = this;
	}

	private void OnMouseUp ( ) {
		mousePoint.LockedPhysicsNumber = null;
	}

	/*private void Update ( ) {
		// Used on the main menu to despawn physics numbers when they fall too low 
		if (transform.position.y < -40f) {
			Destroy(gameObject);
		}
	}*/

	/// <summary>
	/// Update the offset of the operation object from the center of this game object
	/// </summary>
	private void UpdateOffsets ( ) {
		// If there is no value, then the operation should be right in the center of the object
		if (Value == 0) {
			operationGameObject.transform.localPosition = Vector3.zero;
			return;
		}

		float sumOffsetX = 0f;
		for (int i = 0; i < 3; i++) {
			// If the current sprite renderer is not enabled, then break from the loop
			if (!digitSpriteRenderers[i].enabled) {
				break;
			}

			// Add to the offset x for all sprite renderers besides the first one
			if (i > 0) {
				sumOffsetX += digitSpriteRenderers[i - 1].sprite.bounds.extents.x + digitGap + digitSpriteRenderers[i].sprite.bounds.extents.x;
			}

			// Set the transform of the digit object
			digitGameObjects[i].transform.localPosition = new Vector3(sumOffsetX, 0f, 0f);
		}

		// If there is no operation, then do not adjust the offset of it because it does not matter where it is
		if (Operation == Operation.NONE) {
			return;
		}

		// Adjust the position of the operation to the left of the first number
		float operationXOffset = operationSpriteRenderer.sprite.bounds.extents.x + digitGap + digitSpriteRenderers[0].sprite.bounds.extents.x;
		operationGameObject.transform.localPosition = new Vector3(-operationXOffset, 0, 0);
	}
}
