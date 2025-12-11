using DocumentFormat.OpenXml.Office2010.Ink;
using HtsCommon.DBMySql8;
using Microsoft.Win32;
using MiscApi;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SqlSugar;
using synthesis_program.DataBase;
using synthesis_program.Interface;
using synthesis_program.Models;
using synthesis_program.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
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
    public partial class GenerateLineNumberPage : Page, INotifyPropertyChanged
    {
        //站点
        public ObservableCollection<ProdStationModel> Stations { get; set; } = new ObservableCollection<ProdStationModel>();
        //选择的站点
        public ObservableCollection<string> stationSelected { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<ProdLineCreateAndWasteModel> newCreateLabel { get; set; } = new ObservableCollection<ProdLineCreateAndWasteModel>();
        //机型数据源
        public ObservableCollection<Prod_TypeModel> allMachineKind { get; set; } = new ObservableCollection<Prod_TypeModel>();
        //选择的机型
        public ObservableCollection<string> machineKind { get; set; } = new ObservableCollection<string>();
        List<string> generatedCodes = new List<string>();           //生成的码列表,导出用
        string prodType = string.Empty;     //机型码
        string station = string.Empty;      //站点码

        private readonly TableService dbService = new TableService();
        public TagService tagService = new TagService();
        private bool _isEditing = false;
        public ObservableCollection<ProdLineManageModel> LineMaterialList { get; set; } = new ObservableCollection<ProdLineManageModel>();
        public ProdLineManageModel LineSelectedSingle { get; set; }
        private TextBox _comboBoxTextBox;
        public GenerateLineNumberPage()
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


        public void RefreshTexts()
        {
            // 刷新页面标题
            this.Title = Misc.t("标签管理");

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

        private async void Station_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.Station.SelectedItem != null)
            {
                station = this.Station.SelectedItem.ToString().Substring(this.Station.SelectedItem.ToString().LastIndexOf(' ') + 1);
            }

            //查询料号生成初始码
            if (!string.IsNullOrEmpty(prodType) && !string.IsNullOrEmpty(station))
            {
                var lineNumberList = await dbService.GetPartNoListByTypeAndStation(prodType, station);
                this.PartNoPrefixComboBox.ItemsSource = lineNumberList;
            }
        }

        private async void PartNoPrefixComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StartCodeTextBox.Clear();
            newCreateLabel.Clear();
            string partNo = this.PartNoPrefixComboBox.SelectedItem?.ToString();
            //查询料号生成初始码
            if (!string.IsNullOrEmpty(prodType) && !string.IsNullOrEmpty(station))
            {
                var lineNumber = await dbService.GetLineNumberByTypeAndStation(prodType, station,partNo);
                if (!string.IsNullOrEmpty(lineNumber))
                {
                    StartCodeTextBox.Text = (lineNumber.Substring(lineNumber.LastIndexOf('-') + 1).ObjToInt() + 1).ToString("D4");
                }
                else
                {
                    StartCodeTextBox.Text = "0001";
                }

                //查询该料的所有标签
                var labelList = await dbService.GetLabelsByPartNo(prodType, station, partNo);
                foreach (var item in labelList)
                {
                    newCreateLabel.Add(item);
                }
            }
        }

        private async void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            string quantityText = QuantityTextBox.Text.Trim();      //数量
            int quantity = string.IsNullOrEmpty(quantityText)? 0 : quantityText.ObjToInt();           //转换数量
            int startNumber = Convert.ToInt32(StartCodeTextBox.Text.Trim());        //起始码
            string startCode = StartCodeTextBox.Text.Trim();            //起始码文本
            

            if (string.IsNullOrEmpty(quantityText) || quantity == 0)
            {
                MessageBox.Show(Misc.t("请输入正确的数量！"));
                return;
            }
            else
            {
                newCreateLabel.Clear();
                generatedCodes.Clear();
                //查询料号信息
                var partNoInfo = await dbService.GetPartNoInfoByTypeAndStation(prodType, station, PartNoPrefixComboBox.SelectedItem.ToString());


                for (int i = 0; i < quantity; i++)
                {
                    string code = (startNumber + i).ToString("D4");
                    string fullCode = $"{partNoInfo.PartNo}-{code}";
                    generatedCodes.Add(fullCode);
                }

                foreach (var code in generatedCodes)
                {
                    ProdLineCreateAndWasteModel newLabel = new ProdLineCreateAndWasteModel
                    {
                        Prod_Type = prodType,
                        Prod_Name = partNoInfo.Prod_Name,
                        Station_Code = partNoInfo.Station_Code,
                        Station_Name = partNoInfo.Station,
                        PartNo = code,
                        Name = partNoInfo.Name,
                        IsValid = 1,
                        Creator = HtsDB.User.sName + "[" + HtsDB.User.sID + "]",
                        Creatime = DateTime.Now,
                        IsWaste = 0,
                        Waster = "",
                        WasteTime = DateTime.MinValue,
                        Remark = "",
                        Extent_Value = ""
                    };
                    newCreateLabel.Add(newLabel);
                    dbService.InsertLineCreateAndWaste(newLabel);       //批量插入数据库
                }

                StartCodeTextBox.Text = (quantity + startNumber).ToString("D4");
            }
        }


        //将生成的序列号导出到Excel
        // 将生成的序列号导出到Excel
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                if (newCreateLabel == null || !newCreateLabel.Any())
                {
                    MessageBox.Show(Misc.t("没有数据可以导出！"), Misc.t("提示"),
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel文件|*.xlsx",
                    FileName = $"{Misc.t("序列号_")}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (ExcelPackage package = new ExcelPackage())
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(Misc.t("序列号列表"));

                        // 设置表头（使用翻译后文本）
                        string[] headers = {
                            Misc.t("机型码"), Misc.t("机型名"), Misc.t("站点码"), Misc.t("站点名"),
                            Misc.t("料号"), Misc.t("名称"), Misc.t("创建人"), Misc.t("创建时间")
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
                            headerRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }

                        // 填充数据
                        int row = 2;
                        foreach (var item in newCreateLabel)
                        {
                            worksheet.Cells[row, 1].Value = item.Prod_Type;
                            worksheet.Cells[row, 2].Value = item.Prod_Name;
                            worksheet.Cells[row, 3].Value = item.Station_Code;
                            worksheet.Cells[row, 4].Value = item.Station_Name;
                            worksheet.Cells[row, 5].Value = item.PartNo;
                            worksheet.Cells[row, 6].Value = item.Name;
                            worksheet.Cells[row, 7].Value = item.Creator;
                            worksheet.Cells[row, 8].Value = item.Creatime.ToString();

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
    }
}