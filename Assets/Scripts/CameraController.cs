using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    private Camera playerCamera;
    public float panSpeed = 1.0f;
    public float edgeStickiness = 0.2f;
    public GroundArea bindToGroundArea;
    public Rect area = Rect.zero;
    private Vector3 offset;

	// Use this for initialization
	void Start () {
        playerCamera = GetComponent<Camera>();
        if (playerCamera == null) Debug.LogError("CameraController must have a Camera!");
        offset = new Vector3(0.0f, 0.0f, transform.position.z);
        if (bindToGroundArea != null)
        {
            Vector2 center = new Vector2(bindToGroundArea.transform.position.x, bindToGroundArea.transform.position.z);
            area = new Rect(center - bindToGroundArea.areaSize / 2, bindToGroundArea.areaSize);
        }
	}
	
	// Update is called once per frame
	void Update () {
        Move();
	}

    private void Move()
    {
        float vert = Input.GetAxis("Vertical");
        float horiz = Input.GetAxis("Horizontal");
        Vector3 p = transform.position - offset;
        float maxPan = panSpeed * Time.deltaTime;
        Vector2 edgeMultiplier = new Vector2(1.0f, 1.0f);
        if (area != Rect.zero)
        {
            float stickyZone = panSpeed * edgeStickiness;
            if (horiz < 0.0f && p.x < area.x + stickyZone) edgeMultiplier.x = p.x < area.x - stickyZone ? -1.0f : (p.x - area.x) / stickyZone;
            if (horiz > 0.0f && p.x > area.x + area.width - stickyZone) edgeMultiplier.x = p.x > area.x + area.width + stickyZone ? -1.0f : (area.x + area.width - p.x) / stickyZone;
            if (vert < 0.0f && p.z < area.y + stickyZone) edgeMultiplier.y = p.z < area.y - stickyZone ? -1.0f : (p.z - area.y) / stickyZone;
            if (vert > 0.0f && p.z > area.y + area.height - stickyZone) edgeMultiplier.y = p.z > area.y + area.height + stickyZone ? -1.0f : (area.y + area.height - p.z) / stickyZone;
        }
        transform.position = new Vector3(p.x + horiz * maxPan * edgeMultiplier.x, p.y, p.z + vert * maxPan * edgeMultiplier.y) + offset;
    }
}
