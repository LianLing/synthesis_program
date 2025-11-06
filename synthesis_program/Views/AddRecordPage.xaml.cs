using MiscApi;
using synthesis_program.Interface;
using synthesis_program.Models;
using synthesis_program.Service;
using synthesis_program.Tools;
using System;
using System.Windows;
using System.Windows.Controls;

namespace synthesis_program.Views
{
    // 实现语言刷新接口
    public partial class AddRecordPage : Page, ILanguageRefreshable
    {
        public event Action SaveCompleted; // 保存完成事件
        private readonly TagService _tagService = new TagService();
        public TagsModel CurrentTag { get; set; } = new TagsModel();

        public AddRecordPage()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += (s, e) =>
            {
                // 初始化语言
                RefreshTexts();
                // 初始化默认值
                CurrentTag.Creater = Environment.UserName;
                CurrentTag.CreateTime = DateTime.Now;
            };
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 验证必填字段
                if (string.IsNullOrWhiteSpace(CurrentTag.MachineKind))
                {
                    MessageBox.Show(Misc.t("机型不能为空"), Misc.t("验证错误"),
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(CurrentTag.BatchNo))
                {
                    MessageBox.Show(Misc.t("生产批号不能为空"), Misc.t("验证错误"),
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(CurrentTag.SequenceNoStart))
                {
                    MessageBox.Show(Misc.t("生产编号开始不能为空"), Misc.t("验证错误"),
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 检查批号是否已存在
                if (_tagService.BatchNoExists(CurrentTag.BatchNo.Trim()))
                {
                    MessageBox.Show($"{Misc.t("生产批号")} {CurrentTag.BatchNo} {Misc.t("已存在")}",
                                  Misc.t("保存失败"), MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 设置默认值
                CurrentTag.EditTime = DateTime.Now;
                CurrentTag.Editor = Environment.UserName;
                CurrentTag.IsValid = 1;

                // 保存数据
                if (_tagService.InsertTag(CurrentTag))
                {
                    MessageBox.Show(Misc.t("新增记录成功"), Misc.t("提示"),
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    SaveCompleted?.Invoke(); // 触发保存完成事件
                    NavigationService?.GoBack(); // 返回上一页
                }
                else
                {
                    MessageBox.Show(Misc.t("新增记录失败，请重试"), Misc.t("错误"),
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Misc.t("保存时发生错误")}：{ex.Message}", Misc.t("错误"),
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack(); // 返回上一页
        }

        // 实现语言刷新接口
        public void RefreshTexts()
        {
            // 页面标题
            this.Title = Misc.t("新增生产记录");

            // 表单标签
            lblMachineKind.Content = Misc.t("机型：");
            lblBatchNo.Content = Misc.t("生产批号：");
            lblBatchCount.Content = Misc.t("批量：");
            lblVersion.Content = Misc.t("版本：");
            lblMaterialId.Content = Misc.t("整机料号：");
            lblSequenceStart.Content = Misc.t("生产编号开始：");
            lblSequenceEnd.Content = Misc.t("生产编号结束：");
            lblModelAddress.Content = Misc.t("模板地址：");
            lblIsCreated.Content = Misc.t("序列号已生成：");
            lblConnectMachine.Content = Misc.t("关联机型：");
            lblRemark.Content = Misc.t("备注：");

            // 按钮文本
            SaveButton.Content = Misc.t("保存");
            CancelButton.Content = Misc.t("取消");
        }
    }
}