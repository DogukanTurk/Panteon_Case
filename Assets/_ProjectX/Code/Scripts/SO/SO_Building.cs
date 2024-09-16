using DOT.Utilities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "SO_Building", menuName = "Project X/Environment/Building", order = 1)]
public class SO_Building : ScriptableObject
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
    public int MaxHealth;
    public int2 Dimensions;
    
    /// <summary>
    /// I developed this system be able to have an enum with hundereds of selections and have the flagging ability
    /// It does work same as the flagging, but it's not using the bytes as keys so you are safe with any integer keys.
    ///
    /// It's artist, developer, and performance friendly
    ///
    /// It's simple to use; Mask_Production.Contains(any integer key from the spesific Enum)
    /// </summary>
    [EnumSelector(typeof(Unit.Enum_Type))]
    public EnumSelection Mask_Production;

    /* ------------------------------------------ */
}