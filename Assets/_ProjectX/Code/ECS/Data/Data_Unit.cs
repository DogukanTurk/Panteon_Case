using Unity.Entities;
using UnityEngine;

/// <summary>
/// This struct is used for passing unit's data to ECS side.
/// </summary>
public struct Data_Unit : IComponentData
{
    public float Movement_Speed;
}

/// <summary>
/// This struct is used for updating Transform position on the ECS side or otherway around.
/// Mostly used by Movement system both ECS and GO side.
/// </summary>
public class Data_Transform : IComponentData
{
    public Transform Value;
}