using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Authoring-only edge sizes. Generated meshes need no runtime deformation or extra draw calls.</summary>
    public sealed class ObjectRemasterConfig : ScriptableObject
    {
        [SerializeField, Min(.0005f)] private float hardEdgeMetres = .0045f;
        [SerializeField, Min(.0005f)] private float woodEdgeMetres = .007f;
        [SerializeField, Min(.0005f)] private float paddedEdgeMetres = .014f;
        [SerializeField, Range(.02f, .25f)] private float maximumThicknessShare = .12f;
        [SerializeField, Min(.001f)] private float smallestPartMetres = .008f;
        [SerializeField, Min(.1f)] private float largestPartMetres = 6f;
        [SerializeField, Min(100)] private int maximumSourceVertices = 40000;

        public float HardEdge => hardEdgeMetres;
        public float WoodEdge => woodEdgeMetres;
        public float PaddedEdge => paddedEdgeMetres;
        public float ThicknessShare => maximumThicknessShare;
        public float SmallestPart => smallestPartMetres;
        public float LargestPart => largestPartMetres;
        public int MaximumSourceVertices => maximumSourceVertices;
    }
}
