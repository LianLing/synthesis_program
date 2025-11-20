using MiscApi;
using SqlSugar;
using synthesis_program.DataBase;
using synthesis_program.Interface;
using synthesis_program.Models;
using synthesis_program.Service;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace synthesis_program.Views
{
    // 实现语言刷新接口
    public partial class SaveRecordPage : Page, ILanguageRefreshable
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
                    // 初始加载语言
                    RefreshTexts();
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
                        // 使用翻译方法
                        MessageBox.Show(Misc.t("未找到匹配的记录"), Misc.t("提示"),
                                       MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (SqlSugar.SqlSugarException ex)
            {
                // 使用翻译方法
                MessageBox.Show($"{Misc.t("数据库查询失败")}：{ex.Message}", Misc.t("错误"),
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (tagsDataGrid.SelectedItem == null)
            {
                // 使用翻译方法
                MessageBox.Show(Misc.t("请先选择要编辑的记录"));
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
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
                            // 使用翻译方法
                            MessageBox.Show(Misc.t("料号不能为空"), Misc.t("验证错误"),
                                          MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        if (string.IsNullOrWhiteSpace(item.MachineKind))
                        {
                            // 使用翻译方法
                            MessageBox.Show(Misc.t("机型不能为空"), Misc.t("验证错误"),
                                          MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        string sequenceNo = item.SequenceNoStart.Trim();

                        // 业务层重复性检查（防止绕过前端提交重复数据）
                        using (var service = new TagService())
                        {
                            if (service.SequenceExists(sequenceNo))
                            {
                                // 使用翻译方法
                                MessageBox.Show($"{Misc.t("序列号")} {sequenceNo} {Misc.t("已存在")}", Misc.t("验证失败"),
                                              MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            if (service.BatchNoExists(item.BatchNo.Trim()))
                            {
                                // 使用翻译方法
                                MessageBox.Show($"{Misc.t("批号")} {item.BatchNo.Trim()} {Misc.t("已存在")}", Misc.t("验证失败"),
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
                                // 使用翻译方法
                                MessageBox.Show(Misc.t("添加成功"), Misc.t("提示"),
                                              MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                // 使用翻译方法
                                MessageBox.Show(Misc.t("添加失败"), Misc.t("错误"),
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
                                // 使用翻译方法
                                MessageBox.Show($"{Misc.t("序列号")} {item.MaterialId} {Misc.t("已存在")}", Misc.t("验证失败"),
                                              MessageBoxButton.OK, MessageBoxImage.Error);
                                LoadData();
                                return;
                            }
                            // 重置修改标记
                            item.IsModified = false;

                            // 执行更新
                            if (!service.UpdateTag(item))
                            {
                                // 使用翻译方法
                                MessageBox.Show($"{Misc.t("更新记录ID")}:{item.Id}{Misc.t("失败")}");
                            }
                        }
                    }
                }

                // 使用翻译方法
                MessageBox.Show(Misc.t("成功保存记录"));
                _isEditing = false;
            }
            catch (Exception ex)
            {
                // 使用翻译方法
                MessageBox.Show($"{Misc.t("保存失败")}：{ex.Message}");
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

        // 实现语言刷新接口
        public void RefreshTexts()
        {
            // 刷新页面标题
            this.Title = Misc.t("标签管理");

            // 刷新搜索框提示
            searchBox.Tag = Misc.t("输入料号或创建人");

            // 刷新按钮文本
            SearchButton.Content = Misc.t("查询");
            AddButton.Content = Misc.t("新增");
            DeleteButton.Content = Misc.t("删除");
            EditButton.Content = Misc.t("修改");
            SaveButton.Content = Misc.t("保存");

            // 刷新第一个DataGrid列标题
            if (tagsDataGrid.Columns.Count > 0)
            {
                tagsDataGrid.Columns[0].Header = Misc.t("ID");
                tagsDataGrid.Columns[1].Header = Misc.t("机型");
                tagsDataGrid.Columns[2].Header = Misc.t("生产批号");
                tagsDataGrid.Columns[3].Header = Misc.t("批量");
                tagsDataGrid.Columns[4].Header = Misc.t("版本");
                tagsDataGrid.Columns[5].Header = Misc.t("整机料号");
                tagsDataGrid.Columns[6].Header = Misc.t("生产编号开始");
                tagsDataGrid.Columns[7].Header = Misc.t("生产编号结束");
                tagsDataGrid.Columns[8].Header = Misc.t("模板地址");
                tagsDataGrid.Columns[9].Header = Misc.t("序列号已生成");
                tagsDataGrid.Columns[10].Header = Misc.t("关联机型");
                tagsDataGrid.Columns[11].Header = Misc.t("创建人");
                tagsDataGrid.Columns[12].Header = Misc.t("创建时间");
                tagsDataGrid.Columns[13].Header = Misc.t("修改人");
                tagsDataGrid.Columns[14].Header = Misc.t("修改时间");
                tagsDataGrid.Columns[15].Header = Misc.t("备注");
            }

            // 刷新第二个DataGrid列标题
            if (tagsLatestData.Columns.Count > 0)
            {
                tagsLatestData.Columns[0].Header = Misc.t("ID");
                tagsLatestData.Columns[1].Header = Misc.t("机型");
                tagsLatestData.Columns[2].Header = Misc.t("生产批号");
                tagsLatestData.Columns[3].Header = Misc.t("批量");
                tagsLatestData.Columns[4].Header = Misc.t("版本");
                tagsLatestData.Columns[5].Header = Misc.t("整机料号");
                tagsLatestData.Columns[6].Header = Misc.t("生产编号开始");
                tagsLatestData.Columns[7].Header = Misc.t("生产编号结束");
                tagsLatestData.Columns[8].Header = Misc.t("模板地址");
                tagsLatestData.Columns[9].Header = Misc.t("序列号已生成");
                tagsLatestData.Columns[10].Header = Misc.t("关联机型");
                tagsLatestData.Columns[11].Header = Misc.t("创建人");
                tagsLatestData.Columns[12].Header = Misc.t("创建时间");
                tagsLatestData.Columns[13].Header = Misc.t("修改人");
                tagsLatestData.Columns[14].Header = Misc.t("修改时间");
                tagsLatestData.Columns[15].Header = Misc.t("备注");
            }
        }
    }
}