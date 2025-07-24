using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace synthesis_program.Models
{
    [SugarTable("station")]
    public class StationStatusModel
    {
        public int id { get; set; }
        public string code { get; set; }
        public int status { get; set; }
        public string prod_type { get; set; }
        public string remark { get; set; }
        public string extend_value { get; set; }

    }
}
