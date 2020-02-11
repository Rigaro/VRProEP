using System.Collections;
using System.Collections.Generic;
using VRProEP.ExperimentCore;
using UnityEngine;

public class ExperimentLoader : MonoBehaviour
{
    [SerializeField]
    private bool debug = false;
    
    // Start is called before the first frame update
    void Start()
    {
        if (!debug)
        {
            // Get the name of the active experiment from Experiment System
            string experimentName = ExperimentSystem.ActiveExperimentID;

            // Load prefab from Resources folder.
            GameObject experimentPrefab = Resources.Load<GameObject>("Experiments/" + experimentName);
            if (experimentPrefab == null)
                throw new System.Exception("The requested experiment was not found.");

            // Instantiate experiment
            Instantiate(experimentPrefab);
        }
    }
}
