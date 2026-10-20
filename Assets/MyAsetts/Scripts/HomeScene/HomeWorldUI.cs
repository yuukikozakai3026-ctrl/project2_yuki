using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.UI;
using VRBat;

/// <summary>
/// HomeScene用の3Dワールド空間UI構築ヘルパー
/// </summary>
public static class HomeWorldUI
{
    public static GameObject Panel(Camera camera, Font font, string title, string detail,
        string buttonText, Action onClick)
    {
        var root = new GameObject("Home presentation", typeof(RectTransform), typeof(Canvas));
        root.transform.SetParent(camera.transform, false);
        root.transform.localPosition = new Vector3(0, -.05f, 2.2f);
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one * .001f;

        var rect = root.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(1100, 640);

        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = camera;

        root.AddComponent<TrackedDeviceGraphicRaycaster>();

        var background = root.AddComponent<Image>();
        background.color = new Color(.055f, .12f, .18f, .94f);
        background.raycastTarget = false;

        AddText(rect, font, title, new Vector2(0, 180), new Vector2(980, 120), 76,
            new Color(1, .85f, .52f));
        AddText(rect, font, detail, new Vector2(0, 30), new Vector2(980, 180), 56, Color.white);

        var button = new GameObject("Action", typeof(RectTransform), typeof(Image));
        button.transform.SetParent(rect, false);
        var br = button.GetComponent<RectTransform>();
        br.anchoredPosition = new Vector2(0, -195);
        br.sizeDelta = new Vector2(450, 135);

        var buttonImage = button.GetComponent<Image>();
        buttonImage.color = new Color(.09f, .52f, .62f, 1);

        var uiButton = button.AddComponent<Button>();
        uiButton.targetGraphic = buttonImage;
        var colors = uiButton.colors;
        colors.highlightedColor = new Color(1.25f, 1.4f, 1.4f, 1);
        uiButton.colors = colors;
        uiButton.onClick.AddListener(() => onClick?.Invoke());

        var text = AddText(br, font, buttonText, Vector2.zero, br.sizeDelta, 60, Color.white);

        // 3Dレイキャスト検出用コライダーを付与（十分な厚みを持たせてクリック抜けを防止）
        var collider = text.gameObject.AddComponent<BoxCollider>();
        collider.size = new Vector3(450, 135, 50);
        collider.center = Vector3.zero;

        var worldButton = text.gameObject.AddComponent<WorldButton>();
        worldButton.clicked = onClick;
        worldButton.BindUI(uiButton);

        return root;
    }

    static Text AddText(RectTransform parent, Font font, string value, Vector2 at,
        Vector2 size, int fontSize, Color color)
    {
        var go = new GameObject(value, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = at;
        rect.sizeDelta = size;
        var text = go.GetComponent<Text>();
        text.font = font ? font : Resources.GetBuiltinResource<Font>("Arial.ttf") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }
}

/// <summary>
/// HomeSceneでUIを操作するためのポインター処理（PCマウス & VRコントローラー対応）
/// </summary>
public class HomePointer : MonoBehaviour
{
    Camera cameraToUse;
    bool lastTrigger;

    void Update()
    {
        if (!cameraToUse) cameraToUse = Camera.main;
        if (!cameraToUse) return;

        // PCマウス操作（新旧両Inputシステム対応）
        bool mouseClicked = false;
        Vector2 mousePosition = Vector2.zero;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) {
            mouseClicked = true;
            mousePosition = Mouse.current.position.ReadValue();
        } else {
            try {
                if (Input.GetMouseButtonDown(0)) {
                    mouseClicked = true;
                    mousePosition = Input.mousePosition;
                }
            } catch {
                // 新Input System専用設定の場合の例外を吸収
            }
        }

        if (mouseClicked) {
            var viewport = cameraToUse.pixelRect;
            var rayPosition = new Vector2(
                viewport.x + mousePosition.x * viewport.width / Screen.width,
                viewport.y + mousePosition.y * viewport.height / Screen.height
            );
            Click(cameraToUse.ScreenPointToRay(rayPosition));
        }

        // VRデバイスがアクティブでない場合はVR判定をスキップ
        if (!XRSettings.isDeviceActive) return;

        var hand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (!hand.isValid) return;

        bool trigger = hand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out bool pressed) && pressed;
        if (hand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out float triggerValue)) {
            trigger |= triggerValue >= .65f;
        }

        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;
        bool hasPosition = hand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out position);
        bool hasRotation = hand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out rotation);

        if (trigger && !lastTrigger && hasPosition && hasRotation) {
            var trackingSpace = cameraToUse.transform.parent;
            Vector3 origin = trackingSpace ? trackingSpace.TransformPoint(position) : position;
            Vector3 direction = trackingSpace
                ? trackingSpace.TransformDirection(rotation * Vector3.forward)
                : rotation * Vector3.forward;

            Click(new Ray(origin, direction));
        }

        lastTrigger = trigger;
    }

    static void Click(Ray ray)
    {
        foreach (var hit in Physics.RaycastAll(ray, 10f)) {
            var button = hit.collider.GetComponent<WorldButton>()
                ?? hit.collider.GetComponentInParent<WorldButton>();
            if (!button) continue;

            Debug.Log($"[HomePointer] ボタンをクリックしました: {hit.collider.name}");
            button.Click();
            break;
        }
    }
}
