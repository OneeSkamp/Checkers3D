using System.IO.Compression;
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
        private Option<Vector2Int> selected;

        private List<Vector2Int> dirs = new List<Vector2Int>();
        private HashSet<Vector2Int> attacked = new HashSet<Vector2Int>();
        private Dictionary<Vector2Int, List<MoveCell>> possibleMoves;

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
            if (selected.IsSome()) {
                resources.select.SetActive(true);
                resources.select.transform.parent = resources.boardTransform;
                resources.select.transform.localPosition = ToCenterCell(selected.Peel());
            } else {
                resources.select.SetActive(false);
            }

            if (possibleMoves == null) {
                possibleMoves = new Dictionary<Vector2Int, List<MoveCell>>();
                for (int i = 0; i < map.board.GetLength(0); i++) {
                    for (int j = 0; j < map.board.GetLength(1); j++) {
                        var chPos = new Vector2Int(i, j);
                        if (selected.IsSome()) {
                            if (selected.Peel() != chPos) continue;
                        }

                        var chOpt = map.board[chPos.x, chPos.y];
                        if (chOpt.IsNone() || chOpt.Peel().color != moveClr) continue;
                        var ch = chOpt.Peel();

                        var xDir = -1;
                        if (ch.color == ChColor.Black) {
                            xDir = 1;
                        }

                        var moves = new List<MoveCell>();
                        foreach (var dir in dirs) {
                            var checkerFound = false;
                            var nextPos = chPos + dir;
                            while (IsOnBoard(nextPos, map.board)) {
                                var nextOpt = map.board[nextPos.x, nextPos.y];
                                if (nextOpt.IsNone()) {
                                    var wrongDir = xDir != dir.x && ch.type == ChType.Basic;
                                    if (!checkerFound && wrongDir) break;
                                    var moveCell = new MoveCell();
                                    moveCell.isAttack = checkerFound;
                                    moveCell.point = nextPos;
                                    moves.Add(moveCell);
                                    if (ch.type == ChType.Basic) break;
                                } else {
                                    var next = nextOpt.Peel();
                                    if (next.color == ch.color || checkerFound) break;
                                    checkerFound = true;
                                }
                                if (ch.type == ChType.Basic && !checkerFound) break;
                                nextPos += dir;
                            }
                            checkerFound = false;
                        }

                        possibleMoves.Add(chPos, moves);
                    }
                }
            }

            if (!Input.GetMouseButtonDown(0)) {
                return;
            }

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out RaycastHit hit)) {
                return;
            }

            var clicked = ToCell(hit.point, resources.leftTop.position);

            foreach (Transform item in resources.moveHighlights) {
                Destroy(item.gameObject);
            }

            var checkerOpt = map.board[clicked.x, clicked.y];
            if (checkerOpt.IsSome() && checkerOpt.Peel().color == moveClr) {
                selected = Option<Vector2Int>.Some(clicked);
            }

            if (possibleMoves.Count == 0) {
                Debug.Log("end");
                this.enabled = false;
            }

            if (selected.IsSome()) {
                var moveCells = possibleMoves[selected.Peel()];
                var needAttack = false;
                foreach (var moves in possibleMoves) {
                    foreach (var move in moves.Value) {
                        if (move.isAttack) {
                            needAttack = true;
                            break;
                        }
                    }
                }

                foreach (var moveCell in moveCells) {
                    if (needAttack && !moveCell.isAttack) continue;
                    var highlight = Instantiate(resources.moveHighlight);
                    highlight.transform.parent = resources.moveHighlights;
                    highlight.transform.localPosition = ToCenterCell(moveCell.point);
                }

                var slct = selected.Peel();
                if (map.board[clicked.x, clicked.y].IsNone()) {

                    foreach (var moveCell in moveCells) {
                        if (needAttack && !moveCell.isAttack) continue;

                        if (moveCell.point == clicked) {
                            map.board[clicked.x, clicked.y] = map.board[slct.x, slct.y];
                            map.board[slct.x, slct.y] = Option<Checker>.None();

                            var prefab = map.figures[slct.x, slct.y];
                            map.figures[clicked.x, clicked.y] = prefab;

                            prefab.transform.localPosition = ToCenterCell(clicked);

                            var checker = map.board[clicked.x, clicked.y].Peel();
                            if (clicked.x == 0 || clicked.x == map.board.GetLength(0) - 1) {
                                var obj = resources.blackLady;
                                if (moveClr == ChColor.White) {
                                    obj = resources.whiteLady;
                                }
                                var lady = Option<Checker>.Some(Checker.Mk(moveClr, ChType.Lady));
                                map.board[clicked.x, clicked.y] = lady;
                                var pos = map.figures[clicked.x, clicked.y].transform.localPosition;
                                Destroy(map.figures[clicked.x, clicked.y]);
                                var ladyObj = Instantiate(obj);
                                ladyObj.transform.parent = resources.boardTransform;
                                ladyObj.transform.localPosition = pos;
                                map.figures[clicked.x, clicked.y] = ladyObj;
                            }

                            var dif = moveCell.point - selected.Peel();
                            var attackDir = new Vector2Int(
                                dif.x/Mathf.Abs(dif.x),
                                dif.y/Mathf.Abs(dif.y)
                            );

                            var attackPos = selected.Peel() + attackDir;
                            while (IsOnBoard(attackPos, map.board)) {
                                if (attackPos == clicked) break;
                                if (map.board[attackPos.x, attackPos.y].IsSome()) {
                                    attacked.Add(attackPos);
                                }

                                attackPos += attackDir;
                            }

                            if (moveCell.isAttack) {
                                var chOpt = map.board[moveCell.point.x, moveCell.point.y];
                                if (chOpt.IsNone() || chOpt.Peel().color != moveClr) continue;
                                var ch = chOpt.Peel();

                                var moves = new List<MoveCell>();
                                foreach (var dir in dirs) {
                                    var checkerFound = false;
                                    var nextPos = moveCell.point + dir;
                                    while (IsOnBoard(nextPos, map.board)) {
                                        var nextOpt = map.board[nextPos.x, nextPos.y];

                                        if (nextOpt.IsSome()) {
                                            if (checkerFound) break;
                                            var next = nextOpt.Peel();
                                            if (attacked.Contains(nextPos)) break;
                                            if (next.color == ch.color) break;
                                            checkerFound = true;
                                        } else if (nextOpt.IsNone() && checkerFound) {
                                            var move = new MoveCell();
                                            move.isAttack = true;
                                            move.point = nextPos;
                                            moves.Add(move);
                                            if (ch.type == ChType.Basic) break;
                                        }

                                        if (ch.type == ChType.Basic && !checkerFound) break;
                                        nextPos += dir;
                                    }
                                }

                                foreach (var mv in moves) {
                                    if (mv.isAttack) {
                                        possibleMoves.Clear();
                                        possibleMoves.Add(moveCell.point, moves);
                                        selected = Option<Vector2Int>.Some(moveCell.point);
                                        return;
                                    }
                                }
                            }

                            foreach (var attack in attacked) {
                                Destroy(map.figures[attack.x, attack.y]);
                                map.board[attack.x, attack.y] = Option<Checker>.None();
                            }

                            attacked.Clear();
                            selected = Option<Vector2Int>.None();
                            if (moveClr == ChColor.White) {
                                moveClr = ChColor.Black;
                            } else {
                                moveClr = ChColor.White;
                            }
                            possibleMoves = null;
                            return;
                        }
                    }
                }
            }

            if (selected.IsNone()) {
                resources.select.SetActive(false);
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
            return new Vector3(-cell.y, 0.51f, cell.x) * 2 + leftTop - offset;
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