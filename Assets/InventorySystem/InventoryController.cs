using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{

    [SerializeField]
    private InventoryPage inventoryUI;

    [SerializeField]
    private int inventorySize;

    private void Start()
    {
        inventoryUI.InitializeInventory(inventorySize);
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.I)) {

            if (inventoryUI.isActiveAndEnabled == false)
            {
                inventoryUI.ShowInventory();
            }
            else { 
                inventoryUI.HideInventory();
            }

        }

    }

}
