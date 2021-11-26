using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using option;
using controller;

namespace ui {
    public class FillLoadElement : MonoBehaviour {
        public Text date;
        public Image moveColor;
        public Image watch;
        public Image boardImage;
        public Image checker;
        public Button loadBtn;
        public Button deleteBtn;
        public Sprite lady;

        private void Awake() {
            if (date == null) {
                Debug.LogError("Date isn't provided");
                this.enabled = false;
                return;
            }

            if (moveColor == null) {
                Debug.LogError("Move color isn't provided");
                this.enabled = false;
                return;
            }

            if (loadBtn == null) {
                Debug.LogError("Load button isn't provided");
                this.enabled = false;
                return;
            }

            if (deleteBtn == null) {
                Debug.LogError("Delete button isn't provided");
                this.enabled = false;
                return;
            }

            if (boardImage == null) {
                Debug.LogError("Image isn't provided");
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

        public void Fill(
            DateTime saveDate,
            ChColor saveMoveColor,
            Action loadAct,
            Action deleteAct,
            Option<Checker>[,] board
        ) {
            if (saveMoveColor == ChColor.Black) {
                moveColor.color = Color.black;
            }

            date.text = saveDate.ToString("dd.MM.yyyy");

            loadBtn.onClick.AddListener(new UnityAction(loadAct));
            deleteBtn.onClick.AddListener(new UnityAction(deleteAct));

            if (watch.GetComponent<SetHandsClock>() == null) {
                Debug.LogError("no component FillImageBoard");
            } else {
                watch.GetComponent<SetHandsClock>().SetHands(saveDate);
            }

            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; j < board.GetLength(1); j++) {
                    if (board[i, j].IsNone()) continue;

                    var figure = Instantiate(checker);
                    figure.transform.SetParent(boardImage.transform);

                    var ch = board[i, j].Peel();
                    if (ch.type == ChType.Lady) {
                        figure.sprite = lady;
                    }

                    if (ch.color == ChColor.White) {
                        figure.color = Color.white;
                    }

                    var rect = boardImage.GetComponent<RectTransform>().rect;

                    var cell = new Vector2Int(i, j);
                    var x = -rect.height + (25 * cell.y + 12.5f);
                    var y = rect.width - (25 * cell.x + 12.5f);
                    figure.transform.localPosition = new Vector3(x, y, 0f);
                }
            }
        }
    }
}