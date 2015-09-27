using UnityEngine;

namespace Assets {

    /* Handles the "Pegs" in the engine and the user interaction with them */

    internal class BallLogic : MonoBehaviour {

        // helper method to convert r,g,b,a into a Material with that color and transparency
        private static Material MakeColor(float r, float g, float b, float a) {
            return new Material(Shader.Find("Transparent/Diffuse")) {color = new Color(r, g, b, a)};
        }

        // the different "Peg" states as Materials
        private static readonly Material InvisibleMaterial = MakeColor(0, 0, 0, 0);
        private static readonly Material InvisibleOverMaterial = MakeColor(0, 0, 0.1f, 0.5f);
        private static readonly Material VisibleMaterial = MakeColor(0, 0.1f, 0, 0.7f);
        private static readonly Material VisibleOverMaterial = MakeColor(0.3f, 0, 0, 0.7f);
        private static readonly Material VisibleSelectedBallMaterial = MakeColor(0, 0, 0.1f, 0.7f);

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