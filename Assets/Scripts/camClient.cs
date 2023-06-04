using UnityEngine;
using TMPro;
using System.Collections;
using NativeWebSocket;
using WebSocket = NativeWebSocket.WebSocket;
using WebSocketState = NativeWebSocket.WebSocketState;
using Oculus.Platform.Models;

public class camClient : MonoBehaviour
{
    public Camera mainCam;
    public TextMeshProUGUI textElement;
    public string serverUrl = "ws://10.155.234.220:8765";
    private WebSocket websocket;

    private float messageSendTimer = 0f;

    async void Start()
    {
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
        string message = System.Text.Encoding.UTF8.GetString(data);
        UnityEngine.Debug.Log("WebSocket message received: " + message);
    }

    private void OnWebSocketError(string errorMessage)
    {
        UnityEngine.Debug.LogError("WebSocket error: " + errorMessage);
    }

    private void OnWebSocketClose(WebSocketCloseCode closeCode)
    {
        UnityEngine.Debug.Log("WebSocket closed with code: " + closeCode.ToString());
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
#endif
        if (mainCam != null && textElement != null)
        {
            messageSendTimer += Time.deltaTime;
            if (messageSendTimer >= 0.25)
            {
                // Get VR HMD orientation
                string camRotation = mainCam.transform.rotation.eulerAngles.ToString();
                // Get the controller stick values
                Vector2 touchPosition = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick, OVRInput.Controller.Touch);
                string touchPositionString = touchPosition.ToString();
                // Combine camRotation and touchPosition into one message
                string combinedMessage = $"{camRotation};{touchPositionString}";
                textElement.text = combinedMessage;
                SendMessageToServer(combinedMessage);
                messageSendTimer = 0f;
            }
        }
    }
}
