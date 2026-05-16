using BaslangicSeviye.SayiTahminOyunu;
using BaslangicSeviye.SayiTahminOyunu.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class SayiTahminKurucu
{
    private static readonly Color32 BackgroundColor = new Color32(15, 23, 42, 255); // #0F172A
    private static readonly Color32 CardColor = new Color32(30, 41, 59, 245);       // #1E293B
    private static readonly Color32 PrimaryBlue = new Color32(59, 130, 246, 255);    // #3B82F6
    private static readonly Color32 TextColor = new Color32(248, 250, 252, 255);     // #F8FAFC

    [MenuItem("Tools/Sayi Tahmin Oyunu/Kurulumu Olustur")]
    private static void KurulumuOlustur()
    {
        if (!EditorUtility.DisplayDialog("Sayi Tahmin Oyunu", "Modern UI kurulumu yapilsin mi?", "Evet", "Vazgec"))
        {
            return;
        }

        GameObject canvasObject = MevcutVeyaYeniCanvas();
        EskiKurulumuTemizle(canvasObject.transform);
        ArkaPlanOlustur(canvasObject.transform, "BackgroundBase", BackgroundColor, false);
        ArkaPlanOlustur(canvasObject.transform, "BackgroundGlow", new Color32(30, 41, 59, 110), true);
        Transform panel = OyunPaneliOlustur(canvasObject.transform);

        BaslikOlustur(panel);
        TMP_Text remainingLivesText = HakGosterimiOlustur(panel);
        TMP_InputField inputField = InputOlustur(panel);
        TMP_Text messageText = MesajOlustur(panel);
        Button guessButton = ButonOlustur(panel, "GuessButton", "Tahmin Et", new Vector2(-112f, -150f), PrimaryBlue, true);
        Button restartButton = ButonOlustur(panel, "RestartButton", "Yeniden Baslat", new Vector2(112f, -150f), new Color32(51, 65, 85, 255), false);

        GameObject gameManagerObject = new GameObject("GameManager");
        Undo.RegisterCreatedObjectUndo(gameManagerObject, "GameManager olustur");
        SayiTahminOyunu gameManager = gameManagerObject.AddComponent<SayiTahminOyunu>();

        SerializedObject serializedObject = new SerializedObject(gameManager);
        serializedObject.FindProperty("guessInputField").objectReferenceValue = inputField;
        serializedObject.FindProperty("messageText").objectReferenceValue = messageText;
        serializedObject.FindProperty("remainingLivesText").objectReferenceValue = remainingLivesText;
        serializedObject.FindProperty("guessButton").objectReferenceValue = guessButton;
        serializedObject.FindProperty("restartButton").objectReferenceValue = restartButton;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();

        Selection.activeGameObject = gameManagerObject;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    private static void EskiKurulumuTemizle(Transform canvas)
    {
        string[] eskiNesneler = { "BackgroundBase", "BackgroundGlow", "GamePanel" };
        for (int i = 0; i < eskiNesneler.Length; i++)
        {
            Transform nesne = canvas.Find(eskiNesneler[i]);
            if (nesne != null)
            {
                Undo.DestroyObjectImmediate(nesne.gameObject);
            }
        }

        GameObject gameManager = GameObject.Find("GameManager");
        if (gameManager != null)
        {
            Undo.DestroyObjectImmediate(gameManager);
        }
    }

    private static GameObject MevcutVeyaYeniCanvas()
    {
        Canvas mevcutCanvas = Object.FindFirstObjectByType<Canvas>();
        if (mevcutCanvas != null)
        {
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                Undo.RegisterCreatedObjectUndo(eventSystem, "EventSystem olustur");
            }

            return mevcutCanvas.gameObject;
        }

        GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasObject, "Canvas olustur");

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        Undo.RegisterCreatedObjectUndo(eventSystemObject, "EventSystem olustur");

        return canvasObject;
    }

    private static void ArkaPlanOlustur(Transform canvas, string objeAdi, Color32 renk, bool gradient)
    {
        GameObject backgroundObject = new GameObject(objeAdi, typeof(RectTransform), typeof(Image));
        Undo.RegisterCreatedObjectUndo(backgroundObject, $"{objeAdi} olustur");
        backgroundObject.transform.SetParent(canvas, false);
        backgroundObject.transform.SetAsFirstSibling();

        RectTransform rect = backgroundObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = backgroundObject.GetComponent<Image>();
        image.color = renk;
        image.raycastTarget = false;

        if (gradient)
        {
            UIImageGradient gradientEffect = backgroundObject.AddComponent<UIImageGradient>();
            SetGradientColors(gradientEffect, new Color32(30, 58, 138, 145), new Color32(15, 23, 42, 5));
        }
    }

    private static Transform OyunPaneliOlustur(Transform canvas)
    {
        GameObject panelObject = new GameObject("GamePanel", typeof(RectTransform), typeof(Image));
        Undo.RegisterCreatedObjectUndo(panelObject, "GamePanel olustur");
        panelObject.transform.SetParent(canvas, false);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(680f, 450f);
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = CardColor;
        panelImage.sprite = GetRoundedSprite();
        panelImage.type = Image.Type.Sliced;

        UIImageGradient gradient = panelObject.AddComponent<UIImageGradient>();
        SetGradientColors(gradient, new Color32(51, 65, 85, 255), CardColor);

        Outline outline = panelObject.AddComponent<Outline>();
        outline.effectColor = new Color32(15, 23, 42, 180);
        outline.effectDistance = new Vector2(2f, -2f);

        Shadow shadow = panelObject.AddComponent<Shadow>();
        shadow.effectColor = new Color32(2, 6, 23, 110);
        shadow.effectDistance = new Vector2(0f, -8f);

        return panelObject.transform;
    }

    private static TMP_Text BaslikOlustur(Transform parent)
    {
        GameObject titleObject = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        Undo.RegisterCreatedObjectUndo(titleObject, "TitleText olustur");
        titleObject.transform.SetParent(parent, false);

        RectTransform rect = titleObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(600f, 80f);
        rect.anchoredPosition = new Vector2(0f, -30f);

        TMP_Text text = titleObject.GetComponent<TMP_Text>();
        text.text = "Sayi Tahmin Oyunu";
        text.fontSize = 52f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = TextColor;

        Shadow glow = titleObject.AddComponent<Shadow>();
        glow.effectColor = new Color32(59, 130, 246, 120);
        glow.effectDistance = new Vector2(0f, -2f);
        return text;
    }

    private static TMP_Text HakGosterimiOlustur(Transform parent)
    {
        GameObject livesObject = new GameObject("RemainingLivesText", typeof(RectTransform), typeof(TextMeshProUGUI));
        Undo.RegisterCreatedObjectUndo(livesObject, "RemainingLivesText olustur");
        livesObject.transform.SetParent(parent, false);

        RectTransform rect = livesObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.sizeDelta = new Vector2(240f, 46f);
        rect.anchoredPosition = Vector2.zero;

        TMP_Text text = livesObject.GetComponent<TMP_Text>();
        text.text = "Kalan Hak: 5";
        text.fontSize = 24f;
        text.fontStyle = FontStyles.Normal;
        text.fontWeight = FontWeight.Medium;
        text.alignment = TextAlignmentOptions.MidlineRight;
        text.color = TextColor;
        return text;
    }

    private static TMP_InputField InputOlustur(Transform parent)
    {
        TMP_DefaultControls.Resources resources = new TMP_DefaultControls.Resources();
        GameObject inputObject = TMP_DefaultControls.CreateInputField(resources);
        inputObject.name = "GuessInputField";
        Undo.RegisterCreatedObjectUndo(inputObject, "GuessInputField olustur");
        inputObject.transform.SetParent(parent, false);

        RectTransform rect = inputObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(390f, 66f);
        rect.anchoredPosition = new Vector2(0f, 44f);

        Image inputImage = inputObject.GetComponent<Image>();
        if (inputImage != null)
        {
            inputImage.color = Color.white;
            inputImage.sprite = GetRoundedSprite();
            inputImage.type = Image.Type.Sliced;
        }

        Shadow inputShadow = inputObject.AddComponent<Shadow>();
        inputShadow.effectColor = new Color32(15, 23, 42, 80);
        inputShadow.effectDistance = new Vector2(0f, -3f);

        TMP_InputField inputField = inputObject.GetComponent<TMP_InputField>();
        inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        inputField.textViewport.offsetMin = new Vector2(14f, 8f);
        inputField.textViewport.offsetMax = new Vector2(-14f, -8f);

        TMP_Text placeholder = inputObject.transform.Find("Placeholder")?.GetComponent<TMP_Text>();
        if (placeholder != null)
        {
            placeholder.text = "Tahminini yaz...";
            placeholder.fontSize = 23f;
            placeholder.color = new Color32(100, 116, 139, 255);
            placeholder.alignment = TextAlignmentOptions.Center;
        }

        TMP_Text inputText = inputObject.transform.Find("Text Area/Text")?.GetComponent<TMP_Text>();
        if (inputText != null)
        {
            inputText.fontSize = 30f;
            inputText.color = new Color32(15, 23, 42, 255);
            inputText.fontStyle = FontStyles.Bold;
            inputText.alignment = TextAlignmentOptions.Center;
        }

        return inputField;
    }

    private static TMP_Text MesajOlustur(Transform parent)
    {
        GameObject messageObject = new GameObject("MessageText", typeof(RectTransform), typeof(TextMeshProUGUI));
        Undo.RegisterCreatedObjectUndo(messageObject, "MessageText olustur");
        messageObject.transform.SetParent(parent, false);

        RectTransform rect = messageObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(580f, 100f);
        rect.anchoredPosition = new Vector2(0f, -56f);

        TMP_Text text = messageObject.GetComponent<TMP_Text>();
        text.text = "Tahmin bekleniyor...";
        text.fontSize = 32f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = TextColor;
        return text;
    }

    private static Button ButonOlustur(Transform parent, string objeAdi, string yazi, Vector2 posizyon, Color32 renk, bool maviGradient)
    {
        TMP_DefaultControls.Resources resources = new TMP_DefaultControls.Resources();
        GameObject buttonObject = TMP_DefaultControls.CreateButton(resources);
        buttonObject.name = objeAdi;
        Undo.RegisterCreatedObjectUndo(buttonObject, $"{objeAdi} olustur");
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(208f, 56f);
        rect.anchoredPosition = posizyon;

        Image buttonImage = buttonObject.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = renk;
            buttonImage.sprite = GetRoundedSprite();
            buttonImage.type = Image.Type.Sliced;
        }

        if (maviGradient)
        {
            UIImageGradient gradient = buttonObject.AddComponent<UIImageGradient>();
            SetGradientColors(gradient, new Color32(96, 165, 250, 255), new Color32(37, 99, 235, 255));
        }

        TMP_Text buttonText = buttonObject.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.text = yazi;
            buttonText.enableAutoSizing = true;
            buttonText.fontSizeMin = 16f;
            buttonText.fontSizeMax = 23f;
            buttonText.color = TextColor;
            buttonText.fontStyle = FontStyles.Bold;
            buttonText.alignment = TextAlignmentOptions.Center;
        }

        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = renk;
        colors.highlightedColor = new Color32((byte)Mathf.Clamp(renk.r + 28, 0, 255), (byte)Mathf.Clamp(renk.g + 28, 0, 255), (byte)Mathf.Clamp(renk.b + 28, 0, 255), 255);
        colors.pressedColor = new Color32((byte)Mathf.Clamp(renk.r - 20, 0, 255), (byte)Mathf.Clamp(renk.g - 20, 0, 255), (byte)Mathf.Clamp(renk.b - 20, 0, 255), 255);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color32(100, 116, 139, 180);
        button.colors = colors;

        buttonObject.AddComponent<UIButtonMicroInteraction>();
        return button;
    }

    private static Sprite GetRoundedSprite()
    {
        return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
    }

    private static void SetGradientColors(UIImageGradient gradient, Color topColor, Color bottomColor)
    {
        SerializedObject serialized = new SerializedObject(gradient);
        serialized.FindProperty("topColor").colorValue = topColor;
        serialized.FindProperty("bottomColor").colorValue = bottomColor;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
