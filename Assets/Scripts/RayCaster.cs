using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface IRayCaster {

    /// <summary>
    /// 화면상 눌린 마우스에서 카메라 방향으로 ray를 쏜 뒤
    /// 처음 충돌한 충돌체 좌표 반환.
    /// 없을경우 vector3.negativeInfinity 반환
    /// </summary>
    /// <param name="_distance"></param>
    /// <returns></returns>
    Vector3 GetCastHitPoint(float _distance = 100f);


    /// <summary>
    /// 화면상 눌린 마우스에서 카메라 방향으로 ray를 쏜 뒤
    /// mask에 해당하는 충돌체 좌표 반환.
    /// 없을경우 vector3.negativeInfinity 반환
    /// </summary>
    /// <param name="_mask">필터링할 마스크 타입</param>
    /// <returns></returns>
    Vector3 GetCastHitPoint(LayerMask _mask, float _distance = 100f);

    /// <summary>
    /// 이용할 ray를 직접 전달.
    /// </summary>
    /// <param name="_ray">캐스트할 ray객체</param>
    /// <param name="_distance">ray의 최대 거리</param>
    /// <returns></returns>
    Vector3 GetCastHitPoint(Ray _ray, float _distance = 100f);


    /// <summary>
    /// 화면상 눌린 마우스에서 카메라 방향으로 ray를 쏜 뒤
    /// mask에 해당하는 충돌체 좌표 반환.
    /// 없을경우 vector3.negativeInfinity 반환
    /// </summary>
    /// <param name="_mask">필터링할 마스크 타입</param>
    /// <param name="_customMousePos">화면상 출발 ray좌표를 직접 입력</param>
    /// <returns></returns>
    Vector3 GetCastHitPoint(LayerMask _mask, Vector3 _customMousePos, float _distance = 100f);

    /// <summary>
    /// 이용할 ray를 직접 전달.
    /// mask에 해당하는 충돌체 좌표 반환.
    /// 없을경우 vector3.negativeInfinity 반환
    /// </summary>
    /// <param name="_ray">캐스트할 ray객체</param>
    /// <param name="_mask">필터링할 마스크 타입</param>
    /// <returns></returns>
    Vector3 GetCastHitPoint(Ray _ray, LayerMask _mask, float _distance = 100f);



    /// <summary>
    /// 화면상 눌린 마우스에서 카메라 방향으로 ray를 쏜 뒤
    /// mask에 해당하는 충돌체 반환.
    /// 없을경우 null 반환
    /// </summary>
    /// <param name="_mask">필터링할 마스크 타입</param>
    /// <returns></returns>
    GameObject GetCastHit(LayerMask _mask, float _distance = 100f);


    /// <summary>
    /// 화면 상에서 UIBehaviour타입의 ui 객체 위에 마우스가 존재하는지 확인.
    /// </summary>
    /// <param name="_uiObject">대상이 되는 ui 객체</param>
    /// <returns>마우스가 대상 객체의 위라면 true 밖이라면 false</returns>
    bool IsMouseOverUI(UIBehaviour scrollbar);


    /// <summary>
    /// 화면 상에서 UIBehaviour타입의 ui 객체 위에 마우스가 존재하는지 확인.
    /// </summary>
    /// <param name="_uiObjectList">대상이 되는 ui 객체들의 리스트.</param>
    /// <returns></returns>
    bool IsMouseOverUI(List<UIBehaviour> _uiObjectList);

}

public static class IRayCasterFactory
{
    private static IRayCaster _caster = null;

    public static IRayCaster GetRayCaster() {

        if (_caster == null) { 
            _caster = new RayCaster();
        }

        return _caster;
    
    }

}

public class RayCaster : IRayCaster
{

    private RaycastHit hit;

    /// <summary>
    /// 화면상 눌린 마우스에서 카메라 방향으로 ray를 쏜 뒤
    /// 처음 충돌한 충돌체 좌표 반환.
    /// 없을경우 vector3.negativeInfinity 반환
    /// </summary>
    /// <param name="_distance"></param>
    /// <returns></returns>
    public Vector3 GetCastHitPoint(float _distance = 100f)
    {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);//메인카메라
        Vector3 hitPoint = Vector3.negativeInfinity;
        if (Physics.Raycast(ray, out hit, _distance))
        {
            hitPoint = hit.point;
        }
        return hitPoint;
    }


    /// <summary>
    /// 화면상 눌린 마우스에서 카메라 방향으로 ray를 쏜 뒤
    /// mask에 해당하는 충돌체 좌표 반환.
    /// 없을경우 vector3.negativeInfinity 반환
    /// </summary>
    /// <param name="_mask">필터링할 마스크 타입</param>
    /// <returns></returns>
    public Vector3 GetCastHitPoint(LayerMask _mask, float _distance = 100f)
    {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);//메인카메라
        Vector3 hitPoint = Vector3.negativeInfinity;
        if (Physics.Raycast(ray, out hit, _distance, _mask))
        {
            hitPoint = hit.point;
         
        }

        return hitPoint;
    }

    /// <summary>
    /// 이용할 ray를 직접 전달.
    /// </summary>
    /// <param name="_ray">캐스트할 ray객체</param>
    /// <param name="_distance">ray의 최대 거리</param>
    /// <returns></returns>
    public Vector3 GetCastHitPoint(Ray _ray, float _distance = 100f)
    {

        Vector3 hitPoint = Vector3.negativeInfinity;
        if (Physics.Raycast(_ray, out hit, _distance))
        {
            hitPoint = hit.point;
        }

        return hitPoint;
    }

    /// <summary>
    /// 화면상 눌린 마우스에서 카메라 방향으로 ray를 쏜 뒤
    /// mask에 해당하는 충돌체 좌표 반환.
    /// 없을경우 vector3.negativeInfinity 반환
    /// </summary>
    /// <param name="_mask">필터링할 마스크 타입</param>
    /// <param name="_customMousePos">화면상 출발 ray좌표를 직접 입력</param>
    /// <returns></returns>
    public Vector3 GetCastHitPoint(LayerMask _mask,Vector3 _customMousePos, float _distance = 100f)
    {

        Ray ray = Camera.main.ScreenPointToRay(_customMousePos);
        Vector3 hitPoint = Vector3.negativeInfinity;
        if (Physics.Raycast(ray, out hit, _distance, _mask))
        {
            hitPoint = hit.point;
        }

        return hitPoint;
    }

    /// <summary>
    /// 이용할 ray를 직접 전달.
    /// mask에 해당하는 충돌체 좌표 반환.
    /// 없을경우 vector3.negativeInfinity 반환
    /// </summary>
    /// <param name="_ray">캐스트할 ray객체</param>
    /// <param name="_mask">필터링할 마스크 타입</param>
    /// <returns></returns>
    public Vector3 GetCastHitPoint(Ray _ray, LayerMask _mask, float _distance = 100f)
    {

        Vector3 hitPoint = Vector3.negativeInfinity;
        if (Physics.Raycast(_ray, out hit, _distance, _mask))
        {
            hitPoint = hit.point;
        }

        return hitPoint;
    }




    /// <summary>
    /// 화면상 눌린 마우스에서 카메라 방향으로 ray를 쏜 뒤
    /// mask에 해당하는 충돌체 반환.
    /// 없을경우 null 반환
    /// </summary>
    /// <param name="_mask">필터링할 마스크 타입</param>
    /// <returns></returns>
    public GameObject GetCastHit(LayerMask _mask, float _distance = 100f)
    {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);//메인카메라
        GameObject hitPoint = null;
        if (Physics.Raycast(ray, out hit, _distance, _mask))
        {
            hitPoint = hit.transform.gameObject;

        }

        return hitPoint;
    }



    /// <summary>
    /// 화면 상에서 UIBehaviour타입의 ui 객체 위에 마우스가 존재하는지 확인.
    /// </summary>
    /// <param name="_uiObject">대상이 되는 ui 객체</param>
    /// <returns>마우스가 대상 객체의 위라면 true 밖이라면 false</returns>
    public bool IsMouseOverUI(UIBehaviour _uiObject)
    {
        
        if (_uiObject == null)
            return false;

        Type originType = _uiObject.GetType();

        List<RaycastResult> results = getRaycastResults();


        foreach (RaycastResult result in results)
        {
            // 특정 UI 타입과 일치하는지 확인
            if (result.gameObject.GetComponent(originType) == _uiObject)
            {
                return true;
            }
        }
        return false;
    }



    /// <summary>
    /// 화면 상에서 UIBehaviour타입의 ui 객체 위에 마우스가 존재하는지 확인.
    /// </summary>
    /// <param name="_uiObjectList">대상이 되는 ui 객체들의 리스트.</param>
    /// <returns></returns>
    public bool IsMouseOverUI(List<UIBehaviour> _uiObjectList)
    {

        //유효성 검사.
        foreach (UIBehaviour o in _uiObjectList) {
            if (o == null) return false;
        }


        List<Type> originTypeList = new List<Type>();

        //모든 요소의 각각 타입을 저장.
        foreach (UIBehaviour o in _uiObjectList) { 
            originTypeList.Add(o.GetType());
        }

        List<RaycastResult> results = getRaycastResults();


        foreach (RaycastResult result in results)
        {

            //일치하는 종류가 있는지 확인.
            for (int i = 0; i < originTypeList.Count; i++) {

                if (result.gameObject.GetComponent(originTypeList[i]) == _uiObjectList[i]) { 
                    return true;                
                }

            }

        }

        return false;
    }

    /// <summary>
    /// 현재 eventSystem상에서 마우스 위치에 대응하는 모든 ui요소에 대한 ray 결과를 반환.
    /// </summary>
    /// <returns>ui에 대해 캐스트된 결과 리스트.</returns>
    private List<RaycastResult> getRaycastResults() {

        //이벤트 시스템과 마우스 위치를 가져옴.
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        // UI에 대한 이벤트를 확인할 때 RaycastResult 객체를 생성하여 전달
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        return results;

    }

}
