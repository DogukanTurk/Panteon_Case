using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// This system is responsible of moving entities
/// </summary>
[BurstCompile]
public partial class System_Path_Movement : SystemBase
{
    /* ------------------------------------------ */

    [BurstCompile]
    protected override void OnUpdate()
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob, PlaybackPolicy.SinglePlayback);
        EntityCommandBuffer.ParallelWriter entityCommandBufferParallel = ecb.AsParallelWriter();

        // In this version of the ECS we can't iterate the entities as we wish, so we need these HashMaps 
        // in order to iterate the data in the job.
        var hashMap_int_instance = new NativeParallelHashMap<int2, Data_Node>(0, Allocator.TempJob);
        Entities.ForEach((Entity entity, in Data_Node node) => { hashMap_int_instance.TryAdd(node.Position, node); })
            .Schedule();

        // Scheduling the jobs
        Dependency = new Job_CheckIsThePathIsWalkable()
        {
            Ecb = entityCommandBufferParallel,
            HashMap_Int_Nodes = hashMap_int_instance
        }.ScheduleParallel();
        Dependency.Complete();
        
        Dependency = new Job_Movement()
        {
            DeltaTime = UnityEngine.Time.deltaTime,
        }.ScheduleParallel();
        Dependency.Complete();

        // Cleaning
        ecb.Playback(EntityManager);
        ecb.Dispose();

        hashMap_int_instance.Dispose();
    }

    /* ------------------------------------------ */


    /// <summary>
    /// The job responsible for moving the unit to the next position, if there is any unit using ECS movement system.
    /// </summary>
    [BurstCompile]
    [WithNone(typeof(Tag_ControllingOnECSSide))]
    public partial struct Job_CheckIsThePathIsWalkable : IJobEntity
    {
        /* ------------------------------------------ */
        // Variables

        public EntityCommandBuffer.ParallelWriter Ecb;

        /// <summary>
        /// We can't get to iterate components in "ComponentDataFromEntity" so we are using hashmap instead
        /// </summary>
        [NativeDisableContainerSafetyRestriction] [ReadOnly]
        public NativeParallelHashMap<int2, Data_Node> HashMap_Int_Nodes;

        /* ------------------------------------------ */
        // Functions

        [BurstCompile]
        private void Execute(Entity entity, [EntityInQueryIndex] int sortIndex,
            ref DynamicBuffer<Data_Buffer_Path> dynamicBuffer, in Translation translation)
        {
            // It's very basic, so I'll not add many comments. It's basically checks if the path
            // still valid, player might be built a building on the path. So, if it's case,
            // we'll recalculate the path by creating a new request.
            for (int x = 0; x < dynamicBuffer.Length; x++)
            {
                int2 path = dynamicBuffer[0].Path;

                if (HashMap_Int_Nodes.TryGetValue(path, out Data_Node node))
                {
                    if (node.Identity.Entity != Entity.Null)
                    {
                        if (!node.IsWalkable)
                        {
                            Ecb.AddComponent(sortIndex, entity, new Data_Request_Path_Finding()
                            {
                                StartPosition = new int2((int)translation.Value.x, (int)translation.Value.z),
                                TargetPosition = dynamicBuffer[^1].Path
                            });

                            dynamicBuffer.Clear();
                            return;
                        }
                    }
                }
            }
        }

        /* ------------------------------------------ */
    }

    /// <summary>
    /// The job responsible for moving the unit to the next position, if there is any unit using ECS movement system.
    /// </summary>
    [BurstCompile]
    [WithAll(typeof(Tag_ControllingOnECSSide))]
    public partial struct Job_Movement : IJobEntity
    {
        /* ------------------------------------------ */
        // Variables

        [ReadOnly] public float DeltaTime;

        /* ------------------------------------------ */
        // Functions

        [BurstCompile]
        private void Execute(Entity entity, [EntityInQueryIndex] int sortIndex,
            ref DynamicBuffer<Data_Buffer_Path> dynamicBuffer, ref Translation translation, ref Rotation rotation,
            in Data_Unit data_Unit)
        {
            if (dynamicBuffer.Length > 0)
            {
                int2 path = dynamicBuffer[0].Path;
                float3 targetPosition = new float3(path.x, 0, path.y);

                // Rotation
                quaternion targetRotation =
                    quaternion.LookRotationSafe(targetPosition - translation.Value, new float3(0, 1, 0));
                rotation.Value = math.slerp(rotation.Value, targetRotation, DeltaTime * 4);

                // Moving
                float3 moveDirection = math.normalizesafe(targetPosition - translation.Value);

                // We're moving the object here by its movement speed from Data_Unit
                translation.Value += moveDirection * data_Unit.Movement_Speed * DeltaTime;


                // We use "distancesq" because it's faster than the "distance"
                if (math.distancesq(translation.Value, targetPosition) < .005f)
                    dynamicBuffer.RemoveAt(0);
            }
        }

        /* ------------------------------------------ */
    }

    /* ------------------------------------------ */
}