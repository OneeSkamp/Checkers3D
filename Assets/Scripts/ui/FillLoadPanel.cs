using System;
using UnityEngine;
using controller;

namespace ui {
    public class FillLoadPanel : MonoBehaviour {
        public CheckersController chController;
        public GameObject content;
        public GameObject loadItem;
        public GameObject thereIsNothingText;

        private void Awake() {
            if (chController == null) {
                Debug.LogError("chController isn't provided");
                this.enabled = false;
                return;
            }

            if (content == null) {
                Debug.LogError("content isn't provided");
                this.enabled = false;
                return;
            }

            if (loadItem == null) {
                Debug.LogError("loadItem isn't provided");
                this.enabled = false;
                return;
            }

            if (thereIsNothingText == null) {
                Debug.LogError("loadItem isn't thereIsNothingText");
                this.enabled = false;
                return;
            }
        }

        public void FillPanel() {
            var saveInfos = chController.GetSaveInfos(Application.persistentDataPath);
            if (saveInfos.Count == 0) {
                thereIsNothingText.SetActive(true);
            } else {
                thereIsNothingText.SetActive(false);
            }

            foreach (Transform item in content.transform) {
                Destroy(item.gameObject);
            }

            foreach (var saveInfo in saveInfos) {
                var loaderObj = Instantiate(loadItem);
                loaderObj.transform.SetParent(content.transform);
                loaderObj.transform.localScale = new Vector3(1f, 1f, 1f);

                Action loadAction = () => chController.LoadGame(saveInfo.savePath);

                Action deleteAction = () => {
                    Destroy(loaderObj);
                    chController.DeleteFile(saveInfo.savePath);
                    if (content.transform.childCount == 1) {
                        thereIsNothingText.SetActive(true);
                    }
                };

                if (loaderObj.GetComponent<FillLoadElement>() == null) {
                    Debug.LogError("no component FillLoadElement");
                } else {
                    loaderObj.GetComponent<FillLoadElement>().Fill(
                        saveInfo.date,
                        saveInfo.boardInfo.moveColor,
                        saveInfo.boardInfo.type,
                        loadAction,
                        deleteAction,
                        saveInfo.boardInfo.board
                    );
                }
            }
        }
    }
}
