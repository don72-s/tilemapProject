using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraGazer : MonoBehaviour
{

    void Update()
    {
        Quaternion q = Camera.main.transform.rotation;
        q.x = 0;
        q.z = 0;
        transform.rotation = q;
    }
}
