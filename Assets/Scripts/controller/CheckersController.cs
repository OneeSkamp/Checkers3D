using UnityEngine;
using System.Collections.Generic;
using option;

namespace controller {
    public enum CheckerErr{
        None,
        BoardIsNull,
        PosOutsideBoard,
        PossiblePointsErr,
        CheckersMovementsErr,
        NoCheckerOnPosition
    }

    public enum ChColor {
        White,
        Black,
        Count
    }

    public enum ChType {
        Basic,
        Lady
    }

    public struct Checker {
        public ChColor color;
        public ChType type;

        public static Checker Mk (ChColor color, ChType type) {
            return new Checker { color = color, type = type };
        }
    }

    public struct ChInfo {
        public bool attack;
        public List<Vector2Int> points;
    }

    public enum PlayerAction {
        None,
        SelectChecker,
        Move
    }

    public struct Map {
        public GameObject[,] figures;
        public Option<Checker>[,] board;
    }

    public class CheckersController : MonoBehaviour {
        public Resources resources;

        private Map map;
        private PlayerAction playerAction;
        private ChColor moveClr;
        private Vector2Int selected;
        private bool onlyAttack;
        private bool nextAttack;

        private List<Vector2Int> dirs = new List<Vector2Int>();
        private Dictionary<Vector2Int, ChInfo> chInfos = new Dictionary<Vector2Int, ChInfo>();

        private void Awake() {
            if (resources == null) {
                Debug.LogError("Resources isn't provided");
                this.enabled = false;
                return;
            }

            if (resources.boardTransform == null) {
                Debug.LogError("Board transform isn't provided");
                this.enabled = false;
                return;
            }

            if (resources.leftTop == null) {
                Debug.LogError("Left top isn't provided");
                this.enabled = false;
                return;
            }

            if (resources.moveHighlights == null) {
                Debug.LogError("Move highlights isn't provided");
                this.enabled = false;
                return;
            }

            if (resources.whiteChecker == null) {
                Debug.LogError("White checker isn't provided");
                this.enabled = false;
                return;
            }

            if (resources.blackChecker == null) {
                Debug.LogError("Black checker isn't provided");
                this.enabled = false;
                return;
            }

            if (resources.whiteLady == null) {
                Debug.LogError("White lady isn't provided");
                this.enabled = false;
                return;
            }

            if (resources.blackLady == null) {
                Debug.LogError("Black lady isn't provided");
                this.enabled = false;
                return;
            }

            if (resources.moveHighlight == null) {
                Debug.LogError("Move highlight isn't provided");
                this.enabled = false;
                return;
            }

            dirs.Add(new Vector2Int(-1, 1));
            dirs.Add(new Vector2Int(-1, -1));
            dirs.Add(new Vector2Int(1, 1));
            dirs.Add(new Vector2Int(1, -1));

            map.figures = new GameObject[8, 8];
            map.board = new Option<Checker>[8, 8];
            FillBoard(map.board);
            FillCheckers(map.board);
        }

        private void Update() {
            if (!Input.GetMouseButtonDown(0)) {
                return;
            }

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out RaycastHit hit)) {
                return;
            }

            var cell = ToCell(hit.point, resources.leftTop.position);

            var checkerOpt = map.board[cell.x, cell.y];
            if (checkerOpt.IsSome() && checkerOpt.Peel().color == moveClr) {
                playerAction = PlayerAction.SelectChecker;
            }

            foreach (Transform item in resources.moveHighlights) {
                Destroy(item.gameObject);
            }

            if (chInfos.Count == 0) {
                onlyAttack = false;
                for (int i = 0; i < map.board.GetLength(0); i++) {
                    for (int j = 0; j < map.board.GetLength(1); j++) {
                        var chOpt = map.board[i, j];
                        if (chOpt.IsNone()) continue;

                        var ch = chOpt.Peel();
                        if (ch.color != moveClr) continue;

                        var skipXDir = 1;
                        if (ch.color == ChColor.White) {
                            skipXDir = -1;
                        }

                        var chInfo = new ChInfo();
                        var moveCells = new List<Vector2Int>();
                        var attackCells = new List<Vector2Int>();
                        foreach (var dir in dirs) {
                            var k = 0;
                            var condition = true;
                            while (condition) {
                                k++;

                                var nextPos = new Vector2Int(i, j) + dir * k;
                                if (!IsOnBoard(nextPos, map.board)) break;

                                var nextOpt = map.board[nextPos.x, nextPos.y];
                                if (nextOpt.IsNone()) {
                                    if (ch.type == ChType.Basic && dir.x != skipXDir) {
                                        break;
                                    }
                                    moveCells.Add(nextPos);
                                } else {
                                    var next = nextOpt.Peel();
                                    if (next.color == ch.color) break;

                                    var aftPos = nextPos + dir;
                                    if (!IsOnBoard(aftPos, map.board)) break;

                                    var aftOpt = map.board[aftPos.x, aftPos.y];
                                    if (aftOpt.IsSome()) break;

                                    var t = 0;
                                    while (condition) {
                                        t++;

                                        var aftNext = nextPos + dir * t;
                                        if (!IsOnBoard(aftNext, map.board)) {
                                            condition = false;
                                            break;
                                        }

                                        var aftNextOpt = map.board[aftNext.x, aftNext.y];
                                        if (aftNextOpt.IsSome()) {
                                            condition = false;
                                            break;
                                        }

                                        attackCells.Add(aftNext);
                                        if (ch.type == ChType.Basic) {
                                            condition = false;
                                        }
                                    }
                                }

                                if (ch.type == ChType.Basic) break;
                            }
                        }

                        chInfo.points = moveCells;
                        if (attackCells.Count > 0) {
                            onlyAttack = true;
                            chInfo.attack = true;
                            chInfo.points = attackCells;
                        }

                        chInfos.Add(new Vector2Int(i, j), chInfo);
                    }
                }
            }

            switch (playerAction) {
                case PlayerAction.SelectChecker:
                    selected = cell;
                    if (onlyAttack && !chInfos[selected].attack) {
                        playerAction = PlayerAction.None;
                        return;
                    }

                    CreateMoveHighlights(chInfos[cell].points);
                    playerAction = PlayerAction.Move;
                    break;

                case PlayerAction.Move:
                    var moves = chInfos[selected].points;
                    foreach (var emptyCell in moves) {
                        if (cell == emptyCell) {
                            nextAttack = false;
                            var dif = cell - selected;
                            var dir = new Vector2Int(
                                dif.x/Mathf.Abs(dif.x),
                                dif.y/Mathf.Abs(dif.y)
                            );
                            var moveLength = dif.magnitude / Mathf.Sqrt(2);

                            map.board[cell.x, cell.y] = map.board[selected.x, selected.y];
                            map.board[selected.x, selected.y] = Option<Checker>.None();
                            var checker = checkerOpt.Peel();
                            for (int i = 1; i < moveLength; i++) {
                                var nextPos = selected + dir * i;

                                var next = map.figures[nextPos.x, nextPos.y];
                                if (next != null) {
                                    Destroy(map.figures[nextPos.x, nextPos.y]);

                                }

                                var nextOpt = map.board[nextPos.x, nextPos.y];
                                if (nextOpt.IsSome()) {
                                    map.board[nextPos.x, nextPos.y] = Option<Checker>.None();
                                }

                                foreach (var direction in dirs) {
                                    var j = 0;
                                    while (true) {
                                        j++;

                                        var newNextPos = cell + direction * i;
                                        if (!IsOnBoard(newNextPos, map.board)) break;

                                        var newNextOpt = map.board[newNextPos.x, newNextPos.y];
                                        if (newNextOpt.IsSome()) {
                                            var newNext = newNextOpt.Peel();
                                            if (newNext.color == moveClr) break;

                                            var afterPos = newNextPos + direction;
                                            if (!IsOnBoard(afterPos, map.board)) break;

                                            var afterOpt = map.board[afterPos.x, afterPos.y];
                                            if (afterOpt.IsNone()) {
                                                nextAttack = true;
                                                break;
                                            }
                                        }

                                        if (checker.type == ChType.Basic) break;
                                    }
                                }
                            }

                            var x = -cell.y;
                            var z = cell.x;
                            var offset = resources.offset.localPosition;
                            var leftTop = resources.leftTop.localPosition;
                            var newPos = new Vector3(x, 0.5f, z) * 2 + leftTop - offset;
                            map.figures[selected.x, selected.y].transform.localPosition = newPos;
                            map.figures[cell.x, cell.y] = map.figures[selected.x, selected.y];

                            if (cell.x == 0 || cell.x == map.board.GetLength(0) - 1) {
                                var obj = resources.blackLady;
                                if (moveClr == ChColor.White) {
                                    obj = resources.whiteLady;
                                }

                                var lady = Option<Checker>.Some(Checker.Mk(moveClr, ChType.Lady));
                                map.board[cell.x, cell.y] = lady;
                                Destroy(map.figures[cell.x, cell.y]);

                                var ladyObj = Instantiate(obj);
                                ladyObj.transform.parent = resources.boardTransform;

                                var pos = map.figures[cell.x, cell.y].transform.localPosition;
                                ladyObj.transform.localPosition = pos;
                                map.figures[cell.x, cell.y] = ladyObj;
                            }

                            if (!nextAttack) {
                                if (moveClr == ChColor.White) {
                                    moveClr = ChColor.Black;
                                } else {
                                    moveClr = ChColor.White;
                                }
                            }
                        }
                    }

                    chInfos.Clear();
                    playerAction = PlayerAction.None;
                    break;
            }
        }

        private void FillBoard(Option<Checker>[,] board) {
            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; j < board.GetLength(1); j++) {
                    if (i > 2 && i < 5) continue;

                    if (i % 2 == 0 && j % 2 != 0 || i % 2 != 0 && j % 2 == 0) {
                        var color = ChColor.Black;
                        if (i >= 5) {
                            color = ChColor.White;
                        }
                        map.board[i, j] = Option<Checker>.Some(Checker.Mk(color, ChType.Basic));
                    }
                }
            }
        }

        private void FillCheckers(Option<Checker>[,] board) {
            var leftTop = resources.leftTop.localPosition;
            var offset = resources.offset.localPosition;
            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; j < board.GetLength(1); j++) {
                    if (board[i, j].IsNone()) {
                        continue;
                    }
                    var checker = board[i, j].Peel();

                    var prefab = resources.blackChecker;
                    if (checker.color == ChColor.White) {
                        prefab = resources.whiteChecker;
                    }

                    var checkerObj = Instantiate(prefab);
                    checkerObj.transform.parent = resources.boardTransform;
                    var newPos = new Vector3(-j, 0.5f, i) * 2 + leftTop - offset;
                    checkerObj.transform.localPosition = newPos;

                    map.figures[i, j] = checkerObj;
                }
            }
        }

        public Vector2Int ToCell(Vector3 globalPoint, Vector3 leftTopPos) {
            var point = resources.boardTransform.InverseTransformPoint(globalPoint);
            var intermediate = (point - new Vector3(-leftTopPos.x, 0f, leftTopPos.z)) / 2;
            return new Vector2Int(Mathf.Abs((int)intermediate.z), Mathf.Abs((int)intermediate.x));
        }

        public void CreateMoveHighlights(List<Vector2Int> possMoves) {
            if (possMoves == null) return;
            var leftTop = resources.leftTop.localPosition;
            var result = new List<GameObject>();
            var offset = resources.offset.localPosition;

            foreach (var possMove in possMoves) {
                var x = -possMove.y;
                var z = possMove.x;
                var newPos = new Vector3(x, 0.5f, z) * 2 + leftTop - offset;
                var obj = Instantiate(resources.moveHighlight);
                obj.transform.parent = resources.moveHighlights;
                obj.transform.localPosition = newPos;
            }
        }

        public bool IsOnBoard<T>(Vector2Int pos, Option<T>[,] board) {
            var size = new Vector2Int(board.GetLength(0), board.GetLength(1));
            if (pos.x < 0 || pos.y < 0 || pos.x >= size.x || pos.y >= size.y) {
                return false;
            }

            return true;
        }
    }
}