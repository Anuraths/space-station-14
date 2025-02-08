namespace Content.Server.Silicons.Laws;

[RegisterComponent]
public sealed partial class SiliconLawSyncedComponent : Component
{
    [ViewVariables]
    public EntityUid? Syncer;
}
