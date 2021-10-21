using System;
using UnityEngine;
using System.Collections.Generic;
using option;

namespace controller {
    public enum MoveErr{
        None,
        BoardIsNull,
        NoFigureSelected,
        PossiblePointsErr,
        CheckersMovementsErr
    }

    public struct Move {
        public Vector2Int from;
        public Vector2Int to;
        public Option<Vector2Int> destroy;

        public static Move Mk(Vector2Int from, Vector2Int to) {
            return new Move { from = from, to = to };
        }
    }

    public struct Linear {
        public Vector2Int dir;
        public int length;

        public static Linear Mk(Vector2Int dir, int length) {
            return new Linear { dir = dir, length = length };
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

    public struct CheckerMovement {
        public MoveType type;
        public Linear linear;

        public static CheckerMovement Mk(MoveType type, Linear linear) {
            return new CheckerMovement { type = type, linear = linear };
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

    public struct MovePoints {
        public Vector2Int to;
        public Option<Vector2Int> sentenced;

        public static MovePoints Mk(Vector2Int to, Option<Vector2Int> sentenced) {
            return new MovePoints { to = to, sentenced = sentenced };
        }
    }

    public struct Map {
        public GameObject[,] figures;
        public Option<Checker> [,] board;
    }

    public class CheckersController : MonoBehaviour {
        public Transform boardTransform;
        public Transform leftTop;
        public Transform moveHighlights;

        public GameObject whiteChecker;
        public GameObject blackChecker;
        public GameObject moveHighlight; 

        private Map map;
        private PlayerAction playerAction;
        private ChColor moveColor;
        private Vector2Int selectCheckerPos;
        private List<Move> moves = new List<Move>();

        private void Awake() {
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
            RaycastHit hit;

            if (!Physics.Raycast(ray, out hit)) {
                return;
            }

            var localHit = boardTransform.InverseTransformPoint(hit.point);
            var point = TransformToPointOnBoard(localHit, leftTop.position);

            var figOpt = map.board[point.x, point.y];
            if (figOpt.IsSome() && figOpt.Peel().color == moveColor) {
                playerAction = PlayerAction.Select;
            }
            RemoveHighlights(moveHighlights);

            switch (playerAction) {
                case PlayerAction.Select:
                    selectCheckerPos = point;
                    var checkerLoc = new CheckerLoc {
                        pos = selectCheckerPos,
                        board = map.board
                    };

                    var (possMoves, possMovesErr) = GetPossibleMoves(checkerLoc);
                    if (possMovesErr != MoveErr.None) {
                        Debug.LogError("possible moves return isErr");
                    }
                    moves = possMoves;
                    CreateMoveHighlights(moves);
                    playerAction = PlayerAction.Move;

                    break;
                case PlayerAction.Move:
                    var move = Move.Mk(selectCheckerPos, point);
                    foreach (var possMove in moves) {
                        if (move.to == possMove.to && move.from == possMove.from) {
                            Relocate(possMove);
                        }
                    }
                    playerAction = PlayerAction.None;

                    break;
            }
        }

        public (List<Move>, MoveErr) GetPossibleMoves(CheckerLoc checkerLoc) {
            if (checkerLoc.board == null) {
                return (default(List<Move>), MoveErr.BoardIsNull);
            }

            var checkerOpt = checkerLoc.board[checkerLoc.pos.x, checkerLoc.pos.y];
            if (checkerOpt.IsNone()) {
                return (default(List<Move>), MoveErr.NoFigureSelected);
            }

            var moves = new List<Move>();
            var (possPoints, err) = GetPossiblePoints(checkerLoc);
            if (err != MoveErr.None) {
                return (default(List<Move>), MoveErr.PossiblePointsErr);
            }

            if (possPoints.Count != 0) {
                foreach (var possPoint in possPoints) {
                    var move = new Move();
                    move.from = checkerLoc.pos;
                    move.to = possPoint.to;
                    if (possPoint.sentenced.IsSome()) {
                        move.destroy = Option<Vector2Int>.Some(possPoint.sentenced.Peel());
                    }
                    moves.Add(move);
                }
            }

            return (moves, MoveErr.None);
        }

        public (List<MovePoints>, MoveErr) GetPossiblePoints(
            CheckerLoc checkerLoc
        ) {
            var res = new List<MovePoints>();
            var (checkerMovements, err) = GetCheckersMovements(checkerLoc);
            if (err != MoveErr.None) {
                return (default(List<MovePoints>), MoveErr.CheckersMovementsErr);
            }

            foreach (var checkerMovement in checkerMovements) {
                var linear = checkerMovement.linear;
                var pos = checkerLoc.pos;
                var lastPoint = GetLinearPoint(pos, linear, linear.length);
                if (!IsOnBoard(lastPoint, checkerLoc.board)) continue;
                var lastOpt = checkerLoc.board[lastPoint.x, lastPoint.y];
                if (checkerMovement.type == MoveType.Move) {
                    if (lastOpt.IsNone()) {
                        res.Add(MovePoints.Mk(lastPoint, Option<Vector2Int>.None()));
                    }
                }

                if (checkerMovement.type == MoveType.Attack) {
                    if (lastOpt.IsNone()) continue;
                    var last = lastOpt.Peel();
                    if (last.color == moveColor) continue;
                    for (var i = 1; i <= linear.length; i++) {
                        var nextPos = lastPoint + linear.dir * i;
                        if (!IsOnBoard(nextPos, checkerLoc.board)) continue;
                        var nextOpt = checkerLoc.board[nextPos.x, nextPos.y];
                        if (nextOpt.IsNone()) {
                            res.Add(MovePoints.Mk(nextPos, Option<Vector2Int>.Some(lastPoint)));
                        } else {
                            break;
                        }
                    }
                }
            }

            return (res, MoveErr.None);
        }

        public static (List<CheckerMovement>, MoveErr) GetCheckersMovements(
            CheckerLoc checkerLoc
        ) {
            if (checkerLoc.board == null) {
                return (default(List<CheckerMovement>), MoveErr.BoardIsNull);
            }

            var checkerOpt = checkerLoc.board[checkerLoc.pos.x, checkerLoc.pos.y];
            if (checkerOpt.IsNone()) {
                return (default(List<CheckerMovement>), MoveErr.NoFigureSelected);
            }

            var checkerMovements = new List<CheckerMovement>();

            var checker = checkerOpt.Peel();
            Func<int, int, bool> cond;
            switch (checker.type) {
                case ChType.Basic:
                    if (checker.color == ChColor.White) {
                        cond = (int i, int j) => i < 0 && Mathf.Abs(j) == 1;
                        checkerMovements.AddRange(CreateCheckerMovements(checkerLoc, cond, 1));
                    } else {
                        cond = (int i, int j) => i > 0 && Mathf.Abs(j) == 1;
                        checkerMovements.AddRange(CreateCheckerMovements(checkerLoc, cond, 1));
                    }
                    break;
            }

            return (checkerMovements, MoveErr.None);
        }

        public static List<CheckerMovement> CreateCheckerMovements(
            CheckerLoc checkerLoc,
            Func<int, int, bool> condition,
            int length
        ) {
            var checkerMovements = new List<CheckerMovement>();
            for (int i = -1; i <= 1; i++) {
                for (int j = -1; j <= 1; j++) {
                    if (condition(i, j)) {
                        var dir = new Vector2Int(i, j);
                        var linear = Linear.Mk(dir, length);
                        checkerMovements.Add(
                            CheckerMovement.Mk(MoveType.Move, linear)
                        );
                        checkerMovements.Add(
                            CheckerMovement.Mk(MoveType.Attack, linear)
                        );
                    }
                }
            }

            return checkerMovements;
        }

        private void FillBoard(Option<Checker>[,] board) {
            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; j < board.GetLength(1); j++) {
                    if (i % 2 == 0 && j % 2 != 0 || i % 2 != 0 && j % 2 == 0) {
                        if (i <= 2) {
                            map.board[i, j] = Option<Checker>.Some(
                                Checker.Mk(ChColor.Black, ChType.Basic)
                            );
                        }

                        if (i >= 5) {
                            map.board[i, j] = Option<Checker>.Some(
                                Checker.Mk(ChColor.White, ChType.Basic)
                            );
                        }
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
                    if (checker.color == ChColor.Black) {
                        map.figures[i, j] = Instantiate(blackChecker);
                    }

                    if (checker.color == ChColor.White) {
                        map.figures[i, j] = Instantiate(whiteChecker);
                    }

                    map.figures[i, j].transform.parent = boardTransform;
                    var offset = new Vector3(0.95f, 0, -0.95f);
                    var newPos = new Vector3(-j, 0.5f, i) * 2 + leftTop.localPosition - offset;
                    map.figures[i, j].transform.localPosition = newPos;
                }
            }
        }

        public static Vector2Int TransformToPointOnBoard(Vector3 leftTopPos, Vector3 point) {
            var intermediate = (point - new Vector3(-leftTopPos.x, 0f, leftTopPos.z)) / 2;
            return new Vector2Int(Mathf.Abs((int)intermediate.z), Mathf.Abs((int)intermediate.x));
        }

        public void CreateMoveHighlights(List<Move> possMoves) {
            var result = new List<GameObject>();
            if (possMoves == null) return;
            foreach (var possMove in possMoves) {
                var offset = new Vector3(0.95f, 0, -0.95f);
                var x = -possMove.to.y;
                var z = possMove.to.x;
                var newPos = new Vector3(x, 0.5f, z) * 2 + leftTop.localPosition - offset;
                var obj = Instantiate(moveHighlight);
                obj.transform.parent = moveHighlights;
                obj.transform.localPosition = newPos;
            }
        }

        public static void CheckerMove(Move move, Option<Checker>[,] board) {
            if (move.destroy.IsSome()) {
                board[move.destroy.Peel().x, move.destroy.Peel().y] = Option<Checker>.None();
            }
            board[move.to.x, move.to.y] = board[move.from.x, move.from.y];
            board[move.from.x, move.from.y] = Option<Checker>.None();
        }

        private void RemoveHighlights(Transform highlights) {
            foreach (Transform item in highlights) {
                Destroy(item.gameObject);
            }
        }

        public static Vector2Int GetLinearPoint(Vector2Int start, Linear linear, int index) {
            return start + linear.dir * index;
        }

        public static bool IsOnBoard<T>(Vector2Int pos, Option<T>[,] board) {
            var size = new Vector2Int(board.GetLength(0), board.GetLength(1));
            if (pos.x < 0 || pos.y < 0 || pos.x >= size.x || pos.y >= size.y) {
                return false;
            }

            return true;
        }

        private void Relocate(Move move) {
            CheckerMove(move, map.board);
            if (move.destroy.IsSome()) {
                Destroy(map.figures[move.destroy.Peel().x, move.destroy.Peel().y]);
            }

            var offset = new Vector3(0.95f, 0, -0.95f);
            var x = -move.to.y;
            var z = move.to.x;
            var newPos = new Vector3(x, 0.5f, z) * 2 + leftTop.localPosition - offset;
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