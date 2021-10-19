using System;
using UnityEngine;
using System.Collections.Generic;
using option;

namespace controller {
    public enum MoveErr{
        None,
        BoardIsNull,
        NoFigureSelected,
        PossiblePointsErr
    }

    public struct Move {
        public Vector2Int from;
        public Vector2Int to;

        public static Move Mk(Vector2Int from, Vector2Int to) {
            return new Move { from = from, to = to };
        }
    }

    public struct LinearMovement {
        public Vector2Int dir;
        public int length;

        public static LinearMovement Mk(Vector2Int dir, int length) {
            return new LinearMovement { dir = dir, length = length };
        }
    }

    public enum CheckersError{
        None,
        BoardIsNull,
        NoFigureSelected,
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
        public LinearMovement linear;

        public static CheckerMovement Mk(MoveType type, LinearMovement linear) {
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

    public struct Map {
        public GameObject[,] figures;
        public Option<Checker> [,] board;
    }

    public class CheckersController : MonoBehaviour {
        public Transform boardTransform;
        public Transform leftTop;

        public GameObject whiteChecker;
        public GameObject blackChecker;

        private Map map;
        private PlayerAction playerAction;
        private ChColor moveColor;
        private Vector2Int selectCheckerPos;

        private void Awake() {
            map.figures = new GameObject[8, 8];
            map.board = new Option<Checker>[8, 8];
            FillingBoard(map.board);
            FillingCheckers(map.board);
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
            var point = (localHit - new Vector3(-leftTop.position.x, 0f, leftTop.position.z)) / 2;
            var possiblePoint = new Vector2Int(Mathf.Abs((int)point.z), Mathf.Abs((int)point.x));

            var figOpt = map.board[possiblePoint.x, possiblePoint.y];
            if (figOpt.IsSome() && figOpt.Peel().color == moveColor) {
                playerAction = PlayerAction.Select;
            }

            var checkerLoc = new CheckerLoc {
                pos = selectCheckerPos,
                board = map.board
            };

            var (possMoves, possMovesErr) = GetPossibleMoves(checkerLoc);

            switch (playerAction) {
                case PlayerAction.Select:
                    selectCheckerPos = possiblePoint;

                    playerAction = PlayerAction.Move;
                    break;
                case PlayerAction.Move:
                    var move = Move.Mk(selectCheckerPos, possiblePoint);
                    foreach (var possMove in possMoves) {
                        if (move.to == possMove.to && move.from == possMove.from) {
                            Relocate(possMove);
                        }
                    }
                    playerAction = PlayerAction.None;
                    break;
            }
        }

        private Vector2Int TransformToPointOnBoard(Vector3 point, Vector3Int leftTopPos) {
            var inter = (point - new Vector3(-leftTopPos.x, 0f, leftTopPos.z)) / 2;
            return new Vector2Int(Mathf.Abs((int)inter.z), Mathf.Abs((int)inter.x));
        }

        private void FillingBoard(Option<Checker>[,] board) {
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

        private void FillingCheckers(Option<Checker>[,] board) {
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
            return new Vector2Int(Mathf.Abs((int)point.z), Mathf.Abs((int)point.x));
        }

        public static (List<CheckerMovement>, CheckersError) GetCheckersMovements(
            CheckerLoc checkerLoc
        ) {
            if (checkerLoc.board == null) {
                return (default(List<CheckerMovement>), CheckersError.BoardIsNull);
            }

            var checkerOpt = checkerLoc.board[checkerLoc.pos.x, checkerLoc.pos.y];
            if (checkerOpt.IsNone()) {
                return (default(List<CheckerMovement>), CheckersError.NoFigureSelected);
            }

            var checkerMovements = new List<CheckerMovement>();

            var checker = checkerOpt.Peel();
            switch (checker.type) {
                case ChType.Basic:
                    if (checker.color == ChColor.White) {
                        checkerMovements.Add(
                            CheckerMovement.Mk(
                                MoveType.Move,
                                LinearMovement.Mk(new Vector2Int(-1, -1), 1)
                            )
                        );
                        checkerMovements.Add(
                            CheckerMovement.Mk(
                                MoveType.Move,
                                LinearMovement.Mk(new Vector2Int(-1, 1), 1)
                            )
                        );
                    } else {
                        checkerMovements.Add(
                            CheckerMovement.Mk(
                                MoveType.Move,
                                LinearMovement.Mk(new Vector2Int(1, 1), 1)
                            )
                        );
                        checkerMovements.Add(
                            CheckerMovement.Mk(
                                MoveType.Move,
                                LinearMovement.Mk(new Vector2Int(1, -1), 1)
                            )
                        );
                    }
                    break;
            }

            return (checkerMovements, CheckersError.None);
        }

        public static List<CheckerMovement> CreateCheckerMovements(
            CheckerLoc checkerLoc,
            Func<int, int, bool> condition,
            int length
        ) {
            var checkerMovements = new List<CheckerMovement>();
            for (int i = -1; i <= 1; i++) {
                for (int j = -1; j <= 1; j++) {
                    if (condition(i, j)) continue;
                    var dir = new Vector2Int(i, j);
                    var linear = LinearMovement.Mk(dir, length);
                    checkerMovements.Add(
                        CheckerMovement.Mk(MoveType.Move, linear)
                    );
                    checkerMovements.Add(
                        CheckerMovement.Mk(MoveType.Move, linear)
                    );
                }
            }

            return checkerMovements;
        }

        public static (List<Vector2Int>, CheckersError) GetPossiblePoints(
            CheckerLoc checkerLoc
        ) {
            var res = new List<Vector2Int>();
            var (checkerMovements, err) = GetCheckersMovements(checkerLoc);
            if (err != CheckersError.None) {
                return (default(List<Vector2Int>), CheckersError.CheckersMovementsErr);
            }

            foreach (var checkerMovement in checkerMovements) {
                if (checkerMovement.type == MoveType.Move) {
                    var linear = checkerMovement.linear;
                    var pos = checkerLoc.pos;
                    var lastPoint = GetLinearPoint(pos, linear, linear.length);
                    if (checkerLoc.board[lastPoint.x, lastPoint.y].IsNone()) {
                        res.Add(lastPoint);
                    }
                }

                if (checkerMovement.type == MoveType.Attack) {
                    continue;
                }
            }

            return (res, CheckersError.None);
        }

        public static (List<Move>, MoveErr) GetPossibleMoves(CheckerLoc checkerLoc) {
            if (checkerLoc.board == null) {
                return (default(List<Move>), MoveErr.BoardIsNull);
            }

            var checkerOpt = checkerLoc.board[checkerLoc.pos.x, checkerLoc.pos.y];
            if (checkerOpt.IsNone()) {
                return (default(List<Move>), MoveErr.NoFigureSelected);
            }

            var moves = new List<Move>();
            var move = new Move();
            var (possPoints, err) = GetPossiblePoints(checkerLoc);
            if (err != CheckersError.None) {
                return (default(List<Move>), MoveErr.PossiblePointsErr);
            }
            if (possPoints.Count != 0) {
                foreach (var possPoint in possPoints) {
                    move.from = checkerLoc.pos;
                    move.to = possPoint;
                    moves.Add(move);
                }
            }

            return (moves, MoveErr.None);
        }

        public static void CheckerMove(Move move, Option<Checker>[,] board) {
            board[move.to.x, move.to.y] = board[move.from.x, move.from.y];
            board[move.from.x, move.from.y] = Option<Checker>.None();
        }

        public static Vector2Int GetLinearPoint(
            Vector2Int start,
            LinearMovement linear,
            int index
        ) {

            return start + linear.dir * index;
        }

        private void Relocate(Move move) {
            CheckerMove(move, map.board);
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
