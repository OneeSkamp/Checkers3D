using System;
using UnityEngine;
using controller;

namespace ui {
    public class FillLoadPanel : MonoBehaviour {
        public CheckersController chController;
        public GameObject content;
        public GameObject loadItem;

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
        }

        public void FillPanel() {
            var saveInfos = chController.GetSaveInfos(Application.persistentDataPath);
            foreach (var saveInfo in saveInfos) {
                var loaderObj = Instantiate(loadItem);
                loaderObj.transform.SetParent(content.transform);
                loaderObj.transform.localScale = new Vector3(1f, 1f, 1f);

                Action loadAction = () => chController.LoadGame(saveInfo.savePath);

                Action deleteAction = () => {
                    Destroy(loaderObj);
                    chController.DeleteFile(saveInfo.savePath);
                };

                if (loaderObj.GetComponent<FillLoadElement>() == null) {
                    Debug.LogError("no component FillLoadElement");
                } else {
                    loaderObj.GetComponent<FillLoadElement>().Fill(
                        saveInfo.date,
                        saveInfo.moveColor,
                        loadAction,
                        deleteAction,
                        saveInfo.board
                    );
                }
            }
        }
    }
}
