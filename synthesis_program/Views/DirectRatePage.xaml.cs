using Microsoft.Win32;
using MySqlX.XDevAPI.Common;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SqlSugar;
using synthesis_program.Models;
using synthesis_program.Service;
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
    public partial class DirectRatePage : Page
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
        private TextBox _comboBoxTextBox;
        private string code = string.Empty;


        public DirectRatePage()
        {
            InitializeComponent();
            DataContext = this;
            InitializeModels();
            NoticeMsg.Visibility = Visibility.Collapsed;
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
            NoticeMsg.Visibility = Visibility.Collapsed;
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
                var result = await tableService.QueryStations(code, prod_module.SelectedItem.ToString(), prod_model.SelectedItem.ToString());
                //查询当前机型已勾选的站点
                var checkedStations = await tableService.QueryCheckedStations(code);
                // 将已勾选的站点标记为选中
                if (!string.IsNullOrEmpty(checkedStations))
                {
                    string[] checkedCodes = checkedStations.Split(',');
                    foreach (var module in result)
                    {
                        CheckBoxItem checkBoxItem = new CheckBoxItem();
                        checkBoxItem.Name = module.value2 + " " + module.value1;
                        checkBoxItem.Code = module.value1;
                        foreach (var station in checkedCodes)
                        {
                            if (module.value1 == station)// 如果站点代码匹配，则设置为选中
                            {
                                checkBoxItem.IsChecked = true;
                            }
                        }
                                
                        Stations.Add(checkBoxItem);
                    }
                }
                else
                {
                    foreach (var module in result)
                    {
                        CheckBoxItem checkBoxItem = new CheckBoxItem();
                        checkBoxItem.Name = module.value2 + " " + module.value1;
                        checkBoxItem.Code = module.value1;
                        Stations.Add(checkBoxItem);
                    }
                }
            }
        }

        /// <summary>
        /// 对勾选的站点，维护成默认站点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            NoticeMsg.Visibility = Visibility.Collapsed;

            if (prod_type.SelectedItem == null || prod_module.SelectedItem == null || prod_model.SelectedItem == null)
            {
                NoticeMsg.Content = "请先选择机型、模组和工艺";
                NoticeMsg.Visibility = Visibility.Visible;
                return;
            }
            var checkedStations = Stations.Where(s => s.IsChecked).ToList();
            var result = await tableService.UpdateStationStatus(code, checkedStations);
            if (result == 0)
            {
                NoticeMsg.Content = "该机型重复维护";
            }
            else
            {
                NoticeMsg.Content = "该机型维护完成";
            }
            NoticeMsg.Visibility = Visibility.Visible;
        }

        // 站点选择辅助类
        public class CheckBoxItem
        {
            public string Name { get; set; }
            public string Code { get; set; }
            public bool IsChecked { get; set; }
        }

    }
}
