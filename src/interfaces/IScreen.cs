namespace NonsensicalVideoGenerator
{
    /// <summary>
    /// A screen is a graphical container that has update and draw logic.
    /// </summary>
    public interface IScreen : IObject
    {
        /// <summary>
        /// The title of the screen. This is used on the header bar if it is being displayed on the foreground.
        /// </summary>
        public string title { get; }
        public int layer { get; set; }
        public ScreenType screenType { get; set; }
        public int currentPlacement { get; set; }
        public void Show();
        public void Hide();
        public bool Toggle(bool useBool = false, bool toggleTo = false);
    }
    public enum ScreenType
    {
        Drawn,
        Hidden
    }
}
