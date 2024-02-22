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

>목차
>--
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

# 캐릭터_및_상호작용

<br>
<br>
<br>
<br>

# 가구관리

<br>
<br>
<br>
<br>
