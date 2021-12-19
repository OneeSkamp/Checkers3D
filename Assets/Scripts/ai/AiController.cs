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
        public ChColor aiMoveColor;
        private int[,] scoreBoard;

        public Button button;

        public Dictionary<Vector2Int, List<Path>> moves;
        private int counter;

        private void Awake() {
        button.onClick.AddListener(() => {
        //     Debug.Log("---------- Peshka");
        // CheckBuildTree(new Vector2Int(3, 2));
        Debug.Log("----------- Damka");
        CheckBuildTree(new Vector2Int(2, 5));
        });

        }

        private Node BuildTree(Node node, Vector2Int backDir, Option<Checker>[,] board) {
            backDir = -backDir;

            var chOpt = board[node.pos.x, node.pos.y];

            var ch = chOpt.Peel();

            var xDir = -1;
            if (ch.color == ChColor.Black) {
                xDir = 1;
            }

            foreach (var dir in checkersController.dirs) {
                if (dir == backDir) continue;

                var chFound = false;
                var nextPos = node.pos + dir;


                var wrongDir = xDir != dir.x && ch.type == ChType.Basic;

                while (checkersController.IsOnBoard(nextPos, board)) {
                    var nextOpt = board[nextPos.x, nextPos.y];
                    if (nextOpt.IsNone()) {
                        if (!chFound) {
                            if(!node.isAttack && !wrongDir) {
                                node.childs.Add(Node.Mk(false, nextPos, new List<Node>()));
                            }
                            if(ch.type == ChType.Basic) break;
                            nextPos += dir;
                            continue;
                        }

                        var clone = (Option<Checker>[,])board.Clone();
                        Move(node.pos, nextPos, clone);
                        node.childs.Add(BuildTree(Node.Mk(true, nextPos, new List<Node>()), dir, clone));
                        if (ch.type == ChType.Basic) break;

                    } else {

                        var next = nextOpt.Peel();
                        if (next.color == ch.color || chFound) break;
                        chFound = true;
                    }
                    nextPos += dir;
                }
            }
            return node;
        }

        private List<Path> GetPaths(Node node, List<Vector2Int> path) {
            var paths = new List<Path>();
            path.Add(node.pos);
            foreach (var child in node.childs) {
                paths.AddRange(GetPaths(child, path));
            }
            if (node.childs.Count == 0) {
                var newPath = new Path();
                newPath.positions = new List<Vector2Int>(path);
                newPath.isAttack = node.isAttack;
                paths.Add(newPath);
            }

            path.RemoveAt(path.Count - 1);
            return paths;
        }

        private void CheckBuildTree(Vector2Int pos) {
            var clone = (Option<Checker>[,]) checkersController.map.board.Clone();
            var tree = BuildTree(Node.Mk(false, pos, new List<Node>()), new Vector2Int(), clone);
            var list = GetPaths(tree, new List<Vector2Int>());
            ReadTree(list);
        }

        private void ReadTree(Node node) {
            Debug.Log(node.pos + " " + node.isAttack);
            foreach(var child in node.childs) {
                Debug.Log(child.pos + " " + child.isAttack);
            }
        }

        private void ReadTree(List<Path> paths) {
            var count = 0;
            foreach (var path in paths) {
                count++;
                Debug.Log(path.positions.Count + "  " + path.isAttack + " " + "path");
                foreach (var pos in path.positions) {
                    Debug.Log(pos + " in a path number " + count);
                }
            }
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

        private void Move(Vector2Int from, Vector2Int to, Option<Checker>[,] board) {
            board[to.x, to.y] = board[from.x, from.y];
            board[from.x, from.y] = Option<Checker>.None();
        }
    }
}
