using Managers;

namespace Interfaces
{
    public interface IUIPanel
    {
        UIState AssociatedState { get; }
        void Show();
        void Hide();
    }
}