using System.ComponentModel;
using System.Collections.Generic;
using UnityEngine;
using controller;
using option;
using UnityEngine.UI;

namespace ai {
    public struct Path {
        public bool isAttack;
        public List<Vector2Int> positions;
    }

    public struct Move {
        public Vector2Int posFrom;
        public Vector2Int posTo;
    }

    public class AiController : MonoBehaviour {
        public CheckersController checkersController;
        public ChColor aiMoveColor;
        private int[,] scoreBoard;

        public Button button;

        public Dictionary<Vector2Int, List<Path>> moves;
        private int counter;

        private void Awake() {
            button.onClick.AddListener(() => GetPaths(
                new Vector2Int(3, 0),
                new List<Path>(),
                checkersController.map.board)
            );
        }


        private List<Path> GetPaths(Vector2Int pos, List<Path> paths, Option<Checker>[,] board) {
            var chOpt = board[pos.x, pos.y];
            var ch = chOpt.Peel();

            var xDir = -1;
            if (ch.color == ChColor.Black) {
                xDir = 1;
            }

            var curentPath = new Path();
            curentPath.positions = new List<Vector2Int>();
            foreach (var dir in checkersController.dirs) {
                var nextPos = pos + dir;
                var wrongDir = xDir != dir.x && ch.type == ChType.Basic;
                var chFound = false;

                while (checkersController.IsOnBoard(nextPos, board)) {
                    var nextOpt = board[nextPos.x, nextPos.y];
                    if (nextOpt.IsNone()) {
                        curentPath.isAttack = chFound;
                        if (chFound) {
                            curentPath.positions.Add(nextPos);
                            paths.Add(curentPath);

                            var clone = (Option<Checker>[,])board.Clone();
                            clone[nextPos.x, nextPos.y] = chOpt;

                            GetPaths(nextPos, paths, clone);
                        } else {
                            var next = nextOpt.Peel();
                            if (next.color == ch.color || chFound) break;
                            curentPath.positions.Add(nextPos);
                            paths.Add(curentPath);
                        }
                        if (ch.type == ChType.Basic) break;
                    } else {
                        var next = nextOpt.Peel();
                        if (next.color == ch.color) break;
                        chFound = true;
                    }
                    nextPos += dir;
                }
            }
            foreach (var path in paths) {
                Debug.Log("path");
                foreach (var poss in path.positions) {
                    Debug.Log(poss);
                }
            }

            return paths;
        }

        private void ChangeScoreBoard(Option<Checker>[,] board) {
            var possMoves = checkersController.possibleMoves;

            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; j < board.GetLength(1); j++) {
                    if (board[i, j].IsNone()) continue;
                    var ch = board[i, j].Peel();

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

        private Path GetMaxPath(List<Path> paths) {
            var maxPath = new Path();
            foreach (var path in paths) {
                if (path.isAttack && path.positions.Count > maxPath.positions.Count) {
                    maxPath = path;
                }
            }

            return maxPath;
        }

        private void GetMoves(Option<Checker>[,] board) {
            var possMoves = checkersController.possibleMoves;

            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; i < board.GetLength(1); j++) {
                    if (board[i, j].IsNone()) continue;
                    var ch = board[i, j].Peel();

                    var activeCell = new Vector2Int(i, j);
                    var move = GetPaths(activeCell, new List<Path>(), checkersController.map.board);
                    moves.Add(activeCell, move);
                }
            }
        }

        private void Move(Vector2Int from, Vector2Int to, Option<Checker>[,] board) {
            board[to.x, to.y] = board[from.x, from.y];
            board[from.x, from.y] = Option<Checker>.None();
        }
    }
}
