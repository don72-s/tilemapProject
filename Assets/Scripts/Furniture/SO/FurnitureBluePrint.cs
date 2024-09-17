using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class FurnitureBluePrint : ScriptableObject
{

    [SerializeField]
    private string furniitureName;
    [SerializeField]
    private GameObject furniture;
    [SerializeField]
    private GameObject furnitureRed;
    [SerializeField]
    private int widthSize;
    [SerializeField]
    private int heightSize;

    /// <summary>
    /// 가구 설계도 제작.
    /// </summary>
    /// <param name="_originalObject">원본 가구 객체 오브젝트</param>
    /// <param name="_originalObjectRed">원본 가구 무효성 객체 오브젝트</param>
    /// <param name="_widthSize">가구가 차지하는 가로 길이</param>
    /// <param name="_heightSize">가구가 차지하는 세로 길이</param>
    public FurnitureBluePrint(GameObject _originalObject, GameObject _originalObjectRed, int _widthSize, int _heightSize)
    {

        furniture = _originalObject;
        furnitureRed = _originalObjectRed;
        widthSize = _widthSize;
        heightSize = _heightSize;

    }

    public GameObject GetOriginalFurniture()
    {
        return furniture;
    }

    public GameObject GetOriginalRedFurniture()
    {
        return furnitureRed;
    }

    public int GetWidthSize()
    {
        return widthSize;
    }

    public int GetHeightSize()
    {
        return heightSize;
    }

    public string GetFurniitureName()
    {
        return furniitureName;
    }

}
