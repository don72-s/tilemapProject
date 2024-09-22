using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MapManager;

public class FlatInfo {


    private GameObject flatObject;
    private FurnitureInfo curFurniture = null;

    private bool isEmpty;

    private Vector2 myIdx;

    private Vector2 baseIdx;
    private List<Vector2> subIdxes = null;

    /// <summary>
    /// 각 바닥의 정보를 가지고 있는 객체 생성
    /// </summary>
    /// <param name="_g">바닥의 실체 오브젝트</param>
    /// <param name="_idxX">리스트상의 I 인덱스</param>
    /// <param name="_idxY">리스트상의 J 인덱스</param>
    public FlatInfo(GameObject _g, int _idxI, int _idxJ) {

        flatObject = _g;
        isEmpty = true;
        myIdx = new Vector2(_idxI, _idxJ);

    }

    public Vector2 GetBaseIdx() {
        return baseIdx;
    }

    public List<Vector2> GetSubIdxes() {
        return subIdxes;
    }

    public GameObject GetFlatObj() {
        return flatObject;
    }

    public bool GetIsEmpty() {
        return isEmpty;
    }

    /// <summary>
    /// 바닥 공간이 사용됨을 선언, base좌표를 저장.
    /// </summary>
    /// <param name="_baseVec">등록될 가구의 base 좌표</param>
    public void HoldEmptyWidthRegist(Vector2 _baseVec) {

        if (!isEmpty) {
            Debug.Log("무언가 존재.");
            return;
        }

        baseIdx = _baseVec;
        isEmpty = false;

    }


    //todo : 반환 후에는 인벤토리에 보유 가구 갯수 추가.
    /// <summary>
    /// 가구를 반환시키는 함수.
    /// </summary>
    /// <returns>반환된 가구 객체</returns>
    public FurnitureInfo ReleaseFurnitureObject() {

        //todo : FurnitureInfo 안에 가구 종류 저장. 반환시 이용하기.
        FurnitureInfo tmp = curFurniture;
        curFurniture = null;
        subIdxes = null;

        ReleaseEmpty();

        return tmp;

    }

    /// <summary>
    /// 해당(본인) 타일의 점유 해제
    /// </summary>
    public void ReleaseEmpty() {

        baseIdx = Vector2.negativeInfinity;
        isEmpty = true;
    }


    /// <summary>
    /// 가구정보를 바닥 객체에 등록.
    /// </summary>
    /// <param name="_fInfo">실체화 된 등록할 가구 객체 정보.</param>
    /// <param name="_vl">같이 점유될 sub 바닥 객체의 위치 정보 배열.</param>
    public void RegistFurnitureObject(FurnitureInfo _fInfo, List<Vector2> _vl) {

        curFurniture = _fInfo;
        subIdxes = _vl;

    }

    /// <summary>
    /// 디버그1
    /// </summary>
    public void InvisibleFlatInfo() {
        if (curFurniture != null) {
            curFurniture.SetInvisible();
        }

        flatObject.SetActive(false);
    }

    /// <summary>
    /// 디버그2
    /// </summary>
    public void VisibleFlatInfo() {
        if (curFurniture != null) {
            curFurniture.SetValid();
        }
        flatObject.SetActive(true);
    }

    /// <summary>
    /// 디버그3
    /// </summary>
    public void DestroyFlatInfo() {
        if (curFurniture != null) {
            curFurniture.DestroyFurnitureInfo();
        }
        Object.Destroy(GetFlatObj());
    }


}

public class FurnitureInfo {

    private GameObject furniture;
    private GameObject furnitureRed;

    public Vector2 curTilePos;

    private int[,] rotVecArr = new int[4, 4];
    private int rotIndexer;


    private GameObject furnitureParent;

    //짝수일 때 어긋남을 보정하는 offset 벡터
    private Vector3[] offsetPosArr = new Vector3[4];

    /// <summary>
    /// 전달받은 설계도의 가구를 생성.
    /// </summary>
    /// <param name="_furnitureBlueprint">객체화 할 가구의 설계도 객체</param>
    public FurnitureInfo(FurnitureBluePrint _furnitureBlueprint, string _furnitureName) {

        furnitureParent = new GameObject(_furnitureName);

        InitInfo(
                Object.Instantiate(_furnitureBlueprint.GetOriginalFurniture(), new Vector3(0, 1, 0), _furnitureBlueprint.GetOriginalFurniture().transform.rotation),
                Object.Instantiate(_furnitureBlueprint.GetOriginalRedFurniture(), new Vector3(0, 1, 0), _furnitureBlueprint.GetOriginalRedFurniture().transform.rotation),
                _furnitureBlueprint.GetWidthSize(),
                _furnitureBlueprint.GetHeightSize()
            );

        furniture.transform.SetParent(furnitureParent.transform);
        furnitureRed.transform.SetParent(furnitureParent.transform);

        curTilePos = new Vector2(-1, -1);
    }

    /// <summary>
    /// 가구 객체 정보 초기화
    /// </summary>
    /// <param name="_furniture">일반적인 가구 객체</param>
    /// <param name="_furniture_Red">불가능을 나타내는 가구 객체</param>
    /// <param name="_width">가구 객체의 가로 길이</param>
    /// <param name="_height">가구 객체의 세로 길이</param>
    public void InitInfo(GameObject _furniture, GameObject _furniture_Red, int _width, int _height) {


        furniture = _furniture;
        furnitureRed = _furniture_Red;

        furniture.SetActive(true);
        furnitureRed.SetActive(false);

        rotIndexer = 0;

        //홀수면 안더함, 짝수면 더함(offset)
        int left = -(_width / 2) + (_width + 1) % 2;
        int top = -(_height / 2);
        int right = (_width / 2);
        int down = (_height / 2) - (_height + 1) % 2;


        #region 백터 초기화

        rotVecArr[0, 0] = left;
        rotVecArr[0, 1] = top;
        rotVecArr[0, 2] = right;
        rotVecArr[0, 3] = down;

        rotVecArr[1, 0] = -down;
        rotVecArr[1, 1] = left;
        rotVecArr[1, 2] = -top;
        rotVecArr[1, 3] = right;

        rotVecArr[2, 0] = -right;
        rotVecArr[2, 1] = -down;
        rotVecArr[2, 2] = -left;
        rotVecArr[2, 3] = -top;

        rotVecArr[3, 0] = top;
        rotVecArr[3, 1] = -right;
        rotVecArr[3, 2] = down;
        rotVecArr[3, 3] = -left;

        //=============================================

        Vector3 tmpOffsetVec = new Vector3(_width % 2 == 0 ? 1 : 0,
            0,
            _height % 2 == 0 ? 1 : 0);

        offsetPosArr[0] = new Vector3(tmpOffsetVec.x, 0, tmpOffsetVec.z);
        offsetPosArr[1] = new Vector3(tmpOffsetVec.z, 0, -tmpOffsetVec.x);
        offsetPosArr[2] = new Vector3(-tmpOffsetVec.x, 0, -tmpOffsetVec.z);
        offsetPosArr[3] = new Vector3(-tmpOffsetVec.z, 0, tmpOffsetVec.x);


        #endregion

    }

    /// <summary>
    /// 가구를 회전시킴.
    /// </summary>
    public void RotateFurniture() {

        furniture.transform.Rotate(0, 90, 0);
        furnitureRed.transform.Rotate(0, 90, 0);

        rotIndexer = (rotIndexer + 1) % 4;

    }

    /// <summary>
    /// 가구의 위치를 변경함.
    /// </summary>
    /// <param name="_pos">변경할 중심 위치</param>
    public void SetPosition(Vector3 _pos) {

        furniture.transform.position = _pos;
        furnitureRed.transform.position = _pos;

    }

    /// <summary>
    /// 가구의 좌표를 반환. [좌표가 없다면 vector3.negativeinfinity 반환]
    /// </summary>
    /// <returns></returns>
    public Vector3 getPosition() {

        if (furniture == null) {
            return Vector3.negativeInfinity;
        }

        return furniture.transform.position;

    }

    /// <summary>
    /// 유효한 상태로 변환.
    /// </summary>
    public void SetValid() {
        furniture.SetActive(true);
        furnitureRed.SetActive(false);
    }

    /// <summary>
    /// 유효하지 않은 상태로 변환.
    /// </summary>
    public void SetInvalid() {
        furniture.SetActive(false);
        furnitureRed.SetActive(true);
    }

    /// <summary>
    /// 비활성화시킴.
    /// </summary>
    public void SetInvisible() {
        furniture.SetActive(false);
        furnitureRed.SetActive(false);
    }


    /// <summary>
    /// 좌상단기준 차지 공간 좌표 백터 반환.
    /// </summary>
    /// <returns></returns>
    public Vector2 GetLeftTopVector() {
        return new Vector2(rotVecArr[rotIndexer, 0], rotVecArr[rotIndexer, 1]);
    }

    /// <summary>
    /// 우하단기준 차지 공간 좌표 백터 반환.
    /// </summary>
    /// <returns></returns>
    public Vector2 GetRightDownVector() {
        return new Vector2(rotVecArr[rotIndexer, 2], rotVecArr[rotIndexer, 3]);
    }

    /// <summary>
    /// 가구의 정보를 제거함.
    /// </summary>
    public void DestroyFurnitureInfo() {
        Object.Destroy(furniture);
        Object.Destroy(furnitureRed);
        Object.Destroy(furnitureParent);
    }

    /// <summary>
    /// 위치 보정 필요 여부 벡터를 반환
    /// </summary>
    /// <returns>필요할 경우 1 or -1, 필요 없을 경우 0 [ x 와 z ]</returns>
    public Vector3 GetOffsetPosVec() {
        return offsetPosArr[rotIndexer];
    }

    /// <summary>
    /// 객체 오브젝트들의 부모 설정.
    /// </summary>
    /// <param name="_parent">부모가 될 객체</param>
    public void SetParent(GameObject _parent) {
        furnitureParent.transform.SetParent(_parent.transform);
    }
}

public static class FurnitureFactory {

    private static Dictionary<string, FurnitureBluePrint> furnitureDictionay = null;

    /// <summary>
    /// 팩토리 초기화
    /// </summary>
    /// <param name="_dic">딕셔너리로 직접 초기화.</param>
    internal static void InitFurnitureDic(Dictionary<string, FurnitureBluePrint> _dic) {

        furnitureDictionay = _dic;

    }

    /// <summary>
    /// 팩토리 초기화
    /// </summary>
    /// <param name="_dic">에디터에서 생성.</param>
    internal static void InitFurnitureDic(FurnitureBlueprintDictionary _dic) {

        InitFurnitureDic(_dic.GetFurnitureBlueprintDic());

    }


    /// <summary>
    /// 등록된 설계도를 기반으로 새로운 가구 생성
    /// </summary>
    /// <param name="_type">가구 설계도 이름(타입)</param>
    /// <param name="_furName">가구 이름</param>
    /// <returns>만들어진 새 가구 객체 [ 설계도가 없을 시 null 반환 ]</returns>
    internal static FurnitureInfo MakeFurniture(string _type, string _furName) {

        if (furnitureDictionay == null) {
            Debug.Log("팩토리가 초기화되지 않음.");
            return null;
        }

        if (!furnitureDictionay.ContainsKey(_type)) {
            Debug.Log("가구 설계도가 없음.");
            return null;
        }

        return new FurnitureInfo(furnitureDictionay[_type], _furName);
    }


}

public class FurnitureManager : MonoBehaviour {

    [SerializeField]
    private FurnitureBlueprintDictionary furnitureBlueprintDictionary;

    public Button testButton;
    public Button rotButton;
    public Button releaseButton;

    MapManager mapManager;
    FurnitureInfo curForcusedFurniture = null;
    IRayCaster iRayCaster;

    //todo : 타일별 변화 or 동작 요소들 구현 영역.


    private void Start() {
        
        FurnitureFactory.InitFurnitureDic(furnitureBlueprintDictionary);
        mapManager = MapManager.GetInstance();
        iRayCaster = IRayCasterFactory.GetRayCaster();

        testButton.onClick.AddListener(Btn_RegistFurniture);
        rotButton.onClick.AddListener(Btn_RotateFurniture);
        releaseButton.onClick.AddListener(Btn_ReleaseButton);

    }

    private bool IsEditMode() {

        return mapManager.GetMapViewMode() == MapState.EDIT_MODE;
    
    }

    public void Btn_RemoveFocusedFurniture() {

        if (!IsEditMode()) return;

        //가구제거버튼
        if (curForcusedFurniture != null) {
            curForcusedFurniture.DestroyFurnitureInfo();
            curForcusedFurniture = null;
            testButton.gameObject.SetActive(false);
            rotButton.gameObject.SetActive(false);
            releaseButton.gameObject.SetActive(false);
        }

    }

    //todo : 가구 생성 순회용 정적 변수.
    static int counter = 0;
    /// <summary>
    /// 가구생성용 디버그 + counter
    /// </summary>
    public void Btn_Makefurniture() {

        if (!IsEditMode()) return;

        if (curForcusedFurniture != null || 
            !mapManager.CheckMapExist()) return;

        //디버그용 순회식 생성.
        curForcusedFurniture = FurnitureFactory.MakeFurniture("test" + ((counter % 3) + 1).ToString(), "가구 번호 : " + counter.ToString());
        curForcusedFurniture.SetParent(mapManager.curMapData.furnitureParent);

        counter++;

    }

    /// <summary>
    /// 가구 등록용 디버그
    /// </summary>
    public void Btn_RegistFurniture() {

        if(!IsEditMode()) return;
        if (curForcusedFurniture == null) return;

        if (RegistFurniture(curForcusedFurniture.curTilePos, curForcusedFurniture.GetLeftTopVector(), curForcusedFurniture.GetRightDownVector())) {
            curForcusedFurniture = null;
            testButton.gameObject.SetActive(false);
            rotButton.gameObject.SetActive(false);
            releaseButton.gameObject.SetActive(false);
        }

    }

    /// <summary>
    /// 가구 회전용 디버그
    /// </summary>
    public void Btn_RotateFurniture() {

        if (!IsEditMode()) return;
        if (curForcusedFurniture == null) return;

        curForcusedFurniture.RotateFurniture();

        bool isValid = ValidCheck(curForcusedFurniture.curTilePos, curForcusedFurniture.GetLeftTopVector(), curForcusedFurniture.GetRightDownVector());

        if (isValid) {
            curForcusedFurniture.SetValid();
        } else {
            curForcusedFurniture.SetInvalid();
        }

        //지속적으로 상태 업데이트
        curForcusedFurniture.SetPosition(mapManager.curMapData.GetFlatList()[(int)curForcusedFurniture.curTilePos.y][(int)curForcusedFurniture.curTilePos.x].GetFlatObj().transform.position + new Vector3(0, 1.5f, 0) + curForcusedFurniture.GetOffsetPosVec());

    }

    /// <summary>
    /// 모든 버튼을 숨기는 함수.
    /// </summary>
    public void Btn_ReleaseButton() {

        testButton.gameObject.SetActive(false);
        rotButton.gameObject.SetActive(false);
        releaseButton.gameObject.SetActive(false);

    }

    /// <summary>
    /// update의 파생.
    /// </summary>
    public void EditModeUpdate() {

        //버튼 위치 지정
        if (curForcusedFurniture != null) {

            float debugVal = Screen.width / 19.0f;

            testButton.transform.position = Camera.main.WorldToScreenPoint(curForcusedFurniture.getPosition()) + new Vector3(0, debugVal / 3 * 2, 0);
            rotButton.transform.position = Camera.main.WorldToScreenPoint(curForcusedFurniture.getPosition()) + new Vector3(-debugVal, 0, 0);
            releaseButton.transform.position = Camera.main.WorldToScreenPoint(curForcusedFurniture.getPosition()) + new Vector3(debugVal, 0, 0);

        }

#if !UNITY_EDITOR
                //발판 복구
                if (Input.GetKeyDown(KeyCode.A)) {

                    foreach (var list in curMapData.GetFlatList()) {

                        foreach (var item in list) {
                            item.GetFlatObj().SetActive(true);
                        }
            
                    }

                }
#endif




        //선택된 가구가 존재하며, 터치를 떼는 행위가 발생한 경우
        if (Input.GetMouseButtonUp(0) && curForcusedFurniture != null && !testButton.gameObject.activeSelf) {

            SetFurniturePos(curForcusedFurniture);

        }

        //가구 선택 지정.
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetMouseButtonDown(0)) {
            if (curForcusedFurniture != null) return;
            curForcusedFurniture = GetExistFurniture();
            if (curForcusedFurniture == null) return;
            testButton.gameObject.SetActive(true);
            rotButton.gameObject.SetActive(true);
            releaseButton.gameObject.SetActive(true);
        }


        //[컴퓨터용 디버그] - 버튼 대응 함수를 호출.
        //들고있는 가구를 등록.
        if (Input.GetKeyDown(KeyCode.P) && curForcusedFurniture != null) {
            Btn_RegistFurniture();
        }


        //가구를 회전.
        if (Input.GetKeyDown(KeyCode.R) && curForcusedFurniture != null) {
            Btn_RotateFurniture();

        }

    }


    /// <summary>
    /// 중심 위치를 기준으로 좌상단 - 우하단의
    /// 직사각형 영역의 공간 유효성 검사 함수.
    /// </summary>
    /// <param name="_curPos">중심위치 좌표</param>
    /// <param name="_leftTopVec">좌상단 좌표(-)[중앙으로부터 상대 위치]</param>
    /// <param name="_rightDownVec">우하단 좌표(+)[중앙으로부터 상대 위치]</param>
    /// <returns>유효할 경우 true 반환.</returns>
    private bool ValidCheck(Vector2 _curPos, Vector2 _leftTopVec, Vector2 _rightDownVec) {

        if (!mapManager.CheckMapExist()) {
            Debug.Log("맵이 존재하지 않음. 가구 유효성 체크 불가능.");
            return false;
        }

        int x = (int)_curPos.x;
        int y = (int)_curPos.y;

        int leftOffset = (int)_leftTopVec.x;
        int topOffset = (int)_leftTopVec.y;
        int rightOffset = (int)_rightDownVec.x;
        int downOffset = (int)_rightDownVec.y;

        if (x + leftOffset < 0 || x + rightOffset >= mapManager.GetWidth() || y + topOffset < 0 || y + downOffset >= mapManager.GetHeight()) {
            Debug.Log("가구가 영역에서 벗어남.");
            return false;
        }

        for (int i = x + leftOffset; i <= x + rightOffset; i++) {

            for (int j = y + topOffset; j <= y + downOffset; j++) {

                if (!mapManager.curMapData.GetFlatList()[j][i].GetIsEmpty()) {
                    //충분한 가구 공간이 확보되지 않음.
                    return false;
                }

            }

        }

        //공간이 충분히 유효함.
        return true;

    }

    /// <summary>
    /// 가구를 주어진 좌표 타일에 등록하는 함수
    /// </summary>
    /// <param name="_curPos">등록할 타일의 메인좌표</param>
    /// <param name="_leftTopVec">등록 공간의 좌상단 좌표(-)[중앙으로부터 상대 위치]</param>
    /// <param name="_rightDownVec">등록 공간의 우하단 좌표(+)[중앙으로부터 상대 위치]</param>
    /// <returns>등록 성공시 true 반환.</returns>
    private bool RegistFurniture(Vector2 _curPos, Vector2 _leftTopVec, Vector2 _rightDownVec) {

        //공간 유효성 확인.
        if (!ValidCheck(_curPos, _leftTopVec, _rightDownVec)) {
            Debug.Log("가구 등록에 적합하지 않은 공간. [ 가구 등록 실패 ]");
            return false;
        }

        int x = (int)_curPos.x;
        int y = (int)_curPos.y;

        int leftOffset = (int)_leftTopVec.x;
        int topOffset = (int)_leftTopVec.y;
        int rightOffset = (int)_rightDownVec.x;
        int downOffset = (int)_rightDownVec.y;

        List<Vector2> list = new List<Vector2>();
        List<List<FlatInfo>> flatList = mapManager.curMapData.GetFlatList();

        for (int i = x + leftOffset; i <= x + rightOffset; i++) {
            for (int j = y + topOffset; j <= y + downOffset; j++) {

                if (i != x || j != y) {
                    list.Add(new Vector2(j, i));
                }
                flatList[j][i].HoldEmptyWidthRegist(new Vector2(y, x));


                //등록 위치 확인 디버그
                //flatList[j][i].GetFlatObj().SetActive(false);

            }
        }

        flatList[y][x].RegistFurnitureObject(curForcusedFurniture, list);

        return true;

    }

    /// <summary>
    /// 클릭된 flat의 좌표 확인.
    /// </summary>
    /// <returns>flat이 아니라면 vector2.negativeInfinity 반환.</returns>
    private Vector2 GetClickedFlatPos() {

        Vector3 v = iRayCaster.GetCastHitPoint(LayerMasks.GetLayerMask(LayerMaskName.FLAT));

        if (v.Equals(Vector3.negativeInfinity)) {

            Debug.Log("flat이 아님.");
            return Vector2.negativeInfinity;

        }


        float j = v.x + mapManager.GetUnitWidth() / 2;
        float i = -(v.z - mapManager.GetUnitHeight() / 2);

        if (j > 0 && j < mapManager.GetWidth() * mapManager.GetUnitWidth() && i > 0 && i < mapManager.GetHeight() * mapManager.GetUnitHeight()) {

            j = (int)(j / mapManager.GetUnitWidth());
            i = (int)(i / mapManager.GetUnitHeight());

        } else {

            Debug.Log("outer flat area detected");
            return Vector2.negativeInfinity;

        }

        return new Vector2(i, j);

    }



    private void SetFurniturePos(FurnitureInfo _furnitureInfo) {

        Vector2 flatPos = GetClickedFlatPos();

        if (flatPos.Equals(Vector2.negativeInfinity)) {
            Debug.Log("유효한 flat이 아님");
            return;
        }

        int i = (int)flatPos.x;
        int j = (int)flatPos.y;

        //같은위치 두번 터치.
        if ((int)_furnitureInfo.curTilePos.x == j && (int)_furnitureInfo.curTilePos.y == i) {

            //버튼 활성화.
            testButton.gameObject.SetActive(true);
            rotButton.gameObject.SetActive(true);
            releaseButton.gameObject.SetActive(true);
            return;
        }





        List<List<FlatInfo>> flatList = mapManager.curMapData.GetFlatList();

        if (curForcusedFurniture != null) {

            bool isValid = ValidCheck(new Vector2(flatPos.y, flatPos.x), curForcusedFurniture.GetLeftTopVector(), curForcusedFurniture.GetRightDownVector());

            if (isValid) {
                curForcusedFurniture.SetValid();
            } else {
                curForcusedFurniture.SetInvalid();
            }

            //상태 업데이트
            curForcusedFurniture.SetPosition(flatList[i][j].GetFlatObj().transform.position + new Vector3(0, 1.5f, 0) + curForcusedFurniture.GetOffsetPosVec());
            curForcusedFurniture.curTilePos.Set(j, i);
        }

    }

    /// <summary>
    /// 클릭한 위치에 있는 가구 객체 반환.[ 없을경우 null 반환 ]
    /// </summary>
    /// <returns></returns>
    private FurnitureInfo GetExistFurniture() {

        Vector2 flatPos = GetClickedFlatPos();

        if (flatPos.Equals(Vector2.negativeInfinity)) {
            Debug.Log("유효한 flat이 아님");
            return null;
        }

        int i = (int)flatPos.x;
        int j = (int)flatPos.y;


        List<List<FlatInfo>> flatList = mapManager.curMapData.GetFlatList();

        if (flatList[i][j].GetIsEmpty()) {
            //가구를 차지하고 있는 타일이 아님.
            return null;
        }

        Vector2 baseVec = flatList[i][j].GetBaseIdx();
        List<Vector2> vectors = flatList[(int)baseVec.x][(int)baseVec.y].GetSubIdxes();

        foreach (Vector2 _v in vectors) {
            flatList[(int)_v.x][(int)_v.y].ReleaseEmpty();
        }

        return flatList[(int)baseVec.x][(int)baseVec.y].ReleaseFurnitureObject();

    }
}
