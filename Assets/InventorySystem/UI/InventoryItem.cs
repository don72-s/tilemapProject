using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    
    [SerializeField]
    private RectTransform rectTransform;

    [SerializeField]
    private Image ItemImg;
    [SerializeField]
    private Image FocusImg;

    [SerializeField]
    private TMP_Text quantityText;

    private bool isEmpty = true;

    //functionName(InventoryItem _inventoryItem) 타입의 함수를 받아서 저장해두는 역할.
    public event Action<InventoryItem> 
        OnItemClicked, 
        OnItemDropped, 
        OnItemBeginDrag,
        OnItemEndDrag, 
        OnMouseRightClicked;


    public void Awake()
    {
        ResetData();
        DisSelect();
    }

    /// <summary>
    /// 크기 조정.
    /// </summary>
    public void FixScale() {

        rectTransform.localScale = new Vector3(1f, 1f, 1f);

    }


    private void ResetData() { 
    
        ItemImg.gameObject.SetActive(false);
        isEmpty = true;

    }

    private void Select() { 
    
        FocusImg.gameObject.SetActive(true);

    }

    private void DisSelect() { 
        FocusImg.gameObject.SetActive(false);
    }

    public void SetData(Sprite _sprite, int _quantity) { 
    
        ItemImg.gameObject.SetActive(true);
        ItemImg.sprite = _sprite;
        quantityText.text = _quantity.ToString();
        isEmpty = false;

    }


    #region 입력 대응 함수

    /// <summary>
    /// 창에서 드래그가 시작되었을 때.
    /// </summary>
    public void BeginDrag() {

        if (isEmpty) return;
        OnItemBeginDrag?.Invoke(this);

    }

    /// <summary>
    /// 창 안에 드랍된경우.
    /// </summary>
    public void OnDrop() { 
        OnItemDropped?.Invoke(this);
    }

    /// <summary>
    /// 바깥 영역에서 드래그가 종료된 경우.
    /// </summary>
    public void EndDrag() { 
        OnItemEndDrag?.Invoke(this);
    }

    /// <summary>
    /// 클릭되었을 때.
    /// </summary>
    /// <param name="_Data"></param>
    public void OnPointerClick(BaseEventData _Data) { 
    
        PointerEventData data = (PointerEventData)_Data;

        if (data.button == PointerEventData.InputButton.Right)
        {
            OnMouseRightClicked?.Invoke(this);
        }
        else {
            OnItemClicked?.Invoke(this);
        }

    }

    #endregion


}
