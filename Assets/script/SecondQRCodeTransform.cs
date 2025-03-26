/**
 * @file SecondQRCodeTransform.cs
 * @brief Manages tracking and feedback for the small QR code attached to a propeller.
 *
 * This class tracks a secondary QR code and uses visual indicators (ball, tick, cross)
 * to show if it is in the correct position relative to a propeller hologram.
 */

using UnityEngine;
using Vuforia;

public class SecondQRCodeTransform : MonoBehaviour
{
    private ImageTargetBehaviour imageTargetBehaviour; ///< Used for tracking the small QR code.
    /// <summary>
    /// Reference to the PropellerManager.
    /// </summary>
    public PropellerManager propellerManager;

    /// <summary>
    /// Prefab for the ball indicator.
    /// </summary>
    public GameObject ballPrefab;
    private GameObject ballInstance;

    /// <summary>
    /// Prefab for the tick indicator.
    /// </summary>
    public GameObject prefabRight;
    /// <summary>
    /// Prefab for the cross indicator.
    /// </summary>
    public GameObject prefabWrong;
    private GameObject rightInstance;
    private GameObject wrongInstance;

    /// <summary>
    /// The number index of the propeller associated with this QR code.
    /// </summary>
    public int qrCodeID;

    /**
     * @brief Initializes the small QR code tracking and visual indicators.
     *
     * Instantiates the ball, tick, and cross prefabs.
     */
    void Start()
    {
        imageTargetBehaviour = GetComponent<ImageTargetBehaviour>();
        ballInstance = Instantiate(ballPrefab);
        ballInstance.SetActive(false);
        rightInstance = Instantiate(prefabRight);
        rightInstance.SetActive(false);
        wrongInstance = Instantiate(prefabWrong);
        wrongInstance.SetActive(false);
    }

    /**
     * @brief Updates tracking and indicator positions every frame.
     *
     * If the QR code is tracked, updates the positions of the ball, tick, and cross,
     * and calls UpdateQRCodeDisplay to determine which indicator to show.
     */
    void Update()
    {
        if (imageTargetBehaviour != null)
        {
            TargetStatus targetStatus = imageTargetBehaviour.TargetStatus;
            if (targetStatus.Status == Status.TRACKED)
            {
                Vector3 secondQRPosition = imageTargetBehaviour.transform.position;
                Quaternion secondQRRotation = imageTargetBehaviour.transform.rotation;
                ballInstance.transform.position = secondQRPosition - new Vector3(0, 0.03f, 0);
                rightInstance.transform.position = secondQRPosition - new Vector3(0, 0.015f, 0);
                rightInstance.transform.rotation = secondQRRotation * Quaternion.Euler(0, 0, 0);
                wrongInstance.transform.position = secondQRPosition - new Vector3(0, 0.03f, 0);
                wrongInstance.transform.rotation = secondQRRotation * Quaternion.Euler(0, 0, 0);
                UpdateQRCodeDisplay();
                if (propellerManager != null && propellerManager.isMounting)
                {
                    propellerManager.CheckSecondQRCodePosition(ballInstance.transform.position, this);
                }
            }
            else
            {
                HideAll();
            }
        }
    }

    /**
     * @brief Updates which QR code indicator is visible.
     *
     * Shows the ball indicator during mounting; otherwise shows tick or cross based on QR code ID.
     */
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

    /**
     * @brief Displays the ball indicator.
     *
     * Activates the ball and hides tick and cross indicators.
     */
    private void ShowBall()
    {
        ballInstance.SetActive(true);
        rightInstance.SetActive(false);
        wrongInstance.SetActive(false);
    }

    /**
     * @brief Displays the tick or cross indicator.
     *
     * If the QR code ID matches the current propeller index, shows a tick; otherwise, shows a cross.
     */
    private void ShowPrefab()
    {
        if (qrCodeID == propellerManager.currentCopterIndex + 1)
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

    /**
     * @brief Hides all QR code indicators.
     */
    private void HideAll()
    {
        ballInstance.SetActive(false);
        rightInstance.SetActive(false);
        wrongInstance.SetActive(false);
    }

    /**
     * @brief Changes the color of the ball indicator.
     *
     * @param color The new color to apply.
     */
    public void ChangeBallColor(Color color)
    {
        if (ballInstance != null)
        {
            ballInstance.GetComponent<Renderer>().material.color = color;
        }
    }
}
