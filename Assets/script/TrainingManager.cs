using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Vuforia;

public class TrainingManager : MonoBehaviour
{
    private ImageTargetBehaviour imageTargetBehaviour;
    public GameObject cube;
    public PressableButtonHoloLens2 slightAdjustmentButton;
    public PressableButtonHoloLens2 toggleFreezeButton;
    public PressableButtonHoloLens2 calibrationButton;
    private bool isFrozen = false;
    private bool isAdjusting = false;

    public TextMeshPro progressTextSlate;
    public Camera arCamera;
    public Material badMaterial; // Assign this in the Unity Inspector
    public Material goodMaterial; // Assign this in the Unity Inspector
    private float distance = 0;

    private Renderer cubeRenderer; // Reference to the Renderer of the cube

    void Start()
    {
        imageTargetBehaviour = GetComponent<ImageTargetBehaviour>();
        cube.SetActive(false);
        // Get the Renderer component of the cube
        cubeRenderer = cube.GetComponent<Renderer>();

        // Set initial material to badMaterial
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

    public void OnCalibration()
    {
        // Example with manual line breaks
        progressTextSlate.text = $"{distance}";
    }
    public void OnFreezeButtonPressed()
    {
        // Example with manual line breaks
        progressTextSlate.text = "In the experiment,the hologram will be shaking. By clicking\n" +
                                "this button you can stabilize the hologram. When the button is \n" +
                                "clicked, it shows unfreeze.(You can click again to unfreeze).\n" +
                                 "Now press click to adjust.";
    }

    public void OnAdjustPressed()
    {
        // Example with manual line breaks
        progressTextSlate.text = "When the adjust button is clicked,pinch Index finger and thumb\n" +
                                 "to move the cube.Click again to stop ,moving.Press unfreeze,\n" +
                                 "the box will turn purple.In the experiment, you might accidentally \n" +
                                 "click unfreeze, just click again to freeze it(box green). You may\n"+
                                 "want to unfreeze sometimes, remember to freeze it afterwards";
    }

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
        progressTextSlate.text = "In the experiment,the hologram will be shaking. By clicking\n" +
                                "this button you can stabilize the hologram. When the button is \n" +
                                "clicked, it shows unfreeze.(You can click again to unfreeze).\n" +
                                 "Now press click to adjust.";
        

        // Change material based on the freeze state
        if (cubeRenderer != null)
        {
            cubeRenderer.material = isFrozen ? goodMaterial : badMaterial;
        }
    }

    void ToggleAdjustment()
    {
        isAdjusting = !isAdjusting;
        Debug.Log("Adjustment state toggled: " + isAdjusting);

        var objectManipulator = cube.GetComponent<ObjectManipulator>();
        var nearInteractionGrabbable = cube.GetComponent<NearInteractionGrabbable>();
        progressTextSlate.text = "When the adjust button is clicked,pinch Index finger and thumb\n" +
                                 "to move the cube.Click again to stop moving.Press unfreeze,\n" +
                                 "the box will turn purple.In the experiment, you might accidentally \n" +
                                 "click unfreeze, just click again to freeze it(box green). You may\n" +
                                 "want to unfreeze sometimes, remember to freeze it afterwards";

        if (objectManipulator != null)
        {
            objectManipulator.enabled = isAdjusting;
        }
        if (nearInteractionGrabbable != null)
        {
            nearInteractionGrabbable.enabled = isAdjusting;
        }

        // Change material based on the adjusting state
        if (cubeRenderer != null)
        {
            cubeRenderer.material = isAdjusting ? badMaterial : goodMaterial;
        }
    }
}
