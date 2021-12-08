using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using controller;
using UnityEngine.Events;

namespace ui {
    public class LoadPanelController : MonoBehaviour {
        public GameObject content;
        public GameObject loadItem;
        public GameObject thereIsNothingText;
        public CheckersController chController;
        public GameObject pagePanel;
        public Button pageButton;
        public Button nextButton;
        public Button previousButton;
        public Button lastButton;
        public Button firstButton;

        public GameObject loadPanel;
        public int rows;
        public int columns;
        public int pageCountOnPanel;

        private List<Button> pageButtons;

        private void Awake() {
            if (content == null) {
                Debug.LogError("This component requires content");
                this.enabled = false;
                return;
            }

            if (loadItem == null) {
                Debug.LogError("This component requires loadItem");
                this.enabled = false;
                return;
            }

            if (thereIsNothingText == null) {
                Debug.LogError("This component requires thereIsNothingText");
                this.enabled = false;
                return;
            }

            if (chController == null) {
                Debug.LogError("This component requires chController");
                this.enabled = false;
                return;
            }

            if (pagePanel == null) {
                Debug.LogError("This component requires pagePanel");
                this.enabled = false;
                return;
            }

            if (pageButton == null) {
                Debug.LogError("This component requires pageButton");
                this.enabled = false;
                return;
            }
        }

        public void FillPanel(int page) {
            var rect = loadPanel.GetComponent<RectTransform>().rect;
            var width = rect.width;
            var height = rect.height;

            var newWidth = width / columns;
            var newHeight = height / rows;

            var newCellSize = new Vector2(newWidth, newHeight);

            content.GetComponent<GridLayoutGroup>().cellSize = newCellSize;

            foreach (Transform item in content.transform) {
                Destroy(item.gameObject);
            }

            foreach (Transform item in pagePanel.transform) {
                Destroy(item.gameObject);
            }

            var allSaveInfos = chController.GetSaveInfos(Application.persistentDataPath);

            thereIsNothingText.SetActive(false);
            if (allSaveInfos.Count == 0) {
                thereIsNothingText.SetActive(true);
            }

            var saveInfosOnPage = new List<SaveInfo>();

            var count = rows * columns;
            var start = (page - 1) * count;
            for (int i = start; i < start + count; i++) {
                if (i >= allSaveInfos.Count) break;
                saveInfosOnPage.Add(allSaveInfos[i]);
            }

            pageButtons = new List<Button>();
            var pageCount = Math.Ceiling(Convert.ToSingle(allSaveInfos.Count) / count);


            var str = 1;
            if (page != 1 && page >= pageCountOnPanel) {
                str = page - 1;
                if (page <= pageCount && page > pageCount - pageCountOnPanel + 1) {
                    str = (int)pageCount - pageCountOnPanel + 1;
                }
            }

            var first = Instantiate(firstButton, pagePanel.transform);
            var previous = Instantiate(previousButton, pagePanel.transform);

            for (int i = str; i < str + pageCountOnPanel; i++) {
                if (i > pageCount) break;
                int j = i;

                var btn = Instantiate(pageButton, pagePanel.transform);
                btn.image.color = Color.white;
                if (j == page) {
                    btn.image.color = Color.yellow;
                }
                btn.GetComponentInChildren<Text>().text = (j).ToString();
                btn.onClick.AddListener(() => FillPanel(j));
                pageButtons.Add(btn);
            }

            var next = Instantiate(nextButton, pagePanel.transform);
            var last = Instantiate(lastButton, pagePanel.transform);

            if (page != pageCount && pageCount > 0) {
                next.interactable = true;
                last.interactable = true;

                next.onClick.AddListener(() => FillPanel(page + 1));
                last.onClick.AddListener(() => FillPanel((int)pageCount));
            }

            if (page != 1) {
                previous.interactable = true;
                first.interactable = true;

                previous.onClick.AddListener(() => FillPanel(page - 1));
                first.onClick.AddListener(() => FillPanel(1));
            }

            foreach (var saveInfo in saveInfosOnPage) {
                var loaderObj = Instantiate(loadItem, content.transform);
                loaderObj.transform.localScale = new Vector3(1f, 1f, 1f);

                Action loadAction = () => chController.LoadGame(saveInfo.text);

                Action deleteAction = () => {
                    var err = chController.DeleteFile(saveInfo.savePath);
                    if (err == ErrorType.DeleteError) {
                        Debug.LogError(err);
                        return;
                    }

                    Destroy(loaderObj);

                    if (content.transform.childCount == 1 && page != 1) {
                        page -= 1;
                        FillPanel(page);
                    }
                    FillPanel(page);
                };

                if (loaderObj.GetComponent<FillLoadElement>() == null) {
                    Debug.LogError("no component FillLoadElement");
                } else {
                    var loadElem = loaderObj.GetComponent<FillLoadElement>();
                    loadElem.buttons.loadBtn.onClick.AddListener(new UnityAction(loadAction));
                    loadElem.buttons.deleteBtn.onClick.AddListener(new UnityAction(deleteAction));

                    var scale = 2.5f;
                    if (rows > columns) {
                        scale = 1.5f;
                    }
                    var imageSize = Math.Min(newWidth, newHeight) / scale;
                    loadElem.Fill(saveInfo, imageSize);
                }
            }
        }
    }
}
