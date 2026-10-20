using System;
namespace VRBat {
public partial class BatGame {
 // Optional observation run holds the demo bat aside, exposing the whole production route.
 readonly bool observeIncoming=Array.IndexOf(Environment.GetCommandLineArgs(),"--observe-incoming")>=0;
}
}
