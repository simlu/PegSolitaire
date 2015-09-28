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
            Textures.Init();
            InitBoard();
            UpdateBoard();
        }

        // Called when the GUI is being updated
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once InconsistentNaming
        void OnGUI() {
            GUI.DrawTexture(new Rect(Screen.width * .01f, Screen.width * .01f, Screen.width * .05f, Screen.width * .05f), _texture, ScaleMode.ScaleToFit);

            if (GUI.Button(new Rect(Screen.width * .07f, Screen.width * .01f, Screen.width * .05f, Screen.width * .05f), "")) {
                _board.Undo();
            }
            GUI.DrawTexture(new Rect(Screen.width * .07f, Screen.width * .01f, Screen.width * .05f, Screen.width * .05f), Textures.Undo, ScaleMode.ScaleToFit);
            
            if (GUI.Button(new Rect(Screen.width* .13f, Screen.width * .01f, Screen.width * .05f, Screen.width * .05f), "")) {
                _board.Redo();
            }
            GUI.DrawTexture(new Rect(Screen.width * .13f, Screen.width * .01f, Screen.width * .05f, Screen.width * .05f), Textures.Redo, ScaleMode.ScaleToFit);
            
            if (GUI.Button(new Rect(Screen.width * .19f, Screen.width * .01f, Screen.width * .05f, Screen.width * .05f), "")) {
                _drawHints = !_drawHints;
            }
            GUI.DrawTexture(new Rect(Screen.width * .19f, Screen.width * .01f, Screen.width * .05f, Screen.width * .05f), Textures.Hint, ScaleMode.ScaleToFit);
            
            if (GUI.Button(new Rect(Screen.width * .25f, Screen.width * .01f, Screen.width * .05f, Screen.width * .05f), "")) {
                _board.SolveNextStep();
            }
            GUI.DrawTexture(new Rect(Screen.width * .25f, Screen.width * .01f, Screen.width * .05f, Screen.width * .05f), Textures.Magic, ScaleMode.ScaleToFit);
            
            if (GUI.Button(new Rect(Screen.width * .31f, Screen.width * .01f, Screen.width * .05f, Screen.width * .05f), "")) {
                _board.Reset();
            }
            GUI.DrawTexture(new Rect(Screen.width * .31f, Screen.width * .01f, Screen.width * .05f, Screen.width * .05f), Textures.Reset, ScaleMode.ScaleToFit);

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