using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AStar
{

    private class Node {

        public int curCost; // G
        public int huristic; //H
        public int predictCost; // F
        public bool isWall;

        public Vector2Int parent;
        public Vector2Int myPos;

        public Node(Vector2Int _parent, bool _isWall) {
            myPos = _parent;
            parent = _parent;
            isWall = _isWall;
        }

        public Node(int _x, int _y, bool _isWall) : this(new Vector2Int(_x, _y), _isWall) {

        }

    }

    private static Dictionary<(int, int), Node> openQueue;
    private static Dictionary<(int, int), Node> closeQueue;

    private static List<List<Node>> NodeL;

    /// <summary>
    /// A* 알고리즘을 이용하여 최단경로 탐색.
    /// </summary>
    /// <param name="pathList">맵 정보를 담은 2차원 리스트</param>
    /// <param name="startX">출발지 x좌표</param>
    /// <param name="startY">출발지 y좌표</param>
    /// <param name="endX">도착지 x좌표</param>
    /// <param name="endY">도착지 y좌표</param>
    /// <returns>vector2Int 리스트로 경로 반환. [ 없을 경우 null 반환 ]</returns>
    public static List<Vector2Int> FindPath(List<List<int>> pathList, int startX, int startY, int endX, int endY) {

        return FindPath(pathList, new Vector2Int(startX, startY), new Vector2Int(endX, endY));

    }


    /// <summary>
    /// A* 알고리즘을 이용하여 최단경로 탐색.
    /// </summary>
    /// <param name="pathList">맵 정보를 담은 2차원 리스트</param>
    /// <param name="_startPos">출발지 좌표</param>
    /// <param name="_endPos">도착지 좌표</param>
    /// <returns>vector2Int 리스트로 경로 반환. [ 없을 경우 null 반환 ]</returns>
    /// <exception cref="System.Exception">잘못된 노드 접근</exception>
    public static List<Vector2Int> FindPath(List<List<int>> pathList, Vector2Int _startPos, Vector2Int _endPos) {

        if (pathList == null) {
            Debug.Log("맵 정보가 존재하지 않음");
            return null;
        }

        int width = pathList[0].Count;
        int height = pathList.Count;


        if (_startPos.x < 0 || _startPos.x > width - 1 || _startPos.y < 0 || _startPos.y > height - 1 ||
            _endPos.x < 0 || _endPos.x > width - 1 || _endPos.y < 0 || _endPos.y > height - 1) {

            Debug.Log("출발 / 도착지의 좌표가 범위를 벗어남");
            return null;

        }

        NodeL = new List<List<Node>>();

        //배열 상태는 NodeL[ y좌표 ][ x좌표 ] 형식
        for (int i = 0; i < pathList.Count; i++) {//y

            NodeL.Add(new List<Node>());

            for (int j = 0; j < pathList[i].Count; j++) {//x
                NodeL[i].Add(new Node(j, i, pathList[i][j] == 0 ? false : true));// 직관을 위해 좌표 저장은 x, y로 저장.
            }
        
        }


        openQueue = new Dictionary<(int, int), Node>();
        closeQueue = new Dictionary<(int, int), Node>();

        Node startNode = NodeL[_startPos.y][_startPos.x];

        startNode.curCost = 0;
        startNode.huristic = (Mathf.Abs(_startPos.x - _endPos.x) + Mathf.Abs(_startPos.y - _endPos.y)) * 10;
        startNode.predictCost = startNode.curCost + startNode.huristic;

        openQueue.Add((_startPos.x, _startPos.y), startNode);


        while (openQueue.Count > 0) {

            int fValue = int.MaxValue;
            int hValue = int.MaxValue;
            Node nextNode = null;

            foreach (Node node in openQueue.Values) {//최소 예상비용 노드 선택

                if (node.predictCost < fValue)
                {
                    fValue = node.predictCost;
                    hValue = node.huristic;
                    nextNode = node;
                }
                else if (node.predictCost == fValue)
                {
                    if (node.huristic < hValue)
                    {
                        hValue = node.huristic;
                        nextNode = node;
                    }
                }

            }
            
            if (nextNode.myPos.x == _endPos.x && nextNode.myPos.y == _endPos.y) {//도착여부 판단. => 반환.

                Node printNode = nextNode;
                List<Vector2Int> retL = new List<Vector2Int>();

                while (!(printNode.myPos.x == printNode.parent.x && printNode.myPos.y == printNode.parent.y)) {

                    retL.Add(new Vector2Int(printNode.myPos.x, printNode.myPos.y));
                    printNode = NodeL[printNode.parent.y][printNode.parent.x];
                }

                retL.Add(new Vector2Int(printNode.myPos.x, printNode.myPos.y));
                return retL;
            }


            if (nextNode == null) {
                throw new System.Exception("잘못된 노드 접근법");
            }

            int curX = nextNode.myPos.x;
            int curY = nextNode.myPos.y;

            openQueue.Remove((curX, curY));
            closeQueue.Add((curX, curY), nextNode);

            //대각선 우선 탐색.
            checkPath(nextNode, curX + 1, curY - 1, _endPos.x, _endPos.y, width, height, true);
            checkPath(nextNode, curX + 1, curY + 1, _endPos.x, _endPos.y, width, height, true);
            checkPath(nextNode, curX - 1, curY + 1, _endPos.x, _endPos.y, width, height, true);
            checkPath(nextNode, curX - 1, curY - 1, _endPos.x, _endPos.y, width, height, true);

            //전후좌우 탐색
            checkPath(nextNode, curX + 1, curY, _endPos.x, _endPos.y, width, height);
            checkPath(nextNode, curX, curY + 1, _endPos.x, _endPos.y, width, height);
            checkPath(nextNode, curX - 1, curY, _endPos.x, _endPos.y, width, height);
            checkPath(nextNode, curX, curY - 1, _endPos.x, _endPos.y, width, height);



        }


        //목적지 도달 불가.

        return null;
    
    }

    /// <summary>
    /// 목적지 노드에 대한 유효성 확인 및 openL 갱신.
    /// </summary>
    /// <param name="_fromNode">출발지 노드</param>
    /// <param name="_checkX">확인할 노드 x좌표</param>
    /// <param name="_checkY">확인할 노드 y좌표</param>
    /// <param name="_endX">최종 목적지의 x좌표</param>
    /// <param name="_endY">최종 목적지의 y좌표</param>
    /// <param name="_mapWidth">전체맵의 너비</param>
    /// <param name="_mapHeight">전체맵의 높이</param>
    /// <param name="_isCross">대각선 이동 여부</param>
    private static void checkPath(Node _fromNode, int _checkX, int _checkY,int _endX, int _endY, int _mapWidth, int _mapHeight, bool _isCross = false) {

        if (_checkX < 0 || _checkX > _mapWidth - 1 || _checkY < 0 || _checkY > _mapHeight - 1)//유효 범위 확인
        {
            return;
        }

        if (closeQueue.ContainsKey((_checkX, _checkY))) {//closed 확인
            return;
        }

        int fromX = _fromNode.myPos.x;
        int fromY = _fromNode.myPos.y;

        if (_isCross)//벽 통과 유효성 확인.
        {

            if (NodeL[_checkY][fromX].isWall || NodeL[fromY][_checkX].isWall || NodeL[_checkY][_checkX].isWall)
            {
                return;
            }

        }
        else {

            if (NodeL[_checkY][_checkX].isWall) {
                return;
            }

        }


        int moveCost = _isCross ? 14 : 10;

        if (openQueue.ContainsKey((_checkX, _checkY)))//openL의 재방문인 경우
        {
            if (openQueue[(_checkX, _checkY)].curCost > _fromNode.curCost + moveCost) {//기존 open을 갱신할지 여부
                openQueue[(_checkX, _checkY)].parent = new Vector2Int(fromX, fromY);
                openQueue[(_checkX, _checkY)].curCost = _fromNode.curCost + moveCost;
                openQueue[(_checkX, _checkY)].predictCost = openQueue[(_checkX, _checkY)].curCost + openQueue[(_checkX, _checkY)].huristic;
            }

        }
        else {//첫 방문인 경우

            openQueue.Add((_checkX, _checkY), NodeL[_checkY][_checkX]);

            openQueue[(_checkX, _checkY)].huristic = (Mathf.Abs(_endX - _checkX) + Mathf.Abs(_endY - _checkY)) * 10;
            openQueue[(_checkX, _checkY)].parent = new Vector2Int(fromX, fromY);
            openQueue[(_checkX, _checkY)].curCost = _fromNode.curCost + moveCost;
            openQueue[(_checkX, _checkY)].predictCost = openQueue[(_checkX, _checkY)].curCost + openQueue[(_checkX, _checkY)].huristic;
        }



    }

}
