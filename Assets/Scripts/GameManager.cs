using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour {
	[SerializeField] private GameObject physicsNumberPrefab;
	[SerializeField] private CameraBounds cameraBounds;
	[SerializeField] private TextMeshProUGUI targetText;
	[Space]
	[SerializeField] private int _targetNumber;
	[SerializeField, Range(1, 20)] private int totalValueMin = 1;
	[SerializeField, Range(1, 20)] private int totalValueMax = 20;
	[SerializeField, Range(1, 5)] private int stepCountMin = 1;
	[SerializeField, Range(1, 5)] private int stepCountMax = 3;
	[SerializeField, Range(1, 5)] private int redHerringMin = 1;
	[SerializeField, Range(1, 5)] private int redHerringMax = 3;
	[SerializeField, Range(1, 5)] private int physicsNumberSpawnGrid = 2;

	/// <summary>
	/// The current target number of the game
	/// </summary>
	public int TargetNumber {
		get => _targetNumber;
		private set {
			_targetNumber = value;

			targetText.text = "= " + _targetNumber;
		}
	}

	private void Start ( ) {
		GenerateTargetNumber( );
	}

	/// <summary>
	/// Generate a new target number as well as new physics numbers
	/// </summary>
	public void GenerateTargetNumber ( ) {
		// Get a random step count
		int randomStepCount = Random.Range(stepCountMin, stepCountMax + 1);

		// Spawn in some red-herring numbers to make it a little more challenging
		// Also may allow for multiple solutions
		int redHerringCount = Random.Range(redHerringMin, redHerringMax + 1);

		// Make a variable for the sum of all the steps (will become the target number at the end)
		int sum = 0;

		// Generate a list of all possible spawns for the physics numbers (so no two numbers spawn on top of one another)
		List<Vector3> possibleSpawns = new List<Vector3>( );
		float halfCameraWidth = cameraBounds.CameraWidth / 2;
		float halfCameraHeight = cameraBounds.CameraHeight / 2;

		// Loop through all possible locations on the camera for physics numbers to spawn and check to see if they are good
		for (int x = (int) -halfCameraWidth; x <= (int) halfCameraWidth; x += physicsNumberSpawnGrid) {
			// If the x value is too close to the edge of the screen, then skip it
			if (Mathf.Abs(halfCameraWidth - Mathf.Abs(x)) < physicsNumberSpawnGrid) {
				continue;
			}

			for (int y = (int) -halfCameraHeight; y <= (int) halfCameraHeight; y += physicsNumberSpawnGrid) {
				// If the y value is too close to the edge of the screen, then skip it
				if (Mathf.Abs(halfCameraHeight - Mathf.Abs(y)) < physicsNumberSpawnGrid) {
					continue;
				}

				// If the spawn position is valid, then add it to the list
				possibleSpawns.Add(new Vector3(x, y, 0));
			}
		}

		// Calculate each of the steps to get to the target number
		for (int i = 0; i <= randomStepCount + redHerringCount; i++) {
			// Get a random spawn position for the physics number
			int randomSpawnIndex = Random.Range(0, possibleSpawns.Count);
			Vector3 spawnPosition = possibleSpawns[randomSpawnIndex];
			possibleSpawns.RemoveAt(randomSpawnIndex);

			// Generate a new physics number based on the properties above
			PhysicsNumber physicsNumber = Instantiate(physicsNumberPrefab, spawnPosition, Quaternion.identity).GetComponent<PhysicsNumber>( );

			// If i is greater than the random step count, then we should be making red herrings
			if (i > randomStepCount) {
				// Generated a random number for the value
				// Make sure it is not equal to the target number
				// There might be better ways to do this but I need to do this fast
				do {
					physicsNumber.Value = Random.Range(totalValueMin, totalValueMax + 1);
				} while (physicsNumber.Value == sum);

				// Generate a random operation to apply to that number
				float operationChance = Random.Range(0f, 1f);
				if (operationChance < 0.3f) {
					physicsNumber.Operation = Operation.PLUS;
				} else if (operationChance < 0.6f) {
					physicsNumber.Operation = Operation.MINUS;
				}
			} else {
				// Generate a random number to be the next step
				// Make sure the generated number is not the same thing as the current sum (as in the difference will be 0)
				int randomNextNumber;
				do {
					randomNextNumber = Random.Range(totalValueMin, totalValueMax + 1);
				} while (randomNextNumber == sum);

				// Calculate the difference between the sum and the new number
				// Then add the difference to the sum
				int difference = randomNextNumber - sum;
				sum += difference;

				// Use the difference as the value of the number so a solution is possible
				physicsNumber.Value = Mathf.Abs(difference);

				// Only generate an operation if this is not the first number being generated
				if (i != 0) {
					physicsNumber.Operation = (difference > 0 ? Operation.PLUS : Operation.MINUS);
				}
			}
		}

		// Set the target number once the last step has been generated
		TargetNumber = sum;
	}

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
	/// <param name="other">The other physics number to merge with</param>
	/// <returns>true if the merge was successful, false otherwise</returns>
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
						// If the two numbers cannot be divided, then return false as these numbers cannot merge
						if (physicsNumber.Value % other.Value != 0) {
							return false;
						}

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
						// If the two numbers cannot be divided, then return false as these numbers cannot merge
						if (other.Value % physicsNumber.Value != 0) {
							return false;
						}

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
