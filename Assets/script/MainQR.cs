using UnityEngine;
using TMPro;
using Vuforia;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;

public class MainQR : MonoBehaviour
{
    //this class is for handling the big/main QR code on the box. We use it track the position of the real box. Around this coordinate,
    //we place the hologram of the propellers.Due to the fact that when people move their head the hologram of the
    //box will move, we can to freeze the box's hologram. This is done by the freeze button in the scene.
    //Use can either use unfreeze the hologram all the tie, so it will keep tracking the QR code, but if not tracked, no hologram will be shown. Or
    // they can freeze the hologram, so that the hologram will stay in the last position when the QR code is tracked.
    //Due to vuforia's capability, we might manually adjust the hologram's position in the code.
    //We also use this code to manage the color of box's hologram. When the box's hologram is frozen, the box will turn green.
    //We also change the plug's color when needed.

    private ImageTargetBehaviour imageTargetBehaviour;//using vuforia image target behaviour to track the small QR code's position

    public PropellerManager propellerManager;//reference to the propellerManager
    public InteractionManager interactionManager;//reference to the interactionManager
    public GameObject window;  // Reference to the progress text
    public Camera arCamera;  // Reference to the vuforia's AR camera

    private bool isFrozen = false;
    public PressableButtonHoloLens2 toggleFreezeButton;//reference to the freeze button

    private bool hasWindowTrackedOnce = false; // Flag to check if window tracking has occurred.
    private Vector3 lastKnownWindowPosition;//the big QR's position. We only track it once to know where to place the guidance window
    private Quaternion lastKnownWindowRotation;

    public GameObject Box;//reference to the box's hologram
    public GameObject[] boxChildren;//According to the CAD model, the box has 2 childeren, we access them to be able to change their color
    public Material materialBad;//A transparent green materal
    public Material materialGood;//A transparent purple material
    public GameObject[] plugs;//reference to the plugs, a list
    public Material materialPlug;//A orange material to show the plug



    void Start()
    {
        imageTargetBehaviour = GetComponent<ImageTargetBehaviour>();//how to use vuforia to track the big QR code

        if (imageTargetBehaviour == null)
        {
            Debug.LogError("No Image Target found on this GameObject.");
        }

        Box.SetActive(false);

        if (toggleFreezeButton != null)
        {
            toggleFreezeButton.ButtonPressed.AddListener(ToggleFreeze);//attach the ToggleFreeze method to the button   
        }


    }

    void Update()
    {
        if (imageTargetBehaviour != null && !isFrozen)//when unfreeze, we want to track the QR code's position
        {
            TargetStatus targetStatus = imageTargetBehaviour.TargetStatus;

            if (targetStatus.Status == Status.TRACKED)//there is 3 mode of tracking, and we only use tracked status
            {
                Vector3 qrPosition = imageTargetBehaviour.transform.position;
                Quaternion qrRotation = imageTargetBehaviour.transform.rotation;

                Vector3 cameraToQR = qrPosition - arCamera.transform.position;
                cameraToQR.Normalize();//we get a normal vector between the camera and the QR, because we notice that the offset in tracking is in this direction.

                Vector3 adjustedPosition = qrPosition + new Vector3(0, -0.045f, 0) + cameraToQR * 0.03f;//shifted the hologram downwards and push it further a bit

                Box.transform.position = adjustedPosition;
                Box.transform.rotation = qrRotation * Quaternion.Euler(-90, 0, 0);// the -90 is origianted from the box was not facing up

                Box.SetActive(true);
                ColorChange(isFrozen);// call the color change method, so that if the box is froze it's green, otherwise purple

                propellerManager.UpdateCylinderPositions(adjustedPosition, qrRotation);//keep updating the propellers' position around the box
                if (propellerManager.isDisplaying)
                {
                    propellerManager.ChangePlugColor();//this part is for handling when user within the mounting process freeze/unfreeze, the plug color is still shown correctly
                }



                //Box.transform.parent = null;This is to achieve: when we are in unfreeze mode, if we don't track the box, the hologram won't be gone

                // Only update the window position once
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
            // Continue to update the window to face the user, when people move around the box, the window will face them
            UpdateWindowPosition();
        }
    }

    void ToggleFreeze()
    {
        isFrozen = !isFrozen;
        Debug.Log("Freeze state toggled: " + isFrozen);

        if (isFrozen && imageTargetBehaviour)
        {//as long as the hologram is frozen, we update all the propellers' position to match the box's position
            Vector3 qrPosition = imageTargetBehaviour.transform.position;
            Quaternion qrRotation = imageTargetBehaviour.transform.rotation;

            Vector3 cameraToQR = qrPosition - arCamera.transform.position;
            cameraToQR.Normalize();

            Vector3 adjustedPosition = qrPosition + new Vector3(0, -0.045f, 0) + cameraToQR * 0.03f;// same logic as the update

            propellerManager.UpdateCylinderPositions(adjustedPosition, qrRotation);//this method is to position the propellers around the box. Note only when user press the freeze button, the copter position will be updated
        }
        ColorChange(isFrozen);
        if (propellerManager.isDisplaying) { 
        propellerManager.ChangePlugColor();//this part is for handling when user within the mounting process freeze/unfreeze, the plug color is still shown correctly
    }

    }



    void UpdateWindowPosition()
    {
        // Set the window's position 20cm above the QR code's original position
        Vector3 windowPosition = lastKnownWindowPosition + new Vector3(0, 0.2f, 0);
        window.transform.position = windowPosition;

        // Make the window face the user
        Vector3 directionToCamera = arCamera.transform.position - window.transform.position;
        directionToCamera.y = 0; // Lock the rotation in the y-axis to avoid tilting
        Quaternion targetRotation = Quaternion.LookRotation(-directionToCamera);
        window.transform.rotation = targetRotation;
    }



    public void ColorChange(bool isBad)// handling the box's color, if frozen, green, otherwise purple
    {
        foreach (GameObject child in boxChildren)//the box has 2 children, we change their color
        {
            Renderer[] renderers = child.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                {
                    // Get all materials of the renderer
                    Material[] materials = renderer.materials;

                    // Loop through each material and update it
                    for (int i = 0; i < materials.Length; i++)
                    {
                        materials[i] = isBad ? materialGood : materialBad;
                    }

                    // Apply the updated materials back to the renderer
                    renderer.materials = materials;
                }
            }
        }
    }

    public void ShowPlug(int copterOrder)//handling the plug's color
    {
        Renderer plugRenderer = plugs[copterOrder - 1].GetComponent<Renderer>();//plug[0]correspond to the first copterOrder
        if (copterOrder != 1)//when the next plug change to orange, the previous plug will change back to bad
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
        plugRenderer.material = materialPlug;//change the plug's color to orange
    }
}
