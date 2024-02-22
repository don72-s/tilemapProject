using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HoleScript : MonoBehaviour, ExtendObserver, Map_Create_Destroy_Observer
{

    public GameObject mapManager;

    private float extendRatio = 0.6f;

    public GameObject maskPlane;

    //초반 위치로 카메라 이동시키기 위함 + lerp 사용하면 좋음
    private Quaternion defaultHoleRot;

    private Quaternion defaultCameraRot;
    private Vector3 defaultCameraPos;

    //todo : 기본 뷰볼륨 사이즈 => 더욱 축소 불가능? 결정바람.
    private float defaultViewVolumeRatio;


    private float flatExtendUnit = 0.3f;

    //화면 드래그 이동속도 설정.
    public float dragSpeed;

    //회전하는 값.
    private int rotValue = 0;


    MapManager mapManagerScript;

    //todo : ui묶음이든 드래그 예외 묶음이든 한번에 처리.
    //스크롤바 ui객체 [ 드래그영역 예외를 판정할 때 사용 ]
    public Scrollbar scrollbar;

    IRayCaster ray;

    void Start()
    {

        mapManagerScript = mapManager.GetComponent<MapManager>();
        scrollbar.onValueChanged.AddListener(ChangeRotWithScroll);

        float width = 4;
        float height = 4;

        float unitWidth = 4;
        float unitHeight = 4;

        mapManagerScript.AddExtendObserver(this);
        mapManagerScript.AddMapCreateDestroyObserver(this);

        defaultHoleRot = transform.rotation;
        defaultCameraRot = Camera.main.transform.rotation;
        defaultCameraPos = Camera.main.transform.position;

    defaultViewVolumeRatio = extendRatio * Mathf.Max(width * unitWidth, height * unitHeight);
        Camera.main.GetComponent<Camera>().orthographicSize = defaultViewVolumeRatio;

        maskPlane.transform.localScale = new Vector3(
                flatExtendUnit * width * unitWidth,
                maskPlane.transform.localScale.y,
                flatExtendUnit * height * unitHeight
            );

        ray = IRayCasterFactory.GetRayCaster();

    }

    private void OnDestroy()//소멸자의 대체
    {
        scrollbar.onValueChanged.RemoveListener(ChangeRotWithScroll);
    }


    /// <summary>
    /// 스크롤바에 따른 회전 정도 변수 조정.
    /// </summary>
    /// <param name="_value"></param>
    public void ChangeRotWithScroll(float _value) {
        rotValue = (int)((_value - 0.5f) * 500);
    }


    void Update() {

        #region 카메라 회전 관련 코드.

        if (rotValue != 0)
        {
           transform.Rotate(new Vector3(0, rotValue * Time.deltaTime, 0));
        }

        #endregion

        #region 카메라 이동(드래그) 관련 코드.


        //연산 전에 입력 여부를 확인하기 위해 input요소부터 순서를 구성.

        //드래그 중이며, 마우스 또는 터치가 종료되었을 경우.
        if (onDrag && Input.GetMouseButtonUp(0))
        {
            onDrag = false;
            Debug.Log("off");
        }


        //드래그 시작.
        //드래그중이 아님, 마우스 or 터치 이벤트 발생, 회전용 ui 스크롤바 위의 마우스 터치가 아님.
        if (!onDrag && Input.GetMouseButtonDown(0) && !ray.IsMouseOverUI(scrollbar))
        {//todo : ui목록들을 묶어서 리스트화 한 뒤, ismouseoverui(List) 이용으로 전환.
            if (!ray.GetCastHitPoint(LayerMasks.GetLayerMask(LayerMaskName.CHARACTER)).Equals(Vector3.negativeInfinity)) return;

            originPos = Camera.main.transform.position;
            originPosV2 = Input.mousePosition;

            onDrag = true;
        }

        if (onDrag) //드래그상황 중일 경우.
        {
            //디버그시 주석처리. todo : 
#if !UNITY_EDITOR
            if (Input.touchCount != 1) { 
                onDrag = false;
                return; 
            }
#endif
            //드래그 방향으로 카메라 이동.
            Camera.main.transform.position = originPos - Camera.main.transform.right * (Input.mousePosition.x - originPosV2.x) * dragSpeed - Camera.main.transform.up * (Input.mousePosition.y - originPosV2.y) * dragSpeed;
        }


        #endregion

    }

    private Vector3 originPos;
    private Vector3 originPosV2;
    
    private bool onDrag = false;



    /// <summary>
    /// hole과 camera의 위치를 초기 설정으로 되돌림.
    /// </summary>
    public void initCameraSetting() {

        transform.SetPositionAndRotation(Vector3.zero, defaultHoleRot);
        Camera.main.transform.SetPositionAndRotation(defaultCameraPos, defaultCameraRot);
        maskPlane.transform.position = Vector3.zero;

    }



    #region 인터페이스 영역
    //중심축 이동 및 기본 뷰볼륨 비율 변환
    public void OnExtended(Vector3 _centerPos, int _width, int _height, int _unitWidth, int _unitHeight)
    {
        transform.position = _centerPos;
        defaultViewVolumeRatio = extendRatio * Mathf.Max(_width * _unitWidth, _height * _unitHeight);

        Camera.main.GetComponent<Camera>().orthographicSize = defaultViewVolumeRatio;

        maskPlane.transform.position = _centerPos;

        maskPlane.transform.localScale = new Vector3(
                flatExtendUnit * _width * _unitWidth, 
                maskPlane.transform.localScale.y, 
                flatExtendUnit * _height * _unitHeight
            );
    }

    public void MapCreate(Vector3 _centerPos)
    {

        initCameraSetting();

        float width = mapManagerScript.GetWidth();
        float height = mapManagerScript.GetHeight();

        float unitWidth = mapManagerScript.GetUnitWidth();
        float unitHeight = mapManagerScript.GetUnitHeight();

        defaultViewVolumeRatio = extendRatio * Mathf.Max(width * unitWidth, height * unitHeight);

        Camera.main.GetComponent<Camera>().orthographicSize = defaultViewVolumeRatio;

        transform.position = _centerPos;

        maskPlane.transform.position = _centerPos;
        maskPlane.transform.localScale = new Vector3(
                flatExtendUnit * width * unitWidth,
                maskPlane.transform.localScale.y,
                flatExtendUnit * height * unitHeight
            );
    }

    public void MapDestroy()
    {

    }

    #endregion


}
