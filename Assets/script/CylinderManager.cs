using UnityEngine;

public class PropellerManager : MonoBehaviour
{
    public GameObject cylinderPrefab;
    public GameObject cylinderPrefab_black;
    private GameObject[] cylinders;
    public GameObject arrowPrefab; // Reference to the arrow prefab
    private GameObject arrow;
    public MainQR mainQR; // Reference to the ImageTargetTransform

    private int count;
    public Camera ARcamera; // Reference to the Vuforia AR camera

    public bool isMounting { get; private set; } = false;
    public int currentCopterIndex { get; private set; } = 0;

    private bool isSymmetric;
    public bool isDisplaying = false;

    // Configuration placeholders
    private Vector3[][] relativePositions = new Vector3[4][];
    private Vector3[][] relativeRotations = new Vector3[4][];

    private Vector3 boxPosition;

    private float width = 0.1482f;
    private float height = -0.1009f;
    private float length_neg = -0.3505f;
    private float length_pos = 0.3455f;

    private float plugXfar = 0.0375f;
    private float plugXclose = 0.0125f;
    private float plugY = 0.1f;
    private float plugZ = 0.0235f;

    void Start()
    {
        InitializeConfigurations();
        arrow = Instantiate(arrowPrefab);
        arrow.SetActive(false);
    }

    private void InitializeConfigurations()
    {
        // Configuration 1,4 and symmetric, shown clockwise
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


        // Configuration 2 
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

        // Configuration 3 (placeholder)
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

        // Configuration 4 (placeholder)
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

    public void SetConfiguration(bool symmetric, int numberOfCopters)
    {
        isSymmetric = symmetric;
        count = numberOfCopters;
        InstantiateCylinders();
    }

    private void InstantiateCylinders()
    {
        cylinders = new GameObject[count];
        for (int i = 0; i < cylinders.Length; i++)
        {
            if (i % 2 == 0)
            {
                cylinders[i] = Instantiate(cylinderPrefab);
                Debug.Log($"Instantiated red cylinder at index {i}");
            }
            else
            {
                cylinders[i] = Instantiate(cylinderPrefab_black);
                Debug.Log($"Instantiated black cylinder at index {i}");
            }
            cylinders[i].SetActive(false);
        }
    }

    public void UpdateCylinderPositions(Vector3 qrPosition, Quaternion qrRotation)
    {
        boxPosition = qrPosition;  // Update the box position
        int configIndex = GetConfigurationIndex();

        for (int i = 0; i < count; i++)
        {
            if (isDisplaying && i <= currentCopterIndex - 1)
            {
                // Update position of currently displaying copter
                cylinders[i].transform.position = boxPosition + qrRotation * relativePositions[configIndex][i];
                cylinders[i].transform.rotation = qrRotation * Quaternion.Euler(relativeRotations[configIndex][i]);
                cylinders[i].SetActive(true);
                UpdateArrowTransform(cylinders[i].transform.position);
            }
            else
            {
                // Update positions of other copters but keep them inactive
                cylinders[i].transform.position = boxPosition + qrRotation * relativePositions[configIndex][i];
                cylinders[i].transform.rotation = qrRotation * Quaternion.Euler(relativeRotations[configIndex][i]);
                cylinders[i].SetActive(false);
            }
        }
    }

    private int GetConfigurationIndex()
    {
        // Determine the configuration index based on isSymmetric and coptersToMount
        if (isSymmetric)
        {
            return count == 4 ? 0 : 2;
        }
        else
        {
            return count == 4 ? 1 : 3;
        }
    }

    public void ActivateNextCopter()
    {
        if (currentCopterIndex < count)
        {
            cylinders[currentCopterIndex].SetActive(true);
            isDisplaying = true;
            isMounting = true;
            UpdateQRCodeDisplay();
            Debug.Log($"Copter {currentCopterIndex} activated at position: {cylinders[currentCopterIndex].transform.position}");
            UpdateArrowTransform(cylinders[currentCopterIndex].transform.position);
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
            cylinders[currentCopterIndex - 1].SetActive(false);
            isDisplaying = false;
        }
        arrow.SetActive(false);
    }

    public void FinishMountingStep()
    {
        if (isMounting)
        {
            isMounting = false;
            UpdateQRCodeDisplay();
            Debug.Log("Mounting step completed.");
        }
    }

    private void UpdateQRCodeDisplay()
    {
        foreach (SecondQRCodeTransform qrTransform in FindObjectsOfType<SecondQRCodeTransform>())
        {
            qrTransform.UpdateQRCodeDisplay(); // Update QR display based on the mounting state
        }
    }


    public void CheckSecondQRCodePosition(Vector3 secondQRPosition, SecondQRCodeTransform qrTransform)
    {
        bool isVeryClose = false;
        bool isKindOfNear = false;

        if (currentCopterIndex > 0)  // Ensure there is a currently displaying hologram
        {
            // Get the currently displaying cylinder's position
            GameObject currentCylinder = cylinders[currentCopterIndex - 1];
            Vector3 localCylinderPosition = ARcamera.transform.InverseTransformPoint(currentCylinder.transform.position);
            Vector3 localQRPosition = ARcamera.transform.InverseTransformPoint(secondQRPosition);

            // Calculate the distance in both x and y directions of the camera's local coordinate system
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




    private void UpdateArrowTransform(Vector3 copterPosition)
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