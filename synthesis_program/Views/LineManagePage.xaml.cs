using HtsCommon.DBMySql8;
using MiscApi;
using SqlSugar;
using synthesis_program.DataBase;
using synthesis_program.Interface;
using synthesis_program.Models;
using synthesis_program.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Threading;
using static HtsCommon.DBMySql8.HtsDB;

namespace synthesis_program.Views
{
    // 实现语言刷新接口
    public partial class LineManagePage : Page, INotifyPropertyChanged
    {
        //站点
        public ObservableCollection<ProdStationModel> Stations { get; set; } = new ObservableCollection<ProdStationModel>();
        //选择的站点
        public ObservableCollection<string> stationSelected { get; set; } = new ObservableCollection<string>();


        //机型数据源
        public ObservableCollection<Prod_TypeModel> allMachineKind { get; set; } = new ObservableCollection<Prod_TypeModel>();
        //选择的机型
        public ObservableCollection<string> machineKind { get; set; } = new ObservableCollection<string>();

        string prodType = string.Empty;     //机型码
        string station = string.Empty;      //站点码

        private readonly TableService dbService = new TableService();
        public TagService tagService = new TagService();
        private bool _isEditing = false;
        public ObservableCollection<ProdLineManageModel> LineMaterialList { get; set; } = new ObservableCollection<ProdLineManageModel>();
        public ProdLineManageModel LineSelectedSingle { get; set; }
        private TextBox _comboBoxTextBox;
        public LineManagePage()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                if (!IsInitialized)
                {
                    //初始化机型
                    InitializeProdTypes();
                    IsInitialized = true;
                }
            };
            DataContext = this;
        }
        private bool IsInitialized = false;

        public event PropertyChangedEventHandler PropertyChanged;

        private async Task InitializeProdTypes()
        {
            // 初始化机型数据
            var result = await dbService.QueryProdType();
            if (result != null)
            {
                foreach (var item in result)
                {
                    machineKind.Add(item.name);
                    allMachineKind.Add(item);
                }
                ProdTypecmb.ItemsSource = machineKind;
            }
        }

        private void LoadData(string keyword = "")
        {
            LineMaterialList.Clear();
            foreach (var tag in dbService.SearchLines(keyword))
            {
                LineMaterialList.Add(tag);
            }
        }

        private void ModelComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox == null) return;

            // 查找内部的 TextBox
            _comboBoxTextBox = comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;
            if (_comboBoxTextBox != null)
            {
                _comboBoxTextBox.TextChanged += ComboBoxTextBox_TextChanged;
            }
        }

        private void ComboBoxTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string searchText = textBox.Text?.Trim();

            ComboBox parentComboBox = FindParentComboBox(textBox);
            if (parentComboBox == null) return;

            if (string.IsNullOrEmpty(searchText))
            {
                parentComboBox.ItemsSource = machineKind;
                parentComboBox.IsDropDownOpen = false;
            }
            else
            {
                var filteredItems = machineKind
                    .Where(item => item.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
                parentComboBox.ItemsSource = filteredItems;
                parentComboBox.IsDropDownOpen = true;
            }
        }

        // 辅助方法：通过控件树查找父级 ComboBox
        private ComboBox FindParentComboBox(DependencyObject child)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            while (parent != null && !(parent is ComboBox))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as ComboBox;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadData(searchBox.Text);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            lblWarning.Content = "";
            if (linesDataGrid.SelectedItem is ProdLineManageModel selected && selected.ID > 0)
            {
                if (dbService.DeleteLines(selected.ID, HtsDB.User.sName + "[" + HtsDB.User.sID + "]"))
                {
                    LineMaterialList.Remove(selected);
                }
            }
            lblWarning.Content = Misc.t("删除成功");
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

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lblWarning.Content = "";
                LineMaterialList.Clear();
                if (string.IsNullOrEmpty(prodType))
                {
                    MessageBox.Show(Misc.t("请选择机型"), Misc.t("提示"),
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                //if (string.IsNullOrEmpty(station))
                //{
                //    MessageBox.Show(Misc.t("请选择站点"), Misc.t("提示"),
                //                   MessageBoxButton.OK, MessageBoxImage.Warning);
                //    return;
                //}

                var itemSource = await dbService.QueryLinesAsync(prodType, station);
                foreach (var item in itemSource)
                {
                    LineMaterialList.Add(item);
                }
                lblWarning.Content = Misc.t("查询完成");
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
            lblWarning.Content = "";
            if (linesDataGrid.SelectedItem == null)
            {
                // 使用翻译方法
                MessageBox.Show(Misc.t("请先选择要编辑的记录"));
                return;
            }
            // 切换编辑状态
            _isEditing = true;

            // 启动编辑模式
            linesDataGrid.BeginEdit();

            // 自动聚焦到第一个可编辑单元格
            Dispatcher.BeginInvoke((Action)(() =>
            {
                var cellInfo = new DataGridCellInfo(
                    linesDataGrid.SelectedItem,
                    linesDataGrid.Columns[4]); // 聚焦到料号列
                linesDataGrid.CurrentCell = cellInfo;
                linesDataGrid.BeginEdit();
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
                lblWarning.Content = "";
                // 提交所有未保存的编辑
                linesDataGrid.CommitEdit();

                // 遍历所有修改的项
                foreach (var item in LineMaterialList)
                {
                    if (item.ID == 0) // 新增记录
                    {
                        // 前端验证必填字段
                        if (string.IsNullOrWhiteSpace(item.PartNo))
                        {
                            // 使用翻译方法
                            MessageBox.Show(Misc.t("料号不能为空"), Misc.t("验证错误"),
                                          MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        

                        string partNo = item.PartNo.Trim();

                        // 业务层重复性检查（防止绕过前端提交重复数据）
                        using (var service = new TableService())
                        {
                            if (service.PartNoExists(partNo,station))
                            {
                                // 使用翻译方法
                                MessageBox.Show($"{Misc.t("当前站点料号")} {partNo} {Misc.t("已存在")}", Misc.t("验证失败"),
                                              MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            //if (service.BatchNoExists(item.BatchNo.Trim()))
                            //{
                            //    // 使用翻译方法
                            //    MessageBox.Show($"{Misc.t("批号")} {item.BatchNo.Trim()} {Misc.t("已存在")}", Misc.t("验证失败"),
                            //                  MessageBoxButton.OK, MessageBoxImage.Error);
                            //    return;
                            //}
                        }

                        //新增记录时，自动带入机型、站点名、站点码、创建人、创建时间等信息
                        item.Prod_Type = prodType;
                        item.Prod_Name = this.ProdTypecmb.SelectedItem.ToString();
                        if (string.IsNullOrEmpty(this.Station.SelectedItem.ToString()))
                        {
                            MessageBox.Show(Misc.t("站点未选择"), Misc.t("警告"),
                                              MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                        item.Station = this.Station.SelectedItem.ToString();
                        item.Station_Code = station;
                        item.IsValid = 1;
                        item.Creatime = DateTime.Now;
                        item.Editime = DateTime.Now;
                        item.Creator = HtsDB.User.sName + "[" + HtsDB.User.sID + "]";
                        item.Editor = HtsDB.User.sName + "[" + HtsDB.User.sID + "]";

                        using (var service = new TableService())
                        {
                            if (service.InsertTag(item))
                            {
                                // 使用翻译方法
                                //MessageBox.Show(Misc.t("添加成功"), Misc.t("提示"),MessageBoxButton.OK, MessageBoxImage.Information);
                                lblWarning.Content = Misc.t("添加成功");
                            }
                            else
                            {
                                // 使用翻译方法
                                MessageBox.Show(Misc.t("添加失败"), Misc.t("错误"),MessageBoxButton.OK, MessageBoxImage.Error);
                                //lblWarning.Content = Misc.t("添加失败");
                            }
                        }
                    }
                    else // 修改记录
                    {
                        using (var service = new TableService())
                        {
                            if (!service.CheckRepeatPartNoStart(item.PartNo, item.ID))
                            {
                                // 使用翻译方法
                                MessageBox.Show($"{Misc.t("料号")} {item.PartNo} {Misc.t("已存在")}", Misc.t("验证失败"),
                                              MessageBoxButton.OK, MessageBoxImage.Error);
                                LoadData();
                                return;
                            }
                            // 重置修改标记
                            item.IsModified = false;
                            item.Editime = DateTime.Now;
                            item.Editor = HtsDB.User.sName + "[" + HtsDB.User.sID + "]";
                            // 执行更新
                            if (!service.UpdateTag(item))
                            {
                                // 使用翻译方法
                                MessageBox.Show($"{Misc.t("更新记录ID")}:{item.ID}{Misc.t("失败")}");
                            }
                            else
                            {
                                lblWarning.Content = Misc.t("保存成功");
                            }
                        }
                    }
                }

                // 使用翻译方法
                //MessageBox.Show(Misc.t("成功保存记录"));
                _isEditing = false;
            }
            catch (Exception ex)
            {
                // 使用翻译方法
                MessageBox.Show($"{Misc.t("保存失败")}：{ex.Message}");
            }

            var itemSource = await dbService.QueryLinesAsync(prodType, station);
            LineMaterialList.Clear();
            foreach (var item in itemSource)
            {
                LineMaterialList.Add(item);
            }
        }

        //private void ShowLatestData(object sender, SelectionChangedEventArgs e)
        //{
        //    if (LineSelectedSingle == null)
        //    {
        //        LineMaterialList.Clear();
        //        return;
        //    }
        //    if (LineSelectedSingle.ID == 0)     //防止新增报错
        //    {
        //        return;
        //    }
        //    var result = dbService.GetLatestData(LineMaterialList);
        //    if (!LineMaterialList.Any(p => p.ID.Equals(result.Id)))
        //    {
        //        LineMaterialList.Clear();
        //        LineMaterialList.Add(result);
        //    }
        //}


        // 实现语言刷新接口
        public void RefreshTexts()
        {
            // 刷新页面标题
            this.Title = Misc.t("标签管理");

            // 刷新搜索框提示
            searchBox.Tag = Misc.t("输入料号或创建人");

            // 刷新按钮文本
            SearchButton.Content = Misc.t("查询");
            DeleteButton.Content = Misc.t("删除");
            EditButton.Content = Misc.t("修改");
            SaveButton.Content = Misc.t("保存");

            // 刷新第一个DataGrid列标题
            //if (linesDataGrid.Columns.Count > 0)
            //{
            //    linesDataGrid.Columns[0].Header = Misc.t("ID");
            //    linesDataGrid.Columns[1].Header = Misc.t("机型");
            //    linesDataGrid.Columns[11].Header = Misc.t("创建人");
            //    linesDataGrid.Columns[12].Header = Misc.t("创建时间");
            //    linesDataGrid.Columns[13].Header = Misc.t("修改人");
            //    linesDataGrid.Columns[14].Header = Misc.t("修改时间");
            //    linesDataGrid.Columns[15].Header = Misc.t("备注");
            //}

        }

        private async void ProdTypecmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (allMachineKind.Count > 0)
            {
                prodType = ProdTypecmb.SelectedItem != null ? allMachineKind.First(p => p.name == ProdTypecmb.SelectedItem.ToString()).code : string.Empty;
            }
            if (!string.IsNullOrEmpty(prodType))
            {
                var result = await dbService.GetStationsByProdType(prodType);
                if (result != null)
                {
                    this.Station.ItemsSource = result.Select(p => p.name).ToList();
                    stationSelected.Add("");
                    foreach (var item in result)
                    {
                        Stations.Add(item);
                        stationSelected.Add(item.name + " " + "—" + " " + item.code);
                    }
                    this.Station.ItemsSource = stationSelected;
                    this.Station.SelectedIndex = 0;
                }
            }

        }

        private void Station_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.Station.SelectedItem != null)
            {
                station = this.Station.SelectedItem.ToString().Substring(this.Station.SelectedItem.ToString().LastIndexOf(' ') + 1);
            }
        }

       
    }
}