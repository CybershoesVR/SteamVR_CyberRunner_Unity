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
        start.FileName = "C:\\Users\\Cybershoes1\\Desktop\\Nanda Cyberrun Game\\EventLeaderboard\\EventForm\\EventForm\\bin\\Debug\\EventForm.exe";

        Process playerForm = Process.Start(start);

        playerForm.WaitForExit();

        return playerForm.ExitCode;
   }
}
