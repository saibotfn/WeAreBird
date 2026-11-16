using System.Collections;
using UnityEngine;

public class nande_testing_interactable_object_script : MonoBehaviour
{
    [Header("Interaction Type - Select ONE")]
    public bool pickup = false;
    public bool dropoff = false;
    public bool onkel = false;

    [Header("Delivery Set")]
    [Min(1)]
    public int deliverySetNumber = 1;

    [Header("Drop-off Settings")]
    public int dropoffPayment = 100;
    private Transform packagePlacementPoint;
    private Light dropoffLight;
    private TextMesh dropoffLabel;
    private Renderer dropoffBeamRenderer;
    private Collider[] dropoffColliders;
    private Collider[] pickupColliders;
    private Light pickupLight;
    private TextMesh pickupLabel;
    private Renderer pickupBeamRenderer;
    private Transform[] prefabChildren;
    private bool dropoffAvailable;
    private float labelHeight;
    private float labelBobTime;
    private Transform playerTransform;

    private const float LabelVisibleDistance = 5f;

    [Header("Interaction Status")]
    [SerializeField] private bool alreadyUsed = false;

    void Awake()
    {
        prefabChildren = GetComponentsInChildren<Transform>(true);

        if (pickup)
        {
            pickupColliders = GetComponents<Collider>();
            CreatePickupVisuals();
            UpdatePickupAvailability();
        }

        if (!dropoff)
            return;

        dropoffColliders = GetComponents<Collider>();
        CreateDropoffVisuals();
        UpdateDropoffAvailability();
    }

    void Update()
    {
        if (pickup)
        {
            UpdatePickupAvailability();
            FaceTextToPlayer(pickupLabel != null ? pickupLabel.transform : null);
            return;
        }

        if (!dropoff)
            return;

        UpdateDropoffAvailability();
        UpdatePlayerTransform();

        if (alreadyUsed && playerTransform != null &&
            Vector3.Distance(transform.position, playerTransform.position) > LabelVisibleDistance)
        {
            gameObject.SetActive(false);
            return;
        }

        if (dropoffLabel != null)
        {
            labelBobTime += Time.deltaTime;
            Vector3 labelPosition = dropoffLabel.transform.localPosition;
            labelPosition.y = labelHeight + Mathf.Sin(labelBobTime * 2.5f) * 0.12f;
            dropoffLabel.transform.localPosition = labelPosition;
            FaceTextToPlayer(dropoffLabel.transform);
        }

        UpdateLabelVisibility();
    }

    void CreateDropoffVisuals()
    {
        Collider objectCollider = GetComponent<Collider>();
        float topHeight = objectCollider != null
            ? transform.InverseTransformPoint(objectCollider.bounds.max).y
            : 0f;
        float bottomHeight = objectCollider != null
            ? transform.InverseTransformPoint(objectCollider.bounds.min).y
            : 0f;
        labelHeight = topHeight + 0.8f;

        GameObject placementObject = new GameObject("PackagePlacementPoint");
        placementObject.transform.SetParent(transform, false);
        placementObject.transform.localPosition = new Vector3(0f, topHeight + 0.1f, 0f);
        packagePlacementPoint = placementObject.transform;

        GameObject lightObject = new GameObject("DropOffLight");
        lightObject.transform.SetParent(transform, false);
        lightObject.transform.localPosition = new Vector3(0f, topHeight + 0.2f, 0f);
        lightObject.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        dropoffLight = lightObject.AddComponent<Light>();
        dropoffLight.type = LightType.Spot;
        dropoffLight.color = Color.yellow;
        dropoffLight.range = 6f;
        dropoffLight.spotAngle = 45f;
        dropoffLight.innerSpotAngle = 20f;
        dropoffLight.intensity = 12f;
        dropoffLight.renderMode = LightRenderMode.ForcePixel;
        dropoffLight.shadows = LightShadows.None;

        GameObject labelObject = new GameObject("DropOffLabel");
        labelObject.transform.SetParent(transform, false);
        labelObject.transform.localPosition = new Vector3(0f, labelHeight, 0f);
        dropoffLabel = labelObject.AddComponent<TextMesh>();
        dropoffLabel.anchor = TextAnchor.MiddleCenter;
        dropoffLabel.alignment = TextAlignment.Center;
        dropoffLabel.characterSize = 0.05f;
        dropoffLabel.fontSize = 64;
        dropoffLabel.color = Color.yellow;

        dropoffLabel.text = "Drop Off " + deliverySetNumber;
        dropoffLabel.gameObject.SetActive(true);

        CreateDropoffBeam(bottomHeight);
    }

    void CreatePickupVisuals()
    {
        Collider objectCollider = GetComponent<Collider>();
        float topHeight = objectCollider != null
            ? transform.InverseTransformPoint(objectCollider.bounds.max).y
            : 0f;
        float bottomHeight = objectCollider != null
            ? transform.InverseTransformPoint(objectCollider.bounds.min).y
            : 0f;
        labelHeight = topHeight + 0.8f;

        GameObject lightObject = new GameObject("PickupLight");
        lightObject.transform.SetParent(transform, false);
        lightObject.transform.localPosition = new Vector3(0f, topHeight + 0.2f, 0f);
        lightObject.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        pickupLight = lightObject.AddComponent<Light>();
        pickupLight.type = LightType.Spot;
        pickupLight.color = Color.green;
        pickupLight.range = 6f;
        pickupLight.spotAngle = 45f;
        pickupLight.innerSpotAngle = 20f;
        pickupLight.intensity = 12f;
        pickupLight.renderMode = LightRenderMode.ForcePixel;
        pickupLight.shadows = LightShadows.None;

        GameObject labelObject = new GameObject("PickupLabel");
        labelObject.transform.SetParent(transform, false);
        labelObject.transform.localPosition = new Vector3(0f, labelHeight, 0f);
        pickupLabel = labelObject.AddComponent<TextMesh>();
        pickupLabel.anchor = TextAnchor.MiddleCenter;
        pickupLabel.alignment = TextAlignment.Center;
        pickupLabel.characterSize = 0.05f;
        pickupLabel.fontSize = 64;
        pickupLabel.color = Color.green;
        pickupLabel.text = "Pickup";

        CreatePickupBeam(bottomHeight);
    }

    void CreatePickupBeam(float bottomHeight)
    {
        GameObject beamObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        beamObject.name = "PickupBeam";
        beamObject.transform.SetParent(transform, false);
        beamObject.transform.localPosition = new Vector3(0f, bottomHeight + 2.5f, 0f);
        beamObject.transform.localScale = new Vector3(0.7f, 2.5f, 0.7f);

        Collider beamCollider = beamObject.GetComponent<Collider>();
        if (beamCollider != null)
            beamCollider.enabled = false;

        Material beamMaterial = new Material(Shader.Find("Sprites/Default"));
        beamMaterial.color = new Color(0.1f, 1f, 0.1f, 0.18f);
        pickupBeamRenderer = beamObject.GetComponent<Renderer>();
        pickupBeamRenderer.material = beamMaterial;
    }

    void UpdateLabelVisibility()
    {
        UpdatePlayerTransform();

        bool playerIsCloseEnough = playerTransform == null ||
            Vector3.Distance(transform.position, playerTransform.position) <= LabelVisibleDistance;

        if (dropoffLabel != null)
            dropoffLabel.gameObject.SetActive(dropoffAvailable && !alreadyUsed && playerIsCloseEnough);
    }

    void UpdatePlayerTransform()
    {
        if (playerTransform == null)
        {
            nande_testing_inventory_management_script inventory =
                FindFirstObjectByType<nande_testing_inventory_management_script>();

            if (inventory != null)
                playerTransform = inventory.transform;
        }
    }

    void UpdateDropoffAvailability()
    {
        nande_testing_inventory_management_script inventory =
            FindFirstObjectByType<nande_testing_inventory_management_script>();
        bool shouldBeActive = alreadyUsed ||
            (inventory != null && inventory.IsPackagePickedUpForSet(deliverySetNumber));
        dropoffAvailable = shouldBeActive;

        foreach (Collider dropoffCollider in dropoffColliders)
            dropoffCollider.enabled = shouldBeActive;

        if (dropoffLight != null)
            dropoffLight.enabled = shouldBeActive;

        if (dropoffLabel != null)
            dropoffLabel.gameObject.SetActive(shouldBeActive);

        if (dropoffBeamRenderer != null)
            dropoffBeamRenderer.enabled = shouldBeActive;
    }

    void UpdatePickupAvailability()
    {
        if (alreadyUsed)
            return;

        nande_testing_inventory_management_script inventory =
            FindFirstObjectByType<nande_testing_inventory_management_script>();
        bool shouldBeActive = inventory != null &&
            inventory.IsDeliverySetAvailable(deliverySetNumber);

        foreach (Collider pickupCollider in pickupColliders)
            pickupCollider.enabled = shouldBeActive;

        SetChildrenActive(shouldBeActive);
        SetPickupVisualsActive(shouldBeActive);
    }

    void SetPickupVisualsActive(bool isActive)
    {
        if (pickupLight != null)
            pickupLight.enabled = isActive;

        if (pickupLabel != null)
            pickupLabel.gameObject.SetActive(isActive);

        if (pickupBeamRenderer != null)
            pickupBeamRenderer.enabled = isActive;
    }

    void CreateDropoffBeam(float bottomHeight)
    {
        GameObject beamObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        beamObject.name = "DropOffBeam";
        beamObject.transform.SetParent(transform, false);
        beamObject.transform.localPosition = new Vector3(0f, bottomHeight + 2.5f, 0f);
        beamObject.transform.localScale = new Vector3(0.7f, 2.5f, 0.7f);

        Collider beamCollider = beamObject.GetComponent<Collider>();
        if (beamCollider != null)
            beamCollider.enabled = false;

        Material beamMaterial = new Material(Shader.Find("Sprites/Default"));
        beamMaterial.color = new Color(1f, 0.8f, 0.05f, 0.18f);
        dropoffBeamRenderer = beamObject.GetComponent<Renderer>();
        dropoffBeamRenderer.material = beamMaterial;
    }

    public void Interact()
    {
        // Find the player's delivery inventory
        nande_testing_inventory_management_script inventory =
            FindFirstObjectByType<nande_testing_inventory_management_script>();

        if (inventory == null)
        {
            Debug.LogWarning("No nande_testing_inventory_management_script found!");
            return;
        }

        // Validate inspector selection
        int selectedTypes =
            (pickup ? 1 : 0) +
            (dropoff ? 1 : 0) +
            (onkel ? 1 : 0);

        if (selectedTypes != 1)
        {
            Debug.LogWarning(
                "Select exactly ONE interaction type on " +
                gameObject.name
            );
            return;
        }

        if (pickup)
        {
            HandlePickup(inventory);
        }
        else if (dropoff)
        {
            HandleDropoff(inventory);
        }
        else if (onkel)
        {
            HandleOnkel(inventory);
        }
    }

    void HandlePickup(nande_testing_inventory_management_script inventory)
    {
        if (alreadyUsed)
        {
            Debug.Log("This package has already been collected.");
            return;
        }

        if (!inventory.IsDeliverySetAvailable(deliverySetNumber))
        {
            Debug.Log("Complete and deposit the previous delivery first.");
            return;
        }

        if (inventory.PickupPackage(gameObject, deliverySetNumber))
        {
            alreadyUsed = true;
            SetChildrenActive(false);
            SetPickupVisualsActive(false);

            Debug.Log("Dove picked up the package!");
        }
    }

    void HandleDropoff(nande_testing_inventory_management_script inventory)
    {
        if (alreadyUsed)
        {
            Debug.Log("Already delivered here.");
            return;
        }

        Transform placementPoint = packagePlacementPoint != null
            ? packagePlacementPoint
            : transform;

        if (inventory.DropoffPackage(placementPoint, deliverySetNumber))
        {
            alreadyUsed = true;
            SetChildrenActive(false);
            UpdateLabelVisibility();

            inventory.CollectPayment(dropoffPayment);
            ShowPaymentPopup(dropoffPayment);
            Debug.Log("Dove dropped off the package and collected the payment!");
        }
    }

    void HandleOnkel(nande_testing_inventory_management_script inventory)
    {
        if (inventory.DepositMoney())
        {
            ShowMoneyPopup(inventory.LastDepositedAmount, Color.yellow);
            Debug.Log("Money deposited at Onkel's home!");
        }
    }

    void SetChildrenActive(bool isActive)
    {
        foreach (Transform child in prefabChildren)
        {
            if (child != transform)
                child.gameObject.SetActive(isActive);
        }
    }

    void FaceTextToPlayer(Transform textTransform)
    {
        if (textTransform == null)
            return;

        Camera playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindFirstObjectByType<Camera>();

        if (playerCamera == null)
            return;

        Vector3 directionToCamera = textTransform.position - playerCamera.transform.position;
        if (directionToCamera.sqrMagnitude > 0.001f)
            textTransform.rotation = Quaternion.LookRotation(directionToCamera, playerCamera.transform.up);
    }

    void ShowPaymentPopup(int payment)
    {
        ShowMoneyPopup(payment, Color.green);
    }

    void ShowMoneyPopup(int amount, Color color)
    {
        GameObject popupObject = new GameObject("PaymentPopup");
        popupObject.transform.SetParent(transform, false);
        popupObject.transform.localPosition = new Vector3(0f, labelHeight, 0f);

        TextMesh popupText = popupObject.AddComponent<TextMesh>();
        popupText.anchor = TextAnchor.MiddleCenter;
        popupText.alignment = TextAlignment.Center;
        popupText.characterSize = 0.06f;
        popupText.fontSize = 64;
        popupText.color = color;
        popupText.text = "+$" + amount;
        FaceTextToPlayer(popupObject.transform);

        StartCoroutine(AnimatePaymentPopup(popupObject));
    }

    IEnumerator AnimatePaymentPopup(GameObject popupObject)
    {
        const float popupDuration = 1.5f;
        float elapsed = 0f;
        Vector3 startPosition = popupObject.transform.localPosition;

        while (elapsed < popupDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / popupDuration);
            Vector3 position = startPosition;
            position.y += progress * 0.8f;
            popupObject.transform.localPosition = position;
            FaceTextToPlayer(popupObject.transform);

            TextMesh popupText = popupObject.GetComponent<TextMesh>();
            if (popupText != null)
            {
                Color color = popupText.color;
                color.a = 1f - progress;
                popupText.color = color;
            }

            yield return null;
        }

        Destroy(popupObject);
    }
}
