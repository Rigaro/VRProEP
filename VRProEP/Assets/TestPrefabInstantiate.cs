using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPrefabInstantiate : MonoBehaviour {

    public Transform activeTracker;
    public GameObject residualLimbPrefab;
	// Use this for initialization
	void Start () {
        Instantiate(residualLimbPrefab, activeTracker);
	}
	
}
