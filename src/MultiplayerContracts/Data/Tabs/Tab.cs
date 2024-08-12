using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;

namespace MultiplayerContracts
{
    public class Tab
    {
        public Tab(ToggleBtn tabButton, IUiElement tabContent)
        {
            TabButton = tabButton;
            TabContent = tabContent;
        }

        public ToggleBtn TabButton { get; }
        public IUiElement TabContent { get; }
    }
}