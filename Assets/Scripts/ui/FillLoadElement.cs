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
        public Button loadBtn;
        public Button deleteBtn;
        public RawImage image;

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

            if (image == null) {
                Debug.LogError("Image isn't provided");
                this.enabled = false;
                return;
            }
        }

        public void Fill(
            string saveDate,
            ChColor saveMoveColor,
            Action loadAct,
            Action deleteAct,
            Option<Checker>[,] board
        ) {
            date.text = saveDate;

            if (saveMoveColor == ChColor.Black) {
                moveColor.color = Color.black;
            }

            loadBtn.onClick.AddListener(new UnityAction(loadAct));
            deleteBtn.onClick.AddListener(new UnityAction(deleteAct));

            if (image.GetComponent<FillImageBoard>() == null) {
                Debug.LogError("no component FillImageBoard");
            } else {
                image.GetComponent<FillImageBoard>().FillImage(board);
            }
        }
    }
}