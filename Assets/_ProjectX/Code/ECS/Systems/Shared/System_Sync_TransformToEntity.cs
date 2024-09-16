using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

/// <summary>
/// This system is used to sync Transform to Entity, if there are GameObjects which uses ECS for movement
/// </summary>
public partial class System_Sync_TransformToEntity : SystemBase
{
    /* ------------------------------------------ */

    protected override void OnUpdate()
    {
        Entities.ForEach(
            (Entity entity, in Translation translation, in Rotation rotation, in Data_Transform transform) =>
            {
                transform.Value.position = translation.Value;
                transform.Value.rotation = rotation.Value;
            }).WithoutBurst().Run();
    }

    /* ------------------------------------------ */
}