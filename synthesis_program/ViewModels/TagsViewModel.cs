using System.ComponentModel;
using System.Collections.ObjectModel;
using synthesis_program.Models;
using System.Windows.Data;

namespace synthesis_program.ViewModels
{
    public class TagsViewModel : INotifyPropertyChanged
    {
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                FilteredTags.Refresh();
            }
        }

        public ObservableCollection<TagsModel> TagsCollection { get; } = new ObservableCollection<TagsModel>();

        private ICollectionView _filteredTags;
        public ICollectionView FilteredTags
        {
            get => _filteredTags ?? (_filteredTags = CollectionViewSource.GetDefaultView(TagsCollection));
            set
            {
                _filteredTags = value;
                OnPropertyChanged(nameof(FilteredTags));
            }
        }

        public TagsViewModel()
        {
            // 初始化过滤条件
            FilteredTags.Filter = FilterTags;
        }

        private bool FilterTags(object obj)
        {
            if (obj is TagsModel tag)
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                    return true;

                return (tag.MaterialId?.Contains(SearchText) ?? false) ||
                       (tag.Creater?.Contains(SearchText) ?? false);
            }

            return false;
        }



        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}