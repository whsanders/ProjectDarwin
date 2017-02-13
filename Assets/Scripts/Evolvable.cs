using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Evolvable : MonoBehaviour {

    public Dictionary<string, float> fitness;
    public float[] genome;
    public int activeGenes;
    public EvolvableSpawner reportTo;
    public float age;
    public float ageFitnessMultiplier = 1.0f;
    public float maxAge = 30.0f;

	// Use this for initialization
	void Start () {
        fitness = new Dictionary<string, float>();
        age = 0.0f;
	}
	
	// Update is called once per frame
	void Update () {
        age += Time.deltaTime;
        if (age > maxAge) Destroy(gameObject);
	}

    private void OnDestroy()
    {
        if (reportTo != null)
        {
            fitness["ObjectAge"] = age * ageFitnessMultiplier;
            float totalFitness = 0.0f;
            foreach (KeyValuePair<string, float> factor in fitness) {
                totalFitness += factor.Value;
            }
            reportTo.ReportFitness(totalFitness, genome);
        }
    }
}
