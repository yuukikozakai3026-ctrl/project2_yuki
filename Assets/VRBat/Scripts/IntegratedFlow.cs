using UnityEngine;

namespace VRBat
{
    public partial class BatGame
    {
        void ResetIntegrated()
        {
            ResetGame(false);
            if (view.menu) Destroy(view.menu);
            view.menu = null;
            phase = "menu";
            phaseAt = now;
        }

        public void StartIntegratedRound()
        {
            if (heightUi) Destroy(heightUi);
            heightUi = null;
            if (current != null) {
                current.Destroy();
                current = null;
            }
            if (incoming != null) {
                incoming.Destroy();
                incoming = null;
            }
            if (view.menu) Destroy(view.menu);
            view.menu = null;
            BeginRun();
        }

    }
}
