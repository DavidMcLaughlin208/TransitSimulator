using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class CameraController : MonoBehaviour
{
    Datastore datastore;
    public bool dragging = false;
    public Vector3 dragOrigin;
    private float dragSpeed = 20;

    public float cameraSizeMax = 25;
    public float cameraSizeMin = 2;
    public float cameraMoveFactor = 20;

    public Camera mainCamera;
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GetComponent<Camera>();
        datastore = GameObject.Find("God").GetComponent<Datastore>();

        datastore.inputEvents.Receive<ClickEvent>()
            .Where(_ => datastore.activeTool.Value == ToolType.HAND)
            .Subscribe(e =>
            {
                dragOrigin = Input.mousePosition;
                dragging = true;
            });

        datastore.inputEvents.Receive<MouseUpEvent>()
            .Where(_ => datastore.activeTool.Value == ToolType.HAND)
            .Subscribe(e =>
            {
                dragging = false;
            });

        datastore.inputEvents.Receive<MouseMoveEvent>()
            .Where(_ => datastore.activeTool.Value == ToolType.HAND && dragging)
            .Subscribe(e =>
            {
                Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
                Vector3 move = new Vector3(pos.x * dragSpeed, pos.y * dragSpeed, 0);
                transform.position -= move;
                dragOrigin = Input.mousePosition;
            });


    }

    // Update is called once per frame
    void Update()
    {
        float deltaY = Input.mouseScrollDelta.y;
        //if ((deltaY < 0 && mainCamera.orthographicSize < cameraSizeMax) || (deltaY > 0 && mainCamera.orthographicSize > cameraSizeMin))
        //{
        //    Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        //    Vector3 center = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        //    Vector3 currentPosition = transform.position;
        //    Debug.Log("CurrentPosition: " + currentPosition.x + ", " + currentPosition.y);
        //    Debug.Log("MousePosition: " + mousePosition.x + ", " + mousePosition.y);
        //    currentPosition.z = -10;
        //    Vector3 heading = mousePosition - center;
        //    Vector3 normalizedDir = heading / heading.magnitude;

        //    transform.position = currentPosition + normalizedDir * cameraMoveFactor * Time.deltaTime;

        //}
        mainCamera.orthographicSize = Mathf.Min(cameraSizeMax, Mathf.Max(cameraSizeMin, mainCamera.orthographicSize - deltaY));
    }
}
