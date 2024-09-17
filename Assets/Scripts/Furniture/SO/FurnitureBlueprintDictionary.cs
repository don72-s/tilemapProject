using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Furniture List")]
public class FurnitureBlueprintDictionary : ScriptableObject
{

    private Dictionary<string, FurnitureBluePrint> blueprintDictionary = null;

    [SerializeField]
    private List<FurnitureBluePrint> blueprintList;

    public Dictionary<string, FurnitureBluePrint> GetFurnitureBlueprintDic()
    {

        if (blueprintDictionary != null) return blueprintDictionary;

        blueprintDictionary = new Dictionary<string, FurnitureBluePrint>();

        foreach (FurnitureBluePrint blue in blueprintList)
        {

            if (blueprintDictionary.ContainsKey(blue.GetFurniitureName()))
            {
                Debug.Log("중복된 이름의 가구 등록");
            }

            blueprintDictionary.Add(blue.GetFurniitureName(), blue);

        }

        return blueprintDictionary;

    }

}
