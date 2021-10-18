using UnityEngine;
using option;
using checkers;
using move;

namespace controller {
    public enum PlayerAction {
        None,
        Select,
        Move
    }

    public struct Map {
        public GameObject[,] figures;
        public Option<Checker> [,] board;
    }

    public class CheckersController : MonoBehaviour {
        public Transform boardTransform;
        public Transform leftTop;

        public GameObject wChecker;
        public GameObject bChecker;

        private Map map;
        private PlayerAction playerAction;
        private ChColor moveColor;
        private Vector2Int selectCheckerPos;


        private void Awake() {
            map.figures = new GameObject[8, 8];
            map.board = new Option<Checker>[8, 8];
            map.board[0, 1] = Option<Checker>.Some(Checker.Mk(ChColor.Black, ChType.Basic));
            map.board[0, 3] = Option<Checker>.Some(Checker.Mk(ChColor.Black, ChType.Basic));
            map.board[0, 5] = Option<Checker>.Some(Checker.Mk(ChColor.Black, ChType.Basic));
            map.board[0, 7] = Option<Checker>.Some(Checker.Mk(ChColor.Black, ChType.Basic));
            map.board[1, 0] = Option<Checker>.Some(Checker.Mk(ChColor.Black, ChType.Basic));
            map.board[1, 2] = Option<Checker>.Some(Checker.Mk(ChColor.Black, ChType.Basic));
            map.board[1, 4] = Option<Checker>.Some(Checker.Mk(ChColor.Black, ChType.Basic));
            map.board[1, 6] = Option<Checker>.Some(Checker.Mk(ChColor.Black, ChType.Basic));
            map.board[2, 1] = Option<Checker>.Some(Checker.Mk(ChColor.Black, ChType.Basic));
            map.board[2, 3] = Option<Checker>.Some(Checker.Mk(ChColor.Black, ChType.Basic));
            map.board[2, 5] = Option<Checker>.Some(Checker.Mk(ChColor.Black, ChType.Basic));
            map.board[2, 7] = Option<Checker>.Some(Checker.Mk(ChColor.Black, ChType.Basic));
            map.board[7, 0] = Option<Checker>.Some(Checker.Mk(ChColor.White, ChType.Basic));
            map.board[7, 2] = Option<Checker>.Some(Checker.Mk(ChColor.White, ChType.Basic));
            map.board[7, 4] = Option<Checker>.Some(Checker.Mk(ChColor.White, ChType.Basic));
            map.board[7, 6] = Option<Checker>.Some(Checker.Mk(ChColor.White, ChType.Basic));
            map.board[6, 1] = Option<Checker>.Some(Checker.Mk(ChColor.White, ChType.Basic));
            map.board[6, 3] = Option<Checker>.Some(Checker.Mk(ChColor.White, ChType.Basic));
            map.board[6, 5] = Option<Checker>.Some(Checker.Mk(ChColor.White, ChType.Basic));
            map.board[6, 7] = Option<Checker>.Some(Checker.Mk(ChColor.White, ChType.Basic));
            map.board[5, 0] = Option<Checker>.Some(Checker.Mk(ChColor.White, ChType.Basic));
            map.board[5, 2] = Option<Checker>.Some(Checker.Mk(ChColor.White, ChType.Basic));
            map.board[5, 4] = Option<Checker>.Some(Checker.Mk(ChColor.White, ChType.Basic));
            map.board[5, 6] = Option<Checker>.Some(Checker.Mk(ChColor.White, ChType.Basic));
            FillingBoard(map.board);
        }

        private void Update() {
            if (!Input.GetMouseButtonDown(0)) {
                return;
            }

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (!Physics.Raycast(ray, out hit)) {
                return;
            }

            var localHit = boardTransform.InverseTransformPoint(hit.point);
            var point = (localHit - new Vector3(-leftTop.position.x, 0f, leftTop.position.z)) / 2;
            var possiblePoint = new Vector2Int(Mathf.Abs((int)point.z), Mathf.Abs((int)point.x));

            var figOpt = map.board[possiblePoint.x, possiblePoint.y];
            if (figOpt.IsSome() && figOpt.Peel().color == moveColor) {
                playerAction = PlayerAction.Select;
            }

            var checkerLoc = new CheckerLoc {
                pos = selectCheckerPos,
                board = map.board
            };

            var (possMoves, possMovesErr) = MoveEngine.GetPossibleMoves(checkerLoc);

            switch (playerAction) {
                case PlayerAction.Select:
                    selectCheckerPos = possiblePoint;

                    playerAction = PlayerAction.Move;
                    break;
                case PlayerAction.Move:
                    var move = Move.Mk(selectCheckerPos, possiblePoint);
                    foreach (var possMove in possMoves) {
                        if (move.to == possMove.to && move.from == possMove.from) {
                            Relocate(possMove);
                        }
                    }
                    playerAction = PlayerAction.None;
                    break;
            }
        }

        private void FillingBoard(Option<Checker>[,] board) {
            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; j < board.GetLength(1); j++) {
                    if (board[i, j].IsNone()) {
                        continue;
                    }

                    var checker = board[i, j].Peel();
                    if (checker.color == ChColor.Black) {
                        map.figures[i, j] = Instantiate(bChecker);
                    }

                    if (checker.color == ChColor.White) {
                        map.figures[i, j] = Instantiate(wChecker);
                    }

                    map.figures[i, j].transform.parent = boardTransform;
                    var offset = new Vector3(0.95f, 0, -0.95f);
                    var newPos = new Vector3(-j, 0.5f, i) * 2 + leftTop.localPosition - offset;
                    map.figures[i, j].transform.localPosition = newPos;
                }
            }
        }

        private void Relocate(Move move) {
            MoveEngine.Move(move, map.board);
            var offset = new Vector3(0.95f, 0, -0.95f);
            var x = -move.to.y;
            var z = move.to.x;
            var newPos = new Vector3(x, 0.5f, z) * 2 + leftTop.localPosition - offset;
            map.figures[move.from.x, move.from.y].transform.localPosition = newPos;
            map.figures[move.to.x, move.to.y] = map.figures[move.from.x, move.from.y];
            if (moveColor == ChColor.White) {
                moveColor = ChColor.Black;
            } else {
                moveColor = ChColor.White;
            }
        }
    }
}
