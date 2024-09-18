using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public interface ExtendObserver {

    /// <summary>
    /// 바닥의 크기가 변화했을 때 호출되는 함수
    /// </summary>
    /// <param name="_centerPos">갱신된 중심축 전달</param>
    /// <param name="_width">변화 후 너비</param>
    /// <param name="_height">변화 후 높이</param>
    void OnExtended(Vector3 _centerPos, int _width, int _height, int _unitWidth, int _unitHeight);

}

public interface Map_Create_Destroy_Observer {

    /// <summary>
    /// 맵이 생성될 때 호출되는 함수
    /// </summary>
    void MapCreate(Vector3 centerPos);

    /// <summary>
    /// 맵이 제거될 때 호출되는 함수
    /// </summary>
    void MapDestroy();

}


class MapData {

    private GameObject flatParent;
    private GameObject wallWidthParent_1;
    private GameObject wallWidthParent_2;
    private GameObject wallHeightParent_1;
    private GameObject wallHeightParent_2;
    private GameObject cornerParent;

    public GameObject furnitureParent { get; private set; }

    private GameObject baseMapParent;

    private List<List<FlatInfo>> flatList;
    private List<GameObject> widthWallList_1;
    private List<GameObject> widthWallList_2;
    private List<GameObject> heightWallList_1;
    private List<GameObject> heightWallList_2;
    private List<GameObject> cornerList;

    //처음 생성시의 너비, 높이
    public int originalWidth { get; private set; }
    public int originalHeight { get; private set; }

    public int width { get; private set; }
    public int height { get; private set; }

    //바닥타일의 크기 => 짝수 단위로 계산할 것. => 홀수 계산시 중앙위치 계산이 오차가 생길 수도 있음(검증x).
    public int tileWidth { get; private set; }
    public int tileHeight { get; private set; }

    //바닥타일간의 간격 [ 가로 : x, 세로 : -z ] => 홀수인 경우 오차와 오류 발생 ? 홀수가 필수라고 하면 내부 자료형을 float로 수정.
    public int countOffsetX { get; private set; }
    public int countOffsetZ { get; private set; }
    public int minWidth { get; private set; }
    public int minHeight { get; private set; }

    //(0,0)으로부터 좌상단으로 기준점 이동 좌표.
    public int baseOffsetX { get; private set; }
    public int baseOffsetZ { get; private set; }

    //가장자리 벽의 거리 오프셋
    public float widthWallOffset { get; private set; }
    public float heightWallOffset { get; private set; }


    public MapData(int _width, int _height, int _tileWidth, int _tileHeight, GameObject _flatObject, GameObject _wallObject, GameObject _cornerObject) {

        flatParent = new GameObject("flatParent");

        wallWidthParent_1 = new GameObject("wallWidthParent_1");
        wallWidthParent_1.transform.rotation = Quaternion.Euler(0, 90, 0);

        wallWidthParent_2 = new GameObject("wallWidthParent_2");
        wallWidthParent_2.transform.rotation = Quaternion.Euler(0, 270, 0);

        wallHeightParent_1 = new GameObject("wallHeightParent_1");
        wallHeightParent_1.transform.rotation = Quaternion.Euler(0, 180, 0);

        wallHeightParent_2 = new GameObject("wallHeightParent_2");
        wallHeightParent_2.transform.rotation = Quaternion.Euler(0, 0, 0);

        cornerParent = new GameObject("CornerParent");

        furnitureParent = new GameObject("Furniture List");

        baseMapParent = new GameObject("mapBase");

        flatParent.transform.parent = baseMapParent.transform;
        wallWidthParent_1.transform.parent = baseMapParent.transform;
        wallWidthParent_2.transform.parent = baseMapParent.transform;
        wallHeightParent_1.transform.parent = baseMapParent.transform;
        wallHeightParent_2.transform.parent = baseMapParent.transform;
        cornerParent.transform.parent = baseMapParent.transform;
        furnitureParent.transform.parent = baseMapParent.transform;


        //초기 정보 세팅

        originalWidth = _width;
        originalHeight = _height;
        width = _width;
        height = _height;
        tileWidth = _tileWidth;
        tileHeight = _tileHeight;

        countOffsetX = tileWidth;
        countOffsetZ = -tileHeight;

        //초기 길이를 우선 최소 길이로 지정.
        minWidth = width;
        minHeight = height;

        //좌상단 위치 계산을 위한 offset 계산.
        baseOffsetX = ((width / 2) * -countOffsetX) + ((width + 1) % 2) * (tileWidth / 2);
        baseOffsetZ = ((height / 2) * -countOffsetZ) - ((height + 1) % 2) * (tileHeight / 2);

        //벽 오프셋 지정
        widthWallOffset = tileWidth / 2 + 0.15f;
        heightWallOffset = tileHeight / 2 + 0.15f;

        /*#region 생성테스트

        GameObject g;

        flatList = new List<List<FlatInfo>>();
        widthWallList_1 = new List<GameObject>();
        widthWallList_2 = new List<GameObject>();
        heightWallList_1 = new List<GameObject>();
        heightWallList_2 = new List<GameObject>();
        cornerList = new List<GameObject>();

        //바닥 생성
        if (flatList.Count == 0) {
            for (int i = 0; i < height; i++) {
                flatList.Add(new List<FlatInfo>());

                for (int j = 0; j < width; j++) {
                    g = Object.Instantiate(_flatObject, new Vector3(baseOffsetX + countOffsetX * j, _flatObject.transform.position.y, baseOffsetZ + countOffsetZ * i), Quaternion.identity);
                    g.transform.localScale = new Vector3(tileWidth, g.transform.localScale.y, tileHeight);
                    g.transform.parent = flatParent.transform;
                    flatList[i].Add(new FlatInfo(g, i, j));
                }

            }

        }

        //벽 생성.
        //기준점(불변점)에 가까운 요소가 0번째 요소로 등록됨. 가변성이 있는 요소가 가장 끝의 요소.
        for (int i = 0; i < height; i++) {
            g = Object.Instantiate(_wallObject, new Vector3(baseOffsetX - widthWallOffset, _wallObject.transform.position.y, baseOffsetZ + countOffsetZ * i), Quaternion.Euler(0, 0, 0));
            g.transform.localScale = new Vector3(g.transform.localScale.x, g.transform.localScale.y, tileHeight);
            g.transform.parent = wallWidthParent_1.transform;
            widthWallList_1.Add(g);

            g = Object.Instantiate(_wallObject, new Vector3(baseOffsetX + countOffsetX * (width - 1) + widthWallOffset, _wallObject.transform.position.y, baseOffsetZ + countOffsetZ * i), Quaternion.Euler(0, 180, 0));
            g.transform.localScale = new Vector3(g.transform.localScale.x, g.transform.localScale.y, tileHeight);
            g.transform.parent = wallWidthParent_2.transform;
            widthWallList_2.Add(g);
        }

        for (int i = 0; i < width; i++) {

            g = Object.Instantiate(_wallObject, new Vector3(baseOffsetX + countOffsetX * i, _wallObject.transform.position.y, baseOffsetZ + heightWallOffset), Quaternion.Euler(0, 90, 0));
            g.transform.localScale = new Vector3(g.transform.localScale.x, g.transform.localScale.y, tileWidth);
            g.transform.parent = wallHeightParent_1.transform;
            heightWallList_1.Add(g);

            g = Object.Instantiate(_wallObject, new Vector3(baseOffsetX + countOffsetX * i, _wallObject.transform.position.y, baseOffsetZ + countOffsetZ * (height - 1) - heightWallOffset), Quaternion.Euler(0, 270, 0));
            g.transform.localScale = new Vector3(g.transform.localScale.x, g.transform.localScale.y, tileWidth);
            g.transform.parent = wallHeightParent_2.transform;
            heightWallList_2.Add(g);

        }



        //꼭짓점 기둥 생성.

        cornerList.Add(Object.Instantiate(_cornerObject, new Vector3(baseOffsetX - widthWallOffset, _cornerObject.transform.position.y, baseOffsetZ + heightWallOffset), Quaternion.Euler(0, 0, 0)));
        cornerList.Add(Object.Instantiate(_cornerObject, new Vector3(baseOffsetX - widthWallOffset, _cornerObject.transform.position.y, baseOffsetZ + countOffsetZ * (height - 1) - heightWallOffset), Quaternion.Euler(0, 0, 0)));
        cornerList.Add(Object.Instantiate(_cornerObject, new Vector3(baseOffsetX + countOffsetX * (width - 1) + widthWallOffset, _cornerObject.transform.position.y, baseOffsetZ + heightWallOffset), Quaternion.Euler(0, 0, 0)));
        cornerList.Add(Object.Instantiate(_cornerObject, new Vector3(baseOffsetX + countOffsetX * (width - 1) + widthWallOffset, _cornerObject.transform.position.y, baseOffsetZ + countOffsetZ * (height - 1) - heightWallOffset), Quaternion.Euler(0, 0, 0)));

        for (int i = 0; i < cornerList.Count; i++) {
            cornerList[i].transform.parent = cornerParent.transform;
        }

        #endregion*/

        #region 생성테스트 2

        GameObject g;

        flatList = new List<List<FlatInfo>>();
        widthWallList_1 = new List<GameObject>();
        widthWallList_2 = new List<GameObject>();
        heightWallList_1 = new List<GameObject>();
        heightWallList_2 = new List<GameObject>();
        cornerList = new List<GameObject>();

        //바닥 생성
        if (flatList.Count == 0) {
            for (int i = 0; i < height; i++) {
                flatList.Add(new List<FlatInfo>());

                for (int j = 0; j < width; j++) {
                    g = Object.Instantiate(_flatObject, new Vector3(countOffsetX * j, _flatObject.transform.position.y, countOffsetZ * i), Quaternion.identity);
                    g.transform.localScale = new Vector3(tileWidth, g.transform.localScale.y, tileHeight);
                    g.transform.parent = flatParent.transform;
                    flatList[i].Add(new FlatInfo(g, i, j));
                }

            }

        }

        //벽 생성.
        //기준점(불변점)에 가까운 요소가 0번째 요소로 등록됨. 가변성이 있는 요소가 가장 끝의 요소.
        for (int i = 0; i < height; i++) {
            g = Object.Instantiate(_wallObject, new Vector3(- widthWallOffset, _wallObject.transform.position.y, countOffsetZ * i), Quaternion.Euler(0, 0, 0));
            g.transform.localScale = new Vector3(g.transform.localScale.x, g.transform.localScale.y, tileHeight);
            g.transform.parent = wallWidthParent_1.transform;
            widthWallList_1.Add(g);

            g = Object.Instantiate(_wallObject, new Vector3(countOffsetX * (width - 1) + widthWallOffset, _wallObject.transform.position.y, countOffsetZ * i), Quaternion.Euler(0, 180, 0));
            g.transform.localScale = new Vector3(g.transform.localScale.x, g.transform.localScale.y, tileHeight);
            g.transform.parent = wallWidthParent_2.transform;
            widthWallList_2.Add(g);
        }

        for (int i = 0; i < width; i++) {

            g = Object.Instantiate(_wallObject, new Vector3(countOffsetX * i, _wallObject.transform.position.y, heightWallOffset), Quaternion.Euler(0, 90, 0));
            g.transform.localScale = new Vector3(g.transform.localScale.x, g.transform.localScale.y, tileWidth);
            g.transform.parent = wallHeightParent_1.transform;
            heightWallList_1.Add(g);

            g = Object.Instantiate(_wallObject, new Vector3(countOffsetX * i, _wallObject.transform.position.y, countOffsetZ * (height - 1) - heightWallOffset), Quaternion.Euler(0, 270, 0));
            g.transform.localScale = new Vector3(g.transform.localScale.x, g.transform.localScale.y, tileWidth);
            g.transform.parent = wallHeightParent_2.transform;
            heightWallList_2.Add(g);

        }



        //꼭짓점 기둥 생성.

        cornerList.Add(Object.Instantiate(_cornerObject, new Vector3(- widthWallOffset, _cornerObject.transform.position.y, heightWallOffset), Quaternion.Euler(0, 0, 0)));
        cornerList.Add(Object.Instantiate(_cornerObject, new Vector3(- widthWallOffset, _cornerObject.transform.position.y, countOffsetZ * (height - 1) - heightWallOffset), Quaternion.Euler(0, 0, 0)));
        cornerList.Add(Object.Instantiate(_cornerObject, new Vector3(countOffsetX * (width - 1) + widthWallOffset, _cornerObject.transform.position.y, heightWallOffset), Quaternion.Euler(0, 0, 0)));
        cornerList.Add(Object.Instantiate(_cornerObject, new Vector3(countOffsetX * (width - 1) + widthWallOffset, _cornerObject.transform.position.y, countOffsetZ * (height - 1) - heightWallOffset), Quaternion.Euler(0, 0, 0)));

        for (int i = 0; i < cornerList.Count; i++) {
            cornerList[i].transform.parent = cornerParent.transform;
        }

        #endregion

    }


    public void VisibleSetting(Vector3 _camera) {

        bool widthWall_1 = Vector3.Dot(_camera, wallWidthParent_1.transform.forward) > 0.0001f ? false : true;
        bool heightWall_1 = Vector3.Dot(_camera, wallHeightParent_1.transform.forward) > 0.0001f ? false : true;
        bool widthWall_2 = Vector3.Dot(_camera, wallWidthParent_2.transform.forward) > 0.0001f ? false : true;
        bool heightWall_2 = Vector3.Dot(_camera, wallHeightParent_2.transform.forward) > 0.0001f ? false : true;

        SetWallsVisible(widthWall_1, widthWall_2, heightWall_1, heightWall_2);
        SetCornerVisible(widthWall_1 && heightWall_1, widthWall_1 && heightWall_2, widthWall_2 && heightWall_1, widthWall_2 && heightWall_2);
    }

    #region 게터

    public int GetWidth() {
        return width;
    }

    public int GetHeight() {
        return height;
    }

    public int GetTileWidth() {
        return tileWidth;
    }
    public int GetTileHeight() {
        return tileHeight;
    }


    public List<List<FlatInfo>> GetFlatList() {
        return flatList;
    }

    public List<GameObject> GetWidthWallList_1() {
        return widthWallList_1;
    }
    public List<GameObject> GetWidthWallList_2() {
        return widthWallList_2;
    }
    public List<GameObject> GetHeightWallList_1() {
        return heightWallList_1;
    }
    public List<GameObject> GetHeightWallList_2() {
        return heightWallList_2;
    }
    public List<GameObject> GetCornerList() {
        return cornerList;
    }

    #endregion

    #region 벽, 코너, 바닥 가시화

    public void SetWallsVisible(bool _widthWall_1, bool _widthWall_2, bool _heightWall_1, bool _heightWall_2) {

        SetWallWidth_1_Visible(_widthWall_1);
        SetWallWidth_2_Visible(_widthWall_2);
        SetWallHeight_1_Visible(_heightWall_1);
        SetWallHeight_2_Visible(_heightWall_2);

    }

    public void SetWallWidth_1_Visible(bool _visible) {

        foreach (var _o in widthWallList_1) {
            _o.SetActive(_visible);
        }

    }

    public void SetWallWidth_2_Visible(bool _visible) {

        foreach (var _o in widthWallList_2) {
            _o.SetActive(_visible);
        }

    }

    public void SetWallHeight_1_Visible(bool _visible) {

        foreach (var _o in heightWallList_1) {
            _o.SetActive(_visible);
        }

    }

    public void SetWallHeight_2_Visible(bool _visible) {

        foreach (var _o in heightWallList_2) {
            _o.SetActive(_visible);
        }

    }

    public void SetCornerVisible(bool _corner1, bool _corner2, bool _corner3, bool _corner4) {

        cornerList[0].SetActive(_corner1);
        cornerList[1].SetActive(_corner2);
        cornerList[2].SetActive(_corner3);
        cornerList[3].SetActive(_corner4);

    }

    public void SetFlatVisible(bool _visible) {

        if (_visible) {
            foreach (List<FlatInfo> fl in flatList) {
                foreach (FlatInfo o in fl) {
                    o.VisibleFlatInfo();
                }
            }
        } else {
            foreach (List<FlatInfo> fl in flatList) {
                foreach (FlatInfo o in fl) {
                    o.InvisibleFlatInfo();
                }
            }
        }

    }

    #endregion


    #region 맵 확장, 축소

    public void ExtendWidth() {

        //우선 최대는 maxvalue로 지정.
        if (width >= int.MaxValue) {
            Debug.Log("너비 최대 크기 도달.");
            return;
        }

        width++;

        GameObject origin = flatList[0][0].GetFlatObj();

        for (int i = 0; i < flatList.Count; i++) {
            GameObject g = Object.Instantiate(origin, new Vector3(countOffsetX * (width - 1), origin.transform.position.y, flatList[i][0].GetFlatObj().transform.position.z), Quaternion.identity);
            g.transform.localScale = new Vector3(tileWidth, g.transform.localScale.y, tileHeight);
            g.transform.parent = flatParent.transform;
            flatList[i].Add(new FlatInfo(g, i, flatList[i].Count));
        }

        wallWidthParent_2.transform.position = new Vector3(wallWidthParent_2.transform.position.x + countOffsetX, wallWidthParent_2.transform.position.y, wallWidthParent_2.transform.position.z);

        origin = heightWallList_1[heightWallList_1.Count - 1];
        GameObject ob = Object.Instantiate(origin, new Vector3(origin.transform.position.x + countOffsetX, origin.transform.position.y, origin.transform.position.z), origin.transform.rotation);
        ob.transform.parent = wallHeightParent_1.transform;
        heightWallList_1.Add(ob);

        origin = heightWallList_2[heightWallList_2.Count - 1];
        ob = Object.Instantiate(origin, new Vector3(origin.transform.position.x + countOffsetX, origin.transform.position.y, origin.transform.position.z), origin.transform.rotation);
        ob.transform.parent = wallHeightParent_2.transform;
        heightWallList_2.Add(ob);

        cornerList[2].transform.position += new Vector3(countOffsetX, 0, 0);
        cornerList[3].transform.position += new Vector3(countOffsetX, 0, 0);

    }

    public bool ReduceWidth() {

        if (width <= minWidth) {
            Debug.Log("너비 최소 크기 도달");
            return false;
        }

        for (int i = 0; i < flatList.Count; i++) {
            if (!flatList[i][width - 1].GetIsEmpty()) {
                Debug.Log("지울 영역에 가구 존재.");
                return false;
            }
        }


        width--;


        for (int i = 0; i < flatList.Count; i++) {
            Object.Destroy(flatList[i][width].GetFlatObj());
            flatList[i].RemoveAt(width);
        }

        wallWidthParent_2.transform.position = new Vector3(wallWidthParent_2.transform.position.x - countOffsetX, wallWidthParent_2.transform.position.y, wallWidthParent_2.transform.position.z);

        Object.Destroy(heightWallList_1[heightWallList_1.Count - 1]);
        heightWallList_1.RemoveAt(heightWallList_1.Count - 1);

        Object.Destroy(heightWallList_2[heightWallList_2.Count - 1]);
        heightWallList_2.RemoveAt(heightWallList_2.Count - 1);

        cornerList[2].transform.position -= new Vector3(countOffsetX, 0, 0);
        cornerList[3].transform.position -= new Vector3(countOffsetX, 0, 0);

        return true;
    }

    public void ExtendHeight() {

        //최대는 우선 임의 지정.
        if (height >= int.MaxValue) {
            Debug.Log("높이 최대 크기 도달.");
            return;
        }


        height++;


        flatList.Add(new List<FlatInfo>());

        GameObject origin = flatList[0][0].GetFlatObj();

        for (int i = 0; i < width; i++) {
            GameObject g = Object.Instantiate(origin, new Vector3(countOffsetX * i, origin.transform.position.y, countOffsetZ * (height - 1)), Quaternion.identity);
            g.transform.localScale = new Vector3(tileWidth, g.transform.localScale.y, tileHeight);
            g.transform.parent = flatParent.transform;
            flatList[flatList.Count - 1].Add(new FlatInfo(g, flatList.Count, i));
        }

        wallHeightParent_2.transform.position = new Vector3(wallHeightParent_2.transform.position.x, wallHeightParent_2.transform.position.y, wallHeightParent_2.transform.position.z + countOffsetZ);

        origin = widthWallList_1[widthWallList_1.Count - 1];
        GameObject ob = Object.Instantiate(origin, new Vector3(origin.transform.position.x, origin.transform.position.y, origin.transform.position.z + countOffsetZ), origin.transform.rotation);
        ob.transform.parent = wallWidthParent_1.transform;
        widthWallList_1.Add(ob);

        origin = widthWallList_2[widthWallList_2.Count - 1];
        ob = Object.Instantiate(origin, new Vector3(origin.transform.position.x, origin.transform.position.y, origin.transform.position.z + countOffsetZ), origin.transform.rotation);
        ob.transform.parent = wallWidthParent_2.transform;
        widthWallList_2.Add(ob);

        cornerList[1].transform.position += new Vector3(0, 0, countOffsetZ);
        cornerList[3].transform.position += new Vector3(0, 0, countOffsetZ);


    }

    public bool ReduceHeight() {

        if (height <= minHeight) {
            Debug.Log("높이 최소 크기 도달");
            return false;
        }

        for (int i = 0; i < flatList[height - 1].Count; i++) {
            if (!flatList[height - 1][i].GetIsEmpty()) {
                Debug.Log("지울 영역에 가구 존재.");
                return false;
            }
        }

        height--;

        for (int i = 0; i < flatList[height].Count; i++) {
            Object.Destroy(flatList[height][i].GetFlatObj());
        }

        flatList.RemoveAt(height);

        wallHeightParent_2.transform.position = new Vector3(wallHeightParent_2.transform.position.x, wallHeightParent_2.transform.position.y, wallHeightParent_2.transform.position.z - countOffsetZ);

        Object.Destroy(widthWallList_1[widthWallList_1.Count - 1]);
        widthWallList_1.RemoveAt(widthWallList_1.Count - 1);

        Object.Destroy(widthWallList_2[widthWallList_2.Count - 1]);
        widthWallList_2.RemoveAt(widthWallList_2.Count - 1);

        cornerList[1].transform.position -= new Vector3(0, 0, countOffsetZ);
        cornerList[3].transform.position -= new Vector3(0, 0, countOffsetZ);

        return true;
    }

    #endregion

    /// <summary>
    /// 비어있는 발판의 중심좌표를 반환. [ 실패시 negativeinfinity 반환 ]
    /// </summary>
    /// <returns></returns>
    public Vector3 GetEmptyFlat() {


        List<Vector3> emptyFlats = GetEmptyPosList();

        if (emptyFlats == null) {
            return Vector3.negativeInfinity;
        }

        return emptyFlats[Random.Range(0, emptyFlats.Count)];

    }


    /// <summary>
    /// 비어있는 타일들의 중심좌표를 List<vector3>형태로 반환. [ 없다면 null을 반환 ]
    /// </summary>
    /// <returns></returns>
    private List<Vector3> GetEmptyPosList() {

        List<Vector3> retList = new List<Vector3>();

        foreach (List<FlatInfo> _fl in flatList) {

            foreach (FlatInfo _f in _fl) {

                if (_f.GetIsEmpty()) {

                    retList.Add(_f.GetFlatObj().transform.position);

                }

            }

        }

        return retList.Count <= 0 ? null : retList;

    }


    /// <summary>
    /// 현재 맵의 중심 좌표를 반환.
    /// </summary>
    /// <returns></returns>
    public Vector3 GetCenterPos() {

        return new Vector3(
            (flatList[0][0].GetFlatObj().transform.position.x + flatList[flatList.Count - 1][flatList[flatList.Count - 1].Count - 1].GetFlatObj().transform.position.x) / 2,
            0,
            (flatList[0][0].GetFlatObj().transform.position.z + flatList[flatList.Count - 1][flatList[flatList.Count - 1].Count - 1].GetFlatObj().transform.position.z) / 2);

    }




    public void HideMap() {

        SetFlatVisible(false);
        SetWallsVisible(false, false, false, false);
        SetCornerVisible(false, false, false, false);


    }

    public void ShowMap() {

        SetFlatVisible(true);
        SetWallsVisible(true, true, true, true);
        SetCornerVisible(true, true, true, true);

    }

    public void RemoveMap() {

        for (int i = flatList.Count - 1; i >= 0; i--) {
            for (int j = flatList[i].Count - 1; j >= 0; j--) {
                flatList[i][j].DestroyFlatInfo();
                flatList[i].RemoveAt(j);
            }

            flatList.RemoveAt(i);
        }

        ClearGameObjectList(widthWallList_1);
        ClearGameObjectList(widthWallList_2);
        ClearGameObjectList(heightWallList_1);
        ClearGameObjectList(heightWallList_2);
        ClearGameObjectList(cornerList);

        Object.Destroy(flatParent);
        Object.Destroy(wallWidthParent_1);
        Object.Destroy(wallWidthParent_2);
        Object.Destroy(wallHeightParent_1);
        Object.Destroy(wallHeightParent_2);
        Object.Destroy(baseMapParent);

    }

    private void ClearGameObjectList(List<GameObject> _l) {
        for (int i = _l.Count - 1; i >= 0; i--) {
            Object.Destroy(_l[i]);
            _l.RemoveAt(i);
        }
    }


}


public partial class MapManager : MonoBehaviour {

    public enum MapState { VIEW_MODE, EDIT_MODE };
    
    //현재 모드 상태 저장.
    private MapState mapState;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject flatObject;
    [SerializeField]
    private GameObject wallObject;
    [SerializeField]
    private GameObject cornerWallObject;


    //맵 크기의 확대/축소 행동에 대해 등록될 옵저버 목록.
    private List<ExtendObserver> ExtendObserverL = new List<ExtendObserver>();

    //맵 생성, 제거 행동에 대해 등록될 옵저버 목록.
    private List<Map_Create_Destroy_Observer> MapCDObserverL = new List<Map_Create_Destroy_Observer>();

    //RayCaster 인터페이스
    private IRayCaster iRayCaster;

    [Header("Debug Ui")]
    //디버그용 입력 영역
    public InputField a;
    public InputField b;
    public InputField c;
    public InputField d;

    public Text debugModeState;


    private static MapManager instance;

    private void Awake() {

        if (instance == null) {

            instance = this;

        } else { 
        
            Destroy(gameObject);

        }

    }


    private void Start() {

        mapState = MapState.VIEW_MODE;
        iRayCaster = IRayCasterFactory.GetRayCaster();
        initFurnitureDic();

    }


    public void Update() {

        if (!CheckMapExist()) return;

        Vector3 cameraViewVector = new Vector3(Camera.main.transform.parent.position.x, Camera.main.transform.position.y, Camera.main.transform.parent.position.z) - Camera.main.transform.position;

        curMapData.VisibleSetting(cameraViewVector);

        UpdateSwitcher();

    }

    public static MapManager GetInstance() {

        return instance;

    }


    #region debug

    MapData savedMapData = null;
    private MapData curMapData = null;

    public void SaveMap() {

        if (!CheckMapExist()) return;
        if (savedMapData != null) {
            Debug.Log("이미 저장된 맵이 있음");
            return;
        }

        if (GetMapViewMode() != MapState.VIEW_MODE) {
            Debug.Log("관전 상태가 아닙니다.");
            return;
        }

        curMapData.HideMap();
        savedMapData = curMapData;
        curMapData = null;

        foreach (Map_Create_Destroy_Observer _o in MapCDObserverL) {
            _o.MapDestroy();
        }
    }

    public void LoadMap() {

        if (CheckMapExist()) return;
        if (savedMapData == null) {
            Debug.Log("저장된거 없음.");
        }

        curMapData = savedMapData;

        //가시화.
        curMapData.ShowMap();
        savedMapData = null;

        foreach (Map_Create_Destroy_Observer _o in MapCDObserverL) {
            _o.MapCreate(curMapData.GetCenterPos());
        }

    }

    #endregion


    /// <summary>
    /// 맵을 새로 생성.
    /// </summary>
    public void CreateMap() {

        if (CheckMapExist()) {

            Debug.Log("맵이 이미 존재합니다.");
            return;

        }

        // todo : => 값들의 유효성 검사 실행. 
        int inputWidth = int.Parse(a.text);//양수
        int inputHeight = int.Parse(b.text);//양수

        int inputTileWidth = int.Parse(c.text);//양의 짝수
        int inputTileHeight = int.Parse(d.text);//양의 짝수


        //맵 데이터 생성.
        curMapData = new MapData(inputWidth, inputHeight, inputTileWidth, inputTileHeight, flatObject, wallObject, cornerWallObject);


        foreach (Map_Create_Destroy_Observer _o in MapCDObserverL) {
            _o.MapCreate(curMapData.GetCenterPos());
        }

    }


    //todo : 처리영역 고려, 우선 보류.

    /// <summary>
    /// 타일맵 형식으로 현재 맵 정보를 List<List<int>> 타입으로 반환 [ 0 : 빈 공간, -1 : 벽 ]
    /// </summary>
    /// <returns>int형 2차원 배열 형식[ 맵이 없을 경우 null 반환 ]</returns>
    public List<List<int>> GetTilemapInfo() {

        if (!CheckMapExist()) {
            Debug.Log("맵이 없습니다.");
            return null;
        }

        List<List<int>> tilemapInfo = new List<List<int>>();

        foreach (List<FlatInfo> fiL in curMapData.GetFlatList()) {

            tilemapInfo.Add(new List<int>());

            foreach (FlatInfo flatInfo in fiL) {
                tilemapInfo[tilemapInfo.Count - 1].Add(flatInfo.GetIsEmpty() ? 0 : -1);
            }
        }


        return tilemapInfo;

    }

    /// <summary>
    /// 해당 좌표 영역 타일의 중심좌표를 ( x , y ) 형태로 반환.
    /// </summary>
    /// <returns>맵의 바깥이라면 [ int.MinValue, int.MinValue ] 반환</returns>
    public Vector2Int Pos_To_TileXY(Vector3 v) {


        float j = v.x + GetUnitWidth() / 2;
        float i = -(v.z - GetUnitHeight() / 2);

        Debug.Log($"before : {j}, {i}");

        if (j > 0 && j < GetWidth() * GetUnitWidth() && i > 0 && i < GetHeight() * GetUnitHeight()) {

            j = (int)(j / GetUnitWidth());
            i = (int)(i / GetUnitHeight());

        } else {
            Debug.Log("outer flat area detected");
            return new Vector2Int(int.MinValue, int.MinValue);
        }

        return new Vector2Int((int)j, (int)i);


    }

    /// <summary>
    /// 해당 타일맵의 중심좌표를 vec3 형태로 반환.
    /// </summary>
    /// <returns>vec3으로 반환. 맵 밖일 경우 [ int.MinValue, int.MinValue, int.MinValue ] 반환</returns>
    public Vector3 TileXY_To_Pos(Vector2Int _pos) {
        return TileXY_To_Pos(_pos.x, _pos.y);
    }

    /// <summary>
    /// 해당 타일맵의 중심좌표를 vec3 형태로 반환.
    /// </summary>
    /// <returns>vec3으로 반환. 맵 밖일 경우 [ vector3.negativeInfinity ] 반환</returns>
    public Vector3 TileXY_To_Pos(int _posX, int _posY) {

        if (_posX < 0 || _posX >= GetWidth() || _posY < 0 || _posY >= GetHeight()) {
            Debug.Log("타일맵 범위를 벗어난 좌표 변환 시도.");
            return Vector3.negativeInfinity;
        }

        return curMapData.GetFlatList()[_posY][_posX].GetFlatObj().transform.position;

    }

    /// <summary>
    ///맵 제거.
    /// </summary>
    public void RemoveMap() {

        if (!CheckMapExist()) {
            Debug.Log("지울 맵이 존재하지 않습니다.");
            return;
        }

        if (GetMapViewMode() != MapState.VIEW_MODE) {
            Debug.Log("관전 상태가 아닙니다.");
            return;
        }

        curMapData.RemoveMap();
        curMapData = null;

        foreach (Map_Create_Destroy_Observer _o in MapCDObserverL) {
            _o.MapDestroy();
        }

    }

    public bool CheckMapExist() {

        return curMapData != null ? true : false;

    }

    private void UpdateSwitcher() {

        switch (GetMapViewMode()) {

            case MapState.VIEW_MODE:
                //todo : 뷰 모드일 때 돌아갈 업데이트.

                //todo : 디버그용.
                if (Input.GetKeyDown(KeyCode.Z)) {
                    ChangeMapViewMode(MapState.EDIT_MODE);
                }

                break;

            case MapState.EDIT_MODE:
                EditModeUpdate();
                break;

            default:
                break;

        }

    }


    public MapState GetMapViewMode() {
        return mapState;
    }

    public void ChangeMapViewMode(MapState _mapViewMode) {

        //디버그용.
        debugModeState.text = "map state : " + _mapViewMode;
        mapState = _mapViewMode;

    }


    #region 게터

    public int GetOriginalWidth() {

        if (!CheckMapExist()) {
            return -1;
        }

        return curMapData.originalWidth;

    }

    public int GetOriginalHeight() {

        if (!CheckMapExist()) {
            return -1;
        }

        return curMapData.originalHeight;

    }

    public int GetBaseOffsetX() {
        if (!CheckMapExist()) {
            return -1;
        }

        return curMapData.baseOffsetX;
    }

    public int GetBaseOffsetZ() {
        if (!CheckMapExist()) {
            return -1;
        }

        return curMapData.baseOffsetZ;
    }

    public int GetWidth() {

        if (!CheckMapExist()) {
            return -1;
        }

        return curMapData.width;
    }

    public int GetHeight() {
        if (!CheckMapExist()) {
            return -1;
        }

        return curMapData.height;
    }

    public int GetUnitWidth() {

        if (!CheckMapExist()) {
            return -1;
        }

        return curMapData.tileWidth;
    }

    public int GetUnitHeight() {

        if (!CheckMapExist()) {
            return -1;
        }

        return curMapData.tileHeight;
    }

    public Vector3 GetEmptyFlatPos() {

        if (!CheckMapExist()) {
            return Vector3.negativeInfinity;
        }

        return curMapData.GetEmptyFlat();

    }

    #endregion

    #region 바닥 변화 함수 영역

    public void extendWidth() {

        if (!CheckMapExist()) {
            Debug.Log("맵 객체가 존재하지 않음.");
            return;
        }

        curMapData.ExtendWidth();

        notify();

    }

    public void reduceWidth() {

        if (!CheckMapExist()) {
            Debug.Log("맵 객체가 존재하지 않음.");
            return;
        }

        if (!curMapData.ReduceWidth()) return;

        notify();

    }

    public void extendHeight() {

        if (!CheckMapExist()) {
            Debug.Log("맵 객체가 존재하지 않음.");
            return;
        }

        curMapData.ExtendHeight();

        notify();

    }

    public void reduceHeight() {

        if (!CheckMapExist()) {
            Debug.Log("맵 객체가 존재하지 않음.");
            return;
        }


        if (!curMapData.ReduceHeight()) return;

        notify();

    }

    private void notify() {

        foreach (ExtendObserver e in ExtendObserverL) {
            e.OnExtended(curMapData.GetCenterPos(), curMapData.width, curMapData.height, curMapData.tileWidth, curMapData.tileHeight);
        }

    }

    #endregion


    public void AddExtendObserver(ExtendObserver _o) {
        ExtendObserverL.Add(_o);
    }
    public void AddMapCreateDestroyObserver(Map_Create_Destroy_Observer _o) {
        MapCDObserverL.Add(_o);
    }

}

