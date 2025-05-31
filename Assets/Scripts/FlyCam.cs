using System;
using System.IO;
using TMPro;
using UnityEngine;

// ============================================================================
// FLYCAM CONTROLLER
// ============================================================================
// Free-flying camera controller with movement, mouse look, zoom, and screenshot functionality
// Provides FPS-style camera controls for scene navigation and inspection

public class FlyCam : MonoBehaviour
{
    // ========================================================================
    // MOVEMENT CONFIGURATION
    // ========================================================================

    [Header("Movement Settings")] [Tooltip("Base movement speed in units per second")]
    public float moveSpeed = 5f;

    [Tooltip("Mouse look sensitivity")] public float lookSpeed = 2f;

    // ========================================================================
    // ZOOM CONFIGURATION
    // ========================================================================

    [Header("Zoom Settings")] [Tooltip("Field of view when zooming")]
    public float zoomFOV = 30f;

    [Tooltip("Speed of zoom transition")] public float zoomSpeed = 5f;

    // ========================================================================
    // UI CONFIGURATION
    // ========================================================================

    [Header("UI Elements")] [Tooltip("Text component to display current speed")]
    public TextMeshProUGUI speedText;

    // ========================================================================
    // PRIVATE STATE
    // ========================================================================

    private float normalFOV;
    private float targetFOV;
    private Camera cam;
    private float rotationX, rotationY;

    // ========================================================================
    // UNITY LIFECYCLE
    // ========================================================================

    /// <summary>
    ///     Initialize camera settings and lock cursor
    /// </summary>
    private void Start()
    {
        cam = GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
        normalFOV = cam.fieldOfView;
        targetFOV = normalFOV;
    }

    /// <summary>
    ///     Handle all input and camera updates each frame
    /// </summary>
    private void Update()
    {
        HandleMovement();
        HandleMouseLook();
        HandleSpeedAdjustment();
        HandleZoom();
        HandleScreenshot();
        UpdateUI();
    }

    // ========================================================================
    // MOVEMENT HANDLING
    // ========================================================================

    /// <summary>
    ///     Process movement input and apply to camera transform
    /// </summary>
    private void HandleMovement()
    {
        var moveX = Input.GetAxis("Horizontal");
        var moveZ = Input.GetAxis("Vertical");
        var moveY = 0f;

        if (Input.GetKey(KeyCode.E)) moveY = 1f;
        if (Input.GetKey(KeyCode.Q)) moveY = -1f;

        var move = transform.right * moveX + transform.up * moveY + transform.forward * moveZ;
        var currentSpeed = moveSpeed;

        if (Input.GetKey(KeyCode.LeftShift)) currentSpeed *= 2f;

        transform.position += move * currentSpeed * Time.deltaTime;
    }

    // ========================================================================
    // CAMERA ROTATION
    // ========================================================================

    /// <summary>
    ///     Process mouse input for camera rotation with vertical clamping
    /// </summary>
    private void HandleMouseLook()
    {
        rotationX += Input.GetAxis("Mouse X") * lookSpeed;
        rotationY -= Input.GetAxis("Mouse Y") * lookSpeed;
        rotationY = Mathf.Clamp(rotationY, -90f, 90f);
        transform.localRotation = Quaternion.Euler(rotationY, rotationX, 0f);
    }

    // ========================================================================
    // SPEED CONTROL
    // ========================================================================

    /// <summary>
    ///     Adjust movement speed using mouse scroll wheel
    /// </summary>
    private void HandleSpeedAdjustment()
    {
        var scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            moveSpeed += scroll * 10f;
            moveSpeed = Mathf.Clamp(moveSpeed, 1f, 50f);
        }
    }

    // ========================================================================
    // ZOOM FUNCTIONALITY
    // ========================================================================

    /// <summary>
    ///     Handle zoom input and smooth FOV transitions
    /// </summary>
    private void HandleZoom()
    {
        targetFOV = Input.GetKey(KeyCode.F) ? zoomFOV : normalFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
    }

    // ========================================================================
    // SCREENSHOT CAPTURE
    // ========================================================================

    /// <summary>
    ///     Capture and save screenshot with timestamp
    /// </summary>
    private void HandleScreenshot()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            var filename = $"Technical Scene Screenshot {DateTime.Now:dd.MM.yyyy - HH.mm.ss}.png";

            var folder = Path.Combine(Application.dataPath, "../ScreenShots");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            var fullPath = Path.Combine(folder, filename);
            ScreenCapture.CaptureScreenshot(fullPath);

            Debug.Log("Screenshot saved: " + filename);
        }
    }

    // ========================================================================
    // UI UPDATES
    // ========================================================================

    /// <summary>
    ///     Update speed display text with current values
    /// </summary>
    private void UpdateUI()
    {
        if (speedText != null)
        {
            var boost = Input.GetKey(KeyCode.LeftShift) ? " (x2)" : "";
            speedText.text = $"FlyCam Speed: {moveSpeed:F1}{boost}";
        }
    }
}