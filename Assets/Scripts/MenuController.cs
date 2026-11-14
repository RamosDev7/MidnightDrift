using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private string GameSceneName;
    [SerializeField] private GameObject initialMenuPanel;
    [SerializeField] private GameObject optionsPanel;

    public void Jogar()
    {
        SceneManager.LoadScene(GameSceneName);
    }

    public void OpenSettings()
    {
        initialMenuPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        optionsPanel.SetActive(false);
        initialMenuPanel.SetActive(true);
    }

    public void ExitGame()
    {
        Debug.Log("Game Over!");
        Application.Quit();
    }
}