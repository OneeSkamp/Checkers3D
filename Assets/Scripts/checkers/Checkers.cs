using System.Collections.Generic;
using UnityEngine;
using option;

namespace checkers {
    public struct Ch {
        public ChColor color;
        public ChType type;
    }

    public enum ChColor {
        White,
        Black
    }

    public enum ChType {
        Basic,
        Lady
    }

    public struct Cell {
        public int index;
        public bool isAttack;
        public Vector2Int pos;
    }

    public static class Checkers {
        public static readonly List<Vector2Int> dirs = new List<Vector2Int> {
            new Vector2Int (1, 1),
            new Vector2Int (1, -1),
            new Vector2Int (-1, 1),
            new Vector2Int (-1, -1)
        };

        public static Cell[,] GetMovesMatrix(
            Cell cell,
            Vector2Int counter,
            Cell[,] matrix,
            Option<Ch>[,] board
        ) {

            var chOpt = board[cell.pos.x, cell.pos.y];
            var ch = chOpt.Peel();

            var xDir = -1;
            if (ch.color == ChColor.Black) {
                xDir = 1;
            }

            foreach (var dir in dirs) {
                var chFound = false;
                var nextPos = cell.pos + dir;

                var wrongDir = xDir != dir.x && ch.type == ChType.Basic;

                var clone = (Option<Ch>[,])board.Clone();

                while (IsOnBoard(nextPos, board)) {

                    var nextOpt = board[nextPos.x, nextPos.y];
                    if (nextOpt.IsNone()) {
                        if (!chFound) {
                            if (!cell.isAttack && !wrongDir) {
                                matrix[counter.x, counter.y] = new Cell { 
                                    index = 1,
                                    pos = cell.pos,
                                    isAttack = cell.isAttack
                                };

                                matrix[counter.y + 1, counter.y] = new Cell {
                                    index = -1,
                                    pos = nextPos,
                                    isAttack = false
                                };

                                counter.y++;
                            }
                            if (ch.type == ChType.Basic) break;
                            nextPos += dir;
                            continue;
                        }

                        clone[nextPos.x, nextPos.y] = clone[cell.pos.x, cell.pos.y];

                        if (ch.type == ChType.Basic) {
                            var bc = ch.color == ChColor.Black && nextPos.x == board.GetLength(0);
                            var wt = ch.color == ChColor.White && nextPos.x == 0;

                            if (bc || wt) {
                                ch.type = ChType.Lady;
                            }
                        }

                        matrix[counter.x, counter.y] = new Cell { 
                            index = 1,
                            pos = cell.pos,
                            isAttack = cell.isAttack
                        };

                        matrix[counter.y + 1, counter.y] = new Cell {
                            index = -1,
                            pos = nextPos,
                            isAttack = false
                        };

                        counter.y++;

                        GetMovesMatrix(
                            new Cell { index = 1, pos = nextPos, isAttack = true },
                            new Vector2Int(counter.y, counter.y),
                            matrix,
                            clone
                        );

                        counter.y++;
                        if (ch.type == ChType.Basic) break;

                    } else {
                        var next = nextOpt.Peel();
                        if (next.color == ch.color || chFound) break;
                        chFound = true;
                    }
                    nextPos += dir;
                }
            }
            return matrix;
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

                    output +=  $"         { xxx[i, j].index }";
                }
                Debug.Log(output);
                output = "                                  " + (i + 1).ToString() + "|";
            }
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
