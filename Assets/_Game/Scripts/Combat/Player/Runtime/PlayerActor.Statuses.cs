public sealed partial class PlayerActor
{
    private PlayerStatusRuntime statuses;
    public PlayerStatusRuntime Statuses => statuses ?? (statuses = new PlayerStatusRuntime(this));
    private void LateUpdate() => statuses?.PruneFear();
    private void OnDestroy() => statuses?.Dispose();
}
