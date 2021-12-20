using System.Collections.Generic;
using UnityEngine;
using option;

namespace checkersApi {
    public struct Ch {
        public ChColor color;
        public ChType type;

        public static Ch Mk (ChColor color, ChType type) {
            return new Ch { color = color, type = type };
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

    public struct CellNode {
        public bool isAttack;
        public Vector2Int pos;
        public List<CellNode> childs;
        public int score;

        public static CellNode Mk(bool isAttack, Vector2Int pos, List<CellNode> childs, int score) {
            return new CellNode { isAttack = isAttack, pos = pos, childs = childs, score = score };
        }
    }

    public struct MovePath {
        public List<Vector2Int> cells;
        public bool isAttack;
        public int score;

        public static MovePath Mk(List<Vector2Int> cells, bool isAttack, int score) {
            return new MovePath { cells = cells, isAttack = isAttack, score = score };
        }
    }

    public static class CheckersApi {
        public static readonly List<Vector2Int> dirs = new List<Vector2Int> {
            new Vector2Int (1, 1),
            new Vector2Int (1, -1),
            new Vector2Int (-1, 1),
            new Vector2Int (-1, -1)
        };

        public static CellNode BuildTree(CellNode node, Vector2Int backDir, Option<Ch>[,] board) {
            backDir = -backDir;

            var chOpt = board[node.pos.x, node.pos.y];
            // \if (chOpt.IsNone()) return;
            var ch = chOpt.Peel();

            var xDir = -1;
            if (ch.color == ChColor.Black) {
                xDir = 1;
            }

            foreach (var dir in dirs) {
                if (dir == backDir) continue;

                var chFound = false;
                var score = node.score;
                var nextPos = node.pos + dir;

                var wrongDir = xDir != dir.x && ch.type == ChType.Basic;

                while (IsOnBoard(nextPos, board)) {
                    var nextOpt = board[nextPos.x, nextPos.y];
                    if (nextOpt.IsNone()) {
                        if (!chFound) {
                            if(!node.isAttack && !wrongDir) {
                                node.childs.Add(CellNode.Mk(false, nextPos, new List<CellNode>(), score));
                            }
                            if(ch.type == ChType.Basic) break;
                            nextPos += dir;
                            continue;
                        }

                        var clone = (Option<Ch>[,])board.Clone();
                        Move(node.pos, nextPos, clone);
                        if (ch.type == ChType.Basic) {
                            var blackPromote = ch.color == ChColor.Black && nextPos.x == 7;
                            var whitePromote = ch.color == ChColor.White && nextPos.x == 0;
                            if (blackPromote || whitePromote) {
                                ch.type = ChType.Lady;
                                score += 100;
                            }
                        }

                        node.childs.Add(
                            BuildTree(CellNode.Mk(true, nextPos, new List<CellNode>(), score), dir, clone)
                        );
                        if (ch.type == ChType.Basic) break;

                    } else {
                        var next = nextOpt.Peel();
                        if (next.color == ch.color || chFound) break;
                        score += 50;
                        if (next.type == ChType.Lady) {
                            score += 100;
                        }
                        chFound = true;
                    }
                    nextPos += dir;
                }
            }
            return node;
        }

        public static List<MovePath> GetPathsFromTree(CellNode node, List<Vector2Int> path) {
            var paths = new List<MovePath>();
            path.Add(node.pos);
            foreach (var child in node.childs) {
                paths.AddRange(GetPathsFromTree(child, path));
            }
            if (node.childs.Count == 0) {
                var newPath = new MovePath();
                newPath.cells = new List<Vector2Int>(path);
                newPath.isAttack = node.isAttack;
                newPath.score = node.score;
                paths.Add(newPath);
            }

            path.RemoveAt(path.Count - 1);
            return paths;
        }

        public static void Move(Vector2Int from, Vector2Int to, Option<Ch>[,] board) {
            board[to.x, to.y] = board[from.x, from.y];
            board[from.x, from.y] = Option<Ch>.None();
        }

        public static bool IsOnBoard<T>(Vector2Int pos, Option<T>[,] board) {
            var size = new Vector2Int(board.GetLength(0), board.GetLength(1));
            if (pos.x < 0 || pos.y < 0 || pos.x >= size.x || pos.y >= size.y) {
                return false;
            }

            return true;
        }
    }
}
