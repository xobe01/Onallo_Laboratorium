using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

public class NeuralNetCont : MonoBehaviour
{
    [SerializeField] GameObject tracker;

    Renderer trackerRend;

    void Start()
    {
        trackerRend = tracker.GetComponent<Renderer>();
        RunNeuralNet();
    }

    void RunNeuralNet()
	{
        string fileName = Application.dataPath + "/neur/network.py";
        string imageName = "5.jpg";

        Process p = new Process();
        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = "C:/Users/ungbo/anaconda3/envs/tensorflow_gpu";
        psi.Arguments = $"\"{fileName}\" \"{imageName}\"";
        psi.RedirectStandardError = true;
        psi.RedirectStandardOutput = true;
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;

        var result = "";
        var error = "";

        using (var process = Process.Start(psi))
        {
            error = process.StandardError.ReadToEnd();
            result = process.StandardOutput.ReadToEnd();
        }

        print(error);
        print(result);
        if (result != "")
        {
            PlaceProjection(result);
        }
    }

    void PlaceProjection(string result)
	{
        List<int> returnValues = new List<int>();
        string helper = "";
        foreach(char c in result)
		{
            if(c == ';')
			{
                returnValues.Add(int.Parse(helper));
                helper = "";
			}
			else
			{
                helper += c;
			}
		}
        int xMin = returnValues[0];
        int xMax = returnValues[1];
        int yMin = returnValues[2];
        int yMax = returnValues[3];
        int xRot = returnValues[4];
        int yRot = returnValues[6];
        int zRot = returnValues[5];

        tracker.transform.localRotation = Quaternion.Euler(xRot, yRot, zRot);
        tracker.transform.parent.position = new Vector3((xMax + xMin) / 2, (yMax + yMin) / 2, tracker.transform.parent.position.z);
        tracker.transform.localScale = Vector3.one * ((xMax - xMin) / (trackerRend.bounds.extents.x * 2));
    }
}
   
