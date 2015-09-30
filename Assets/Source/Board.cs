using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Source {

    /* Peg board data structure and interaction logic, including undo and redo */
    internal class Board {

        // abstract class to define event actions
        public class EventListener {
            public Action OnEvent;
            public EventListener(Action OnEventAction) {
                OnEvent = OnEventAction;
            }
        }
        // holds all attached listeners
        private readonly List<EventListener> _listeners = new List<EventListener>();
        // attach a listener to this instance
        public void AddListener(EventListener listener) {
            _listeners.Add(listener);
        }

        // notify all attached listeners
        private void NotifyListeners() {
            foreach (EventListener listener in _listeners) {
                listener.OnEvent();
            }
        }

        // ----------------------

        // the current board index
        private int _idx;
        // stack of boards that allow for undo and redo
        private readonly List<long> _boards = new List<long>() {Constants.InitialBoard};

        // check if a peg is set for the current board
        public bool IsSet(int id) {
            return ((1L << id) & _boards[_idx]) != 0L;
        }

        // check if a given position is valid (on the board)
        public bool IsValidPos(int pos) {
            return (Constants.ValidBoardCells & (1L << pos)) != 0;
        }

        // check if a move is valid, assumes that "from" and "to" are on the board
        public bool IsValidMove(int from, int to) {
            if (!IsValidPos(from) || !IsValidPos(to)) return false;
            // check if the "set" state are correct for this move
            if (!IsSet(from) || IsSet(to) || !IsSet((from + to)/2)) {
                return false;
            }
            // check if the fields are in a line
            int xFrom = from%7 - 3;
            int yFrom = from/7 - 3;
            int xTo = to%7 - 3;
            int yTo = to/7 - 3;
            return (xFrom == xTo && Math.Abs(yFrom - yTo) == 2) || (Math.Abs(xFrom - xTo) == 2 && yFrom == yTo);
        }

        // moves a peg from "from" to "to"
        public bool Move(int from, int to) {
            bool result = IsValidMove(from, to);
            if (result) {
                long newBoard = _boards[_idx] ^ ((1L << from) | (1L << ((from + to)/2)) | (1L << to)); // apply move
                // remove any previously undone boards from the list
                _boards.RemoveRange(_idx + 1, _boards.Count - (_idx + 1));
                _boards.Add(newBoard);
                _idx++;
                NotifyListeners();
            }
            return result;
        }

        // undo a move if possible
        public bool Undo() {
            bool result = _idx != 0;
            if (result) {
                _idx = _idx - 1;
                NotifyListeners();
            }
            return result;
        }

        // redo a move if possible
        public bool Redo() {
            bool result = _idx != _boards.Count - 1;
            if (result) {
                _idx = _idx + 1;
                NotifyListeners();
            }
            return result;
        }

        // retrieve the current board
        public long GetCurrentBoard() {
            return _boards[_idx];
        }

        // return true if board is reset
        public bool IsReset() {
            return _boards.Count == 1;
        }

        // reset the current board
        public bool Reset() {
            bool result = !IsReset();
            if (result) {
                _idx = 0;
                _boards.RemoveRange(1, _boards.Count - 1);
                NotifyListeners();
            }
            return result;
        }

        // do a solution step
        public bool SolveNextStep() {
            // find a solution and apply the next step
            long[] solution = Solver.Solve(_boards[_idx]);
            bool result = solution.Length != 0;
            if (result) {
                int[] move = Solver.GetNextMove(solution);
                if (move != null) {
                    Move(move[0], move[1]);
                }
            }
            return result;
        }
    }
}