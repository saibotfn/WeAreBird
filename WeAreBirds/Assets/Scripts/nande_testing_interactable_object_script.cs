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
    private LineRenderer dropoffRing;
    private Renderer dropoffBeamRenderer;
    private Collider[] dropoffColliders;
    private bool dropoffAvailable;
    private float labelHeight;
    private float labelBobTime;
    private Transform playerTransform;

    private const float LabelVisibleDistance = 5f;

    [Header("Interaction Status")]
    [SerializeField] private bool alreadyUsed = false;

    void Awake()
    {
        if (!dropoff)
            return;

        dropoffColliders = GetComponents<Collider>();
        CreateDropoffVisuals();
        UpdateDropoffAvailability();
    }

    void Update()
    {
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
        }

        UpdateLabelVisibility();
    }

    void CreateDropoffVisuals()
    {
        Collider objectCollider = GetComponent<Collider>();
        float topHeight = objectCollider != null
            ? transform.InverseTransformPoint(objectCollider.bounds.max).y
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

        CreateDropoffRing(objectCollider, topHeight);
        CreateDropoffBeam(topHeight);
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

    void CreateDropoffRing(Collider objectCollider, float topHeight)
    {
        float radius = objectCollider != null
            ? Mathf.Max(objectCollider.bounds.extents.x, objectCollider.bounds.extents.z) + 0.15f
            : 1.2f;

        GameObject ringObject = new GameObject("DropOffRing");
        ringObject.transform.SetParent(transform, false);
        ringObject.transform.localPosition = new Vector3(0f, topHeight + 0.04f, 0f);

        dropoffRing = ringObject.AddComponent<LineRenderer>();
        dropoffRing.useWorldSpace = false;
        dropoffRing.loop = true;
        dropoffRing.positionCount = 48;
        dropoffRing.widthMultiplier = 0.08f;
        dropoffRing.material = new Material(Shader.Find("Sprites/Default"));
        dropoffRing.material.color = new Color(1f, 0.8f, 0.05f, 0.9f);

        for (int index = 0; index < dropoffRing.positionCount; index++)
        {
            float angle = index * Mathf.PI * 2f / dropoffRing.positionCount;
            dropoffRing.SetPosition(index, new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
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

        if (dropoffRing != null)
            dropoffRing.enabled = shouldBeActive;

        if (dropoffBeamRenderer != null)
            dropoffBeamRenderer.enabled = shouldBeActive;
    }

    void CreateDropoffBeam(float topHeight)
    {
        GameObject beamObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        beamObject.name = "DropOffBeam";
        beamObject.transform.SetParent(transform, false);
        beamObject.transform.localPosition = new Vector3(0f, topHeight + 2.5f, 0f);
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

        if (inventory.PickupPackage(gameObject, deliverySetNumber))
        {
            alreadyUsed = true;

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
            UpdateLabelVisibility();

            inventory.CollectPayment(dropoffPayment);
            Debug.Log("Dove dropped off the package and collected the payment!");
        }
    }

    void HandleOnkel(nande_testing_inventory_management_script inventory)
    {
        if (inventory.DepositMoney())
            Debug.Log("Money deposited at Onkel's home!");
    }
}
