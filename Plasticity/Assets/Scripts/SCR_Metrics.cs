using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.Events;
// This class encapsulates all of the metrics that need to be tracked in your game. These may range
// from number of deaths, number of times the player uses a particular mechanic, or the total time
// spent in a level. These are unique to your game and need to be tailored specifically to the data
// you would like to collect. The examples below are just meant to illustrate one way to interact
// with this script and save data.
public class SCR_Metrics : MonoBehaviour
{
    // You'll have more interesting metrics, and they will be better named.
    private int TimesFlippedMetric;
    private float LengthOfPuzzle;
    [SerializeField]
    private string PlaytestIdentifier;
    private UnityAction<float> GameTimerListener;
    private UnityAction<int> LadderFlippedListener;



    private void Awake()
    {
        GameTimerListener = new UnityAction<float>(GameTimerDone);
        LadderFlippedListener = new UnityAction<int>(LadderAction);
    }

    private void OnEnable()
    {
        SCR_EventManager.StartListening("GameTimerResult", GameTimerListener);
        SCR_EventManager.StartListening("LadderFlipped", LadderFlippedListener);
    }

    private void OnDisable()
    {
        SCR_EventManager.StopListening("GameTimerResult", GameTimerListener);
        SCR_EventManager.StopListening("LadderFlipped", LadderFlippedListener);
    }

    private void GameTimerDone(float Input)
    {
        AddToMetric2(Input);
    }

    private void LadderAction(int value)
    {
        TimesFlippedMetric = value;
    }

    // Public method to add to Metric 1.
    private void AddToMetric1(int valueToAdd)
    {
        TimesFlippedMetric += valueToAdd;
    }

    // Public method to add to Metric 2.
    private void AddToMetric2(float valueToAdd)
    {
        LengthOfPuzzle += valueToAdd;
    }


    // Converts all metrics tracked in this script to their string representation
    // so they look correct when printing to a file.
    private string ConvertMetricsToStringRepresentation()
    {
        string metrics = "Here are my metrics:\n";
        metrics += "The ladder was flipped " + TimesFlippedMetric.ToString() + " times\n";
        if (LengthOfPuzzle != 0.0f) metrics += "This is how long it took to do the first puzzle: " + LengthOfPuzzle.ToString() + " seconds\n";
        else metrics += "The player did not complete the first puzzle";
        return metrics;
    }

    // Uses the current date/time on this computer to create a uniquely named file,
    // preventing files from colliding and overwriting data.
    private string CreateUniqueFileName()
    {
        string dateTime = System.DateTime.Now.ToString();
        dateTime = dateTime.Replace("/", "_");
        dateTime = dateTime.Replace(":", "_");
        dateTime = dateTime.Replace(" ", "___");
        return "Metrics/" + PlaytestIdentifier + "_" + dateTime + "_metrics_.txt";
    }

    // Generate the report that will be saved out to a file.
    private void WriteMetricsToFile()
    {
        string totalReport = "Report generated on " + System.DateTime.Now + "\n\n";
        totalReport += "Total Report:\n";
        totalReport += ConvertMetricsToStringRepresentation();
        totalReport = totalReport.Replace("\n", System.Environment.NewLine);
        string reportFile = CreateUniqueFileName();

#if !UNITY_WEBPLAYER
        File.WriteAllText(reportFile, totalReport);
#endif
    }

    // The OnApplicationQuit function is a Unity-Specific function that gets
    // called right before your application actually exits. You can use this
    // to save information for the next time the game starts, or in our case
    // write the metrics out to a file.
    private void OnApplicationQuit()
    {
        WriteMetricsToFile();
    }
}
