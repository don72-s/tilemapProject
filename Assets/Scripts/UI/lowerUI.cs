using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lowerUI : MonoBehaviour
{

    private float height;
    private RectTransform rectTransform;
    private bool isHide;

    public RectTransform test;

    private Vector3 originalPos;
    private Vector3 hidePos;

    // Start is called before the first frame update
    void Start()
    {

        originalPos = transform.position;
        hidePos = originalPos - new Vector3(0, transform.position.y * 2, 0);

        isHide = false;

    }

    public void ChangeLowerState() {

        transform.position = isHide ? originalPos : hidePos;

        isHide = !isHide;

    }

}
