using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvolvableTarget : MonoBehaviour {

    public float agePenaltyMultiplier = 1.0f;
    public float distanceFitnessMultiplier = 100.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerStay(Collider other)
    {
        Evolvable ev = other.GetComponent<Evolvable>();
        if (ev != null)
        {
            Vector3 toOther = other.transform.position - transform.position;
            float distance = toOther.magnitude;
            float distanceFactor = 1.0f / (1.0f + distance);
            float fitnessFactor = (distanceFactor * distanceFitnessMultiplier) - (ev.age * agePenaltyMultiplier);
            if (!ev.fitness.ContainsKey("TargetDistance") || fitnessFactor > ev.fitness["TargetDistance"])
            {
                ev.fitness["TargetDistance"] = fitnessFactor;
            }
        }
    }
}
