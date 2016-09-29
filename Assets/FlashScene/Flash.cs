using UnityEngine;
using System.Collections;
using Assets.LSL4Unity;
using Assets.LSL4Unity.Scripts;

public class Flash : MonoBehaviour
{

    public FlaschScene experiment; 

    void OnEnable()
    {
        experiment.writeMarker("Flash Enabled");
    }

    public void OnDisable()
    {
        experiment.writeMarker("Flash disabled");
    }

    public void OnRenderObject()
    {
        experiment.writeMarker("OnRenderObject");
    }
}
