using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using synthesis_program.Models;

namespace synthesis_program.ViewModels
{
    public class ProductPassRateViewModel : ProductPassRateModel
    {
        //月份
        public string Month { get; set; }
        //周别
        public string Week { get; set; }
        //日期
        public string Date { get; set; }
        //线体
        public string Line { get; set; }
        //班别
        public string Shift { get; set; }

        public string IPQC { get; set; }
        public string PQE { get; set; }
        //制令号
        public string Monumber { get; set; }
       public string Status {  get; set; }
        //版型
        public string Version { get; set; }
        public string MachineKind { get; set; }
        //直通率目标
        public string TargetRate { get; set; }
        //产品直通率
        public string PassRate { get; set; }
        //检验数
        public int CheckCount { get; set; }
        //合格数
        public int CosmeticPassCount { get; set; }
        //性能不良数
        public int ErrorCount { get; set; }
        //外观不良数
        public int CosmeticErrorCount { get; set; }
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
        //TOP1外观
        public string Top1Surface { get; set; }
        //修理原因
        public string RepairReason_1 { get; set; }
        //数量
        public string Count_1 { get; set; }
        //TOP2外观
        public string Top2Surface { get; set; }
        //修理原因
        public string RepairReason_2 { get; set; }
        //数量
        public string Count_2 { get; set; }
        //TOP3外观
        public string Top3Surface { get; set; }
        //修理原因
        public string RepairReason_3 { get; set; }
        //数量
        public string Count_3 { get; set; }

    }
}
