using UnityEngine;

public class nande_testing_player_info_script : MonoBehaviour
{
    [Header("Bird Alert")]
    [Tooltip("Set this from the bird AI when birds are actively after the player.")]
    public bool birdsAfterYou;

    private nande_testing_inventory_management_script inventory;
    private GUIStyle panelStyle;
    private GUIStyle titleStyle;
    private GUIStyle valueStyle;
    private GUIStyle iconStyle;
    private GUIStyle statusStyle;
    private bool previousBirdsAfterYou;
    private float alertUntil;

    void Update()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<nande_testing_inventory_management_script>();

        if (birdsAfterYou != previousBirdsAfterYou)
        {
            if (birdsAfterYou)
                alertUntil = Time.unscaledTime + 4f;

            previousBirdsAfterYou = birdsAfterYou;
        }
    }

    public void SetBirdsAfterYou(bool active)
    {
        birdsAfterYou = active;
    }

    void OnGUI()
    {
        if (inventory == null)
            return;

        EnsureStyles();

        const float margin = 24f;
        const float panelWidth = 164f;
        const float panelHeight = 76f;
        const float gap = 8f;
        float right = Screen.width - margin;
        float top = margin;

        DrawInfoPanel(
            new Rect(right - panelWidth, top, panelWidth, panelHeight),
            "$",
            "CASH",
            "$" + inventory.cashOnHand.ToString("N0"),
            new Color(0.18f, 0.75f, 0.42f));
        DrawInfoPanel(
            new Rect(right - panelWidth, top + panelHeight + gap, panelWidth, panelHeight),
            "▣",
            "BANK",
            "$" + inventory.money.ToString("N0"),
            new Color(0.29f, 0.63f, 0.95f));

        const float deliveryWidth = 246f;
        float bottom = Screen.height - margin - panelHeight;
        string deliveryStatus = inventory.hasPackage
            ? "PACKAGE " + inventory.CarriedPackageSetNumber
            : "NO PACKAGE";
        DrawInfoPanel(
            new Rect(margin, bottom, deliveryWidth, panelHeight),
            "◆",
            "DELIVERY " + inventory.deliveredPackageCount,
            deliveryStatus,
            new Color(0.95f, 0.67f, 0.22f));

        if (birdsAfterYou)
        {
            float wantedWidth = 108f;
            Rect wantedRect = new Rect(
                right - panelWidth - gap - wantedWidth,
                top,
                wantedWidth,
                panelHeight);
            DrawWantedPanel(wantedRect);

            if (Time.unscaledTime < alertUntil)
                DrawBirdAlert();
        }
    }

    void EnsureStyles()
    {
        if (panelStyle != null)
            return;

        panelStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.UpperLeft,
            padding = new RectOffset(14, 12, 10, 8),
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.78f, 0.82f, 0.86f) }
        };
        valueStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleRight,
            normal = { textColor = Color.white }
        };
        iconStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };
        statusStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };
    }

    void DrawInfoPanel(Rect rect, string icon, string title, string value, Color accent)
    {
        Color previousColor = GUI.color;
        GUI.color = new Color(0.035f, 0.045f, 0.06f, 0.92f);
        GUI.Box(rect, GUIContent.none, panelStyle);
        GUI.color = accent;
        GUI.DrawTexture(new Rect(rect.x, rect.y, 4f, rect.height), Texture2D.whiteTexture);
        GUI.color = previousColor;

        GUI.Label(new Rect(rect.x + 10f, rect.y + 8f, 34f, 34f), icon, iconStyle);
        GUI.Label(new Rect(rect.x + 48f, rect.y + 10f, rect.width - 58f, 18f), title, titleStyle);
        GUI.Label(new Rect(rect.x + 48f, rect.y + 28f, rect.width - 58f, 34f), value, valueStyle);
    }

    void DrawWantedPanel(Rect rect)
    {
        Color accent = new Color(0.91f, 0.2f, 0.2f);
        Color previousColor = GUI.color;
        GUI.color = new Color(0.035f, 0.045f, 0.06f, 0.92f);
        GUI.Box(rect, GUIContent.none, panelStyle);
        GUI.color = accent;
        GUI.DrawTexture(new Rect(rect.x, rect.y, 4f, rect.height), Texture2D.whiteTexture);
        GUI.color = previousColor;

        GUI.Label(new Rect(rect.x + 10f, rect.y + 7f, rect.width - 20f, 18f), "BIRDS", titleStyle);
        GUI.Label(new Rect(rect.x + 8f, rect.y + 22f, rect.width - 16f, 32f), "♠", iconStyle);
        GUI.color = accent;
        GUI.Label(new Rect(rect.x + 8f, rect.y + 53f, rect.width - 16f, 16f), "ON", statusStyle);
        GUI.color = previousColor;
    }

    void DrawBirdAlert()
    {
        const float width = 330f;
        const float height = 68f;
        Rect rect = new Rect((Screen.width - width) * 0.5f, 26f, width, height);
        Color accent = new Color(0.91f, 0.2f, 0.2f);
        Color previousColor = GUI.color;

        GUI.color = new Color(0.035f, 0.045f, 0.06f, 0.95f);
        GUI.Box(rect, GUIContent.none, panelStyle);
        GUI.color = accent;
        GUI.DrawTexture(new Rect(rect.x, rect.y, 4f, rect.height), Texture2D.whiteTexture);
        GUI.color = previousColor;

        GUI.Label(new Rect(rect.x + 14f, rect.y + 8f, 30f, 44f), "!", iconStyle);
        GUI.Label(new Rect(rect.x + 52f, rect.y + 9f, width - 66f, 22f), "BIRDS APPROACHING", titleStyle);
        GUI.Label(new Rect(rect.x + 52f, rect.y + 30f, width - 66f, 22f), "STAY ALERT", statusStyle);
    }
}