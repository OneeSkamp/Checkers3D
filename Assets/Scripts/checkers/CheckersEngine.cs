using System.Collections.Generic;
using UnityEngine;
using board;
using option;

namespace checkers {
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

    public static class CheckersEngine {
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
                        checkerMovements.Add(
                            CheckerMovement.Mk(
                                MoveType.Attack,
                                LinearMovement.Mk(new Vector2Int(1, 1), 1))
                        );
                        checkerMovements.Add(
                            CheckerMovement.Mk(
                                MoveType.Attack,
                                LinearMovement.Mk(new Vector2Int(1, -1), 1))
                        );
                        checkerMovements.Add(
                            CheckerMovement.Mk(
                                MoveType.Attack,
                                LinearMovement.Mk(new Vector2Int(-1, -1), 1))
                        );
                        checkerMovements.Add(
                            CheckerMovement.Mk(
                                MoveType.Attack,
                                LinearMovement.Mk(new Vector2Int(-1, 1), 1))
                        );
                    if (checker.color == ChColor.White) {
                        checkerMovements.Add(
                            CheckerMovement.Mk(
                                MoveType.Move,
                                LinearMovement.Mk(new Vector2Int(-1, -1), 1))
                        );
                        checkerMovements.Add(
                            CheckerMovement.Mk(
                                MoveType.Move,
                                LinearMovement.Mk(new Vector2Int(-1, 1), 1))
                        );
                    } else {
                        checkerMovements.Add(
                            CheckerMovement.Mk(
                                MoveType.Move,
                                LinearMovement.Mk(new Vector2Int(1, 1), 1))
                        );
                        checkerMovements.Add(
                            CheckerMovement.Mk(
                                MoveType.Move,
                                LinearMovement.Mk(new Vector2Int(1, -1), 1))
                        );
                    }
                    break;
            }

            return (checkerMovements, CheckersError.None);
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
                    var lastPoint = BoardEngine.GetLinearPoint(pos, linear, linear.length);
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
    }
}

