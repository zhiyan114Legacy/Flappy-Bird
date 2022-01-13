using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class UIServiceHandler : MonoBehaviour
{
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        // Default UI configuration setup
        _DeadMenu.SetActive(false);
        _StartWindow.SetActive(true);
        _PauseMenu.SetActive(false);
        HighScore = SaveManager.Data.HighScore;
    }
    // Player Score Handling
    [SerializeField]
    private TextMeshProUGUI _ScoreUI;
    [SerializeField]
    private TextMeshProUGUI _HighScoreUI;
    private int HighScore 
    {
        get => SaveManager.Data.HighScore;
        set => _HighScoreUI.text = "High Score: " + value.ToString(); 
    }
    public static UIServiceHandler instance;
    public int ScoreUI
    {
        get => int.TryParse(_ScoreUI.text, out int a) ? a : 0;
        set 
        {
            _ScoreUI.text = value.ToString();
            if (value > HighScore) HighScore = value;
        }
    }
    // Dead Menu Hanlding
    [SerializeField]
    private GameObject _DeadMenu;
    [SerializeField]
    private TextMeshProUGUI _DeadScoreUI;
    public void ShowDeadMenu()
    {
        _ScoreUI.gameObject.SetActive(false);
        _HighScoreUI.gameObject.SetActive(false);
        _DeadScoreUI.text = string.Format("You scored {0} points with a high score of {1} points.",MapRenderer.getScore,SaveManager.Data.HighScore);
        _DeadMenu.SetActive(true);
    }
    public void Retrybtn_Handler()
    {
        SceneManager.LoadScene("MainGame", LoadSceneMode.Single);
    }
    public void BackMenubtn_Handler()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
    // Start Window Handler
    [SerializeField]
    private GameObject _StartWindow;
    public static void closeStartWindow()
    {
        instance._StartWindow.SetActive(false);
    }
    // Pause Menu Handler
    [SerializeField]
    private GameObject _PauseMenu;
    public static bool pauseMenuVisible
    {
        get => instance._PauseMenu.activeSelf;
    }
    public void PauseKeyPressed(InputAction.CallbackContext cb)
    {
        if (cb.phase == InputActionPhase.Started && PlayerHandler.GameState != GameState.Dead)
        {
            _PauseMenu.SetActive(!_PauseMenu.activeSelf);
            Time.timeScale = _PauseMenu.activeSelf ? 0 : 1;
        }
    }
    public void Resumebtn_Handler()
    {
        _PauseMenu.SetActive(false);
        Time.timeScale = 1;
    }
    public void Exitbtn_Handler()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
}
