/**
 * @file MainQR.cs
 * @brief Manages the main QR code for tracking the box in AR.
 *
 * This class handles QR code tracking using Vuforia, updates the box hologram's position and color,
 * and manages plug and guidance window displays.
 */

using UnityEngine;
using TMPro;
using Vuforia;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;

public class MainQR : MonoBehaviour
{
    private ImageTargetBehaviour imageTargetBehaviour; ///< Tracks the main QR code.
    
    /// <summary>
    /// Reference to the PropellerManager.
    /// </summary>
    public PropellerManager propellerManager;
    /// <summary>
    /// Reference to the InteractionManager.
    /// </summary>
    public InteractionManager interactionManager;
    /// <summary>
    /// Reference to the UI window for progress display.
    /// </summary>
    public GameObject window;
    /// <summary>
    /// Reference to the AR camera.
    /// </summary>
    public Camera arCamera;

    private bool isFrozen = false;
    /// <summary>
    /// Reference to the freeze button.
    /// </summary>
    public PressableButtonHoloLens2 toggleFreezeButton;

    private bool hasWindowTrackedOnce = false;
    private Vector3 lastKnownWindowPosition;
    private Quaternion lastKnownWindowRotation;

    /// <summary>
    /// Reference to the box hologram.
    /// </summary>
    public GameObject Box;
    /// <summary>
    /// Array of child objects of the box used for color changes.
    /// </summary>
    public GameObject[] boxChildren;
    /// <summary>
    /// Material applied when the box is frozen.
    /// </summary>
    public Material materialBad;
    /// <summary>
    /// Material applied when the box is unfrozen.
    /// </summary>
    public Material materialGood;
    /// <summary>
    /// Array of plugs whose colors are updated.
    /// </summary>
    public GameObject[] plugs;
    /// <summary>
    /// Material for the plug when activated.
    /// </summary>
    public Material materialPlug;

    /**
     * @brief Initializes the main QR code and associated components.
     *
     * Sets up the image target for QR tracking, hides the box, and attaches the freeze button event.
     */
    void Start()
    {
        imageTargetBehaviour = GetComponent<ImageTargetBehaviour>();
        if (imageTargetBehaviour == null)
        {
            Debug.LogError("No Image Target found on this GameObject.");
        }
        Box.SetActive(false);
        if (toggleFreezeButton != null)
        {
            toggleFreezeButton.ButtonPressed.AddListener(ToggleFreeze);
        }
    }

    /**
     * @brief Updates the box hologram and guidance window each frame.
     *
     * If the QR code is tracked and not frozen, updates the box position, color, and propeller positions.
     */
    void Update()
    {
        if (imageTargetBehaviour != null && !isFrozen)
        {
            TargetStatus targetStatus = imageTargetBehaviour.TargetStatus;
            if (targetStatus.Status == Status.TRACKED)
            {
                Vector3 qrPosition = imageTargetBehaviour.transform.position;
                Quaternion qrRotation = imageTargetBehaviour.transform.rotation;
                Vector3 cameraToQR = qrPosition - arCamera.transform.position;
                cameraToQR.Normalize();
                Vector3 adjustedPosition = qrPosition + new Vector3(0, -0.045f, 0) + cameraToQR * 0.03f;
                Box.transform.position = adjustedPosition;
                Box.transform.rotation = qrRotation * Quaternion.Euler(-90, 0, 0);
                Box.SetActive(true);
                ColorChange(isFrozen);
                propellerManager.UpdateCylinderPositions(adjustedPosition, qrRotation);
                if (propellerManager.isDisplaying)
                {
                    propellerManager.ChangePlugColor();
                }
                if (!hasWindowTrackedOnce)
                {
                    lastKnownWindowPosition = qrPosition;
                    lastKnownWindowRotation = Quaternion.LookRotation(arCamera.transform.position - lastKnownWindowPosition);
                    UpdateWindowPosition();
                    hasWindowTrackedOnce = true;
                }
            }
            else
            {
                Box.SetActive(false);
            }
        }
        if (hasWindowTrackedOnce)
        {
            UpdateWindowPosition();
        }
    }

    /**
     * @brief Toggles the freeze state of the box hologram.
     *
     * Updates propeller positions and changes the box color based on the freeze state.
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
            propellerManager.UpdateCylinderPositions(adjustedPosition, qrRotation);
        }
        ColorChange(isFrozen);
        if (propellerManager.isDisplaying)
        { 
            propellerManager.ChangePlugColor();
        }
    }

    /**
     * @brief Updates the position and orientation of the guidance window.
     *
     * Positions the window above the box and rotates it to face the AR camera.
     */
    void UpdateWindowPosition()
    {
        Vector3 windowPosition = lastKnownWindowPosition + new Vector3(0, 0.2f, 0);
        window.transform.position = windowPosition;
        Vector3 directionToCamera = arCamera.transform.position - window.transform.position;
        directionToCamera.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(-directionToCamera);
        window.transform.rotation = targetRotation;
    }

    /**
     * @brief Changes the box's color based on its freeze state.
     *
     * Iterates over the box's children and updates their materials.
     *
     * @param isBad If true, applies the good material; otherwise, applies the bad material.
     */
    public void ColorChange(bool isBad)
    {
        foreach (GameObject child in boxChildren)
        {
            Renderer[] renderers = child.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                {
                    Material[] materials = renderer.materials;
                    for (int i = 0; i < materials.Length; i++)
                    {
                        materials[i] = isBad ? materialGood : materialBad;
                    }
                    renderer.materials = materials;
                }
            }
        }
    }

    /**
     * @brief Updates the plug color based on the current propeller.
     *
     * Changes the material of the plug corresponding to the current propeller index.
     *
     * @param copterOrder The order number of the propeller.
     */
    public void ShowPlug(int copterOrder)
    {
        Renderer plugRenderer = plugs[copterOrder - 1].GetComponent<Renderer>();
        if (copterOrder != 1)
        {
            Renderer previous_plug = plugs[copterOrder - 2].GetComponent<Renderer>();
            if (isFrozen)
            {
                previous_plug.material = materialGood;
            }
            else
            {
                previous_plug.material = materialBad;
            }
        }
        plugRenderer.material = materialPlug;
    }
}
