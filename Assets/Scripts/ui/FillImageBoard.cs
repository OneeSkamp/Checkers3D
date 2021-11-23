using UnityEngine;
using controller;
using option;
using UnityEngine.UI;

namespace ui {
    public class FillImageBoard : MonoBehaviour {
        public GameObject leftTop;
        public RawImage blackCh;
        public RawImage whiteCh;
        public RawImage blackLady;
        public RawImage whiteLady;

        private void Awake() {
            if (leftTop == null) {
                Debug.LogError("LeftTop isn't provided");
                this.enabled = false;
                return;
            }

            if (blackCh == null) {
                Debug.LogError("Black checker isn't provided");
                this.enabled = false;
                return;
            }

            if (whiteCh == null) {
                Debug.LogError("White checker isn't provided");
                this.enabled = false;
                return;
            }

            if (blackLady == null) {
                Debug.LogError("Black lady isn't provided");
                this.enabled = false;
                return;
            }

            if (whiteLady == null) {
                Debug.LogError("White lady isn't provided");
                this.enabled = false;
                return;
            }
        }

        public void FillImage(Option<Checker>[,] board) {
            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; j < board.GetLength(1); j++) {
                    if (board[i, j].IsNone()) continue;
                    var ch = board[i, j].Peel();
                    RawImage checker = null;
                    if (ch.color == ChColor.White) {
                        checker = whiteCh;
                        if (ch.type == ChType.Lady) {
                            checker = whiteLady;
                        }
                    }

                    if (ch.color == ChColor.Black) {
                        checker = blackCh;
                        if (ch.type == ChType.Lady) {
                            checker = blackLady;
                        }
                    }

                    var img = Instantiate(checker);
                    img.transform.SetParent(gameObject.transform);
                    var cell = new Vector2Int(i, j);
                    var x = leftTop.transform.localPosition.x + (25 * cell.y + 12.5f);
                    var y = leftTop.transform.localPosition.y - (25 * cell.x + 12.5f);
                    img.transform.localPosition = new Vector3(x, y, 0f);
                }
            }
        }
    }

}
