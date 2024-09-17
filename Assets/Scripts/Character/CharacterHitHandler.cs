using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum MOUSE_STATE { MOUSE_DOWN, MOUSE_UP };
enum MouseButton { LEFT, MIDDLE, RIGHT, NONE };

public class CharacterHitHandler : MonoBehaviour
{

    private MOUSE_STATE mouseState;

    public GameObject characterObj;
    private Character charComp;

    public void Awake()
    {
        mouseState = MOUSE_STATE.MOUSE_UP;
        charComp = characterObj.GetComponent<Character>();
    }


    //터치상태에서 드래그상태로 전환되기 위한 최소 터치 유지시간
    private const float dragCnt = 0.1f;
    private float delatT = 0;

    /// <summary>
    /// 현재 마우스 클릭 상태가 변할 때 호출되는 함수
    /// </summary>
    /// <param name="_state">변하는 상태</param>
    private void ChangeMouseState(MOUSE_STATE _state) {

        switch (_state) {

            case MOUSE_STATE.MOUSE_DOWN:
                charComp.ChangeState(Character.STATE.TOUCHED);
                charComp.changeAcceptable(Character.ACCEPTABLE.DENY);
                delatT = 0;
                break;

            case MOUSE_STATE.MOUSE_UP:

                if (charComp.getState() == Character.STATE.TOUCHED)
                {
                    charComp.TouchedAction();
                }
                else if (charComp.getState() == Character.STATE.DRAG)
                {
                    charComp.ChangeState(Character.STATE.IDLE);
                    charComp.setTimeDelayOffset(3);
                    charComp.DragReleased();

                }
                else {
                    //todo : 예외상황 분석
                    Debug.Log("mouse release occured!");
                }

                charComp.changeAcceptable(Character.ACCEPTABLE.ACCEPT);

                break;

            default:
                break;

        }

        mouseState = _state;
    }

    /// <summary>
    /// 캐릭터 영역에서 마우스가 눌렸을 때 호출
    /// </summary>
    private void OnMouseDown()
    {
        ChangeMouseState(MOUSE_STATE.MOUSE_DOWN);
    }

    /// <summary>
    /// 캐릭터 영역에서 마우스가 해제되었을 때 호출
    /// </summary>
    private void OnMouseUp()
    {
        ChangeMouseState(MOUSE_STATE.MOUSE_UP);
    }




    //todo : 둘 사이의 전환시간이 길 경우 어색하지만 터치는 짧은 순간에 이뤄지므로 짧을수록 어색함이 없어져서 자연스러움.
    /// <summary>
    /// 단위 시간 이상으로 눌렸을 경우 상태를 touch에서 drag로 변환.
    /// </summary>
    void Update()
    {
        if (mouseState == MOUSE_STATE.MOUSE_DOWN) {
            delatT += Time.deltaTime;
            if (delatT > dragCnt) {
                charComp.ChangeState(Character.STATE.DRAG);
            }
        }
    }

}
