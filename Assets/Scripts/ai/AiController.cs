using UnityEngine;
using controller;
using option;

namespace ai {
    public class AiController : MonoBehaviour {
        public CheckersController checkersController;
        public ChColor aiMoveColor;
        public int[,] scoreBoard;

        private void Update() {
            if (checkersController.moveClr != aiMoveColor) return;
            foreach (var a in checkersController.possibleMoves) {
                foreach (var b in a.Value) {
                    if (checkersController.needAttack && b.isAttack) {

                    }
                }
            }
        }

        private void ChangeMoveColor(){

        }

        private Option<Checker>[,] GetScoreBoard(Option<Checker>[,] board) {
            var possMoves = checkersController.possibleMoves;

            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; i < board.GetLength(1); j++) {
                    if (board[i, j].IsNone()) continue;
                    var ch = board[i, j].Peel();
                    if (ch.color != aiMoveColor) continue;

                }
            }
            return default;
        }

        private void ChangeScoreBoard(Option<Checker>[,] board) {
            var possMoves = checkersController.possibleMoves;

            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; i < board.GetLength(1); j++) {
                    if (board[i, j].IsNone()) continue;
                    var ch = board[i, j].Peel();

                    // if (ch.color != aiMoveColor) continue;
                    var factor = 1;
                    if (ch.color == ChColor.White) {
                        factor = -1;
                    }

                    int moveLength = 1;
                    var activeCell = new Vector2Int(i, j);
                    foreach (var cell in possMoves[activeCell]) {
                        if (cell.isAttack) {
                            scoreBoard[i, j] = 2 * factor * moveLength;
                        } else {
                            scoreBoard[i, j] = 1 * factor;
                        }
                    }
                }
            }
        }

        private void test(Option<Checker>[,] board) {
            var possMoves = checkersController.possibleMoves;

            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; i < board.GetLength(1); j++) {
                    if (board[i, j].IsNone()) continue;
                    var ch = board[i, j].Peel();

                    var activeCell = new Vector2Int(i, j);
                    foreach (var cell in possMoves[activeCell]) {
                        Move(new Vector2Int(i, j), cell.pos, board);
                        ChangeScoreBoard(board);
                    }
                }
            }
        }

        private void Move(Vector2Int from, Vector2Int to, Option<Checker>[,] board) {
            board[to.x, to.y] = board[from.x, from.y];
            board[from.x, from.y] = Option<Checker>.None();
        }
    }
}
