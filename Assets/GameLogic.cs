using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Assets {
    /* Holds constants */
    public static class Constants {
        public const long InitialBoard = 124141717933596L;
        public const long GoalBoard = 16777216L;
        public const long ValidBoardCells = 124141734710812L; // equal "InitialBoard | GoalBoard"
        public static readonly int[] ValidBoardCellsList = {
            2, 3, 4, 9, 10, 11, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
            25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 37, 38, 39, 44, 45, 46
        };

        // holds all 76 moves that are possible
        // the inner array is structures as following:
        // - first entry holds the peg that is added by the move
        // - second entry holds the two pegs that are removed by the move
        // - third entry holds all three involved pegs
        public static readonly long[][] Moves = _InitMoves();

        // helper
        private static void _CreateMoves(int bit1, int bit2, int bit3, List<long[]> moves) {
            moves.Add(new[] { (1L << bit1), (1L << bit2) | (1L << bit3), (1L << bit1) | (1L << bit2) | (1L << bit3) });
            moves.Add(new[] { (1L << bit3), (1L << bit2) | (1L << bit1), (1L << bit1) | (1L << bit2) | (1L << bit3) });
        }

        // initialize all valid moves
        private static long[][] _InitMoves() {
            List<long[]> moves = new List<long[]>();
            int[] starts = { 2, 9, 14, 15, 16, 17, 18, 21, 22, 23, 24, 25, 28, 29, 30, 31, 32, 37, 44 };
            foreach (int start in starts) {
                _CreateMoves(start, start + 1, start + 2, moves);
                _CreateMoves((start % 7) * 7 + start / 7, (start % 7 + 1) * 7 + start / 7,
                             (start % 7 + 2) * 7 + start / 7, moves);
            }
            return moves.ToArray();
        }

    }

    /* Solver utility for a given board */
    internal class BoardSolver {
        // caches all found solutions (and their paths)
        private static readonly Dictionary<long, long[]> SeenSolutions = new Dictionary<long, long[]>(); 
        // Recursive helper method to find a solution
        private static bool _Search(long board, HashSet<long> seenBoards, List<long> solution) {
            foreach (long[] move in Constants.Moves) {
                if ((move[1] & board) != 0L || (move[0] & board) == 0L) continue;
                long newBoard = board ^ move[2];
                if (seenBoards.Contains(newBoard)) continue;
                seenBoards.Add(newBoard);
                if (newBoard != Constants.InitialBoard && !_Search(newBoard, seenBoards, solution)) continue;
                solution.Add(board);
                return true;
            }
            return false;
        }

        // Find a solution for a given board
        public static long[] Solve(long board) {
            if (!SeenSolutions.ContainsKey(board)) {
                List<long> solution = new List<long>();
                // invert for more performant search
                _Search(board ^ Constants.ValidBoardCells, new HashSet<long>(), solution);
                long[] path = new long[0];
                if (solution.Count != 0) {
                    solution.Reverse();
                    solution.Add(Constants.InitialBoard);
                    path = solution.ToArray();
                    // reverse inversion
                    for (var i = 0; i < path.Length; i++) {
                        path[i] = path[i] ^ Constants.ValidBoardCells;
                    }
                }
                // store solution
                if (path.Length == 0) {  // no soltion found
                    SeenSolutions.Add(board, path);
                } else {
                    Debug.Assert(board == path[0]);
                    // store solution and all parts of it
                    for (int i = 0; i < path.Length - 1; i++) {
                        if (!SeenSolutions.ContainsKey(path[i])) {
                            SeenSolutions.Add(path[i], path.Skip(i).ToArray());
                        }
                    }
                }
            }
            return SeenSolutions[board];
        }

        // Extract next move as "from" and "to" for a given solution
        public static int[] GetNextMove(long[] solution) {
            if (solution.Length < 2) return null;
            long move = solution[0] ^ solution[1];
            int[] cells = Constants.ValidBoardCellsList.Where(cell => (move & (1L << cell)) != 0).ToArray();
            int v1 = cells.Min();
            int v2 = cells.Max();
            return (solution[0] & (move & (1L << v1))) == 0L ? new[] {v2, v1} : new[] {v1, v2};
        }

    }

    /* Peg board data structure and interaction logic, including undo and redo */
    internal class Board {
        // observer that is notified when the board state changes
        private readonly GameLogic _observer;
        // constructor
        public Board(GameLogic observer) {
            _observer = observer;
        }

        // ----------------------

        // the current board index
        private int _idx;
        // stack of boards that allow for undo and redo
        private readonly List<long> _boards = new List<long>() { Constants.InitialBoard };

        // check if a peg is set for the current board
        public bool IsSet(int id) {
            return ((1L << id) & _boards[_idx]) != 0L;
        }

        // check if a move is valid, assumes that "from" and "to" are on the board
        public bool IsValidMove(int from, int to) {
            Debug.Assert((Constants.ValidBoardCells & (1L << from)) != 0);
            Debug.Assert((Constants.ValidBoardCells & (1L << to)) != 0);
            // check if the "set" state are correct for this move
            if (!IsSet(from) || IsSet(to) || !IsSet((from + to) / 2)) {
                return false;
            }
            // check if the fields are in a line
            int xFrom = from % 7 - 3;
            int yFrom = from / 7 - 3;
            int xTo = to % 7 - 3;
            int yTo = to / 7 - 3;
            return (xFrom == xTo && Math.Abs(yFrom - yTo) == 2) || (Math.Abs(xFrom - xTo) == 2 && yFrom == yTo);
        }

        // moves a peg from "from" to "to"
        public bool Move(int from, int to) {
            if (IsValidMove(from, to)) {
                long newBoard = _boards[_idx] ^ ((1L << from) | (1L << ((from + to)/2)) | (1L << to));  // apply move
                // remove any previously undone boards from the list
                _boards.RemoveRange(_idx + 1, _boards.Count - (_idx + 1));
                _boards.Add(newBoard);
                _idx++;
                _observer.UpdateBoard();
                return true;
            }
            return false;
        }

        // undo a move if possible
        public void Undo() {
            _idx = _idx == 0 ? 0 : _idx - 1;
            _observer.UpdateBoard();
        }

        // redo a move if possible
        public void Redo() {
            _idx = _idx == _boards.Count - 1 ? _idx : _idx + 1;
            _observer.UpdateBoard();
        }

        // retrieve the current board
        public long GetCurrentBoard() {
            return _boards[_idx];
        }

        // reset the current board
        public void Reset() {
            _idx = 0;
            _boards.RemoveRange(1, _boards.Count - 1);
            _observer.UpdateBoard();
        }
    }

    /* Handles the "Pegs" in the engine and the user interaction with them */
    internal class BallLogic : MonoBehaviour {

        // helper method to convert r,g,b,a into a Material with that color and transparency
        private static Material MakeColor(float r, float g, float b, float a) {
            return new Material(Shader.Find("Transparent/Diffuse")) { color = new Color(r, g, b, a) };
        }

        // the different "Peg" states as Materials
        private static readonly Material InvisibleMaterial = MakeColor(0, 0, 0, 0);
        private readonly static Material InvisibleOverMaterial = MakeColor(0, 0, 0.1f, 0.5f);
        private readonly static Material VisibleMaterial = MakeColor(0, 0.1f, 0, 0.7f);
        private readonly static Material VisibleOverMaterial = MakeColor(0.3f, 0, 0, 0.7f);
        private readonly static Material VisibleSelectedBallMaterial = MakeColor(0, 0, 0.1f, 0.7f);

        // Ensure that this "Peg" has the right material assigned to it
        private void _Update() {
            if (_visible && _selected == _id) {
                gameObject.GetComponent<Renderer>().material = VisibleSelectedBallMaterial;
                return;
            }
            if (_visible && _over == _id && _selected == -1) {
                gameObject.GetComponent<Renderer>().material = VisibleOverMaterial;
                return;
            }
            if (_visible) {
                gameObject.GetComponent<Renderer>().material = VisibleMaterial;
                return;
            }
            if (!_visible && _selected != -1 && _over == _id && _board.IsValidMove(_selected, _over)) {
                gameObject.GetComponent<Renderer>().material = InvisibleOverMaterial;
                return;
            }
            gameObject.GetComponent<Renderer>().material = InvisibleMaterial;
        }

        private Board _board;  // board that this peg belongs to
        private int _id;  // the id of this peg

        // constructor
        public void Init(int id, Board board) {
            _id = id;
            _board = board;
            // ensure correct position and scale
            gameObject.transform.position = new Vector3(_id % 7 - 3, 0, _id / 7 - 3);
            gameObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            // update the state
            _Update();
        }

        // peg visibility
        private bool _visible = true;
        public void SetVisible(bool flag) {
            if (_visible == flag) return;
            _visible = flag;
            _Update();
        }

        // mouse over peg
        private static int _over = -1;
        public void OnMouseEnter() {
            _over = _id;
            _Update();
        }
        public void OnMouseExit() {
            _over = -1;
            _Update();
        }

        // mouse down on peg
        private static int _selected = -1;
        public void OnMouseDown() {
            if (!_visible) return;
            _selected = _id;
            _Update();
        }
        public void OnMouseUp() {
            // handle move logic
            if (_selected != -1 && _over != -1 && _over != _selected) {
                _board.Move(_selected, _over);
            }
            _selected = -1;
            _Update();
        }
    }

    /* The core game logic */
    public class GameLogic : MonoBehaviour {
        private readonly Board _board;

        // current text displayed
        private string _text = "";
        // current solution
        private long[] _solution = new long[0];

        // -------

        // All possible "pegs" on the board, pegs can be shown or hidden
        private readonly Dictionary<int, GameObject> _balls = new Dictionary<int, GameObject>();
        // initialize the peg models
        private void InitBoard() {
            foreach (int id in Constants.ValidBoardCellsList) {
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.AddComponent<BallLogic>().Init(id, _board);
                _balls.Add(id, sphere);
            }
        }

        // Check and display if the current board is solvable
        private void UpdateSolvable() {
            long currentBoard = _board.GetCurrentBoard();
            if (currentBoard == Constants.GoalBoard) {
                _text = "Solved!";
                _solution = new long[0];
            } else {
                _text = "Thinking...";
                long[] solution = BoardSolver.Solve(currentBoard);
                bool solvable = solution.Length != 0;
                lock(this) {
                    // make sure we only update if this board didn't change
                    if (_board.GetCurrentBoard() == currentBoard) {
                        _text = solvable ? "Solvable" : "Not Solvable";
                        _solution = solution;
                    }
                }
            }
        }

        // display the board by changing visiblity for all pegs
        public void UpdateBoard() {
            foreach (int id in Constants.ValidBoardCellsList) {
                _balls[id].GetComponent<BallLogic>().SetVisible(_board.IsSet(id));
            }
            new Thread(UpdateSolvable).Start();
        }

        // constructor
        public GameLogic() {
            _board = new Board(this);
        }

        // Initialize everything
        // ReSharper disable once UnusedMember.Local
        void Start() {
            InitBoard();
            UpdateBoard();
        }

        // Called when the GUI is being updated
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once InconsistentNaming
        void OnGUI() {
            if (Event.current.Equals(Event.KeyboardEvent("z"))) {  // #^z
                _board.Undo();
            }
            if (Event.current.Equals(Event.KeyboardEvent("y"))) {  // #^y
                _board.Redo();
            }
            if (Event.current.Equals(Event.KeyboardEvent("r"))) {  // #^r
                _board.Reset();
            }
            if (Event.current.Equals(Event.KeyboardEvent("s"))) {  // #^s
                // find a solution and apply the next step
                long[] solution = BoardSolver.Solve(_board.GetCurrentBoard());
                int[] move = BoardSolver.GetNextMove(solution);
                if (move != null) {
                    _board.Move(move[0], move[1]);
                }
            }
            GUI.Label(new Rect(40, 40, 120, 50), _text);
        }

        // Update is called once per frame
        // ReSharper disable once UnusedMember.Local
        void Update() {
            if (_solution.Length > 1) {
                int[] move = BoardSolver.GetNextMove(_solution);
                if (move != null) {
                    Vector3 v1 = new Vector3(move[0]%7 - 3, 0, move[0]/7 - 3);
                    Vector3 v2 = new Vector3(move[1]%7 - 3, 0, move[1]/7 - 3);
                    Debug.DrawLine(v1, v2, Color.red);
                }
            }
        
        }
    }
}