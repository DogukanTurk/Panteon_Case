using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// This struct is used to build a structure around the being.
/// For example, we can add layers of identifications such as Teams, Nations, etc.
/// </summary>
public struct Identity : IComponentData
{
    public Entity Entity;
}

/// <summary>
/// This struct is used to pass the scale of the buildings to the ECS side,
/// We need this because the Scale component of Unity.Transforms doesn't have the scale
/// as int2/vector2, we need 2 dimensions and it's have one.
/// </summary>
public struct Data_Scale : IComponentData
{
    public int2 Value;
}