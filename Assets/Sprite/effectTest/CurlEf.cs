using UnityEngine;
using System.Collections;

[ExecuteInEditMode] 
public class CurlEf : MonoBehaviour {

    public Transform _Front; 
    public Transform _Mask;
    public Transform _GradOutter;
    public Vector3 _Pos = new Vector3(200, 200, 0.0f);



    void LateUpdate() { 
        transform.position = _Pos;
        transform.eulerAngles = Vector3.zero;
        Vector3 pos = _Front.position/* - zero */;

        float theta = Mathf.Atan2(pos.y, pos.x) * 180.0f / Mathf.PI;
        //Debug.Log(theta);
        if (theta <= 0.0f || theta >= 90.0f) return;

        float deg = -(90.0f - theta) * 2.0f;//회전방향은 상관 없을듯.
        _Front.eulerAngles = new Vector3(0.0f, 0.0f, deg);


        _Mask.position = (transform.position + _Front.position) * 0.5f;
        _Mask.eulerAngles = new Vector3(0.0f, 0.0f, deg * 0.5f);

/*        _GradOutter.position = _Mask.position;
        _GradOutter.eulerAngles = new Vector3(0.0f, 0.0f, deg * 0.5f + 90.0f);*/


        transform.position = _Pos;
        transform.eulerAngles = Vector3.zero;
        Debug.Log(_Front.position.x);
        Debug.Log(_Front.position.y);
    } 
}
