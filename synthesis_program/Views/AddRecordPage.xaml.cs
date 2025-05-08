using System.Windows;
using System.Windows.Controls;
using synthesis_program.Models;
using synthesis_program.Service;
using Microsoft.Win32;
using System;

namespace synthesis_program.Views
{
    public partial class AddRecordPage : Page
    {
        private readonly TagService _tagService = new TagService();
        public event Action SaveCompleted;
        // 文件类型过滤器
        private const string ImageFilter =
            "图像文件|*.jpg;*.jpeg;*.png;*.bmp|所有文件|*.*";
        private const string TemplateFilter =
            "模板文件|*.qdf;*.tpl;*.template|所有文件|*.*";
        public AddRecordPage()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (!System.IO.File.Exists(txtPicture.Text))
                //{
                //    MessageBox.Show("图纸文件不存在！");
                //    return;
                //}

                // 第一步：前端验证必填字段
                if (string.IsNullOrWhiteSpace(txtMaterialId.Text))
                {
                    MessageBox.Show("料号不能为空", "验证错误",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtMachineKind.Text))
                {
                    MessageBox.Show("机型不能为空", "验证错误",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                ////检查序列号格式
                //if (!txtSequenceNo.Text.Contains('-'))
                //{
                //    MessageBox.Show("序列号格式不正确", "序列号错误",
                //                  MessageBoxButton.OK, MessageBoxImage.Warning);
                //    return;
                //}

                string sequenceNo = txtSequenceNoStart.Text.Trim();

                // 第二步：业务层重复性检查（防止绕过前端提交重复数据）
                using (var service = new TagService())
                {
                    if (service.SequenceExists(sequenceNo))
                    {
                        MessageBox.Show($"序列号 {sequenceNo} 已存在", "验证失败",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    if (service.BatchNoExists(txtBatchNo.Text.Trim()))
                    {
                        MessageBox.Show($"批号 {txtBatchNo.Text.Trim()} 已存在", "验证失败",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                }

                //string[] sequenceStr = txtSequenceNo.Text.Split('-');
                //string sequenceText = string.Empty;
                //foreach (var p in sequenceStr)
                //{
                //    sequenceText += p;
                //}

                // 第三步：构建实体
                var newTag = new TagsModel
                {
                    MachineKind = txtMachineKind.Text?.Trim(),
                    BatchNo = txtBatchNo.Text?.Trim(),
                    BatchCount = txtBatchCount.Text?.Trim(),
                    Version = txtVersion.Text?.Trim(),
                    MaterialId = txtMaterialId.Text?.Trim(),
                    SequenceNoStart = txtSequenceNoStart.Text?.Trim(),
                    SequenceNoEnd = txtSequenceNoEnd.Text?.Trim(),
                    ModelAddress = txtModelAddress.Text?.Trim(),
                    ConnectMachine = txtConnectMachine.Text?.Trim(),
                    Remark = txtRemark.Text?.Trim(),
                    IsValid = 1,
                    IsCreated = IsCreated.IsChecked == true ? 1 : 0,
                    Creater = txtCreater.Text?.Trim(),
                    CreateTime = DateTime.Now,
                    EditTime = DateTime.Now
                };

                //var newSequence = new SequenceModel
                //{
                //    SequenceNo = txtSequenceNo.Text?.Trim(),
                //    UpdateRate = txtUpdateRate.Text?.Trim(),
                //    NumberIsEnd = NumberIsEnd.IsChecked == true ? 1 : 0,
                //    Remark = txtRemark.Text?.Trim(),
                //    CreateTime = DateTime.Now,
                //    EditTime = DateTime.Now,
                //    isValid = 1
                //};

                // 第四步：保存数据
                using (var service = new TagService())
                {
                    if (service.InsertTag(newTag))
                    {
                        SaveCompleted?.Invoke();
                        MessageBox.Show("添加成功", "提示",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                        NavigationService?.GoBack();
                    }
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败：{ex.Message}", "错误",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        

        //private void BrowsePicture_Click(object sender, RoutedEventArgs e)
        //{
        //    var dialog = new OpenFileDialog
        //    {
        //        Title = "选择图纸文件",
        //        Filter = ImageFilter,
        //        CheckFileExists = true
        //    };

        //    if (dialog.ShowDialog() == true)
        //    {
        //        txtPicture.Text = dialog.FileName;
        //    }
        //}

        private void BrowseModel_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "选择模板文件",
                Filter = TemplateFilter,
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
            {
                txtModelAddress.Text = dialog.FileName;
            }
        }

    }
}