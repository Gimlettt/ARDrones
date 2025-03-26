/**
 * @file PropellerManager.cs
 * @brief Handles the display and positioning of propeller holograms.
 *
 * This class instantiates propeller holograms, updates their positions relative to a target,
 * and manages the guidance arrow for proper placement.
 */

using UnityEngine;

public class Propellermanager : MonoBehaviour
{
    /// <summary>
    /// Prefab for the red propeller hologram.
    /// </summary>
    public GameObject propPrefab;
    /// <summary>
    /// Prefab for the black propeller hologram.
    /// </summary>
    public GameObject propPrefab_black;
    /// <summary>
    /// Prefab for the guidance arrow.
    /// </summary>
    public GameObject arrowPrefab;
    private GameObject arrow;

    private GameObject[] props;
    private int count;

    /// <summary>
    /// Reference to the MainQR script.
    /// </summary>
    public MainQR mainQR;
    /// <summary>
    /// Reference to the AR camera.
    /// </summary>
    public Camera ARcamera;

    /// <summary>
    /// Indicates if the mounting process is active.
    /// </summary>
    public bool isMounting { get; private set; } = false;
    /// <summary>
    /// Index of the currently active propeller.
    /// </summary>
    public int currentCopterIndex { get; private set; } = 0;
    private bool isSymmetric;
    /// <summary>
    /// Flag indicating if a propeller is currently displayed.
    /// </summary>
    public bool isDisplaying = false;

    // Configuration arrays for positions and rotations
    private Vector3[][] relativePositions = new Vector3[4][];
    private Vector3[][] relativeRotations = new Vector3[4][];

    private Vector3 boxPosition;

    // Configuration parameters for positioning
    private float width = 0.1482f;
    private float height = -0.1009f;
    private float length_neg = -0.3505f;
    private float length_pos = 0.3455f;

    /**
     * @brief Initializes configurations and instantiates the guidance arrow.
     *
     * Called on startup to set up propeller configurations.
     */
    void Start()
    {
        InitializeConfigurations();
        arrow = Instantiate(arrowPrefab);
        arrow.SetActive(false);
    }

    /**
     * @brief Initializes the configuration arrays for propeller positions and rotations.
     *
     * Sets up four different configuration layouts.
     */
    private void InitializeConfigurations()
    {
        // Configuration 1: 4 propellers, symmetric layout
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

        // Configuration 2: 4 propellers, asymmetric layout
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

        // Configuration 3: 6 propellers, symmetric layout
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

        // Configuration 4: 6 propellers, asymmetric layout
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
     * @brief Configures the propeller layout.
     *
     * Sets the symmetric flag and number of propellers, and instantiates them.
     *
     * @param symmetric True for symmetric layout.
     * @param numberOfCopters Number of propellers.
     */
    public void SetConfiguration(bool symmetric, int numberOfCopters)
    {
        isSymmetric = symmetric;
        count = numberOfCopters;
        InstantiateCylinders();
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
     * @brief Instantiates propeller holograms.
     *
     * Creates the propeller objects and initializes them to inactive.
     */
    private void InstantiateCylinders()
    {
        props = new GameObject[count];
        for (int i = 0; i < props.Length; i++)
        {
            if (i % 2 == 0)
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

    /**
     * @brief Updates positions and rotations of propeller holograms.
     *
     * Positions each propeller around the box based on the given QR code transform.
     *
     * @param qrPosition The position of the main QR code.
     * @param qrRotation The rotation of the main QR code.
     */
    public void UpdateCylinderPositions(Vector3 qrPosition, Quaternion qrRotation)
    {
        boxPosition = qrPosition;
        int configIndex = GetConfigurationIndex();

        for (int i = 0; i < count; i++)
        {
            if (isDisplaying && i <= currentCopterIndex - 1)
            {
                props[i].transform.position = boxPosition + qrRotation * relativePositions[configIndex][i];
                props[i].transform.rotation = qrRotation * Quaternion.Euler(relativeRotations[configIndex][i]);
                props[i].SetActive(true);
                UpdateArrowTransform(props[i].transform.position);
            }
            else
            {
                props[i].transform.position = boxPosition + qrRotation * relativePositions[configIndex][i];
                props[i].transform.rotation = qrRotation * Quaternion.Euler(relativeRotations[configIndex][i]);
                props[i].SetActive(false);
            }
        }
    }

    /**
     * @brief Activates the next propeller hologram.
     *
     * Sets the next propeller as active, updates the guidance arrow, and logs the activation.
     */
    public void ActivateNextCopter()
    {
        if (currentCopterIndex < count)
        {
            props[currentCopterIndex].SetActive(true);
            isDisplaying = true;
            isMounting = true;
            UpdateQRCodeDisplay();
            Debug.Log($"Copter {currentCopterIndex} activated at position: {props[currentCopterIndex].transform.position}");
            UpdateArrowTransform(props[currentCopterIndex].transform.position);
            currentCopterIndex++;
        }
        else
        {
            Debug.LogError("All copters have been activated.");
        }
        ChangePlugColor();
    }

    /**
     * @brief Hides the currently active propeller hologram.
     *
     * Deactivates the last active propeller and hides the guidance arrow.
     */
    public void HideCurrentCopter()
    {
        if (currentCopterIndex > 0)
        {
            props[currentCopterIndex - 1].SetActive(false);
            isDisplaying = false;
        }
        arrow.SetActive(false);
    }

    /**
     * @brief Completes the current mounting step.
     *
     * Marks the mounting process as complete and updates QR code displays.
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
     * @brief Updates the display of QR code indicators.
     *
     * Calls the update method on all SecondQRCodeTransform instances.
     */
    private void UpdateQRCodeDisplay()
    {
        foreach (SecondQRCodeTransform qrTransform in FindObjectsOfType<SecondQRCodeTransform>())
        {
            qrTransform.UpdateQRCodeDisplay();
        }
    }

    /**
     * @brief Checks if the secondary QR code is near the current propeller.
     *
     * Evaluates the proximity and updates the indicator color.
     *
     * @param secondQRPosition The position of the secondary QR code.
     * @param qrTransform Reference to the SecondQRCodeTransform.
     */
    public void CheckSecondQRCodePosition(Vector3 secondQRPosition, SecondQRCodeTransform qrTransform)
    {
        bool isVeryClose = false;
        bool isKindOfNear = false;

        if (currentCopterIndex > 0)
        {
            GameObject currentCylinder = props[currentCopterIndex - 1];
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
     * Delegates the plug color update to the MainQR instance.
     */
    public void ChangePlugColor()
    {
        mainQR.ShowPlug(currentCopterIndex);
    }
}
