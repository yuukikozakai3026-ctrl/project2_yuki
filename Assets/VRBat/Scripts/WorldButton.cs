using System;
using UnityEngine;
using UnityEngine.UI;
namespace VRBat {
public class WorldButton:MonoBehaviour {
 public Action clicked;
 public void BindUI(Button button){button.onClick.AddListener(Click);}
 public void Click(){Debug.Log("[VR UI] Button clicked: "+gameObject.name);clicked?.Invoke();}
}
}
