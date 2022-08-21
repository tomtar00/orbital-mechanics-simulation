using UnityEngine.EventSystems;
using UnityEngine;

public class GUIHoverListener : MonoBehaviour
{
    public static bool focusingOnGUI { get; private set; }
    void Update()
    {
        focusingOnGUI = EventSystem.current.IsPointerOverGameObject();
    }
}