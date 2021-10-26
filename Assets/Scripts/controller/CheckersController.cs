using UnityEngine;
using System.Collections.Generic;
using option;

namespace controller {
    public enum CheckerErr{
        None,
        BoardIsNull,
        PosOutsideBoard,
        PossiblePointsErr,
        CheckersMovementsErr
    }

    public enum ChColor {
        White,
        Black
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

    public struct CheckerLoc {
        public Vector2Int pos;
        public Option<Checker>[,] board;
    }

    public enum PlayerAction {
        None,
        Select,
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
        private ChColor moveColor;
        private Vector2Int selected;
        private Vector3 leftTopLocal;
        private Vector3 offset;
        private bool onlyAttack;

        private List<Vector2Int> moveCells = new List<Vector2Int>();
        private List<Vector2Int> attackCells = new List<Vector2Int>();
        private List<Vector2Int> possibleCells = new List<Vector2Int>();
        private List<Vector2Int> dirs = new List<Vector2Int>();
        private CheckerLoc checkerLoc = new CheckerLoc();

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

            leftTopLocal = resources.leftTop.localPosition;
            offset = resources.offset.localPosition;

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
            if (checkerOpt.IsSome() && checkerOpt.Peel().color == moveColor) {
                playerAction = PlayerAction.Select;
            }

            foreach (Transform item in resources.moveHighlights) {
                Destroy(item.gameObject);
            }

            switch (playerAction) {
                case PlayerAction.Select:
                    selected = cell;
                    checkerLoc = new CheckerLoc {
                        pos = selected,
                        board = map.board
                    };
                    moveCells.Clear();
                    attackCells.Clear();
                    var checker = checkerOpt.Peel();

                    int maxCount = Mathf.Max(map.board.GetLength(0), map.board.GetLength(1));
                    var skipDir = 1;
                    if (checker.type == ChType.Basic) {
                        maxCount = 1;
                        if (checker.color == ChColor.White) {
                            skipDir = -1;
                        }
                    }

                    foreach (var dir in dirs) {
                        for (int i = 1; i <= maxCount; i++) {
                            var nextPos = checkerLoc.pos + dir * i;
                            if (!IsOnBoard(nextPos, checkerLoc.board)) break;
                            var nextOpt = checkerLoc.board[nextPos.x, nextPos.y];
                            if (nextOpt.IsNone()) {
                                if (checker.type == ChType.Basic && dir.x != skipDir) break;
                                moveCells.Add(nextPos);
                            }

                            if (nextOpt.IsSome()) {
                                var next = nextOpt.Peel();
                                if (next.color == checker.color) break;
                                var afterOnePos = nextPos + dir;
                                if (!IsOnBoard(afterOnePos, checkerLoc.board)) break;
                                var afterOneOpt = checkerLoc.board[afterOnePos.x, afterOnePos.y];
                                if (afterOneOpt.IsSome()) break;
                                for (var j = 1; j <= maxCount; j++) {
                                    var afterNext = nextPos + dir * j;
                                    if (!IsOnBoard(afterNext, checkerLoc.board)) break;
                                    nextOpt = checkerLoc.board[afterNext.x, afterNext.y];
                                    if (nextOpt.IsSome()) break;
                                    attackCells.Add(afterNext);
                                }
                            }
                        }
                    }

                    onlyAttack = false;
                    for (int i = 0; i <= map.board.GetLength(0) - 1; i++) {
                        for (int j = 0; j <= map.board.GetLength(1) - 1; j++) {
                            var chOpt = map.board[i, j];
                            if (chOpt.IsNone()) continue;
                            var ch = chOpt.Peel();
                            var count = Mathf.Max(map.board.GetLength(0), map.board.GetLength(1));
                            if (ch.type == ChType.Basic) {
                                count = 1;
                            }
                            if (ch.color != moveColor) continue;
                            foreach (var dir in dirs) {
                                for (int k = 1; k <= count; k++) {
                                    var nextPos = new Vector2Int(i, j) + dir * k;
                                    if (!IsOnBoard(nextPos, map.board)) break;
                                    var nextOpt = map.board[nextPos.x, nextPos.y];
                                    if (nextOpt.IsSome()) {
                                        if (nextOpt.Peel().color == moveColor) break;
                                        var afterOnePos = nextPos + dir;
                                        if (!IsOnBoard(afterOnePos, map.board)) break;
                                        var afterOneOpt = map.board[afterOnePos.x, afterOnePos.y];
                                        if (afterOneOpt.IsNone()) {
                                            onlyAttack = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (onlyAttack) {
                        CreateMoveHighlights(attackCells);
                    } else {
                        CreateMoveHighlights(moveCells);
                    }
                    playerAction = PlayerAction.Move;
                    break;

                case PlayerAction.Move:
                    possibleCells = moveCells;
                    if (onlyAttack) {
                        possibleCells = attackCells;
                    }
                    foreach (var emptyCell in possibleCells) {
                        if (cell == emptyCell) {
                            Relocate(checkerLoc, emptyCell);
                            if (cell.x == 0 || cell.x == map.board.GetLength(0) - 1) {
                                PromoteChecker(cell);
                            }

                            if (moveColor == ChColor.White) {
                                moveColor = ChColor.Black;
                            } else {
                                moveColor = ChColor.White;
                            }
                        }
                    }

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
                    var newPos = new Vector3(-j, 0.5f, i) * 2 + leftTopLocal - offset;
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
            var result = new List<GameObject>();
            if (possMoves == null) return;
            foreach (var possMove in possMoves) {
                var x = -possMove.y;
                var z = possMove.x;
                var newPos = new Vector3(x, 0.5f, z) * 2 + leftTopLocal - offset;
                var obj = Instantiate(resources.moveHighlight);
                obj.transform.parent = resources.moveHighlights;
                obj.transform.localPosition = newPos;
            }
        }

        public void CheckerMove(CheckerLoc chLoc, Vector2Int to) {
            var pos = chLoc.pos;
            var dif = to - pos;
            var dir = new Vector2Int(dif.x/Mathf.Abs(dif.x), dif.y/Mathf.Abs(dif.y));
            var moveLength = dif.magnitude / Mathf.Sqrt(2);
            for (int i = 1; i < moveLength; i++) {
                var nextPos = pos + dir * i;
                var nextOpt = chLoc.board[nextPos.x, nextPos.y];
                if (nextOpt.IsSome()) {
                    map.board[nextPos.x, nextPos.y] = Option<Checker>.None();
                }
            }
            map.board[to.x, to.y] = map.board[pos.x, pos.y];
            map.board[pos.x, pos.y] = Option<Checker>.None();
        }

        public bool IsOnBoard<T>(Vector2Int pos, Option<T>[,] board) {
            var size = new Vector2Int(board.GetLength(0), board.GetLength(1));
            if (pos.x < 0 || pos.y < 0 || pos.x >= size.x || pos.y >= size.y) {
                return false;
            }

            return true;
        }

        private void PromoteChecker(Vector2Int promotePos) {
            var obj = resources.blackLady;
            if (moveColor == ChColor.White) {
                obj = resources.whiteLady;
            }
            var lady = Option<Checker>.Some(Checker.Mk(moveColor, ChType.Lady));
            map.board[promotePos.x, promotePos.y] = lady;
            var pos = map.figures[promotePos.x, promotePos.y].transform.localPosition;
            Destroy(map.figures[promotePos.x, promotePos.y]);
            var ladyObj = Instantiate(obj);
            ladyObj.transform.parent = resources.boardTransform;
            ladyObj.transform.localPosition = pos;
            map.figures[promotePos.x, promotePos.y] = ladyObj;
        }

        private void Relocate(CheckerLoc chLoc, Vector2Int to) {
            CheckerMove(chLoc, to);
            var pos = chLoc.pos;
            var dif = to - pos;
            var dir = new Vector2Int(dif.x/Mathf.Abs(dif.x), dif.y/Mathf.Abs(dif.y));
            var moveLength = dif.magnitude / Mathf.Sqrt(2);;
            for (int i = 1; i < moveLength; i++) {
                var nextPos = pos + dir * i;
                var next = map.figures[nextPos.x, nextPos.y];
                if (next != null) {
                    Destroy(map.figures[nextPos.x, nextPos.y]);
                }
            }
            var x = -to.y;
            var z = to.x;
            var newPos = new Vector3(x, 0.5f, z) * 2 + leftTopLocal - offset;
            map.figures[chLoc.pos.x, chLoc.pos.y].transform.localPosition = newPos;
            map.figures[to.x, to.y] = map.figures[chLoc.pos.x, chLoc.pos.y];
        }
    }

}