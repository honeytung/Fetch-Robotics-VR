using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using NativeWebSocket;
using WebSocket = NativeWebSocket.WebSocket;
using WebSocketState = NativeWebSocket.WebSocketState;
using Oculus.Platform.Models;
using Oculus.Interaction;
using System;

public class camClient : MonoBehaviour
{
    public string serverUrl = "ws://10.155.234.220:8765";
    public Camera mainCam;
    public GameObject canvas;
    public GameObject fetchModel;
    public RawImage rawImage;
    public GameObject sphere;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI printMessage;
    public TextMeshProUGUI printResponse;
    public Button homeButton;
    public Button toolWallButton;
    public Button tableButton;
    public Button stopButton;
    public OVRInput.Controller controller = OVRInput.Controller.RTouch;

    private WebSocket websocket;
    private float messageSendTimer = 0f;
    private string coordinates = "(0.00, 0.00)";
    private int grabBool = 0;
    private int homeBool = 0;
    private int toolWallBool = 0;
    private int tableBool = 0;
    private int stopBool = 0;
    private string response;

    async void Start()
    {
        // buttons
        homeButton.onClick.AddListener(clickHome);
        toolWallButton.onClick.AddListener(clickToolWall);
        tableButton.onClick.AddListener(clickTable);
        stopButton.onClick.AddListener(clickStop);
        // websocket
        websocket = new WebSocket(serverUrl);
        websocket.OnOpen += OnWebSocketOpen;
        websocket.OnMessage += OnWebSocketMessage;
        websocket.OnError += OnWebSocketError;
        websocket.OnClose += OnWebSocketClose;
        await websocket.Connect();
    }

    async void OnDestroy()
    {
        if (websocket != null)
        {
            await websocket.Close();
            websocket = null;
        }
    }

    public async void SendMessageToServer(string message)
    {
        if (websocket.State == WebSocketState.Open)
        {
            await websocket.SendText(message);
        }
        else
        {
            UnityEngine.Debug.LogError("WebSocket is not connected.");
        }
    }

    private void OnWebSocketOpen()
    {
        UnityEngine.Debug.Log("WebSocket connected.");
    }

    private void changeFetchPosition(float poseX, float poseZ, float orienX, float orienY, float orienZ, float orienW) 
    {
        Vector3 targetPosition = fetchModel.transform.position;
        Quaternion targetRotation = fetchModel.transform.rotation;

        targetPosition.x = 20 - (12.1782f - poseX);
        targetPosition.z = poseZ - 20.8214f;

        //targetRotation.x = orienX;
        //targetRotation.y = orienY;
        //targetRotation.z = orienZ;
        //targetRotation.w = orienW;

        fetchModel.transform.position = targetPosition;
        fetchModel.transform.rotation = targetRotation;
    }

    private void updateUIStatus(String userStatus)
    {
        int statusValue;
        bool parseSuccess = int.TryParse(userStatus, out statusValue);
        Color textColor = statusText.color;

        if (parseSuccess)
        {
            String status;

            if (statusValue == 0)
            {
                status = "Idle";
                textColor = Color.green;
            }
            else if (statusValue == 1)
            {
                status = "Traveling";
                textColor = Color.yellow;
            }
            else if (statusValue == 3)
            {
                status = "Picking";
                textColor = Color.yellow;
            }
            else if (statusValue == 4)
            {
                status = "Placing";
                textColor = Color.yellow;
            }
            else if (statusValue == 5)
            {
                status = "Inoperable";
                textColor = Color.red;
            }
            else if (statusValue == 6)
            {
                status = "Waiting";
                textColor = Color.green;
            }
            else
            {
                status = "Unknown";
                textColor = Color.red;
            }

            statusText.color = textColor;
            statusText.text = status;
        }
    }

    private void OnWebSocketMessage(byte[] data)
    {
        response = System.Text.Encoding.UTF8.GetString(data);
        printResponse.text = response;
        UnityEngine.Debug.Log("WebSocket message received: " + response);

        string[] delimiters = { ",", ";" };
        string[] substrings = response.Split(delimiters, StringSplitOptions.None);

        if (substrings.Length != 9) {
            UnityEngine.Debug.LogError("Message is invalid.");
            return;
        }

        String userStatus = substrings[0];
        String maniupulationStatus = substrings[1];
        float poseX = float.Parse(substrings[2]);
        float poseY = float.Parse(substrings[3]);
        float poseZ = float.Parse(substrings[4]);
        float orienX = float.Parse(substrings[5]);
        float orienY = float.Parse(substrings[6]);
        float orienZ = float.Parse(substrings[7]);
        float orienW = float.Parse(substrings[8]);

        changeFetchPosition(poseX, poseY, orienX, orienY, orienZ, orienW);
        updateUIStatus(userStatus);
    }

    private void OnWebSocketError(string errorMessage)
    {
        UnityEngine.Debug.LogError("WebSocket error: " + errorMessage);
    }

    private void OnWebSocketClose(WebSocketCloseCode closeCode)
    {
        UnityEngine.Debug.Log("WebSocket closed with code: " + closeCode.ToString());
    }

    void clickHome()
    {
        homeBool = 1;
    }

    void clickToolWall()
    {
        toolWallBool = 1;
    }

    void clickTable()
    {
        tableBool = 1;
    }

    void clickStop()
    {
        stopBool = 1;
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
#endif
        if (sphere.activeSelf)
        {
            Vector3 spherePosition = sphere.transform.position;
            Vector2 canvasPosition = canvas.transform.InverseTransformPoint(spherePosition);
            Vector2 rawImagePosInCanvas = rawImage.rectTransform.anchoredPosition;
            Vector2 rawImageSize = rawImage.rectTransform.rect.size;
            Vector2 rawImageNormalizedPos = new Vector2(
                ((canvasPosition.x - rawImagePosInCanvas.x) / rawImageSize.x + 0.5f) * rawImageSize.x, 
                ((1 - ((canvasPosition.y - rawImagePosInCanvas.y) / rawImageSize.y + 0.5f)) * rawImageSize.y));

            coordinates = rawImageNormalizedPos.ToString();
        }

        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, controller)) 
        {
            grabBool = 1;
        }

        if (mainCam != null)
        {
            messageSendTimer += Time.deltaTime;
            if (messageSendTimer >= 0.25)
            {
                // Get VR HMD orientation
                string camRotation = mainCam.transform.rotation.eulerAngles.ToString();
                // Get the controller stick values
                Vector2 touchPosition = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick, OVRInput.Controller.Touch);
                string touchPositionString = touchPosition.ToString();
                string combinedMessage = $"{camRotation};{touchPositionString};{coordinates};{grabBool};{homeBool};{toolWallBool};{tableBool};{stopBool}";
                SendMessageToServer(combinedMessage);
                messageSendTimer = 0f;
                grabBool = 0;
                homeBool = 0;
                toolWallBool = 0;
                tableBool = 0;
                stopBool = 0;
            }
        }
    }
}
