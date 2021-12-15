using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using controller;
using UnityEngine.Events;

namespace ui {
    [Serializable]
    public struct PageArrows {
        public Button next;
        public Button previous;
        public Button last;
        public Button first;
    }

    public class LoadPanelController : MonoBehaviour {
        public GameObject content;
        public GameObject loadItem;
        public GameObject thereIsNothingText;
        public CheckersController chController;
        public GameObject pagePanel;
        public PageArrows pageArrows;
        public Button pageButton;
        public List<Button> arrowButtons;

        public int rows;
        public int columns;
        public int pageCountOnPanel;

        private int defoultPageCount;
        private List<Button> pageButtons;

        private void Awake() {
            pageButtons = new List<Button>();
            defoultPageCount = pageCountOnPanel;
        }

        public void FillPanel(int page) {
            if (content == null) {
                Debug.LogError("This component requires content");
                return;
            }

            if (content.GetComponent<GridLayoutGroup>() == null) {
                Debug.LogError("This component requires GridLayoutGroup");
                return;
            }
            content.GetComponent<GridLayoutGroup>().constraintCount = columns;

            foreach (Transform item in content.transform) {
                Destroy(item.gameObject);
            }

            if (chController == null) {
                Debug.LogError("This component requires chController");
                return;
            }
            var allSaveInfos = chController.GetSaveInfos();

            if (thereIsNothingText == null) {
                Debug.LogError("This component requires thereIsNothingText");
                return;
            }
            thereIsNothingText.SetActive(allSaveInfos.Count == 0);
            if (allSaveInfos.Count == 0) return;

            if (pagePanel == null) {
                Debug.LogError("This component requires pagePanel");
                return;
            }

            var maxCount = rows * columns;
            var pageCount = Mathf.CeilToInt(allSaveInfos.Count / (float)maxCount);

            pageCountOnPanel = defoultPageCount;
            if (pageCount <= pageCountOnPanel) {
                pageCountOnPanel = pageCount;
            }

            for (int i = 0; i < arrowButtons.Count; i++) {
                arrowButtons[i].interactable = false;
                arrowButtons[i].onClick.RemoveAllListeners();
                if (page != pageCount && i > 1 || page != 1 && i < 2) {
                    arrowButtons[i].interactable = true;
                    if (page != 1) {
                        arrowButtons[0].onClick.AddListener(() => FillPanel(1));
                        arrowButtons[1].onClick.AddListener(() => FillPanel(page - 1));
                    }

                    if (page != pageCount) {
                        arrowButtons[2].onClick.AddListener(() => FillPanel(page + 1));
                        arrowButtons[3].onClick.AddListener(() => FillPanel((int)pageCount));
                    }
                }
            }

            if (pageButtons.Count != pageCountOnPanel) {
                if (pageButtons.Count < pageCountOnPanel) {
                    if (pageButton == null) {
                        Debug.LogError("This component requires pageButton");
                        return;
                    }

                    for (int i = pageButtons.Count; i < pageCountOnPanel; i++) {
                        var btn = Instantiate(pageButton, pagePanel.transform);
                        pageButtons.Add(btn);
                    }
                } else if (pageButtons.Count > pageCountOnPanel) {
                    for (int i = pageCountOnPanel; i < pageButtons.Count; i++) {
                        Destroy(pageButtons[i].gameObject);
                        pageButtons.RemoveAt(i);
                    }
                }
            }

            var startPage = 1;
            if (page != 1 && page >= pageCountOnPanel) {
                startPage = page - 1;
                if (page <= pageCount && page > pageCount - pageCountOnPanel + 1) {
                    startPage = (int)pageCount - pageCountOnPanel + 1;
                }
            }

            if (pageButton.GetComponent<SetText>() == null) {
                Debug.LogError("No component SetText");
                return;
            }

            var curPage = startPage;
            for (int i = curPage; i <= pageCount && i - curPage < pageButtons.Count; i++) {
                var btn = pageButtons[i - startPage];

                var clr = Color.white;
                if (i == page) {
                    clr = Color.yellow;
                }
                btn.image.color = clr;
                btn.GetComponent<SetText>().Set(i.ToString());
                btn.onClick.RemoveAllListeners();
                var buttonPage = i;
                btn.onClick.AddListener(() => FillPanel(buttonPage));
            }

            if (loadItem == null) {
                Debug.LogError("This component requires Load item");
                return;
            }

            if (loadItem.GetComponent<FillLoadElement>() == null) {
                Debug.LogError("no component FillLoadElement");
            }

            var startSave = (page - 1) * maxCount;

            for (int i = startSave; i < startSave + maxCount && i < allSaveInfos.Count; i++) {
                var loaderObj = Instantiate(loadItem, content.transform);

                var j = i;
                Action loadAction = () => chController.LoadGame(allSaveInfos[j].text);

                Action deleteAction = () => {
                    var err = chController.DeleteFile(allSaveInfos[j].savePath);
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

                var loadElem = loaderObj.GetComponent<FillLoadElement>();
                loadElem.buttons.loadBtn.onClick.AddListener(new UnityAction(loadAction));
                loadElem.buttons.deleteBtn.onClick.AddListener(new UnityAction(deleteAction));

                loadElem.Fill(allSaveInfos[i]);
            }
        }
    }
}
