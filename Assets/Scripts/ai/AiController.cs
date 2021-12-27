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
                // GetBeterPath(checkersController.map.board);
                // var a = new Cell[10, 10];
                // a = Checkers.GetMovesMatrix(
                //     new Cell {pos = new Vector2Int(5, 2),
                //     isAttack = false, index = 1},
                //     new Vector2Int(),
                //     a,
                //     checkersController.map.board);
                // // var paths = CheckersApi.FindPaths(a, 0, new List<List<Cell>>());
                // // ReadTree(paths);
                // Checkers.ShowBoard(a);
                // Debug.Log(a[1,0].pos);
                // Debug.Log(a[2,1].pos);
                var path = new List<Cell> {
                    new Cell {pos = new Vector2Int(4, 3), isAttack = false},
                    new Cell {pos = new Vector2Int(2, 1), isAttack = true},
                    new Cell {pos = new Vector2Int(0, 3), isAttack = true},
                    new Cell {pos = new Vector2Int(2, 5), isAttack = true}
                };

                Debug.Log(GetPathScore(path, checkersController.map.board));
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
