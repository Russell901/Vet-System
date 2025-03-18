using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

namespace Vet_System.Services
{
    public interface INavigationService
    {
        bool CanGoBack { get; }
        bool NavigateTo(Type pageType, object parameter = null, bool clearNavigation = false);
        bool GoBack();
        void Initialize(Frame frame);
        event EventHandler<Type> Navigated;
    }

    public class NavigationService : INavigationService
    {
        private Frame _frame;
        private object _lastParameterUsed;
        private readonly Stack<Type> _pageStack;

        public event EventHandler<Type> Navigated;

        public bool CanGoBack => _frame?.CanGoBack ?? false;

        public NavigationService(Frame frame)
        {
            _frame = frame ?? throw new ArgumentNullException(nameof(frame));
            _pageStack = new Stack<Type>();
            _frame.Navigated += Frame_Navigated;
        }

        public void Initialize(Frame frame)
        {
            if (_frame != null)
            {
                _frame.Navigated -= Frame_Navigated;
            }

            _frame = frame ?? throw new ArgumentNullException(nameof(frame));
            _frame.Navigated += Frame_Navigated;
        }

        public bool NavigateTo(Type pageType, object parameter = null, bool clearNavigation = false)
        {
            if (_frame == null)
                throw new InvalidOperationException("Frame not initialized.");

            if (pageType == null)
                throw new ArgumentNullException(nameof(pageType));

            // Check if we're already on the page we're trying to navigate to
            bool isSamePageWithDifferentParameter =
                _frame.Content?.GetType() == pageType &&
                (parameter != null && !parameter.Equals(_lastParameterUsed));

            if (_frame.Content?.GetType() != pageType || isSamePageWithDifferentParameter)
            {
                if (clearNavigation)
                {
                    _frame.BackStack.Clear();
                    _pageStack.Clear();
                }

                var result = _frame.Navigate(pageType, parameter);
                if (result)
                {
                    _lastParameterUsed = parameter;
                    _pageStack.Push(pageType);
                    Navigated?.Invoke(this, pageType);
                }
                return result;
            }

            return false;
        }

        public bool GoBack()
        {
            if (CanGoBack)
            {
                _frame.GoBack();
                _lastParameterUsed = null;
                if (_pageStack.Count > 0)
                {
                    _pageStack.Pop();
                    var previousPage = _pageStack.Count > 0 ? _pageStack.Peek() : null;
                    if (previousPage != null)
                    {
                        Navigated?.Invoke(this, previousPage);
                    }
                }
                return true;
            }
            return false;
        }

        //public void SetListDataItemForNextConnectedAnimation(object item)
        //{
        //    _frame?.SetListDataItemForNextConnectedAnimation(item);
        //}

        private void Frame_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            if (e.NavigationMode == Microsoft.UI.Xaml.Navigation.NavigationMode.Back && _pageStack.Count > 0)
            {
                _pageStack.Pop();
            }
        }

        public Type CurrentPage => _frame?.Content?.GetType();

        public void ClearNavigationHistory()
        {
            _frame?.BackStack.Clear();
            _pageStack.Clear();
            _lastParameterUsed = null;
        }
    }
}
