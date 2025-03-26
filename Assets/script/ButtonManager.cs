/**
 * @file ButtonManager.cs
 * @brief Manages UI button functionality for freeze/unfreeze and adjust operations.
 *
 * This class handles the UI interactions for freezing and adjusting elements in the scene.
 */

using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    /// <summary>
    /// Reference to the TextMeshPro component on the freeze button.
    /// </summary>
    public TextMeshPro freezeButtonText;

    /// <summary>
    /// Reference to the TextMeshPro component on the adjust button (used in the training scene).
    /// </summary>
    public TextMeshPro adjustButtonText;

    private bool Frozen = false;   ///< Initial freeze state.
    private bool Locked = true;    ///< Initial adjust state.

    /**
     * @brief Initializes button texts.
     *
     * Called on startup to set default texts for the freeze and adjust buttons.
     */
    void Start()
    {
        freezeButtonText.text = "Freeze!";
        adjustButtonText.text = "Click to adjust ";
    }

    /**
     * @brief Handles the freeze button click event.
     *
     * Toggles the freeze state and updates the button text accordingly.
     */
    public void OnFreezeButtonClick()
    {
        Frozen = !Frozen;
        if (Frozen)
        {
            freezeButtonText.text = "Unfreeze!";
        }
        else
        {
            freezeButtonText.text = "Freeze!";
        }
    }

    /**
     * @brief Handles the adjust button click event.
     *
     * Toggles the adjust state and updates the button text accordingly.
     */
    public void OnAdjustButtonClick()
    {
        Locked = !Locked;
        if (Locked)
        { 
            adjustButtonText.text = "Click to adjust";
        }
        else
        {
            adjustButtonText.text = "Adjusting";
        }
    }
}
