
// This script should be attached to a GameObject in the scene to ensure that the XUIHoverUtility's Update method is called every frame.

using UnityEngine;
namespace XUtils.UIUtils
{
    public class XUIHoverUtilityUpdater : MonoBehaviour { void Update() => XUIHoverUtility.Update(); }
}