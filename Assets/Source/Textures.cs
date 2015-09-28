using System.IO;
using UnityEngine;

namespace Assets.Source {
    public static class Textures {

        public static readonly Texture2D Undo = new Texture2D(1, 1);
        public static readonly Texture2D Redo = new Texture2D(1, 1);
        public static readonly Texture2D Reset = new Texture2D(1, 1);
        public static readonly Texture2D Magic = new Texture2D(1, 1);
        public static readonly Texture2D Hint = new Texture2D(1, 1);
        public static readonly Texture2D OnTrack = new Texture2D(1, 1);
        public static readonly Texture2D Danger = new Texture2D(1, 1);
        public static readonly Texture2D Solved = new Texture2D(1, 1);
        public static readonly Texture2D Fail = new Texture2D(1, 1);
        public static readonly Texture2D Thinking = new Texture2D(1, 1);

        static Textures() {
            Undo.LoadImage(File.ReadAllBytes("Assets/Icons/undo.png"));
            Redo.LoadImage(File.ReadAllBytes("Assets/Icons/redo.png"));
            Reset.LoadImage(File.ReadAllBytes("Assets/Icons/reset.png"));
            Magic.LoadImage(File.ReadAllBytes("Assets/Icons/magic.png"));
            Hint.LoadImage(File.ReadAllBytes("Assets/Icons/hint.png"));
            OnTrack.LoadImage(File.ReadAllBytes("Assets/Icons/ontrack.png"));
            Danger.LoadImage(File.ReadAllBytes("Assets/Icons/danger.png"));
            Solved.LoadImage(File.ReadAllBytes("Assets/Icons/solved.png"));
            Fail.LoadImage(File.ReadAllBytes("Assets/Icons/fail.png"));
            Thinking.LoadImage(File.ReadAllBytes("Assets/Icons/thinking.png"));
        }
    }
}
