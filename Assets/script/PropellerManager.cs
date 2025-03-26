using UnityEngine;

public class Propellermanager : MonoBehaviour
{
    // this class is for handling the propeller hologram
    public GameObject propPrefab;//the red propller hologram prefab
    public GameObject propPrefab_black;//black propeller hologram prefab
    public GameObject arrowPrefab; // Reference to the arrow prefab
    private GameObject arrow; // Reference to the arrow instance

    private GameObject[] props;//a list to hold the propeller holograms
    private int count;//the number of propellers to be mounted

    public MainQR mainQR; // Reference to the ImageTargetTransform
    public Camera ARcamera; // Reference to the Vuforia AR camera

    public bool isMounting { get; private set; } = false;//when isMounting is true, if user scan the QR code, a sphere will be shown to indicate if the position of the small QR code is matching the propeller hologram    
    public int currentCopterIndex { get; private set; } = 0;
    private bool isSymmetric;
    public bool isDisplaying = false;//when a propeller hologram is being displayed, if user scan the QR code, a tick or cross will be shown to indicate if the user is grabbing the correct number of propeller

    // Configuration placeholders
    private Vector3[][] relativePositions = new Vector3[4][];
    private Vector3[][] relativeRotations = new Vector3[4][];

    private Vector3 boxPosition;//for holding the main QR's position obtained from the MainQR script

    private float width = 0.1482f;//the parameter of the real box
    private float height = -0.1009f;
    private float length_neg = -0.3505f;
    private float length_pos = 0.3455f;

    void Start()
    {
        InitializeConfigurations();
        arrow = Instantiate(arrowPrefab);
        arrow.SetActive(false);
    }

    private void InitializeConfigurations()
    {
        // Configuration 1,4 propellers and symmetric, shown clockwise
        relativePositions[0] = new Vector3[]
        {
            new Vector3(length_pos,height, 0),
            new Vector3(-0.0025f,height, width),
            new Vector3(length_neg, height, 0),
            new Vector3(-0.0025f, height, -width)
        };

        relativeRotations[0] = new Vector3[]
        {
            new Vector3(-90, 0, 90),
            new Vector3(-90, 0, 0),
            new Vector3(-90, 0, -90),
            new Vector3(-90, 0, 180)
        };


        // Configuration 2, 4 propellers and asymmetric, shown clockwise
        relativePositions[1] = new Vector3[]
        {
            new Vector3(length_pos, -0.1009f, -0.0402f),
            new Vector3(-0.1225f, -0.1009f, width),
            new Vector3(length_neg, -0.1009f, 0.0198f),
            new Vector3(0.1975f, -0.1009f, -width)
        };

        relativeRotations[1] = new Vector3[]
        {
            new Vector3(-90, 0, 90),
            new Vector3(-90, 0, 0),
            new Vector3(-90, 0, -90),
            new Vector3(-90, 0, 180)
        };

        // Configuration 3, 6 propellers and symmetric, shown clockwise
        relativePositions[2] = new Vector3[]
        {
                new Vector3(length_pos, height, 0),
                new Vector3(0.1975f, height, width),
                new Vector3(-0.2025f, height, width ),
                new Vector3(length_neg, height, 0),
                new Vector3(-0.2025f, height, -width),
                new Vector3(0.1975f, height, -width)


        };

        relativeRotations[2] = new Vector3[]
        {
                new Vector3(-90, 0, 90),
                new Vector3(-90, 0, 0),
                new Vector3(-90, 0, 0),
                new Vector3(-90, 0, -90),
                new Vector3(-90, 0, 180),
                new Vector3(-90, 0, 180),

        };

        // Configuration 4, 6 propellers and asymmetric, shown clockwise
        relativePositions[3] = new Vector3[]
        {
                new Vector3(length_pos, height, -0.052f),
                new Vector3(0.2725f, height, width),
                new Vector3(-0.1425f, height, width),
                new Vector3(length_neg, height, 0.0198f),
                new Vector3(-0.1025f, height, -width ),
                new Vector3(0.2725f, height, -width)
        };

        relativeRotations[3] = new Vector3[]
        {
                new Vector3(-90, 0, 90),
                new Vector3(-90, 0, 0),
                new Vector3(-90, 0, 0),
                new Vector3(-90, 0, -90),
                new Vector3(-90, 0, 180),
                new Vector3(-90, 0, 180),
        };
    }

    public void SetConfiguration(bool symmetric, int numberOfCopters)//Interaction manager handle the input of the choice, and process this information here
    {
        isSymmetric = symmetric;
        count = numberOfCopters;
        InstantiateCylinders();
    }

    private int GetConfigurationIndex()
    {
        // Determine the configuration index(4 kinds in total) based on isSymmetric and coptersToMount
        if (isSymmetric)
        {
            return count == 4 ? 0 : 2;
        }
        else
        {
            return count == 4 ? 1 : 3;
        }
    }

    private void InstantiateCylinders()
    {
        props = new GameObject[count];
        for (int i = 0; i < props.Length; i++)
        {
            if (i % 2 == 0)//One red, one black after each other
            {
                props[i] = Instantiate(propPrefab);
                Debug.Log($"Instantiated red cylinder at index {i}");
            }
            else
            {
                props[i] = Instantiate(propPrefab_black);
                Debug.Log($"Instantiated black cylinder at index {i}");
            }
            props[i].SetActive(false);
        }
    }

    public void UpdateCylinderPositions(Vector3 qrPosition, Quaternion qrRotation)//This method is called by the MainQR script to update the position of the propellers
    {
        boxPosition = qrPosition;  // Update the box position
        int configIndex = GetConfigurationIndex();

        for (int i = 0; i < count; i++)
        {
            if (isDisplaying && i <= currentCopterIndex - 1)//this is to handle when this function is called during a mounting process, the currently displayed propeller hologram will also be updated
            {
                // Update position of currently displaying copter
                props[i].transform.position = boxPosition + qrRotation * relativePositions[configIndex][i];
                props[i].transform.rotation = qrRotation * Quaternion.Euler(relativeRotations[configIndex][i]);
                props[i].SetActive(true);
                UpdateArrowTransform(props[i].transform.position);
            }
            else
            {
                // Update positions of other copters but keep them inactive
                props[i].transform.position = boxPosition + qrRotation * relativePositions[configIndex][i];
                props[i].transform.rotation = qrRotation * Quaternion.Euler(relativeRotations[configIndex][i]);
                props[i].SetActive(false);
            }
        }
    }



    public void ActivateNextCopter()//This method is called by the InteractionManager script to activate the next propeller hologram
    {
        if (currentCopterIndex < count)
        {
            props[currentCopterIndex].SetActive(true);
            isDisplaying = true;
            isMounting = true;
            UpdateQRCodeDisplay();//
            Debug.Log($"Copter {currentCopterIndex} activated at position: {props[currentCopterIndex].transform.position}");
            UpdateArrowTransform(props[currentCopterIndex].transform.position);//Show an arrow to better indicate where the propeller hologram is
            currentCopterIndex++;
        }
        else
        {
            Debug.LogError("All copters have been activated.");
        }
        ChangePlugColor();
    }

    public void HideCurrentCopter()
    {
        if (currentCopterIndex > 0)
        {
            props[currentCopterIndex - 1].SetActive(false);
            isDisplaying = false;
        }
        arrow.SetActive(false);
    }

    public void FinishMountingStep()//called in InteractionManager script, so that if scan the small QR, the ball will not be shown but the tick and cross is
    {
        if (isMounting)
        {
            isMounting = false;
            UpdateQRCodeDisplay();
            Debug.Log("Mounting step completed.");
        }
    }

    private void UpdateQRCodeDisplay()//used with secondQRCodeTransform script to show the sphere or tick/cross
    {
        foreach (SecondQRCodeTransform qrTransform in FindObjectsOfType<SecondQRCodeTransform>())
        {
            qrTransform.UpdateQRCodeDisplay(); // Update QR display based on the mounting state
        }
    }


    public void CheckSecondQRCodePosition(Vector3 secondQRPosition, SecondQRCodeTransform qrTransform)
        //the logic for checking if the small QR code is in the correct position
    {
        bool isVeryClose = false;
        bool isKindOfNear = false;//for now we only use two state, but more can be implemented, as well as threshold change

        if (currentCopterIndex > 0)  // Ensure there is a currently displaying hologram
        {
            // Get the currently displaying cylinder's position.we don't check absolute coordinate, but using the local coordinate of the camera.
            GameObject currentCylinder = props[currentCopterIndex - 1];
            Vector3 localCylinderPosition = ARcamera.transform.InverseTransformPoint(currentCylinder.transform.position);
            Vector3 localQRPosition = ARcamera.transform.InverseTransformPoint(secondQRPosition);

            // Calculate the distance in both x and y directions of the camera's local coordinate system. we don't check absolute coordinate, but using the local coordinate of the camera.
            float distanceX = Mathf.Abs(localCylinderPosition.x - localQRPosition.x);
            float distanceY = Mathf.Abs(localCylinderPosition.y - localQRPosition.y);

            if (distanceX < 0.03f && distanceY < 0.03f)
            {
                isVeryClose = true;
                Debug.Log("QR code is very close to the currently displaying cylinder.");
            }
            else if (distanceX < 0.03f && distanceY < 0.03f)
            {
                isKindOfNear = true;
                Debug.Log("QR code is kind of near the currently displaying cylinder.");
            }
        }

        if (qrTransform != null)
        {
            if (isVeryClose)
            {
                qrTransform.ChangeBallColor(Color.green);
            }
            else if (isKindOfNear)
            {
                qrTransform.ChangeBallColor(Color.green);
            }
            else
            {
                qrTransform.ChangeBallColor(Color.red);
            }
        }
    }




    private void UpdateArrowTransform(Vector3 copterPosition)//We use an Arrow to help finding the hologram for user. The arrow will be shown at the 4 edges of the box to indicate where to slide in the propeller
    {
        // Determine the relative position of the arrow based on the copter position
        if (copterPosition.x <= boxPosition.x && copterPosition.y <= boxPosition.y)
        {
            arrow.transform.position = boxPosition + new Vector3(length_neg, height, -0.14f);
        }
        else if (copterPosition.x > boxPosition.x && copterPosition.y <= boxPosition.y)
        {
            arrow.transform.position = boxPosition + new Vector3(length_pos, height, -0.14f);
        }
        else if (copterPosition.x <= boxPosition.x && copterPosition.y > boxPosition.y)
        {
            arrow.transform.position = boxPosition + new Vector3(length_neg, -height, 0.15f);
        }
        else
        {
            arrow.transform.position = boxPosition + new Vector3(length_pos, -height, 0.15f);
        }


        // Calculate the direction from the arrow to the copter position
        Vector3 directionToCopter = copterPosition - arrow.transform.position;
        directionToCopter.z = 0f;

        // Calculate the angle in the XY plane
        float angle = Mathf.Atan2(directionToCopter.y, directionToCopter.x) * Mathf.Rad2Deg + 180f;

        // Create a rotation around the z-axis
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

        // Apply the rotation to the arrow
        arrow.transform.rotation = targetRotation;

        // Make sure the arrow is active
        arrow.SetActive(true);
    }


    public void ChangePlugColor()
    {
        mainQR.ShowPlug(currentCopterIndex);
    }

}