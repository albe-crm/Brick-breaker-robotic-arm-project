using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
// --- WIFI Communication ---
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


public class PaddleScript : MonoBehaviour
{
    [Header("WiFi Settings (Replaces Serial)")]
    public string arduinoIP = "10.80.184.28"; // 10.80.184.28 <--- PUT ARDUINO IP HERE
    public int arduinoPort = 4242;
    public int localPort = 4241;

    [Header("Paddle movement")]
    public float speedPaddle = 5f;
    public float upScreenEdge = 4f;
    public float downScreenEdge = -4f;

    [Header("Game refs")]
    public GameManager gm;
    public BallScript Ball;
    public TMPro.TMP_Dropdown paddleDimDrop;

    [Header("Debug / angle info")]
    public float alpha;  // Arduino value (0–100)
    public float beta;   // kept for GameManager compatibility
    public float gamma;  // kept for compatibility

    [Header("Smoothing")]
    public float snapStep = 0.25f;   // world-units snapping to reduce jitter

    [Header("Visual Debug")]
    public SpriteRenderer borderSR;

    public int collision_number = 0;

    Vector3 oldscale;
    float oldupScreenEdge;
    float olddownScreenEdge;

    // WiFi UDP communication
    UdpClient udpClient;
    Thread receiveThread;
    bool running = true;
    float receivedAlpha = 0;
    bool serialDisabled = false;


    // Recording
    List<int> recordedValues = new List<int>();
    List<int> recordedAssist = new List<int>();
    bool recording = false;

    public int currentAssistLevel = 0; // 0=none,1=gentle,2=strong

    void Start()
    {
        try
        {
            udpClient = new UdpClient(localPort);
            
            receiveThread = new Thread(new ThreadStart(ListenForData));
            receiveThread.IsBackground = true;
            receiveThread.Start();

            Debug.Log($"[Paddle] WiFi Listening on port {localPort}");
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Paddle] WiFi Error: " + e.Message);
            serialDisabled = true;
        }
    }

    void ListenForData()
    {
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        while (running)
        {
            try
            {
                byte[] data = udpClient.Receive(ref remoteEndPoint);
                string text = Encoding.ASCII.GetString(data);
                
                Debug.Log("Received from Arduino: " + text);

                if (float.TryParse(text, out float tempVal))
                {
                    receivedAlpha = tempVal;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("WiFi Error: " + e.Message);
            }
        }
    }


    void Update()
    {
        // Update alpha from Arduino (for inspector) when available
        if (!serialDisabled)
            UpdateFromArduino();

        // Stop movement if not in game
        if (gm != null && !gm.inGame)
        {
            // Handle end-of-game save
            if (recording)
            {
                recording = false;
                SaveToCSV();
            }
            return;
        }

        // When game starts
        if (gm != null && gm.inGame)
            recording = true;

        // Keyboard movement (optional)
        UpdateFromKeyboard();

        // Clamp paddle to screen edges
        float clampedY = Mathf.Clamp(transform.position.y, downScreenEdge, upScreenEdge);
        transform.position = new Vector3(transform.position.x, clampedY, transform.position.z);
    }

    void UpdateFromKeyboard()
    {
        float vertical = Input.GetAxis("Vertical");
        if (Mathf.Abs(vertical) > 0.01f)
            transform.Translate(Vector2.up * vertical * Time.deltaTime * speedPaddle, Space.World);
    }

    void UpdateFromArduino()
    {
        if (udpClient == null) return;

        alpha = receivedAlpha;
        alpha = Mathf.Clamp(alpha, 0, 100);

        // 0..100 -> 0..1
        float t = alpha / 100f;

        // optional color feedback
        if (borderSR != null)
            borderSR.color = Color.Lerp(Color.red, Color.green, t);

        // map to paddle Y range
        float posY = Mathf.Lerp(downScreenEdge, upScreenEdge, t);

        // reduce jitter
        if (snapStep > 0f)
            posY = Mathf.Round(posY / snapStep) * snapStep;

        transform.position = new Vector3(transform.position.x, posY, transform.position.z);
    }

    // Arduino command: EXACTLY 2 bytes
    // byte0 = '0','1','2'
    // byte1 = targetAngle01 (0..100)
    public void SendAssistCommand(int assistLevel, int targetAngle01)
    {
        if (udpClient == null) return;

        assistLevel = Mathf.Clamp(assistLevel, 0, 2);
        targetAngle01 = Mathf.Clamp(targetAngle01, 0, 100);

        currentAssistLevel = assistLevel;

        try
        {
            byte[] data = new byte[2];
            data[0] = (byte)('0' + assistLevel);
            data[1] = (byte)targetAngle01;
            
            udpClient.Send(data, data.Length, arduinoIP, arduinoPort);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Paddle] UDP send error: " + e.Message);
        }
    }

    // Optional: send only assist level (1 byte)
    public void SendAssistLevel(int level)
    {
        if (udpClient == null) return;

        level = Mathf.Clamp(level, 0, 2);
        currentAssistLevel = level;

        byte code = (byte)('0' + level);

        try
        {
            byte[] data = new byte[] { code };
            udpClient.Send(data, data.Length, arduinoIP, arduinoPort);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Paddle] Error sending assist level: " + e.Message);
        }
    }


    void OnApplicationQuit()
    {
        running = false;
        if (udpClient != null) udpClient.Close();
        if (receiveThread != null && receiveThread.IsAlive) receiveThread.Abort();
    }

    // -----------------------------
    // Existing game logic
    // -----------------------------

    public void ResetPaddle()
    {
        transform.position = new Vector2(transform.position.x, downScreenEdge);
        collision_number = 0;
    }

    public void PaddleDimSelector()
    {
        if (paddleDimDrop == null) return;

        if (paddleDimDrop.value == 0)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
            upScreenEdge = 4f;
            downScreenEdge = -4f;
        }
        else if (paddleDimDrop.value == 1)
        {
            transform.localScale = new Vector3(0.7f, 1f, 1f);
            upScreenEdge = 3.5f;
            downScreenEdge = -3.5f;
        }
        else if (paddleDimDrop.value == 2)
        {
            transform.localScale = new Vector3(1.3f, 1f, 1f);
            upScreenEdge = 4.5f;
            downScreenEdge = -4.5f;
        }
    }

    public void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            collision_number += 1;
            if (gm != null && gm.audioSource != null && gm.audioSource.Length > 0)
                gm.audioSource[0].Play();

            if (Ball != null)
                Ball.cont = 0;
        }
    }

    public void AumentaPaddle()
    {
        oldscale = new Vector3(transform.localScale.x, 1f, 1f);
        oldupScreenEdge = upScreenEdge;
        olddownScreenEdge = downScreenEdge;

        transform.localScale = new Vector3(oldscale.x + 0.3f, 1f, 1f);
        upScreenEdge = oldupScreenEdge - 0.75f;
        downScreenEdge = olddownScreenEdge + 0.75f;

        StartCoroutine(RiduciPaddle());
    }

    System.Collections.IEnumerator RiduciPaddle()
    {
        yield return new WaitForSeconds(10f);

        transform.localScale = new Vector3(oldscale.x, 1f, 1f);
        upScreenEdge = oldupScreenEdge;
        downScreenEdge = olddownScreenEdge;
    }

    void SaveToCSV()
    {
        int fileIndex = GetNextFileIndex();

        // saves inside Assets folder by default
        string path = Application.dataPath + "/arduino_values_" + fileIndex + ".csv";

        try
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine("Value,AssistLevel");

                int n = Mathf.Min(recordedValues.Count, recordedAssist.Count);
                for (int i = 0; i < n; i++)
                    sw.WriteLine(recordedValues[i] + "," + recordedAssist[i]);
            }

            Debug.Log("Data saved to: " + path);
        }
        catch (Exception e)
        {
            Debug.LogWarning("CSV save failed: " + e.Message);
        }

        recordedValues.Clear();
        recordedAssist.Clear();
    }

    int GetNextFileIndex()
    {
        // Change this folder if you want.
        // If it doesn't exist, we fall back to Assets folder.
        string folder = @"C:\Users\rebec\OneDrive\Documenti\GitHub\BIATHLON_\Data";

        try
        {
            if (!Directory.Exists(folder))
                return 1;

            string[] files = Directory.GetFiles(folder, "arduino_values_*.csv");

            int maxIndex = 0;
            foreach (string f in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(f);
                string[] parts = fileName.Split('_');

                if (parts.Length == 3 && int.TryParse(parts[2], out int index))
                    if (index > maxIndex) maxIndex = index;
            }

            return maxIndex + 1;
        }
        catch
        {
            return 1;
        }
    }
}
