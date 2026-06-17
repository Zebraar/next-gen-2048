using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class WinHandler : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text bestScoreText;

    [Header("Other")]
    [SerializeField] private GameManager gameManager;

    private RectTransform rect;

    void Awake()
    {
        rect = winPanel.GetComponent<RectTransform>();
    }
    void Start()
    {
        winPanel.SetActive(false);
    }
    public void OnWin()
    {
        if(gameManager.GetScore() > SaveManager.GetPlayerData().highScore)
        {
            SaveManager.Save(gameManager.GetScore());
            scoreText.text = gameManager.ScoreText.text;
            bestScoreText.text = "High Score: " + gameManager.GetScore();
        } else if(gameManager.GetScore() < SaveManager.GetPlayerData().highScore)
        {
            SaveManager.Save(SaveManager.GetPlayerData().highScore);
            scoreText.text = gameManager.ScoreText.text;
            bestScoreText.text = "High Score: " + SaveManager.GetPlayerData().highScore;
        }
        rect.localScale = Vector3.zero;
        winPanel.SetActive(true);
        rect.DOScale(new Vector3(1, 1, 1), 1f).SetEase(Ease.OutBack);
    }
    public void Restart()
    {
        StartCoroutine(HideWinPanel());
        gameManager.RestartGame();
    }
    IEnumerator HideWinPanel()
    {
        rect.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
        yield return new WaitForSeconds(0.35f);
        winPanel.SetActive(false);
    }
}
