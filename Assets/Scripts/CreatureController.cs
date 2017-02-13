using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureController : MonoBehaviour {

    private Rigidbody rb;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();

        Evolvable ev = GetComponent<Evolvable>();
        if (ev != null)
        {
            float[] genome = ev.genome;
            Vector3 selfKick = new Vector3();
            if (genome.Length > 0) selfKick.x = genome[0] / 50.0f;
            if (genome.Length > 1) selfKick.y = genome[1] / 50.0f;
            if (genome.Length > 2) selfKick.z = genome[2] / 50.0f;
            rb.velocity = rb.velocity + selfKick;
        }
	}

    void Update()
    {
        if (rb.position.y < -100) Destroy(gameObject);
    }

}
