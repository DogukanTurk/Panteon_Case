using Cysharp.Threading.Tasks;
using DOT.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UI_Ingame_Prefab_Information : MonoBehaviour
{
    /* ------------------------------------------ */

    public TextMeshProUGUI Txt_Title;

    public Image Img_Icon;

    public Button Btn;

    public SO_Unit SO_Unit;

    public GameObject Building;

    /* ------------------------------------------ */

    public void Setup(SO_Unit data, GameObject Building)
    {
        SO_Unit = data;
        this.Building = Building;

        Txt_Title.text = SO_Unit.Name;
        Img_Icon.sprite = SO_Unit.Spr_Icon_UI;

        Btn.onClick.RemoveAllListeners();
        Btn.onClick.AddListener(FunButtonClicked);
    }

    /* ------------------------------------------ */

    private void FunButtonClicked()
    {
        // This is against the Single responsibility principle but I'm running out of time so, I'll cheat here a bit :)
        // TODO: But I'll put a todo pin here anyway, maybe I can fix it later :)
        Vector3 spawnPos = Building.GetComponent<Transform>().position;
        
        // If there is no space at the bottom, it does use top of the building.
        // If it gets stuck, it can destroy the building on its way, anyway.
        if (spawnPos.z < 1)
            spawnPos.z += Building.GetComponent<Transform>().localScale.z / 2f;
        else
            spawnPos.z -= Building.GetComponent<Transform>().localScale.z / 2f;

        Factory_Unit.Soldier.instance.Create(SO_Unit, spawnPos).Forget();
    }

    /* ------------------------------------------ */
}