using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Assets;
using Byn.Net;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class WebCom : MonoBehaviour
{
    public enum Comands
    {
        Forward,
        Backward,
        Break
    }

    public string uSignalingUrl = "wss://because-why-not.com:12777/chatapp";

    public string uStunServer = "stun:stun.l.google.com:19302";

    public InputField joinRoomName;

    public static bool mIsServer;

    public Text rotation;

    public static bool isConnceted;

    public static CarDataController carDataController = new CarDataController();

    private IBasicNetwork mNetwork = null;

    private string accessCode;

    private string carData;

    private float gyroDirection;

    private List<ConnectionId> mConnections = new List<ConnectionId>();

    private void Start()
    {
        SendButtonPressed();
        InvokeRepeating("SendButtonPressed", 0.5f, 0.3f);
    }

    private void Setup()
    {

        mNetwork = WebRtcNetworkFactory.Instance.CreateDefault(uSignalingUrl, new string[] { uStunServer });
    }

    private void Reset()
    {
        mIsServer = false;
        isConnceted = false;
        mConnections = new List<ConnectionId>();
        Cleanup();
    }

    public void LeaveButtonPressed()
    {
        Reset();
    }

    private void Cleanup()
    {
        mNetwork.Dispose();
        mNetwork = null;
    }

    private void OnDestroy()
    {
        if (mNetwork != null)
        {
            Cleanup();
        }
    }

    private void FixedUpdate()
    {

        gyroDirection = Input.acceleration.x;
        gyroDirection = Mathf.Clamp(gyroDirection, -1, 1);
        Debug.Log(gyroDirection);
        carDataController.gyro = gyroDirection;
        HandleNetwork();
    }

    private void HandleNetwork()
    {
        //check if the network was created
        if (mNetwork != null)
        {
            mNetwork.Update();

            NetworkEvent evt;

            while (mNetwork != null && mNetwork.Dequeue(out evt))
            {

                switch (evt.Type)
                {
                    case NetEventType.ServerInitialized:
                        {
                            mIsServer = true;

                        }
                        break;
                    case NetEventType.ServerInitFailed:
                        {

                            mIsServer = false;
                            Reset();
                        }
                        break;
                    case NetEventType.ServerClosed:
                        {
                            mIsServer = false;
                        }
                        break;
                    case NetEventType.NewConnection:
                        {
                            mConnections.Add(evt.ConnectionId);
                        }
                        break;
                    case NetEventType.ConnectionFailed:
                        {
                            Reset();
                        }
                        break;
                    case NetEventType.Disconnected:
                        {
                            mConnections.Remove(evt.ConnectionId);

                            if (mIsServer == false)
                            {
                                Reset();
                            }

                        }
                        break;
                    case NetEventType.ReliableMessageReceived:
                    case NetEventType.UnreliableMessageReceived:
                        {
                            HandleIncommingMessage(ref evt);
                        }
                        break;
                }
            }

            //finish this update by flushing the messages out if the network wasn't destroyed during update
            if (mNetwork != null)
                mNetwork.Flush();
        }
    }

    private void HandleIncommingMessage(ref NetworkEvent evt)
    {
        MessageDataBuffer buffer = (MessageDataBuffer)evt.MessageData;

        string msg = Encoding.UTF8.GetString(buffer.Buffer, 0, buffer.ContentLength);


        if (mIsServer)
        {
            SendString(msg);
        }

        buffer.Dispose();
    }

    private void SendString(string msg, bool reliable = true)
    {

        byte[] msgData = Encoding.UTF8.GetBytes(msg);

        foreach (ConnectionId id in mConnections)
        {
            mNetwork.SendData(id, msgData, 0, msgData.Length, reliable);
        }
    }

    public void JoinRoomButtonPressed(string roomId)
    {
        Setup();
        mNetwork.Connect(roomId);

        if (mNetwork != null)
        {
            isConnceted = true;
        }

        StartCoroutine(CheckForConnection());
    }

    private IEnumerator CheckForConnection()
    {
        yield return new WaitUntil(() => mNetwork != null);
        isConnceted = true;
    }

    private void OpenRoomButtonPressed()
    {
        Setup();
        mNetwork.StartServer(accessCode);
    }


    private void SendButtonPressed(string inputMsg)
    {

        if (mIsServer)
        {
            SendString(inputMsg);
        }
        else
        {
            SendString(inputMsg);
        }

    }


    private void SendButtonPressed()
    {
        carData = JsonUtility.ToJson(carDataController);
        SendString(carData);
    }

}
