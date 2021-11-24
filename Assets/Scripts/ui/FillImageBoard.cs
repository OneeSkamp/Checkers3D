using UnityEngine;
using controller;
using option;
using UnityEngine.UI;

namespace ui {
    public class FillImageBoard : MonoBehaviour {
        public GameObject leftTop;
        public Image checker;
        public Sprite lady;

        private void Awake() {
            if (leftTop == null) {
                Debug.LogError("LeftTop isn't provided");
                this.enabled = false;
                return;
            }

            if (checker == null) {
                Debug.LogError("Checker checker isn't provided");
                this.enabled = false;
                return;
            }

            if (lady == null) {
                Debug.LogError("Lady checker isn't provided");
                this.enabled = false;
                return;
            }
        }

        public void FillImage(Option<Checker>[,] board) {
            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; j < board.GetLength(1); j++) {
                    if (board[i, j].IsNone()) continue;
                    var ch = board[i, j].Peel();

                    var figure = Instantiate(checker);
                    figure.transform.SetParent(gameObject.transform);
                    if (ch.type == ChType.Lady) {
                        figure.sprite = lady;
                    }

                    if (ch.color == ChColor.White) {
                        figure.color = Color.white;
                    }

                    var cell = new Vector2Int(i, j);
                    var x = leftTop.transform.localPosition.x + (25 * cell.y + 12.5f);
                    var y = leftTop.transform.localPosition.y - (25 * cell.x + 12.5f);
                    figure.transform.localPosition = new Vector3(x, y, 0f);
                }
            }
        }
    }

}
