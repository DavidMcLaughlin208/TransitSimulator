using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float cameraSizeMax = 25;
    public float cameraSizeMin = 25;
    public float cameraMoveFactor = 10;

    public Camera mainCamera;
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GetComponent<Camera>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
        

        //float deltaY = Input.mouseScrollDelta.y;
        //if (deltaY != 0)
        //{
        //    Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        //    Vector3 currentPosition = transform.position;
        //    currentPosition.z = -10;
        //    Vector3 heading = mousePosition - currentPosition;
        //    Vector3 normalizedDir = heading / heading.magnitude;

        //    transform.position = currentPosition + normalizedDir * cameraMoveFactor * Time.deltaTime;
            
        //}
        //mainCamera.orthographicSize = Mathf.Min(cameraSizeMax, Mathf.Max(cameraSizeMin, mainCamera.orthographicSize - deltaY));
        //mainCamera.orthographicSize -= deltaY;

    }
}
