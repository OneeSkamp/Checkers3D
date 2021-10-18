using System.Collections.Generic;
using UnityEngine;
using option;
using checkers;

namespace move {
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

    public static class MoveEngine {
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
            var (possPoints, err) = CheckersEngine.GetPossiblePoints(checkerLoc);
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

        public static void Move(Move move, Option<Checker>[,] board) {
            board[move.to.x, move.to.y] = board[move.from.x, move.from.y];
            board[move.from.x, move.from.y] = Option<Checker>.None();
        }
    }
}
