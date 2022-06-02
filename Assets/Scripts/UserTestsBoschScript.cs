// ===============================
// AUTHOR     : Rafael Maio (rafael.maio@ua.pt)
// PURPOSE     : Handles the behaviour for usability tests (at Bosch Aveiro) purposes.
// ===============================

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UserTestsBoschScript : MonoBehaviour
{
    /// <summary>
    /// For verifying if the usability tests option is enabled.
    /// </summary>
    private bool usabilityEnabled = false;

    /// <summary>
    /// Participant identifier.
    /// </summary>
    private int participantId = 0;

    /// <summary>
    /// File path for saving the usability tests results.
    /// </summary>
    private string filePathName;

    /// <summary>
    /// Usability test time countage.
    /// </summary>
    private DateTime date;

    /// <summary>
    /// Count the kit number to be assembled.
    /// </summary>
    private int kitCount = 0;

    /// <summary>
    /// Kit identifier - 0, 1, 2 or 3
    /// </summary>
    private int kit_id= -1;

    /// <summary>
    /// Enable the usability test.
    /// Get the participant identifier.
    /// </summary>
    public void enableTest()
    {
        int count_number_of_participants = 0;
        DirectoryInfo d = new DirectoryInfo(Application.persistentDataPath);
        foreach (var file in d.GetFiles("results_user*.txt"))
        {
            count_number_of_participants += 1;
        }
        participantId = count_number_of_participants + 1;
        filePathName = Application.persistentDataPath + "/results_user" + participantId.ToString() + ".txt";
        File.Create(Application.persistentDataPath + "/results_user" + participantId.ToString() + ".txt");
        date = DateTime.Now;
        usabilityEnabled = true;
    }

    /// <summary>
    /// Returns kitCount.
    /// </summary>
    /// <returns></returns>
    public int getKitCount()
    {
        return kitCount;
    }

    /// <summary>
    /// One more kit being assembled.
    /// </summary>
    public void increaseKitCount()
    {
        kitCount += 1;
    }

    /// <summary>
    /// Set the initial time.
    /// </summary>
    public void setTime()
    {
        date = DateTime.Now;
    }

    /// <summary>
    /// Verify if the usability tests are enabled.
    /// </summary>
    public bool getUsabilityEnabled()
    {
        return usabilityEnabled;
    }

    /// <summary>
    /// Stop the timer when a piece is caught.
    /// </summary>
    /// <param name="num_pieces">Number of pieces that the kit contains.</param>
    public void breakTime(int num_pieces)
    {
        TimeSpan time = DateTime.Now - date;
        using (StreamWriter sw = File.AppendText(filePathName))
        {
            sw.WriteLine("Kit" + kit_id.ToString() + ";" + "TempoTotal-"+ time.TotalSeconds.ToString()  + "s;TempoMedPeca-" + ((float)time.TotalSeconds/(float)num_pieces).ToString() + "s\n");
        }
    }

    /// <summary>
    /// Set the identifier for the current kit.
    /// </summary>
    /// <param name="id">Current kit identifier.</param>
    public void setKitId(int id)
    {
        kit_id = id;
    }
}