using System.Collections.Generic;
using UnityEngine;
using option;

namespace checkersApi {
    public struct Ch {
        public ChColor color;
        public ChType type;

        public static Ch Mk(ChColor color, ChType type) {
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

        public static CellNode Mk(
            bool isAttack,
            Vector2Int pos,
            List<CellNode> childs,
            int score
        ) {
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

    public struct Cell {
        public int s;
        public bool isAttack;
        public Vector2Int pos;
    }

    public static class CheckersApi {
        public static int[,] matrix = new int[100, 100];

        public static readonly List<Vector2Int> dirs = new List<Vector2Int> {
            new Vector2Int (1, 1),
            new Vector2Int (1, -1),
            new Vector2Int (-1, 1),
            new Vector2Int (-1, -1)
        };

        public static Cell[,] GetMatrix(Cell cell, Vector2Int counter, Vector2Int backDir, Cell[,] m, Option<Ch>[,] board) {
            backDir = -backDir;

            var chOpt = board[cell.pos.x, cell.pos.y];
            var ch = chOpt.Peel();

            var xDir = -1;
            if (ch.color == ChColor.Black) {
                xDir = 1;
            }

            int countDir = 0;
            foreach (var dir in dirs) {
                if (dir == backDir) continue;

                var chFound = false;
                var nextPos = cell.pos + dir;

                var wrongDir = xDir != dir.x && ch.type == ChType.Basic;

                var clone = (Option<Ch>[,])board.Clone();

                while (IsOnBoard(nextPos, board)) {

                    var nextOpt = board[nextPos.x, nextPos.y];
                    if (nextOpt.IsNone()) {
                        if (!chFound) {
                            if (!cell.isAttack && !wrongDir) {
                                m[counter.x, counter.y] = new Cell { s = 1, pos = cell.pos, isAttack = cell.isAttack };
                                m[counter.y + 1, counter.y] = new Cell {s = -1, pos = nextPos, isAttack = false };
                                // m[countStep, numV + countStep] = new Cell { s = 1, pos = cell.pos, isAttack = cell.isAttack };
                                // m[numV + countStep + 1, numV + countStep] = new Cell {s = -1, pos = nextPos, isAttack = false };
                                // Debug.Log("countstep ob move" + countStep);
                                counter.y++;
                            }
                            if (ch.type == ChType.Basic) break;
                            nextPos += dir;
                            continue;
                        }

                        clone[nextPos.x, nextPos.y] = clone[cell.pos.x, cell.pos.y];
                        // clone[cell.pos.x, cell.pos.y] = Option<Ch>.None();

                        if (ch.type == ChType.Basic) {
                            var bc = ch.color == ChColor.Black && nextPos.x == board.GetLength(0);
                            var wt = ch.color == ChColor.White && nextPos.x == 0;

                            if (bc || wt) {
                                ch.type = ChType.Lady;
                            }
                        }

                        // Debug.Log("countstep att move" + countStep);
                        // // m[countStep, numV + countStep] = new Cell { s = 1, pos = cell.pos, isAttack = cell.isAttack };
                        // // m[numV + countStep + 1, numV + countStep] = new Cell {s = -1, pos = nextPos, isAttack = true };
                        // countStep++;
                        m[counter.x, counter.y] = new Cell { s = 1, pos = cell.pos, isAttack = cell.isAttack };
                        m[counter.y + 1, counter.y] = new Cell {s = -1, pos = nextPos, isAttack = false };
                                // m[countStep, numV + countStep] = new Cell { s = 1, pos = cell.pos, isAttack = cell.isAttack };
                                // m[numV + countStep + 1, numV + countStep] = new Cell {s = -1, pos = nextPos, isAttack = false };
                                // Debug.Log("countstep ob move" + countStep);
                        counter.y++;

                        GetMatrix(new Cell { s = 1, pos = nextPos, isAttack = true }, new Vector2Int(counter.y, counter.y), dir, m, clone);
                        counter.y++;
                        if (ch.type == ChType.Basic) break;

                    } else {
                        var next = nextOpt.Peel();
                        if (next.color == ch.color || chFound) break;
                        chFound = true;
                    }
                    nextPos += dir;
                }
                countDir++;
            }
            return m;
        }

        public static List<List<Cell>> FindPaths(Cell[,] matrix, int numV, List<List<Cell>> paths) {
            for (int i = 0; i < matrix.GetLength(0); i++) {
                if (matrix[numV, i].s == 1) {
                    var path = new List<Cell>();
                    for (int j = 0; j < matrix.GetLength(1); j++) {
                        if (matrix[j, i].s == -1) {
                            path.Add(matrix[numV, i]);
                            path.Add(matrix[j, i]);
                            paths.Add(path);
                            FindPaths(matrix, j, paths);
                        }
                    }
                }
            }

            return paths;
        }

        public static void ShowBoard(Cell[,] xxx) {
            var output = "                                  0";
            for (int i = 0; i < xxx.GetLength(0); i++) {
                output += $"         {i}";
            }
            Debug.Log(output);
            output = "                                  0|";

            for (int i = 0; i < xxx.GetLength(1); i++) {
                for (int j = 0; j < xxx.GetLength(0); j++) {

                    output +=  $"         { xxx[i, j].s }";
                }
                Debug.Log(output);
                output = "                                  " + (i + 1).ToString() + "|";
            }
        }

        public static CellNode BuildTree(CellNode node, Vector2Int backDir, Option<Ch>[,] board) {
            backDir = -backDir;

            var chOpt = board[node.pos.x, node.pos.y];
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
                            score = 10;
                            if(!node.isAttack && !wrongDir) {
                                node.childs.Add(
                                    CellNode.Mk(false, nextPos, new List<CellNode>(),score)
                                );
                            }
                            if(ch.type == ChType.Basic) break;
                            nextPos += dir;
                            continue;
                        }

                        var clone = (Option<Ch>[,])board.Clone();
                        board[nextPos.x, nextPos.y] = board[node.pos.x, node.pos.y];
                        board[node.pos.x, node.pos.y] = Option<Ch>.None();

                        if (ch.type == ChType.Basic) {
                            var bc = ch.color == ChColor.Black && nextPos.x == board.GetLength(0);
                            var wt = ch.color == ChColor.White && nextPos.x == 0;

                            if (bc || wt) {
                                ch.type = ChType.Lady;
                                score += 100;
                            }
                        }

                        node.childs.Add(
                            BuildTree(
                                CellNode.Mk(true, nextPos, new List<CellNode>(), score),
                                dir,
                                clone
                            )
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

        public static CellNode BuildTree2(CellNode node, Vector2Int backDir, Option<Ch>[,] board) {
            backDir = -backDir;

            var chOpt = board[node.pos.x, node.pos.y];
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
                            score = 10;
                            if(!node.isAttack && !wrongDir) {
                                node.childs.Add(
                                    CellNode.Mk(false, nextPos, new List<CellNode>(),score)
                                );
                            }
                            if(ch.type == ChType.Basic) break;
                            nextPos += dir;
                            continue;
                        }

                        var clone = (Option<Ch>[,])board.Clone();
                        board[nextPos.x, nextPos.y] = board[node.pos.x, node.pos.y];
                        board[node.pos.x, node.pos.y] = Option<Ch>.None();

                        if (ch.type == ChType.Basic) {
                            var bc = ch.color == ChColor.Black && nextPos.x == board.GetLength(0);
                            var wt = ch.color == ChColor.White && nextPos.x == 0;

                            if (bc || wt) {
                                ch.type = ChType.Lady;
                                score += 100;
                            }
                        }

                        node.childs.Add(
                            BuildTree2(
                                CellNode.Mk(true, nextPos, new List<CellNode>(), score),
                                dir,
                                clone
                            )
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

        public static Option<T>[,] CopyBoard<T>(Option<T>[,] board) {
            var clone = new Option<T>[board.GetLength(0), board.GetLength(1)];
            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; j < board.GetLength(1); j++) {
                    clone[i, j] = board[i, j];
                }
            }

            return clone;
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
