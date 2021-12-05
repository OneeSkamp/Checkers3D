using System;
using UnityEngine;
using UnityEngine.UI;
using controller;

namespace ui {
    [Serializable]
    public struct Texts {
        public Text date;
        public Text gameType;
    }

    [Serializable]
    public struct Images {
        public Image moveColor;
        public GameObject watch;
        public Image boardImage;
        public Image checker;
        public Sprite ladyLabel;
    }

    [Serializable]
    public struct Buttons {
        public Button loadBtn;
        public Button deleteBtn;
    }

    public class FillLoadElement : MonoBehaviour {
        public Texts texts;
        public Images images;
        public Buttons buttns;

        private void Awake() {
            if (texts.date == null) {
                Debug.LogError("Date isn't provided");
                this.enabled = false;
                return;
            }

            if (images.moveColor == null) {
                Debug.LogError("Move color isn't provided");
                this.enabled = false;
                return;
            }

            if (buttns.loadBtn == null) {
                Debug.LogError("Load button isn't provided");
                this.enabled = false;
                return;
            }

            if (buttns.deleteBtn == null) {
                Debug.LogError("Delete button isn't provided");
                this.enabled = false;
                return;
            }

            if (images.boardImage == null) {
                Debug.LogError("Image isn't provided");
                this.enabled = false;
                return;
            }

            if (images.checker == null) {
                Debug.LogError("Checker checker isn't provided");
                this.enabled = false;
                return;
            }

            if (images.ladyLabel  == null) {
                Debug.LogError("Lady checker isn't provided");
                this.enabled = false;
                return;
            }
        }

        public void Fill(SaveInfo saveInfo) {
            if (saveInfo.boardInfo.moveColor == ChColor.Black) {
                images.moveColor.color = Color.black;
            }

            texts.date.text = saveInfo.date.ToString("dd.MM.yyyy");
            texts.gameType.text = saveInfo.boardInfo.type.ToString();

            if (images.watch.GetComponent<SetHandsClock>() == null) {
                Debug.LogError("no component FillImageBoard");
            } else {
                images.watch.GetComponent<SetHandsClock>().SetHands(saveInfo.date);
            }

            for (int i = 0; i < saveInfo.boardInfo.board.GetLength(0); i++) {
                for (int j = 0; j < saveInfo.boardInfo.board.GetLength(1); j++) {
                    if (saveInfo.boardInfo.board[i, j].IsNone()) continue;

                    var figure = Instantiate(images.checker, images.boardImage.transform);

                    var ch = saveInfo.boardInfo.board[i, j].Peel();
                    if (ch.type == ChType.Lady) {
                        figure.sprite = images.ladyLabel ;
                    }

                    if (ch.color == ChColor.White) {
                        figure.color = Color.white;
                    }

                    var rect = images.boardImage.GetComponent<RectTransform>().rect;

                    var cell = new Vector2Int(i, j);
                    var x = -rect.height + (rect.height/8 * cell.y + rect.height/16);
                    var y = rect.width - (rect.width/8 * cell.x + rect.width/16);
                    figure.transform.localPosition = new Vector3(x, y, 0f);
                }
            }
        }
    }
}