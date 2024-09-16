using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// This component is used to create requests
/// </summary>
public struct Data_Request_Path_Finding : IComponentData
{
    /// <summary>
    /// The starting position as Vector2/int2
    /// </summary>
    public int2 StartPosition;
    
    /// <summary>
    /// The target position as Vector2/int2
    /// </summary>
    public int2 TargetPosition;
}

/// <summary>
/// This component is used to hold the available paths as response to the "Data_Request_Path_Finding"
/// </summary>
public struct Data_Buffer_Path : IBufferElementData
{
    /// <summary>
    /// The world position as Vector2/int2 of the available paths/waypoints.
    /// </summary>
    public int2 Path;
}