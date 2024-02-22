


public interface ICharacterAction {

    /// <summary>
    /// idle 행동블록 행동 정의.
    /// </summary>
    void PlayIdleAction();

    /// <summary>
    /// move 행동블록 행동 정의.
    /// </summary>
    /// <param name="_startPos">출발할 좌표</param>
    /// <param name="_endPos">도착지 좌표</param>
    void PlayMoveAction(UnityEngine.Vector3 _endPos);
}
