using System.IO;
using System.Collections.Generic;
using UnityEngine;
using controller;
using option;
using UnityEngine.UI;
using checkersApi;

namespace ai {
    // public struct Path {
    //     public bool isAttack;
    //     public List<Vector2Int> positions;
    // }

    public struct Node {
        public bool isAttack;
        public Vector2Int pos;
        public List<Node> childs;

        public static Node Mk(bool isAttack, Vector2Int pos, List<Node> childs) {
            return new Node { isAttack = isAttack, pos = pos, childs = childs };
        }
    }

    public struct Move {
        public Vector2Int posFrom;
        public Vector2Int posTo;
    }

    public class AiController : MonoBehaviour {
        public CheckersController checkersController;

        private int[,] scoreBoard;

        public Button button;

        public Dictionary<Vector2Int, List<MovePath>> moves;
        private int counter;

        private void Awake() {
        button.onClick.AddListener(() => {
            // Debug.Log("----------- Damka");
            // CheckBuildTree(new Vector2Int(4, 1));
            BotMove(checkersController.map.board);
        });

        }

        private void CheckBuildTree(Vector2Int pos) {
            var clone = (Option<Ch>[,])checkersController.map.board.Clone();
            var tree = CheckersApi.BuildTree(CellNode.Mk(false, pos, new List<CellNode>(), 0), new Vector2Int(), clone);
            var list = CheckersApi.GetPathsFromTree(tree, new List<Vector2Int>());
            ReadTree(list);
        }

        private void ReadTree(Node node) {
            Debug.Log(node.pos + " " + node.isAttack);
            foreach(var child in node.childs) {
                Debug.Log(child.pos + " " + child.isAttack);
            }
        }

        private void ReadTree(List<MovePath> paths) {
            var count = 0;
            foreach (var path in paths) {
                count++;
                Debug.Log(path.cells.Count + "  " + path.isAttack + " " + "score" + path.score + "path");
                foreach (var pos in path.cells) {
                    Debug.Log(pos + " in a path number " + count);
                }
            }
        }

        private MovePath GetBeterPath(List<MovePath> paths) {
            var bpath = new MovePath();
            foreach (var path in paths) {
                if (path.score > bpath.score) {
                    bpath = path;
                }
            }

            return bpath;
        }

        private void BotMove(Option<Ch>[,] board) {
            var bbPath = new MovePath();
            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; j < board.GetLength(1); j++) {
                    var pos = new Vector2Int(i, j);
                    var node = CellNode.Mk(false, pos, new List<CellNode>(), 0);
                    var clone = (Option<Ch>[,])board.Clone();
                    var tree = CheckersApi.BuildTree(node, new Vector2Int(), clone);
                    var paths = CheckersApi.GetPathsFromTree(tree, new List<Vector2Int>());
                    var bPath = GetBeterPath(paths);
                    if (bPath.score > bbPath.score) {
                        bbPath = bPath;
                    }
                }
            }
            // Debug.Log(bbPath.cells.Count);
            Debug.Log(bbPath.cells[0] + " ____ " + bbPath.cells[bbPath.cells.Count - 1] + "score " + bbPath.score);
        }

        // private void ChangeScoreBoard(Option<Checker>[,] board) {
        //     var possMoves = checkersController.possibleMoves;

        //     for (int i = 0; i < board.GetLength(0); i++) {
        //         for (int j = 0; j < board.GetLength(1); j++) {
        //             if (board[i, j].IsNone()) continue;
        //             var ch = board[i, j].Peel();

        //             var factor = 1;
        //             if (ch.color == ChColor.White) {
        //                 factor = -1;
        //             }

        //             int moveLength = 1;
        //             var activeCell = new Vector2Int(i, j);
        //             foreach (var cell in possMoves[activeCell]) {
        //                 if (cell.isAttack) {
        //                     scoreBoard[i, j] = 2 * factor * moveLength;
        //                 } else {
        //                     scoreBoard[i, j] = 1 * factor;
        //                 }
        //             }
        //         }
        //     }
        // }

        // private void Move(Vector2Int from, Vector2Int to, Option<Checker>[,] board) {
        //     board[to.x, to.y] = board[from.x, from.y];
        //     board[from.x, from.y] = Option<Checker>.None();
        // }
    }
}
