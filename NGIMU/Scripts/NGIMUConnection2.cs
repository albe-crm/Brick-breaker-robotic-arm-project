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
    public class NGIMUConnection2 : MonoBehaviour
    {
        Vector3 LastAcc;
        Vector3 LastPos;
        Vector3 LastVel;
        Vector3 LastVelFilt;
        Vector3 Acc;
        Vector3 Vel;
        Vector3 VelFilt;
        Vector3 Pos;
        public double LastAccZ;
        public double LastVelZ;
        public double LastPosZ;
        public double AccZ;
        public double Velx;
        public double Vely;
        public double VelZ;
        public double Posx;
        public double Posy;
        public double PosZ;
        public double LastVelZFilt;
        public double VelZcalc;
        public double AccZRaw;
        public double AccZcalc;
        public double PosZcalc;
        Quaternion Quat;
        Vector3 AccRaw;
        Vector3 AccEarth;
        Vector3 AccLinear;
        Quaternion LastQuat;
        ButterworthFilter lowpassfilter;
        ButterworthFilter highpassfilterVel;
        ButterworthFilter highpassfilterPos;
        public double meanZ;
        public double dt;
        public double g;
        // Matrix4x4 RotationMatrix;
        public GameManager gm;

        

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
            //Initialize the acceleration vector
            LastAcc = new Vector3(0.0f, 0.0f, 0.0f);
            //Initialize the velocity vector
            LastVel = new Vector3(0.0f, 0.0f, 0.0f);
            //Initialize the velocity vector
            LastVelFilt = new Vector3(0.0f, 0.0f, 0.0f);
            //Initialize the position vector
            LastPos = new Vector3(0.0f, 0.0f, 0.0f);
            LastAccZ = 0.0;
            LastVelZ = 0.0;
            LastPosZ = 0.0;
            LastVelZFilt = 0.0;
            VelZcalc = 0.0;
            AccZRaw = 0.0;
            AccZcalc = 0.0;
            PosZcalc = 0.0;
            AccZ = 0.0;
            Velx = 0.0;
            Vely = 0.0;
            VelZ = 0.0;
            Posx = 0.0;
            Posy = 0.0;
            PosZ = 0.0;
            meanZ = - 0.0184;
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
            ModelTransform.position = new Vector3 (0f,0f,0f);
        }
        private static readonly NgimuApi.Maths.Quaternion zero = new NgimuApi.Maths.Quaternion(0, 0, 0, 0);
        private static readonly NgimuApi.Maths.Vector3 inizio = new NgimuApi.Maths.Vector3(0,0,0);

        [Header("Header")]

        
        [Header("Model")]
        [SerializeField, Tooltip("Model to transform", order = 10)] public Transform ModelTransform;
        
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

            connection = new Connection(connectionInfo);

            connection.Connected += reporter.OnConnected;
            connection.Disconnected += reporter.OnDisconnected;
            connection.Error += ConnectionOnError;
            connection.Exception += ConnectionOnException;
            connection.Info += reporter.OnInfo;
            connection.Message += reporter.OnMessage;
            connection.UnknownAddress += reporter.OnUnknownAddress;

            connection.Connect();

            connection.Settings.ReadAync(new ISettingItem[] {connection.Settings}, reporter, 100, 3);
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
            gm.Back();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) == true)
            {
                Disconnect();
                SceneManager.LoadScene("3");
            }


            if (connection == null)
            {
                return;
            }
        
            if (connection.Quaternion.Quaternion != zero)
            {
                Quat = NgimuMathUtils.NgimuToUnityQuaternion(connection.Quaternion.Quaternion);                 // Quaternion from IMU
                //AccLinear = NgimuMathUtils.NgimuToUnityVector(connection.LinearAcceleration.LinearAcceleration);   // Linear Acceleration from IMU
                AccEarth = NgimuMathUtils.NgimuToUnityVector(connection.EarthAcceleration.EarthAcceleration);   // Earth Acceleration from IMU
                //string folder_path = "C:\\Users\\15-EG1016nl\\OneDrive - Politecnico di Milano\\Sara Passerini - Master thesis\\unity\\NGIMU\\HAND\\Assets\\NGIMU\\Scripts\\";

                //printf("Sara ")
                AccZRaw = (AccEarth[2] - meanZ);

                // Filter the input data                
                AccZcalc = lowpassfilter.Filter(AccZRaw); // Che in realtà è un highpass
                AccZ = AccZcalc*g; 
/*
                if (Math.Abs(AccZcalc) > 0.0450)
                {   
                    VelZ = LastVelZ + (AccZ + LastAccZ)*dt/2.0; // z velocity
                }
                else
                {   
                    VelZ = LastVelZ;
                }
                Non usiamo questa condizione sull'accelerazione perché altrimenti mi rimane un residuo di velocità se supero la threshold che mi fa driftare la posizione
*/
                VelZ = LastVelZ + (AccZ + LastAccZ)*dt/2.0; // z velocity

                // highpass filter velocity to remove drift
                VelZcalc = highpassfilterVel.Filter(VelZ);

                if (Math.Abs(VelZ) < 0.2000)
                {
                    PosZ = LastPosZ;
                }
                else
                {
                    PosZ = LastPosZ + (VelZcalc + LastVelZFilt)*dt/2.0; // z position
                }

                //PosZcalc = highpassfilterPos.Filter(PosZ);

                /* // Saving
                 // Save raw acceleration
                 List<string> SmyListAccZ = new List<string>();
                 List<double> myListAccZ = new List<double>();

                 string sAccZ = AccZRaw.ToString("0.000000000");
                 SmyListAccZ.Add(sAccZ);
                 myListAccZ.Add(AccZRaw);

                 System.IO.File.AppendAllLines(folder_path + "testAccZ.txt",SmyListAccZ);

                 List<string> SmyListAccY = new List<string>();
                 List<double> myListAccY = new List<double>();

                 string sAccY = AccEarth[1].ToString("0.000000000");
                 SmyListAccY.Add(sAccY);
                 myListAccY.Add(AccEarth[1]);

                 System.IO.File.AppendAllLines(folder_path + "testAccY.txt",SmyListAccY);

                 List<string> SmyListAccX = new List<string>();
                 List<double> myListAccX= new List<double>();

                 string sAccX = AccEarth[0].ToString("0.000000000");
                 SmyListAccX.Add(sAccX);
                 myListAccX.Add(AccEarth[0]);

                 System.IO.File.AppendAllLines(folder_path + "testAccX.txt",SmyListAccX);

                 // Save filtered acceleration           
                 List<string> SmyListAccZcalc = new List<string>();
                 List<double> myListAccZcalc = new List<double>();

                 string sAccZcalc = AccZcalc.ToString("0.000000000");
                 SmyListAccZcalc.Add(sAccZcalc);
                 myListAccZcalc.Add(AccZcalc);

                 System.IO.File.AppendAllLines(folder_path + "testAccZcalc.txt",SmyListAccZcalc);

                 // Save raw velocity
                 List<string> SmyListVelZ = new List<string>();
                 List<double> myListVelZ = new List<double>();

                 string sVelZ = VelZ.ToString("0.000000000");
                 SmyListVelZ.Add(sVelZ);
                 myListVelZ.Add(VelZ);

                 System.IO.File.AppendAllLines(folder_path + "testVelZ.txt",SmyListVelZ);

                 // Save filtered velocity           
                 List<string> SmyListVelZcalc = new List<string>();
                 List<double> myListVelZcalc = new List<double>();

                 string sVelZcalc = VelZcalc.ToString("0.000000000");
                 SmyListVelZcalc.Add(sVelZcalc);
                 myListVelZcalc.Add(VelZcalc);

                 System.IO.File.AppendAllLines(folder_path + "testVelZcalc.txt",SmyListVelZcalc);

                 // Save raw position           
                 List<string> SmyListPosZ = new List<string>();
                 List<double> myListPosZ = new List<double>();

                 string sPosZ = PosZ.ToString("0.000000000");
                 SmyListPosZ.Add(sPosZ);
                 myListPosZ.Add(PosZ);

                 System.IO.File.AppendAllLines(folder_path + "testPosZ.txt",SmyListPosZ);

                 // Save filtered position           
                 List<string> SmyListPosZcalc = new List<string>();
                 List<double> myListPosZcalc = new List<double>();

                 string sPosZcalc = PosZcalc.ToString("0.000000000");
                 SmyListPosZcalc.Add(sPosZcalc);
                 myListPosZcalc.Add(PosZcalc);

                 System.IO.File.AppendAllLines(folder_path + "testPosZcalc.txt",SmyListPosZcalc);*/

                // Creo i vettori velocità e posizione
                //Vel = new Vector3 ((float)Velx,(float)Vely,(float)VelZ);
                //VelFilt = new Vector3 ((float)Velx,(float)Vely,(float)VelZcalc);
                //Pos = new Vector3((float)Posx,(float)Posy,(float)PosZ); // Position in Earth reference frame


                Vector3 Euler = Quat.eulerAngles;

                double PosZGame = PosZ * 40;
                
                
                // Read values on Unity
              //  ModelTransform.rotation = new Quaternion(0.707f,0.0f,0.0f,0.707f); // non ci interessa la rotazione
                                                                            // ModelTransform.position = new Vector3(0.0f,(float)PosZGame,0f);
                ModelTransform.position = new Vector3(9f, (Euler.x / 45) * 9f - 4.5f, 0f);
                //Debug.Log("x " + Euler.x);
                //Debug.Log("y " + Euler.y);
                //Debug.Log("z " + Euler.z);


                // Update acceleration and velocity values
                LastAccZ = AccZ;
                LastVelZ = VelZ;
                LastVelZFilt = VelZcalc;
                LastPosZ = PosZ;
                LastQuat = Quat;
            }
        }
    }
}