using DOT.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "SO_Weapon", menuName = "Project X/Equipment/Weapon", order = 1)]
public class SO_Weapon : ScriptableObject
{
    /* ------------------------------------------ */

    [Header("Primitive Properties")]
    public string Name;

    [Header("Physical Properties")] 
    public int Damage;
    public int Range;
    public float FireRate;

    /* ------------------------------------------ */
}