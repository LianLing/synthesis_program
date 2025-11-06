using HtsCommon.DBMySql8;
using Microsoft.Win32;
using MiscApi;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SqlSugar;
using synthesis_program.Interface;
using synthesis_program.Models;
using synthesis_program.Service;
using synthesis_program.Tools; // 引入翻译工具类
using synthesis_program.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using static HtsCommon.DBMySql8.HtsDB;

namespace synthesis_program.Views
{
    // 实现语言刷新接口
    public partial class ProductPassRate : Page, ILanguageRefreshable
    {
        // 机型数据源
        public ObservableCollection<string> machineKind { get; set; } = new ObservableCollection<string>();
        // 班组
        public ObservableCollection<string> Teams { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<Prod_TypeModel> allMachineKind { get; set; } = new ObservableCollection<Prod_TypeModel>();
        // 工单
        public ObservableCollection<string> AllMo { get; set; } = new ObservableCollection<string> { };
        // 线别
        public ObservableCollection<string> AllLine { get; set; } = new ObservableCollection<string> { };
        // 数据源
        public ObservableCollection<ProductPassRateViewModel> SourceList { get; set; } = new ObservableCollection<ProductPassRateViewModel> { };

        TableService tableService = new TableService();
        private TextBox _comboBoxTextBox;
        private string code = string.Empty;
        private List<string> stations;

        public ProductPassRate()
        {
            InitializeComponent();
            DataContext = this;
            InitializeModels();
            lblWarning.Visibility = Visibility.Collapsed;
            // 初始化语言
            Loaded += (s, e) => RefreshTexts();
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

            // 初始化班组信息（仅南京工厂）
            if (HtsDB.Servers.sFactoryCode.ToUpper() == "NJ")
            {
                var allTeam = tableService.QueryAllTeam();
                Teams.Add("");
                foreach (var item in allTeam)
                {
                    Teams.Add(item);
                }
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
            lblWarning.Visibility = Visibility.Visible;
            lblWarning.InvalidateVisual();             // 强制刷新
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);

            string stationstr = "(";

            if (stations == null || stations.Count == 0)
            {
                // 使用翻译方法
                MessageBox.Show(Misc.t("该机型未维护站点"), Misc.t("提示"),
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                lblWarning.Visibility = Visibility.Collapsed;
                return;
            }

            if (stations.Count != 0)
            {
                foreach (var item in stations)
                {
                    stationstr = stationstr + "'" + item + "',";
                }
                stationstr = stationstr.Substring(0, stationstr.Length - 1) + ")";
            }

            // 构建查询参数对象
            ProductPassRateModel passRateModel = new ProductPassRateModel()
            {
                prod_type = code,
                mo = mo.SelectedItem?.ToString(),
                finished_stamp = datePick.SelectedDate?.ObjToDate(),
                prod_team = team.SelectedItem?.ToString(),
                station_curr = stationstr,
                pass_rate = "0",
                line_id = line.SelectedItem?.ToString()
            };

            var list = await tableService.QueryPassRate(passRateModel, code);
            SourceList.Clear(); // 清空现有数据
            foreach (var item in list)
            {
                SourceList.Add(item);
            }
            lblWarning.Visibility = Visibility.Collapsed;
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

        private async void MachineSelectChanged(object sender, SelectionChangedEventArgs e)
        {
            stations = new List<string>();
            if (prod_type.SelectedItem != null)
            {
                code = allMachineKind.First(p => p.name == prod_type.SelectedItem.ToString()).code;

                var StationStr = await tableService.QueryStationsByProdType(code);
                if (!string.IsNullOrEmpty(StationStr))
                {
                    var StationList = StationStr.Split(',');
                    foreach (var item in StationList)
                    {
                        stations.Add(item);
                    }
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
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                if (SourceList == null || !SourceList.Any())
                {
                    MessageBox.Show(Misc.t("没有数据可以导出！"), Misc.t("提示"),
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel文件|*.xlsx",
                    FileName = $"{Misc.t("产品直通率")}_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (ExcelPackage package = new ExcelPackage())
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(Misc.t("直通率报告"));

                        // 设置表头（使用翻译后文本）
                        string[] headers = {
                            Misc.t("月份"), Misc.t("周别"), Misc.t("日期"), Misc.t("线体"),
                            Misc.t("班别"), Misc.t("IPQC"), Misc.t("PQE"), Misc.t("制令号"),
                            Misc.t("状态"), Misc.t("机型"), Misc.t("版型"), Misc.t("直通率目标"),
                            Misc.t("产品直通率"), Misc.t("检验数"), Misc.t("外观合格数"),
                            Misc.t("性能不良数"), Misc.t("外观不良数"), Misc.t("TOP1（性能）"),
                            Misc.t("修理原因"), Misc.t("数量"), Misc.t("TOP2（性能）"),
                            Misc.t("修理原因"), Misc.t("数量"), Misc.t("TOP3（性能）"),
                            Misc.t("修理原因"), Misc.t("数量"), Misc.t("TOP1（外观）"),
                            Misc.t("修理原因"), Misc.t("数量"), Misc.t("TOP2（外观）"),
                            Misc.t("修理原因"), Misc.t("数量"), Misc.t("TOP3（外观）"),
                            Misc.t("修理原因"), Misc.t("数量")
                        };
                        for (int i = 0; i < headers.Length; i++)
                        {
                            worksheet.Cells[1, i + 1].Value = headers[i];
                        }

                        // 表头样式
                        using (ExcelRange headerRange = worksheet.Cells[1, 1, 1, headers.Length])
                        {
                            headerRange.Style.Font.Bold = true;
                            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                            headerRange.AutoFilter = true;
                        }

                        // 填充数据
                        int row = 2;
                        foreach (var item in SourceList)
                        {
                            worksheet.Cells[row, 1].Value = item.Month;
                            worksheet.Cells[row, 2].Value = item.Week;
                            worksheet.Cells[row, 3].Value = item.Date;
                            worksheet.Cells[row, 4].Value = item.Line;
                            worksheet.Cells[row, 5].Value = item.Shift;
                            worksheet.Cells[row, 6].Value = item.IPQC;
                            worksheet.Cells[row, 7].Value = item.PQE;
                            worksheet.Cells[row, 8].Value = item.Monumber;
                            worksheet.Cells[row, 9].Value = item.Status;
                            worksheet.Cells[row, 10].Value = item.MachineKind;
                            worksheet.Cells[row, 11].Value = item.Version;
                            worksheet.Cells[row, 12].Value = item.TargetRate;
                            worksheet.Cells[row, 13].Value = item.PassRate;
                            worksheet.Cells[row, 14].Value = item.CheckCount;
                            worksheet.Cells[row, 15].Value = item.CosmeticPassCount;
                            worksheet.Cells[row, 16].Value = item.ErrorCount;
                            worksheet.Cells[row, 17].Value = item.CosmeticErrorCount;
                            worksheet.Cells[row, 18].Value = item.Top1Capcity;
                            worksheet.Cells[row, 19].Value = item.RepairReason1;
                            worksheet.Cells[row, 20].Value = item.Count1;
                            worksheet.Cells[row, 21].Value = item.Top2Capcity;
                            worksheet.Cells[row, 22].Value = item.RepairReason2;
                            worksheet.Cells[row, 23].Value = item.Count2;
                            worksheet.Cells[row, 24].Value = item.Top3Capcity;
                            worksheet.Cells[row, 25].Value = item.RepairReason3;
                            worksheet.Cells[row, 26].Value = item.Count3;
                            worksheet.Cells[row, 27].Value = item.Top1Surface;
                            worksheet.Cells[row, 28].Value = item.RepairReason_1;
                            worksheet.Cells[row, 29].Value = item.Count_1;
                            worksheet.Cells[row, 30].Value = item.Top2Surface;
                            worksheet.Cells[row, 31].Value = item.RepairReason_2;
                            worksheet.Cells[row, 32].Value = item.Count_2;
                            worksheet.Cells[row, 33].Value = item.Top3Surface;
                            worksheet.Cells[row, 34].Value = item.RepairReason_3;
                            worksheet.Cells[row, 35].Value = item.Count_3;

                            // 直通率单元格特殊处理
                            var rateCell = worksheet.Cells[row, 12];
                            rateCell.Value = item.pass_rate;

                            // 当直通率<95%时设置红色背景
                            if (!item.pass_rate.Contains("NaN") &&
                                Convert.ToInt32(item.pass_rate.Substring(0, item.pass_rate.Length - 1)) < 95)
                            {
                                rateCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                rateCell.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFC7CE"));
                            }

                            row++;
                        }

                        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                        FileInfo excelFile = new FileInfo(saveFileDialog.FileName);
                        package.SaveAs(excelFile);

                        MessageBox.Show(Misc.t("导出成功！"), Misc.t("提示"),
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Misc.t("导出失败")}：{ex.Message}", Misc.t("错误"),
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void prod_type_DropDownOpened(object sender, EventArgs e)
        {
            // 保持原逻辑
        }

        private async void datePick_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // 初始化工单信息
            if (prod_type.SelectedItem != null && datePick != null && datePick.SelectedDate.HasValue)
            {
                var allMo = await tableService.QueryAllMoAsync(code, datePick.SelectedDate.Value);
                AllMo.Clear();
                AllMo.Add("");
                forechAdd(allMo, AllMo);
            }
            else
            {
                lblWarning.Content = Misc.t("请先选择机型！");
                return;
            }

            // 初始化线别信息
            if (prod_type.SelectedItem != null && datePick != null && datePick.SelectedDate.HasValue)
            {
                var allLine = await tableService.QueryAllLineAsync(code, datePick.SelectedDate.Value);
                AllLine.Clear();
                AllLine.Add("");
                forechAdd(allLine, AllLine);
            }
            else
            {
                lblWarning.Content = Misc.t("请先选择机型！");
                return;
            }
        }

        private async void mo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 保持原逻辑
        }

        // 站点选择辅助类
        public class CheckBoxItem
        {
            public string Name { get; set; }
            public bool IsChecked { get; set; }
        }

        // 实现语言刷新接口
        public void RefreshTexts()
        {
            // 表单标签
            tbMachineKind.Text = Misc.t("机型");
            tbMo.Text = Misc.t("工单");
            tbLine.Text = Misc.t("线别");
            tbTeam.Text = Misc.t("班组");

            // 按钮文本
            btnQuery.Content = Misc.t("查询");
            btnExport.Content = Misc.t("导出");

            // 提示文本
            lblWarning.Content = Misc.t("正在查询，请勿重复点击!");

            // DataGrid列头（确保列已初始化）
            if (rateDataGrid.Columns.Count > 0)
            {
                colMonth.Header = Misc.t("月份");
                colWeek.Header = Misc.t("周别");
                colDate.Header = Misc.t("日期");
                colLine.Header = Misc.t("线体");
                colShift.Header = Misc.t("班别");
                colIPQC.Header = Misc.t("IPQC");
                colPQE.Header = Misc.t("PQE");
                colMonumber.Header = Misc.t("制令号");
                colStatus.Header = Misc.t("状态");
                colMachineKind.Header = Misc.t("机型");
                colVersion.Header = Misc.t("版型");
                colTargetRate.Header = Misc.t("直通率目标");
                colPassRate.Header = Misc.t("产品直通率");
                colCheckCount.Header = Misc.t("检验数");
                colCosmeticPassCount.Header = Misc.t("外观合格数");
                colErrorCount.Header = Misc.t("性能不良数");
                colCosmeticErrorCount.Header = Misc.t("外观不良数");
                colTop1Capcity.Header = Misc.t("TOP1（性能）");
                colRepairReason1.Header = Misc.t("修理原因");
                colCount1.Header = Misc.t("数量");
                colTop2Capcity.Header = Misc.t("TOP2（性能）");
                colRepairReason2.Header = Misc.t("修理原因");
                colCount2.Header = Misc.t("数量");
                colTop3Capcity.Header = Misc.t("TOP3（性能）");
                colRepairReason3.Header = Misc.t("修理原因");
                colCount3.Header = Misc.t("数量");
                colTop1Surface.Header = Misc.t("TOP1（外观）");
                colRepairReason_1.Header = Misc.t("修理原因");
                colCount_1.Header = Misc.t("数量");
                colTop2Surface.Header = Misc.t("TOP2（外观）");
                colRepairReason_2.Header = Misc.t("修理原因");
                colCount_2.Header = Misc.t("数量");
                colTop3Surface.Header = Misc.t("TOP3（外观）");
                colRepairReason_3.Header = Misc.t("修理原因");
                colCount_3.Header = Misc.t("数量");
            }
            else
            {
                // 若列未加载，延迟刷新
                Dispatcher.BeginInvoke((Action)RefreshTexts, TimeSpan.FromMilliseconds(100));
            }
        }
    }
}