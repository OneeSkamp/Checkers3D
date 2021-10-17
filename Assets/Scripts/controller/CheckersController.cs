using UnityEngine;
using option;

namespace controller {
    public enum CheckerColor {
        White,
        Black
    }

    public enum CheckerType {
        Basic,
        Lady
    }

    public struct Checker {
        public CheckerColor color;
        public CheckerType type;

        public static Checker Mk (CheckerColor color, CheckerType type) {
            return new Checker { color = color, type = type };
        }
    }

    public class CheckersController : MonoBehaviour {
        public Transform boardTransform;
        public Transform leftTop;
        public Transform rightBottom;
        public GameObject wChecker;
        public GameObject bChecker;
        public int cellsOnSide;
        public GameObject[,] figures;
        public Option<Checker> [,] board;

        private void Awake() {
            figures = new GameObject[8, 8];
            board = new Option<Checker>[8, 8];
            board[0, 1] = Option<Checker>.Some(Checker.Mk(CheckerColor.Black, CheckerType.Basic));
            board[0, 3] = Option<Checker>.Some(Checker.Mk(CheckerColor.Black, CheckerType.Basic));
            board[0, 5] = Option<Checker>.Some(Checker.Mk(CheckerColor.Black, CheckerType.Basic));
            board[0, 7] = Option<Checker>.Some(Checker.Mk(CheckerColor.Black, CheckerType.Basic));
            board[1, 0] = Option<Checker>.Some(Checker.Mk(CheckerColor.Black, CheckerType.Basic));
            board[1, 2] = Option<Checker>.Some(Checker.Mk(CheckerColor.Black, CheckerType.Basic));
            board[1, 4] = Option<Checker>.Some(Checker.Mk(CheckerColor.Black, CheckerType.Basic));
            board[1, 6] = Option<Checker>.Some(Checker.Mk(CheckerColor.Black, CheckerType.Basic));
            board[2, 1] = Option<Checker>.Some(Checker.Mk(CheckerColor.Black, CheckerType.Basic));
            board[2, 3] = Option<Checker>.Some(Checker.Mk(CheckerColor.Black, CheckerType.Basic));
            board[2, 5] = Option<Checker>.Some(Checker.Mk(CheckerColor.Black, CheckerType.Basic));
            board[2, 7] = Option<Checker>.Some(Checker.Mk(CheckerColor.Black, CheckerType.Basic));
            board[7, 0] = Option<Checker>.Some(Checker.Mk(CheckerColor.White, CheckerType.Basic));
            board[7, 2] = Option<Checker>.Some(Checker.Mk(CheckerColor.White, CheckerType.Basic));
            board[7, 4] = Option<Checker>.Some(Checker.Mk(CheckerColor.White, CheckerType.Basic));
            board[7, 6] = Option<Checker>.Some(Checker.Mk(CheckerColor.White, CheckerType.Basic));
            board[6, 1] = Option<Checker>.Some(Checker.Mk(CheckerColor.White, CheckerType.Basic));
            board[6, 3] = Option<Checker>.Some(Checker.Mk(CheckerColor.White, CheckerType.Basic));
            board[6, 5] = Option<Checker>.Some(Checker.Mk(CheckerColor.White, CheckerType.Basic));
            board[6, 7] = Option<Checker>.Some(Checker.Mk(CheckerColor.White, CheckerType.Basic));
            board[5, 0] = Option<Checker>.Some(Checker.Mk(CheckerColor.White, CheckerType.Basic));
            board[5, 2] = Option<Checker>.Some(Checker.Mk(CheckerColor.White, CheckerType.Basic));
            board[5, 4] = Option<Checker>.Some(Checker.Mk(CheckerColor.White, CheckerType.Basic));
            board[5, 6] = Option<Checker>.Some(Checker.Mk(CheckerColor.White, CheckerType.Basic));
            FillingBoard(board);
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
            var point = (localHit + leftTop.position) / 2;
            var possiblePoint = new Vector2Int(Mathf.Abs((int)point.x), Mathf.Abs ((int)point.z));
        }

        private void FillingBoard(Option<Checker>[,] board) {
            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; j < board.GetLength(1); j++) {
                    if (board[i, j].IsNone()) {
                        continue;
                    }

                    var checker = board[i, j].Peel();
                    if (checker.color == CheckerColor.Black) {
                        figures[i, j] = Instantiate(bChecker);
                    }

                    if (checker.color == CheckerColor.White) {
                        figures[i, j] = Instantiate(wChecker);
                    }

                    figures[i, j].transform.parent = boardTransform;
                    var offset = new Vector3(0.95f, 0, -0.95f);
                    var newPos = new Vector3(-j, 0.5f, i) * 2 + leftTop.localPosition - offset;
                    figures[i, j].transform.localPosition = newPos;
                }
            }
        }
    }
}
