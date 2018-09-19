using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour {
    
    public void LoadEasyMode() {
        SceneManager.LoadScene("Game_Easy", LoadSceneMode.Single);
    }

    public void LoadHardMode() {
        SceneManager.LoadScene("Game_Hard", LoadSceneMode.Single);
    }
}