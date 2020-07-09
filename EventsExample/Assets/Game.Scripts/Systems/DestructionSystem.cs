using Unity.Entities;

public class DestructionSystem : SystemBase
{
    private EntityQuery _query;

    protected override void OnCreate() => _query = GetEntityQuery(ComponentType.ReadOnly<PendingDestruction>());

    protected override void OnUpdate() => EntityManager.DestroyEntity(_query);

}
