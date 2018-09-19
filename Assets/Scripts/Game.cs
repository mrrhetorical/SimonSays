using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Advertisements;

[Serializable]
public class Square {

	public static Square BLUE = new Square(1);
	public static Square RED = new Square(2);
	public static Square GREEN = new Square(3);
	public static Square YELLOW = new Square(4);
	public static Square NULL = new Square (0);

	private readonly int _quadrant;

	private Square(int quadrant) {
		_quadrant = quadrant;
	}

	public int GetQuadrant() {
		return _quadrant;
	}

	public override string ToString() {
		switch(_quadrant) {
			case 0:
				return "NULL";
			case 1:
				return "BLUE";
			case 2:
				return "RED";
			case 3:
				return "GREEN";
			case 4:
				return "YELLOW";
			default:
				return "NULL";
		}
	}
}

public enum GameState {
	TEACHING, LEARNING, ENDED
}

public enum Difficulty {
	Easy, Hard
}

public class Game : MonoBehaviour {

	[SerializeField] private int _score = 0;
	[SerializeField] private int _round = 1;
	[SerializeField] private GameState _gameState;
	[SerializeField] private Difficulty _difficulty;

	public Text Score;

	public Image RedSquare;
	public Image BlueSquare;
	public Image YellowSquare;
	public Image GreenSquare;

	public Text Timer;
	public Text LoseScore;
	public Image LoseDimmer;
	public GameObject LoseOverlay;

	/* vvv RUNTIME VARIABLES  vvv */
	[SerializeField] private Square[] _currentPattern;
	[SerializeField] private List<Square> _currentInput = new List<Square>();

	private void Start() {
		if (RedSquare == null
		|| BlueSquare == null
		|| YellowSquare == null
		|| GreenSquare == null) {
			Debug.LogError("Squares not defined!");
		}

		StartGame();
	}

	private void Update() {

	}

	private void StartGame() {
		_gameState = GameState.TEACHING;
		_currentPattern = GeneratePattern(_round);
		StartCoroutine(TeachPattern(_currentPattern, GetSpeedForLevel(_round)));
	}

	private Square[] GeneratePattern(int count) {
		Square[] pattern = new Square[count];

		for (int i = 0; i < count; i++) {
			Square chosen = Square.NULL;
			int rand = UnityEngine.Random.Range(0, 4);
			switch(rand) {
				case 0:
					chosen = Square.BLUE;
					break;
				case 1:
					chosen = Square.RED;
					break;
				case 2:
					chosen = Square.GREEN;
					break;
				case 3:
					chosen = Square.YELLOW;
					break;
				default:
					chosen = Square.NULL;
					break;
			}

			pattern[i] = chosen;
		}

		return pattern;
	}

	private void AddToPattern() {
		Square[] newPattern = new Square[_currentPattern.Length + 1];
		for (int i = 0; i < _currentPattern.Length; i++) {
			newPattern[i] = _currentPattern[i];
		}

		newPattern[_currentPattern.Length] = GeneratePattern(1)[0];

		_currentPattern = newPattern;
	}

	private float GetSpeedForLevel(int level) {
		return 0.5f;
	}

	private IEnumerator TeachPattern(Square[] pattern, float speed) {
		Timer.text = GetFancyTime(_round == 1 ? 2 : _round * _round);
		for (int i = 0; i < pattern.Length; i++) {
			Square current = pattern[i];
			Image image;
			if (current == Square.BLUE) {
				image = BlueSquare;
			} else if (current == Square.RED) {
				image = RedSquare;
			} else if (current == Square.YELLOW) {
				image = YellowSquare;
			} else if (current == Square.GREEN) {
				image = GreenSquare;
			} else {
				image = null;
			}
			yield return new WaitForSeconds(speed);
			
			if (image != null) {
				image.enabled = false;
			}

			yield return new WaitForSeconds(speed);

			if (image != null) {
				image.enabled = true;
			}
		}

		_gameState = GameState.LEARNING;
		StartCoroutine(StartTimer());
	}

	private IEnumerator StartTimer() {
		int timer = _round == 1 ? 2 : _round * _round;
		while(timer >= 0) {
			Timer.text = GetFancyTime(timer);
			yield return new WaitForSeconds(1f);
			timer--;
		}

		EndGame();
	}

	private string GetFancyTime(int time) {

		string seconds = (time % 60) + "";

		if (seconds.Length == 1) {
			seconds = "0" + seconds;
		}

		String minutes = ( time / 60 )+ "";

		if (minutes.Length == 1) {
			minutes = "0" + minutes;
		}

		return (minutes + ":" + seconds);
	}

	private void EndGame() {
		LoseScore.text = _score + "";
		LoseDimmer.enabled = true;
		LoseOverlay.SetActive(true);
		ShowAdvertisement();

	}

	public void RecieveInput(string squareName) {
		if (_gameState != GameState.LEARNING) return;

		Square square = Square.NULL;

		switch(squareName.ToUpper()) {
			case "BLUE":
				square = Square.BLUE;
				break;
			case "RED":
				square = Square.RED;
				break;
			case "YELLOW":
				square = Square.YELLOW;
				break;
			case "GREEN":
				square = Square.GREEN;
				break;
			default:
				square = Square.NULL;
				break;
		}

		_currentInput.Add(square);
		CheckInput();
	}

	private void CheckInput() {
		if (_currentInput.Count < _currentPattern.Length) {
			for (int i = 0; i < _currentInput.Count; i++) {
				bool correct = _currentInput[i] == _currentPattern[i];

				if(!correct) {
					EndGame();
					return;
				}
			}
			return;
		}

		for (int i = 0; i < _currentPattern.Length; i++) {
			bool correct = _currentInput[i] == _currentPattern[i];

			if(!correct) {
				EndGame();
				return;
			}
		}

		IncrementScore();
	}

	private IEnumerator WinScreen() {
		for (int i = 0; i < 2; i++) {
			yield return new WaitForSeconds(0.25f);
			BlueSquare.enabled = false;
			RedSquare.enabled = false;
			YellowSquare.enabled = false;
			GreenSquare.enabled = false;
			yield return new WaitForSeconds(0.25f); 
			BlueSquare.enabled = true;
			RedSquare.enabled = true;
			YellowSquare.enabled = true;
			GreenSquare.enabled = true;
		}

		_gameState = GameState.TEACHING;
		if (_difficulty == Difficulty.Easy)
			AddToPattern();
		else
			_currentPattern = GeneratePattern(_round);

		_currentInput = new List<Square>();
		StartCoroutine(TeachPattern(_currentPattern, GetSpeedForLevel(_round)));
	}

	private void IncrementScore() {
		StopAllCoroutines();
		_score++;
		_round++;
		Score.text = _score + "";
		StartCoroutine(WinScreen());
	}

	public void RestartGame() {
		SceneManager.LoadScene("Game_" + _difficulty, LoadSceneMode.Single);
	}

	public void LoadMenu() {
		SceneManager.LoadScene("Menu", LoadSceneMode.Single);
	}

	public void ShowAdvertisement() {
		if (Advertisement.IsReady()) {
			Advertisement.Show();
		}
	}
}