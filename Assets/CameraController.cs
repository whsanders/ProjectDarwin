using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    private Camera camera;
    public float vert;
    public float horiz;
    public float panSpeed;

	// Use this for initialization
	void Start () {
        camera = GetComponent<Camera>();
        if (camera == null) Debug.LogError("CameraController must have a Camera!");
	}
	
	// Update is called once per frame
	void Update () {
        vert = Input.GetAxis("Vertical");
        horiz = Input.GetAxis("Horizontal");
        Vector3 p = transform.position;
        float maxPan = panSpeed * Time.deltaTime;
        transform.position = new Vector3(p.x + horiz * maxPan, p.y, p.z + vert * maxPan);
	}
}
