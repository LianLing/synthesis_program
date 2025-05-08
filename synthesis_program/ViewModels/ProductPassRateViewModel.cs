using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace synthesis_program.ViewModels
{
    internal class ProductPassRateViewModel
    {
        //月份
        public string Month { get; set; }
        //周别
        public string Week { get; set; }
        //日期
        public string Date { get; set; }
        //线体
        public string Line { get; set; }
        //制令号
        public string Monumber { get; set; }
        //状态
        public string Status { get; set; }
        //机型
        public string MachineKind { get; set; }
        //直通率目标
        public string TargetRate { get; set; }
        //产品直通率
        public string PassRate { get; set; }
        //检验数
        public string CheckCount { get; set; }
        //外观合格数
        public string CosmeticPassCount { get; set; }
        //性能不良数
        public string ErrorCount { get; set; }
        //外观不良数
        public string CosmeticErrorCount { get; set; }
        //TOP1(性能)
        public string Top1Capcity { get; set; }
        //修理原因
        public string RepairReason1 { get; set; }
        //数量
        public string Count1 { get; set; }
        //TOP2(性能)
        public string Top2Capcity { get; set; }
        //修理原因
        public string RepairReason2 { get; set; }
        //数量
        public string Count2 { get; set; }
        //TOP3(性能)
        public string Top3Capcity { get; set; }
        //修理原因
        public string RepairReason3 { get; set; }
        //数量
        public string Count3 { get; set; }

    }
}
