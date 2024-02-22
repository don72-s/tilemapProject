using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MapScrollbarHandler : MonoBehaviour, IPointerUpHandler
{

    public Scrollbar scrollbar;

    public void OnPointerUp(PointerEventData eventData)
    {
        scrollbar.value = 0.5f;
    }

}
