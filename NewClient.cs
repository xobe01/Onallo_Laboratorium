using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
using System;
using System.Text;

public class NewClient : MonoBehaviour
{

    private bool connected = false;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;
    string IP = "192.168.0.18";
    int port = 8888;
    TrackerController trackerCont;

    int bytesReceived;
    string receivedMessage;
    byte[] buffer = new byte[20000];

    public void StartClient()
    {
        trackerCont = FindObjectOfType<TrackerController>();
        ConnectToServer();
    }

    public void ConnectToServer()
    {
        if (connected) 
            return;

        try
        {
            socket = new TcpClient(IP, port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);
            connected = true;
            Debug.Log("Connected to: " + IP + ":" + port.ToString());
        }
        catch (Exception e)
        {
            Debug.Log("Socket error: " + e.Message);
        }
    }

    private void onIncomingData(string data)
    {
        if (data != "null") trackerCont.PlaceProjection(data);
        else trackerCont.SetProjectionEnabled(false);
    }

    private void Update()
    {
        if (!connected)
        {
            return;
        }
        if (stream.DataAvailable)
        {
                
            stream.BeginRead(buffer, 0, buffer.Length, MessageReceived, null);

            if (bytesReceived > 0)
            {
                onIncomingData(receivedMessage);
                bytesReceived = 0;
            }
        }
    }

    private void MessageReceived(IAsyncResult result)
    {
        if (result.IsCompleted && socket.Connected)
        {
            bytesReceived = stream.EndRead(result);
            receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
        }
    }

    public void Send(byte[] data)
    {
        if (!connected)
            return;
        stream.Write(data, 0, data.Length);
    }    
}
