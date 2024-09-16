using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// This system is responsible for calculating and updating the Node's IsWalkible parametre.
/// To do so, it check every Obstacle object positions and compare it with all of the Nodes.
/// </summary>
[BurstCompile]
public partial class System_Grid_Obstacles : SystemBase
{
    /* ------------------------------------------ */

    public int _gridSize = -1;

    /* ------------------------------------------ */

    /// <summary>
    /// We can put frame counter here to improve the performance, probably we don't need that much controlling anyway.
    /// </summary>
    [BurstCompile]
    protected override void OnUpdate()
    {
        if (_gridSize == -1)
            return;

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob, PlaybackPolicy.SinglePlayback);
        EntityCommandBuffer.ParallelWriter entityCommandBufferParallel = ecb.AsParallelWriter();

        // In this version of the ECS we can't iterate the entities as we wish, so we need these HashMaps 
        // in order to iterate the data in the job.
        var hashMap_entity_instance = new NativeParallelHashMap<Entity, Data_Node>(0, Allocator.TempJob);
        var hashMap_Obstacles = new NativeParallelHashMap<Entity, Data_Obstacle>(0, Allocator.TempJob);
        
        // Getting the components here, nothing special.
        Entities.ForEach((Entity entity, ref Data_Node node) =>
        {
            hashMap_entity_instance.TryAdd(entity, node);
        }).Run();

        Entities.ForEach((Entity entity, in Tag_Obstacle_Static obstacle, in Translation translation, in Data_Scale scale) =>
        {
            hashMap_Obstacles.TryAdd(entity, new Data_Obstacle()
            {
                Position = translation.Value,
                Size = scale.Value
            });
        }).Run();

        Entities.ForEach(
            (Entity entity, in Tag_Obstacle_Dynamic obstacle, in Translation translation, in Data_Scale scale) =>
            {
                hashMap_Obstacles.TryAdd(entity, new Data_Obstacle()
                {
                    Position = translation.Value,
                    Size = scale.Value
                });
            }).Run();

        // Creating a request here in order to run the job
        EntityManager.CreateEntity(typeof(Data_Request_Obstacle_Check));

        // Running the job
        Dependency = new Job_CheckObstacles()
        {
            Ecb = entityCommandBufferParallel,
            HashMap_Entity_Nodes = hashMap_entity_instance,
            HashMap_Entity_Obstacles = hashMap_Obstacles,
            GridSize = _gridSize
        }.ScheduleParallel(Dependency);
        Dependency.Complete();

        // Writing results and cleaning
        ecb.Playback(EntityManager);
        ecb.Dispose();

        hashMap_entity_instance.Dispose();
        hashMap_Obstacles.Dispose();
    }

    /* ------------------------------------------ */

    /// <summary>
    /// Till we call this method the system will not run, because we need the grid size
    /// </summary>
    /// <param name="data"></param>
    public void Setup(SO_Settings_Grid data)
    {
        _gridSize = data.Size;
    }

    /* ------------------------------------------ */

    /// <summary>
    /// This job will update each Grid as isWalkable if there are any obstacle
    ///
    /// TODO: We can implement different jobs for static and dynamic and we can make static obstacle run once to
    /// improve the performance. 
    /// </summary>
    [BurstCompile]
    public partial struct Job_CheckObstacles : IJobEntity
    {
        /* ------------------------------------------ */
        // Variables

        public EntityCommandBuffer.ParallelWriter Ecb;

        /// <summary>
        /// We can't get to iterate components in "ComponentDataFromEntity" so we are using hashmap instead
        /// </summary>
        [NativeDisableContainerSafetyRestriction] [ReadOnly]
        public NativeParallelHashMap<Entity, Data_Node> HashMap_Entity_Nodes;

        [NativeDisableContainerSafetyRestriction] [ReadOnly]
        public NativeParallelHashMap<Entity, Data_Obstacle> HashMap_Entity_Obstacles;

        [ReadOnly] public int GridSize;

        /* ------------------------------------------ */
        // Functions

        [BurstCompile]
        private void Execute(Entity entity, [EntityInQueryIndex] int sortIndex, in Data_Request_Obstacle_Check request)
        {
            // Getting the data from the hashmaps
            var data = HashMap_Entity_Nodes.GetValueArray(Allocator.Temp);
            var obstacleData = HashMap_Entity_Obstacles.GetValueArray(Allocator.Temp);

            float2 obstaclePositionMin = float2.zero;
            float2 obstaclePositionMax = float2.zero;

            // We'll iterate each node with each obstacle and compare the positions and 
            // decide the nodes are isWalkable or not
            for (int x = 0; x < data.Length; x++)
            {
                Data_Node tempData = data[x];
                bool isNodeCollideWithObstacle = false;

                for (int y = 0; y < obstacleData.Length; y++)
                {
                    Data_Obstacle tempObstacleData = obstacleData[y];

                    // We have to round the numbers to correct grid size to get the actual grid in ECS side
                    obstaclePositionMin = new float2(
                        Mathf.RoundToInt(math.ceil(tempObstacleData.Position.x) / GridSize) * GridSize,
                        Mathf.RoundToInt(math.ceil(tempObstacleData.Position.z) / GridSize) * GridSize
                    );

                    // We will check also the size of the obstacle here
                    obstaclePositionMax = new float2((int)(obstaclePositionMin.x + tempObstacleData.Size.x),
                        (int)(obstaclePositionMin.y + tempObstacleData.Size.y));

                    if (data[x].Position.x >= obstaclePositionMin.x && data[x].Position.x < obstaclePositionMax.x)
                        if (data[x].Position.y >= obstaclePositionMin.y && data[x].Position.y < obstaclePositionMax.y)
                            isNodeCollideWithObstacle = true;

                    // We'll deciding here.
                    if (isNodeCollideWithObstacle)
                    {
                        tempData.IsWalkable = false;
                        data[x] = tempData;
                    }
                    else
                    {
                        tempData.IsWalkable = true;
                        data[x] = tempData;
                    }

                    // Setting the data
                    Ecb.SetComponent(sortIndex, tempData.Identity.Entity, tempData);
                }
            }

            // Cleaning
            data.Dispose();
            obstacleData.Dispose();

            // More cleaning :)
            Ecb.DestroyEntity(sortIndex, entity);
        }

        /* ------------------------------------------ */
    }

    /* ------------------------------------------ */
}