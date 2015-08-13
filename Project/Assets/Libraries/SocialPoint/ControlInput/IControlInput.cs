public interface IControlInput
{
    bool Pressed{ get; }

    /// <summary>
    /// Checks it was pressed this update. It's important that this value is true only for one frame
    /// </summary>
    /// <returns><c>true</c>, if just pressed, <c>false</c> otherwise.</returns>
    bool JustPressed{ get; }

    /// <summary>
    /// Checks it was released this update. It's important that this value is true only for one frame
    /// </summary>
    /// <returns><c>true</c>, if just released, <c>false</c> otherwise.</returns>
    bool JustReleased{ get; }
}