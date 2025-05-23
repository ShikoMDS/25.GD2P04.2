using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarCameraFollow : MonoBehaviour
{
    public Transform cameraTransform;

    void LateUpdate()
    {
        if (cameraTransform != null)
        {
            transform.position = cameraTransform.position;
        }
    }
}
