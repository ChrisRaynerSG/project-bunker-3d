/// <summary>
/// Implemented by any component that wants to receive a per-frame update tick
/// from the single, centralized <see cref="UpdateManager"/> loop.
/// </summary>
public interface IUpdatable
{
    /// <summary>
    /// Called once per frame by <see cref="UpdateManager"/>.
    /// This replaces the individual Unity <c>Update()</c> messages so that the
    /// whole project runs from a single update loop.
    /// </summary>
    void OnUpdate();
}
