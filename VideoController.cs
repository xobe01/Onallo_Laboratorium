using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System;

public class VideoController : MonoBehaviour
{
    [SerializeField] Renderer renderer;
    WebCamTexture webcam;
    //private string savePath;

    void Awake()
    {
        //renderer.transform.localScale = new Vector3(Screen.width / 12, 1, Screen.height / 12);
        WebCamDevice[] devices = WebCamTexture.devices;  //List all devices
        /*for (var i = 0; i < devices.Length; i++)
            Debug.Log(devices[i].name);*/

        /*savePath = Application.dataPath + "/SavedPictures/";
        string[] filePaths = Directory.GetFiles(savePath);
        foreach (string filePath in filePaths)
          //  File.Delete(filePath);*/
        webcam = new WebCamTexture(devices[0].name);
        renderer.material.mainTexture = webcam;
        webcam.Play();
        StartRecording();
    }

    public void StartRecording()
    {
        StartCoroutine(TakeSnapshot());
    }

    IEnumerator TakeSnapshot()
    {
        int webCamHeight = webcam.height;
        int webCamWidth = webcam.width;
        //int captureCounter = 0;
        
        /*while (false)
        {
            snap.SetPixels(webcam.GetPixels());
            snap.Apply();

            System.IO.File.WriteAllBytes(savePath + captureCounter.ToString() + ".jpg", snap.EncodeToJPG());
            captureCounter++;
            if (captureCounter >= 20) File.Delete(savePath + (captureCounter - 20).ToString() + ".jpg");
            yield return new WaitForSeconds(0.05f);
        }*/
        NewClient client = FindObjectOfType<NewClient>();
        client.StartClient();
        yield return new WaitForSeconds(1);
        while (true)
        {
            Texture2D snap = new Texture2D(webCamWidth, webCamHeight);
            yield return new WaitForSeconds(0.1f);
            snap.SetPixels(webcam.GetPixels());
            snap = ScaleTexture(snap, 300, 300);
            snap.Apply();
            byte[] bytes = snap.EncodeToJPG();
            client.Send(bytes);
            Destroy(snap);
        }        
    }

    private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
        Color[] rpixels = result.GetPixels(0);
        float incX = (1.0f / (float)targetWidth);
        float incY = (1.0f / (float)targetHeight);
        for (int px = 0; px < rpixels.Length; px++)
        {
            rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth), incY * ((float)Mathf.Floor(px / targetWidth)));
        }
        result.SetPixels(rpixels, 0);
        result.Apply();
        return result;
    }
}
