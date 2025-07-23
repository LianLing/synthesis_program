using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using synthesis_program.Service;
using synthesis_program.Models;
using System.Windows.Media;
using Microsoft.Win32;
using OfficeOpenXml;
using System.IO;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using static HtsCommon.DBMySql8.HtsDB;
using System.Windows.Threading;
using synthesis_program.ViewModels;
using SqlSugar;

namespace synthesis_program.Views
{
    public partial class ProductPassRate : Page
    {
        //站点
        public ObservableCollection<CheckBoxItem> Stations { get; set; } = new ObservableCollection<CheckBoxItem>();
        //机型数据源
        public ObservableCollection<string> machineKind { get; set; } = new ObservableCollection<string>();
        //模组
        public ObservableCollection<string> Modules { get; set; } = new ObservableCollection<string>();
        public string ModuleSingle { get; set; }
        //工艺
        public ObservableCollection<string> Processes { get; set; } = new ObservableCollection<string>();
        public string processSingle { get; set; }
        //班组
        public ObservableCollection<string> Teams { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<Prod_TypeModel> allMachineKind { get; set; } = new ObservableCollection<Prod_TypeModel>();
        //工单
        public ObservableCollection<string> AllMo { get; set; } = new ObservableCollection<string> { };

        //表单
        public ObservableCollection<ProductPassRateModel> RateList { get; set; } = new ObservableCollection<ProductPassRateModel> { };
        //数据源
        public ObservableCollection<ProductPassRateViewModel> SourceList { get; set; } = new ObservableCollection<ProductPassRateViewModel> { };

        TableService tableService = new TableService();
        ProductPassRateModel rateModel = new ProductPassRateModel();
        private TextBox _comboBoxTextBox;
        private string code = string.Empty;


        public ProductPassRate()
        {
            InitializeComponent();
            DataContext = this;
            InitializeModels();
            lbl_warning.Visibility = Visibility.Collapsed;
        }

        private void InitializeModels()
        {
            // 初始化机型数据
            var result = tableService.QueryMachineKind();

            foreach (var item in result)
            {
                machineKind.Add(item.name);
                allMachineKind.Add(item);
            }

            ////初始化工单信息
            //var allMo = await tableService.QueryAllMoAsync(prod_type.SelectedItem.ToString());
            //forechAdd(allMo, AllMo);
            //初始化班组信息
            var allTeam = tableService.QueryAllTeam();
            Teams.Add("");
            foreach (var item in allTeam)
            {
                Teams.Add(item);
            }
        }

        public void forechAdd(List<string> source, ObservableCollection<string> obj)
        {
            foreach (var item in source)
            {
                obj.Add(item);
            }
        }

        private async void QueryButton_Click(object sender, RoutedEventArgs e)
        {
            lbl_warning.Visibility = Visibility.Visible;
            lbl_warning.InvalidateVisual();             //强制刷新
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);
            string stationstr = "(";
            var selectedStations = Stations.Where(s => s.IsChecked)
                                           .Select(s => s.Name.Substring(s.Name.LastIndexOf(' ')+1))
                                           .ToList();
            if (selectedStations.Count == 0)
            {
                MessageBox.Show("请先选择站点");
                return;
            }
            if (selectedStations.Count != 0)
            {
                foreach (var item in selectedStations)
                {
                    stationstr = stationstr + "'" + item + "',";
                }
                stationstr = stationstr.Substring(0, stationstr.Length - 1) + ")";
            }
            // 构建查询参数对象
            ProductPassRateModel passRateModel = new ProductPassRateModel()
            {
                prod_type = code,
                process_grp_curr = prod_module.SelectedItem?.ToString(),
                model_curr = prod_model.SelectedItem?.ToString(),
                mo = mo.SelectedItem?.ToString(),
                finished_stamp = datePick.SelectedDate?.ObjToDate(),
                prod_team = team.SelectedItem?.ToString(),
                station_curr = stationstr,
                pass_rate = "0"
            };

            var list = await tableService.QueryPassRate(passRateModel, prod_type.SelectedItem.ToString());
            //await Task.Run(() =>
            //{
                foreach (var item in list)
                {
                //Application.Current.Dispatcher.Invoke(() => SourceList.Add(item));
                SourceList.Add(item);
                }
            //});
            lbl_warning.Visibility = Visibility.Collapsed;
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
            // 过滤逻辑
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

        private void MachineSelectChanged(object sender, SelectionChangedEventArgs e)
        {
            Modules.Clear();
            if (prod_type.SelectedItem != null)
            {
                //查询模组数据
                code = allMachineKind.First(p => p.name == prod_type.SelectedItem.ToString()).code;
                var result = tableService.QueryModules(code);
                foreach (var module in result)
                {
                    Modules.Add(module);
                }
            }

        }

        private void prod_module_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Processes.Clear();
            if (prod_type.SelectedItem != null)
            {
                //string code = allMachineKind.First(p => p.name == prod_type.SelectedItem.ToString()).code;
                //查询工艺数据
                var result = tableService.QueryProcesses(code);
                foreach (var module in result)
                {
                    Processes.Add(module);
                }
            }

        }

        private async void prod_model_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Stations.Clear();
            AllMo.Clear();
            if (prod_type.SelectedItem != null && prod_module.SelectedItem != null && prod_model.SelectedItem != null)
            {
                //string code = allMachineKind.First(p => p.name == prod_type.SelectedItem.ToString()).code;
                //查询站点数据
                var result = tableService.QueryStations(code, prod_module.SelectedItem.ToString(), prod_model.SelectedItem.ToString());
                foreach (var module in result)
                {
                    CheckBoxItem checkBoxItem = new CheckBoxItem();
                    checkBoxItem.Name = module.value2 + " " + module.value1;
                    Stations.Add(checkBoxItem);
                }

            }

        }

        private void mo_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox == null) return;

            // 查找内部的 TextBox
            _comboBoxTextBox = comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;
            if (_comboBoxTextBox != null)
            {
                _comboBoxTextBox.TextChanged += MoTextBox_TextChanged;
            }
        }

        private async void MoTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            //textBox.Text = "";
            string searchText = textBox.Text?.Trim();

            ComboBox parentComboBox = FindParentComboBox(textBox);
            if (parentComboBox == null) return;

            if (string.IsNullOrEmpty(searchText))
            {
                parentComboBox.ItemsSource = AllMo;
                parentComboBox.IsDropDownOpen = false;
            }
            else
            {
                var filteredItems = AllMo
                    .Where(item => item.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
                parentComboBox.ItemsSource = filteredItems;
                parentComboBox.IsDropDownOpen = true;
            }
        }


        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 添加许可证声明（必须在所有EPPlus操作之前）
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial; // 非商业用途

                if (SourceList == null || !SourceList.Any())
                {
                    MessageBox.Show("没有数据可以导出！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel文件|*.xlsx",
                    FileName = $"产品直通率_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (ExcelPackage package = new ExcelPackage())
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("直通率报告");

                        // 设置表头
                        string[] headers = { "月份", "周别", "日期", "线体", "IPQC", "PQE", "制令号", "状态", "机型", "版型", "直通率目标", "产品直通率", "检验数", "外观合格数", "性能不良数", "外观不良数", "TOP1（性能）", "修理原因", "数量", "TOP2（性能）", "修理原因", "数量", "TOP3（性能）", "修理原因", "数量", "TOP1（外观）", "修理原因", "数量", "TOP2（外观）", "修理原因", "数量", "TOP3（外观）", "修理原因", "数量" };
                        for (int i = 0; i < headers.Length; i++)
                        {
                            worksheet.Cells[1, i + 1].Value = headers[i];
                        }

                        // 添加表头样式
                        using (ExcelRange headerRange = worksheet.Cells[1, 1, 1, headers.Length])
                        {
                            headerRange.Style.Font.Bold = true;
                            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                            // 启用排序功能（EPPlus 5.6+）
                            headerRange.AutoFilter = true; // 允许Excel客户端排序
                        }

                        // 填充数据并设置条件格式
                        int row = 2;
                        foreach (var item in SourceList)
                        {
                            worksheet.Cells[row, 1].Value = item.Month;
                            worksheet.Cells[row, 2].Value = item.Week;
                            worksheet.Cells[row, 3].Value = item.Date;
                            worksheet.Cells[row, 4].Value = item.Line;
                            worksheet.Cells[row, 5].Value = item.IPQC;
                            worksheet.Cells[row, 6].Value = item.PQE;
                            worksheet.Cells[row, 7].Value = item.Monumber;
                            worksheet.Cells[row, 8].Value = item.Status;
                            worksheet.Cells[row, 9].Value = item.MachineKind;
                            worksheet.Cells[row, 10].Value = item.Version;
                            worksheet.Cells[row, 11].Value = item.TargetRate;
                            worksheet.Cells[row, 12].Value = item.PassRate;
                            worksheet.Cells[row, 13].Value = item.CheckCount;
                            worksheet.Cells[row, 14].Value = item.CosmeticPassCount;
                            worksheet.Cells[row, 15].Value = item.ErrorCount;
                            worksheet.Cells[row, 16].Value = item.CosmeticErrorCount;
                            worksheet.Cells[row, 17].Value = item.Top1Capcity;
                            worksheet.Cells[row, 18].Value = item.RepairReason1;
                            worksheet.Cells[row, 19].Value = item.Count1;
                            worksheet.Cells[row, 20].Value = item.Top2Capcity;
                            worksheet.Cells[row, 21].Value = item.RepairReason2;
                            worksheet.Cells[row, 22].Value = item.Count2;
                            worksheet.Cells[row, 23].Value = item.Top3Capcity;
                            worksheet.Cells[row, 24].Value = item.RepairReason3;
                            worksheet.Cells[row, 25].Value = item.Count3;
                            worksheet.Cells[row, 26].Value = item.Top1Surface;
                            worksheet.Cells[row, 27].Value = item.RepairReason_1;
                            worksheet.Cells[row, 28].Value = item.Count_1;
                            worksheet.Cells[row, 29].Value = item.Top2Surface;
                            worksheet.Cells[row, 30].Value = item.RepairReason_2;
                            worksheet.Cells[row, 31].Value = item.Count_2;
                            worksheet.Cells[row, 32].Value = item.Top3Surface;
                            worksheet.Cells[row, 33].Value = item.RepairReason_3;
                            worksheet.Cells[row, 34].Value = item.Count_3;

                            // 直通率单元格特殊处理
                            var rateCell = worksheet.Cells[row, 12];
                            rateCell.Value = item.pass_rate;

                            // 当直通率<95%时设置红色背景
                            if (!item.pass_rate.Contains("NaN") && Convert.ToInt32(item.pass_rate.Substring(0, item.pass_rate.Length - 1)) < 95)
                            {
                                rateCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                rateCell.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFC7CE"));
                            }

                            row++;
                        }

                        // 设置自适应列宽
                        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                        // 保存文件
                        FileInfo excelFile = new FileInfo(saveFileDialog.FileName);
                        package.SaveAs(excelFile);

                        MessageBox.Show("导出成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void prod_type_DropDownOpened(object sender, EventArgs e)
        {
            //await Task.Run(() => 
            //{
            //    var list = tableService.QueryNewMachineKind();
            //    foreach (var item in allMachineKind)
            //    {
            //        if (item == list.Result.Find(p=>p.code != null).code)
            //        {

            //        }
            //    }
            //});
        }

        private async void datePick_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            //初始化工单信息
            if (prod_type.SelectedItem != null && datePick != null)
            {
                var allMo = await tableService.QueryAllMoAsync(code, (DateTime)datePick.SelectedDate);
                AllMo.Add("");
                forechAdd(allMo, AllMo);
            }
            else
                return;
        }

        // 站点选择辅助类
        public class CheckBoxItem
        {
            public string Name { get; set; }
            public bool IsChecked { get; set; }
        }

        private async void mo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //选择工单带出线别
            //await tableService.GetLineByMo(prod_type.SelectedItem.ToString(), mo.SelectedItem.ToString());
        }

    }
}
