using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

namespace VRBat
{
    public sealed class VRMenuRay : MonoBehaviour
    {
        XRRayInteractor ray;
        XRInputButtonReader press;

        public static VRMenuRay Create(Transform parent, bool rightHand)
        {
            var root = new GameObject("Tracked menu ray");
            root.SetActive(false);
            root.transform.SetParent(parent, false);
            var menu = root.AddComponent<VRMenuRay>();
            menu.ray = root.AddComponent<XRRayInteractor>();
            menu.ray.rayOriginTransform = root.transform;
            menu.ray.lineType = XRRayInteractor.LineType.StraightLine;
            menu.ray.maxRaycastDistance = 10f;
            menu.ray.enableUIInteraction = true;
            menu.ray.interactionLayers = 0;
            menu.ray.selectInput.inputSourceMode = XRInputButtonReader.InputSourceMode.Unused;
            menu.ray.activateInput.inputSourceMode = XRInputButtonReader.InputSourceMode.Unused;
            menu.press = new XRInputButtonReader("Menu press", inputSourceMode: XRInputButtonReader.InputSourceMode.InputAction);
            var hand = rightHand ? "RightHand" : "LeftHand";
            menu.press.inputActionPerformed.AddBinding("<XRController>{" + hand + "}/trigger").WithInteraction("Press(pressPoint=0.65)");
            menu.press.inputActionValue.AddBinding("<XRController>{" + hand + "}/trigger");
            menu.ray.uiPressInput = menu.press;
            root.SetActive(true);
            return menu;
        }

        public void SetPose(Vector3 position, Quaternion rotation, bool visible)
        {
            transform.SetPositionAndRotation(position, rotation);
            gameObject.SetActive(visible);
        }

        void OnDestroy()
        {
            press?.inputActionPerformed.Dispose();
            press?.inputActionValue.Dispose();
        }
    }
}
