using UnityEngine;
using option;

namespace board {
    public struct linearMovement {
        public Vector2Int dir;
        public int length;
    }

    public static class BoardEngine {
        public static int GetLenUntilFig<T>(
            Vector2Int start,
            linearMovement linear,
            Option<T>[,] board
        ) {
            var length = 0;
            for (int i = 1; i < linear.length; i++) {
                var nextPos = start + linear.dir * i;
                if (board[nextPos.x, nextPos.y].IsNone()) {
                    length++;
                }

                if (board[nextPos.x, nextPos.y].IsSome()) {
                    length++;
                    break;
                }
            }

            return length;
        }
    }
}


