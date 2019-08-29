using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public static class PlayerForm
{
    public static int AddPlayer(string scoreListPath)
   {
        // Prepare the process to run
        ProcessStartInfo start = new ProcessStartInfo();
        // Enter in the command line arguments, everything you would enter after the executable name itself
        start.Arguments = scoreListPath;
        // Enter the executable to run, including the complete path
        start.FileName = $"{Application.streamingAssetsPath}\\PlayerForm\\PlayerForm.exe";

        Process playerForm = Process.Start(start);

        playerForm.WaitForExit();

        return playerForm.ExitCode;
   }
}
