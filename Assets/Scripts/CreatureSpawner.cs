using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureSpawner : MonoBehaviour {

    public int spawnCount;
    public float elapsedDeltaTime;
    public float howOftenToSpawn;
    public CreatureController thingToSpawn;
    public float lastSpawnTime;
    public Vector3 spawnVelocity;
    public float velocityVariance = 0.001f;
    public string status;
    public Vector3 lastAppliedVelocity;

	// Use this for initialization
	void Start () {
        spawnCount = 0;
        elapsedDeltaTime = 0.0f;
        lastSpawnTime = elapsedDeltaTime;
        if (velocityVariance < 0) velocityVariance = -velocityVariance;
	}
	
	// Update is called once per frame
	void Update () {
        float dt = Time.deltaTime;
        elapsedDeltaTime += dt;
        if (elapsedDeltaTime > lastSpawnTime + howOftenToSpawn)
        {
            spawnCount++;
            lastSpawnTime = elapsedDeltaTime;
            Vector3 velocity = new Vector3(
                spawnVelocity.x + Random.Range(-velocityVariance, velocityVariance),
                spawnVelocity.y + Random.Range(-velocityVariance, velocityVariance),
                spawnVelocity.z + Random.Range(-velocityVariance, velocityVariance)
            );
            CreatureController obj = Instantiate<CreatureController>(thingToSpawn, transform.position, transform.rotation);
            lastAppliedVelocity = velocity;
            obj.GetComponent<Rigidbody>().velocity = velocity;
            obj.name = "Creature" + spawnCount.ToString();
        }
    }
}
