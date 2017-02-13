using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour {

    public bool exited;
    public float cancelAxis;

    private void Start()
    {
        exited = false;
    }

    // Update is called once per frame
    void Update () {
        cancelAxis = Input.GetAxis("Cancel");
        if (cancelAxis > 0.5f)
        {
            exited = true;
            Application.Quit();
        }
	}
}
