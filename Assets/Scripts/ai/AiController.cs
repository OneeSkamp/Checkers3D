using System.IO;
using System.Collections.Generic;
using UnityEngine;
using controller;
using option;
using UnityEngine.UI;
using checkers;

namespace ai {
    public class AiController : MonoBehaviour {
        public CheckersController checkersController;

        private int[,] scoreBoard;

        public Button button;

        private void Awake() {
            button.onClick.AddListener(() => {
                var matrix = new Cell[10,10];
                var m = Checkers.GetMovesMatrix(
                    new Cell { index = 1, pos = new Vector2Int(5, 2), isAttack = false },
                    new Vector2Int(),
                    matrix,
                    checkersController.map.board
                );

                var p = new List<List<Cell>>();
                p.Add(new List<Cell>());

                var paths = GetPaths(m);
                foreach (var path in paths) {
                    Debug.Log("patsh");
                    foreach ( var cell in path) {
                        Debug.Log(cell.pos);
                    }
                }
            });
        }

        private int GetPathScore(List<Cell> path, Option<Ch>[,] board) {
            var ch = board[path[0].pos.x, path[0].pos.y].Peel();
            var score = 0;
            for (int i = 0; i < path.Count - 1; i++) {
                if (ch.type == ChType.Basic) {
                    var bc = ch.color == ChColor.Black && path[i + 1].pos.x == board.GetLength(0);
                    var wt = ch.color == ChColor.White && path[i + 1].pos.x == 0;

                    if (bc || wt) {
                        ch.type = ChType.Lady;
                        score += 100;
                    }
                }

                if (path[i + 1].isAttack) {
                    score += 50;
                    var dif = path[i + 1].pos - path[i].pos;
                    var attackDir = new Vector2Int(
                        dif.x / Mathf.Abs(dif.x),
                        dif.y / Mathf.Abs(dif.y)
                    );

                    var attackPos = path[i].pos + attackDir;
                    if (board[attackPos.x, attackPos.y].IsSome()) {
                        var att = board[attackPos.x, attackPos.y].Peel();
                        if (att.type == ChType.Lady) {
                            score += 100;
                        }
                    }
                } else {
                    score += 10;
                }
            }

            return score;
        }

        public static List<List<Cell>> GetPaths(Cell[,] matrix) {
            var index = 0;
            var paths = new List<List<Cell>>();
            var path = new List<Cell>();
            for (int i = 0; i < matrix.GetLength(0); i++) {
                if (matrix[index, i].index == 0) {
                    path.Add(matrix[index, i - 1]);
                    paths.Add(path);
                    index = 0;
                    if (matrix[index, i].index == 0) {
                        break;
                    }
                    path = new List<Cell>();
                }

                path.Add(matrix[index, i]);

                for (int j = index; j < matrix.GetLength(1); j++) {
                    if (matrix[j, i].index == -1) {
                        index = j;
                        break;
                    }
                }
            }

            return paths;
        }

        private void Move(List<Cell> path, Option<Ch>[,] board) {
            var attacked = new List<Vector2Int>();
            var pos = path[0].pos;
            var movePos = path[path.Count - 1].pos;
            for (int i = 1; i <= path.Count - 1; i++) {
                var dif = path[i].pos - path[i - 1].pos;
                var attackDir = new Vector2Int(
                    dif.x / Mathf.Abs(dif.x),
                    dif.y / Mathf.Abs(dif.y)
                );

                var attackPos = path[i - 1].pos + attackDir;

                attacked.Add(attackPos);
            }

            foreach (var att in attacked) {
                board[att.x, att.y] = Option<Ch>.None();
            }

            board[movePos.x, movePos.y] = board[pos.x, pos.y];
            board[pos.x, pos.y] = Option<Ch>.None();
        }
    }
}
