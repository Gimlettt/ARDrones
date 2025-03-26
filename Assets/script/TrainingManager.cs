/**
 * @file TrainingManager.cs
 * @brief Manages the training mode for the AR application.
 *
 * This class handles calibration, freeze/adjust toggling, and UI updates during training sessions.
 */

using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Vuforia;

public class TrainingManager : MonoBehaviour
{
    private ImageTargetBehaviour imageTargetBehaviour; ///< For tracking the QR code.
    public GameObject cube; ///< Reference to the cube hologram.
    public PressableButtonHoloLens2 slightAdjustmentButton;
    public PressableButtonHoloLens2 toggleFreezeButton;
    public PressableButtonHoloLens2 calibrationButton;
    private bool isFrozen = false;
    private bool isAdjusting = false;

    public TextMeshPro progressTextSlate; ///< UI element for instructions.
    public Camera arCamera; ///< Reference to the AR camera.
    public Material badMaterial; ///< Material used when not calibrated or frozen.
    public Material goodMaterial; ///< Material used when calibrated/frozen.
    private float distance = 0;

    private Renderer cubeRenderer; ///< Renderer for the cube.

    /**
     * @brief Initializes the training manager.
     *
     * Sets up the image target, hides the cube, assigns materials, and attaches button event listeners.
     */
    void Start()
    {
        imageTargetBehaviour = GetComponent<ImageTargetBehaviour>();
        cube.SetActive(false);
        cubeRenderer = cube.GetComponent<Renderer>();
        if (cubeRenderer != null && badMaterial != null)
        {
            cubeRenderer.material = badMaterial;
        }
        if (toggleFreezeButton != null)
        {
            toggleFreezeButton.ButtonPressed.AddListener(ToggleFreeze);
        }
        if (slightAdjustmentButton != null)
        {
            slightAdjustmentButton.ButtonPressed.AddListener(ToggleAdjustment);
        }
        calibrationButton.ButtonPressed.AddListener(OnCalibration);

        var objectManipulator = cube.GetComponent<ObjectManipulator>();
        if (objectManipulator != null)
        {
            objectManipulator.enabled = false;
        }
        var nearInteractionGrabbable = cube.GetComponent<NearInteractionGrabbable>();
        if (nearInteractionGrabbable != null)
        {
            nearInteractionGrabbable.enabled = false;
        }
    }

    /**
     * @brief Updates the training mode each frame.
     *
     * When not frozen or adjusting, tracks the QR code to update the cube's position and rotation.
     */
    void Update()
    {
        if (imageTargetBehaviour != null && !isFrozen && !isAdjusting)
        {
            TargetStatus targetStatus = imageTargetBehaviour.TargetStatus;
            if (targetStatus.Status == Status.TRACKED)
            {
                Vector3 qrPosition = imageTargetBehaviour.transform.position;
                Quaternion qrRotation = imageTargetBehaviour.transform.rotation;
                Vector3 cameraToQR = qrPosition - arCamera.transform.position;
                distance = cameraToQR.magnitude;
                cameraToQR.Normalize();
                Vector3 adjustedPosition = qrPosition + new Vector3(0, -0.045f, 0) + cameraToQR * 0.03f;
                cube.transform.position = adjustedPosition;
                cube.transform.rotation = qrRotation * Quaternion.Euler(-90, 0, 0);
                cube.SetActive(true);
                cube.transform.parent = null;
            }
        }
    }

    /**
     * @brief Handles calibration.
     *
     * Updates the progress text with the current distance.
     */
    public void OnCalibration()
    {
        progressTextSlate.text = $"{distance}";
    }

    /**
     * @brief Displays freeze instructions.
     *
     * Updates the progress text with instructions for freezing the hologram.
     */
    public void OnFreezeButtonPressed()
    {
        progressTextSlate.text = "In the experiment, the hologram will be shaking. By clicking\n" +
                                "this button you can stabilize the hologram. When the button is \n" +
                                "clicked, it shows unfreeze.(You can click again to unfreeze).\n" +
                                "Now press click to adjust.";
    }

    /**
     * @brief Displays adjustment instructions.
     *
     * Updates the progress text with instructions for adjusting the cube.
     */
    public void OnAdjustPressed()
    {
        progressTextSlate.text = "When the adjust button is clicked, pinch Index finger and thumb\n" +
                                 "to move the cube. Click again to stop moving. Press unfreeze,\n" +
                                 "the box will turn purple. In the experiment, you might accidentally\n" +
                                 "click unfreeze, just click again to freeze it (box green). You may\n" +
                                 "want to unfreeze sometimes, remember to freeze it afterwards";
    }

    /**
     * @brief Toggles the freeze state.
     *
     * Changes the cube material and updates the progress text based on the freeze state.
     */
    void ToggleFreeze()
    {
        isFrozen = !isFrozen;
        Debug.Log("Freeze state toggled: " + isFrozen);
        if (isFrozen && imageTargetBehaviour)
        {
            Vector3 qrPosition = imageTargetBehaviour.transform.position;
            Quaternion qrRotation = imageTargetBehaviour.transform.rotation;
            Vector3 cameraToQR = qrPosition - arCamera.transform.position;
            cameraToQR.Normalize();
            Vector3 adjustedPosition = qrPosition + new Vector3(0, -0.045f, 0) + cameraToQR * 0.03f;
        }
        progressTextSlate.text = "In the experiment, the hologram will be shaking. By clicking\n" +
                                "this button you can stabilize the hologram. When the button is \n" +
                                "clicked, it shows unfreeze.(You can click again to unfreeze).\n" +
                                "Now press click to adjust.";
        if (cubeRenderer != null)
        {
            cubeRenderer.material = isFrozen ? goodMaterial : badMaterial;
        }
    }

    /**
     * @brief Toggles the adjustment state.
     *
     * Enables or disables object manipulation and updates the cube material and instructions.
     */
    void ToggleAdjustment()
    {
        isAdjusting = !isAdjusting;
        Debug.Log("Adjustment state toggled: " + isAdjusting);
        var objectManipulator = cube.GetComponent<ObjectManipulator>();
        var nearInteractionGrabbable = cube.GetComponent<NearInteractionGrabbable>();
        progressTextSlate.text = "When the adjust button is clicked, pinch Index finger and thumb\n" +
                                 "to move the cube. Click again to stop moving. Press unfreeze,\n" +
                                 "the box will turn purple. In the experiment, you might accidentally\n" +
                                 "click unfreeze, just click again to freeze it (box green). You may\n" +
                                 "want to unfreeze sometimes, remember to freeze it afterwards";
        if (objectManipulator != null)
        {
            objectManipulator.enabled = isAdjusting;
        }
        if (nearInteractionGrabbable != null)
        {
            nearInteractionGrabbable.enabled = isAdjusting;
        }
        if (cubeRenderer != null)
        {
            cubeRenderer.material = isAdjusting ? badMaterial : goodMaterial;
        }
    }
}
