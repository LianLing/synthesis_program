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

        private async Task InitializeModels()
        {
            // 初始化机型数据
            var result = await tableService.QueryMachineKind();
            await Task.Run(() =>
            {
                foreach (var item in result)
                {
                    machineKind.Add(item.name);
                    allMachineKind.Add(item);
                }
            });
            
            ////初始化工单信息
            //var allMo = await tableService.QueryAllMoAsync(prod_type.SelectedItem.ToString());
            //forechAdd(allMo, AllMo);
            //初始化班组信息
            var allTeam = await tableService.QueryAllTeam();
            Teams.Add("");
            forechAdd(allTeam, Teams);
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
            string stationstr = "(";
            var selectedStations = Stations.Where(s => s.IsChecked)
                                           .Select(s => s.Name)
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
                finished_stamp = datePick.SelectedDate,
                prod_team = team.SelectedItem?.ToString(),
                station_curr = stationstr,
                pass_rate = "0"
            };

           var list = await tableService.QueryPassRate(passRateModel);
            await Task.Run(() =>
            {
                foreach (var item in list)
                {
                    Application.Current.Dispatcher.Invoke(() => RateList.Add(item));
                }
            });
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
                    checkBoxItem.Name = module;
                    Stations.Add(checkBoxItem);
                }

            }
            //初始化工单信息
            if (prod_type.SelectedItem != null)
            {
                //string code = allMachineKind.First(p => p.name == prod_type.SelectedItem.ToString()).code;
                var allMo = await tableService.QueryAllMoAsync(code);
                if (allMo != null)
                {
                    forechAdd(allMo, AllMo);
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

                if (RateList == null || !RateList.Any())
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
                        string[] headers = { "机型", "模组", "工艺", "站点", "工单", "班组", "直通率" };
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
                        foreach (var item in RateList)
                        {
                            worksheet.Cells[row, 1].Value = item.prod_type;
                            worksheet.Cells[row, 2].Value = item.process_grp_curr;
                            worksheet.Cells[row, 3].Value = item.model_curr;
                            worksheet.Cells[row, 4].Value = item.station_curr;
                            worksheet.Cells[row, 5].Value = item.mo;
                            worksheet.Cells[row, 6].Value = item.prod_team;

                            // 直通率单元格特殊处理
                            var rateCell = worksheet.Cells[row, 7];
                            rateCell.Value = item.pass_rate;

                            // 当直通率<95%时设置红色背景
                            if (!item.pass_rate.Contains("NaN") &&　Convert.ToInt32(item.pass_rate.Substring(0, item.pass_rate.Length - 1)) < 95)
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
    }

    // 站点选择辅助类
    public class CheckBoxItem
    {
        public string Name { get; set; }
        public bool IsChecked { get; set; }
    }
}
