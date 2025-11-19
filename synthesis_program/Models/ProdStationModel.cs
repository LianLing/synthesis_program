using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace synthesis_program.Models
{
    [SugarTable("prod_station")]
    public class ProdStationModel
    {
        public int id { get; set; }
        public string prod_type { get; set; }
        public string code { get; set; }
        public string name { get; set; }
    }
}
