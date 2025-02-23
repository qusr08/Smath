using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum MenuState {
	MENU, GAME, WIN
}

public class MenuManager : MonoBehaviour {
	[SerializeField] private SpriteRenderer transitionPanel;
	[SerializeField] private MenuButton homeButton;
	[SerializeField] private MenuButton resetButton;
	[SerializeField] private MenuButton playButton;
	[SerializeField] private TextMeshProUGUI playButtonText;
	[SerializeField] private TextMeshProUGUI creditsText;
	[SerializeField] private TextMeshProUGUI targetNumberText;
	[SerializeField] private TextMeshProUGUI winText;
	[SerializeField] private GameObject cameraBoundsContainer;
	[Space]
	[SerializeField] private GameManager gameManager;
	[SerializeField] private CameraController cameraController;
	[SerializeField] private GameObject physicsNumberPrefab;
	[Space]
	[SerializeField] private MenuState _menuState;

	private float spawnTimer;

	/// <summary>
	/// The current menu state of the game
	/// </summary>
	public MenuState MenuState {
		get => _menuState;
		private set {
			_menuState = value;

			// Clear all physics number objects based on the menu state
			if (_menuState != MenuState.WIN) {
				PhysicsNumber[ ] activePhysicsNumbers = FindObjectsOfType<PhysicsNumber>( );
				for (int i = activePhysicsNumbers.Length - 1; i >= 0; i--) {
					Destroy(activePhysicsNumbers[i].gameObject);
				}
			}

			switch (_menuState) {
				case MenuState.MENU:
					homeButton.IsHidden = true;
					resetButton.IsHidden = true;
					playButton.IsHidden = false;
					playButtonText.text = "play";

					creditsText.gameObject.SetActive(true);
					winText.gameObject.SetActive(false);
					cameraBoundsContainer.SetActive(false);

					targetNumberText.color = new Color(136 / 255f, 98 / 255f, 235 / 255f);
					targetNumberText.fontStyle = FontStyles.Underline;
					targetNumberText.text = "smath";


					break;
				case MenuState.GAME:
					targetNumberText.color = new Color(156 / 255f, 150 / 255f, 218 / 255f);
					targetNumberText.fontStyle = FontStyles.Normal;

					homeButton.IsHidden = false;
					resetButton.IsHidden = false;
					playButton.IsHidden = true;
					playButtonText.text = "play again";

					creditsText.gameObject.SetActive(false);
					cameraBoundsContainer.SetActive(true);
					winText.gameObject.SetActive(false);

					// Generate the game's target number and start the game
					gameManager.GenerateTargetNumber( );

					break;
				case MenuState.WIN:
					winText.gameObject.SetActive(true);

					playButton.IsHidden = false;

					break;
			}
		}
	}

	private void Start ( ) {
		SetMenuState((int) MenuState.MENU);
	}

	private void Update ( ) {
		// Spawn a physics number every 2 seconds while in the menu
		// Adds a little bit of movement to the scene
		/*if (MenuState == MenuState.MENU) {
			if (spawnTimer >= 0.5f) {
				float randomX = Random.Range(-cameraController.CameraWidth / 2f, cameraController.CameraWidth / 2f);
				PhysicsNumber physicsNumber = Instantiate(physicsNumberPrefab, new Vector3(randomX, cameraController.CameraHeight / 2f + 2f), Quaternion.identity).GetComponent<PhysicsNumber>();
				physicsNumber.Value = Random.Range(gameManager.TargetNumberMin, gameManager.TargetNumberMax + 1);
				physicsNumber.Operation = (Operation) Random.Range(0, 5);
				physicsNumber.transform.eulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));

				spawnTimer -= 0.5f;
			}
		}
		spawnTimer += Time.deltaTime;*/
	}

	/// <summary>
	/// Set the state of the menu with a transition
	/// </summary>
	/// <param name="menuState">The menu state to set</param>
	public void SetMenuState (int menuState) {
		StopAllCoroutines( );
		StartCoroutine(ISetMenuState((MenuState) menuState));
	}

	private IEnumerator ISetMenuState (MenuState menuState) {
		// Fade the transition panel to white over a period of 1 second
		float alpha = transitionPanel.color.a;
		while (alpha < 1f) {
			transitionPanel.color = new Color(1f, 1f, 1f, alpha);

			alpha = Mathf.Min(1f, alpha + (Time.deltaTime * 2f));

			yield return null;
		}

		// Set the menu state
		MenuState = menuState;

		// Fade the transition panel to transparent over a period of 1 second
		while (alpha > 0f) {
			transitionPanel.color = new Color(1f, 1f, 1f, alpha);

			alpha = Mathf.Max(0f, alpha - (Time.deltaTime * 2f));

			yield return null;
		}

		// Set the final state of the transition panel
		transitionPanel.color = new Color(1f, 1f, 1f, 0f);
	}
}
