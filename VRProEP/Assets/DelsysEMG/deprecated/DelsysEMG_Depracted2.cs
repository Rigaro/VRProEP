//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using System.Collections.Generic;

// TCP includes
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

// Threading includes
using System.Threading;
// Debug
using UnityEngine;


public class DelsysEMG_Depracted2
{
    //example of creating a list of sensor types to keep track of various TCP streams...
    enum SensorTypes { SensorTrigno, SensorTrignoImu, SensorTrignoMiniHead, NoSensor };
    private List<SensorTypes> _sensors = new List<SensorTypes>();
    private Dictionary<string, SensorTypes> sensorList = new Dictionary<string, SensorTypes>();
    // Active sensor channel
    private List<int> activeSensorChannels = new List<int>();

    //For TCP/IP connections
    private TcpClient commandSocket;
    private TcpClient emgSocket;
    private const int commandPort = 50040;  //server command port
    private const int emgDataPort = 50043; //server emg data port

    //The following are streams and readers/writers for communication
    private NetworkStream commandStream;
    private NetworkStream emgStream;
    private StreamReader commandReader;
    private StreamWriter commandWriter;


    //Server commands
    private const string COMMAND_QUIT = "QUIT";
    private const string COMMAND_GETTRIGGERS = "TRIGGER?";
    private const string COMMAND_SETSTARTTRIGGER = "TRIGGER START";
    private const string COMMAND_SETSTOPTRIGGER = "TRIGGER STOP";
    private const string COMMAND_START = "START";
    private const string COMMAND_STOP = "STOP";
    private const string COMMAND_SENSOR_TYPE = "TYPE?";


    //Threads for acquiring emg and acc data
    //private Timer emgTimer;
    private Thread emgThread;
    //private long t0_in_ticks;

    //The following are storage for acquired data
    private List<float>[] emgDataList = new List<float>[16];
    private float[] tempEmgDataList = new float[16];
    private List<float> timeStampList = new List<float>();

    //Sensor status
    private bool connected = false; //true if connected to server
    private bool running = false;   //true when acquiring data
    private bool recording = false; //for EMG data recording


    //Saving file settings
    StringBuilder csvEMG = new StringBuilder();
    String Filename;

    #region Initialization and connection
    //Initialization
    public void Init()
    {
        //t0_in_ticks = t0; //Initialize start time tick

        sensorList.Add("A", SensorTypes.SensorTrigno);
        sensorList.Add("D", SensorTypes.SensorTrigno);
        sensorList.Add("L", SensorTypes.SensorTrignoImu);
        sensorList.Add("J", SensorTypes.SensorTrignoMiniHead);
        sensorList.Add("O", SensorTypes.SensorTrignoImu);

        Debug.Log("Delsys-> Initialize DelsysEMG");
    }

    

    //Establish sensors connnection
    public bool Connect()
    {
        try
        {
            //Establish TCP/IP connection to server using URL entered
            commandSocket = new TcpClient("localhost", commandPort);

            //Set up communication streams
            commandStream = commandSocket.GetStream();
            commandReader = new StreamReader(commandStream, Encoding.ASCII);
            commandWriter = new StreamWriter(commandStream, Encoding.ASCII);

            //Get initial response from server and display
            commandReader.ReadLine();
            commandReader.ReadLine();   //get extra line terminator
            connected = true;   //indicate that we are connected
        }
        catch (Exception connectException)
        {
            //connection failed, display error message
            Debug.Log("Delsys-> Could not connect.\n" + connectException.Message);
            connected = false;
            return connected;
        }

        //build a list of connected sensor types
        _sensors = new List<SensorTypes>();
        for (int i = 1; i <= 16; i++)
        {
            string query = "SENSOR " + i + " " + COMMAND_SENSOR_TYPE;
            string response = SendCommand(query);
            Debug.Log("Delsys-> " + query);
            Debug.Log("<- Server Delsys " + response);
            _sensors.Add(response.Contains("INVALID") ? SensorTypes.NoSensor : sensorList[response]);
        }

        // Add the active sensor channel to a list for query
        for (int i = 0; i < 16; i++)
        {
            if (_sensors[i] == SensorTypes.SensorTrignoImu)
            {
                activeSensorChannels.Add(i + 1);
            }
        }

        SendCommand("UPSAMPLE OFF");


        Debug.Log("Delsys-> Connected EMG sensor number: " + GetNbActiveSensors());
        foreach (int element in GetChannelsActiveSensor())
        {
            Debug.Log("Delsys-> Connected EMG sensor channel: " + element);
        }

        return connected;
    }

    // Check time interval
    public float CheckSamplingRate()
    {
        float samplingRate;

        string interval = SendCommand("FRAME INTERVAL?");
        string maxSample = SendCommand("MAX SAMPLES EMG?");

        //Debug.Log(interval);
        //Debug.Log(maxSample);

        samplingRate = float.Parse(interval) / float.Parse(maxSample);
        return samplingRate;
    }

    //Close connection
    public void Close()
    {
        //Check if running and display error message if not
        if (running)
        {
            Debug.Log("Delsys-> Can't quit while acquiring data!");
            return;
        }

        //send QUIT command
        SendCommand(COMMAND_QUIT);
        connected = false;  //no longer connected

        //Close all streams and connections
        commandReader.Close();
        commandWriter.Close();
        commandStream.Close();
        commandSocket.Close();
        emgStream.Close();
        emgSocket.Close();

        Debug.Log("Delsys-> Disconnect from server and quit!");
    }


    #endregion

    #region Data acquisition

    //Start acquisition
    public void StartAcquisition()
    {
        if (!connected)
        {
            Debug.Log("Delsys-> EMG Not connected.");
            return;
        }

        

        for (int i = 0; i < 16; i++)
        {
            emgDataList[i] = new List<float>();
        }

        //Establish data connections and creat streams
        emgSocket = new TcpClient("localhost", emgDataPort);

        //emgStream = emgSocket.GetStream();
        emgStream = emgSocket.GetStream();

        //Create data acquisition threads
        emgThread = new Thread(ImuEmgThreadRoutine);
        emgThread.IsBackground = true;



        
        //Indicate we are running and start up the acquisition threads
        running = true;
        //emgTimer = new Timer(new TimerCallback(this.ImuEmgThreadRoutine), null, new TimeSpan(10000), new TimeSpan(10000));

        emgThread.Start();


        //Send start command to server to stream data
        string response = SendCommand(COMMAND_START);

        //check response
        if (response.StartsWith("OK"))
        {
            Debug.Log("Delsys-> Server OK to start acquisition!");
        }
        else
        {
            running = false;    //stop threads
            Debug.Log("Delsys-> Server ERROR to start acquisition!");
        }

        
    }

    //Stop acquisition and write to file
    public void StopAcquisition()
    {
        running = false;    //no longer running
                            //Wait for threads to terminate
        emgThread.Join();

        //Send stop command to server
        string response = SendCommand(COMMAND_STOP);
        if (!response.StartsWith("OK"))
            Debug.Log("Delsys->: Server failed to stop. Further actions may fail.");
        else
            Debug.Log("Delsys->: Server stops!");
    }

    

    //Start log to csv, set the filename directly
    public void StartRecording(String filename)
    {
       

        //write data to csv
        csvEMG = new StringBuilder();

        Filename = filename;
        recording = true;
        //Clear lists imuEmgDataList[sn] emgDataList[sn] x16
        for (int i = 0; i < 16; i++)
        {
            emgDataList[i].Clear();
        }
        timeStampList.Clear();

        csvEMG.Length = 0;
        csvEMG.Capacity = 0;
        Debug.Log("Delsys-> Recording...");
    }

    // Create log file
    private void CreateLogFile(String fileName)
    { 
        
    }
    
    // Stream data logger
    private void StreamDataLogger()
    {


    }


    // Thread for imu emg acquisition
    private void ImuEmgThreadRoutine()
    {
        emgStream.ReadTimeout = 1000;    //set timeout

        BinaryReader reader = new BinaryReader(emgStream);
        while (running)
        {
            try
            {
                // Stream the data;
                for (int sn = 0; sn < 16; ++sn)
                    tempEmgDataList[sn] = reader.ReadSingle();

                // Record the data if recording
                if (recording)
                {
                    //timeStampList.Add((float)((DateTime.Now.Ticks - t0_in_ticks) / (float)10000000));
                    double time = (timeStampList.Count - 1) * 0.0009;
                    timeStampList.Add((float)time);
                    for (int sn = 0; sn < 16; ++sn)
                        emgDataList[sn].Add(tempEmgDataList[sn]);
                }

            }
            catch (IOException e)
            {
                Debug.Log(e.ToString());
            }
        }

    }

    //Stop log to csv and stop 
    public void StopRecording()
    {
        while (emgStream.DataAvailable)
        {
            Debug.Log("Data still streaming");
        }// block until all data are recorded

        recording = false;

        Debug.Log("Delsys-> " + emgDataList[0].Count + " samples recorded.");

        //Write header to file
        string header = "Time,";

        for (int i = 0; i < 16; i++) //emgDataList will contain the most samples
        {
            //Write sensors values
            if (_sensors[i] == SensorTypes.SensorTrignoImu)
            {
                header += "No. " + (i+1).ToString() +",";
            }
        }
        csvEMG.Append(header);
        csvEMG.Append(Environment.NewLine);


        //Write to file
        int imEmgIndex = 0;
        float tStart = timeStampList[imEmgIndex];
        
        while (imEmgIndex < emgDataList[0].Count - 1)
        {
            //Write time stamps
            csvEMG.Append(timeStampList[imEmgIndex]- tStart + ",");

            for (int i = 0; i < 16; i++) //emgDataList will contain the most samples
            {
                //Write sensors values
                if (_sensors[i] == SensorTypes.SensorTrignoImu)
                {
                    csvEMG.Append(emgDataList[i][imEmgIndex] + ",");
                }
            }
            imEmgIndex++;
            csvEMG.Remove(csvEMG.Length - 1, 1);
            csvEMG.Append(Environment.NewLine);
        }

        File.WriteAllText(Filename, csvEMG.ToString());
    }




    #endregion

    #region Get sensor status and readings
    //Return the latest readings
    public float[] GetRawEMGData()
    {
        float[] data = new float[activeSensorChannels.Count];

        int activeSensroNb = 0;
        foreach (int element in activeSensorChannels)
        {
            data[activeSensroNb] = tempEmgDataList[element-1];
            activeSensroNb++;
        }

        return data;
    }

    //Return the number of active sensor
    public int GetNbActiveSensors(){ return activeSensorChannels.Count;}

    //Set channels of active sensor
    public int[] GetChannelsActiveSensor(){ return activeSensorChannels.ToArray(); }

    // Return status
    public bool IsConnected() { return connected; }
    public bool IsRunning() { return running; }
   
    #endregion


    //Send a command to the server and get the response
    private string SendCommand(string command)
    {
        string response = "";

        //Check if connected
        if (connected)
        {
            //Send the command
            commandWriter.WriteLine(command);
            commandWriter.WriteLine();  //terminate command
            commandWriter.Flush();  //make sure command is sent immediately

            //Read the response line and display    
            response = commandReader.ReadLine();
            commandReader.ReadLine();   //get extra line terminator
        }
        else
            Debug.Log("Delsys-> EMG: Not connected.");
        return response;    //return the response we got
    }


   


}
