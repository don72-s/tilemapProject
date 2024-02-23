>타일맵 동작 구현 프로젝트
>===============================

<br>

>개요
>--
>
><br>
>
>* 타일맵의 뷰, 상호작용, 내부 시스템 등에 대해 개발하고 공부하는 프로젝트입니다.
>
><br>

![head](https://github.com/don72-s/tilemapProject/assets/66211881/008c2405-97ee-46ed-b237-498ba0ed1b7d)


<br>

># 목차
>
>
><h3>
>
>[1. 맵 구성](#맵_구성)   
>
><br>
>
>[2. 캐릭터 및 상호작용](#캐릭터_및_상호작용)   
>
><br>
>
>[3. 가구관리](#가구관리)   
>
>
></h3>
>
><h1></h1>

<br>
<br>
<br>
<br>



># 맵_구성
>
><h3>
>
> - [기본 구조](#기본_구조)
>
><br>
>
>- [생성과 삭제 및 구독](#생성과_삭제_및_구독)
>
><br>
>
>- [뷰의 회전](#뷰의_회전)
>
></h3>
>
><h1></h1>
<br><br>

>## 기본_구조
>맵은 기본적으로 타일 형식으로 구성되며 x,z방향으로의 단방향 확장/축소가 가능하다.   
>해당 타일을 2차원 배열 형식으로 관리하며 맵 자체의 정보를 관리하는 MapData 클래스를 정의하여 이용한다.   
>
>
>```cpp
>class MapData{
>  ...생략...
>
>  //FlatInfo는 바닥단위의 정보를 저장하는 2차원 배열.
>  private List<List<FlatInfo>> flatList; 
>
>  //사방의 벽과 코너를 저장하는 리스트.
>  private List<GameObject> widthWallList_1;
>  private List<GameObject> widthWallList_2;
>  private List<GameObject> heightWallList_1;
>  private List<GameObject> heightWallList_2;
>  private List<GameObject> cornerList;
>
>  //맵 확장 & 축소 관련 메소드 제공.
>  public void ExtendWidth() { ... }
>  public bool ReduceWidth() { ... }
>  ...
>
>  //벽, 코너기둥등의 가시 여부를 설정하는 메소드 제공.
>  public void SetWallsVisible(bool _widthWall_1, bool _widthWall_2, bool _heightWall_1, bool _heightWall_2) { ... }
>  ...
>  ...생략...
>}
> ```
<br><br>


>## 생성과_삭제_및_구독
>
>
>위의 MapData클래스를 생성하거나 제거(참조취소)하는 식의 동작으로 구현되며 이는 MapManager 오브젝트에서
>관리하고 실행한다.
><br><br>
>
>>## 생성
>>
>>기본적으로 **가로(width)**, **세로(height)** 의 길이와 **단위너비(unitWidth)**, **단위높이(unitHeight)** 를 입력받아 맵을 생성한다.   
>>**※단, 정확한 연산을 위해 단위너비, 단위높이는 짝수로 입력할 것을 권장한다.**
>><h1></h1>
>>
>>### MapManager의 CreateMap 메소드
>> ```cpp
>> public void CreateMap(){
>>   ...생략...
>>
>>   //맵 데이터 생성 (뒤에서 3종류의 매개변수는 instantiate시킬 gameobject instance)
>>   curMapData = new MapData(inputWidth, inputHeight, inputTileWidth, /*inputTileHeight, flatObject, wallObject, cornerWallObject*/);
>>
>>   //맵이 생성될 때 필요한 동작을 위해 구독한 객체들에게 알림.
>>   foreach (Map_Create_Destroy_Observer _o in MapCDObserverL) {
>>      _o.MapCreate(curMapData.GetCenterPos());
>>   }
>> }
>> ```
>>
><br><br>
>
>>## 삭제
>>
>>**curMapData** 변수에 저장된 **MapData**타입의 객체를 이용하여 맵에 관련된 동작을 실행하므로, 필요한 행위를 마친 뒤   
>> **curMapData**를 null로 지정하는것으로 구현.
>><h1></h1>
>>
>>```cpp 
>>public void RemoveMap(){
>>  ...생략...
>>
>>  //제거전에 필요한 사전 작업.
>>  curMapData.RemoveMap();
>>
>>  //실질적인 제거
>>  curMapData = null;
>>
>>  //맵이 제거될 때 필요한 동작을 위해 구독한 객체들에게 알림.
>>  foreach (Map_Create_Destroy_Observer _o in MapCDObserverL)
>>  {
>>    _o.MapDestroy();
>>  }
>>}
>>```
>>
>>
><br><br>
>
>>## 옵저버의 등록
>>맵의 확장/축소와 생성/제거가 일어날 때 알림을 원하는 객체들은 알림을 받을 수 있도록 옵저버 패턴을 적용   
>>**확장/축소**옵저버와 **생성/제거**옵저버로 구분.
>>
>><h1></h1>
>>
>>MapManager에 구현된 등록 요청 메소드
>>```cpp 
>>      //확장&축소 옵저버 등록
 > >     public void AddExtendObserver(ExtendObserver _o)
 > >     {
 > >         ExtendObserverL.Add(_o);
 > >     }
>>
>>      //생성&제거 옵저버 등록
 > >     public void AddMapCreateDestroyObserver(Map_Create_Destroy_Observer _o)
 > >     {
 > >         MapCDObserverL.Add(_o);
 > >     }
>>```
>><br>
>>옵저버 등록을 위해 구현해야 하는 메소드가 정의된 인터페이스.
>>
>>```cpp
>>
>>  ///확장&축소 옵저버 등록을 위한 인터페이스
 > > public interface ExtendObserver
 > > {
 > > 
 > >     void OnExtended(Vector3 _centerPos, int _width, int _height, int _unitWidth, int _unitHeight);
 > > 
 > > }
 > > 
>>  ///생성&제거 옵저버 등록을 위한 인터페이스
 > > public interface Map_Create_Destroy_Observer {
>>
 > >     void MapCreate(Vector3 centerPos);
 > > 
 > >     void MapDestroy();
 > > 
 > > }
>>```
>>
<br><br>

>## 뷰의_회전
>맵과 그 구성요소는 **현재상태를 유지**하며 뷰 시점에 해당하는 **메인 카메라**를 회전시킨다.   
><br>방향성이 있는 객체(ex : 2D 스프라이트)들과 시점에 따라 관측성이 변화하는(벽, 코너기둥)객체들은 카메라의 전방 벡터와의
>내적 연산을 통해 관측성, 방향등을 조정한다.   
><br>다만 관측 방향의 특성상 카메라의 전방 벡터가 사선 아래로 향해있으므로, 수평방향 보정 역할을 할 부모 객체(empty object)를 생성해
>이를 부모로 설정한 뒤, 해당 객체의 z단위벡터를 이용하기로 한다.
>
>![camera](https://github.com/don72-s/tilemapProject/assets/66211881/43dd446c-0c05-4481-9494-901dafd214be)
>![cameraParent](https://github.com/don72-s/tilemapProject/assets/66211881/30851dca-132c-417d-9576-3c33ee449e8b)
>
>**Z좌표인 푸른색 화살표가 아래를 향한 메인카메라와 수평을 바라보고있는 부모 오브젝트**
>
><br>
>회전의 입력은 스크롤바로 받게되며, 카메라의 부모 오브젝트를 회전시켜 메인카메라와 회전 중심축 사이의 연산을 최소화.<br><br>
>   
> >### 부모 오브젝트의 코드   
> >
> >```cpp
> > 
> >     //스크롤바의 입력 정도를 변수에 저장.
> >     public void ChangeRotWithScroll(float _value) {
> >         rotValue = (int)((_value - 0.5f) * 500);
> >     }
> > 
> >     //해당 변수값에 의존해 rot값을 변환.
> >     void Update() {
> > 
> >         if (rotValue != 0)
> >         {
> >            transform.Rotate(new Vector3(0, rotValue * Time.deltaTime, 0));
> >         }
> > 
> >     }
> >```
><br>
>
> >### MapManager에서는 지속적으로 z벡터와 내적하여 벽의 가시 여부를 연산/갱신
> >
> >```cpp
> > 
> >     //================================== MapManager의 Update문 =================================
> >     public void Update()
> >     {
> > 
> >         if (!CheckMapExist()) return;
> > 
> >         //y좌표는 동일선상에 둔 채 x,z좌표간의 차이만을 가지는 메인카메라 -> 부모 객체 방향의 벡터 생성
> >         Vector3 cameraViewVector = new Vector3(Camera.main.transform.parent.position.x, Camera.main.transform.position.y, Camera.main.transform.parent.position.z) - Camera.main.transform.position;
> > 
> >         //현재 유효한 MapData객체에 주시방향벡터정보 전달.
> >         curMapData.VisibleSetting(cameraViewVector);
> > 
> >     }
> > 
> > 
> >     //================================== MapData의 VisibleSetting 메소드=========================
> >     public void VisibleSetting(Vector3 _camera)
> >     {
> >         //각자 벽의 전방벡터와 내적연산 후 활성화 여부 저장.
> >         bool widthWall_1 = Vector3.Dot(_camera, wallWidthParent_1.transform.forward) > 0.0001f ? false : true;
> >         bool heightWall_1 = Vector3.Dot(_camera, wallHeightParent_1.transform.forward) > 0.0001f ? false : true;
> >         bool widthWall_2 = Vector3.Dot(_camera, wallWidthParent_2.transform.forward) > 0.0001f ? false : true;
> >         bool heightWall_2 = Vector3.Dot(_camera, wallHeightParent_2.transform.forward) > 0.0001f ? false : true;
> > 
> >         //벽과 코너의 활성화 설정
> >         SetWallsVisible(widthWall_1, widthWall_2, heightWall_1, heightWall_2);
> >         SetCornerVisible(widthWall_1 && heightWall_1, widthWall_1 && heightWall_2, widthWall_2 && heightWall_1, widthWall_2 && heightWall_2);
> >     }
> > 
><br>
>
>>### 동작 예시
>>![spining](https://github.com/don72-s/tilemapProject/assets/66211881/f6134ef9-e764-4537-bde1-14ca0df6e62a)
>>#### 카메라 주시 방향에 따라 벽이 비활성화됨을 확인할 수 있다.
>>




<br>
<br>
<br>
<br>

># 캐릭터_및_상호작용
>
><h3>
>
> - [애니메이션](#애니메이션)
>
><br>
>
>- [상태 및 상호작용](#상태_및_상호작용)
>
><br>
>
>- [배치와 길찾기](#배치와_길찾기)
>
><br>
>
>- [행동 블록 정의](#행동_블록_정의)
>
></h3>
>
><h1></h1>
<br><br>

>## 애니메이션
>
>우선 눈에 보이는 캐릭터가 필요하므로 어디선가 캐릭터 스프라이트를 가져와서 애니메이션을 만든다.   
><br> **idle, walk, touched, taking 4가지를 구상**
>
>![idle](https://github.com/don72-s/tilemapProject/assets/66211881/a9d75474-791d-4e08-b9f7-660117489cd4)
>![walk](https://github.com/don72-s/tilemapProject/assets/66211881/aa53bb6d-705a-4c1c-922e-7f1343fa9462)
>![touched](https://github.com/don72-s/tilemapProject/assets/66211881/fa3256b2-ff58-4f89-947d-31e5ef2672a2)
>![taking](https://github.com/don72-s/tilemapProject/assets/66211881/382cb72f-0227-45fd-80cd-8c4ee0377068)
>
><br><br>
>
>**다음으로 애니메이션 상태 전이도를 조건에 맞게 설정한다.**   
>![animator](https://github.com/don72-s/tilemapProject/assets/66211881/2a870363-5b94-45bc-8745-24ff0762d90c)![setting](https://github.com/don72-s/tilemapProject/assets/66211881/fef6a604-f3ff-46bd-9a68-39cfb7fd75a8)   
>상태전환간의 즉각적인 변화를 위해 crossfading옵션을 꺼주고, 전환텀도 0으로 바꿔준다.
>
<br><br>


>## 상태_및_상호작용
>
>유한상태기계를 이용한 간략한 상태 패턴을 설계하고 입력에 대응하는 핸들러 함수들을 설계한다.   
>또한 캐릭터의 행동과 관련된 모든 행위는 **Character**클래스를 정의하여 구현한다.   
><br><br>**Character**클래스는 맵의 변화에 민감하므로 맵의 변화에 대한 옵저버를 구독한다.   
>```cpp
>public class Character : MonoBehaviour, ExtendObserver, Map_Create_Destroy_Observer{
>
>    ...생략...
>
>    public void Start()
>    {
>        //MapManager인스턴스를 가져오고 맵 변화 옵저버를 구독.
>         
>        mapScript = mapManager.GetComponent<MapManager>();
> 
>        mapScript.AddExtendObserver(this);
>        mapScript.AddMapCreateDestroyObserver(this);
>         
>    }
>
>    ...생략...
>
>    //확장, 축소 옵저버 메소드 오버라이드.
>    public void OnExtended(Vector3 _centerPos, int _width, int _height, int _unitWidth, int _unitHeight)
>    {
>        //맵이 확장되거나 축소되었을 때 대응할 행동 정의.
>    }
> 
>    //맵 생성 옵저버 메소드 오버라이드.
>    public void MapCreate(Vector3 _centerPos)
>    {
>        initCharacter();
>        gameObject.SetActive(true);
>    }
> 
>    //맵 제거 옵저버 메소드 오버라이드.
>    public void MapDestroy()
>    {
>        gameObject.SetActive(false);
>    }
>}
>```
><br><br>
>
>>## 상태 정의
>>
>>캐릭터가 가질 상태와 대응하는 4가지 상태 **idle, walk, touched, taking**를 정의하며 curState변수로 본인의 상태를 정의한다.   
>>```cpp
>>public class Character : MonoBehaviour, ExtendObserver, Map_Create_Destroy_Observer{
>>
>>  ...생략...
>>
>>  //상태를 enum으로 정의
>>  public enum STATE { IDLE, ACTING, TAKING, DRAG, TOUCHED };
>>  private STATE curState;
>>
>>  ...생략...
>>
>>}
>>```
><br><br>
>
>>## 상태 전이
>>
>>다른 상태로 전이를 원할 때 호출될 함수를 정의해둔다.
> >```cpp
> > public void ChangeState(STATE _state)
> > {
> >
>>  //상태 전환 시 할 행동 정의.
>>  switch (_state)
> >  {
> >    case STATE.ACTING:
> >      ChangeAnimation(STATE.ACTING);
> >      break;
> >
> >    case STATE.IDLE:
> >      ChangeAnimation(STATE.IDLE);
> >      break;
> > 
> >    case STATE.DRAG:
> >      ChangeAnimation(STATE.DRAG);
> >      break;
> > 
> >    case STATE.TOUCHED:
> >      OffsetInit();
> >      ClearActionQueue();
> >      break;
> >   }
> >
>>  //상태를 전환.
> >  curState = _state;
> >}
>>```
><br><br>
>
>>### 상호작용
>>
>>캐릭터는 2d 스프라이트기 때문에 카메라의 시점 회전에 따라 rot가 변한다. 하지만 다른 객체들은 움직이지 않고 다른 객체들과의 연산을 위해서는
>>회전해서는 안되므로, 이전의 카메라와 비슷한 방식을 사용한다.   
>><br>
>>실제 캐릭터의 입력과 시각화 출력을 담당하는 **CharacterHitHandler**스크립트를 작성한 뒤, 해당 스크립트를 가진 객체를 **Character**객체의 자식으로 지정한다.   
>><br>
>>이후 받은 입력을 부모**Character**의 행동 대응 함수를 호출하는 식의 구조를 설계한다.   
>> ```cpp
>>     //마우스 입력에 따른 행동 처리 분기
>>     private void ChangeMouseState(MOUSE_STATE _state) {
> > 
> >         switch (_state) {
> > 
> >             case MOUSE_STATE.MOUSE_DOWN:
> >                 charComp.ChangeState(Character.STATE.TOUCHED);
> >                 charComp.changeAcceptable(Character.ACCEPTABLE.DENY);
> >                 delatT = 0;
> >                 break;
> > 
> >             case MOUSE_STATE.MOUSE_UP:
> > 
> >                 if (charComp.getState() == Character.STATE.TOUCHED)
> >                 {
> >                     charComp.TouchedAction();
> >                 }
> >                 else if (charComp.getState() == Character.STATE.DRAG)
> >                 {
> >                     charComp.ChangeState(Character.STATE.IDLE);
> >                     charComp.setTimeDelayOffset(3);
> >                     charComp.DragReleased();
> >                 }
> >                 else {
> >                     Debug.Log("mouse release occured!");
> >                 }
> > 
> >                 charComp.changeAcceptable(Character.ACCEPTABLE.ACCEPT);
> > 
> >                 break;
> > 
> >             default:
> >                 break;
> > 
> >         }
> > 
> >         mouseState = _state;
> >     }
>> ```
>>
<br><br>

>## 배치와_길찾기
>
>캐릭터를 드래그하는 등의 이동에 의해 필요한 재배치 기능과 캐릭터의 경로탐색 기능을 구현한다.
>
><br>
>
> >### 배치
> >캐릭터의 배치는 해당 타일이 유효한지, 맵의 바깥인지 두가지의 예외상황을 제외하면 문제가 발생하지 않는다.
> >
> >연산을 위해 캐릭터가 활성화 될 때 가져온 맵의 정보와 MapData에 구현한 좌표변환 메소드를 이용하여 유효성을 판단한다.
> >```cpp
> >//initCharacter메소드에 존재하는 맵 정보 초기화 영역
> >public void initCharacter(){
> >
> >    width = mapScript.GetWidth();
> >    height = mapScript.GetHeight();
> >    widthOffsetCount = mapScript.GetOriginalWidth() / 2;
> >    heightOffsetCount = mapScript.GetOriginalHeight() / 2;
> >    unitMultiplizerX = mapScript.GetUnitWidth();
> >    unitMultiplizerZ = mapScript.GetUnitHeight();
> >    rangeWidthOffset = mapScript.GetOriginalWidth() % 2 == 0 ? mapScript.GetUnitWidth() / 2 : 0;
> >    rangeHeightOffset = mapScript.GetOriginalHeight() % 2 == 0 ? mapScript.GetUnitHeight() / 2 : 0;
> > 
> >}
> >
> >```
> >```cpp
> >//MapData클래스에 있는 좌표변환 메소드, 3차원 좌표를 타일좌표로 또는 그 반대로 변환해 반환한다.
> >public Vector2Int Pos_To_TileXY(Vector3 v){ ... }
> >public Vector3 TileXY_To_Pos(Vector2Int _pos){ ... }
> >
> >```
> >
><br><br>
>
>>
>>### 유효성 판단과 재배치
>><br>
>>캐릭터의 드래그 또는 배치 시 유효한 좌표인지 확인하는 작업을 거친다.   
>><br><br><br>
>>
> >>### 드래그 범위의 보정
> >>init시에 가져왔던 정보를 바탕으로 캐릭터 드래그의 최대/최소의 범위를 보정한다.
> >>```cpp
> > >   
> > >   new Vector3(Mathf.Clamp(tmpPos.x, -widthOffsetCount * unitMultiplizerX + rangeWidthOffset, (width - widthOffsetCount - 1) * unitMultiplizerX + rangeWidthOffset),
> > >      tmpPos.y,
> > >      Mathf.Clamp(tmpPos.z, -((height - heightOffsetCount - 1) * unitMultiplizerZ + rangeHeightOffset), -(-heightOffsetCount * unitMultiplizerZ + rangeHeightOffset))
> > >      );
> >>
> >>```
>>><br>
>>>
> >>![clamp](https://github.com/don72-s/tilemapProject/assets/66211881/58539845-8784-42fd-ba29-a862d6d448f6)   
>>>**맵의 최대 범위를 벗어나지 않음을 확인**
>>
> ><br><br>
>>
> >>### 좌표의 유효성 확인 및 재배치
> >>**MapData**클래스에서 구현한 **GetTilemapInfo** 메소드를 이용해 유효성을 판단한다.   
> >><br>
> >>**GetTilemapInfo**메소드는 int형 2차원 리스트를 반환하며 유효할 경우 0, 유효하지 않을 경우 -1의 값을 가지는 리스트다.   
> >><br>
> >>이후 부적절한 배치 위치를 가진다면, **initCharacter** 메소드를 이용해 임의의 유효한 좌표로 재배치한다.
> > >```cpp
> >>   //판단할 좌표를 tile좌표로 변환
> >>   Vector2Int pos = mapScript.Pos_To_TileXY(new Vector3(transform.position.x, heightOffset, transform.position.z));
> >>
> >>   //배치하기에 유효한 좌표인지 체크
> >>   if (mapScript.GetTilemapInfo()[pos.y][pos.x] == -1)
> > >   {
> >>      //부적절한 좌표인 경우 랜덤으로 재배치
> > >      initCharacter();
> > >   }
> >> ```
>>><br>
>>>
>>>![repos](https://github.com/don72-s/tilemapProject/assets/66211881/1941ae8e-c456-4930-bced-9047ffe7cf89)   
>>>**유효하지 않을 경우 랜덤으로 재배치됨을 확인**   
>
><br>
>
>>### 길찾기
>>해당 프로젝트에서의 길찾기는 **반드시 최적의 경로일 필요성**을 느끼지 못함, 3D지만 캐릭터의 이동범위는 **2D타일맵**으로 한정   
>>이와같은 이유로 길찾기 알고리즘은 A*를 채택.   
>><br>
>>
>>A*의 역할은 (정보 입력) -> (연산) -> (결과 경로 반환) 의 순으로 이루어지므로 **static class**로 구성해도 중분하다고 생각하여 **static class**로 구성   
>><br>
>>**heuristic**은 단순하게 출발지~목적지 사이의 (가로길이 + 세로길이) 로 지정   
>>
> >```cpp
> > 
> > //맵의 유효성 정보를 가진 2차원 리스트를 받아와 출발지부터 도착지까지의 경로를 찾는 메소드
> > public static List<Vector2Int> FindPath(List<List<int>> pathList, Vector2Int _startPos, Vector2Int _endPos) {
> > 
> > ...생략...
> > 
> >         while (openQueue.Count > 0) {
> > 
> >             ...생략...
> > 
> >             //목적지 도착 여부
> >             if (nextNode.myPos.x == _endPos.x && nextNode.myPos.y == _endPos.y) {
> > 
> >                 Node printNode = nextNode;
> >                 List<Vector2Int> retL = new List<Vector2Int>();
> > 
> >                 while (!(printNode.myPos.x == printNode.parent.x && printNode.myPos.y == printNode.parent.y)) {
> > 
> >                     retL.Add(new Vector2Int(printNode.myPos.x, printNode.myPos.y));
> >                     printNode = NodeL[printNode.parent.y][printNode.parent.x];
> >                 }
> > 
> >                 retL.Add(new Vector2Int(printNode.myPos.x, printNode.myPos.y));
> > 
> >                 //도착지로부터 출발지까지의 타일 좌표 경로를 리스트로 반환.
> >                 return retL;
> >             }
> > 
> >             
> >             //주위 노드들을 순회하며 탐색.
> >             //대각선 우선 탐색.
> >             checkPath(nextNode, curX + 1, curY - 1, _endPos.x, _endPos.y, width, height, true);
> >             checkPath(nextNode, curX + 1, curY + 1, _endPos.x, _endPos.y, width, height, true);
> >             checkPath(nextNode, curX - 1, curY + 1, _endPos.x, _endPos.y, width, height, true);
> >             checkPath(nextNode, curX - 1, curY - 1, _endPos.x, _endPos.y, width, height, true);
> > 
> >             //전후좌우 탐색
> >             checkPath(nextNode, curX + 1, curY, _endPos.x, _endPos.y, width, height);
> >             checkPath(nextNode, curX, curY + 1, _endPos.x, _endPos.y, width, height);
> >             checkPath(nextNode, curX - 1, curY, _endPos.x, _endPos.y, width, height);
> >             checkPath(nextNode, curX, curY - 1, _endPos.x, _endPos.y, width, height);
> > 
> >         }
> > 
> >         //목적지 도달 불가.
> > 
> >         return null;
> >  
> > }
> > 
> > 
> > //_fromNode로부터 _checkPos까지의 openNode설정의 역할을 하는 메소드
> > private static void checkPath(Node _fromNode, int _checkX, int _checkY,int _endX, int _endY, int _mapWidth, int _mapHeight, bool _isCross = false) {
> > 
> >         ...생략...
> >         
> >         if (openQueue.ContainsKey((_checkX, _checkY)))//openL의 재방문인 경우
> >         {
> >             if (openQueue[(_checkX, _checkY)].curCost > _fromNode.curCost + moveCost) {//기존 open을 갱신할지 여부
> >                 openQueue[(_checkX, _checkY)].parent = new Vector2Int(fromX, fromY);
> >                 openQueue[(_checkX, _checkY)].curCost = _fromNode.curCost + moveCost;
> > 
> >             }
> > 
> >         }
> >         else {//첫 방문인 경우
> > 
> >             openQueue.Add((_checkX, _checkY), NodeL[_checkY][_checkX]);
> > 
> >             openQueue[(_checkX, _checkY)].huristic = (Mathf.Abs(_endX - _checkX) + Mathf.Abs(_endY - _checkY)) * 10;
> >             openQueue[(_checkX, _checkY)].parent = new Vector2Int(fromX, fromY);
> >             openQueue[(_checkX, _checkY)].curCost = _fromNode.curCost + moveCost;
> >         }
> > 
> >     }
> >```
>>
>>캐릭터 상호작용에서 첫 캐릭터 클릭 시 캐릭터의 좌표를 시작 좌표, 두번때 클릭시의 빈 좌표를 도착 좌표로 설정하여 길찾기 및 이동 시행   
>>
>>![astar](https://github.com/don72-s/tilemapProject/assets/66211881/82ec4808-7377-41e3-8ac1-ee61d45fd628)   
>>**최단거리가 아니더라도 상관없으므로 설계 의도대로 작동한다고 판단**


<br><br>


<br>
<br>
<br>
<br>

# 가구관리

<br>
<br>
<br>
<br>
