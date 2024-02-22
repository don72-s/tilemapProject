using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraGazer : MonoBehaviour
{
    // Start is called before the first frame update

/*    public GameObject hole;
*/
    // Update is called once per frame
    void Update()
    {
/*        transform.rotation = hole.transform.rotation;
*/        Quaternion tq = Camera.main.transform.rotation;
        tq.x = 0;
        tq.z = 0;
        transform.rotation = tq;
    }
}
