using DOT.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "SO_Unit", menuName = "Project X/Creature/Unit", order = 1)]
public class SO_Unit : ScriptableObject
{
    /* ------------------------------------------ */

    [Header("Primitive Properties")]
    public string Name;
    
    [Header("Economy Properties")]
    public int Cost;
    
    [Header("Visual Properties")]
    public Sprite Spr_Icon;
    public Sprite Spr_Icon_UI;

    [Header("Game World Properties")] 
    public AssetReference Prefab;

    [Header("Physical Properties")] 
    public float MovementSpeed;
    public int MaxHealth;

    [Header("Equipment Properties")] 
    public SO_Weapon Weapon;

    [Header("")] public Unit.Enum_Type Type;

    /* ------------------------------------------ */
}