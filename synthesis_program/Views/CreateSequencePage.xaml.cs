using System.Windows;
using System.Windows.Controls;
using synthesis_program.Models;
using System.Collections.ObjectModel;
using synthesis_program.Service;
using SqlSugar;
using System.Windows.Threading;
using System.Linq;
using System;

namespace synthesis_program.Views
{
    public partial class CreateSequencePage : Page
    {
        private readonly SequenceService sequenceService = new SequenceService();
        private bool _isEditing = false;
        public ObservableCollection<TagsModel> TagsList { get; set; } = new ObservableCollection<TagsModel>();
        public TagsModel TagsListSingle { get; set; }

        public CreateSequencePage()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                if (!IsInitialized)
                {
                    // 初始化数据加载
                    LoadData();
                    IsInitialized = true;
                }
            };
            DataContext = this;
            //if (!string.IsNullOrEmpty(TagsListSingle.SequenceNo))
            //{
            //    //ToDO:如果所选项存在初始序列号，显示到文本框中
            //}
        }
        private bool IsInitialized = false;

        private void LoadData(string keyword = "")
        {
            TagsList.Clear();
            foreach (var tag in sequenceService.SearchTags(keyword))
            {
                TagsList.Add(tag);
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadData(searchBox.Text);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (tagsDataGrid.SelectedItem is TagsModel selected && selected.Id > 0)
            {
                if (sequenceService.DeleteTag(selected.Id))
                {
                    TagsList.Remove(selected);
                }
            }
        }

        private void ShowAsDialog()
        {
            var window = new Window
            {
                Content = this,
                SizeToContent = SizeToContent.WidthAndHeight
            };
            window.ShowDialog();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 从TextBox获取搜索条件
                string searchKeyword = searchBox.Text.Trim();

                using (var sequenceService = new SequenceService())
                {
                    // 执行模糊查询（匹配料号或创建人）
                    var results = sequenceService.SearchSequence(searchKeyword);

                    // 清空现有数据
                    TagsList.Clear();

                    // 绑定查询结果到UI
                    foreach (var tag in results)
                    {
                        TagsList.Add(tag);
                    }

                    // 如果没有结果提示
                    if (!results.Any())
                    {
                        MessageBox.Show("未找到匹配的记录", "提示",
                                       MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (SqlSugar.SqlSugarException ex)
            {
                MessageBox.Show($"数据库查询失败：{ex.Message}", "错误",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var addPage = new AddRecordPage();
            addPage.SaveCompleted += () => LoadData();      //订阅新增成功事件
            this.NavigationService?.Navigate(addPage);
        }


        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (tagsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("请先选择要编辑的记录");
                return;
            }
            // 切换编辑状态
            _isEditing = true;

            // 启动编辑模式
            tagsDataGrid.BeginEdit();

            // 自动聚焦到第一个可编辑单元格
            Dispatcher.BeginInvoke((Action)(() =>
            {
                var cellInfo = new DataGridCellInfo(
                    tagsDataGrid.SelectedItem,
                    tagsDataGrid.Columns[2]); // 聚焦到模板名称列
                tagsDataGrid.CurrentCell = cellInfo;
                tagsDataGrid.BeginEdit();
            }), DispatcherPriority.Background);
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 提交所有未保存的编辑
                tagsDataGrid.CommitEdit();

                // 获取变更记录
                var changedItem = TagsListSingle;

                using (var service = new TagService())
                {
                    if (!service.CheckRepeatSequenceNoStart(changedItem.SequenceNoStart, changedItem.Id))
                    {
                        MessageBox.Show($"料号 {changedItem.SequenceNoStart} 已存在", "验证失败",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                        LoadData();
                        return;
                    }
                    // 重置修改标记
                    changedItem.IsModified = false;

                        // 执行更新
                        if (!service.UpdateTag(changedItem))
                        {
                            MessageBox.Show($"更新记录ID:{changedItem.Id}失败");
                        }
                    
                }

                MessageBox.Show($"成功保存1条记录");
                _isEditing = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败：{ex.Message}");
            }
            LoadData();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {

        }

        //private void CreateButton_Click(object sender, RoutedEventArgs e)
        //{
        //    int count = Convert.ToInt32(txt_quality.Text.Trim());
        //    List<TagsModel> SequenceList = new List<TagsModel>();
        //    string sequenceText = text1.Text + '-' + text2.Text + '-' + text3.Text + '-';
        //    //string[] arrText = sequenceText.Split(new char[] { '-'});
        //    for (int i = 0; i < count; i++)
        //    {
        //        SequenceList[i].SequenceNo = sequenceText + (Convert.ToInt32(text4.Text) + i).ToString();
        //    }
        //   // sequenceService.InsertSequences()
        //}
    }
}