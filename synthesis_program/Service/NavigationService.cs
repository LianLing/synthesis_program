using System.Windows.Controls;

namespace synthesis_program.Service
{
    public class NavigationService
    {
        private Frame _frame;

        public NavigationService() { }

        public void Initialize(Frame frame)
        {
            _frame = frame ?? throw new System.ArgumentNullException(nameof(frame));
        }

        public void NavigateTo(Page page)
        {
            if (_frame == null)
            {
                throw new System.InvalidOperationException("请先调用Initialize方法");
            }
            _frame.Navigate(page);
        }
    }
}