using DataStorage;
using UnityEngine;

namespace UI.Game.Popups
{
    public class ObligatoryActionWindowPositioner : MonoBehaviour
    {
        void Update()
        {
            // Commented out to prevent the window from being shifted/pushed to the center
            /*
            var t = transform;
            var pos = t.localPosition;
            pos.x = GameManager.PopupManager.PopupOffset/2;
            t.localPosition = pos;
            */
        }
    }
}