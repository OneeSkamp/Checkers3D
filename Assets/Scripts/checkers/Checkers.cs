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

        public static int BuildConnections(
            Vector2Int pos,
            Option<Ch>[,] board,
            Vector2Int[] points,
            int[,] connections
        ) {
            if (!IsOnBoard(pos, board)) {
                Debug.LogError("pos isn't board");
                return -1;
            }

            if (board == null) {
                Debug.LogError("board is null");
                return -1;
            }

            if (points == null) {
                Debug.LogError("points is null");
                return -1;
            }

            if (connections == null) {
                Debug.LogError("connections is null");
                return -1;
            }

            var chOpt = board[pos.x, pos.y];
            if (chOpt.IsNone()) {
                Debug.LogError("no checker on position");
            }

            var ch = board[pos.x, pos.y].Peel();
            points[0] = pos;
            var count = 1;
            count = GetPossibleConnections(pos, board, ch, points, connections, count);
            return count;
        }

        private static int GetPossibleConnections(
            Vector2Int pos,
            Option<Ch>[,] board,
            Ch ch,
            Vector2Int[] points,
            int[,] connections,
            int count
        ) {
            if (!IsOnBoard(pos, board)) {
                Debug.LogError("pos no is on board");
                return -1;
            }

            if (count < 0) {
                Debug.LogError("incorrect value for count");
                return -1;
            }

            if (points == null) {
                Debug.LogError("points is null");
                return -1;
            }

            if (connections == null) {
                Debug.LogError("connections is null");
                return -1;
            }

            if (board == null) {
                Debug.LogError("board is null");
                return -1;
            }

            var xDir = -1;
            if (ch.color == ChColor.Black) {
                xDir = 1;
            }

            foreach (var dir in dirs) {
                var wrongDir = xDir != dir.x && ch.type == ChType.Basic;

                var length = GetMaxLength(pos, board, dir);
                if (length == -1) {
                    Debug.LogError("incorrect finding length");
                    return -1;
                }

                var maxLength = length;

                if (ch.type == ChType.Basic) maxLength = 1;

                var nextPos = pos + dir * maxLength;
                if (!IsOnBoard(nextPos, board)) break;

                if (!IsAttackDir(dir, pos, board, ch.color)) {
                    for (var i = 1; i <= maxLength; i++) {
                        if (board[nextPos.x, nextPos.y].IsSome()) break;
                        var point = nextPos;

                        var colNum = count;
                        for (var j = 0; j < count; j++) {
                            if (points[j] == point) {
                                colNum = j;
                                break;
                            }
                        }
                        points[colNum] = point;
                        connections[count - 1, colNum] = 1;
                        count++;
                    }
                } else {
                    for (int i = 1; i < count; i++) {
                        points[i] = default;
                    }
                    count = 1;

                    count = GetAttackConnections(pos, board, ch, points, connections, count);
                }
            }

            return count;
        }

        private static int GetAttackConnections(
            Vector2Int pos,
            Option<Ch>[,] board,
            Ch ch,
            Vector2Int[] points,
            int[,] connections,
            int count
        ) {
            if (!IsOnBoard(pos, board)) {
                Debug.LogError("pos no is on board");
                return -1;
            }

            if (count < 0) {
                Debug.LogError("incorrect value for count");
                return -1;
            }

            if (points == null) {
                Debug.LogError("points is null");
                return -1;
            }

            if (connections == null) {
                Debug.LogError("connections is null");
                return -1;
            }

            if (board == null) {
                Debug.LogError("board is null");
                return -1;
            }

            var xDir = -1;
            if (ch.color == ChColor.Black) {
                xDir = 1;
            }

            foreach (var dir in dirs) {
                if (!IsAttackDir(dir, pos, board, ch.color)) continue;

                var length = GetMaxLength(pos, board, dir);
                if (length == -1) {
                    Debug.LogError("incorrect finding length");
                    return -1;
                }

                var wrongDir = xDir != dir.x && ch.type == ChType.Basic;
                var maxLength = length;

                if (ch.type == ChType.Basic) maxLength = 1;

                var nextPos = pos + dir * maxLength;
                if (!IsOnBoard(nextPos, board)) {
                    continue;
                }

                var point = nextPos + dir;
                if (!IsOnBoard(point, board)) {
                    continue;
                }

                var colNum = count;
                var rowNum = count;
                var oldPos = false;
                for (var i = 0; i < count; i++) {
                    if (points[i] == pos) {
                        rowNum = i;
                    }

                    if (points[i] == point) {
                        colNum = i;
                        for (int j = 0; j < board.GetLength(1); j++) {
                            if (connections[i, j] == 0) continue;

                            if (points[j] == pos) {
                                oldPos = true;
                                break;
                            }
                        }
                        break;
                    }
                }

                if (oldPos) continue;
                points[colNum] = point;
                connections[rowNum, colNum] = 1;
                count++;

                count = GetAttackConnections(point, board, ch, points, connections, count);
            }

            return count;
        }

        private static int GetMaxLength(Vector2Int pos, Option<Ch>[,] board, Vector2Int dir) {
            if (!IsOnBoard(pos, board)) {
                Debug.LogError("pos outside board");
                return -1;
            }

            if (board == null) {
                Debug.LogError("board is null");
                return -1;
            }

            var nextPos = pos + dir;
            var length = 0;
            while (IsOnBoard(nextPos, board)) {
                if (board[nextPos.x, nextPos.y].IsSome()) break;
                length++;
                nextPos += dir;
            }

            return length;
        }

        private static bool IsAttackDir(
            Vector2Int dir,
            Vector2Int pos,
            Option<Ch>[,] board,
            ChColor chColor
        ) {
            if (!IsOnBoard(pos, board)) {
                Debug.LogError("pos outside board");
                return false;
            }

            if (board == null) {
                Debug.LogError("board is null");
                return false;
            }

            var nextPos = pos + dir;
            if (!IsOnBoard(nextPos, board)) {
                return false;
            }

            var attackChOpt = board[nextPos.x, nextPos.y];
            if (attackChOpt.IsNone()) {
                return false;
            }

            var attackCh = attackChOpt.Peel();
            if (attackCh.color == chColor) {
                return false;
            }

            var attackPos = nextPos + dir;
            if (!IsOnBoard(attackPos, board)) {
                return false;
            }

            if (board[attackPos.x, attackPos.y].IsSome()) {
                return false;
            }

            return true;
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
