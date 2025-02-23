using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
	[SerializeField] private GameObject physicsNumberPrefab;

	/// <summary>
	/// Try to split a physics number into its operation and value
	/// </summary>
	/// <param name="physicsNumber">The physics number to split</param>
	/// <returns>true if the split was successful, false otherwise</returns>
	public bool SplitPhysicsNumber (PhysicsNumber physicsNumber) {
		// If the physics number has no operation or no number component, then return false as this cannot be split
		if (physicsNumber.Operation == Operation.NONE || physicsNumber.Value <= 0) {
			return false;
		}

		// Create a new physics number just for the operation of the split number
		Vector3 splitPosition = physicsNumber.transform.position + (Vector3.right * 2f);
		PhysicsNumber splitPhysicsNumber = Instantiate(physicsNumberPrefab, splitPosition, Quaternion.identity).GetComponent<PhysicsNumber>( );
		splitPhysicsNumber.Operation = physicsNumber.Operation;
		splitPhysicsNumber.RigidBody2D.velocity = physicsNumber.RigidBody2D.velocity;

		// Set the original physics number to have no operation and just the value
		physicsNumber.Operation = Operation.NONE;

		return true;
	}

	/// <summary>
	/// Try to merge two physics numbers together
	/// </summary>
	/// <param name="physicsNumber">The physics number that the values will be merged into</param>
	/// <param name="other"></param>
	/// <returns></returns>
	public bool MergePhysicsNumbers (PhysicsNumber physicsNumber, PhysicsNumber other) {
		// v = has value, o = has operation
		// vo & v = T
		// v & vo = T
		// v & o = T
		// o & v = T

		// vo & o F
		// o & vo = F
		// vo & vo = F
		// v & v = F
		// o & o = F

		// If both the numbers have operators, then do not merge
		// Similarly, if neither of the numbers have operators, then do not merge
		if ((physicsNumber.Operation != Operation.NONE) == (other.Operation != Operation.NONE)) {
			return false;
		}

		// Need to make sure that the physics number with the operation is the one applying it to the other one
		// Order for subtraction/division is important
		if (other.Operation != Operation.NONE) {
			// If the other physics number has no value, then just merge the two and do no calculations
			// If it does have a value, then use that operation on the other physics number
			if (other.Value == 0) {
				physicsNumber.Operation = other.Operation;
			} else {
				switch (other.Operation) {
					case Operation.PLUS:
						physicsNumber.Value += other.Value;
						break;
					case Operation.MINUS:
						physicsNumber.Value -= other.Value;
						break;
					case Operation.MULTIPLY:
						physicsNumber.Value *= other.Value;
						break;
					case Operation.DIVIDE:
						physicsNumber.Value /= other.Value;
						break;
				}
			}
		} else if (physicsNumber.Operation != Operation.NONE) {
			// If the physics number has no value, then just merge the two and do no calculations
			// If it does have a value, then use that operation with the physics number
			if (physicsNumber.Value == 0) {
				physicsNumber.Value = other.Value;
			} else {
				switch (physicsNumber.Operation) {
					case Operation.PLUS:
						physicsNumber.Value = other.Value + physicsNumber.Value;
						break;
					case Operation.MINUS:
						physicsNumber.Value = other.Value - physicsNumber.Value;
						break;
					case Operation.MULTIPLY:
						physicsNumber.Value = other.Value * physicsNumber.Value;
						break;
					case Operation.DIVIDE:
						physicsNumber.Value = other.Value / physicsNumber.Value;
						break;
				}

				// Reset the operation after it was used
				physicsNumber.Operation = Operation.NONE;
			}
		}

		// If the physics number has no more value (it went negative or reached 0) then destroy it
		// For right now, we want no negative numbers
		if (physicsNumber.Operation == Operation.NONE && physicsNumber.Value == 0) {
			Destroy(physicsNumber.gameObject);
		}

		// Destroy the other physics object because they have now been merged
		Destroy(other.gameObject);

		return true;
	}
}
