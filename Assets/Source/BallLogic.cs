using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Source {

    /* Handles the "Pegs" in the engine and the user interaction with them */
    internal class BallLogic : MonoBehaviour {

        private Board _board; // board that this peg belongs to
        private int _id; // the id of this peg
        private Camera _camera;  // global camera
        private GameObject _visibleSphere;  // the inner, visible sphere

        // List of all known spheres
        private static readonly Dictionary<int, BallLogic> _balls = new Dictionary<int, BallLogic>();

        // constructor
        public void Init(int id, Board board, Camera globalCamera) {
            _id = id;
            _board = board;
            _camera = globalCamera;
            _balls.Add(_id, this);
            // ensure correct position and scale
            _visibleSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _visibleSphere.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            _visibleSphere.transform.SetParent(transform);
            // the selectable sphere is bigger
            transform.position = new Vector3(_id%7 - 3, 0, _id/7 - 3);
            GetComponent<Renderer>().material = Materials.InvisibleMaterial;
            // update the state
            Update();
        }

        // Ensure that this "Peg" has the right material assigned to it
        public void Update() {
            if (_visible && _down == _id) {
                _visibleSphere.GetComponent<Renderer>().material = Materials.VisibleSelectedBallMaterial;  // clicked peg
                return;
            }
            if (_visible && _over == _id && _down == -1) {
                _visibleSphere.GetComponent<Renderer>().material = Materials.VisibleOverMaterial;  // over peg
                return;
            }
            if (_visible) {
                _visibleSphere.GetComponent<Renderer>().material = Materials.VisibleMaterial;  // visible peg
                return;
            }
            if (!_visible && _down != -1 && _drag == _id && _board.IsValidMove(_down, _drag)) {
                _visibleSphere.GetComponent<Renderer>().material = Materials.InvisibleOverMaterial;  // "drag to" peg
                return;
            }
            _visibleSphere.GetComponent<Renderer>().material = Materials.InvisibleMaterial;  // invisible peg
        }

        // peg visibility
        private bool _visible = true;
        public void SetVisible(bool flag) {
            if (_visible == flag) return;
            _visible = flag;
            Update();
        }

        // mouse over peg
        private static int _over = -1;
        public void OnMouseEnter() {
            _over = _id;
            Update();
        }
        public void OnMouseExit() {
            _over = -1;
            Update();
        }

        // mouse down on peg
        private static int _down = -1;
        public void OnMouseDown() {
            if (!_visible) return;
            _down = _id;
            Update();
        }
        public void OnMouseUp() {
            // handle move logic
            if (_down != -1 && _drag != -1 && _drag != _down) {
                _board.Move(_down, _drag);
            }
            _down = -1;
            _drag = -1;
            Update();
        }

        // mouse drag peg
        private static int _drag = -1;
        public void OnMouseDrag() {
            if (_down != -1) {
                var dragNew = -1;
                Vector3 pos = Input.mousePosition;
                var origin = _camera.WorldToScreenPoint(_balls[_down].transform.position);
                var distX = origin.x - pos.x;
                var distY = origin.y - pos.y;
                var distXAbs = Mathf.Abs(distX);
                var distYAbs = Mathf.Abs(distY);
                var significantDist = Screen.width*0.05;
                if (distXAbs > distYAbs && distXAbs > significantDist) {
                    dragNew = _down - Math.Sign(distX)*2;
                }
                if (distYAbs > distXAbs && distYAbs > significantDist) {
                    dragNew = _down - Math.Sign(distY)*14;
                }
                if (_drag != dragNew) {
                    _drag = dragNew;
                    if (_drag != -1 && _board.IsValidPos(_drag)) {
                        // update the ball that we're dragging "over"
                        _balls[_drag].Update();
                    }
                }
            }
        }
    }
}