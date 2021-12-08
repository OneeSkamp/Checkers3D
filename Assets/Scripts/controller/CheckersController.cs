using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using option;
using System.IO;

namespace controller {
    public enum ErrorType {
        None,
        DeleteError
    }

    public enum ChColor {
        White,
        Black
    }

    public enum ChType {
        Basic,
        Lady
    }

    public enum GameType {
        Russian,
        English,
        International
    }

    public struct Checker {
        public ChColor color;
        public ChType type;

        public static Checker Mk (ChColor color, ChType type) {
            return new Checker { color = color, type = type };
        }
    }

    public struct MoveCell {
        public Vector2Int pos;
        public bool isAttack;

        public static MoveCell Mk(Vector2Int point, bool isAttack) {
            return new MoveCell { pos = point, isAttack = isAttack };
        }
    }

    public struct BoardInfo {
        public Option<Checker>[,] board;
        public GameType type;
        public ChColor moveColor;
    }

    public struct SaveInfo {
        public DateTime date;
        public string savePath;
        public BoardInfo boardInfo;
        public string text;
    }

    public struct Map {
        public GameObject[,] figures;
        public Option<Checker>[,] board;
    }

    public class CheckersController : MonoBehaviour {
        public Resources resources;
        public GameObject checkers;
        public GameObject checkers10x10;

        public UnityEvent savedSuccessfully;
        public UnityEvent loadGame;
        public UnityEvent gameOver;

        private GameObject selHighlight;
        private GameObject highlightsObj;
        private bool needAttack;

        private Map map;
        private GameType gameType;
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

            if (resources.board8x8Transform == null) {
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

            dirs.Add(new Vector2Int(1, 1));
            dirs.Add(new Vector2Int(1, -1));
            dirs.Add(new Vector2Int(-1, 1));
            dirs.Add(new Vector2Int(-1, -1));

            map.figures = new GameObject[10, 10];
            map.board = new Option<Checker>[10, 10];

            highlightsObj = new GameObject();
            highlightsObj.name = "Highlights";
            highlightsObj.transform.SetParent(resources.board8x8Transform);

            highlightsObj.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

            selHighlight = Instantiate(resources.selectedHighlight, resources.board8x8Transform);
            selHighlight.SetActive(false);
        }

        private void Update() {
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

                            var wrongDir = xDir != dir.x && ch.type == ChType.Basic;

                            while (IsOnBoard(nextPos, map.board)) {
                                var nextOpt = map.board[nextPos.x, nextPos.y];
                                if (nextOpt.IsNone()) {
                                    if (!chFound && wrongDir) break;

                                    moves.Add(MoveCell.Mk(nextPos, chFound));
                                    if (ch.type == ChType.Basic) break;
                                    if (gameType == GameType.English && ch.type == ChType.Lady) {
                                        break;
                                    }
                                } else {
                                    if (gameType == GameType.English && wrongDir) break;
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

                if (possibleMoves.Count == 0) {
                    gameOver?.Invoke();
                    this.enabled = false;
                }
            }

            if (selected.IsSome() && highlightsObj.transform.childCount == 0) {
                var moveCells = possibleMoves[selected.Peel()];

                highlightsObj.transform.SetParent(resources.board8x8Transform);
                if (gameType == GameType.International) {
                    highlightsObj.transform.SetParent(resources.board10x10Transform);
                }

                foreach (var moveCell in moveCells) {
                    if (needAttack && !moveCell.isAttack) continue;
                    var highlight = Instantiate(resources.moveHighlight, highlightsObj.transform);
                    highlight.transform.localPosition = ToCenterCell(moveCell.pos);
                }
            }

            if (!Input.GetMouseButtonDown(0)) {
                return;
            }

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out RaycastHit hit)) {
                return;
            }

            var clicked = ToCell(hit.point, resources.leftTop.position);
            if (gameType == GameType.International) {
                clicked = ToCell(hit.point, resources.leftTop10x10.position);
            }

            foreach (Transform item in highlightsObj.transform) {
                Destroy(item.gameObject);
            }

            needAttack = false;
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

                selHighlight.transform.SetParent(resources.board8x8Transform);
                if (gameType == GameType.International) {
                    selHighlight.transform.SetParent(checkers10x10.transform);
                }

                selHighlight.SetActive(true);
                selHighlight.transform.localPosition = ToCenterCell(selected.Peel());
            }

            if (selected.IsSome()) {
                var moveCells = possibleMoves[selected.Peel()];
                var sel = selected.Peel();

                if (map.board[clicked.x, clicked.y].IsSome()) {
                    return;
                }

                var moveCellOpt = Option<MoveCell>.None();
                foreach (var cell in moveCells) {
                    if (needAttack && !cell.isAttack) continue;
                    if (cell.pos == clicked) {
                        moveCellOpt = Option<MoveCell>.Some(cell);
                        break;
                    }
                }

                if (moveCellOpt.IsNone()) return;
                var moveCell = moveCellOpt.Peel();

                var edge = 0;
                if (moveClr == ChColor.Black) {
                    edge = map.board.GetLength(0) - 1;
                }

                var promote = clicked.x == edge;

                if (moveCell.pos == clicked) {
                    map.board[clicked.x, clicked.y] = map.board[sel.x, sel.y];
                    map.board[sel.x, sel.y] = Option<Checker>.None();

                    var chObj = map.figures[sel.x, sel.y];
                    map.figures[clicked.x, clicked.y] = chObj;

                    chObj.transform.localPosition = ToCenterCell(clicked);

                    if (clicked.x == edge) {
                        var obj = resources.blackLady;
                        if (moveClr == ChColor.White) {
                            obj = resources.whiteLady;
                        }

                        var lady = Option<Checker>.Some(Checker.Mk(moveClr, ChType.Lady));
                        map.board[clicked.x, clicked.y] = lady;

                        Destroy(map.figures[clicked.x, clicked.y]);
                        var ladyObj = Instantiate(obj, checkers.transform);
                        if (gameType == GameType.International) {
                            ladyObj.transform.SetParent(checkers10x10.transform);
                        }

                        var pos = map.figures[clicked.x, clicked.y].transform.localPosition;
                        ladyObj.transform.localPosition = pos;
                        map.figures[clicked.x, clicked.y] = ladyObj;
                    }

                    var dif = moveCell.pos - selected.Peel();
                    var attackDir = new Vector2Int(
                        dif.x / Mathf.Abs(dif.x),
                        dif.y / Mathf.Abs(dif.y)
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
                        var chOpt = map.board[moveCell.pos.x, moveCell.pos.y];
                        if (chOpt.IsNone() || chOpt.Peel().color != moveClr) return;
                        var ch = chOpt.Peel();

                        var xDir = -1;
                        if (ch.color == ChColor.Black) {
                            xDir = 1;
                        }
                        var moves = new List<MoveCell>();
                        foreach (var dir in dirs) {
                            var chFound = false;
                            var nextPos = moveCell.pos + dir;
                            while (IsOnBoard(nextPos, map.board)) {
                                var nextOpt = map.board[nextPos.x, nextPos.y];

                                var wrongDir = xDir != dir.x && ch.type == ChType.Basic;

                                if (nextOpt.IsSome()) {
                                    if (wrongDir && gameType == GameType.English) break;
                                    if (chFound) break;
                                    var next = nextOpt.Peel();
                                    if (attacked.Contains(nextPos)) break;
                                    if (next.color == ch.color) break;
                                    chFound = true;
                                } else if (nextOpt.IsNone() && chFound) {
                                    moves.Add(MoveCell.Mk(nextPos, true));
                                    if (ch.type == ChType.Basic) break;
                                    if (gameType == GameType.English) break;
                                } else if (nextOpt.IsNone() && ch.type == ChType.Basic){
                                    break;
                                } else if (gameType == GameType.English) {
                                    break;
                                }

                                nextPos += dir;
                            }
                        }

                        selected = Option<Vector2Int>.None();
                        foreach (var mv in moves) {
                            if (mv.isAttack) {
                                possibleMoves.Clear();
                                possibleMoves.Add(moveCell.pos, moves);
                                selected = Option<Vector2Int>.Some(moveCell.pos);

                                var centerCell = ToCenterCell(selected.Peel());
                                selHighlight.transform.localPosition = centerCell;
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

            if (selected.IsNone()) {
                selHighlight.SetActive(false);
            }
        }

        public void FillCheckers(Option<Checker>[,] board) {
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
                    checkerObj.transform.SetParent(checkers.transform);
                    if (gameType == GameType.International) {
                        checkerObj.transform.SetParent(checkers10x10.transform);
                    }
                    checkerObj.transform.localPosition = ToCenterCell(new Vector2Int(i, j));

                    map.figures[i, j] = checkerObj;
                }
            }
        }

        public Vector3 ToCenterCell(Vector2Int cell) {
            var offset = resources.offset.localPosition;
            var leftTop = resources.leftTop.localPosition;
            if (gameType == GameType.International) {
                leftTop = resources.leftTop10x10.localPosition;
            }
            return new Vector3(-cell.y, 0.56f, cell.x) * 2 + leftTop - offset;
        }

        public Vector2Int ToCell(Vector3 globalPoint, Vector3 leftTopPos) {
            var point = resources.board8x8Transform.InverseTransformPoint(globalPoint);
            var intermediate = (point - new Vector3(-leftTopPos.x, 0f, leftTopPos.z)) / 2;
            return new Vector2Int(Mathf.Abs((int)intermediate.z), Mathf.Abs((int)intermediate.x));
        }

        public string GetTextFromCSV(string path) {
            try {
                return File.ReadAllText(path);
            } catch (Exception e) {
                Debug.LogError(e);
                return null;
            }
        }

        public List<List<string>> GetRowsFromCSV(string text) {
            if (text == null) {
                Debug.LogError("Path is null");
            }

            try {
                return CSV.Parse(text).rows;
            } catch (Exception e) {
                Debug.LogError(e);
                return null;
            }
        }

        public BoardInfo BoardInfoFromCSV(List<List<string>> rows) {

            var boardInfo = new BoardInfo();

            var x = 0;
            foreach (var row in rows) {

                var y = 0;
                foreach (var cell in row) {
                    if (x == 0 && y == 0) {
                        switch (cell) {
                            case "0":
                                boardInfo.type = GameType.Russian;
                                boardInfo.board = new Option<Checker>[8, 8];
                                break;
                            case "1":
                                boardInfo.type = GameType.English;
                                boardInfo.board = new Option<Checker>[8, 8];
                                break;
                            case "2":
                                boardInfo.type = GameType.International;
                                boardInfo.board = new Option<Checker>[10, 10];
                                break;
                        }
                        break;
                    }

                    if (x == 1 && y == 0) {
                        if (cell == "1") {
                            boardInfo.moveColor = ChColor.Black;
                        } else {
                            boardInfo.moveColor = ChColor.White;
                        }
                        x++;
                        break;
                    }

                    if (cell == "") {
                        boardInfo.board[x - 3, y] = Option<Checker>.None();
                        y++;
                        continue;
                    }

                    var value = Int32.TryParse(cell, out int res);
                    if (!value) {
                        Debug.LogError("Parse error");
                        break;
                    }

                    var color = ChColor.Black;
                    if (res % 2 == 0) {
                        color = ChColor.White;
                    }

                    var chType = ChType.Basic;
                    if (res > 1) {
                        chType = ChType.Lady;
                    }

                    boardInfo.board[x - 3, y] = Option<Checker>.Some(Checker.Mk(color, chType));
                    y++;
                }
                x++;
            }

            possibleMoves = null;
            selected = Option<Vector2Int>.None();
            foreach (Transform item in highlightsObj.transform) {
                Destroy(item.gameObject);
            }

            return boardInfo;
        }

        public List<SaveInfo> GetSaveInfos(string pathToFolder) {
            string[] allfiles;
            try {
                allfiles = Directory.GetFiles(pathToFolder, "*.save");
            } catch (Exception e) {
                allfiles = default;
                Debug.LogError(e);
            }

            var saveInfos = new List<SaveInfo>();
            foreach (string filename in allfiles) {
                var saveInfo = new SaveInfo();
                var text = GetTextFromCSV(filename);
                var rows = GetRowsFromCSV(text);
                var boardInfo = BoardInfoFromCSV(rows);
                saveInfo.boardInfo.board = boardInfo.board;
                saveInfo.boardInfo.moveColor = boardInfo.moveColor;
                saveInfo.boardInfo.type = boardInfo.type;

                var date = new DateTime();
                try {
                    date = File.GetCreationTime(filename);
                } catch (Exception e) {
                    Debug.LogError(e);
                }

                saveInfo.date = date;
                saveInfo.savePath = filename;
                saveInfo.text = GetTextFromCSV(filename);
                saveInfos.Add(saveInfo);
            }

            try {
                saveInfos.Sort((x, y) => DateTime.Compare(y.date, x.date));
            } catch (Exception e) {
                Debug.LogError(e);
            }

            return saveInfos;
        }

        public void SaveGame() {
            var name = $@"{Guid.NewGuid()}.save";
            var filePath = Path.Combine(Application.persistentDataPath, name);

            var cells = new List<List<string>>();

            switch (gameType) {
                case GameType.Russian:
                    cells.Add(new List<string> {"0"});
                    break;
                case GameType.English:
                    cells.Add(new List<string> {"1"});
                    break;
                case GameType.International:
                    cells.Add(new List<string> {"2"});
                    break;
            }

            if (moveClr == ChColor.White) {
                cells.Add(new List<string> {"0"});
            } else {
                cells.Add(new List<string> {"1"});
            }

            for (int i = 0; i < map.board.GetLength(0); i++) {
                var cellsRow = new List<string>();
                for (int j = 0; j < map.board.GetLength(1); j++) {
                    var strCh = "";
                    if (map.board[i, j].IsSome()) {
                        var ch = map.board[i, j].Peel();

                        if (ch.type == ChType.Basic) {
                            strCh = "0";
                            if (ch.color == ChColor.Black) {
                                strCh = "1";
                            }
                        }

                        if (ch.type == ChType.Lady) {
                            strCh = "2";
                            if (ch.color == ChColor.Black) {
                                strCh = "3";
                            }
                        }
                    }
                    cellsRow.Add(strCh);
                }
                cells.Add(cellsRow);
            }

            string output = CSV.Generate(cells);
            try {
                File.WriteAllText(filePath, output);
                savedSuccessfully?.Invoke();
            } catch (Exception e) {
                Debug.LogError(e);
                return;
            }
        }

        public void LoadGame(string text) {
            resources.board8x8Transform.gameObject.SetActive(false);
            resources.board10x10Transform.gameObject.SetActive(false);

            selHighlight.SetActive(false);
            var rows = GetRowsFromCSV(text);
            var boardInfo = BoardInfoFromCSV(rows);
            map.board = boardInfo.board;
            moveClr = boardInfo.moveColor;
            gameType = boardInfo.type;

            if (boardInfo.type == GameType.International) {
                resources.board10x10Transform.gameObject.SetActive(true);
            } else {
                resources.board8x8Transform.gameObject.SetActive(true);
            }

            foreach (var obj in map.figures) {
                Destroy(obj);
            }
            loadGame?.Invoke();
            FillCheckers(map.board);
        }

        public ErrorType DeleteFile(string path) {
            try {
                File.Delete(path);
                return ErrorType.None;
            } catch (Exception e) {
                Debug.LogError(e);
                return ErrorType.DeleteError;
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