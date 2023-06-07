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
    public RawImage rawImage;
    public GameObject sphere;
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

    private void OnWebSocketMessage(byte[] data)
    {
        response = System.Text.Encoding.UTF8.GetString(data);
        UnityEngine.Debug.Log("WebSocket message received: " + response);
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

        if (mainCam != null && printMessage != null)
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
                printMessage.text = combinedMessage;
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
