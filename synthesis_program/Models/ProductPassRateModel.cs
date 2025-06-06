using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace synthesis_program.Models
{
    [SugarTable("prod_test_rcds")]
    public class ProductPassRateModel 
    {
        //机型
        public string prod_type { get; set; }
        //模组
        public string process_grp_curr { get; set; }
        //工艺
        public string model_curr { get; set; }
        //站点
        public string station_curr { get; set; }
        //工单
        public string mo { get; set; }
        //班组
        public string prod_team { get; set; }
        //时间
        public DateTime? finished_stamp { get; set; }
        //直通率
        public string pass_rate { get; set; }
        public string sn { get; set; }
        public int status { get; set; }
        public int tst_rlt { get; set; }
        public string line_id { get; set; }

    }
}
