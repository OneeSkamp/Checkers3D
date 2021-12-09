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
        public Sprite board10x10Sprite;
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

        public void Fill(SaveInfo saveInfo) {
            if (images.moveColor == null) {
                Debug.LogError("Move color isn't provided");
                return;
            }
            if (saveInfo.boardInfo.moveColor == ChColor.Black) {
                images.moveColor.color = Color.black;
            }

            if (texts.date == null) {
                Debug.LogError("Date isn't provided");
                return;
            }
            texts.date.text = saveInfo.date.ToString("dd.MM.yyyy");

            if (texts.gameType == null) {
                Debug.LogError("GameType text isn't provided");
                return;
            }
            texts.gameType.text = saveInfo.boardInfo.type.ToString();

            if (images.watch.GetComponent<SetHandsClock>() == null) {
                Debug.LogError("no component FillImageBoard");
            } else {
                images.watch.GetComponent<SetHandsClock>().SetHands(saveInfo.date);
            };

            var countCells = saveInfo.boardInfo.board.GetLength(0);
            if (countCells == 10) {
                if (images.boardImage == null) {
                    Debug.LogError("BoardImage isn't provided");
                    return;
                }
                images.boardImage.sprite = images.board10x10Sprite;
            }

            var maxY = 1f - 1f / countCells / 2;
            for (int i = 0; i < saveInfo.boardInfo.board.GetLength(0); i++) {
                var maxX = 0f + 1f / countCells / 2;
                for (int j = 0; j < saveInfo.boardInfo.board.GetLength(1); j++) {
                    if (saveInfo.boardInfo.board[i, j].IsNone()) {
                        maxX += 1f / countCells;
                        continue;
                    }

                    if (images.checker == null) {
                        Debug.LogError("Checker isn't provided");
                        return;
                    }
                    var fig = Instantiate(images.checker, images.boardImage.transform);

                    var ch = saveInfo.boardInfo.board[i, j].Peel();
                    if (ch.type == ChType.Lady) {
                        if (images.ladyLabel  == null) {
                            Debug.LogError("Lady checker isn't provided");
                            return;
                        }

                        fig.sprite = images.ladyLabel;
                        if (ch.color == ChColor.Black) {
                            fig.color = Color.red;
                        }
                    }

                    if (ch.color == ChColor.White) {
                        fig.color = Color.white;
                    }

                    var figRect = fig.GetComponent<RectTransform>();
                    var boardRect = images.boardImage.GetComponent<RectTransform>();

                    figRect.anchorMax = new Vector2(maxX, maxY);
                    figRect.anchorMin = new Vector2(maxX, maxY);
                    figRect.sizeDelta = boardRect.sizeDelta / countCells / 1.3f;
                    fig.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);

                    maxX += 1f / countCells;
                }
                maxY -= 1f / countCells;
            }

        }
    }
}