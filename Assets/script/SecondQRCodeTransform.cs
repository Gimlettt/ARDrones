using UnityEngine;
using Vuforia;

public class SecondQRCodeTransform : MonoBehaviour
{
    //this class is responsible for managing the small QR code attached to the propeller. There are 6 game objects in the scene(Propeller 1 to 6), each of them
    // has this script as their component.Using this script, you can track the small QR's position.
    //A small sphere is used to indicate whether the small QR code is in the correct position or not. If the small QR code is in the correct position, the sphere will turn green.
    //Also the tick and cross is used to indicate if user is using the correct number index of propeller.


    private ImageTargetBehaviour imageTargetBehaviour;//using vuforia image target behaviour to track the small QR code's position

    public PropellerManager propellerManager; // Reference to the propeller manager script, which is handling the propellers' hologram

    public GameObject ballPrefab; // Prefab for the ball
    private GameObject ballInstance;

    public GameObject prefabRight; // Prefab for a tick
    public GameObject prefabWrong; // Prefab for a cross
    private GameObject rightInstance;
    private GameObject wrongInstance;

    public int qrCodeID;//this is the number index of the propeller


    void Start()
    {
        // Find the ImageTargetBehaviour component on the same GameObject
        imageTargetBehaviour = GetComponent<ImageTargetBehaviour>();



        // Create the ball and set it to inactive initially
        ballInstance = Instantiate(ballPrefab);
        ballInstance.SetActive(false);

        rightInstance = Instantiate(prefabRight);
        rightInstance.SetActive(false);

        wrongInstance = Instantiate(prefabWrong);
        wrongInstance.SetActive(false);

    }

    void Update()
    {
        if (imageTargetBehaviour != null)
        {
            // Get the status of the Image Target
            TargetStatus targetStatus = imageTargetBehaviour.TargetStatus;

            // Check if the Image Target is being tracked
            if (targetStatus.Status == Status.TRACKED)
            {
                // Get the position of the Image Target
                Vector3 secondQRPosition = imageTargetBehaviour.transform.position;
                Quaternion secondQRRotation = imageTargetBehaviour.transform.rotation;

                // Update ball, tick and corss's position
                ballInstance.transform.position = secondQRPosition - new Vector3(0, 0.03f, 0); // Adjust offset if needed( this is becuase tracking quality is bad, so we may obtain a wrong coordinate, so you could update it accordingly)
                rightInstance.transform.position = secondQRPosition - new Vector3(0, 0.015f, 0);
                rightInstance.transform.rotation = secondQRRotation * Quaternion.Euler(0, 0, 0);
                wrongInstance.transform.position = secondQRPosition - new Vector3(0, 0.03f, 0);
                wrongInstance.transform.rotation = secondQRRotation * Quaternion.Euler(0, 0, 0);

                UpdateQRCodeDisplay();//decide whether to show the ball or tick or cross


                if (propellerManager != null&& propellerManager.isMounting)//when user is mounting the propeller, show the ball
                {
                    propellerManager.CheckSecondQRCodePosition(ballInstance.transform.position, this);
                }
            }
            else
            {
                // Hide everything if the QR code is not being tracked
                HideAll();
            }
        }

    }
    public void UpdateQRCodeDisplay()
    {
        if (propellerManager.isMounting)
        {
            ShowBall();
        }
        else
        {
            ShowPrefab();
        }
    }

    private void ShowBall()
    {
        ballInstance.SetActive(true);
        rightInstance.SetActive(false);
        wrongInstance.SetActive(false);
    }

    private void ShowPrefab()
    {
        if (qrCodeID == propellerManager.currentCopterIndex+1)
        {
            rightInstance.SetActive(true);
            wrongInstance.SetActive(false);
        }
        else
        {
            rightInstance.SetActive(false);
            wrongInstance.SetActive(true);
        }
        ballInstance.SetActive(false);
    }

    private void HideAll()
    {
        ballInstance.SetActive(false);
        rightInstance.SetActive(false);
        wrongInstance.SetActive(false);
    }
    public void ChangeBallColor(Color color)//this function is used in propeller manager to change the ball color
    {
        if (ballInstance != null)
        {
            ballInstance.GetComponent<Renderer>().material.color = color;
        }
    }

}