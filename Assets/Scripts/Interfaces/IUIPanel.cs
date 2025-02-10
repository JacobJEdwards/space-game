using Managers;

namespace Interfaces
{
public interface IUIPanel
{
    void Show();
    void Hide();
    UIState AssociatedState { get; }
}
}