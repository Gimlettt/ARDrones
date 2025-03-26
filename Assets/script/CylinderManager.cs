/**
 * @file CylinderManager.cs
 * @brief Manages the propeller (cylinder) holograms in AR.
 *
 * This class handles instantiation, positioning, and state management of propeller holograms.
 */

using UnityEngine;

public class PropellerManager : MonoBehaviour
{
    /// <summary>
    /// Prefab for red cylinders.
    /// </summary>
    public GameObject cylinderPrefab;
    /// <summary>
    /// Prefab for black cylinders.
    /// </summary>
    public GameObject cylinderPrefab_black;
    private GameObject[] cylinders;

    /// <summary>
    /// Prefab for the guidance arrow.
    /// </summary>
    public GameObject arrowPrefab;
    private GameObject arrow;

    /// <summary>
    /// Reference to the MainQR script.
    /// </summary>
    public MainQR mainQR;

    private int count;
    /// <summary>
    /// Reference to the Vuforia AR camera.
    /// </summary>
    public Camera ARcamera;

    /// <summary>
    /// Indicates whether the mounting process is active.
    /// </summary>
    public bool isMounting { get; private set; } = false;
    /// <summary>
    /// Index of the currently active propeller.
    /// </summary>
    public int currentCopterIndex { get; private set; } = 0;

    private bool isSymmetric;
    /// <summary>
    /// Flag indicating if a propeller is being displayed.
    /// </summary>
    public bool isDisplaying = false;

    // Configuration arrays for positions and rotations
    private Vector3[][] relativePositions = new Vector3[4][];
    private Vector3[][] relativeRotations = new Vector3[4][];

    private Vector3 boxPosition;

    // Box and plug configuration parameters
    private float width = 0.1482f;
    private float height = -0.1009f;
    private float length_neg = -0.3505f;
    private float length_pos = 0.3455f;

    private float plugXfar = 0.0375f;
    private float plugXclose = 0.0125f;
    private float plugY = 0.1f;
    private float plugZ = 0.0235f;

    /**
     * @brief Initializes configurations and instantiates the arrow.
     *
     * Called on startup to initialize propeller configurations and create the arrow instance.
     */
    void Start()
    {
        InitializeConfigurations();
        arrow = Instantiate(arrowPrefab);
        arrow.SetActive(false);
    }

    /**
     * @brief Initializes configuration arrays for propeller positions and rotations.
     *
     * Sets up multiple configurations based on different layouts.
     */
    private void InitializeConfigurations()
    {
        // Configuration 1, 4 propellers and symmetric layout, shown clockwise
        relativePositions[0] = new Vector3[]
        {
            new Vector3(length_pos, height, 0),
            new Vector3(-0.0025f, height, width),
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

        // Configuration 2 for asymmetric layout with 4 propellers
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

        // Configuration 3 (placeholder for 6 propellers and symmetric layout)
        relativePositions[2] = new Vector3[]
        {
            new Vector3(length_pos, height, 0),
            new Vector3(0.1975f, height, width),
            new Vector3(-0.2025f, height, width),
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
            new Vector3(-90, 0, 180)
        };

        // Configuration 4 (placeholder for 6 propellers and asymmetric layout)
        relativePositions[3] = new Vector3[]
        {
            new Vector3(length_pos, height, -0.052f),
            new Vector3(0.2725f, height, width),
            new Vector3(-0.1425f, height, width),
            new Vector3(length_neg, height, 0.0198f),
            new Vector3(-0.1025f, height, -width),
            new Vector3(0.2725f, height, -width)
        };

        relativeRotations[3] = new Vector3[]
        {
            new Vector3(-90, 0, 90),
            new Vector3(-90, 0, 0),
            new Vector3(-90, 0, 0),
            new Vector3(-90, 0, -90),
            new Vector3(-90, 0, 180),
            new Vector3(-90, 0, 180)
        };
    }

    /**
     * @brief Sets the propeller configuration.
     *
     * Determines the layout based on symmetry and the number of propellers.
     *
     * @param symmetric True if a symmetric layout should be used.
     * @param numberOfCopters Number of propellers to mount.
     */
    public void SetConfiguration(bool symmetric, int numberOfCopters)
    {
        isSymmetric = symmetric;
        count = numberOfCopters;
        InstantiateCylinders();
    }

    /**
     * @brief Instantiates propeller game objects.
     *
     * Creates the propeller objects and initializes them to inactive.
     */
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

    /**
     * @brief Updates the positions and rotations of propellers.
     *
     * Positions each propeller relative to the given QR code position and rotation.
     *
     * @param qrPosition The position of the QR code.
     * @param qrRotation The rotation of the QR code.
     */
    public void UpdateCylinderPositions(Vector3 qrPosition, Quaternion qrRotation)
    {
        boxPosition = qrPosition;
        int configIndex = GetConfigurationIndex();

        for (int i = 0; i < count; i++)
        {
            if (isDisplaying && i <= currentCopterIndex - 1)
            {
                // Update position for the currently displaying propeller
                cylinders[i].transform.position = boxPosition + qrRotation * relativePositions[configIndex][i];
                cylinders[i].transform.rotation = qrRotation * Quaternion.Euler(relativeRotations[configIndex][i]);
                cylinders[i].SetActive(true);
                UpdateArrowTransform(cylinders[i].transform.position);
            }
            else
            {
                // Update position for inactive propellers
                cylinders[i].transform.position = boxPosition + qrRotation * relativePositions[configIndex][i];
                cylinders[i].transform.rotation = qrRotation * Quaternion.Euler(relativeRotations[configIndex][i]);
                cylinders[i].SetActive(false);
            }
        }
    }

    /**
     * @brief Determines the configuration index based on layout settings.
     *
     * @return The index corresponding to the current configuration.
     */
    private int GetConfigurationIndex()
    {
        if (isSymmetric)
        {
            return count == 4 ? 0 : 2;
        }
        else
        {
            return count == 4 ? 1 : 3;
        }
    }

    /**
     * @brief Activates the next propeller hologram.
     *
     * Sets the next propeller as active, updates display state, and logs activation.
     */
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

    /**
     * @brief Hides the currently active propeller.
     *
     * Deactivates the last active propeller and hides the guidance arrow.
     */
    public void HideCurrentCopter()
    {
        if (currentCopterIndex > 0)
        {
            cylinders[currentCopterIndex - 1].SetActive(false);
            isDisplaying = false;
        }
        arrow.SetActive(false);
    }

    /**
     * @brief Completes the mounting step.
     *
     * Marks the mounting process as finished and updates QR code displays.
     */
    public void FinishMountingStep()
    {
        if (isMounting)
        {
            isMounting = false;
            UpdateQRCodeDisplay();
            Debug.Log("Mounting step completed.");
        }
    }

    /**
     * @brief Updates the QR code displays.
     *
     * Iterates through all SecondQRCodeTransform objects to refresh their state.
     */
    private void UpdateQRCodeDisplay()
    {
        foreach (SecondQRCodeTransform qrTransform in FindObjectsOfType<SecondQRCodeTransform>())
        {
            qrTransform.UpdateQRCodeDisplay();
        }
    }

    /**
     * @brief Checks the position of a secondary QR code relative to the current propeller.
     *
     * Evaluates if the secondary QR code is close to the current propeller and updates its indicator color.
     *
     * @param secondQRPosition The position of the secondary QR code.
     * @param qrTransform Reference to the SecondQRCodeTransform component.
     */
    public void CheckSecondQRCodePosition(Vector3 secondQRPosition, SecondQRCodeTransform qrTransform)
    {
        bool isVeryClose = false;
        bool isKindOfNear = false;

        if (currentCopterIndex > 0)
        {
            GameObject currentCylinder = cylinders[currentCopterIndex - 1];
            Vector3 localCylinderPosition = ARcamera.transform.InverseTransformPoint(currentCylinder.transform.position);
            Vector3 localQRPosition = ARcamera.transform.InverseTransformPoint(secondQRPosition);

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

    /**
     * @brief Updates the guidance arrow's position and rotation.
     *
     * Positions the arrow relative to the current propeller and orients it toward the target.
     *
     * @param copterPosition The position of the active propeller.
     */
    private void UpdateArrowTransform(Vector3 copterPosition)
    {
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

        Vector3 directionToCopter = copterPosition - arrow.transform.position;
        directionToCopter.z = 0f;
        float angle = Mathf.Atan2(directionToCopter.y, directionToCopter.x) * Mathf.Rad2Deg + 180f;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        arrow.transform.rotation = targetRotation;
        arrow.SetActive(true);
    }

    /**
     * @brief Updates the plug color based on the current propeller index.
     *
     * Delegates the color update to the MainQR instance.
     */
    public void ChangePlugColor()
    {
        mainQR.ShowPlug(currentCopterIndex);
    }
}
