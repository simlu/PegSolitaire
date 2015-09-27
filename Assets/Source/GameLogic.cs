﻿using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Assets {

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
                long[] solution = Solver.Solve(currentBoard);
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
                long[] solution = Solver.Solve(_board.GetCurrentBoard());
                int[] move = Solver.GetNextMove(solution);
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
                int[] move = Solver.GetNextMove(_solution);
                if (move != null) {
                    Vector3 v1 = new Vector3(move[0]%7 - 3, 0, move[0]/7 - 3);
                    Vector3 v2 = new Vector3(move[1]%7 - 3, 0, move[1]/7 - 3);
                    Debug.DrawLine(v1, v2, Color.red);
                }
            }
        
        }
    }
}