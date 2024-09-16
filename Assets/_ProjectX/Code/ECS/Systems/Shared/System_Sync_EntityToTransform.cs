using Unity.Entities;
using Unity.Transforms;

/// <summary>
/// This system is used to sync Transform to Entity, if there are GameObjects which uses ECS for movement
/// </summary>
public partial class System_Sync_EntityToTransform : SystemBase
{
    /* ------------------------------------------ */

    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref Translation translation, ref Rotation rotation, in Data_Transform transform, in Tag_Sync_EntityToTransform tag) =>
        {
            translation.Value = transform.Value.position;
            rotation.Value = transform.Value.rotation;
        }).WithoutBurst().Run();
    }
    
    /* ------------------------------------------ */
}