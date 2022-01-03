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

    public static class Checkers {
        public static readonly List<Vector2Int> dirs = new List<Vector2Int> {
            new Vector2Int (1, 1),
            new Vector2Int (1, -1),
            new Vector2Int (-1, 1),
            new Vector2Int (-1, -1)
        };

        public static int GetConnections(
            Vector2Int pos,
            int[,] connections,
            Vector2Int[] points,
            Option<Ch>[,] board
        ) {
            var chOpt = board[pos.x, pos.y];
            if (chOpt.IsNone()) {
                return -1;
            }
            var ch = chOpt.Peel();

            var xDir = -1;
            if (ch.color == ChColor.Black) {
                xDir = 1;
            }

            var count = GetPointsCount(points);
            var posNum = PosInPoints(pos, points);
            if (posNum == -1) {
                points[count] = pos;
                posNum = count;
            }

            foreach (var dir in dirs) {
                var wrongDir = xDir != dir.x && ch.type == ChType.Basic;

                if (!wrongDir) {
                    var point = MovePosOnDirection(pos, ch, dir, board);
                    if (point != Vector2Int.zero) {
                        points[count + 1] = point;
                        var nextPosNum = count + 1;
                        count++;
                        connections[posNum, nextPosNum] = 1;

                        if (ch.type == ChType.Basic) continue;

                        if (IsAttackPos(point, pos, board)) {
                            if (ch.type == ChType.Basic) {
                                var bc = ch.color == ChColor.Black && point.x == board.GetLength(0);
                                var wt = ch.color == ChColor.White && point.x == 0;

                                if (bc || wt) {
                                    ch.type = ChType.Lady;
                                }
                            }

                            board[point.x, point.y] = Option<Ch>.Some(ch);
                            GetConnections(point, connections, points, board);
                        } else {
                            if (ch.type == ChType.Basic) {
                                continue;
                            }
                        }
                    }
                }
            }

            return count;
        }

        public static Vector2Int MovePosOnDirection(
            Vector2Int pos,
            Ch ch,
            Vector2Int dir,
            Option<Ch>[,] board
        ) {
            var nextPos = pos + dir;
            var chFound = false;
            while (IsOnBoard(nextPos, board)) {
                var nextOpt = board[nextPos.x, nextPos.y];
                if (nextOpt.IsNone()) {
                    return nextPos;
                } else {
                    var next = nextOpt.Peel();
                    if (next.color == ch.color || chFound) break;
                    chFound = true;
                }

                nextPos += dir;
            }

            return default;
        }

        public static int GetPointsCount(Vector2Int[] points) {
            var count = 0;
            foreach (var point in points) {
                if (point == Vector2Int.zero) break;
                count++;
            }

            return count;
        }

        public static bool IsAttackPos(
            Vector2Int pos,
            Vector2Int previusPos,
            Option<Ch>[,] board
        ) {
            var dif = pos - previusPos;
            var attackDir = new Vector2Int(
                dif.x / Mathf.Abs(dif.x),
                dif.y / Mathf.Abs(dif.y)
            );

            var attackPos = previusPos + attackDir;

            if (board[attackPos.x, attackPos.y].IsSome()) {
                return true;
            }

            return false;
        }

        public static int PosInPoints(Vector2Int pos, Vector2Int[] arr) {
            for (int i = 0; i < arr.Length; i++) {
                if (pos == arr[i]) {
                    return i;
                }
            }

            return -1;
        }

        public static void ShowMatrix(int[,] xxx) {
            var output = "                                  0";
            for (int i = 0; i < xxx.GetLength(0); i++) {
                output += $"         {i}";
            }
            Debug.Log(output);
            output = "                                  0|";

            for (int i = 0; i < xxx.GetLength(1); i++) {
                for (int j = 0; j < xxx.GetLength(0); j++) {

                    output +=  $"         { xxx[i, j] }";
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
