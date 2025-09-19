using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.UIElements;

public class ZoomCamera : MonoBehaviour
{
    public CinemachineCamera cineCam;
    public float zoomStep = 1f;
    public float minZoom = 2f;
    public float maxZoom = 10f;

    private float currentZoom;

    void Start()
    {
        currentZoom = cineCam.Lens.OrthographicSize;
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f) // zoom in
            currentZoom -= zoomStep;
        else if (scroll < 0f) // zoom out
            currentZoom += zoomStep;

        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

        cineCam.Lens.OrthographicSize = currentZoom;
    }
}
