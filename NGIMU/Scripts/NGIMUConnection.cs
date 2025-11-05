using System;
using System.Text;
using System.Threading;
using NgimuApi;
using NgimuApi.SearchForConnections;
using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;



namespace NGIMU.Scripts
{
    public class NGIMUConnection : MonoBehaviour
    {
        public const float PI = 180f;
        Vector3 LastAcc, LastAcc2;
        Vector3 LastPos, LastPos2;
        Vector3 LastVel, LastVel2;
        Vector3 LastVelFilt, LastVelFilt2;
        Vector3 Acc, Acc2;
        Vector3 Vel, Vel2;
        Vector3 VelFilt, VelFilt2;
        Vector3 Pos, Pos2;
        public double LastAccZ, LastAccZ2;
        public double LastVelZ2, LastVelZ;
        public double LastPosZ2, LastPosZ;
        public double AccZ, AccZ2;
        public double Velx, Velx2;
        public double Vely, Vely2;
        public double VelZ, VelZ2;
        public double Posx, Posx2;
        public double Posy, Posy2;
        public double PosZ, PosZ2;
        public double LastVelZFilt, LastVelZFilt2;
        public double VelZcalc, VelZcalc2;
        public double AccZRaw, AccZRaw2;
        public double AccZcalc, AccZcalc2;
        public double PosZcalc, PosZcalc2;
        public float gamma;
        Quaternion Quat, Quat2;
        Vector3 AccRaw, AccRaw2;
        Vector3 AccEarth, AccEarth2;
        Vector3 AccLinear, AccLinear2;
        Quaternion LastQuat, LastQuat2;
        ButterworthFilter lowpassfilter;
        ButterworthFilter highpassfilterVel;
        ButterworthFilter highpassfilterPos;
        public double meanZ,meanZ2;
        public double dt;
        public double g;
        public float acc,acc2;
        public string accS,accS2;
        public int liv;        
        public Calibration calib;
        public PaddleScript paddle3;
        public float startPosX, startPosX2;
        public GameManager gm;
        //string folder_path;
        public string path, path2;
        public string eulerXs, eulerXs2;
        public string gammaS;
        Vector3 direzioneY1, direzioneY2;


        // Matrix4x4 RotationMatrix;

        public enum ConnectionTab
        {
            None = 0,
            ModelView = 1, 
            ConsoleView = 2,
            SettingsView = 3,
            ErrorsView = 4,
        }

        void Start()
        {
            path = Application.dataPath;
            path2 = Application.dataPath;


            //Initialize the acceleration vector
            LastAcc = new Vector3(0.0f, 0.0f, 0.0f);
            LastAcc2 = new Vector3(0.0f, 0.0f, 0.0f);
            //Initialize the velocity vector
            LastVel = new Vector3(0.0f, 0.0f, 0.0f);
            LastVel2 = new Vector3(0.0f, 0.0f, 0.0f);
            //Initialize the velocity vector
            LastVelFilt = new Vector3(0.0f, 0.0f, 0.0f);
            LastVelFilt2 = new Vector3(0.0f, 0.0f, 0.0f);
            //Initialize the position vector
            LastPos = new Vector3(0.0f, 0.0f, 0.0f);
            LastPos2 = new Vector3(0.0f, 0.0f, 0.0f);
            LastAccZ = 0.0;
            LastAccZ2 = 0.0;
            LastVelZ = 0.0;
            LastVelZ2 = 0.0;
            LastPosZ = 0.0;
            LastPosZ2 = 0.0;
            LastVelZFilt = 0.0;
            LastVelZFilt2 = 0.0;
            VelZcalc = 0.0;
            VelZcalc2 = 0.0;
            AccZRaw = 0.0;
            AccZRaw2 = 0.0;
            AccZcalc = 0.0;
            AccZcalc2 = 0.0;
            PosZcalc = 0.0;
            PosZcalc2 = 0.0;
            AccZ = 0.0;
            AccZ2 = 0.0;
            Velx = 0.0;
            Velx2 = 0.0;
            Vely = 0.0;
            Vely2 = 0.0;
            VelZ = 0.0;
            VelZ2 = 0.0;
            Posx = 0.0;
            Posx2 = 0.0;
            Posy = 0.0;
            Posy2 = 0.0;
            PosZ = 0.0;
            PosZ2 = 0.0;
            meanZ = -0.0184;
            meanZ2 = -0.0184;
            dt = 0.005;
            g = 9.81;

            

            // Define the filter coefficients (b and a parameters)
            double[] b = { 0.99686824, -0.99686824 };
            double[] a = { 1.00000000, -0.99373647 };

            // Create a lowpass (it became a highpass) Butterworth filter with the precomputed coefficients
            lowpassfilter = new ButterworthFilter(b, a);

            // Define the filter coefficients (c and d parameters)
            double[] c = { 0.99686824, -0.99686824 };
            double[] d = { 1.00000000, -0.99373647 };

            // Create a highpass Butterworth filter with the precomputed coefficients
            highpassfilterVel = new ButterworthFilter(c, d);

            /*// Define the filter coefficients (e and f parameters)
            double[] e = { .99375596, -0.99375596 };
            double[] f = { 1.00000000, -0.98751193 };

            // Create a highpass Butterworth filter with the precomputed coefficients
            highpassfilterPos = new ButterworthFilter(e, f);*/
            startPosX = 7.6f;
            ModelTransform.position = new Vector3 (startPosX,0f,0f);        
            ModelTransform2.position = new Vector3(startPosX, 0f, 0f);        
        }
        private static readonly NgimuApi.Maths.Quaternion zero = new NgimuApi.Maths.Quaternion(0, 0, 0, 0);
        private static readonly NgimuApi.Maths.Vector3 inizio = new NgimuApi.Maths.Vector3(0,0,0);
        private NgimuApi.Maths.RotationMatrix rot_matrix;
        private NgimuApi.Maths.RotationMatrix rot_matrix2;

        [Header("Header")]
       
        
        [Header("Model")]
        [SerializeField, Tooltip("Model to transform", order = 10)] public Transform ModelTransform;
        [SerializeField, Tooltip("Model to transform", order = 10)] public Transform ModelTransform2;

        [Header("Tabs")]
        [SerializeField, Tooltip("3d model view", order = 20)] public GameObject ModelView;
        [SerializeField, Tooltip("Console view", order = 21)] public GameObject ConsoleView;
        [SerializeField, Tooltip("Settings view", order = 22)] public GameObject SettingsView;
        [SerializeField, Tooltip("Errors view", order = 23)] public GameObject ErrorsView;
        
        [Header("Text")]
        [SerializeField, Tooltip("Settings text", order = 30)] public TMP_Text SettingsText;
        [SerializeField, Tooltip("Errors text", order = 31)] public TMP_Text ErrorsText;

        private readonly UnityReporter reporter = new UnityReporter();

        private Connection connection;
        private Connection connection2;

        public void SelectTab(int tab)
        {
            SelectTab((ConnectionTab) tab);
        }

        public void SelectTab(ConnectionTab tab)
        {
            ModelView.SetActive(tab == ConnectionTab.ModelView);
            ConsoleView.SetActive(tab == ConnectionTab.ConsoleView);
            SettingsView.SetActive(tab == ConnectionTab.SettingsView);
            ErrorsView.SetActive(tab == ConnectionTab.ErrorsView);

            if (connection != null)
            {
                StringBuilder sb = new StringBuilder();
                
                foreach (var setting in connection.Settings.Values)
                {
                    sb.AppendLine(setting.Message.ToString()); 
                }

                SettingsText.text = sb.ToString();
            }
        }

        public void ConnectTo(ConnectionSearchResult info)
        { 
            SelectTab(ConnectionTab.ModelView);
            ErrorsText.text = "No errors"; 
            
            reporter.Clear();

            Debug.Log($"ConnectTo ({info.DeviceDescriptor})");


            switch (info.ConnectionType)
            {
                case ConnectionType.Udp:
                    UdpConnectionInfo udpInfo = info.ConnectionInfo as UdpConnectionInfo;

                    Thread thread = new Thread(
                        delegate()
                        {
                            try
                            {
                                Debug.Log("Configure unique UDP connection");

                                UdpConnectionInfo newInfo = Connection.ConfigureUniqueUdpConnection(udpInfo, reporter);

                                ConnectTo(newInfo);
                            }
                            catch (Exception ex)
                            {
                                reporter.OnException(this, new ExceptionEventArgs("The device could not be configured to use a unique connection. " + ex.Message, ex));
                            }
                        }
                    );
                    thread.Name = "Configure Unique UDP Connection";
                    thread.Start();

                    break;

                case ConnectionType.Serial:
                    ConnectTo(info.ConnectionInfo as SerialConnectionInfo);
                    break;

                default:
                    break;
            }
        }

        private void ConnectTo(IConnectionInfo connectionInfo)
        {
            Debug.Log("ConnectTo: " + connectionInfo.ToString());

            if (connection == null)
            {
                connection = new Connection(connectionInfo);

                connection.Connected += reporter.OnConnected;
                connection.Disconnected += reporter.OnDisconnected;
                connection.Error += ConnectionOnError;
                connection.Exception += ConnectionOnException;
                connection.Info += reporter.OnInfo;
                connection.Message += reporter.OnMessage;
                connection.UnknownAddress += reporter.OnUnknownAddress;

                connection.Connect();

                connection.Settings.ReadAync(new ISettingItem[] { connection.Settings }, reporter, 100, 3);
            }
            else
            {
                connection2 = new Connection(connectionInfo);

                connection2.Connected += reporter.OnConnected;
                connection2.Disconnected += reporter.OnDisconnected;
                connection2.Error += ConnectionOnError;
                connection2.Exception += ConnectionOnException;
                connection2.Info += reporter.OnInfo;
                connection2.Message += reporter.OnMessage;
                connection2.UnknownAddress += reporter.OnUnknownAddress;

                connection2.Connect();

                connection2.Settings.ReadAync(new ISettingItem[] { connection2.Settings }, reporter, 100, 3);
            }
        }

        private void ConnectionOnException(object sender, ExceptionEventArgs e)
        {
            reporter.OnException(sender, e);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Exception");
            
            sb.AppendLine(e.Message);

            if (e.Exception != null)
            {
                sb.AppendLine(e.Exception.ToString());
            }
            
            ErrorsText.text = sb.ToString();
        }

        private void ConnectionOnError(object sender, MessageEventArgs e)
        {
            reporter.OnError(sender, e); 
            
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Error");
            
            sb.AppendLine(e.Message);

            if (e.Detail != null)
            {
                sb.AppendLine(e.Detail);
            }

            ErrorsText.text = sb.ToString();
        }       

        public void Disconnect()
        {
            SelectTab(ConnectionTab.None);

            if (connection == null)
            {
                return;
            }
            if (connection2 == null)
            {
                return;
            }

            Debug.Log("Disconnect");

            connection.Connected -= reporter.OnConnected;
            connection.Disconnected -= reporter.OnDisconnected;
            connection.Error -= ConnectionOnError;
            connection.Exception -= ConnectionOnException;
            connection.Info -= reporter.OnInfo;
            connection.Message -= reporter.OnMessage;
            connection.UnknownAddress -= reporter.OnUnknownAddress;
            connection.Dispose();
            connection = null;

            connection2.Connected -= reporter.OnConnected;
            connection2.Disconnected -= reporter.OnDisconnected;
            connection2.Error -= ConnectionOnError;
            connection2.Exception -= ConnectionOnException;
            connection2.Info -= reporter.OnInfo;
            connection2.Message -= reporter.OnMessage;
            connection2.UnknownAddress -= reporter.OnUnknownAddress;
            connection2.Dispose();
            connection2 = null;
        }

        private void Update()
        {
            if (connection2.Quaternion.Quaternion != zero)
            {
                ModelTransform2.rotation = NgimuMathUtils.NgimuToUnityQuaternion(connection2.Quaternion.Quaternion);

            }
            if (Input.GetKeyDown(KeyCode.Escape) == true)
            {
                Disconnect();
                SceneManager.LoadScene("5");             //inutile
            }

            if (connection == null)
            {
                return;
            }
            if (connection2 == null)
            {
                return;
            }

            if (connection.Quaternion.Quaternion != zero)
            {
                Quat = NgimuMathUtils.NgimuToUnityQuaternion(connection.Quaternion.Quaternion);                 // Quaternion from IMU
                AccLinear = NgimuMathUtils.NgimuToUnityVector(connection.LinearAcceleration.LinearAcceleration);   // Linear Acceleration from IMU
                AccEarth = NgimuMathUtils.NgimuToUnityVector(connection.EarthAcceleration.EarthAcceleration);   // Earth Acceleration from IMU
                rot_matrix = NgimuApi.Maths.Quaternion.ToRotationMatrix(connection.Quaternion.Quaternion);

                Vector3 Euler = Quat.eulerAngles;
                direzioneY1 = new Vector3(rot_matrix[1], rot_matrix[4], rot_matrix[7]);
                
                acc = AccEarth[2];

                if (Euler.x > 180f)     // se l'angolo è maggiore di 180° lo esprimo come angolo negativo
                    Euler.x = Euler.x - 360f;


                //// SALVO I DATI (ANGOLI, ACCELERAZIONI + ISTANTE DI TEMPO) 
                //if (liv==6)
                //{
                //    accS = acc.ToString();
                //    List<string> ListAcc = new List<string>();
                //    ListAcc.Add(accS + "+" + Time.time.ToString());

                //    eulerXs = Euler.x.ToString();
                //    List<string> ListAngl = new List<string>();
                //    ListAngl.Add(eulerXs + "+" + Time.time.ToString());

                //    File.AppendAllLines(path + "datiAcc" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAcc);
                //    File.AppendAllLines(path + "datiAng" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAngl);

                //}

                //if (liv == 1 || liv == 12)
                //{
                //    if (liv != 0 && gm.inGame == true)
                //    {
                //        accS = acc.ToString();
                //        List<string> ListAcc = new List<string>();
                //        ListAcc.Add(accS + "+" + Time.time.ToString());


                //        eulerXs = Euler.x.ToString();
                //        List<string> ListAngl = new List<string>();
                //        ListAngl.Add(eulerXs + "+" + Time.time.ToString());

                //        File.AppendAllLines(path + "datiAcc" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAcc);


                //        File.AppendAllLines(path + "datiAng" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAngl);

                //    }
                //}
                //if (liv == 2 || liv == 22)
                //{
                //    if (liv != 0 && gm.inGame == true)
                //    {
                //        accS = acc.ToString();
                //        List<string> ListAcc = new List<string>();
                //        ListAcc.Add(accS + "+" + Time.time.ToString());


                //        eulerXs = Euler.x.ToString();
                //        List<string> ListAngl = new List<string>();
                //        ListAngl.Add(eulerXs + "+" + Time.time.ToString());


                //        File.AppendAllLines(path + "datiAcc" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAcc);
                //        File.AppendAllLines(path + "datiAng" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAngl);

                //    }
                //}

                //if (liv == 3 || liv == 32)
                //{
                //    if (liv != 0 && gm.inGame == true)
                //    {
                //        accS = acc.ToString();
                //        List<string> ListAcc = new List<string>();
                //        ListAcc.Add(accS + "+" + Time.time.ToString());


                //        eulerXs = Euler.x.ToString();
                //        List<string> ListAngl = new List<string>();
                //        ListAngl.Add(eulerXs + "+" + Time.time.ToString());


                //        File.AppendAllLines(path + "datiAcc" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAcc);
                //        File.AppendAllLines(path + "datiAng" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAngl);
                //    }
                //}

                //if (liv == 4 || liv == 42)
                //{
                //    if (liv != 0 && gm.inGame == true)
                //    {
                //        accS = acc.ToString();
                //        List<string> ListAcc = new List<string>();
                //        ListAcc.Add(accS + "+" + Time.time.ToString());


                //        eulerXs = Euler.x.ToString();
                //        List<string> ListAngl = new List<string>();
                //        ListAngl.Add(eulerXs + "+" + Time.time.ToString());


                //        File.AppendAllLines(path + "datiAcc" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAcc);
                //        File.AppendAllLines(path + "datiAng" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAngl);
                //    }
               
                //}


                //if (liv == 5 || liv == 52)
                //{
                //    if (liv != 0 && gm.inGame == true)
                //    {
                //        accS = acc.ToString();
                //        List<string> ListAcc = new List<string>();
                //        ListAcc.Add(accS + "+" + Time.time.ToString());


                //        eulerXs = Euler.x.ToString();
                //        List<string> ListAngl = new List<string>();
                //        ListAngl.Add(eulerXs + "+" + Time.time.ToString());


                //        File.AppendAllLines(path + "datiAcc" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAcc);
                //        File.AppendAllLines(path + "datiAng" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAngl);

                //    }
                //}

                


                    // Update acceleration and velocity values
                    LastAccZ = AccZ;
                    LastVelZ = VelZ;
                    LastVelZFilt = VelZcalc;
                    LastPosZ = PosZ;
                    LastQuat = Quat;
             }
            if (connection2.Quaternion.Quaternion != zero)
            {
                Quat2 = NgimuMathUtils.NgimuToUnityQuaternion(connection2.Quaternion.Quaternion);                 // Quaternion from IMU
                AccLinear2 = NgimuMathUtils.NgimuToUnityVector(connection2.LinearAcceleration.LinearAcceleration);   // Linear Acceleration from IMU
                AccEarth2 = NgimuMathUtils.NgimuToUnityVector(connection2.EarthAcceleration.EarthAcceleration);   // Earth Acceleration from IMU
                rot_matrix2 = NgimuApi.Maths.Quaternion.ToRotationMatrix(connection2.Quaternion.Quaternion);

                acc2 = AccEarth2[2];

                Vector3 Euler2 = Quat2.eulerAngles;

                direzioneY2 = new Vector3(rot_matrix2[1], rot_matrix2[4], rot_matrix2[7]);

                if (Euler2.x > 180f)     // se l'angolo è maggiore di 180° lo esprimo come angolo negativo
                    Euler2.x = Euler2.x - 360f;


                //// SALVO I DATI (ANGOLI, ACCELERAZIONI + ISTANTE DI TEMPO) 
                //if (liv == 6)
                //{
                //    accS2 = acc2.ToString();
                //    List<string> ListAcc2 = new List<string>();
                //    ListAcc2.Add(accS + "+" + Time.time.ToString());

                //    eulerXs2 = Euler2.x.ToString();
                //    List<string> ListAngl2 = new List<string>();
                //    ListAngl2.Add(eulerXs2 + "+" + Time.time.ToString());

                //    File.AppendAllLines(path2 + "datiAcc" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAcc2);
                //    File.AppendAllLines(path2 + "datiAng" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAngl2);

                //}

                //if (liv == 1 || liv == 12)
                //{
                //    if (liv != 0 && gm.inGame == true)
                //    {
                //        accS2 = acc2.ToString();
                //        List<string> ListAcc2 = new List<string>();
                //        ListAcc2.Add(accS + "+" + Time.time.ToString());

                //        eulerXs2 = Euler2.x.ToString();
                //        List<string> ListAngl2 = new List<string>();
                //        ListAngl2.Add(eulerXs2 + "+" + Time.time.ToString());

                //        File.AppendAllLines(path2 + "datiAcc" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAcc2);
                //        File.AppendAllLines(path2 + "datiAng" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAngl2);

                //    }
                //}
                //if (liv == 2 || liv == 22)
                //{
                //    if (liv != 0 && gm.inGame == true)
                //    {
                //        accS2 = acc2.ToString();
                //        List<string> ListAcc2 = new List<string>();
                //        ListAcc2.Add(accS + "+" + Time.time.ToString());

                //        eulerXs2 = Euler2.x.ToString();
                //        List<string> ListAngl2 = new List<string>();
                //        ListAngl2.Add(eulerXs2 + "+" + Time.time.ToString());

                //        File.AppendAllLines(path2 + "datiAcc" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAcc2);
                //        File.AppendAllLines(path2 + "datiAng" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAngl2);

                //    }
                //}

                //if (liv == 3 || liv == 32)
                //{
                //    if (liv != 0 && gm.inGame == true)
                //    {
                //        accS2 = acc2.ToString();
                //        List<string> ListAcc2 = new List<string>();
                //        ListAcc2.Add(accS + "+" + Time.time.ToString());

                //        eulerXs2 = Euler2.x.ToString();
                //        List<string> ListAngl2 = new List<string>();
                //        ListAngl2.Add(eulerXs2 + "+" + Time.time.ToString());

                //        File.AppendAllLines(path2 + "datiAcc" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAcc2);
                //        File.AppendAllLines(path2 + "datiAng" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAngl2);
                //    }
                //}

                //if (liv == 4 || liv == 42)
                //{
                //    if (liv != 0 && gm.inGame == true)
                //    {
                //        accS2 = acc2.ToString();
                //        List<string> ListAcc2 = new List<string>();
                //        ListAcc2.Add(accS + "+" + Time.time.ToString());

                //        eulerXs2 = Euler2.x.ToString();
                //        List<string> ListAngl2 = new List<string>();
                //        ListAngl2.Add(eulerXs2 + "+" + Time.time.ToString());

                //        File.AppendAllLines(path2 + "datiAcc" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAcc2);
                //        File.AppendAllLines(path2 + "datiAng" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAngl2);
                //    }

                //}


                //if (liv == 5 || liv == 52)
                //{
                //    if (liv != 0 && gm.inGame == true)
                //    {
                //        accS2 = acc2.ToString();
                //        List<string> ListAcc2 = new List<string>();
                //        ListAcc2.Add(accS + "+" + Time.time.ToString());

                //        eulerXs2 = Euler2.x.ToString();
                //        List<string> ListAngl2 = new List<string>();
                //        ListAngl2.Add(eulerXs2 + "+" + Time.time.ToString());

                //        File.AppendAllLines(path2 + "datiAcc" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAcc2);
                //        File.AppendAllLines(path2 + "datiAng" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAngl2);

                //    }
                //}


                //if (liv == 6) //livello 6 è la calibrazione  -->sono passata da 0 a 6
                //{
                //    calib.currentAngle2 = Euler2.x;
                //}

                ////importo l'angolo nello script del paddle
                //if (liv == 1 || liv == 12 || liv == 0)
                //{
                //    paddle3.beta = Euler2.x;
                //}
                //else if (liv == 2 || liv == 22)
                //{
                //    paddle3.beta = Euler2.x;
                //}
                //else if (liv == 3 || liv == 32)
                //{
                //    paddle3.beta = Euler2.x;
                //}
                //else if (liv == 4 || liv == 42)
                //{
                //    paddle3.beta = Euler2.x;
                //}
                //else if (liv == 5 || liv == 52)
                //{
                //    paddle3.beta = Euler2.x;
                //}

                // Update acceleration and velocity values
                LastAccZ2 = AccZ2;
                LastVelZ2 = VelZ2;
                LastVelZFilt2 = VelZcalc2;
                LastPosZ2 = PosZ;
                LastQuat2 = Quat2;
            }


            gamma = Vector3.Angle(direzioneY1,direzioneY2);
            if (liv == 6) //livello 6 è la calibrazione  
            {
                calib.currentAngle = gamma;
            }

            //importo l'angolo nello script del paddle
            if (liv == 1 || liv == 12 || liv == 0)
            {
                paddle3.alpha = gamma;
                //paddle3.alpha = Euler.x;
            }
            else if (liv == 2 || liv == 22)
            {
                paddle3.alpha = gamma;
            }
            else if (liv == 3 || liv == 32)
            {
                paddle3.alpha = gamma;
            }
            else if (liv == 4 || liv == 42)
            {
                paddle3.alpha = gamma;
            }
            else if (liv == 5 || liv == 52)
            {
                paddle3.alpha = gamma;
            }

            //salvataggio dati
            if (liv == 1 || liv == 12)
            {
                if (liv != 0 && gm.inGame == true)
                {

                    List<string> ListTempo = new List<string>();
                    ListTempo.Add(Time.time.ToString());

                    accS = acc.ToString();
                    accS2 = acc2.ToString();
                    List<string> ListAcc = new List<string>();
                    ListAcc.Add(accS + ";       " + accS2 + ";      " + Time.time.ToString());

                    gammaS = gamma.ToString();
                    List<string> ListAngl = new List<string>();
                    ListAngl.Add(gammaS + ";        " + Time.time.ToString());      //inizializza gammaS

                    File.AppendAllLines(path + "datiTempo" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListTempo);
                    File.AppendAllLines(path + "datiAcc" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAcc);
                    File.AppendAllLines(path + "datiAng" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAngl);

                }
            }
            if (liv == 2 || liv == 22)
            {
                if (liv != 0 && gm.inGame == true)
                {

                    List<string> ListTempo = new List<string>();
                    ListTempo.Add(Time.time.ToString());

                    accS = acc.ToString();
                    accS2 = acc2.ToString();
                    List<string> ListAcc = new List<string>();
                    ListAcc.Add(accS + ";       " + accS2 + ";      " + Time.time.ToString());

                    gammaS = gamma.ToString();
                    List<string> ListAngl = new List<string>();
                    ListAngl.Add(gammaS + ";        " + Time.time.ToString());          //inizializza gammaS

                    File.AppendAllLines(path + "datiTempo" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListTempo);
                    File.AppendAllLines(path + "datiAcc" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAcc);
                    File.AppendAllLines(path + "datiAng" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAngl);

                }
            }
            if (liv == 3 || liv == 32)
            {
                if (liv != 0 && gm.inGame == true)
                {

                    List<string> ListTempo = new List<string>();
                    ListTempo.Add(Time.time.ToString());

                    accS = acc.ToString();
                    accS2 = acc2.ToString();
                    List<string> ListAcc = new List<string>();
                    ListAcc.Add(accS + ";       " + accS2 + ";      " + Time.time.ToString());

                    gammaS = gamma.ToString();
                    List<string> ListAngl = new List<string>();
                    ListAngl.Add(gammaS + ";        " + Time.time.ToString());          //inizializza gammaS

                    File.AppendAllLines(path + "datiTempo" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListTempo);
                    File.AppendAllLines(path + "datiAcc" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAcc);
                    File.AppendAllLines(path + "datiAng" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAngl);

                }
            }
            if (liv == 4 || liv == 42)
            {
                if (liv != 0 && gm.inGame == true)
                {

                    List<string> ListTempo = new List<string>();
                    ListTempo.Add(Time.time.ToString());

                    accS = acc.ToString();
                    accS2 = acc2.ToString();
                    List<string> ListAcc = new List<string>();
                    ListAcc.Add(accS + ";       " + accS2 + ";      " + Time.time.ToString());

                    gammaS = gamma.ToString();
                    List<string> ListAngl = new List<string>();
                    ListAngl.Add(gammaS + ";        " + Time.time.ToString());          //inizializza gammaS

                    File.AppendAllLines(path + "datiTempo" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListTempo);
                    File.AppendAllLines(path + "datiAcc" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAcc);
                    File.AppendAllLines(path + "datiAng" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAngl);

                }
            }
            if (liv == 5 || liv == 52)
            {
                if (liv != 0 && gm.inGame == true)
                {

                    List<string> ListTempo = new List<string>();
                    ListTempo.Add(Time.time.ToString());

                    accS = acc.ToString();
                    accS2 = acc2.ToString();
                    List<string> ListAcc = new List<string>();
                    ListAcc.Add(accS + ";       " + accS2 + ";      " + Time.time.ToString());

                    gammaS = gamma.ToString();
                    List<string> ListAngl = new List<string>();
                    ListAngl.Add(gammaS + ";        " + Time.time.ToString());          //inizializza gammaS

                    File.AppendAllLines(path + "datiTempo" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListTempo);
                    File.AppendAllLines(path + "datiAcc" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAcc);
                    File.AppendAllLines(path + "datiAng" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAngl);

                }
            }
            if (liv == 6)
            {
                if (liv != 0 && gm.inGame == true)
                {

                    List<string> ListTempo = new List<string>();
                    ListTempo.Add(Time.time.ToString());

                    accS = acc.ToString();
                    accS2 = acc2.ToString();
                    List<string> ListAcc = new List<string>();
                    ListAcc.Add(accS + ";       " + accS2 + ";      " + Time.time.ToString());

                    gammaS = gamma.ToString();
                    List<string> ListAngl = new List<string>();
                    ListAngl.Add(gammaS + ";        " + Time.time.ToString());          //inizializza gammaS

                    File.AppendAllLines(path + "datiTempo" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListTempo);
                    File.AppendAllLines(path + "datiAcc" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAcc);
                    File.AppendAllLines(path + "datiAng" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt", ListAngl);

                }
            }


            //if(paddle3.beta>=0)
            //paddle3.gamma = paddle3.alpha + paddle3.beta;
            //else
            //paddle3.gamma = PI - (paddle3.alpha - paddle3.beta);
        }


        public void DeleteFiles()
        {
            if (File.Exists(path + "datiAcc" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt"))
            {
                File.Delete(path + "datiAcc" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt");
                UnityEditor.AssetDatabase.Refresh();
            }
            if (File.Exists(path + "datiTemp" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt"))
            {
                File.Delete(path + "datiTemp" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt");
                UnityEditor.AssetDatabase.Refresh();
            }
            if (File.Exists(path + "datiAng" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt"))
            {
                File.Delete(path + "datiAng" + SaveID.saveID.ToString() + "_" + liv.ToString() + ".txt");
                UnityEditor.AssetDatabase.Refresh();
            }
        }




    }
}