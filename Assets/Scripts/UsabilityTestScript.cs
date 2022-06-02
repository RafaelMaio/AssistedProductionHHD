// ===============================
// AUTHOR     : Rafael Maio (rafael.maio@ua.pt)
// PURPOSE     : Handles the behaviour for usability tests (IEETA) purposes.
// ===============================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class UsabilityTestScript : MonoBehaviour
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
    /// Usability test start countage for total test time.
    /// </summary>
    private DateTime total_date;

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
        total_date = DateTime.Now;
        usabilityEnabled = true;
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
    /// <param name="destroyed">All pieces of that type were fetched.</param>
    /// <param name="peca">Piece number.</param>
    public void breakTime(int peca, bool destroyed)
    {
        TimeSpan time = DateTime.Now - date;
        string pecaId = peca.ToString();
        using (StreamReader sr = new StreamReader(filePathName))
        {
            int count = 0;
            string line;
            // Read and display lines from the file until the end of
            // the file is reached.
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Split(';')[0].Contains("_"))
                {
                    if (line.Split(';')[0].Split('_')[0].Equals(pecaId))
                    {
                        count += 1;
                    }
                }
                else
                {
                    if (line.Split(';')[0].Equals(pecaId))
                    {
                        count += 1;
                    }
                }
            }
            if (count > 0)
            {
                pecaId = peca.ToString() + "_" + (count + 1).ToString();
            }
            else if (!(count > 0) && !destroyed)
            {
                pecaId += "_1";
            }
        }
        using (StreamWriter sw = File.AppendText(filePathName))
        {
            sw.WriteLine(pecaId + ";" + time.ToString());
        }
        setTime();
    }

    /// <summary>
    /// Stop time, experience over.
    /// </summary>
    public void stopTime()
    {
        TimeSpan time = DateTime.Now - total_date;
        using (StreamWriter sw = File.AppendText(filePathName))
        {
            sw.WriteLine("Total;" + time.ToString());
        }
    }

    /// <summary>
    /// Set the initial time.
    /// </summary>
    public void setTime()
    {
        date = DateTime.Now;
    }
}