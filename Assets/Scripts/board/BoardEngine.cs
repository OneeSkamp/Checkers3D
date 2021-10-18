using UnityEngine;
using option;

namespace board {
    public struct LinearMovement {
        public Vector2Int dir;
        public int length;

        public static LinearMovement Mk(Vector2Int dir, int length) {
            return new LinearMovement { dir = dir, length = length };
        }
    }

    public static class BoardEngine {
        public static Vector2Int GetLinearPoint(
            Vector2Int start,
            LinearMovement linear,
            int index
        ) {

            return start + linear.dir * index;
        }

        public static int GetLenUntilFig<T>(
            Vector2Int start,
            LinearMovement linear,
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


