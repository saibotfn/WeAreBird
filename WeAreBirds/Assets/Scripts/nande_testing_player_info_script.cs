using UnityEngine;

public class nande_testing_player_info_script : MonoBehaviour
{
    private nande_testing_inventory_management_script inventory;

    void Update()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<nande_testing_inventory_management_script>();
    }

    void OnGUI()
    {
        if (inventory == null)
            return;

        GUI.Box(new Rect(20f, 20f, 260f, 145f), "PLAYER INFO");
        GUI.Label(new Rect(35f, 50f, 230f, 22f),
            inventory.hasPackage
                ? "Carrying package " + inventory.CarriedPackageSetNumber
                : "Carrying package: None");
        GUI.Label(new Rect(35f, 75f, 230f, 22f),
            "Packages delivered: " + inventory.deliveredPackageCount);
        GUI.Label(new Rect(35f, 100f, 230f, 22f),
            "Cash on hand: $" + inventory.cashOnHand);
        GUI.Label(new Rect(35f, 125f, 230f, 22f),
            "Deposited: $" + inventory.money);
    }
}