/**
 * @file InteractionManager.cs
 * @brief Manages user interactions for the AR propeller mounting experiment.
 *
 * This class handles dialogs, UI updates, and action logging during the propeller mounting process.
 */

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections.Generic;
using System.IO;
using System;

public class InteractionManager : MonoBehaviour
{
    /// <summary>
    /// Dialog for choosing symmetric or unsymmetric setting.
    /// </summary>
    public GameObject chooseSettingSymmetricDialog;
    /// <summary>
    /// Dialog for choosing the number of propellers.
    /// </summary>
    public GameObject chooseSettingNumberDialog;

    /// <summary>
    /// UI element displaying progress or instructions.
    /// </summary>
    public TextMeshPro progressTextSlate;

    public GameObject startbutton;
    public GameObject toggleFreezeButton;
    public GameObject slightAdjustmentButton;
    public GameObject confirmCopterButton;
    public GameObject adjustmentFinishButton;
    public GameObject confirmCopterIndexButton;
    public GameObject Cable_button;

    private int currentStep = 0;
    private int coptersToMount;
    private bool isSymmetric;

    /// <summary>
    /// Reference to the PropellerManager.
    /// </summary>
    public PropellerManager propellerManager;

    private string logPath;
    private float startTime;
    private float overallStartTime;
    private List<float> copterMountTimes = new List<float>();

    /**
     * @brief Initializes the interaction manager.
     *
     * Sets up logging and displays the initial configuration dialog.
     */
    void Start()
    {
        string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", System.Globalization.CultureInfo.InvariantCulture);
        logPath = Application.persistentDataPath + $"/copterMountTimes_{timestamp}.txt";
        ShowChooseSettingSymmetricDialog();
    }

    /**
     * @brief Displays the symmetric/unsymmetric configuration dialog.
     *
     * Logs the action and updates the progress text.
     */
    void ShowChooseSettingSymmetricDialog()
    {
        LogAction("ShowChooseSettingSymmetricDialog");
        chooseSettingSymmetricDialog.SetActive(true);
        progressTextSlate.text = "Choose Symmetric or Unsymmetric";
    }

    /**
     * @brief Handles selection of symmetric configuration.
     *
     * Sets the symmetric flag, hides the dialog, and shows the number selection dialog.
     */
    public void OnSymmetricSelected()
    {
        LogAction("OnSymmetricSelected");
        isSymmetric = true;
        chooseSettingSymmetricDialog.SetActive(false);
        ShowChooseSettingNumberDialog();
    }

    /**
     * @brief Handles selection of unsymmetric configuration.
     *
     * Sets the symmetric flag to false, hides the dialog, and shows the number selection dialog.
     */
    public void OnUnsymmetricSelected()
    {
        LogAction("OnUnsymmetricSelected");
        isSymmetric = false;
        chooseSettingSymmetricDialog.SetActive(false);
        ShowChooseSettingNumberDialog();
    }

    /**
     * @brief Displays the dialog to select the number of propellers.
     *
     * Logs the action and updates the progress text.
     */
    void ShowChooseSettingNumberDialog()
    {
        LogAction("ShowChooseSettingNumberDialog");
        chooseSettingNumberDialog.SetActive(true);
        progressTextSlate.text = "Choose Number of Propellers";
    }

    /**
     * @brief Handles the selection of the number of propellers.
     *
     * Configures the PropellerManager and shows the start button.
     *
     * @param numberOfCopters Number of propellers selected.
     */
    public void OnNumberSelected(int numberOfCopters)
    {
        LogAction("OnNumberSelected: " + numberOfCopters);
        coptersToMount = numberOfCopters;
        if (coptersToMount > 6) {
            coptersToMount = 6;
        }
        chooseSettingNumberDialog.SetActive(false);
        propellerManager.SetConfiguration(isSymmetric, coptersToMount);
        Show_Start_button();
    }

    /**
     * @brief Displays the start button and hides other UI elements.
     *
     * Updates the progress text with instructions.
     */
    public void Show_Start_button()
    {
        LogAction("Show_Start_button");
        progressTextSlate.text = "Press Start to start the experiment";
        startbutton.SetActive(true);
        toggleFreezeButton.SetActive(false);
        slightAdjustmentButton.SetActive(false);
        confirmCopterButton.SetActive(false);
        confirmCopterIndexButton.SetActive(false);
        Cable_button.SetActive(false);
        adjustmentFinishButton.SetActive(false);
    }

    /**
     * @brief Handles the start button click event.
     *
     * Updates instructions, toggles UI elements, and starts the overall experiment timer.
     */
    public void On_start_clicked()
    {
        LogAction("On_start_clicked");
        progressTextSlate.text = "Look at the QR Code and Click the freeze button.\n" +
                                 "Move the box to align (High precision not required)\n" +
                                 "Under unfreeze, the hologram tracks the QR code. Under freeze,\n" +
                                 "the hologram will remain in its position. You can choose between";
        startbutton.SetActive(false);
        toggleFreezeButton.SetActive(true);
        adjustmentFinishButton.SetActive(true);
        overallStartTime = Time.time;
    }

    /**
     * @brief Handles the confirmation of adjustments.
     *
     * Hides the adjustment button and prompts the user to grab a propeller.
     */
    public void OnAdjustmentConfirmed()
    {
        LogAction("OnAdjustmentConfirmed");
        adjustmentFinishButton.SetActive(false);
        AskGrabCopter();
    }

    /**
     * @brief Prompts the user to grab the next propeller.
     *
     * Updates the progress text and starts an individual timer.
     */
    void AskGrabCopter()
    {
        LogAction("AskGrabCopter");
        progressTextSlate.text = $"Check the number on the propeller and find number {currentStep + 1}\n" +
                                  "Grab it and press confirm number button";
        confirmCopterIndexButton.SetActive(true);
        startTime = Time.time;
    }

    /**
     * @brief Handles the confirmation of the propeller index.
     *
     * Updates UI elements and activates the next propeller hologram.
     */
    public void OnconfirmNumber()
    {
        LogAction("OnconfirmNumber");
        confirmCopterIndexButton.SetActive(false);
        confirmCopterButton.SetActive(true);
        progressTextSlate.text = "SLIDE the propeller to match the holograms.\n" +
                                 "If the small QR code is scanned, the ball will turn green at the correct position.\n" +
                                 $"Then press Confirm Propeller.                  {currentStep + 1}/{coptersToMount}.";
        propellerManager.ActivateNextCopter();
    }

    /**
     * @brief Handles the mounting of the propeller.
     *
     * Updates instructions and shows the cable connection UI.
     */
    public void OnMountCopterButtonClicked()
    {
        LogAction("OnMountCopterButtonClicked");
        currentStep++;
        progressTextSlate.text = "Insert the cable of propeller to the plug\n" +
                                 "It is an orange circle on the hologram, flip the glasses to insert";
        Cable_button.SetActive(true);
        confirmCopterButton.SetActive(false);
    }

    /**
     * @brief Handles the cable mounting event.
     *
     * Logs the mounting time, updates the PropellerManager, and either prompts for the next propeller or finalizes the process.
     */
    public void OnCableMounted()
    {
        LogAction("OnCableMounted");
        float copterMountTime = Time.time - startTime;
        copterMountTimes.Add(copterMountTime);
        propellerManager.FinishMountingStep();
        if (currentStep < coptersToMount)
        {
            AskGrabCopter();
        }
        else
        {
            progressTextSlate.text = "All Propellers Mounted!";
            float overallTime = Time.time - overallStartTime;
            LogTimes(overallTime);
        }
        Cable_button.SetActive(false);
    }

    /**
     * @brief Logs the overall and individual propeller mounting times.
     *
     * @param overallTime The total time taken for the experiment.
     */
    private void LogTimes(float overallTime)
    {
        using (StreamWriter writer = new StreamWriter(logPath, true))
        {
            writer.WriteLine("Overall Time: " + overallTime);
            for (int i = 0; i < copterMountTimes.Count; i++)
            {
                writer.WriteLine("Copter " + (i + 1) + " Mount Time: " + copterMountTimes[i]);
            }
        }
        Debug.Log("Times logged to " + logPath);
    }

    /**
     * @brief Logs an action with a timestamp.
     *
     * @param action A description of the action.
     */
    private void LogAction(string action)
    {
        using (StreamWriter writer = new StreamWriter(logPath, true))
        {
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
            writer.WriteLine($"{timestamp} - {action}");
        }
        Debug.Log(action + " logged to " + logPath);
    }
}
