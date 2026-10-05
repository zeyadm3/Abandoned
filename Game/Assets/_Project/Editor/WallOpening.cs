namespace Abandoned.EditorTools
{
    /// <summary>A door or window cut into a greybox wall panel, centred along the panel.</summary>
    public readonly struct WallOpening
    {
        public readonly float Width;
        public readonly float Bottom;
        public readonly float Height;

        public WallOpening(float width, float bottom, float height)
        {
            Width = width;
            Bottom = bottom;
            Height = height;
        }
    }
}
