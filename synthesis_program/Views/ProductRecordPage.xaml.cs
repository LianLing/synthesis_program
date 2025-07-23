using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Drawing;
using System.IO;
using System.Collections.ObjectModel;
using Org.BouncyCastle.Utilities.IO;
using synthesis_program.Service;
using System.Threading.Tasks;
using synthesis_program.Models;
using synthesis_program.ViewModels;
using static HtsCommon.DBMySql8.HtsDB;
using System.Windows.Threading;
using System.Collections.Generic;

namespace synthesis_program.Views
{
    /// <summary>
    /// ProductRecordPage.xaml 的交互逻辑
    /// </summary>
    public partial class ProductRecordPage : Page
    {
        private TextBox _comboBoxTextBox;
        TableService tableService = new TableService();
        public ObservableCollection<Prod_TypeModel> allMachineKind { get; set; } = new ObservableCollection<Prod_TypeModel>();
        //数据源
        public ObservableCollection<ProductRecords> SourceList { get; set; } = new ObservableCollection<ProductRecords> { };
        //机型数据源
        public ObservableCollection<string> machineKind { get; set; } = new ObservableCollection<string>();
        private string code = string.Empty;     //机型码
        //订单号集合
        public ObservableCollection<string> orderNoList { get; set; } = new ObservableCollection<string>();
        
        public ProductRecordPage()
        {
            InitializeComponent();
            InitializeModels();
            DataContext = this;
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
        }

        /// <summary>
        ///查订单号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void prod_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (prod_type.SelectedItem != null)
            {
                var result = await tableService.GetOrderNo(prod_type.SelectedItem.ToString());
                foreach (var item in result)
                {
                    orderNoList.Add(item);
                }
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



        private async void QueryButton_Click(object sender, RoutedEventArgs e)
        {
            if(prod_type.SelectedItem == null || (prod_type.SelectedItem == null && Order_No.SelectedItem == null))
            {
                return;
            }
            //string orderNo = prod_type.SelectedItem?.ToString();
            lbl_warning.Visibility = Visibility.Visible;
            lbl_warning.InvalidateVisual();             //强制刷新
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);
            code = allMachineKind.First(p => p.name == prod_type.SelectedItem.ToString()).code;

            var list = await tableService.QueryProductInfo(code, prod_type.SelectedItem.ToString(), Order_No.SelectedItem?.ToString());
            await Task.Run(() =>
            {
                foreach (var item in list)
                {
                    Application.Current.Dispatcher.Invoke(() => SourceList.Add(item));
                }
            });
            
            lbl_warning.Visibility = Visibility.Collapsed;

        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 添加许可证声明
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial; // 非商业用途

                if (SourceList == null || !SourceList.Any())
                {
                    MessageBox.Show("没有数据可以导出！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel文件|*.xlsx",
                    FileName = $"生产编号记录_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (ExcelPackage package = new ExcelPackage())
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("直通率报告");

                        // 设置表头
                        string[] headers = { "日期", "线体", "机型", "生产批号", "批量", "版本", "整机料号", "生产编号"};
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
                            worksheet.Cells[row, 1].Value = item.Date;
                            worksheet.Cells[row, 2].Value = item.LineId;
                            worksheet.Cells[row, 3].Value = item.MODEL;
                            worksheet.Cells[row, 4].Value = item.BatchNo;
                            worksheet.Cells[row, 5].Value = item.COMPLETED_QTY;
                            worksheet.Cells[row, 6].Value = item.Version;
                            worksheet.Cells[row, 7].Value = item.PartNo;
                            worksheet.Cells[row, 8].Value = item.ProductCode;
                            
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

        private void OrderComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox == null) return;

            // 查找内部的 TextBox
            _comboBoxTextBox = comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;
            if (_comboBoxTextBox != null)
            {
                _comboBoxTextBox.TextChanged += ComboBoxOrderNo_TextChanged;
            }
        }

        private void ComboBoxOrderNo_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string searchText = textBox.Text?.Trim();

            ComboBox parentComboBox = FindParentComboBox(textBox);
            if (parentComboBox == null) return;

            if (string.IsNullOrEmpty(searchText))
            {
                parentComboBox.ItemsSource = orderNoList;
                parentComboBox.IsDropDownOpen = false;
            }
            else
            {
                var filteredItems = orderNoList
                    .Where(item => item.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
                parentComboBox.ItemsSource = filteredItems;
                parentComboBox.IsDropDownOpen = true;
            }
        }
    }
}
