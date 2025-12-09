using Sdl.MultiSelectComboBox.API;
using System;

namespace Sdl.MultiSelectComboBox.WinUI.Services
{
    public sealed class WinUIFilterService : IFilterService
    {
        private string _filterCriteria;

        public Predicate<object> Filter { get; set; }

        public void SetFilter(string criteria)
        {
            _filterCriteria = criteria;

            // Default filter implementation
            if (string.IsNullOrEmpty(_filterCriteria))
            {
                Filter = null;
            }
            else
            {
                Filter = item =>
                {
                    if (item == null)
                        return false;

                    return item.ToString().IndexOf(_filterCriteria, StringComparison.OrdinalIgnoreCase) >= 0;
                };
            }
        }
    }
}
