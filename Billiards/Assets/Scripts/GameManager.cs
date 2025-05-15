using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;


public class GameManager : MonoBehaviour
{
    enum CurrentPlayer
    {
        Player1,
        Player2
    }

    CurrentPlayer currentPlayer;
    bool isWinningShotForPlayer1 = false;
    bool isWinningShotForPlayer2 = false;
    int player1BallsRemaining = 7;
    int player2BallsRemaining = 7;
    bool isWaitingForBallMovementToStop = false;
    bool willSwapPlayers = false;
    bool isGameOver = false;
    bool ballPocketed = false;
    [SerializeField] float shotTimer = 3f;
    private float currentTimer;
    [SerializeField] float movementThreshold;

    [SerializeField] TextMeshProUGUI player1BallsText;
    [SerializeField] TextMeshProUGUI player2BallsText;
    [SerializeField] TextMeshProUGUI currentTurnText;
    [SerializeField] TextMeshProUGUI messageText;

    [SerializeField] GameObject restartButton;

    [SerializeField] Transform headPosition;

    [SerializeField] Camera cueStickCamera;
    [SerializeField] Camera overheadCamera;
    Camera currentCamera;

    [SerializeField] bool isSinglePlayer = true;
    [SerializeField] float aiShotDelay = 2f;
    Transform cueBall;

    [SerializeField] float turnTimeLimit = 30f;
    float turnTimer;

    [SerializeField] TextMeshProUGUI timerText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentPlayer = CurrentPlayer.Player1;
        currentCamera = cueStickCamera;
        currentTimer = shotTimer;
        // Cache cue ball
        foreach (GameObject ball in GameObject.FindGameObjectsWithTag("Ball"))
        {
            if (ball.GetComponent<Ball>().IsCueBall())
            {
                cueBall = ball.transform;
                break;
            }
        }
        turnTimer = turnTimeLimit;

    }

    // Update is called once per frame
    void Update()
    {
        if (isWaitingForBallMovementToStop && !isGameOver)
        {
            currentTimer -= Time.deltaTime;
            if (currentTimer > 0)
            {
                return;
            }

            bool allStopped = true;
            foreach (GameObject ball in GameObject.FindGameObjectsWithTag("Ball"))
            {
                if (ball.GetComponent<Rigidbody>().angularVelocity.magnitude >= movementThreshold)
                {
                    allStopped = false;
                    break;
                }
            }

            if (allStopped)
            {
                isWaitingForBallMovementToStop = false;

                if (willSwapPlayers || !ballPocketed)
                {
                    NextPlayerTurn();
                }
                else
                {
                    // Stay with current player
                    SwitchCameras();

                    // Check if AI should go again
                    if (isSinglePlayer && currentPlayer == CurrentPlayer.Player2)
                    {
                        StartCoroutine(TakeAITurn());
                    }
                }

                currentTimer = shotTimer;
                ballPocketed = false;
            }
        }

        // Turn Timer logic (for Player 1 only)
        if (!isWaitingForBallMovementToStop && !isGameOver && currentCamera == cueStickCamera)
        {
            if (!isSinglePlayer || currentPlayer == CurrentPlayer.Player1)
            {
                turnTimer -= Time.deltaTime;
                timerText.text = "Time: " + Mathf.CeilToInt(turnTimer) + "s";

                if (turnTimer <= 0)
                {
                    NextPlayerTurn();
                }
            }
        }
    }


    public void SwitchCameras()
    {
        if (currentCamera == cueStickCamera)
        {
            cueStickCamera.enabled = false;
            overheadCamera.enabled = true;
            currentCamera = overheadCamera;
            isWaitingForBallMovementToStop = true;
        }
        else
        {
            cueStickCamera.enabled = true;
            overheadCamera.enabled = false;
            currentCamera = cueStickCamera;
            currentCamera.gameObject.GetComponent<CameraController>().ResetCamera();
        }
    }

    //public void RestartTheGame()
    //{
    //    SceneManager.LoadScene(0);
    //}

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    } 
    bool Scratch()
    {
        if (currentPlayer == CurrentPlayer.Player1)
        {
            if (isWinningShotForPlayer1)
            {
                ScratchOnWinningShot("Player 1");
                return true;
            }
        }
        else
        {
            if(isWinningShotForPlayer2)
            {
                ScratchOnWinningShot("Player 2");
                return true;
            }
        }
        willSwapPlayers = true;
        return false;
    }

    void EarlyEightBall()
    {
        if (currentPlayer == CurrentPlayer.Player1)
        {
            Lose("Player 1 Hit in the Eight Ball Too Early and Has Lost!");
        }
        else
        {
            Lose("Player 2 Hit in the Eight Ball Too Early and Has Lost!");
        }
    }

    void ScratchOnWinningShot(string player)
    {
        Lose(player + "Scratched on Their Final Shot and Has Lost!");
    }

    bool CheckBall(Ball ball)
    {
        if (ball.IsCueBall())
        {
            if (Scratch())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (ball.IsEightBall())
        {
            if (currentPlayer == CurrentPlayer.Player1)
            {
                if (isWinningShotForPlayer1)
                {
                    Win("Player 1");
                    return true;
                }
            }
            else
            {
                if (isWinningShotForPlayer2)
                {
                    Win("Player 2");
                    return true;
                }
            }
            EarlyEightBall();
        }
        else
        {
            //All other logic when not eight ball or cue ball
            if (ball.IsBallRed())
            {
                player1BallsRemaining--;
                player1BallsText.text = "Player 1 Balls Remaining: " + player1BallsRemaining;
                if (player1BallsRemaining <= 0)
                {
                    isWinningShotForPlayer1 = true;
                }
                if (currentPlayer != CurrentPlayer.Player1)
                {
                    willSwapPlayers = true;
                }
            }
            else
            {
                player2BallsRemaining--;
                player2BallsText.text = "Player 2 Balls Remaining: " + player2BallsRemaining;
                if (player2BallsRemaining <= 0)
                {
                    isWinningShotForPlayer2 = true;
                }
                if (currentPlayer != CurrentPlayer.Player2)
                {
                    willSwapPlayers = true;
                }
            }
        }
        return true;
    }

    void Lose(string message)
    {
        isGameOver = true;
        messageText.gameObject.SetActive(true);
        messageText.text = message;
        restartButton.SetActive(true);
    }

    void Win(string player)
    {
        isGameOver = true;
        messageText.gameObject.SetActive(true);
        messageText.text = player + "Has Won!";
        restartButton.SetActive(true);
    }

    void NextPlayerTurn()
    {
        if (currentPlayer == CurrentPlayer.Player1)
        {
            currentPlayer = CurrentPlayer.Player2;
            currentTurnText.text = "Current Turn: Player 2";
        }
        else
        {
            currentPlayer = CurrentPlayer.Player1;
            currentTurnText.text = "Current Turn: Player 1";
            // Reset Power UI for Player 1
            CameraController cam = cueStickCamera.GetComponent<CameraController>();
            if (cam != null)
            {
                cam.ResetPowerUI();
            }
        }
        willSwapPlayers = false;
        SwitchCameras();
        if (isSinglePlayer && currentPlayer == CurrentPlayer.Player2)
        {
            StartCoroutine(TakeAITurn());
        }
        turnTimer = turnTimeLimit;

    }
    IEnumerator TakeAITurn()
    {
        yield return new WaitForSeconds(aiShotDelay);

        GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");
        Transform targetBall = balls.FirstOrDefault(b => !b.GetComponent<Ball>().IsCueBall())?.transform;

        if (targetBall == null) yield break;

        Vector3 direction = (targetBall.position - cueBall.position).normalized;
        direction = new Vector3(direction.x, 0, direction.z).normalized;
        // 🎯 Simulate random power (e.g. 30% to 100%)
        int aiPower = Random.Range(30, 100);

        // 🔁 Optional: update power bar UI to show it visually
        CameraController camController = cueStickCamera.GetComponent<CameraController>();
        if (camController != null)
        {
            camController.ShowAIPower(aiPower);  
        }

        // 💥 Shoot using scaled power
        cueBall.GetComponent<Rigidbody>().AddForce(direction * (8f * aiPower / 100f), ForceMode.Impulse);

        cueStickCamera.enabled = false;
        overheadCamera.enabled = true;
        overheadCamera.targetDisplay = 0;  // Ensure it's rendering to Display 1

        currentCamera = overheadCamera;
        isWaitingForBallMovementToStop = true;

         Debug.Log($"AI took shot with power: {aiPower}%");
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Ball")
        {
            ballPocketed = true;
            if (CheckBall(other.gameObject.GetComponent<Ball>()))
            {
                Destroy(other.gameObject);
            }
            else
            {
                other.gameObject.transform.position = headPosition.position;
                other.gameObject.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
                other.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            }
        }
            
    }
}
