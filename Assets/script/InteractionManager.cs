using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections.Generic;
using System.IO;
using System;

public class InteractionManager : MonoBehaviour
    //this class is for handling the logic of the app
{
    public GameObject chooseSettingSymmetricDialog;
    public GameObject chooseSettingNumberDialog;

    public TextMeshPro progressTextSlate;//reference to the window that shows the instruction

    public GameObject startbutton;
    public GameObject toggleFreezeButton;
    public GameObject slightAdjustmentButton;//this button is not used in the app, is for my training scene
    public GameObject confirmCopterButton;//when one propeller is mounted, user press this button
    public GameObject adjustmentFinishButton;//this is after user freeze the box's hologram,user press the Next button
    public GameObject confirmCopterIndexButton;//this is for user confirm they are grabbing the correct propeller
    public GameObject Cable_button;//this is for user to confirm the plug is plugged in

    private int currentStep = 0;
    private int coptersToMount;//overall how many propellers to mount
    private bool isSymmetric;//decide for configuration

    public PropellerManager propellerManager; //reference to the propellerManager

    private string logPath;// the path to save the time log
    private float startTime;//this is to log the time user uses to mount one propeller
    private float overallStartTime;//to log the time for the whole process
    private List<float> copterMountTimes = new List<float>();//each time the propeller is mounted,log to a list the time

    void Start()
    {
        string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", System.Globalization.CultureInfo.InvariantCulture);//to find a ti
        logPath = Application.persistentDataPath + $"/copterMountTimes_{timestamp}.txt";
        ShowChooseSettingSymmetricDialog();//the first dialog, to choose configuration
    }

    void ShowChooseSettingSymmetricDialog()
    {
        LogAction("ShowChooseSettingSymmetricDialog");//take down the time when press the button
        chooseSettingSymmetricDialog.SetActive(true);
        progressTextSlate.text = "Choose Symmetric or Unsymmetric";
    }

    public void OnSymmetricSelected()//if choose the symmetric configuration, note this step is done by me not the user, so all the other buttons are hanging around
    {
        LogAction("OnSymmetricSelected");
        isSymmetric = true;
        chooseSettingSymmetricDialog.SetActive(false);
        ShowChooseSettingNumberDialog();
    }

    public void OnUnsymmetricSelected()
    {
        LogAction("OnUnsymmetricSelected");
        isSymmetric = false;
        chooseSettingSymmetricDialog.SetActive(false);
        ShowChooseSettingNumberDialog();
    }

    void ShowChooseSettingNumberDialog()
    {
        LogAction("ShowChooseSettingNumberDialog");
        chooseSettingNumberDialog.SetActive(true);
        progressTextSlate.text = "Choose Number of Propellers";
    }

    public void OnNumberSelected(int numberOfCopters)//choose the number configuration
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

    public void Show_Start_button()//user start from this step
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

    public void On_start_clicked()
    {
        LogAction("On_start_clicked");
        progressTextSlate.text = "Look at the QR Code and Click the freeze button.\n" +
                                 "Move the box to align (High precision not required)\n" +
                                 "Under unfreeze, the hologram track the QR code. Under freeze\n" +
                                 "the hologram will remain its position. You can choose between";
        startbutton.SetActive(false);
        toggleFreezeButton.SetActive(true);
        adjustmentFinishButton.SetActive(true);//the next button in the scene

        // Start overall timer
        overallStartTime = Time.time;
    }

    public void OnAdjustmentConfirmed()//the next button is attached with this funtion
    {
        LogAction("OnAdjustmentConfirmed");
        adjustmentFinishButton.SetActive(false);
        AskGrabCopter();
    }

    void AskGrabCopter()//ask user to grab a propeller
    {
        LogAction("AskGrabCopter");
        progressTextSlate.text = $"Check the number on the propeller and find number  {currentStep + 1}\n"+
                                  "Grab it and press confirm number button";
        confirmCopterIndexButton.SetActive(true);
        // Start individual copter timer
        startTime = Time.time;

        //check if the relavant small QR has been ticked green, if so directly call OnconfirmNumber.This can be implement here, if user scan the small QR and
        //if there is right tick, can direclty call OnconfirmNumber(). But I haven't implement it. It's optional.
    }

    public void OnconfirmNumber()//the confirmCopterIndexButton is attached with this function
    {
        LogAction("OnconfirmNumber");
        confirmCopterIndexButton.SetActive(false);
        confirmCopterButton.SetActive(true);

        progressTextSlate.text = "SLIDE the propeller to match the holograms.\n" +
                                  "If the small QR code is scanned, the ball will turn green at correct position.\n"+
                                  $"Then press Confirm Propeller.                  {currentStep + 1}/{coptersToMount}.";

        // Activate the next propeller's hologram after updating the UI
        propellerManager.ActivateNextCopter();

        
    }

    public void OnMountCopterButtonClicked()//the fonfirmCopterButton is attached with this function
    {
        LogAction("OnMountCopterButtonClicked");
        currentStep++;
        progressTextSlate.text = "Insert the cable of propeller to the plug\n"+
                                 "it is an orange circle on the hologram, flip the glasses to insert";
        Cable_button.SetActive(true);
        confirmCopterButton.SetActive(false);
    }

    public void OnCableMounted()//this si attached to the Cable_button
    {
        LogAction("OnCableMounted");
        float copterMountTime = Time.time - startTime;// individual propeller mount time is logged
        copterMountTimes.Add(copterMountTime);//save to a list
        propellerManager.FinishMountingStep();//inform the propeller Manager the mounting is finish, toggle the isMounting flag
        if (currentStep < coptersToMount)
        {
            AskGrabCopter();
            
        }
        else
        {
            progressTextSlate.text = "All Propellers Mounted!";
            
            // Stop overall timer and log times
            float overallTime = Time.time - overallStartTime;
            LogTimes(overallTime);
        }
        Cable_button.SetActive(false);
    }

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

    private void LogAction(string action)
    {
        using (StreamWriter writer = new StreamWriter(logPath, true))
        {
            // Get the current time in UTC with the full date, time, and milliseconds
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);

            // Write the timestamp and action to the log file
            writer.WriteLine($"{timestamp} - {action}");
        }
        Debug.Log(action + " logged to " + logPath);
    }
}
