using System.Collections;
using System.Collections.Generic;
using UnityEngine;





public class SceneManager : MonoBehaviour
{

    public MapManager mapManager;

    public Character characterInstance;

    private Character curFocusedCharacter = null;

    
    public List<Character> characterList;

    private IRayCaster ray;

    public float zoomSpeed;

    private void Start()
    {

        characterList.Add(Instantiate(characterInstance));
        characterList.Add(Instantiate(characterInstance));

        ray = IRayCasterFactory.GetRayCaster();
    }

    private void Update()
    {

        if (Input.GetMouseButtonDown(0)
        #if !UNITY_EDITOR 
            && Input.touchCount == 1
        #endif
            ) {//마우스클릭 또는 터치가 일어난 순간 호출.

            GameObject hitChar = ray.GetCastHit(LayerMasks.GetLayerMask(LayerMaskName.CHARACTER));

            if (hitChar != null){//캐릭터가 눌렸을 경우.
            
                curFocusedCharacter = hitChar.transform.parent.GetComponent<Character>();

                if (curFocusedCharacter == null)
                {
                    Debug.Log("잘못된 캐릭터 스크립트 필터링");
                }

            }


            if (ray.GetCastHitPoint(LayerMasks.GetLayerMask(LayerMaskName.CHARACTER)).Equals(Vector3.negativeInfinity) && curFocusedCharacter != null) {//캐릭터가 눌리지 않았고, 특정 캐릭터가 포커스되어 있는 경우.

                Vector3 pos = ray.GetCastHitPoint(LayerMasks.GetLayerMask(LayerMaskName.FLAT));

                if (pos.x == float.NegativeInfinity) return;

                Vector2Int tilePos = mapManager.Pos_To_TileXY(pos);

                if (tilePos.x == int.MinValue) return;

                //액션큐를 비움과 동시에 이동 행위 실행.
                curFocusedCharacter.AddMoveActionWithClearAciton(tilePos.x, tilePos.y);

                //주석처리 할 경우 한번의 포커스로 여러번 이동시킴. => 포커스 해제방식을 정의할 필요 존재.
                curFocusedCharacter = null;

            }


        }


        //멀티터치에 대응하는 코드.
        if (Input.touchCount == 2)
        {

            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            //이전 프레임의 터치점을 가져옴.
            Vector2 prevTouch1 = touch1.position - touch1.deltaPosition;
            Vector2 prevTouch2 = touch2.position - touch2.deltaPosition;

            float prevMagnitude = (prevTouch1 - prevTouch2).magnitude;
            float currMagnitude = (touch1.position - touch2.position).magnitude;

            float diff = currMagnitude - prevMagnitude;

            Camera.main.orthographicSize -= diff * zoomSpeed * Time.deltaTime;

        }

        Camera.main.orthographicSize -= Input.GetAxis("Mouse ScrollWheel");

    }


}
