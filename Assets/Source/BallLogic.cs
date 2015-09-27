using UnityEngine;

namespace Assets.Source {

    /* Handles the "Pegs" in the engine and the user interaction with them */
    internal class BallLogic : MonoBehaviour {

        // Ensure that this "Peg" has the right material assigned to it
        private void _Update() {
            if (_visible && _selected == _id) {
                gameObject.GetComponent<Renderer>().material = Materials.VisibleSelectedBallMaterial;
                return;
            }
            if (_visible && _over == _id && _selected == -1) {
                gameObject.GetComponent<Renderer>().material = Materials.VisibleOverMaterial;
                return;
            }
            if (_visible) {
                gameObject.GetComponent<Renderer>().material = Materials.VisibleMaterial;
                return;
            }
            if (!_visible && _selected != -1 && _over == _id && _board.IsValidMove(_selected, _over)) {
                gameObject.GetComponent<Renderer>().material = Materials.InvisibleOverMaterial;
                return;
            }
            gameObject.GetComponent<Renderer>().material = Materials.InvisibleMaterial;
        }

        private Board _board; // board that this peg belongs to
        private int _id; // the id of this peg

        // constructor
        public void Init(int id, Board board) {
            _id = id;
            _board = board;
            // ensure correct position and scale
            gameObject.transform.position = new Vector3(_id%7 - 3, 0, _id/7 - 3);
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
}