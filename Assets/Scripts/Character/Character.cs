using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


class ActionBlock
{

    protected Character.STATE state;
    protected Character.ACCEPTABLE acceptable;
    protected ICharacterAction character;

    public ActionBlock() { }

    public ActionBlock(Character.STATE _state, Character.ACCEPTABLE _acceptable, ICharacterAction _character)
    {
        state = _state;
        acceptable = _acceptable;
        character = _character;
    }

    public Character.STATE getState()
    {
        return state;
    }

    public Character.ACCEPTABLE getAcceptable()
    {
        return acceptable;
    }

    public virtual void PlayAction()
    {
        character.PlayIdleAction();
    }

}

class MoveActionBlock : ActionBlock
{

    private Vector3 endPos;

    public MoveActionBlock(Character.STATE _state, Character.ACCEPTABLE _acceptable, ICharacterAction _character, Vector3 _endPos)
    {
        state = _state;
        acceptable = _acceptable;
        endPos = _endPos;
        character = _character;
    }

    public Vector3 GetEndPos() {
        return endPos;
    }

    override public void PlayAction()
    {
        character.PlayMoveAction(endPos);
    }


}



public class Character : MonoBehaviour, ExtendObserver, Map_Create_Destroy_Observer, ICharacterAction
{

    public enum STATE { IDLE, ACTING, TAKING, DRAG, TOUCHED };
    public enum ACCEPTABLE { ACCEPT, DENY };

    //현재 캐릭터의 상태 의미
    private STATE curState;
    private ACCEPTABLE accState;

    //행동블록이 유지되는 시간 설정.
    private const float decideDelay = 1;
    private float timerTick;

    //목적지와 이동방향, 초과 이동 확인용 거리 및 벡터
    private Vector3 destVec;
    private Vector3 dirVec;
    private float lastDistance;

    //캐릭터의 속도
    public float moveSpeed;

    //캐릭터의 높이 오프셋
    public float heightOffset;

    //ray의 각도에 따른 캐릭터 터치 위치 보정 백터
    Vector3 touchOffsetVec = Vector3.zero;

    //예정된 행동 큐 정의
    //행동은 반환값이 없다는 가정하에 Action타입으로 정의
    private List<ActionBlock> actionQueue;


    //관리용 애니메이터
    private Animator animator;
    private Dictionary<STATE, int> aniHashDic;


    private SpriteRenderer spriteRenderer;

    private GameObject mapManager;
    private MapManager mapScript;

    private IRayCaster iRayCaster;


    private void Awake()
    {
        animator = gameObject.GetComponentInChildren<Animator>();
        spriteRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();
        aniHashDic = new Dictionary<STATE, int> {
            { STATE.IDLE, Animator.StringToHash("Idle") },
            { STATE.ACTING, Animator.StringToHash("Walk") },
            { STATE.DRAG, Animator.StringToHash("Taking") },
            { STATE.TOUCHED, Animator.StringToHash("Touched") }
        };

    }

    public void Start()
    {

        gameObject.SetActive(false);

        iRayCaster = IRayCasterFactory.GetRayCaster();
        mapScript = MapManager.GetInstance();

        mapScript.AddExtendObserver(this);
        mapScript.AddMapCreateDestroyObserver(this);

        actionQueue = new List<ActionBlock>();

    }

    public void initCharacter()
    {
        timerTick = 0;

        ChangeState(STATE.IDLE);
        changeAcceptable(ACCEPTABLE.ACCEPT);
        changeDirectionSprite(true);

        destVec = Vector3.zero;
        dirVec = Vector3.zero;

        ClearActionQueue();

        width = mapScript.GetWidth();
        height = mapScript.GetHeight();
        widthOffsetCount = mapScript.GetOriginalWidth() / 2;
        heightOffsetCount = mapScript.GetOriginalHeight() / 2;
        unitMultiplizerX = mapScript.GetUnitWidth();
        unitMultiplizerZ = mapScript.GetUnitHeight();
        rangeWidthOffset = mapScript.GetOriginalWidth() % 2 == 0 ? mapScript.GetUnitWidth() / 2 : 0;
        rangeHeightOffset = mapScript.GetOriginalHeight() % 2 == 0 ? mapScript.GetUnitHeight() / 2 : 0;

        Vector3 initPos = mapScript.GetEmptyFlatPos();

        if (initPos == Vector3.negativeInfinity)
        {
            Debug.LogError("빈 좌표공간이 없음.");
            transform.position = new Vector3(0, heightOffset, 0);
        }
        else {
            initPos.y = heightOffset;
            transform.position = initPos;
        }

    }

    public void Update()
    {

        AddTimerTick();
        ActionSwitcher();

        #region 좌우방향 판정 및 스프라이트 플립

        Vector3 v = Camera.main.transform.right;
        float dot = Vector3.Dot(dirVec, v);

        changeDirectionSprite(dot >= 0 ? true : false);

        #endregion

    }


    /// <summary>
    /// 타이머 진행
    /// </summary>
    void AddTimerTick()
    {
        if (timerTick < decideDelay)
        {
            timerTick += Time.deltaTime;
        }
        else
        {
            if (accState == ACCEPTABLE.DENY) return;//조건에 따른 행동이 끝나지 않았을 경우 대기.
            //다음 행동 실행.
            actionQueuePop();
            timerTick = 0;
        }

    }


    /// <summary>
    /// 액션큐에서 pop하여 행동 수행을 시작하는 함수.
    /// </summary>
    void actionQueuePop()
    {
        //액션큐에 다음 행동이 없을 경우.
        if (actionQueue.Count <= 0)
        {
            //임시로 디폴트 행위 지정 => 두 지점 왕복하게 만듦.

            #region 임시로 자동 행위 지정해놓은 영역, 정책에 따라 정의 필요 [ todo : ]

            AddMoveAction(1, 3);
            AddMoveAction(5, 1);


            if (actionQueue.Count <= 0) {
                Debug.Log("움직임 정의 실패");
                return;
            }

            #endregion
        }


        //행동블록 실행.
        ActionBlock ab = actionQueue[0];
        actionQueue.RemoveAt(0);
        changeAcceptable(ab.getAcceptable());
        ChangeState(ab.getState());
        ab.PlayAction();

    }

    /// <summary>
    /// 현재의 액션큐를 모두 비움.
    /// </summary>
    public void ClearActionQueue() {
        actionQueue.Clear();
        changeAcceptable(ACCEPTABLE.ACCEPT);
        Debug.Log("액션 큐 초기화됨.");
        return;
    }

    /// <summary>
    /// 상태를 변환시키고 싶을 때 호출되는 함수.
    ///  + 상태 변환시 최초 한번 호출될 메소드들을 등록.
    /// </summary>
    /// <param name="_state">전환 및 진입할 상태</param>
    public void ChangeState(STATE _state)
    {

        //character debuger
        //Debug.Log("state : " + _state);

        switch (_state)
        {
            case STATE.ACTING:
                ChangeAnimation(STATE.ACTING);
                break;

            case STATE.IDLE:
                ChangeAnimation(STATE.IDLE);
                break;

            case STATE.DRAG:
                ChangeAnimation(STATE.DRAG);
                break;

            case STATE.TOUCHED:
                OffsetInit();
                ClearActionQueue();
                break;
        }

        curState = _state;
    }

    /// <summary>
    /// 애니메이션을 전환하는 함수.
    /// </summary>
    /// <param name="destState">목표로 하는 애니메이션 상태.</param>
    private void ChangeAnimation(STATE _destState) {

        if (!gameObject.activeSelf)
            return;

        if (aniHashDic.ContainsKey(_destState)) {

            animator.Play(aniHashDic[_destState]);

        } else {

            Debug.LogWarning("상태에 대응하는 애니메이션이 존재하지 않음.");

        }

    }


    /// <summary>
    /// 현재 상태에 따라 계속해서 호출될 동작(함수)들을 등록.
    /// </summary>
    void ActionSwitcher()
    {

        switch (curState)
        {

            case STATE.IDLE:
                break;

            case STATE.ACTING:
                moving();
                break;

            case STATE.DRAG:
                DragUpdate();
                break;

            case STATE.TOUCHED:
                break;

            default:
                break;

        }

    }


    /// <summary>
    /// 캐릭터가 최초 터치되었을때의 바닥과 터치 위치간의 offset 설정.
    /// </summary>
    private void OffsetInit()
    {
        Vector3 charHitPoint = iRayCaster.GetCastHitPoint(LayerMasks.GetLayerMask(LayerMaskName.CHARACTER));
        Vector3 flatHitPoint = iRayCaster.GetCastHitPoint(LayerMasks.GetLayerMask(LayerMaskName.FLAT));

        if (!charHitPoint.Equals(Vector3.negativeInfinity) && !flatHitPoint.Equals(Vector3.negativeInfinity))
        {
                         //충돌바닥 -> 충돌 캐릭터 지점 // + // 충돌 캐릭터 지점 -> 캐릭터 객체 중심좌표  :  충돌바닥 지점 -> 캐릭터 객체 중심좌표
            touchOffsetVec = (charHitPoint - flatHitPoint) + (transform.position - charHitPoint);
        }
        else
        {
            //todo : 예외처리
            Debug.Log("충돌 대상 존재x");
        }


    }


    /// <summary>
    /// 드래그되었을 때 계속해서 호출될 함수.
    /// </summary>
    private void DragUpdate()
    {

        Vector3 flatHitPoint = iRayCaster.GetCastHitPoint(LayerMasks.GetLayerMask(LayerMaskName.FLAT));
        if (!flatHitPoint.Equals(Vector3.negativeInfinity))
        {
            Vector3 tmpPos = flatHitPoint + touchOffsetVec;

            tmpPos = new Vector3(Mathf.Clamp(tmpPos.x, 0, (width - 1) * unitMultiplizerX),
                                  tmpPos.y,
                                  Mathf.Clamp(tmpPos.z, -((height - 1) * unitMultiplizerZ), 0)
                                );

            transform.position = tmpPos;
        }
        else
        {
            Debug.Log("충돌체 발견 x");
            //todo : 백업좌표 or 기본좌표 지정 및 이동
            //예외상황 : 끌어 움직이는데 제한 영역을 벗어난 경우.
        }

    }


    //todo : 이동 알고리즘에 따라서 내용 수정 필요.
    /// <summary>
    /// 움직임 상태일 때 계속해서 호출될 함수.
    /// </summary>
    void moving()
    {

        float curDistance = (destVec - transform.position).magnitude;


        if (curDistance < 0.07f || (lastDistance - curDistance) < 0)
        {
            changeAcceptable(ACCEPTABLE.ACCEPT);

            transform.position = destVec;
            
            if (actionQueue.Count > 0)
            {
                setTimeDelayOffset(0);//바로 다음행동하도록 지시
            }
        }
        else
        {
            transform.Translate(dirVec * moveSpeed * Time.deltaTime);
            lastDistance = curDistance;
        }

    }



    #region 대응 호출 함수(외부 호출 가능)

    /// <summary>
    /// 캐릭터 터치 이벤트가 발생했을 경우 호출될 함수.
    /// </summary>
    public void TouchedAction()
    {
        ChangeAnimation(STATE.TOUCHED);

        //5초동안 touched 애니메이션 재생.
        setTimeDelayOffset(5);

        //이후 idle 액션을 실행.
        AddIdleAction();
    }

    /// <summary>
    /// 드래그가 종료되었을 때 호출되는 함수.
    /// </summary>
    public void DragReleased() {
        Vector2Int pos = mapScript.Pos_To_TileXY(new Vector3(transform.position.x, heightOffset, transform.position.z));
        if (mapScript.GetTilemapInfo()[pos.y][pos.x] == -1)
        {
            initCharacter();
        }
    }


    #endregion


    #region 행동 정의 영역 ( action interface )

    /// <summary>
    /// idle행동을 액션큐에 추가.
    /// </summary>
    public void AddIdleAction()
    {
        actionQueue.Add(new ActionBlock(STATE.IDLE, ACCEPTABLE.ACCEPT, this));
    }

    //행동 인터페이스 정의.
    public void PlayIdleAction() {
        dirVec = Vector3.zero;
    }


    /// <summary>
    /// 움직이는 행동 액션큐에 추가
    /// </summary>
    /// <param name="endx">도착지 x좌표</param>
    /// <param name="endy">도착지 y좌표</param>
    public void AddMoveAction(int endx, int endy)
    {

        bool isFirstMove = true;

        //맵 정보를 받아옴 / 유효성 확인
        List<List<int>> maps = mapScript.GetTilemapInfo();
        if (maps == null)
        {
            //character debuger
            //Debug.Log("move action debuger : 잘못된 맵 정보");
            return;
        }

        //A*를 이용하여 경로 획득 / 유효성 확인, 목적지 좌표 설정.
        Vector2Int startPos = Vector2Int.zero;

        for (int i = actionQueue.Count - 1; i >= 0; i--) {
            //이전에 이동하는 경우가 존재하는 경우. 해당 행동의 마지막 목적지로부터 출발.
            if (actionQueue[i].getState() == STATE.ACTING) {
                startPos = mapScript.Pos_To_TileXY(((MoveActionBlock)actionQueue[i]).GetEndPos());
                isFirstMove = false;
                break;
            }
        }

        if(isFirstMove)//액션큐에 이동이 없다면 현재 위치로부터 출발.
        {
            startPos = mapScript.Pos_To_TileXY(transform.position);
        }

        #region 유효성 검사.

        if (startPos.x == int.MinValue && startPos.y == int.MinValue)
        {
            //character debuger
            //Debug.Log("move action debuger : 캐릭터가 맵 바깥에 있음.");
            return;
        }


        if (maps[startPos.y][startPos.x] == -1)
        {
            //character debuger
            //Debug.Log("move action debuger : 캐릭터가 벽에 있음.");
            return;
        }


        if (endx < 0 || endx >= maps[0].Count || endy < 0 || endy >= maps.Count)
        {
            //character debuger
            //Debug.Log("move action debuger : 도착지가 맵 바깥에 있음.");
            return;
        }

        if (maps[endy][endx] == -1)
        {
            //character debuger
            //Debug.Log("move action debuger : 도착지로 벽이 지정됨.");
            return;
        }

        #endregion

        //경로에 대한 정보 확인 및 행동 등록.
        List<Vector2Int> pathL = AStar.FindPath(maps, startPos, new Vector2Int(endx, endy));

        if (pathL == null)
        {
            //character debuger
            //Debug.Log("move action debuger : 목적지까지의 경로가 존재하지 않음.");
            return;
        }


        for (int i = pathL.Count - 1; i >= 0; i--)
        {

            Vector3 endPosV3 = new Vector3(
                pathL[i].x * unitMultiplizerX,
                heightOffset,
                -pathL[i].y * unitMultiplizerZ);

            actionQueue.Add(new MoveActionBlock(STATE.ACTING, ACCEPTABLE.DENY, this, endPosV3));


        }

        actionQueue.RemoveAt(actionQueue.Count - pathL.Count);//첫 본인 위치로는 이동x, 제자리 이동등의 부자연스러움 제거.

        //이동 완료 후 한번의 idle액션 추가. todo : 상황에 따라 조절하게 수정.
        AddIdleAction();


    }

    /// <summary>
    /// 액션큐를 지운 뒤 움직이는 행동을 액션큐에 추가 후 즉시 실행.
    /// </summary>
    /// <param name="endx">도착지 x좌표</param>
    /// <param name="endy">도착지 y좌표</param>
    public void AddMoveActionWithClearAciton(int endx, int endy) {

        ClearActionQueue();

        AddMoveAction(endx, endy);

        setTimeDelayOffset(0);

    }


    //행동 인터페이스 정의.
    public void PlayMoveAction(Vector3 _endPos) {

        destVec = _endPos;//모든 영역 기반 목적지 결정
        dirVec = Vector3.Normalize(destVec - transform.position);//단위벡터 변환
        lastDistance = float.MaxValue;

    }



    #endregion


    public STATE getState()
    {
        return curState;
    }

    /// <summary>
    /// 다음 행동 결정까지의 딜레이를 수동으로 지정
    /// </summary>
    /// <param name="_delay"></param>
    public void setTimeDelayOffset(float _delay)
    {
        timerTick = decideDelay - _delay;
    }

    public void changeAcceptable(ACCEPTABLE _acc)
    {
        //character debuger
        //Debug.Log("acceptable : " + _acc);
        accState = _acc;
    }

    /// <summary>
    /// 카메라 각도와 이동 방향에 따른 스프라이트 좌우 지정
    /// </summary>
    /// <param name="_isRight">시점 기준 우방향으로 이동중인지 여부</param>
    void changeDirectionSprite(bool _isRight)
    {
        //character debuger
        //Debug.Log(_isRight ? "direction : Right" : "direction : Left");

        if (_isRight)
        {
            spriteRenderer.flipX = true;
        }
        else {
            spriteRenderer.flipX = false;
        }
    }


    //todo : 임시변수이므로 알고리즘에 따라 수정 필요.[ 2칸 단위 ]
    int rangeWidthOffset;
    int widthOffsetCount;
    int rangeHeightOffset;
    int heightOffsetCount;
    int unitMultiplizerX;//바닥의 x크기
    int unitMultiplizerZ;//바닥의 y크기
    int width;
    int height;


    #region 맵 변화 인터페이스 영역

    //todo : 맵이 새로만들어질 때(맵이 없을 때) 비활성화 등등 해결법 고려
    public void OnExtended(Vector3 _centerPos, int _width, int _height, int _unitWidth, int _unitHeight)
    {
        Debug.Log("크기 변경됨. + 이동영역 변환.");

        width = _width;
        height = _height;


        Vector3 curPos = transform.position;

        if (curPos.x < 0 ||
           curPos.x > (width - 1) * unitMultiplizerX ||
           curPos.z < -((height - 1) * unitMultiplizerZ) ||
           curPos.z > 0)
        {
            
            //행동중지 및 가까운 영역으로 이동
            setTimeDelayOffset(3);
            ChangeState(STATE.IDLE);
            ClearActionQueue();

            curPos = new Vector3(Mathf.Clamp(curPos.x, 0, (width - 1) * unitMultiplizerX),
                              curPos.y,
                              Mathf.Clamp(curPos.z, -((height - 1) * unitMultiplizerZ), 0)
                            );
            //todo : 확장과 축소가 캐릭터가 이동하는 중에 이루어질 수 있는지 확인.
            //동시에 이루어질 수 있다면 위,아래 사항 고려 필요.
            //아닌경우 따로 고민.

            transform.position = curPos;

            Debug.Log("행동중지 및 강제 이동.");

            //todo : 강제이동 시 이동된 타일이 서있을 수 없을 경우 처리.
        }




    }

    public void MapCreate(Vector3 _centerPos)
    {
        initCharacter();
        gameObject.SetActive(true);
    }

    public void MapDestroy()
    {
        gameObject.SetActive(false);
    }

    #endregion

}
