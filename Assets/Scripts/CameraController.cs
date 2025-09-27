using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Vector3 fixedPosition = new Vector3(0, 0, -10); // Default stationary position
    [SerializeField] private float orthographicSize = 10f; // Adjust to fit scene

    private void Start()
    {
        transform.position = fixedPosition;
        Camera.main.orthographicSize = orthographicSize; 
        Debug.Log("CameraController Start - Position set to: " + fixedPosition + ", Size: " + orthographicSize);
    }

    private void LateUpdate()
    {
        // Keeping camera stationary
        transform.position = fixedPosition;

        #if UNITY_EDITOR
        Debug.Log("Camera LateUpdate - Position: " + transform.position + ", Active: " + gameObject.activeSelf);
        #endif
    }
}