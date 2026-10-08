namespace Abandoned.EditorTools
{
    public readonly struct MeridianRenderSample
    {
        public readonly string Name;
        public readonly int Batches, SetPass, Triangles, Vertices, Flashlights;
        public MeridianRenderSample(string name, int batches, int setPass, int triangles, int vertices, int flashlights)
        { Name = name; Batches = batches; SetPass = setPass; Triangles = triangles; Vertices = vertices; Flashlights = flashlights; }
    }
}
