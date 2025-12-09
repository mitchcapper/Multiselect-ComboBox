using System.ComponentModel;

namespace Sdl.MultiSelectComboBox.Models
{
    /// <summary>
    /// Represents an item in the MultiSelectComboBox with selection state
    /// </summary>
    public class SelectableItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        private string _displayText;

        /// <summary>
        /// The original data item
        /// </summary>
        public object Item { get; set; }

        /// <summary>
        /// Indicates whether this item is currently selected
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        /// <summary>
        /// The text to display for this item
        /// </summary>
        public string DisplayText
        {
            get => _displayText;
            set
            {
                if (_displayText != value)
                {
                    _displayText = value;
                    OnPropertyChanged(nameof(DisplayText));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
