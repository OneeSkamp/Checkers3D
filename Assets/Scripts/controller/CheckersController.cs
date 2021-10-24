using System;
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

    public struct Move {
        public Vector2Int from;
        public Vector2Int to;
        public Option<Vector2Int> destroy;

        public static Move Mk(Vector2Int from, Vector2Int to, Option<Vector2Int> destroy) {
            return new Move { from = from, to = to, destroy = destroy };
        }
    }

    public struct Linear {
        public MoveType type;
        public Vector2Int dir;
        public int length;

        public static Linear Mk(MoveType type, Vector2Int dir, int length) {
            return new Linear {type = type, dir = dir, length = length };
        }
    }

    public enum ChColor {
        White,
        Black
    }

    public enum ChType {
        Basic,
        Lady
    }

    public enum MoveType {
        Move,
        Attack
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
        private readonly Vector3 offset = new Vector3(0.95f, 0, -0.95f);
        private List<Move> moves = new List<Move>();

        private void Awake() {
            if (resources == null) {
                Debug.LogError("Resources isn't provided");
                this.enabled = false;
                return;
            }

            leftTopLocal = resources.leftTop.localPosition;

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

            var figOpt = map.board[cell.x, cell.y];
            if (figOpt.IsSome() && figOpt.Peel().color == moveColor) {
                playerAction = PlayerAction.Select;
            }

            foreach (Transform item in resources.moveHighlights) {
                Destroy(item.gameObject);
            }

            switch (playerAction) {
                case PlayerAction.Select:
                    selected = cell;
                    var checkerLoc = new CheckerLoc {
                        pos = selected,
                        board = map.board
                    };

                    var (possMoves, possMovesErr) = GetMoves(checkerLoc);

                    if (possMovesErr != CheckerErr.None) {
                        Debug.LogError("possible moves return isErr");
                    }
                    moves = possMoves;
                    var attackMoves = new List<Move>();
                    foreach (var possMove in moves) {
                        if (possMove.destroy.IsSome()) {
                            attackMoves.Add(possMove);
                        }
                    }
                    if (IsNeedAttack(map.board)) {
                        moves = attackMoves;
                    }
                    CreateMoveHighlights(moves);
                    playerAction = PlayerAction.Move;

                    break;
                case PlayerAction.Move:

                    var move = Move.Mk(selected, cell, Option<Vector2Int>.None());
                    foreach (var possMove in moves) {
                        if (move.to == possMove.to && move.from == possMove.from) {
                            Relocate(possMove);
                            if (possMove.to.x == 0 || possMove.to.x == 7) {
                                PromoteChecker(possMove.to);
                            }

                            var newCheckerLoc = new CheckerLoc {
                                pos = possMove.to,
                                board = map.board
                            };

                            if (IsAttackPos(newCheckerLoc)) {
                                if (moveColor == ChColor.Black) {
                                    moveColor = ChColor.White;
                                } else {
                                    moveColor = ChColor.Black;
                                }
                            }
                        }
                    }
                    playerAction = PlayerAction.None;

                    break;
            }
        }

        public (List<Move>, CheckerErr) GetMoves(
            CheckerLoc checkerLoc
        ) {
            var attackRes = new List<Move>();
            var movesRes = new List<Move>();
            var (checkerMovements, err) = GetCheckersMovements(checkerLoc);
            if (err != CheckerErr.None) {
                return (default(List<Move>), CheckerErr.CheckersMovementsErr);
            }

            foreach (var checkerMovement in checkerMovements) {
                var pos = checkerLoc.pos;

                for (var i = 1; i <= checkerMovement.length; i++) {
                    var next = GetLinearPoint(pos, checkerMovement, i);
                    if (!IsOnBoard(next, checkerLoc.board)) continue;
                    var lastOpt = checkerLoc.board[next.x, next.y];
                    if (checkerMovement.type == MoveType.Move) {
                        if (lastOpt.IsNone()) {
                            movesRes.Add(Move.Mk(checkerLoc.pos, next, Option<Vector2Int>.None()));
                        }
                    }

                    if (checkerMovement.type == MoveType.Attack) {
                        if (lastOpt.IsNone()) continue;
                        var last = lastOpt.Peel();
                        if (last.color == moveColor) continue;

                        var nextPos = next + checkerMovement.dir;
                        if (!IsOnBoard(nextPos, checkerLoc.board)) continue;
                        var nextOpt = checkerLoc.board[nextPos.x, nextPos.y];
                        if (nextOpt.IsNone()) {
                            attackRes.Add(
                                Move.Mk(checkerLoc.pos, nextPos, Option<Vector2Int>.Some(next))
                            );
                        } else {
                            break;
                        }
                    }
                }
            }

            if (attackRes.Count > 0) {
                return (attackRes, CheckerErr.None);
            }
            return (movesRes, CheckerErr.None);
        }

        public (List<Linear>, CheckerErr) GetCheckersMovements(
            CheckerLoc checkerLoc
        ) {
            if (checkerLoc.board == null) {
                return (default(List<Linear>), CheckerErr.BoardIsNull);
            }

            var checkerOpt = checkerLoc.board[checkerLoc.pos.x, checkerLoc.pos.y];
            if (checkerOpt.IsNone()) {
                return (default(List<Linear>), CheckerErr.PosOutsideBoard);
            }

            var movements = new List<Linear>();

            var checker = checkerOpt.Peel();
            Func<int, int, bool> cond;
            switch (checker.type) {
                case ChType.Basic:
                    cond = (int i, int j) => i != 0 && j != 0;
                    movements.AddRange(CreateLinear(checkerLoc, cond, MoveType.Attack, 1));
                    if (checker.color == ChColor.White) {
                        cond = (int i, int j) => i < 0 && Mathf.Abs(j) == 1;
                        movements.AddRange(CreateLinear(checkerLoc, cond, MoveType.Move, 1));
                    } else {
                        cond = (int i, int j) => i > 0 && Mathf.Abs(j) == 1;
                        movements.AddRange(CreateLinear(checkerLoc, cond, MoveType.Move, 1));
                    }
                    break;
                case ChType.Lady:
                        cond = (int i, int j) => i != 0 && j != 0;
                        movements.AddRange(CreateLinear(checkerLoc, cond, MoveType.Move, 8));
                    break;
            }

            return (movements, CheckerErr.None);
        }

        public List<Linear> CreateLinear(
            CheckerLoc checkerLoc,
            Func<int, int, bool> condition,
            MoveType moveType,
            int length
        ) {
            var checkerMovements = new List<Linear>();
            for (int i = -1; i <= 1; i++) {
                for (int j = -1; j <= 1; j++) {
                    if (condition(i, j)) {
                        var dir = new Vector2Int(i, j);
                        checkerMovements.Add(Linear.Mk(moveType, dir, length));
                    }
                }
            }

            return checkerMovements;
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

        public void CreateMoveHighlights(List<Move> possMoves) {
            var result = new List<GameObject>();
            if (possMoves == null) return;
            foreach (var possMove in possMoves) {
                var x = -possMove.to.y;
                var z = possMove.to.x;
                var newPos = new Vector3(x, 0.5f, z) * 2 + leftTopLocal - offset;
                var obj = Instantiate(resources.moveHighlight);
                obj.transform.parent = resources.moveHighlights;
                obj.transform.localPosition = newPos;
            }
        }

        public bool IsNeedAttack(Option<Checker>[,] board) {
            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; j < board.GetLength(1); j++) {
                    if (board[i, j].IsNone()) continue;
                    var checker = board[i, j].Peel();
                    if (checker.color != moveColor) continue;
                    var checkerLoc = new CheckerLoc { pos = new Vector2Int(i, j), board = board };
                    var (points, err) = GetMoves(checkerLoc);
                    foreach (var point in points) {
                        if (point.destroy.IsSome()) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void CheckerMove(Move move, Option<Checker>[,] board) {
            if (move.destroy.IsSome()) {
                board[move.destroy.Peel().x, move.destroy.Peel().y] = Option<Checker>.None();
            }
            board[move.to.x, move.to.y] = board[move.from.x, move.from.y];
            board[move.from.x, move.from.y] = Option<Checker>.None();
        }

        public Vector2Int GetLinearPoint(Vector2Int start, Linear linear, int index) {
            return start + linear.dir * index;
        }

        public bool IsAttackPos(CheckerLoc checkerLoc) {
            var (checkerLinear, err) = GetCheckersMovements(checkerLoc);
            if (err != CheckerErr.None) {
                return false;
            }
            var checker = checkerLoc.board[checkerLoc.pos.x, checkerLoc.pos.y].Peel();

            foreach (var linear in checkerLinear) {
                for (int i = 1; i <= linear.length; i++) {
                    var nextPos = checkerLoc.pos + linear.dir * i;
                    if (!IsOnBoard(nextPos, checkerLoc.board)) break;
                    var nextOpt = checkerLoc.board[nextPos.x, nextPos.y];
                    if (nextOpt.IsNone()) continue;
                    var next = nextOpt.Peel();
                    if (next.color == checker.color) break;
                    var lastPos = nextPos + linear.dir;
                    if (!IsOnBoard(lastPos, checkerLoc.board)) break;
                    var lastOpt = checkerLoc.board[lastPos.x, lastPos.y];
                    if (lastOpt.IsNone()) {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsOnBoard<T>(Vector2Int pos, Option<T>[,] board) {
            var size = new Vector2Int(board.GetLength(0), board.GetLength(1));
            if (pos.x < 0 || pos.y < 0 || pos.x >= size.x || pos.y >= size.y) {
                return false;
            }

            return true;
        }

        private void PromoteChecker(Vector2Int promotePos) {
            var color = ChColor.White;
            if (moveColor == ChColor.White) {
                color = ChColor.Black;
            }
            var obj = resources.blackLady;
            if (color == ChColor.White) {
                obj = resources.whiteLady;
            }
            var lady = Option<Checker>.Some(Checker.Mk(color, ChType.Lady));
            map.board[promotePos.x, promotePos.y] = lady;
            var pos = map.figures[promotePos.x, promotePos.y].transform.localPosition;
            Destroy(map.figures[promotePos.x, promotePos.y]);
            var ladyObj = Instantiate(obj);
            ladyObj.transform.parent = resources.boardTransform;
            ladyObj.transform.localPosition = pos;
            map.figures[promotePos.x, promotePos.y] = ladyObj;
        }

        private void Relocate(Move move) {
            CheckerMove(move, map.board);
            if (move.destroy.IsSome()) {
                Destroy(map.figures[move.destroy.Peel().x, move.destroy.Peel().y]);
            }

            var x = -move.to.y;
            var z = move.to.x;
            var newPos = new Vector3(x, 0.5f, z) * 2 + leftTopLocal - offset;
            map.figures[move.from.x, move.from.y].transform.localPosition = newPos;
            map.figures[move.to.x, move.to.y] = map.figures[move.from.x, move.from.y];
            if (moveColor == ChColor.White) {
                moveColor = ChColor.Black;
            } else {
                moveColor = ChColor.White;
            }
        }
    }

}