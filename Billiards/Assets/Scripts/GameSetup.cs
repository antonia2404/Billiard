using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    int redBallsRemaining = 7;
    int blueBallsRemaining = 7;
    float ballRadius;
    float ballDiameter;

    [SerializeField] GameObject ballPrefab;
    [SerializeField] Transform cueBallPosition;
    [SerializeField] Transform headBallPosition;

    //called before any Start method is called
    private void Awake()
    {
        ballRadius = ballPrefab.GetComponent<SphereCollider>().radius * 100f;
        ballDiameter = ballRadius * 2f;
        PlaceAllBalls();
    }
 
    void PlaceAllBalls()
    {
        PlaceCueBall();
        PlaceRandomBalls();
    }

    void PlaceCueBall()
    {
        GameObject ball = Instantiate(ballPrefab, cueBallPosition.position, Quaternion.identity);
        ball.GetComponent<Ball>().MakeCueBall();
    }

    void PlaceEightBall(Vector3 position)
    {
        GameObject ball = Instantiate(ballPrefab, position, Quaternion.identity);
        ball.GetComponent<Ball>().MakeEightBall();
    }

    void PlaceRandomBalls()
    {
        int NumInThisRow = 1;
        int rand;
        Vector3 firstInRoomPosition = headBallPosition.position;
        Vector3 currentPosition = firstInRoomPosition;

         void PlaceRedBall(Vector3 position)
        {
            GameObject ball = Instantiate(ballPrefab, position, Quaternion.identity);
            ball.GetComponent<Ball>().BallSetup(true);
            redBallsRemaining--;
        }

        void PlaceBlueBall(Vector3 position)
        {
            GameObject ball = Instantiate(ballPrefab, position, Quaternion.identity);
            ball.GetComponent<Ball>().BallSetup(false);
            blueBallsRemaining--;
        }

        //outer loop is the 5 rows
        for (int i = 0; i < 5; i++)
        {
            //inner loop are the balls in each row
            for (int j = 0; j < NumInThisRow; j++)
            {
                //check to see if its the middle spot where the 8 ball goes
                if (i == 2 && j == 1)
                {
                    PlaceEightBall(currentPosition);
                }
                //if there are red and blue balls remaining, randomly choose one and place it
                else if (redBallsRemaining > 0 && blueBallsRemaining > 0)
                {
                    rand = Random.Range(0, 2);
                    if (rand == 0)
                    {
                        PlaceRedBall(currentPosition);
                    }
                    else
                    {
                        PlaceBlueBall(currentPosition);
                    }
                }
                //if only red balls are left, place one
                else if (redBallsRemaining > 0)
                {
                    PlaceRedBall(currentPosition);
                }
                //otherwise, place a blue ball
                else
                {
                    PlaceBlueBall(currentPosition);
                }
                //move the current position for the next ball in this row to the right
                currentPosition += new Vector3(1, 0, 0).normalized * ballDiameter;
            }
            //once all the balls in the row have been placed, move to the next row
            firstInRoomPosition += Vector3.back * (Mathf.Sqrt(3) * ballRadius) + Vector3.left * ballRadius;
            currentPosition = firstInRoomPosition;
            NumInThisRow++;
        }
    } 
}
