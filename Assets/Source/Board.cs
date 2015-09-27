using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Source {

    /* Peg board data structure and interaction logic, including undo and redo */
    internal class Board {

        // abstract class to define event actions
        public class EventListener {
            public Action OnEvent;
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

        // check if a move is valid, assumes that "from" and "to" are on the board
        public bool IsValidMove(int from, int to) {
            Debug.Assert((Constants.ValidBoardCells & (1L << from)) != 0);
            Debug.Assert((Constants.ValidBoardCells & (1L << to)) != 0);
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
            if (IsValidMove(from, to)) {
                long newBoard = _boards[_idx] ^ ((1L << from) | (1L << ((from + to)/2)) | (1L << to)); // apply move
                // remove any previously undone boards from the list
                _boards.RemoveRange(_idx + 1, _boards.Count - (_idx + 1));
                _boards.Add(newBoard);
                _idx++;
                NotifyListeners();
                return true;
            }
            return false;
        }

        // undo a move if possible
        public void Undo() {
            _idx = _idx == 0 ? 0 : _idx - 1;
            NotifyListeners();
        }

        // redo a move if possible
        public void Redo() {
            _idx = _idx == _boards.Count - 1 ? _idx : _idx + 1;
            NotifyListeners();
        }

        // retrieve the current board
        public long GetCurrentBoard() {
            return _boards[_idx];
        }

        // reset the current board
        public void Reset() {
            _idx = 0;
            _boards.RemoveRange(1, _boards.Count - 1);
            NotifyListeners();
        }
    }
}