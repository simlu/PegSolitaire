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

        // true if we have cheated
        private bool _cheat = false;

        // -------

        // All possible "pegs" on the board, pegs can be shown or hidden
        private readonly Dictionary<int, GameObject> _balls = new Dictionary<int, GameObject>();
        // initialize the peg models
        private void InitBoard() {
            foreach (int id in Constants.ValidBoardCellsList) {
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.AddComponent<BallLogic>().Init(id, _board, gameObject.GetComponent<Camera>());
                _balls.Add(id, sphere);
            }
        }

        // Check and display if the current board is solvable
        private void UpdateSolvable() {
            long currentBoard = _board.GetCurrentBoard();
            _solution = new long[0];
            if (currentBoard == Constants.GoalBoard) {
                _texture = Textures.Solved;
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
            _board.AddListener(new Board.EventListener(() => {
                UpdateBoard();
                // disable hints when board is updated
                _drawHints = false;
            }));
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
            GUI.DrawTexture(new Rect(Screen.height * .01f, Screen.height * .01f, Screen.height * .1f, Screen.height * .1f), _texture, ScaleMode.ScaleToFit);

            if (_cheat) {
                GUI.DrawTexture(new Rect(Screen.height * .1f, Screen.height * .01f, Screen.height * .05f, Screen.height * .05f), Textures.Magic, ScaleMode.ScaleToFit);
            }

            if (GUI.Button(new Rect(Screen.height * .01f, Screen.height * .12f, Screen.height * .1f, Screen.height * .1f), "") || Event.current.Equals(Event.KeyboardEvent("z"))) {
                if (_board.Undo()) {
                    _cheat = true;
                }
            }
            GUI.DrawTexture(new Rect(Screen.height * .01f, Screen.height * .12f, Screen.height * .1f, Screen.height * .1f), Textures.Undo, ScaleMode.ScaleToFit);
            
            if (GUI.Button(new Rect(Screen.height * .01f, Screen.height * .23f, Screen.height * .1f, Screen.height * .1f), "") || Event.current.Equals(Event.KeyboardEvent("y"))) {
                if (_board.Redo()) {
                    _cheat = true;
                }
            }
            GUI.DrawTexture(new Rect(Screen.height * .01f, Screen.height * .23f, Screen.height * .1f, Screen.height * .1f), Textures.Redo, ScaleMode.ScaleToFit);
            
            if (GUI.Button(new Rect(Screen.height * .01f, Screen.height * .34f, Screen.height * .1f, Screen.height * .1f), "") || Event.current.Equals(Event.KeyboardEvent("h"))) {
                _drawHints = !_drawHints;
                if (_drawHints && !_board.IsReset()) {
                    _cheat = true;
                }
            }
            GUI.DrawTexture(new Rect(Screen.height * .01f, Screen.height * .34f, Screen.height * .1f, Screen.height * .1f), Textures.Hint, ScaleMode.ScaleToFit);
            
            if (GUI.Button(new Rect(Screen.height * .01f, Screen.height * .45f, Screen.height * .1f, Screen.height * .1f), "") || Event.current.Equals(Event.KeyboardEvent("s"))) {
                if (_board.SolveNextStep()) {
                    _cheat = true;
                }
            }
            GUI.DrawTexture(new Rect(Screen.height * .01f, Screen.height * .45f, Screen.height * .1f, Screen.height * .1f), Textures.Magic, ScaleMode.ScaleToFit);
            
            if (GUI.Button(new Rect(Screen.height * .01f, Screen.height * .56f, Screen.height * .1f, Screen.height * .1f), "") || Event.current.Equals(Event.KeyboardEvent("r"))) {
                if (_board.Reset()) {
                    _cheat = false;
                }
            }
            GUI.DrawTexture(new Rect(Screen.height * .01f, Screen.height * .56f, Screen.height * .1f, Screen.height * .1f), Textures.Reset, ScaleMode.ScaleToFit);
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