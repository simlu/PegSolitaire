using UnityEngine;

namespace Assets.Source {
    public static class Textures {

        public static Texture2D Undo;
        public static Texture2D Redo;
        public static Texture2D Reset;
        public static Texture2D Magic;
        public static Texture2D Hint;
        public static Texture2D OnTrack;
        public static Texture2D Danger;
        public static Texture2D Solved;
        public static Texture2D Fail;
        public static Texture2D Thinking;

        public static void Init() {
            Undo = Resources.Load<Texture2D>("undo");
            Redo = Resources.Load<Texture2D>("redo");
            Reset = Resources.Load<Texture2D>("reset");
            Magic = Resources.Load<Texture2D>("magic");
            Hint = Resources.Load<Texture2D>("hint");
            OnTrack = Resources.Load<Texture2D>("ontrack");
            Danger = Resources.Load<Texture2D>("danger");
            Solved = Resources.Load<Texture2D>("solved");
            Fail = Resources.Load<Texture2D>("fail");
            Thinking = Resources.Load<Texture2D>("thinking");
        }
    }
}
