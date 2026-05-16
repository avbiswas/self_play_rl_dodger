using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BulletCooldownVisualizer : MonoBehaviour
{
    [SerializeField] Image cooldownFill;
    [SerializeField] TextMeshProUGUI cooldownText;
    [SerializeField] Vector3 localOffset = new Vector3(0, 1.4f, 0);
    [SerializeField] Vector3 localScale = new Vector3(0.01f, 0.01f, 0.01f);
    [SerializeField] Vector2 barSize = new Vector2(120, 16);
    [SerializeField] float cooldownTextFontSize = 90;

    SmartBulletSpawner spawner;
    bool blockedByCrouch;
    Canvas canvas;

    void Awake()
    {
        EnsureVisuals();
        gameObject.SetActive(false);
    }

    public void AttachToPlayer(Transform player)
    {
        transform.SetParent(player, false);
        transform.localPosition = localOffset;
        transform.localRotation = Quaternion.identity;
        transform.localScale = localScale;
        EnsureVisuals();
    }

    public void Show(SmartBulletSpawner source, bool isBlockedByCrouch = false)
    {
        spawner = source;
        blockedByCrouch = isBlockedByCrouch;
        EnsureVisuals();
        gameObject.SetActive(true);
        UpdateDisplay();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (spawner == null)
        {
            Hide();
            return;
        }

        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        float progress = spawner.GetBulletCooldownProgress();
        float remaining = spawner.GetBulletCooldownRemaining();

        if (cooldownFill != null)
        {
            cooldownFill.fillAmount = progress;
        }

        if (cooldownText != null)
        {
            if (blockedByCrouch)
            {
                cooldownText.text = "CAN'T SHOOT";
            }
            else if (spawner.CanShoot())
            {
                cooldownText.text = "Loaded";
            }
            else
            {
                cooldownText.text = remaining.ToString("0.0");
            }
        }
    }

    void EnsureVisuals()
    {
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 50;

        RectTransform root = GetComponent<RectTransform>();
        if (root != null)
        {
            root.sizeDelta = barSize;
        }

        if (cooldownFill == null)
        {
            Transform fillTransform = transform.Find("CooldownBackground/CooldownFill");
            if (fillTransform != null)
            {
                cooldownFill = fillTransform.GetComponent<Image>();
            }
        }

        if (cooldownText == null)
        {
            Transform textTransform = transform.Find("CooldownText");
            if (textTransform != null)
            {
                cooldownText = textTransform.GetComponent<TextMeshProUGUI>();
            }
        }

        if (cooldownFill == null)
        {
            Image background = CreateImage("CooldownBackground", transform, new Color(0, 0, 0, 0.6f));
            RectTransform backgroundRect = background.GetComponent<RectTransform>();
            backgroundRect.sizeDelta = barSize;

            cooldownFill = CreateImage("CooldownFill", background.transform, new Color(0.2f, 0.85f, 1f, 0.95f));
            RectTransform fillRect = cooldownFill.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0, 0);
            fillRect.anchorMax = new Vector2(1, 1);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
        }

        cooldownFill.type = Image.Type.Filled;
        cooldownFill.fillMethod = Image.FillMethod.Horizontal;
        cooldownFill.fillOrigin = (int)Image.OriginHorizontal.Left;

        if (cooldownText == null)
        {
            cooldownText = CreateText("CooldownText", transform);
        }
    }

    Image CreateImage(string objectName, Transform parent, Color color)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        Image image = imageObject.GetComponent<Image>();
        image.color = color;

        RectTransform rectTransform = image.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;

        return image;
    }

    TextMeshProUGUI CreateText(string objectName, Transform parent)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.fontSize = cooldownTextFontSize;
        text.fontStyle = FontStyles.Bold;
        text.raycastTarget = false;

        RectTransform rectTransform = text.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(0, 78);
        rectTransform.sizeDelta = new Vector2(barSize.x * 5, 120);

        return text;
    }
}
