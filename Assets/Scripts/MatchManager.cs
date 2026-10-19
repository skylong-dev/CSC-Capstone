using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance;

    [Header("Match Settings")]
    public int startingStocks = 3;
    public float matchTime = 180f;

    [Header("Player References")]
    public Damage player1;
    public Damage player2;

    [Header("UI Text")]
    public TMP_Text timerText;
    public TMP_Text player1StocksText;
    public TMP_Text player2StocksText;

    private int player1Stocks;
    private int player2Stocks;
    private bool matchEnded = false;

    public static string WinnerMessage = "";

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        player1Stocks = startingStocks;
        player2Stocks = startingStocks;

        UpdateUI();
    }

    void Update()
    {
        if (matchEnded)
        {
            return;
        }

        matchTime -= Time.deltaTime;

        if (matchTime < 0)
        {
            matchTime = 0;
        }

        UpdateUI();

        if (matchTime <= 0)
        {
            EndMatch();
        }
    }

    void UpdateUI()
    {
        int minutes = Mathf.FloorToInt(matchTime / 60);
        int seconds = Mathf.FloorToInt(matchTime % 60);

        timerText.text = string.Format(
            "{0:00}:{1:00}", minutes, seconds
        );

        player1StocksText.text = "P1 Stocks: " + player1Stocks;
        player2StocksText.text = "P2 Stocks: " + player2Stocks;
    }

    public void PlayerLostStock(int playerNumber)
    {
        if (matchEnded)
        {
            return;
        }

        if (playerNumber == 1)
        {
            player1Stocks--;
            player1Stocks = Mathf.Max(0, player1Stocks);

            if (player1Stocks <= 0)
            {
                player1.gameObject.SetActive(false);
            }
            else
            {
                player1.RespawnPlayer();
            }
        }
        else if (playerNumber == 2)
        {
            player2Stocks--;
            player2Stocks = Mathf.Max(0, player2Stocks);

            if (player2Stocks <= 0)
            {
                player2.gameObject.SetActive(false);
            }
            else
            {
                player2.RespawnPlayer();
            }
        }

        UpdateUI();

        if (player1Stocks <= 0 || player2Stocks <= 0)
        {
            EndMatch();
        }
    }

    void EndMatch()
    {
        if (matchEnded)
        {
            return;
        }

        matchEnded = true;

        if (player1Stocks > player2Stocks)
        {
            WinnerMessage = "Player 1 Wins!";
        }
        else if (player2Stocks > player1Stocks)
        {
            WinnerMessage = "Player 2 Wins!";
        }
        else
        {
            WinnerMessage = "It's a Draw!";
        }

        SceneManager.LoadScene("Winner");
    }
}