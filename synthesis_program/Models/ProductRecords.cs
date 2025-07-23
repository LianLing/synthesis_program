using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace synthesis_program.Models
{
    public class ProductRecords
    {
        //工单
        public string  Mo { get; set; }
        //日期
        public string Date { get; set; }
        //线体
        public string LineId { get; set; }
        //机型
        public string MODEL { get; set; }
        //生产批号
        public string BatchNo { get; set; }
        //批量
        public string COMPLETED_QTY { get; set; }
        //版本
        public string Version { get; set; }
        //整机料号
        public string PartNo { get; set; }
        //生产编号
        public string ProductCode { get; set; }
    }
}
