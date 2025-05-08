using System.Windows;
using System.Windows.Controls;
using synthesis_program.Models;
using System.Collections.ObjectModel;
using synthesis_program.Service;
using SqlSugar;
using synthesis_program.DataBase;
using System.Windows.Threading;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Linq;
using System;

namespace synthesis_program.Views
{
    public partial class SaveRecordPage : Page
    {
        private readonly TagService _tagService = new TagService();
        private bool _isEditing = false;
        public ObservableCollection<TagsModel> TagsList { get; set; } = new ObservableCollection<TagsModel>();
        public ObservableCollection<TagsModel> TagsLatestList { get; set; } = new ObservableCollection<TagsModel>();
        public TagsModel TagsListSingle { get; set; }

        public SaveRecordPage()
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
        }
        private bool IsInitialized = false;

        private void LoadData(string keyword = "")
        {
            TagsList.Clear();
            foreach (var tag in _tagService.SearchTags(keyword))
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
                if (_tagService.DeleteTag(selected.Id))
                {
                    TagsList.Remove(selected);
                }
            }
            //LoadData();
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

                using (var tagService = new TagService())
                {
                    // 执行模糊查询（匹配批号或序列号）
                    var results = tagService.SearchTags(searchKeyword);

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

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var addPage = new AddRecordPage();
            addPage.SaveCompleted += () => LoadData();      //订阅新增成功事件
            this.NavigationService?.Navigate(addPage);
        }
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 提交所有未保存的编辑
                tagsDataGrid.CommitEdit();

                // 遍历所有修改的项
                foreach (var item in TagsList)
                {
                    if (item.Id == 0) // 新增记录
                    {
                        // 前端验证必填字段
                        if (string.IsNullOrWhiteSpace(item.MaterialId))
                        {
                            MessageBox.Show("料号不能为空", "验证错误",
                                          MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        if (string.IsNullOrWhiteSpace(item.MachineKind))
                        {
                            MessageBox.Show("机型不能为空", "验证错误",
                                          MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        string sequenceNo = item.SequenceNoStart.Trim();

                        // 业务层重复性检查（防止绕过前端提交重复数据）
                        using (var service = new TagService())
                        {
                            if (service.SequenceExists(sequenceNo))
                            {
                                MessageBox.Show($"序列号 {sequenceNo} 已存在", "验证失败",
                                              MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            if (service.BatchNoExists(item.BatchNo.Trim()))
                            {
                                MessageBox.Show($"批号 {item.BatchNo.Trim()} 已存在", "验证失败",
                                              MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }

                        item.CreateTime = DateTime.Now;
                        item.EditTime = DateTime.Now;
                        item.IsValid = 1;

                        using (var service = new TagService())
                        {
                            if (service.InsertTag(item))
                            {
                                MessageBox.Show("添加成功", "提示",
                                              MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("添加失败", "错误",
                                              MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    else // 修改记录
                    {
                        using (var service = new TagService())
                        {
                            if (!service.CheckRepeatSequenceNoStart(item.SequenceNoStart, item.Id))
                            {
                                MessageBox.Show($"序列号 {item.MaterialId} 已存在", "验证失败",
                                              MessageBoxButton.OK, MessageBoxImage.Error);
                                LoadData();
                                return;
                            }
                            // 重置修改标记
                            item.IsModified = false;

                            // 执行更新
                            if (!service.UpdateTag(item))
                            {
                                MessageBox.Show($"更新记录ID:{item.Id}失败");
                            }
                        }
                    }
                }

                MessageBox.Show($"成功保存记录");
                _isEditing = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败：{ex.Message}");
            }
            LoadData();
        }

        private void ShowLatestData(object sender, SelectionChangedEventArgs e)
        {
            if (TagsListSingle == null)
            {
                TagsLatestList.Clear();
                return;
            }
            if (TagsListSingle.Id == 0)     //防止新增报错
            {
                return;
            } 
            var result = _tagService.GetLatestData(TagsListSingle);
            if (!TagsLatestList.Any(p => p.Id.Equals(result.Id)))
            {
                TagsLatestList.Clear();

                TagsLatestList.Add(result);
            }
        }
    }
}