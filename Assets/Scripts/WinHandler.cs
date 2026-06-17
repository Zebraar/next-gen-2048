using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class WinHandler : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private Text winScoreText;
    [SerializeField] private Text winBestScoreText;
    [SerializeField] private Text loseScoreText;
    [SerializeField] private Text loseBestScoreText;

    [Header("Other")]
    [SerializeField] private GameManager gameManager;

    private RectTransform winRect;
    private RectTransform loseRect;

    void Awake()
    {
        winRect = winPanel.GetComponent<RectTransform>();
        loseRect = losePanel.GetComponent<RectTransform>();
    }
    void Start()
    {
        winPanel.SetActive(false);
        losePanel.SetActive(false);
    }
    public void OnWin()
    {
        if(gameManager.GetScore() > SaveManager.GetPlayerData().highScore)
        {
            SaveManager.Save(gameManager.GetScore());
            winScoreText.text = gameManager.ScoreText.text;
            winBestScoreText.text = "High Score: " + gameManager.GetScore();
        } else if(gameManager.GetScore() < SaveManager.GetPlayerData().highScore)
        {
            SaveManager.Save(SaveManager.GetPlayerData().highScore);
            winScoreText.text = gameManager.ScoreText.text;
            winBestScoreText.text = "High Score: " + SaveManager.GetPlayerData().highScore;
        }
        winRect.localScale = Vector3.zero;
        winPanel.SetActive(true);
        winRect.DOScale(new Vector3(1, 1, 1), 1f).SetEase(Ease.OutBack);
    }
    public void OnLose()
    {
        if(gameManager.GetScore() > SaveManager.GetPlayerData().highScore)
        {
            SaveManager.Save(gameManager.GetScore());
            loseScoreText.text = gameManager.ScoreText.text;
            loseBestScoreText.text = "High Score: " + gameManager.GetScore();
        } else if(gameManager.GetScore() < SaveManager.GetPlayerData().highScore)
        {
            SaveManager.Save(SaveManager.GetPlayerData().highScore);
            loseScoreText.text = gameManager.ScoreText.text;
            loseBestScoreText.text = "High Score: " + SaveManager.GetPlayerData().highScore;
        }
        loseRect.localScale = Vector3.zero;
        losePanel.SetActive(true);
        loseRect.DOScale(new Vector3(1, 1, 1), 1f).SetEase(Ease.OutBack);
    }
    public void Restart()
    {
        StartCoroutine(HideWinPanel());
        StartCoroutine(HideLosePanel());
        gameManager.RestartGame();
    }
    IEnumerator HideWinPanel()
    {
        winRect.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
        yield return new WaitForSeconds(0.35f);
        winPanel.SetActive(false);
    }
    IEnumerator HideLosePanel()
    {
        loseRect.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
        yield return new WaitForSeconds(0.35f);
        losePanel.SetActive(false);
    }
}
