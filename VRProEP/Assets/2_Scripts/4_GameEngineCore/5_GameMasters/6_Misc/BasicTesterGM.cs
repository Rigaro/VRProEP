using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VRProEP.Utilities;
using VRProEP.GameEngineCore;
using VRProEP.ProsthesisCore;

public class BasicTesterGM : MonoBehaviour
{
    public string ipAddress = "192.168.137.75";
    public int port = 2390;
    public bool sendData = false;
    public float frequency = 1000.0f;
    public float amplitude = 0.5f;
    public Transform cube;
    [Range(0.0f, 1.0f)]
    public float force;
    [Range(0.0f, 1.0f)]
    public float roughness;

    private UDPWriter writer;
    private BoniManager boniManager;
    // Start is called before the first frame update
    void Start()
    {
        /*
        writer = new UDPWriter(ipAddress, port, "Boni");
        */
        SaveSystem.LoadUserData("MD1942");
        float[] xBar = new float[] { 1000.0f, 0.0f, 1.0f };
        BoneConductionReferenceGenerator bcRG = new BoneConductionReferenceGenerator(xBar, (BoneConductionCharacterization)SaveSystem.LoadFeedbackCharacterization(SaveSystem.ActiveUser.id, FeedbackType.BoneConduction));
        BoneConductionController bcC = new BoneConductionController(ipAddress, port, 1, 1, 1);
        boniManager = new BoniManager(bcC, bcRG);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        roughness += 0.01f;
        float[] sensorData = new float[] { roughness, force, 1.0f };
        boniManager.UpdateFeedback(0, sensorData);
        /*
        if (sendData)
        {
            float amp = amplitude*Mathf.Sin(3 * 2 * Mathf.PI * Time.time) + 0.5f;
            cube.position = new Vector3(cube.position.x, amp, cube.position.z);
            List<float> data = new List<float>();
            data.Add(frequency);
            data.Add(amp);
            writer.SendData(data);
        }
        */
    }

    private void OnApplicationQuit()
    {
        float[] sensorData = new float[] { 0.0f, 0.0f, 1.0f };
        boniManager.UpdateFeedback(0, sensorData);
    }

    public void SetRoughness(float roughness)
    {
        this.roughness = roughness;
    }
}
