using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using controller;
using option;
using UnityEngine.UI;
using checkersApi;

namespace ai {
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


        private int counter;

        private void Awake() {
            button.onClick.AddListener(() => {
                // GetBeterPath(checkersController.map.board);
                var a = new Cell[10, 10];
                a = CheckersApi.GetMovesMatrix(
                    new Cell {pos = new Vector2Int(5, 2),
                    isAttack = false, index = 1},
                    new Vector2Int(),
                    a,
                    checkersController.map.board);
                // var paths = CheckersApi.FindPaths(a, 0, new List<List<Cell>>());
                // ReadTree(paths);
                CheckersApi.ShowBoard(a);
                Debug.Log(a[1,0].pos);
                Debug.Log(a[2,1].pos);
            });
        }

        private void ReadTree(Node node) {
            Debug.Log(node.pos + " " + node.isAttack);
            foreach(var child in node.childs) {
                Debug.Log(child.pos + " " + child.isAttack);
            }
        }

        private void ReadTree(List<List<Cell>> paths) {
            var count = 0;
            foreach (var path in paths) {
                count++;
                Debug.Log(path.Count + "  " + path + " ");
                foreach (var pos in path) {
                    Debug.Log(pos.pos + " in a path number " + count);
                }
            }
        }

        // private MovePath GetBeterPath(Option<Ch>[,] board) {
        //     var bPath = new MovePath();
        //     scorePaths = new Dictionary<MovePath, int>();
        //     for (int i = 0; i < board.GetLength(0); i++) {
        //         for (int j = 0; j < board.GetLength(1); j++) {
        //             if (board[i, j].IsNone()) continue;
        //             if (board[i, j].Peel().color != checkersController.moveClr) continue;
        //             var pos = new Vector2Int(i, j);
        //             var node = CellNode.Mk(false, pos, new List<CellNode>(), 0);
        //             var clone = CheckersApi.CopyBoard(board);
        //             var tree = CheckersApi.BuildTree(node, new Vector2Int(), clone);
        //             var paths = CheckersApi.GetPathsFromTree(tree, new List<Vector2Int>());

        //             foreach (var path in paths) {
        //                 var cloneClone = CheckersApi.CopyBoard(clone);
        //                 if (checkersController.needAttack != path.isAttack) continue;
        //                 Move(path, cloneClone);
        //                 var mScore = 0;
        //                 for (int x = 0; x < board.GetLength(0); x++) {
        //                     for (int y = 0; y < board.GetLength(1); y++) {
        //                         if (board[x, y].IsNone()) continue;
        //                         if (board[x, y].Peel().color == checkersController.moveClr) {
        //                             continue;
        //                         }
        //                         var pos2 = new Vector2Int(x, y);
        //                         var node2 = CellNode.Mk(false, pos2, new List<CellNode>(), 0);
        //                         var clone2 = CheckersApi.CopyBoard(cloneClone);
        //                         var tree2 = CheckersApi.BuildTree(node2, new Vector2Int(), clone2);
        //                         var paths2 = CheckersApi.GetPathsFromTree(
        //                             tree2,
        //                             new List<Vector2Int>()
        //                         );
        //                         foreach (var a in paths2) {
        //                             mScore += a.score;
        //                         }
        //                     }
        //                 }
        //                 scorePaths.Add(path, mScore);
        //             }
        //         }
        //     }

        //     var s = new MovePath();
        //     var min = int.MaxValue;
        //     foreach (var item in scorePaths) {
        //         if (item.Key.score != 0 && item.Value - item.Key.score < min){
        //             s = item.Key;
        //             min = item.Value - item.Key.score;
        //         }
        //     }

        //     Debug.Log(bPath.cells == null);
        //     Debug.Log(s.cells[0] + "____" + s.cells[s.cells.Count - 1] + "score " + s.score);

        //     return bPath;
        // }

        // private void Move(MovePath path, Option<Ch>[,] board) {
        //     var attacked = new List<Vector2Int>();
        //     var pos = path.cells[0];
        //     var movePos = path.cells[path.cells.Count - 1];
        //     for (int i = 1; i <= path.cells.Count - 1; i++) {
        //         var dif = path.cells[i] - path.cells[i - 1];
        //         var attackDir = new Vector2Int(
        //             dif.x / Mathf.Abs(dif.x),
        //             dif.y / Mathf.Abs(dif.y)
        //         );

        //         var attackPos = path.cells[i - 1] + attackDir;

        //         attacked.Add(attackPos);
        //     }

        //     foreach (var att in attacked) {
        //         board[att.x, att.y] = Option<Ch>.None();
        //     }

        //     board[movePos.x, movePos.y] = board[pos.x, pos.y];
        //     board[pos.x, pos.y] = Option<Ch>.None();
        // }
    }
}
