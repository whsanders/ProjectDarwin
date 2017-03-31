using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureController : MonoBehaviour {

    private Rigidbody rb;
    public float maxKickEachAxis = 20.0f;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();

        Evolvable ev = GetComponent<Evolvable>();
        if (ev != null)
        {
            float[] genes = ev.genome.Read();
            Vector3 selfKick = new Vector3();
            if (genes.Length > 0) selfKick.x = (genes[0] - 0.5f) * 2.0f * maxKickEachAxis;
            if (genes.Length > 1) selfKick.y = (genes[1] - 0.5f) * 2.0f * maxKickEachAxis;
            if (genes.Length > 2) selfKick.z = (genes[2] - 0.5f) * 2.0f * maxKickEachAxis;
            rb.velocity = rb.velocity + selfKick;
        }
	}

    void Update()
    {
        if (rb.position.y < -100) Destroy(gameObject);
    }

}
