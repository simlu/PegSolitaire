using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

namespace Assets.Source {

    /* The core game logic */
    public class GameLogic : MonoBehaviour {
        private readonly Board _board;

        // draw hints
        private bool _drawHints = true;

        // current text displayed
        private Texture2D _texture = Textures.OnTrack;
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
                _texture = Textures.Solved;
                _solution = new long[0];
            } else {
                _texture = Textures.Thinking;
                long[] solution = Solver.Solve(currentBoard);
                bool solvable = solution.Length != 0;
                lock(this) {
                    // make sure we only update if this board didn't change
                    if (_board.GetCurrentBoard() == currentBoard) {
                        _texture = solvable ? Textures.Danger : Textures.Fail;
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
            _board = new Board();
            // listen to board changes
            _board.AddListener(new Board.EventListener(UpdateBoard));
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
            GUI.Label(new Rect(10, 10, 120, 50), _texture);
            if (GUI.Button(new Rect(52, 10, 32, 32), Textures.Undo)) {
                _board.Undo();
            }
            if (GUI.Button(new Rect(94, 10, 32, 32), Textures.Redo)) {
                _board.Redo();
            }
            if (GUI.Button(new Rect(136, 10, 32, 32), Textures.Hint)) {
                _drawHints = !_drawHints;
            }
            if (GUI.Button(new Rect(178, 10, 32, 32), Textures.Magic)) {
                _board.SolveNextStep();
            }
            if (GUI.Button(new Rect(220, 10, 32, 32), Textures.Reset)) {
                _board.Reset();
            }

            if (Event.current.Equals(Event.KeyboardEvent("z"))) {  // #^z
                _board.Undo();
            }
            if (Event.current.Equals(Event.KeyboardEvent("y"))) {  // #^y
                _board.Redo();
            }
            if (Event.current.Equals(Event.KeyboardEvent("r"))) {  // #^r
                _board.Reset();
            }
            if (Event.current.Equals(Event.KeyboardEvent("h"))) {  // #^h
                _drawHints = !_drawHints;
            }
            if (Event.current.Equals(Event.KeyboardEvent("s"))) {  // #^s
                _board.SolveNextStep();
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void OnPostRender() {
            if (!_drawHints) return;
            if (_solution.Length > 1) {
                int[] move = Solver.GetNextMove(_solution);
                if (move != null) {
                    // Apply the line material
                    Materials.LineMaterial.SetPass(0);
                    // Draw Line
                    GL.PushMatrix();
                    GL.Begin(GL.LINES);
                    GL.Color(new Color(1, 0, 0, 1F));
                    GL.Vertex3(move[0] % 7 - 3, 0, move[0] / 7 - 3);
                    GL.Vertex3(move[1] % 7 - 3, 0, move[1] / 7 - 3);
                    GL.End();
                    GL.PopMatrix();
                }
            }
        }

    }
}