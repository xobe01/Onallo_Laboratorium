using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

public class NeuralNetCont : MonoBehaviour
{
    [SerializeField] GameObject tracker;

    TrackerController trackerCont;
    Process process;

    string newValue;

    private void Start()
    {
        newValue = "";
        trackerCont = FindObjectOfType<TrackerController>();
        RunNeuralNet();
    }

    void RunNeuralNet()
    {
        string fileName = "-u " + Application.dataPath + "/neur/network.py";
        process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "C:/Users/ungbo/anaconda3/envs/tensorflow_gpu/python.exe",
                Arguments = fileName,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };
        process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                newValue = e.Data.ToString();
            }
        });
        process.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                print(e.Data.ToString());
            }
        });
        StartCoroutine(NewValueListener());
        Thread th = new Thread(() => StartProcess());
        th.Start();
    }

    void StartProcess()
    {
        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();
        process.WaitForExit();
        Console.Read();
    }

    IEnumerator NewValueListener()
    {
        VideoController videoCont = FindObjectOfType<VideoController>();
        while (newValue != "ready")
        {
            if(newValue!="")
            {
                print(newValue);
                newValue = "";
            }
            yield return null;
        }
        print("Ready");
        newValue = "";
        //videoCont.StartRecording();
        while (true)
        {
            if(newValue != "")
            {
                if (newValue == "null") trackerCont.SetProjectionEnabled(false);
                else trackerCont.PlaceProjection(newValue);                    
                print(newValue);
                newValue = "";
            }
            yield return null;
        }
    }
}
   
