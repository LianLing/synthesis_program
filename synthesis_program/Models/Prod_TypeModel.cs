using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace synthesis_program.Models
{
    [SugarTable("prod_type")]
    public class Prod_TypeModel
    {
        public int id { get; set; }
        public string MyProperty { get; set; }
        public string prod_class { get; set; }
        public string code { get; set; }
        public string part_no { get; set; }
        public string name { get; set; }
        public string alias { get; set; }

        public string prod_factory_code { get; set; }
        public string erp_type { get; set; }
        public int log_level { get; set; }

        public string ext_info { get; set; }
        public int status { get; set; }
        public string note { get; set; }

        public string factory_code { get; set; }
        public DateTime imp_time { get; set; }
        public string stat_date { get; set; }


    }
}
