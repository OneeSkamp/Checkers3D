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
        public Buttons buttons;

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

            if (buttons.loadBtn == null) {
                Debug.LogError("Load button isn't provided");
                this.enabled = false;
                return;
            }

            if (buttons.deleteBtn == null) {
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

        public void Fill(SaveInfo saveInfo, float imageSize) {
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

            var imgSize = new Vector2(imageSize / 3, imageSize / 3);
            var boardSize = new Vector2(imageSize, imageSize);

            images.boardImage.GetComponent<RectTransform>().sizeDelta = boardSize;
            images.moveColor.GetComponent<RectTransform>().sizeDelta = imgSize;
            images.watch.GetComponent<RectTransform>().sizeDelta = imgSize;
            buttons.deleteBtn.GetComponent<RectTransform>().sizeDelta = imgSize;
            buttons.loadBtn.GetComponent<RectTransform>().sizeDelta = imgSize;

            var maxY = 1f - 0.125f / 2;
            for (int i = 0; i < saveInfo.boardInfo.board.GetLength(0); i++) {
                var maxX = 0f + 0.125f / 2;
                for (int j = 0; j < saveInfo.boardInfo.board.GetLength(1); j++) {
                    if (saveInfo.boardInfo.board[i, j].IsNone()) {
                        maxX += 0.125f;
                        continue;
                    }

                    var fig = Instantiate(images.checker, images.boardImage.transform);

                    var ch = saveInfo.boardInfo.board[i, j].Peel();
                    if (ch.type == ChType.Lady) {
                        fig.sprite = images.ladyLabel ;
                    }

                    if (ch.color == ChColor.White) {
                        fig.color = Color.white;
                    }

                    var figRect = fig.GetComponent<RectTransform>();
                    var boardRect = images.boardImage.GetComponent<RectTransform>();

                    figRect.anchorMax = new Vector2(maxX, maxY);
                    figRect.anchorMin = new Vector2(maxX, maxY);
                    figRect.sizeDelta = boardRect.sizeDelta / 8;
                    fig.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);

                    maxX += 0.125f;
                }
                maxY -= 0.125f;
            }

        }
    }
}