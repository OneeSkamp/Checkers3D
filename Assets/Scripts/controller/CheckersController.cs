using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using option;
using System.IO;

namespace controller {
    public enum ChColor {
        White,
        Black
    }

    public enum ChType {
        Basic,
        Lady
    }

    public struct Checker {
        public ChColor color;
        public ChType type;

        public static Checker Mk (ChColor color, ChType type) {
            return new Checker { color = color, type = type };
        }
    }

    public struct MoveCell {
        public Vector2Int point;
        public bool isAttack;

        public static MoveCell Mk(Vector2Int point, bool isAttack) {
            return new MoveCell { point = point, isAttack = isAttack };
        }
    }

    public struct Map {
        public GameObject[,] figures;
        public Option<Checker>[,] board;
    }

    public class CheckersController : MonoBehaviour {
        public Resources resources;
        public GameObject loadPanel;
        public GameObject menu;
        public GameObject loadItem;
        public GameObject checkers;
        public Button newGameBtn;
        public Camera screenCamera;

        private GameObject slctObj;
        private GameObject highlightsObj;

        private Map map;
        private ChColor moveClr;
        private Option<Vector2Int> selected;

        private List<Vector2Int> dirs = new List<Vector2Int>();
        private HashSet<Vector2Int> attacked = new HashSet<Vector2Int>();
        private Dictionary<Vector2Int, List<MoveCell>> possibleMoves;

        private void Awake() {
            if (resources == null) {
                Debug.LogError("Resources isn't provided");
                this.enabled = false;
                return;
            }

            if (resources.boardTransform == null) {
                Debug.LogError("Board transform isn't provided");
                this.enabled = false;
                return;
            }

            if (resources.leftTop == null) {
                Debug.LogError("Left top isn't provided");
                this.enabled = false;
                return;
            }

            if (resources.whiteChecker == null) {
                Debug.LogError("White checker isn't provided");
                this.enabled = false;
                return;
            }

            if (resources.blackChecker == null) {
                Debug.LogError("Black checker isn't provided");
                this.enabled = false;
                return;
            }

            if (resources.whiteLady == null) {
                Debug.LogError("White lady isn't provided");
                this.enabled = false;
                return;
            }

            if (resources.blackLady == null) {
                Debug.LogError("Black lady isn't provided");
                this.enabled = false;
                return;
            }

            if (resources.moveHighlight == null) {
                Debug.LogError("Move highlight isn't provided");
                this.enabled = false;
                return;
            }

            var newGamePath = Path.Combine(Application.streamingAssetsPath, "newgame.csv");
            newGameBtn.onClick.AddListener(() => LoadGame(newGamePath));

            dirs.Add(new Vector2Int(1, 1));
            dirs.Add(new Vector2Int(1, -1));
            dirs.Add(new Vector2Int(-1, 1));
            dirs.Add(new Vector2Int(-1, -1));

            map.figures = new GameObject[8, 8];
            map.board = new Option<Checker>[8, 8];
            FillBoard(map.board);
            FillCheckers(map.board);

            highlightsObj = new GameObject();
            highlightsObj.name = "Highlights";
            highlightsObj.transform.parent = resources.boardTransform;
            highlightsObj.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

            slctObj = Instantiate(resources.selectedHighlight);
            slctObj.transform.parent = resources.boardTransform;
            slctObj.SetActive(false);
        }

        private void Update() {


            if (!Input.GetMouseButtonDown(0)) {
                return;
            }

            if (possibleMoves == null) {
                possibleMoves = new Dictionary<Vector2Int, List<MoveCell>>();
                for (int i = 0; i < map.board.GetLength(0); i++) {
                    for (int j = 0; j < map.board.GetLength(1); j++) {
                        var chPos = new Vector2Int(i, j);

                        var chOpt = map.board[chPos.x, chPos.y];
                        if (chOpt.IsNone() || chOpt.Peel().color != moveClr) continue;
                        var ch = chOpt.Peel();

                        var xDir = -1;
                        if (ch.color == ChColor.Black) {
                            xDir = 1;
                        }

                        var moves = new List<MoveCell>();
                        foreach (var dir in dirs) {
                            var chFound = false;
                            var nextPos = chPos + dir;
                            while (IsOnBoard(nextPos, map.board)) {
                                var nextOpt = map.board[nextPos.x, nextPos.y];
                                if (nextOpt.IsNone()) {
                                    var wrongDir = xDir != dir.x && ch.type == ChType.Basic;
                                    if (!chFound && wrongDir) break;
                                    moves.Add(MoveCell.Mk(nextPos, chFound));
                                    if (ch.type == ChType.Basic) break;
                                } else {
                                    var next = nextOpt.Peel();
                                    if (next.color == ch.color || chFound) break;
                                    chFound = true;
                                }
                                nextPos += dir;
                            }
                        }
                        possibleMoves.Add(chPos, moves);
                    }
                }
            }

            if (possibleMoves.Count == 0) {
                menu.SetActive(true);
                this.enabled = false;
            }

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out RaycastHit hit)) {
                return;
            }

            var clicked = ToCell(hit.point, resources.leftTop.position);

            foreach (Transform item in highlightsObj.transform) {
                Destroy(item.gameObject);
            }

            var needAttack = false;
            foreach (var moves in possibleMoves) {
                foreach (var move in moves.Value) {
                    if (move.isAttack) {
                        needAttack = true;
                        break;
                    }
                }
            }

            var checkerOpt = map.board[clicked.x, clicked.y];
            if (checkerOpt.IsSome() && checkerOpt.Peel().color == moveClr) {
                if (!possibleMoves.ContainsKey(clicked)) return;

                selected = Option<Vector2Int>.Some(clicked);
                var moveCells = possibleMoves[selected.Peel()];
                foreach (var moveCell in moveCells) {
                    if (needAttack && !moveCell.isAttack) continue;
                    var highlight = Instantiate(resources.moveHighlight);
                    highlight.transform.parent = highlightsObj.transform;
                    highlight.transform.localPosition = ToCenterCell(moveCell.point);
                }

                slctObj.SetActive(true);
                slctObj.transform.localPosition = ToCenterCell(selected.Peel());
            }


            if (selected.IsSome()) {
                var moveCells = possibleMoves[selected.Peel()];

                var slct = selected.Peel();
                if (map.board[clicked.x, clicked.y].IsNone()) {

                    foreach (var moveCell in moveCells) {
                        if (needAttack && !moveCell.isAttack) continue;

                        if (moveCell.point == clicked) {
                            map.board[clicked.x, clicked.y] = map.board[slct.x, slct.y];
                            map.board[slct.x, slct.y] = Option<Checker>.None();

                            var prefab = map.figures[slct.x, slct.y];
                            map.figures[clicked.x, clicked.y] = prefab;

                            prefab.transform.localPosition = ToCenterCell(clicked);

                            var checker = map.board[clicked.x, clicked.y].Peel();
                            var boardSizeX = map.board.GetLength(0) - 1;
                            if (clicked.x == 0 && moveClr == ChColor.White
                                || clicked.x == boardSizeX && moveClr == ChColor.Black) {
                                var obj = resources.blackLady;
                                if (moveClr == ChColor.White) {
                                    obj = resources.whiteLady;
                                }
                                var lady = Option<Checker>.Some(Checker.Mk(moveClr, ChType.Lady));
                                map.board[clicked.x, clicked.y] = lady;
                                var pos = map.figures[clicked.x, clicked.y].transform.localPosition;
                                Destroy(map.figures[clicked.x, clicked.y]);
                                var ladyObj = Instantiate(obj);
                                ladyObj.transform.parent = resources.boardTransform;
                                ladyObj.transform.localPosition = pos;
                                map.figures[clicked.x, clicked.y] = ladyObj;
                            }

                            var vectorDif = moveCell.point - selected.Peel();
                            var attackDir = new Vector2Int(
                                vectorDif.x/Mathf.Abs(vectorDif.x),
                                vectorDif.y/Mathf.Abs(vectorDif.y)
                            );

                            var attackPos = selected.Peel() + attackDir;
                            while (IsOnBoard(attackPos, map.board)) {
                                if (attackPos == clicked) break;
                                if (map.board[attackPos.x, attackPos.y].IsSome()) {
                                    attacked.Add(attackPos);
                                }

                                attackPos += attackDir;
                            }

                            if (moveCell.isAttack) {
                                var chOpt = map.board[moveCell.point.x, moveCell.point.y];
                                if (chOpt.IsNone() || chOpt.Peel().color != moveClr) continue;
                                var ch = chOpt.Peel();

                                var moves = new List<MoveCell>();
                                foreach (var dir in dirs) {
                                    var chFound = false;
                                    var nextPos = moveCell.point + dir;
                                    while (IsOnBoard(nextPos, map.board)) {
                                        var nextOpt = map.board[nextPos.x, nextPos.y];

                                        if (nextOpt.IsSome()) {
                                            if (chFound) break;
                                            var next = nextOpt.Peel();
                                            if (attacked.Contains(nextPos)) break;
                                            if (next.color == ch.color) break;
                                            chFound = true;
                                        } else if (nextOpt.IsNone() && chFound) {
                                            moves.Add(MoveCell.Mk(nextPos, true));
                                            if (ch.type == ChType.Basic) break;
                                        }

                                        nextPos += dir;
                                    }
                                }

                                foreach (var mc in moves) {
                                    if (needAttack && !moveCell.isAttack) continue;
                                    var highlight = Instantiate(resources.moveHighlight);
                                    highlight.transform.parent = highlightsObj.transform;
                                    highlight.transform.localPosition = ToCenterCell(mc.point);
                                }

                                selected = Option<Vector2Int>.None();
                                foreach (var mv in moves) {
                                    if (mv.isAttack) {
                                        possibleMoves.Clear();
                                        possibleMoves.Add(moveCell.point, moves);
                                        selected = Option<Vector2Int>.Some(moveCell.point);

                                        slctObj.transform.localPosition = ToCenterCell(
                                            selected.Peel()
                                        );
                                        break;
                                    }
                                }
                            } else {
                                selected = Option<Vector2Int>.None();
                            }

                            if (selected.IsNone()) {
                                foreach (var attack in attacked) {
                                    Destroy(map.figures[attack.x, attack.y]);
                                    map.board[attack.x, attack.y] = Option<Checker>.None();
                                }

                                attacked.Clear();
                                if (moveClr == ChColor.White) {
                                    moveClr = ChColor.Black;
                                } else {
                                    moveClr = ChColor.White;
                                }
                                possibleMoves = null;
                            }
                        }
                    }
                }
            }

            if (selected.IsNone()) {
                slctObj.SetActive(false);
            }
        }

        private void FillBoard(Option<Checker>[,] board) {
            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; j < board.GetLength(1); j++) {
                    if (i > 2 && i < 5) continue;

                    if (i % 2 == 0 && j % 2 != 0 || i % 2 != 0 && j % 2 == 0) {
                        var color = ChColor.Black;
                        if (i >= 5) {
                            color = ChColor.White;
                        }
                        map.board[i, j] = Option<Checker>.Some(Checker.Mk(color, ChType.Basic));
                    }
                }
            }
        }

        private void FillCheckers(Option<Checker>[,] board) {
            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; j < board.GetLength(1); j++) {
                    if (board[i, j].IsNone()) {
                        continue;
                    }
                    var checker = board[i, j].Peel();

                    var prefab = resources.blackChecker;
                    if (checker.type == ChType.Lady) {
                        prefab = resources.blackLady;
                    }

                    if (checker.color == ChColor.White) {
                        prefab = resources.whiteChecker;
                        if (checker.type == ChType.Lady) {
                            prefab = resources.whiteLady;
                        }
                    }

                    var checkerObj = Instantiate(prefab);
                    checkerObj.transform.parent = checkers.transform;
                    checkerObj.transform.localPosition = ToCenterCell(new Vector2Int(i, j));

                    map.figures[i, j] = checkerObj;
                }
            }
        }

        public Vector3 ToCenterCell(Vector2Int cell) {
            var offset = resources.offset.localPosition;
            var leftTop = resources.leftTop.localPosition;
            return new Vector3(-cell.y, 0.51f, cell.x) * 2 + leftTop - offset;
        }

        public Vector2Int ToCell(Vector3 globalPoint, Vector3 leftTopPos) {
            var point = resources.boardTransform.InverseTransformPoint(globalPoint);
            var intermediate = (point - new Vector3(-leftTopPos.x, 0f, leftTopPos.z)) / 2;
            return new Vector2Int(Mathf.Abs((int)intermediate.z), Mathf.Abs((int)intermediate.x));
        }

        public void OpenSavePanel() {
            menu.SetActive(false);
            loadPanel.SetActive(false);
            this.enabled = false;
        }

        public void Save() {
            var date = DateTime.Now.ToString("dd.MM.yyyy HH-mm-ss");
            var filePath = Path.Combine(Application.streamingAssetsPath, date);

            File.Create(filePath + ".csv").Dispose();
            var cells = new List<List<string>>();
            for (int i = 0; i < map.board.GetLength(0); i++) {
                var cellsRow = new List<string>();
                for (int j = 0; j < map.board.GetLength(1); j++) {
                    var strCh = "";
                    if (map.board[i, j].IsSome()) {
                        var ch = map.board[i, j].Peel();
                        if (ch.type == ChType.Basic) {
                            if (ch.color == ChColor.White) {
                                strCh = "c";
                            }

                            if (ch.color == ChColor.Black) {
                                strCh = "C";
                            }
                        }

                        if (ch.type == ChType.Lady) {
                            if (ch.color == ChColor.White) {
                                strCh = "l";
                            }

                            if (ch.color == ChColor.Black) {
                                strCh = "L";
                            }
                        }
                    }
                    cellsRow.Add(strCh);
                }

                if (moveClr == ChColor.White) {
                    cellsRow.Add("a");
                } else {
                    cellsRow.Add("A");
                }

                cells.Add(cellsRow);
            }

                string output = CSV.Generate(cells);
            try {
                File.WriteAllText(filePath + ".csv", output);
            } catch (FileNotFoundException e) {
                Debug.LogError(e);
                return;
            }

            RenderTexture currentRT = RenderTexture.active;
            RenderTexture.active = screenCamera.targetTexture;

            screenCamera.Render();
            var width = screenCamera.targetTexture.width;
            var height = screenCamera.targetTexture.height;

            Texture2D texture = new Texture2D(width, height);

            Rect rect = new Rect(0, 0, width, height);
            texture.ReadPixels(rect, 0, 0);
            texture.Apply();
            RenderTexture.active = currentRT;

            var Bytes = texture.EncodeToPNG();
            Destroy(texture);

            try {
                File.WriteAllBytes(filePath + ".png", Bytes);
                Debug.Log(filePath + " game saves");
            } catch (Exception e) {
                Debug.LogError(e);
            }

            menu.SetActive(false);
            this.enabled = true;
        }

        public List<List<string>> FromCSV(string path) {
            if (path == null) {
                Debug.LogError("Path is null");
            }

            try {
                var str = File.ReadAllText(path);
                return CSV.Parse(str).rows;
            } catch (Exception e) {
                Debug.LogError(e);
                return null;
            }
        }

        public void OpenLoadPanel() {
            var pathToFolder = Application.streamingAssetsPath;
            menu.SetActive(false);
            loadPanel.SetActive(true);
            this.enabled = false;

            foreach (Transform item in loadPanel.transform) {
                Destroy(item.gameObject);
            }

            string[] allfiles;
            try {
                allfiles = Directory.GetFiles(pathToFolder, "*.csv");
            } catch (Exception e) {
                allfiles = default;
                Debug.LogError(e);
            }

            foreach (string filename in allfiles) {
                if (filename == Path.Combine(pathToFolder, "newgame.csv")) continue;
                var loaderObj = Instantiate(loadItem);
                loaderObj.transform.parent = loadPanel.transform;
                loaderObj.transform.localScale = new Vector3(1f, 1f, 1f);

                var textObj = loaderObj.transform.GetChild(0);
                var text = textObj.GetComponent<Text>();
                var saveName = filename.Replace(pathToFolder, "");
                saveName = saveName.Replace(".csv", "");
                text.text = saveName;

                var imageObj = loaderObj.transform.GetChild(1);
                var image = imageObj.GetComponent<RawImage>();
                try {
                    byte[] data = File.ReadAllBytes(filename.Replace(".csv", ".png"));
                    Texture2D tex = new Texture2D(2, 2);
                    tex.LoadImage(data);
                    image.texture = tex;
                } catch (Exception e ) {
                    Debug.LogError(e);
                    continue;
                }

                var loadObj = loaderObj.transform.GetChild(2);
                var loadBtn = loadObj.GetComponent<Button>();
                loadBtn.onClick.AddListener(() => LoadGame(filename));

                var deleteObj = loaderObj.transform.GetChild(3);
                var deleteBtn = deleteObj.GetComponent<Button>();
                deleteBtn.onClick.AddListener(() => {
                    Destroy(loaderObj);
                    try {
                        File.Delete(filename);
                    } catch (Exception e) {
                        Debug.LogError(e);
                    }

                    try {
                        File.Delete(filename.Replace(".csv", ".png"));
                    } catch (Exception e) {
                        Debug.LogError(e);
                    }
                });
            }
        }

        public void LoadGame(string path) {
            if (path == null) {
                Debug.LogError("Path is null");
            }

            var csvBoard = FromCSV(path);

            var x = 0;
            foreach (var row in csvBoard) {

                var y = 0;
                foreach (var col in row) {
                    switch (col) {
                        case "a":
                            moveClr = ChColor.White;
                            break;
                        case "A":
                            moveClr = ChColor.Black;
                            break;
                        case "c":
                            map.board[x, y] = Option<Checker>.Some(
                                Checker.Mk(ChColor.White, ChType.Basic)
                            );
                            break;
                        case "C":
                            map.board[x, y] = Option<Checker>.Some(
                                Checker.Mk(ChColor.Black, ChType.Basic)
                            );
                            break;
                        case "L":
                            map.board[x, y] = Option<Checker>.Some(
                                Checker.Mk(ChColor.Black, ChType.Basic)
                            );
                            break;
                        case "l":
                            map.board[x, y] = Option<Checker>.Some(
                                Checker.Mk(ChColor.White, ChType.Basic)
                            );
                            break;
                        case "":
                            map.board[x, y] = Option<Checker>.None();
                            break;
                    }
                    y++;
                }
                x++;
            }

            possibleMoves = null;
            selected = Option<Vector2Int>.None();
            foreach (Transform item in highlightsObj.transform) {
                Destroy(item.gameObject);
            }

            foreach (var obj in map.figures) {
                Destroy(obj);
            }

            FillCheckers(map.board);
            menu.SetActive(false);
            loadPanel.SetActive(false);
            this.enabled = true;
        }

        public void OpenMenu() {
            loadPanel.SetActive(false);
            menu.SetActive(!menu.activeSelf);
            this.enabled = true;
            if (menu.activeSelf) {
                this.enabled = false;
            }
        }

        public bool IsOnBoard<T>(Vector2Int pos, Option<T>[,] board) {
            var size = new Vector2Int(board.GetLength(0), board.GetLength(1));
            if (pos.x < 0 || pos.y < 0 || pos.x >= size.x || pos.y >= size.y) {
                return false;
            }

            return true;
        }
    }
}