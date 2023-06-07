/*
 *  
 *  Modified from MJPEGStreamDecoder.cs for TECHIN 517 Robotics Lab Perception Team
 *  Original Author: lightfromshadows
 *  Original Source: https://gist.github.com/lightfromshadows/79029ca480393270009173abc7cad858 
 *  Modified By: Harry Tung
 *  Date: May 11, 2023
 *  Description: Streaming JMPEG from Flask Server to RawImage in Unity Oculus Integration SDK
 *
 */

using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class WebcamStreamer : MonoBehaviour {
    [SerializeField] bool tryOnStart = true;
    [SerializeField] bool debugImage = false;
    [SerializeField] string defaultStreamURL = "http://10.155.234.37:5000";
    [SerializeField] RawImage rawImage;

    int MAX_RETRIES = 3;
    int retryCount = 0;

    byte[] nextFrame = null;

    Thread worker;
    int threadID = 0;

    static System.Random randu; 
    List<BufferedStream> trackedBuffers = new List<BufferedStream>();

    // Start is called before the first frame update
    void Start() {
        randu = new System.Random(Random.Range(0, 65536));
        if (tryOnStart) {
            StartStream(defaultStreamURL);
        }
    }

    private void Update() {
        if (nextFrame != null) {
            SendFrame(nextFrame);
            nextFrame = null;
        }
    }

    private void OnDestroy() {
        foreach (var b in trackedBuffers) {
            if (b != null) {
                b.Close();
            }
        }
    }

    public void StartStream(string url) {
        retryCount = 0;
        StopAllCoroutines();
        foreach (var b in trackedBuffers)
            b.Close();

        worker = new Thread(() => ReadMJPEGStreamWorker(threadID = randu.Next(65536), url));
        worker.Start();
    }

    void ReadMJPEGStreamWorker(int id, string url) {
        var webRequest = WebRequest.Create(url);
        webRequest.Method = "GET";
        List<byte> frameBuffer = new List<byte>();

        int lastByte = 0x00;
        bool addToBuffer = false;

        BufferedStream buffer = null;
        try {
            Stream stream = webRequest.GetResponse().GetResponseStream();
            buffer = new BufferedStream(stream);
            trackedBuffers.Add(buffer);
        }
        catch (System.Exception ex) {
            Debug.LogError(ex);
        }

        int newByte;

        while (buffer != null) {
            if (threadID != id) {
                return; // We are no longer the active thread! stop doing things damnit!
            }

            if (!buffer.CanRead) {
                Debug.LogError("Can't read buffer!");
                break;
            }

            newByte = -1;

            try {
                newByte = buffer.ReadByte();
            }
            catch {
                break; // Something happened to the stream, start a new one
            }

            if (newByte < 0) {
                continue; // End of data
            }

            if (addToBuffer) {
                frameBuffer.Add((byte)newByte);
            }

            if (lastByte == 0xFF) {
                if (!addToBuffer) {
                    if (IsStartOfImage(newByte)) {
                        addToBuffer = true;
                        frameBuffer.Add((byte)lastByte);
                        frameBuffer.Add((byte)newByte);
                    }
                }
                else {
                    if (newByte == 0xD9) {
                        frameBuffer.Add((byte)newByte);
                        addToBuffer = false;
                        nextFrame = frameBuffer.ToArray();
                        frameBuffer.Clear();
                    }
                }
            }

            lastByte = newByte;
        }

        if (retryCount < MAX_RETRIES) {
            retryCount++;
            Debug.LogFormat("[{0}] Retrying Connection {1}...", id, retryCount);

            foreach (var b in trackedBuffers) {
                b.Dispose();
            }

            trackedBuffers.Clear();
            worker = new Thread(() => ReadMJPEGStreamWorker(threadID = randu.Next(65536), url));
            worker.Start();
        }
    }

    bool IsStartOfImage(int command) {
        switch (command) {
            case 0x8D:
                Debug.Log("Command SOI");
                return true;
            case 0xC0:
                Debug.Log("Command SOF0");
                return true;
            case 0xC2:
                Debug.Log("Command SOF2");
                return true;
            case 0xC4:
                Debug.Log("Command DHT");
                break;
            case 0xD8:
                //Debug.Log("Command DQT");
                return true;
            case 0xDD:
                Debug.Log("Command DRI");
                break;
            case 0xDA:
                Debug.Log("Command SOS");
                break;
            case 0xFE:
                Debug.Log("Command COM");
                break;
            case 0xD9:
                Debug.Log("Command EOI");
                break;
        }
        return false;
    }

    void SendFrame(byte[] bytes) {
        Texture2D texture2D = new Texture2D(2, 2);
        Texture2D previousTexture2D = (Texture2D) rawImage.texture;

        texture2D.LoadImage(bytes);

        if(debugImage) {
            Debug.LogFormat("Loaded {0}b image [{1},{2}]", bytes.Length, texture2D.width, texture2D.height);
        }

        if (texture2D.width == 2 || rawImage == null || texture2D == null) {
            Debug.LogFormat("Error: Image format not correct!");
            return;
        }
        rawImage.texture = texture2D;

        Destroy(previousTexture2D);
    }
}