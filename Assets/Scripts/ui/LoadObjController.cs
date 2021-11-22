using System;
using UnityEngine;
using UnityEngine.UI;
using controller;

namespace ui {
    public class LoadObjController : MonoBehaviour {
        public Text date;
        public Text moveColor;
        public Button loadButton;
        public Button deleteButton;

        public void SetDate(string date) {
            var textTransform = gameObject.transform.GetChild(0);
            var text = textTransform.GetComponent<Text>();
            text.text = date;
        }

        public void SetMoveColor(ChColor color) {
            var moveColorTransform = gameObject.transform.GetChild(5);
            var moveClrText = moveColorTransform.GetComponent<Text>();
            if (color == ChColor.White) {
                moveClrText.text = "WHITE";
            }
        }

        // public void SetLoadAction(Action action) {
        //     var loadTransform = gameObject.transform.GetChild(2);
        //     var loadBtn = loadTransform.GetComponent<Button>();
        //     loadBtn.onClick.AddListener(() => { action };);
        // }
    }
}
