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

        public static int GetMovesMatrix(
            // Cell cell,
            Vector2Int pos,
            Ch ch,
            Ch moveCh,
            int count,
            bool isAttack,
            int[,] matrix,
            Vector2Int?[] arr,
            Option<Ch>[,] board
        ) {
            var xDir = -1;
            if (ch.color == ChColor.Black) {
                xDir = 1;
            }

            var posNum = PosInArr(pos, arr);
            if (posNum == null) {
                arr[count] = pos;
                posNum = count;
                count++;
            }

            var isMove = false;
            foreach (var dir in dirs) {
                var chFound = false;
                var nextPos = pos + dir;

                var wrongDir = xDir != dir.x && ch.type == ChType.Basic;

                while (IsOnBoard(nextPos, board)) {
                    var nextOpt = board[nextPos.x, nextPos.y];
                    var nextPosNum = PosInArr(nextPos, arr);
                    if (nextOpt.IsNone()) {
                        if (!chFound) {
                            if (!isAttack && !wrongDir) {
                                if (nextPosNum == null) {
                                    arr[count] = nextPos;
                                    nextPosNum = count;
                                    isMove = true;
                                }

                                matrix[posNum.Value, nextPosNum.Value] = 1;
                                count++;
                            }

                            if (ch.type == ChType.Basic) break;
                            nextPos += dir;
                            continue;
                        }

                        if (ch.type == ChType.Basic) {
                            var bc = ch.color == ChColor.Black && nextPos.x == board.GetLength(0);
                            var wt = ch.color == ChColor.White && nextPos.x == 0;

                            if (bc || wt) {
                                ch.type = ChType.Lady;
                            }
                        }
                        moveCh = ch;
                        board[nextPos.x, nextPos.y] = Option<Ch>.Some(moveCh);

                        var value = 1;
                        if (nextPosNum == null) {
                            arr[count] = nextPos;
                            nextPosNum = count;
                        } else {
                            value = 2;
                        }

                        matrix[posNum.Value, nextPosNum.Value] = value;
                        count++;

                        GetMovesMatrix(nextPos, ch, moveCh, count, isAttack, matrix, arr, board);

                        if (ch.type == ChType.Basic) break;

                    } else {
                        var next = nextOpt.Peel();
                        if (next.color == ch.color || chFound) break;
                        chFound = true;
                        isAttack = true;
                        if (isMove) {
                            for (int i = 0; i < matrix.GetLength(0); i++) {
                                matrix[0, i] = 0;
                            }

                            for (int i = 1; i < arr.Length; i++) {
                                if (arr[i] == null) break;
                                arr[i] = null;
                                count--;
                            }
                        }
                    }

                    nextPos += dir;
                }
            }

            return count;
        }

        public static int? PosInArr(Vector2Int pos, Vector2Int?[] arr) {
            for (int i = 0; i < arr.Length; i++) {
                if (pos == arr[i]) {
                    return i;
                }
            }

            return null;
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
