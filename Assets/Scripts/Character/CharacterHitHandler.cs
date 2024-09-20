using System.Collections;
using UnityEngine;

enum MOUSE_STATE { MOUSE_DOWN, MOUSE_UP };
enum MouseButton { LEFT, MIDDLE, RIGHT, NONE };

public class CharacterHitHandler : MonoBehaviour {

    //터치상태에서 드래그상태로 전환되기 위한 최소 터치 유지시간
    private const float dragCnt = 0.1f;
    private Coroutine dragDelayCoroutine = null;
    private WaitForSeconds dragWaitTime;

    [SerializeField]
    private Character charComp;

    public void Awake() {

        dragWaitTime = new WaitForSeconds(dragCnt);

    }

    IEnumerator CountDragDelay() {

        yield return dragWaitTime;
        charComp.ChangeState(Character.STATE.DRAG);
        dragDelayCoroutine = null;

    }

    /// <summary>
    /// 현재 마우스 클릭 상태가 변할 때 호출되는 함수
    /// </summary>
    /// <param name="_state">변하는 상태</param>
    private void ChangeMouseState(MOUSE_STATE _state) {

        switch (_state) {

            case MOUSE_STATE.MOUSE_DOWN:

                if (dragDelayCoroutine == null)
                    dragDelayCoroutine = StartCoroutine(CountDragDelay());

                break;

            case MOUSE_STATE.MOUSE_UP:

                if (dragDelayCoroutine != null) {

                    //터치상태로 들어가는 부분
                    charComp.ChangeState(Character.STATE.TOUCHED);

                    StopCoroutine(dragDelayCoroutine);
                    dragDelayCoroutine = null;

                } else {

                    //드래그에서 빠져나가는 부분.
                    charComp.ChangeState(Character.STATE.IDLE);
                    charComp.setTimeDelayOffset(3);

                }

                break;

            default:
                break;

        }

    }



    /// <summary>
    /// 캐릭터 영역에서 마우스가 눌렸을 때 호출
    /// </summary>
    private void OnMouseDown() {

        charComp.OffsetInit();
        ChangeMouseState(MOUSE_STATE.MOUSE_DOWN);

    }

    /// <summary>
    /// 캐릭터 영역에서 마우스가 해제되었을 때 호출
    /// </summary>
    private void OnMouseUp() {

        ChangeMouseState(MOUSE_STATE.MOUSE_UP);

    }


}
