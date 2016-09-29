using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;
using System.IO.Ports;
using Assets.LSL4Unity.Scripts;

/// <summary>
/// This class is inspired by http://www.alanzucconi.com/2015/10/07/how-to-integrate-arduino-with-unity/
/// </summary>
public class ArduinoController : MonoBehaviour
{
    /// <summary>
    /// An UnityEvent implementation as a custom event containing informations on what happens on the arduino
    /// </summary>
    public ArduinoSignalEvent WhenArduinoHasAResult;

    public int BaudRate = 9600;
    public string ComId = "COM3"; // default for arduino uno

    public int readingTimeout = 50;

    private SerialPort stream;

    private LSLMarkerStream marker;

    void Start()
    {
        marker = GetComponent<LSLMarkerStream>();

        try
        {
            stream = new SerialPort(ComId, BaudRate);
        }
        catch (Exception)
        {
            stream = null;
            Debug.Log("Can't communicate over Port: " + ComId);
        }

        stream.ReadTimeout = readingTimeout;
        stream.Open();

        StartCoroutine(
            AsynchronousReadFromArduino(
                // lambda expression - anonymous function
                (incomingString) =>
                {
                    marker.Write("Recieve response from Arduino { " + incomingString + " }");

                    if (WhenArduinoHasAResult.GetPersistentEventCount() > 0)
                    {
                        ArduinoEvent evt = Parse(incomingString);

                        WhenArduinoHasAResult.Invoke(evt);
                    }
                }
            ));

    }

    private ArduinoEvent Parse(string incomingString)
    {
        //Debug.Log("Recieved from Arduino: " + incomingString);

        ArduinoEvent evt = new ArduinoEvent();

        if(!double.TryParse(incomingString, out evt.reactionTime))
        {
            evt.reactionTime = -1;    
        }

        return evt;
    }

    public void AwaitButtonPress()
    {
        if (stream == null || !stream.IsOpen) { 
            Debug.LogError("Connection to Arduino not available... Discard Command");
            return;
        }

        marker.Write("Enable Response on Arduino");

        stream.WriteLine("Await");
        stream.BaseStream.Flush();
    }

    public void ResetArduino()
    {
        if (stream == null)
        {
            Debug.LogError("Connection to Arduino not available... Discard Command");
            return;
        }

        marker.Write("Reset To Arduino");

        stream.WriteLine("Reset");
        stream.BaseStream.Flush();
    }

    public IEnumerator AsynchronousReadFromArduino(Action<string> callback, Action fail = null, float timeout = float.PositiveInfinity)
    {
        var initialTime = Time.realtimeSinceStartup;
        var timeAsyncReadIsRunning = 0f;

        string dataString = null;

        do
        {
            try
            {
                dataString = stream.ReadLine();
            }
            catch (TimeoutException)
            {
                dataString = null;
            }

            if (dataString != null)
            {
                callback(dataString);
                yield return null;
            }
            else
                yield return new WaitForSeconds(0.05f);

            var nowTime = Time.realtimeSinceStartup;
            timeAsyncReadIsRunning = nowTime - initialTime;

        } while (timeAsyncReadIsRunning < timeout);

        if (fail != null)
            fail();

        yield return null;
    }

}

#region Implementing a custom UnityEvent

[Serializable]
public class ArduinoEvent
{
    public double reactionTime;
    public string result;
}

[Serializable]
public class ArduinoSignalEvent : UnityEvent<ArduinoEvent>
{
    // it's empty - it's intented!
}

#endregion