using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryPage : MonoBehaviour
{
    [SerializeField]
    private InventoryItem itemPrefab;

    [SerializeField]
    private RectTransform contentPanel;


    List<InventoryItem> itemList = new List<InventoryItem>();

    /// <summary>
    /// 인벤토리 크기만큼 슬롯 초기화.
    /// </summary>
    /// <param name="_inventorySize">인벤토리의 아이템 갯수.</param>
    public void InitializeInventory(int _inventorySize) {

        for (int i = 0; i < _inventorySize; i++) { 
        
            InventoryItem tmpItem = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
            tmpItem.transform.SetParent(contentPanel);
            tmpItem.FixScale();
            itemList.Add(tmpItem);

            tmpItem.OnItemClicked += HandleItemClick;
            tmpItem.OnItemDropped += HandleItemDrop;
            tmpItem.OnItemBeginDrag += HandleItemBeginDrag;
            tmpItem.OnItemEndDrag += HandleItemEndDrag;
            tmpItem.OnMouseRightClicked += HandleItemRightClick;

        }

    }

    private void HandleItemRightClick(InventoryItem obj)
    {
        Debug.Log("우클릭");
    }

    private void HandleItemEndDrag(InventoryItem obj)
    {
        Debug.Log("드래그 종료");
    }

    private void HandleItemBeginDrag(InventoryItem obj)
    {
        Debug.Log("드래그 시작");
    }

    private void HandleItemDrop(InventoryItem obj)
    {
        Debug.Log("영역 드랍");
    }

    private void HandleItemClick(InventoryItem obj)
    {
        Debug.Log("좌클릭");
    }

    #region 전달할 대응 행동 구역



    #endregion


    /// <summary>
    /// 인벤토리 전체를 보여줌.
    /// </summary>
    public void ShowInventory() {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 인벤토리 전체를 숨김.
    /// </summary>
    public void HideInventory() { 
        gameObject.SetActive(false);
    }

}
