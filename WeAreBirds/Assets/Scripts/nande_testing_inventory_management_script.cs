using UnityEngine;

public class nande_testing_inventory_management_script : MonoBehaviour
{
    [Header("Delivery Status")]
    public bool hasPackage = false;
    public bool packageDelivered = false;
    public int deliveredPackageCount = 0;

    private GameObject carriedPackage;
    private int carriedPackageSetNumber = -1;

    public bool IsPackagePickedUpForSet(int deliverySetNumber)
    {
        return hasPackage && carriedPackageSetNumber == deliverySetNumber;
    }

    public int CarriedPackageSetNumber => carriedPackageSetNumber;

    [Header("Currency")]
    public int money = 0;
    public int cashOnHand = 0;

    public bool PickupPackage(GameObject package, int deliverySetNumber)
    {
        if (hasPackage || packageDelivered)
        {
            Debug.Log("Cannot pick up another package.");
            return false;
        }

        hasPackage = true;
        carriedPackage = package;
        carriedPackageSetNumber = deliverySetNumber;
        package.SetActive(false);
        Debug.Log("Package " + deliverySetNumber + " collected!");
        return true;
    }

    public bool DropoffPackage(Transform placementPoint, int deliverySetNumber)
    {
        if (!hasPackage || carriedPackage == null)
        {
            Debug.Log("No package to drop off!");
            return false;
        }

        if (carriedPackageSetNumber != deliverySetNumber)
        {
            Debug.Log("This drop-off is for package " + deliverySetNumber + ".");
            return false;
        }

        carriedPackage.transform.SetParent(placementPoint, false);
        carriedPackage.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        carriedPackage.SetActive(true);
        carriedPackage = null;
        carriedPackageSetNumber = -1;
        hasPackage = false;
        packageDelivered = true;
        deliveredPackageCount++;

        Debug.Log("Package " + deliverySetNumber + " delivered!");
        return true;
    }

    public bool CollectPayment(int payment)
    {
        if (!packageDelivered)
        {
            Debug.Log("Complete the dropoff first!");
            return false;
        }

        cashOnHand += payment;
        packageDelivered = false;

        Debug.Log("Payment received: $" + payment);
        Debug.Log("Cash on hand: $" + cashOnHand);

        return true;
    }

    public bool DepositMoney()
    {
        if (cashOnHand <= 0)
        {
            Debug.Log("You have no cash to deposit.");
            return false;
        }

        money += cashOnHand;
        Debug.Log("Deposited: $" + cashOnHand);
        cashOnHand = 0;
        Debug.Log("Total money: $" + money);

        return true;
    }
}
