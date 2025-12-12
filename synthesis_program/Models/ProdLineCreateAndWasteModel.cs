using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace synthesis_program.Models
{
    //线束报废表
    [SugarTable("prod_line_CreateAndWaste")]
    public class ProdLineCreateAndWasteModel : INotifyPropertyChanged
    {
        [SugarColumn(IsPrimaryKey = true)]
        public int ID { get; set; }

        public string Prod_Type { get; set; }   //机型码
        public string Prod_Name { get; set; }   //机型名
        public string Station_Code { get; set; }    //站点编码
        public string Station_Name { get; set; }     //站点名

        public string PartNo { get; set; }      //线束料号
        public string Name { get; set; }       //线束名称
        public int IsValid { get; set; } = 1;        //是否可用 0否 1是
        public string Creator { get; set; }      //创建人
        public DateTime Creatime { get; set; }     //创建时间

        public int IsWaste { get; set; } = 0;        //是否报废 0否 1是
        public string Waster { get; set; }      //报废人
        public DateTime WasteTime { get; set; }     //报废时间
        public string Remark { get; set; }      //备注
        public string Extent_Value { get; set; }        //扩展字段

        public int IsUsed { get; set; } = 0;        //是否使用 0否 1是

        
        private bool _isChecked;
        [SugarColumn(IsIgnore = true)]
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged(nameof(IsChecked));
                    
                    if (_isChecked)
                    {
                        IsUsed = 1;
                    }
                    else
                    {
                        IsUsed = 0;
                    }
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
