using System;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float speedModifier;
    public float zoomModifier;
    public Camera cam;
    private Vector3 _lastPos = Vector2.zero;


    // Update is called once per frame
    void Update()
    {
        // Camera Movement
        if (Input.GetMouseButtonDown(0))
        {
            // save initial mouse position on first frame the mouse is pressed
            _lastPos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            // calculate camera movement depending on offset to last frame
            var offset = Input.mousePosition - _lastPos;
            _lastPos = Input.mousePosition;
            transform.position -= speedModifier * cam.orthographicSize * offset;
        }

        // Camera Zooming
        var pos = transform.position;
        var scrollDelta = Input.mouseScrollDelta.y;
        cam.orthographicSize -= scrollDelta * zoomModifier;
    }
}