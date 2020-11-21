using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System;

public class NewServer : MonoBehaviour
{
    int port = 8888;
    string ipAdress = "192.168.0.18";
    private List<ServerClient> clients;
    private List<ServerClient> disconnects;
    private TcpListener server;
    private bool started = false;

    private void Start()
    {
        IPAddress IP = IPAddress.Parse(ipAdress);
        clients = new List<ServerClient>();
        disconnects = new List<ServerClient>();

        try
        {
            server = new TcpListener(IP, port);
            server.Start();

            StartListening();
            started = true;
            Debug.Log("Server started on port: " + port.ToString());
        }
        catch (Exception e)
        {
            Debug.Log("Socket error: " + e.Message);
        }
    }

    private void StartListening()
    {
        server.BeginAcceptTcpClient(AcceptTcpClient, server);
    }

    private void AcceptTcpClient(IAsyncResult ar)
    {
        TcpListener listener = (TcpListener)ar.AsyncState;
        clients.Add(new ServerClient(listener.EndAcceptTcpClient(ar)));
        Debug.Log("New connection established");
        StartListening();
    }

    private bool isConnected(TcpClient c)
    {
        try
        {
            if (c != null && c.Client != null && c.Client.Connected)
                return true;
            else
                return false;
        }
        catch
        {
            return false; //not able to reach client
        }

    }

    private void onIncomingData(ServerClient c, string data)
    {
        Debug.Log(c.clientName + "Has sent the following message: " + data);
    }


    private void Update()
    {
        if (!started)
            return;

        foreach (ServerClient c in clients)
        {
            //is the client still connected
            if (!isConnected(c.tcp))
            {
                c.tcp.Close();
                disconnects.Add(c);
                continue;
            }

            //check for message
            NetworkStream s = c.tcp.GetStream();
            if (s.DataAvailable)
            {
                StreamReader reader = new StreamReader(s, true);
                string data = reader.ReadLine();

                if (data != null)
                    onIncomingData(c, data);
            }
        }
    }
}

public class ServerClient
{
    public TcpClient tcp;
    public string clientName;

    public ServerClient(TcpClient clientSocket)
    {
        clientName = "Guest";
        tcp = clientSocket;
    }
}
