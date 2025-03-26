using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    //this class is responsible for managing the freeze/unfreeze button in the scene
    public TextMeshPro freezeButtonText; // Reference to the TextMeshPro text component on the freeze button
    public TextMeshPro adjustButtonText; // Reference to the TextMeshPro text component on the adjust button,this is only used in my training scene


    private bool Frozen = false; // Initial state for the freeze button
    private bool Locked = true; // Initial state for the adjust button

    void Start()
    {
        // Initialize button texts
        freezeButtonText.text = "Freeze!";
        adjustButtonText.text = "Click to adjust ";
    }

    // Method to be called when the freeze button is clicked
    public void OnFreezeButtonClick()
    {
        // Toggle the state
        Frozen = !Frozen;

        // Update the button text based on the state
        if (Frozen)
        {
            freezeButtonText.text = "Unfreeze!";
        }
        else
        {
            freezeButtonText.text = "Freeze!";
        }


    }

    public void OnAdjustButtonClick()
    {
        // Toggle the state
        Locked = !Locked;
        if (Locked) { 
            adjustButtonText.text = "Click to adjust";
        }
        else
        {
            adjustButtonText.text = "Adjusting";
        }

    }
}
