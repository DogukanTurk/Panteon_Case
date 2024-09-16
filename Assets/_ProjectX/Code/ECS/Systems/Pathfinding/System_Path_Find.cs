using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// The main system of path finding.
/// This system iterate the path finding requests and do the calculation and returns the results.
/// </summary>
[BurstCompile]
public partial class System_Path_Find : SystemBase
{
    /* ------------------------------------------ */

    [BurstCompile]
    protected override void OnUpdate()
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob, PlaybackPolicy.SinglePlayback);
        EntityCommandBuffer.ParallelWriter entityCommandBufferParallel = ecb.AsParallelWriter();

        // In this version of the ECS we can't iterate the entities as we wish, so we need these HashMaps 
        // in order to iterate the data in the job.
        var hashMap_entity_instance = new NativeParallelHashMap<Entity, Data_Node>(0, Allocator.TempJob);
        Entities.ForEach((Entity entity, in Data_Node node) => { hashMap_entity_instance.TryAdd(entity, node); })
            .Schedule();

        // Scheduling the job
        Dependency = new Job_Request_Path_Finding()
        {
            Ecb = entityCommandBufferParallel,
            HashMap_Entity_Nodes = hashMap_entity_instance
        }.ScheduleParallel(Dependency);
        Dependency.Complete();

        // Cleaning
        ecb.Playback(EntityManager);
        ecb.Dispose();

        hashMap_entity_instance.Dispose();
    }

    /* ------------------------------------------ */

    /// <summary>
    /// This is the main job responsible pathfinding algorithm
    /// </summary>
    [BurstCompile]
    public partial struct Job_Request_Path_Finding : IJobEntity
    {
        /* ------------------------------------------ */
        // Variables

        public EntityCommandBuffer.ParallelWriter Ecb;

        [NativeDisableContainerSafetyRestriction] [ReadOnly]
        public NativeParallelHashMap<Entity, Data_Node> HashMap_Entity_Nodes;

        /* ------------------------------------------ */
        // Functions

        [BurstCompile]
        private void Execute(Entity entity, [EntityInQueryIndex] int sortIndex, in Data_Request_Path_Finding request)
        {
            // Creating lists here, nothing special
            NativeList<Data_Node> openList = new NativeList<Data_Node>(Allocator.Temp);
            NativeList<Data_Node> closedList = new NativeList<Data_Node>(Allocator.Temp);

            NativeList<int2> paths = new NativeList<int2>(Allocator.Temp);

            var _hashMap_intNodes_Instance = new NativeParallelHashMap<int2, Data_Node>(0, Allocator.Temp);
            var _hashmap_entityNodes_Instance = new NativeParallelHashMap<Entity, Data_Node>(0, Allocator.Temp);

            var tempData = HashMap_Entity_Nodes.GetValueArray(Allocator.Temp);
            for (int x = 0; x < tempData.Length; x++)
            {
                Data_Node newData = new Data_Node()
                {
                    Identity = new Identity()
                    {
                        Entity = tempData[x].Identity.Entity
                    },
                    Position = tempData[x].Position,
                    GCost = -61,
                    IsWalkable = tempData[x].IsWalkable,
                    Size = tempData[x].Size,
                };

                _hashmap_entityNodes_Instance.Add(tempData[x].Identity.Entity, newData);
                _hashMap_intNodes_Instance.Add(tempData[x].Position, newData);
            }

            // Getting start and target node
            Data_Node tempStartNode = GetNodeAtPosition(ref _hashMap_intNodes_Instance, request.StartPosition);
            Data_Node tempTargetNode = GetNodeAtPosition(ref _hashMap_intNodes_Instance, request.TargetPosition);

            // If we don't have proper target and start point we'll cancel the job but before returning
            // we have to make sure, we cleaned the lists.
            if (tempStartNode.Identity.Entity == Entity.Null || tempTargetNode.Identity.Entity == Entity.Null)
            {
                openList.Dispose();
                closedList.Dispose();

                paths.Dispose();

                _hashMap_intNodes_Instance.Dispose();
                _hashmap_entityNodes_Instance.Dispose();

                return;
            }

            // Initialize the start node
            SetCosts(ref tempStartNode, 0,
                GetDistanceAsManhattan(request.StartPosition, request.TargetPosition));

            openList.Add(tempStartNode);

            // Calculation starting
            while (openList.Length > 0)
            {
                // Find the closest node from openList
                Data_Node currentNode = GetLowestFCostNode(openList);
                if (currentNode.Identity.Entity == tempTargetNode.Identity.Entity)
                    break;

                // Remote it from the openList
                for (int x = 0; x < openList.Length; x++)
                    if (openList[x].Identity.Entity == currentNode.Identity.Entity)
                        openList.RemoveAt(x);

                closedList.Add(currentNode);

                // Find it's parent (aka back path)
                FindParentNode(ref currentNode, ref _hashMap_intNodes_Instance, ref _hashmap_entityNodes_Instance,
                    sortIndex, ref openList, closedList, request.TargetPosition);
            }

            // Updating the references here, it's the same nodes but updated version.
            tempStartNode = GetNodeAtPosition(ref _hashMap_intNodes_Instance, request.StartPosition);
            tempTargetNode = GetNodeAtPosition(ref _hashMap_intNodes_Instance, request.TargetPosition);

            // If everything is okay and we have a path.
            if (tempTargetNode.Parent.Entity != Entity.Null)
            {
                // Reverse the path so we can go :)
                paths.Clear();
                RetracePath(ref paths, ref _hashMap_intNodes_Instance, ref _hashmap_entityNodes_Instance, sortIndex,
                    ref openList, closedList, request.TargetPosition, tempStartNode, tempTargetNode);

                // Give the path to the Unit here
                for (int x = 0; x < paths.Length; x++)
                {
                    Ecb.AppendToBuffer(sortIndex, entity, new Data_Buffer_Path()
                    {
                        Path = paths[x]
                    });
                }
            }

            // Cleaning
            openList.Dispose();
            closedList.Dispose();

            paths.Dispose();

            _hashMap_intNodes_Instance.Dispose();
            _hashmap_entityNodes_Instance.Dispose();

            Ecb.RemoveComponent<Data_Request_Path_Finding>(sortIndex, entity);
        }

        /* ------------------------------------------ */
        // Helpers

        /// <summary>
        /// It's for updating node's costs.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="gCost"></param>
        /// <param name="hCost"></param>
        private void SetCosts(ref Data_Node node, int gCost, int hCost)
        {
            node.GCost = gCost;
            node.HCost = hCost;
        }

        /// <summary>
        /// Calculating the distance as Manhattan distance calculation algorithm
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="targetPos"></param>
        /// <returns></returns>
        private int GetDistanceAsManhattan(int2 startPos, int2 targetPos)
        {
            return math.abs(startPos.x - targetPos.x) + math.abs(startPos.y - targetPos.y);
        }

        /// <summary>
        /// Find and return the lowest f cost aka closest node
        /// </summary>
        /// <param name="openList"></param>
        /// <returns></returns>
        private Data_Node GetLowestFCostNode(NativeList<Data_Node> openList)
        {
            Data_Node tempLowestFCostNode = openList[0];
            int tempLowestFCost = tempLowestFCostNode.GCost + tempLowestFCostNode.HCost;

            for (int i = 1; i < openList.Length; i++)
            {
                Data_Node tempNode = openList[i];
                int tempNodeFCost = tempNode.GCost + tempNode.HCost;
                if (tempNodeFCost < tempLowestFCost)
                {
                    tempLowestFCost = tempNodeFCost;
                    tempLowestFCostNode = tempNode;
                }
            }

            return tempLowestFCostNode;
        }

        /// <summary>
        /// Reverse the path and find the nodes. I'm not going to fully write comments on this method.
        /// In theory it's simple method, it does recursive checks on back paths on each path, in the end
        /// it reaches the first/initial node, while doing recursive checks it creates a list of nodes.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="_hashMap_int_instance"></param>
        /// <param name="_hashMap_entity_instance"></param>
        /// <param name="sortIndex"></param>
        /// <param name="openList"></param>
        /// <param name="closedList"></param>
        /// <param name="targetPosition"></param>
        /// <param name="startNode"></param>
        /// <param name="endNode"></param>
        private void RetracePath(ref NativeList<int2> path,
            ref NativeParallelHashMap<int2, Data_Node> _hashMap_int_instance,
            ref NativeParallelHashMap<Entity, Data_Node> _hashMap_entity_instance,
            int sortIndex, ref NativeList<Data_Node> openList, NativeList<Data_Node> closedList, int2 targetPosition,
            Data_Node startNode, Data_Node endNode)
        {
            Data_Node currentNode = endNode;

            while (currentNode.Identity.Entity != startNode.Identity.Entity)
            {
                path.Add(currentNode.Position);

                if (currentNode.Parent.Entity == Entity.Null ||
                    currentNode.Parent.Entity == currentNode.Identity.Entity)
                {
                    FindParentNode(ref currentNode, ref _hashMap_int_instance, ref _hashMap_entity_instance, sortIndex,
                        ref openList, closedList, targetPosition);
                }

                if (currentNode.Parent.Entity == currentNode.Identity.Entity)
                {
                    var tempNode = _hashMap_entity_instance[currentNode.Parent.Entity];
                    FindParentNode(ref tempNode, ref _hashMap_int_instance, ref _hashMap_entity_instance, sortIndex,
                        ref openList, closedList, targetPosition);

                    _hashMap_entity_instance[currentNode.Parent.Entity] = tempNode;
                }

                currentNode = _hashMap_entity_instance[currentNode.Parent.Entity];
            }

            if (path.Length > 0)
                ReverseNativeList(ref path);
        }

        /// <summary>
        /// We are reversing the list deconstruction here bc we can't use Linq in Job with Burst.
        /// </summary>
        /// <param name="list"></param>
        private void ReverseNativeList(ref NativeList<int2> list)
        {
            int count = list.Length;
            for (int x = 0; x < count / 2; x++)
                (list[x], list[count - 1 - x]) = (list[count - 1 - x], list[x]);
        }

        // Finds and returns the neighbor nodes
        private NativeList<Data_Node> GetNeighborNodes(ref NativeParallelHashMap<int2, Data_Node> _hashMapInstance,
            Data_Node node)
        {
            NativeList<Data_Node> neighbors = new NativeList<Data_Node>(Allocator.Temp);

            int2 currentPos = node.Position;
            int tempSize = node.Size;

            NativeArray<int2> directions = new NativeArray<int2>(8, Allocator.Temp);
            directions[0] = new int2(0, tempSize); // Yukarı
            directions[1] = new int2(0, -tempSize); // Aşağı
            directions[2] = new int2(tempSize, 0); // Sağ
            directions[3] = new int2(-tempSize, 0); // Sol
            directions[4] = new int2(tempSize, tempSize); // Sağ üst
            directions[5] = new int2(-tempSize, tempSize); // Sol üst
            directions[6] = new int2(tempSize, -tempSize); // Sağ alt
            directions[7] = new int2(-tempSize, -tempSize); // Sol alt

            Data_Node tempDefaultNode = default;
            foreach (var direction in directions)
            {
                int2 neighborPos = currentPos + direction;
                Data_Node neighborNode = GetNodeAtPosition(ref _hashMapInstance, neighborPos);

                if (neighborNode.Identity.Entity != tempDefaultNode.Identity.Entity && neighborNode.IsWalkable)
                    neighbors.Add(neighborNode);
            }

            return neighbors;
        }

        /// <summary>
        /// Finds a node at given position
        /// </summary>
        /// <param name="_hashMapInstance"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        private Data_Node GetNodeAtPosition(ref NativeParallelHashMap<int2, Data_Node> _hashMapInstance, int2 position)
        {
            if (_hashMapInstance.TryGetValue(position, out Data_Node nodeEntity))
                return nodeEntity;

            return default;
        }

        /// <summary>
        /// Calculation total cost of the given position
        /// </summary>
        /// <param name="nodeFirst"></param>
        /// <param name="nodeSecond"></param>
        /// <returns></returns>
        private int CalculateDistanceCost(int2 nodeFirst, int2 nodeSecond)
        {
            int distanceX = math.abs(nodeFirst.x - nodeSecond.x);
            int distanceY = math.abs(nodeFirst.y - nodeSecond.y);
            int distanceDifference = math.abs(distanceX - distanceY);

            return 14 * math.min(distanceX, distanceY) + 10 * distanceDifference;
        }

        /// <summary>
        /// Updates the parent
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parent"></param>
        private void SetParentNode(ref Data_Node node, Data_Node parent)
        {
            node.Parent = parent.Identity;
        }

        /// <summary>
        /// This method is responsible to find the parent/back path of given node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="_hashMap_int_instance"></param>
        /// <param name="_hashMap_entity_instance"></param>
        /// <param name="sortIndex"></param>
        /// <param name="openList"></param>
        /// <param name="closedList"></param>
        /// <param name="targetPosition"></param>
        private void FindParentNode(ref Data_Node node,
            ref NativeParallelHashMap<int2, Data_Node> _hashMap_int_instance,
            ref NativeParallelHashMap<Entity, Data_Node> _hashMap_entity_instance, int sortIndex,
            ref NativeList<Data_Node> openList, NativeList<Data_Node> closedList, int2 targetPosition)
        {
            // We have to define this here in order to use ref in foreach later.
            Data_Node neighbor = default;

            // We are defining a flag here to being able to break/continue the foreach loop later.
            bool shouldBreak = false;

            NativeList<Data_Node> neighbors = GetNeighborNodes(ref _hashMap_int_instance, node);
            for (int y = 0; y < neighbors.Length; y++)
            {
                neighbor = neighbors[y];
                shouldBreak = false;

                // If it's already checked, skip
                for (int x = 0; x < closedList.Length; x++)
                    if (closedList[x].Identity.Entity == neighbor.Identity.Entity)
                        shouldBreak = true;

                // If it's is not walkable, skip
                if (shouldBreak || !neighbor.IsWalkable)
                    continue;

                // Check is it in openList, we need that information a little bit later.
                bool isItInOpenList = false;
                for (int x = 0; x < openList.Length; x++)
                    if (openList[x].Identity.Entity == neighbor.Identity.Entity)
                        isItInOpenList = true;

                // We should do these setups somewhere in this job in order to make the data able to process in multithreading.
                if (node.GCost == -61)
                    node.Setup(int.MaxValue, CalculateDistanceCost(node.Position, targetPosition));

                if (neighbor.GCost == -61)
                    neighbor.Setup(int.MaxValue, CalculateDistanceCost(neighbor.Position, targetPosition));

                // Calculating the g cost here
                int tentativeGCost = node.GCost + CalculateDistanceCost(node.Position, neighbor.Position);
                if (tentativeGCost < neighbor.GCost)
                {
                    // Setting the parent and updating the component data
                    SetCosts(ref neighbor, tentativeGCost,
                        GetDistanceAsManhattan(neighbor.Position, targetPosition));
                    SetParentNode(ref neighbor, node);

                    // Updating the lists
                    neighbors[y] = neighbor;
                    _hashMap_int_instance[neighbor.Position] = neighbor;
                    _hashMap_entity_instance[neighbor.Identity.Entity] = neighbor;

                    // Add it to the openList to check it's neighbors 
                    if (!isItInOpenList)
                        openList.Add(neighbor);
                }
            }

            // Cleaning
            neighbors.Dispose();
        }

        /* ------------------------------------------ */
    }

    /* ------------------------------------------ */
}