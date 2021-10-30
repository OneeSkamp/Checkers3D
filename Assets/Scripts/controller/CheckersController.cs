using System.Data;
using UnityEngine;
using System.Collections.Generic;
using option;

namespace controller {
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

    public struct MoveCell {
        public Vector2Int point;
        public bool isAttack;
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
        private ChColor moveClr;
        private Option<Vector2Int> selected;

        private List<Vector2Int> attackCells = new List<Vector2Int>();
        private List<Vector2Int> dirs = new List<Vector2Int>();
        private Dictionary<Vector2Int, List<MoveCell>> possibleMoves = new Dictionary<Vector2Int, List<MoveCell>>();

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

            dirs.Add(new Vector2Int(1, 1));
            dirs.Add(new Vector2Int(1, -1));
            dirs.Add(new Vector2Int(-1, 1));
            dirs.Add(new Vector2Int(-1, -1));

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

            if (map.board[cell.x, cell.y].IsSome()) {
                selected = Option<Vector2Int>.Some(cell);
            }

            if (possibleMoves.Count == 0) {
                for (int i = 0; i < map.board.GetLength(0); i++) {
                    for (int j = 0; j < map.board.GetLength(1); j++) {
                        var chOpt = map.board[i, j];
                        if (chOpt.IsNone()) continue;

                        var moveCell = new MoveCell();
                        var moveCells = new List<MoveCell>();
                        foreach (var dir in dirs) {
                            var nextPos = new Vector2Int(i, j) + dir;
                            if (!IsOnBoard(nextPos, map.board)) continue;
                            var nextOpt = map.board[nextPos.x, nextPos.y];
                            if (nextOpt.IsNone()) {
                                moveCell.isAttack = false;
                                moveCell.point = nextPos;
                                moveCells.Add(moveCell);
                            } else {
                                var next = nextOpt.Peel();
                                if (next.color == chOpt.Peel().color) continue;
                                var afterNextPos = nextPos + dir;
                                if (!IsOnBoard(afterNextPos, map.board)) continue;

                                var afterNextOpt = map.board[afterNextPos.x, afterNextPos.y];
                                if (afterNextOpt.IsNone()) {
                                    moveCell.isAttack = true;
                                    moveCell.point = afterNextPos;
                                    moveCells.Add(moveCell);
                                }
                            }
                        }

                        possibleMoves.Add(new Vector2Int(i, j), moveCells);
                    }
                }
            }

            if (selected.IsSome()) {

                var moveCells = possibleMoves[selected.Peel()];

                // if (map.board[cell.x, cell.y].IsSome()) return;

                foreach (var moveCell in moveCells) {
                    Debug.Log(moveCell.point);
                    if (moveCell.point == cell) {
                        if (moveCell.isAttack) {
                            //selected = Option<Vector2Int>.Some(cell);
                            //possibleMoves.Clear();
                        }

                        if (!moveCell.isAttack) {

                        }
                    }
                }
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

        public bool IsOnBoard<T>(Vector2Int pos, Option<T>[,] board) {
            var size = new Vector2Int(board.GetLength(0), board.GetLength(1));
            if (pos.x < 0 || pos.y < 0 || pos.x >= size.x || pos.y >= size.y) {
                return false;
            }

            return true;
        }
    }
}