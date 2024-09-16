using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// It represents a node from a grid. It's used to hold information about the node.
/// </summary>
public struct Data_Node : IComponentData
{
    /// <summary>
    /// We need this reference some places to get Entity reference
    /// </summary>
    public Identity Identity;
    
    /// <summary>
    /// We need this reference while reversing the path.
    /// </summary>
    public Identity Parent;

    /// <summary>
    /// World position of the node
    /// </summary>
    public int2 Position;
    
    /// <summary>
    /// The cost from the starting position to this node
    /// </summary>
    public int GCost;
    
    /// <summary>
    /// The cost for the target position from this node
    /// </summary>
    public int HCost;

    /// <summary>
    /// Is this node is walkable or not. Basically, is it obstacle or got blocked by one
    /// </summary>
    public bool IsWalkable;

    /// <summary>
    /// The size of the grid to calculate neighbor
    /// </summary>
    public int Size;

    public void Setup(int gCost, int hCost)
    {
        GCost = gCost;
        HCost = hCost;
    }
}

/// <summary>
/// This struct is being used in the calculation of obstacles positions
/// to decide is Node walkable or not.
/// </summary>
public struct Data_Obstacle : IComponentData
{
    public float3 Position;
    public int2 Size;
}

/// <summary>
/// This struct used to create a request to handle calculation inside a job
/// </summary>
public struct Data_Request_Obstacle_Check : IComponentData
{
}

/// <summary>
/// This struct is for tagging an entity as static Obstacle
/// </summary>
public struct Tag_Obstacle_Static : IComponentData
{
}

/// <summary>
/// This struct is for tagging an entity as dynamic Obstacle
/// </summary>
public struct Tag_Obstacle_Dynamic : IComponentData
{
}
