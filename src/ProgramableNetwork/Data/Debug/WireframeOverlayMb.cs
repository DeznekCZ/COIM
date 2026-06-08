using UnityEngine;

namespace ProgramableNetwork.Data.Debug
{
    /// <summary>
    /// Camera-attached MonoBehaviour that toggles Unity's global <c>GL.wireframe</c>
    /// flag around each frame's render pass so every visible mesh is drawn with its
    /// triangle edges instead of filled faces.  Used by the controller inspector's
    /// wireframe button as a "report this mesh issue" diagnostic — the player flips
    /// it on, screenshots whatever rendering glitch they're seeing, then flips it
    /// back off.
    ///
    /// The flag MUST be reset on <c>OnPostRender</c> (not just toggled), otherwise
    /// any UI rendering that comes after also draws wireframe and the HUD becomes
    /// unusable.  The Enabled flag is static so multiple inspector instances share
    /// one truth — the component is only attached once per camera.
    /// </summary>
    public class WireframeOverlayMb : MonoBehaviour
    {
        public static bool Enabled;

        private void OnPreRender()
        {
            if (Enabled)
            {
                GL.wireframe = true;
            }
        }

        private void OnPostRender()
        {
            if (Enabled)
            {
                GL.wireframe = false;
            }
        }
    }
}
