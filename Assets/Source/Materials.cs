using UnityEngine;

namespace Assets.Source {
    public static class Materials {
        public static readonly Material LineMaterial = CreateLineMaterial();
        private static Material CreateLineMaterial() {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            var shader = Shader.Find("Hidden/Internal-Colored");
            Material lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
            return lineMaterial;
        }

        // helper method to convert r,g,b,a into a Material with that color and transparency
        private static Material MakeColor(float r, float g, float b, float a) {
            return new Material(Shader.Find("Transparent/Diffuse")) { color = new Color(r, g, b, a) };
        }

        // the different "Peg" states as Materials
        public static readonly Material InvisibleMaterial = MakeColor(0, 0, 0, 0);
        public static readonly Material InvisibleOverMaterial = MakeColor(0, 0, 0.1f, 0.5f);
        public static readonly Material VisibleMaterial = MakeColor(0, 0.1f, 0, 0.7f);
        public static readonly Material VisibleOverMaterial = MakeColor(0.3f, 0, 0, 0.7f);
        public static readonly Material VisibleSelectedBallMaterial = MakeColor(0, 0, 0.1f, 0.7f);
    }
}
