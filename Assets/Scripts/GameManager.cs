using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour {
	[SerializeField] private GameObject physicsNumberPrefab;
	[SerializeField] private GameObject smashParticleSystemPrefab;
	[SerializeField] private CameraController cameraController;
	[SerializeField] private TextMeshProUGUI targetText;
	[SerializeField] private MenuManager menuManager;
	[Space]
	[SerializeField] private int _targetNumber;
	[SerializeField, Range(1, 20)] private int _targetNumberMin = 1;
	[SerializeField, Range(1, 20)] private int _targetNumberMax = 20;
	[SerializeField, Range(1, 5)] private int stepCountMin = 1;
	[SerializeField, Range(1, 5)] private int stepCountMax = 3;
	[SerializeField, Range(1, 5)] private int redHerringMin = 1;
	[SerializeField, Range(1, 5)] private int redHerringMax = 3;
	[SerializeField, Range(1, 5)] private int physicsNumberSpawnGrid = 2;

	[HideInInspector] public List<int> SpawnedPhysicsNumberValues = new List<int>( );
	[HideInInspector] public List<Operation> SpawnedPhysicsNumberOperations = new List<Operation>( );

	/// <summary>
	/// The minimum value of the target number 
	/// </summary>
	public int TargetNumberMin => _targetNumberMin;

	/// <summary>
	/// The maximum value of the target number
	/// </summary>
	public int TargetNumberMax => _targetNumberMax;

	/// <summary>
	/// The current target number of the game
	/// </summary>
	public int TargetNumber {
		get => _targetNumber;
		private set {
			_targetNumber = value;

			targetText.text = "=" + _targetNumber;
		}
	}

	/// <summary>
	/// Called whenever the target number has been reached
	/// </summary>
	private void OnWin ( ) {
		menuManager.SetMenuState((int) MenuState.WIN);
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
		float halfCameraWidth = cameraController.CameraWidth / 2;
		float halfCameraHeight = cameraController.CameraHeight / 2;

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

		// If there were already some physics numbers spawned, just respawn the same ones
		if (SpawnedPhysicsNumberValues.Count != 0) {
			for (int i = 0; i < SpawnedPhysicsNumberValues.Count; i++) {
				// Get a random spawn position for the physics number
				int randomSpawnIndex = Random.Range(0, possibleSpawns.Count);
				Vector3 spawnPosition = possibleSpawns[randomSpawnIndex];
				possibleSpawns.RemoveAt(randomSpawnIndex);

				// Generate a new physics number based on the properties in the lists
				PhysicsNumber physicsNumber = Instantiate(physicsNumberPrefab, spawnPosition, Quaternion.identity).GetComponent<PhysicsNumber>( );
				physicsNumber.Operation = SpawnedPhysicsNumberOperations[i];
				physicsNumber.Value = SpawnedPhysicsNumberValues[i];
			}

			return;
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
				// Make sure no red herrings are equal to the final solution
				do {
					physicsNumber.Value = Random.Range(TargetNumberMin, TargetNumberMax + 1);
				} while (SpawnedPhysicsNumberValues.Contains(physicsNumber.Value));

				// Generate a random operation to apply to that number
				float operationChance = Random.Range(0f, 1f);
				if (operationChance < 0.2f) {
					physicsNumber.Operation = Operation.PLUS;
				} else if (operationChance < 0.4f) {
					physicsNumber.Operation = Operation.MINUS;
				}
			} else {
				// Generate a random number to be the next step
				// Make sure the generated number is not the same thing as the current sum (as in the difference will be 0)
				int randomNextNumber;
				do {
					randomNextNumber = Random.Range(TargetNumberMin, TargetNumberMax + 1);
				} while (randomNextNumber == sum || SpawnedPhysicsNumberValues.Contains(randomNextNumber));

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

			// Add the values of the latest spawned physics number to the lists
			SpawnedPhysicsNumberValues.Add(physicsNumber.Value);
			SpawnedPhysicsNumberOperations.Add(physicsNumber.Operation);
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

		// Spawn in a particle system
		SpawnSmashParticleSystem(physicsNumber.OperationSpriteRenderer.color, splitPosition - Vector3.right);

		if (physicsNumber.Value == TargetNumber) {
			OnWin( );
		}

		return true;
	}

	/// <summary>
	/// Try to merge two physics numbers together
	/// </summary>
	/// <param name="physicsNumber1">The physics number that the values will be merged into</param>
	/// <param name="physicsNumber2">The other physics number to merge with</param>
	/// <returns>true if the merge was successful, false otherwise</returns>
	public bool MergePhysicsNumbers (PhysicsNumber physicsNumber1, PhysicsNumber physicsNumber2) {
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
		if ((physicsNumber1.Operation != Operation.NONE) == (physicsNumber2.Operation != Operation.NONE)) {
			return false;
		}

		// Need to make sure that the physics number with the operation is the one applying it to the other one
		// Order for subtraction/division is important
		if (physicsNumber2.Operation != Operation.NONE) {
			// If the other physics number has no value, then just merge the two and do no calculations
			// If it does have a value, then use that operation on the other physics number
			if (physicsNumber2.Value == 0) {
				physicsNumber1.Operation = physicsNumber2.Operation;

				// Spawn in a particle system
				SpawnSmashParticleSystem(physicsNumber1.OperationSpriteRenderer.color, physicsNumber1.transform.position);
			} else {
				switch (physicsNumber2.Operation) {
					case Operation.PLUS:
						physicsNumber1.Value += physicsNumber2.Value;
						// physicsNumber1.CalculationPath += $"+{physicsNumber2.Value}";
						break;
					case Operation.MINUS:
						physicsNumber1.Value -= physicsNumber2.Value;
						// physicsNumber1.CalculationPath += $"-{physicsNumber2.Value}";
						break;
					case Operation.MULTIPLY:
						physicsNumber1.Value *= physicsNumber2.Value;
						// physicsNumber1.CalculationPath += $"×{physicsNumber2.Value}";
						break;
					case Operation.DIVIDE:
						// If the two numbers cannot be divided, then return false as these numbers cannot merge
						if (physicsNumber1.Value % physicsNumber2.Value != 0) {
							return false;
						}

						physicsNumber1.Value /= physicsNumber2.Value;
						// physicsNumber1.CalculationPath += $"÷{physicsNumber2.Value}";
						break;
				}

				// Reset the operation after it was used
				physicsNumber1.Operation = Operation.NONE;

				// Spawn in a particle system
				SpawnSmashParticleSystem(physicsNumber1.OperationSpriteRenderer.color, physicsNumber1.transform.position);

				// If the physics number has no more value (it went negative or reached 0) then destroy it
				// For right now, we want no negative numbers
				// If it is not 0, then check to see if we have reached the target number
				if (physicsNumber1.Value == 0) {
					Destroy(physicsNumber1.gameObject);
				} else if (physicsNumber1.Value == TargetNumber) {
					OnWin( );
				}
			}

			// Destroy the other physics object because they have now been merged
			Destroy(physicsNumber2.gameObject);
		} else if (physicsNumber1.Operation != Operation.NONE) {
			// If the physics number has no value, then just merge the two and do no calculations
			// If it does have a value, then use that operation with the physics number
			if (physicsNumber1.Value == 0) {
				physicsNumber2.Operation = physicsNumber1.Operation;

				// Spawn in a particle system
				SpawnSmashParticleSystem(physicsNumber2.OperationSpriteRenderer.color, physicsNumber2.transform.position);
			} else {
				switch (physicsNumber1.Operation) {
					case Operation.PLUS:
						physicsNumber2.Value += physicsNumber1.Value;
						// physicsNumber2.CalculationPath += $"+{physicsNumber1.Value}";
						break;
					case Operation.MINUS:
						physicsNumber2.Value -= physicsNumber1.Value;
						// physicsNumber2.CalculationPath += $"-{physicsNumber1.Value}";
						break;
					case Operation.MULTIPLY:
						physicsNumber2.Value *= physicsNumber1.Value;
						// physicsNumber2.CalculationPath += $"×{physicsNumber1.Value}";
						break;
					case Operation.DIVIDE:
						// If the two numbers cannot be divided, then return false as these numbers cannot merge
						if (physicsNumber2.Value % physicsNumber1.Value != 0) {
							return false;
						}

						physicsNumber2.Value /= physicsNumber1.Value;
						// physicsNumber2.CalculationPath += $"÷{physicsNumber1.Value}";
						break;
				}

				// Reset the operation after it was used
				physicsNumber2.Operation = Operation.NONE;

				// Spawn in a particle system
				SpawnSmashParticleSystem(physicsNumber2.OperationSpriteRenderer.color, physicsNumber2.transform.position);

				// If the physics number has no more value (it went negative or reached 0) then destroy it
				// For right now, we want no negative numbers
				// If it is not 0, then check to see if we have reached the target number
				if (physicsNumber2.Value == 0) {
					Destroy(physicsNumber2.gameObject);
				} else if (physicsNumber2.Value == TargetNumber) {
					OnWin( );
				}
			}

			// Destroy the other physics object because they have now been merged
			Destroy(physicsNumber1.gameObject);
		}

		return true;
	}

	/// <summary>
	/// Spawn a particle system for when two physics numbers smash together (or apart)
	/// </summary>
	/// <param name="color">The color to set the particle system</param>
	/// <param name="position">The position to spawn the particles at</param>
	private void SpawnSmashParticleSystem (Color color, Vector3 position) {
		ParticleSystem system = Instantiate(smashParticleSystemPrefab, position, Quaternion.identity).GetComponent<ParticleSystem>();
		ParticleSystem.MainModule mainSystem = system.main;
		mainSystem.startColor = color;
		system.Play( );

		cameraController.ShakeCamera(1f);
	}
}
