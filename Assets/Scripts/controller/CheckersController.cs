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

    public struct Map {
        public GameObject[,] figures;
        public Option<Checker>[,] board;
    }

    public class CheckersController : MonoBehaviour {
        public Resources resources;

        private Map map;
        private ChColor moveClr;
        private bool needAttack;
        private Option<Vector2Int> selected;

        private List<Vector2Int> dirs = new List<Vector2Int>();
        private Dictionary<Vector2Int, List<MoveCell>> possibleMoves;
        private Dictionary<Vector2Int, Vector2Int> attackSegments =
            new Dictionary<Vector2Int, Vector2Int>();

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

            foreach (Transform item in resources.moveHighlights) {
                Destroy(item.gameObject);
            }

            if (possibleMoves == null) {
                possibleMoves = new Dictionary<Vector2Int, List<MoveCell>>();
                for (int i = 0; i < map.board.GetLength(0); i++) {
                    for (int j = 0; j < map.board.GetLength(1); j++) {
                        var chOpt = map.board[i, j];
                        if (chOpt.IsNone() || chOpt.Peel().color != moveClr) continue;
                        var ch = chOpt.Peel();
                        var moveCell = new MoveCell();

                        var moveCells = new List<MoveCell>();
                        foreach (var dir in dirs) {
                            var nextPos = new Vector2Int(i, j) + dir;
                            if (!IsOnBoard(nextPos, map.board)) continue;
                            var nextOpt = map.board[nextPos.x, nextPos.y];
                            if (nextOpt.IsNone()) {
                                if (ch.color == ChColor.White && dir.x == 1) continue;
                                if (ch.color == ChColor.Black && dir.x == -1) continue;

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

                                    var repeat = false;
                                    foreach (var attack in attackSegments) {
                                        if (afterNextPos == attack.Key
                                            || afterNextPos == attack.Value) {

                                            repeat = true;
                                            break;
                                        }
                                    }

                                    if (repeat == true) continue;
 
                                    needAttack = true;
                                    moveCell.isAttack = true;
                                    moveCell.point = afterNextPos;
                                    moveCells.Add(moveCell);
                                }
                            }
                        }

                        possibleMoves.Add(new Vector2Int(i, j), moveCells);
                    }
                }

                if (selected.IsSome()) {
                    var moves = possibleMoves[selected.Peel()];
                    var a = false;
                    foreach (var move in moves) {
                        if (move.isAttack) {
                            a = true;
                        }
                    }

                    if (!a) {
                        needAttack = false;
                        selected = Option<Vector2Int>.None();
                        possibleMoves = null;

                        if (moveClr == ChColor.White) {
                            moveClr = ChColor.Black;
                        } else {
                            moveClr = ChColor.White;
                        }
                        return;

                    }

                }
            }

            if (map.board[cell.x, cell.y].IsSome()) {

                var checkerOpt = map.board[cell.x, cell.y];
                if (checkerOpt.IsSome()) {
                    var checker = checkerOpt.Peel();
                    if (checker.color != moveClr) return;

                    selected = Option<Vector2Int>.Some(cell);
                    var moveCells = possibleMoves[selected.Peel()];
                    foreach (var moveCell in moveCells) {
                        if (needAttack && !moveCell.isAttack) continue;
                        var highlight = Instantiate(resources.moveHighlight);
                        highlight.transform.parent = resources.moveHighlights;
                        highlight.transform.localPosition = ToCenterCell(moveCell.point);
                    }
                }
            } else if(selected.IsSome()) {
                var slct = selected.Peel();
                if (map.board[cell.x, cell.y].IsNone()) {
                    var moveCells = possibleMoves[slct];

                    foreach (var moveCell in moveCells) {
                        if (needAttack && !moveCell.isAttack) continue;

                        if (moveCell.point == cell) {
                            map.board[cell.x, cell.y] = map.board[slct.x, slct.y];
                            map.board[slct.x, slct.y] = Option<Checker>.None();

                            var prefab = map.figures[slct.x, slct.y];
                            map.figures[cell.x, cell.y] = prefab;

                            prefab.transform.localPosition = ToCenterCell(cell);

                            if (moveCell.isAttack) {
                                attackSegments.Add(slct, moveCell.point);
                                selected = Option<Vector2Int>.Some(moveCell.point);
                            } else {
                                needAttack = false;
                                selected = Option<Vector2Int>.None();
                                if (moveClr == ChColor.White) {
                                    moveClr = ChColor.Black;
                                } else {
                                    moveClr = ChColor.White;
                                }
                            }

                            possibleMoves = null;
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

        public Vector3 ToCenterCell(Vector2Int cell) {
            var offset = resources.offset.localPosition;
            var leftTop = resources.leftTop.localPosition;
            return new Vector3(-cell.y, 0.5f, cell.x) * 2 + leftTop - offset;
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