using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public class FlyCam : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float lookSpeed = 2f;
    public float zoomFOV = 30f;
    public float zoomSpeed = 5f;
    public TextMeshProUGUI speedText;

    private float normalFOV;
    private float targetFOV;
    private Camera cam;
    private float rotationX, rotationY;

    void Start()
    {
        cam = GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
        normalFOV = cam.fieldOfView;
        targetFOV = normalFOV;
    }

    void Update()
    {
        // Movement
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        float moveY = 0f;

        if (Input.GetKey(KeyCode.E)) moveY = 1f;
        if (Input.GetKey(KeyCode.Q)) moveY = -1f;

        Vector3 move = (transform.right * moveX + transform.up * moveY + transform.forward * moveZ);
        float currentSpeed = moveSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed *= 2f; // Double speed when holding Left Shift
        }

        transform.position += move * currentSpeed * Time.deltaTime;


        // Mouse Look
        rotationX += Input.GetAxis("Mouse X") * lookSpeed;
        rotationY -= Input.GetAxis("Mouse Y") * lookSpeed;
        rotationY = Mathf.Clamp(rotationY, -90f, 90f);
        transform.localRotation = Quaternion.Euler(rotationY, rotationX, 0f);

        // Adjust speed with mouse scroll
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            moveSpeed += scroll * 10f;
            moveSpeed = Mathf.Clamp(moveSpeed, 1f, 50f); // Clamp
        }

        // Zoom
        targetFOV = Input.GetKey(KeyCode.F) ? zoomFOV : normalFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);

        // Screenshot
        if (Input.GetKeyDown(KeyCode.R))
        {
            string filename = $"Technical Scene Screenshot {System.DateTime.Now:dd.MM.yyyy - HH.mm.ss}.png";

            string folder = Path.Combine(Application.dataPath, "../ScreenShots");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            string fullPath = Path.Combine(folder, filename);
            ScreenCapture.CaptureScreenshot(fullPath);

            Debug.Log("Screenshot saved: " + filename);
        }

        // Speed text
        if (speedText != null)
        {
            string boost = Input.GetKey(KeyCode.LeftShift) ? " (x2)" : "";
            speedText.text = $"FlyCam Speed: {currentSpeed:F1}{boost}";
        }

    }
}
