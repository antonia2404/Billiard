using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 10f;
    [SerializeField] Vector3 offset;
    [SerializeField] float downAngle;
    [SerializeField] float power;
    [SerializeField] GameObject cueStick;
    private float horizntalInput;
    private bool isTakingShot = false;
    [SerializeField] float maxDrawDistance;
    private float savedMousePosition;

    Transform cueBall;
    GameManager gameManager;
    [SerializeField] TextMeshProUGUI powerText;

    [SerializeField] LineRenderer trajectoryLine;
    [SerializeField] LayerMask collisionMask;

    [SerializeField] Image powerBarFill;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        foreach (GameObject ball in GameObject.FindGameObjectsWithTag("Ball"))
        {
            if (ball.GetComponent<Ball>().IsCueBall())
            {
                cueBall = ball.transform;
                break;
            }
        }

        ResetCamera();
    }

    // Update is called once per frame
    void Update()
    {

        if (cueBall != null && !isTakingShot)
        {
            horizntalInput = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            transform.RotateAround(cueBall.position, Vector3.up, horizntalInput);
            DrawTrajectory();  // NEW
        }

        Shoot();
    }

    void DrawTrajectory()
    {
        if (trajectoryLine == null || cueBall == null) return;

        Vector3 origin = cueBall.position;
        Vector3 direction = transform.forward;
        direction = new Vector3(direction.x, 0, direction.z).normalized;

        // Max number of reflections
        int reflections = 2;
        Vector3[] points = new Vector3[reflections + 1];
        points[0] = origin;

        trajectoryLine.positionCount = reflections + 1;

        for (int i = 0; i < reflections; i++)
        {
            if (Physics.Raycast(origin, direction, out RaycastHit hit, 10f, collisionMask))
            {
                points[i + 1] = hit.point;

                // Check if we hit a wall or a ball
                if (hit.collider.CompareTag("Ball"))
                {
                    // Stop here – don't reflect off balls
                    break;
                }

                direction = Vector3.Reflect(direction, hit.normal);
                origin = hit.point + direction * 0.01f;

                trajectoryLine.material.color = Color.red;
            }
            else
            {
                // If no hit, just draw straight
                points[i + 1] = origin + direction * 5f;
                trajectoryLine.material.color = Color.white;
                break;
            }
        }

        // Apply points to LineRenderer
        for (int i = 0; i < points.Length; i++)
        {
            trajectoryLine.SetPosition(i, points[i]);
        }
    }



    public void ResetCamera()
    {
        cueStick.SetActive(true);
        transform.position = cueBall.position + offset;
        transform.LookAt(cueBall.position);
        transform.localEulerAngles = new Vector3(downAngle, transform.localEulerAngles.y, 0);
    }

    void Shoot()
    {
        if (gameObject.GetComponent<Camera>().enabled)
        {
            if (Input.GetButtonDown("Fire1") && !isTakingShot)
            {
                isTakingShot = true;
                savedMousePosition = 0f;
            }
            else if (isTakingShot)
            {
                if(savedMousePosition + Input.GetAxis("Mouse Y") <= 0)
                {
                    savedMousePosition += Input.GetAxis("Mouse Y");
                    if (savedMousePosition <= maxDrawDistance)
                    {
                        savedMousePosition = maxDrawDistance;
                    }
                    float powerValueNumber = ((savedMousePosition - 0) / (maxDrawDistance - 0)) * (100 - 0) + 0;
                    int powerValueInt = Mathf.RoundToInt(powerValueNumber);
                    powerText.text = "Power:" + powerValueInt + "%";
                    powerBarFill.color = Color.Lerp(Color.green, Color.red, powerValueInt / 100f);
                    powerBarFill.fillAmount = powerValueInt / 100f;

                }
                if (Input.GetButtonUp("Fire1"))
                {
                    Vector3 hitDirection = transform.forward;
                    hitDirection = new Vector3(hitDirection.x, 0, hitDirection.z).normalized;

                    cueBall.gameObject.GetComponent<Rigidbody>().AddForce(hitDirection * power * Mathf.Abs(savedMousePosition), ForceMode.Impulse);
                    cueStick.SetActive(false);
                    gameManager.SwitchCameras();
                    isTakingShot = false;

                    // Reset power bar
                    powerText.text = "Power: 0%";
                    powerBarFill.fillAmount = 0f;
                    powerBarFill.color = Color.green;
                }
            }
        }
    }
    public void ShowAIPower(int aiPower)
    {
        powerText.text = $"Power: {aiPower}%";
        powerBarFill.fillAmount = aiPower / 100f;
        powerBarFill.color = Color.Lerp(Color.green, Color.red, aiPower / 100f);
    }

    public void ResetPowerUI()
    {
        powerText.text = "Power: 0%";
        powerBarFill.fillAmount = 0f;
        powerBarFill.color = Color.green;
    }

}
