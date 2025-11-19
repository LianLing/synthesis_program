using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace synthesis_program.Models
{
    [SugarTable("Prod_line_Material")]
    public class ProdLineManageModel:INotifyPropertyChanged
    {
        /// <summary>
        /// 主键ID（自增）
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int ID { get; set; }

        /// <summary>
        /// 机型
        /// </summary>
        private string _prod_Type;
        [SugarColumn(ColumnName = "Prod_Type", Length = 255)]
        public string Prod_Type
        {
            get => _prod_Type;
            set
            {
                if (_prod_Type != value)
                {
                    _prod_Type = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// 站点
        /// </summary>
        private string _station;
        [SugarColumn(ColumnName = "Station", Length = 255)]
        public string Station
        {
            get => _station;
            set
            {
                if (_station != value)
                {
                    _station = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 站点编码
        /// </summary>
        private string _station_Code;
        [SugarColumn(ColumnName = "Station_Code", Length = 255)]
        public string Station_Code
        {
            get => _station_Code;
            set
            {
                if (_station_Code != value)
                {
                    _station_Code = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// 料号
        /// </summary>
        private string _partNo;
        [SugarColumn(ColumnName = "PartNo", Length = 255)]
        public string PartNo
        {
            get => _partNo;
            set
            {
                if (_partNo != value)
                {
                    _partNo = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// 线材名称
        /// </summary>
        private string _name;
        [SugarColumn(ColumnName = "Name", Length = 255)]
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// 耗材次数
        /// </summary>
        private int _usedTimes;
        [SugarColumn(ColumnName = "UsedTimes", Length = 255)]
        public int UsedTimes
        {
            get => _usedTimes;
            set
            {
                if (_usedTimes != value)
                {
                    _usedTimes = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// 最大耗材次数
        /// </summary>
        private int _maxUesdTimes;
        [SugarColumn(ColumnName = "MaxUesdTimes", Length = 255)]
        public int MaxUesdTimes
        {
            get => _maxUesdTimes;
            set
            {
                if (_maxUesdTimes != value)
                {
                    _maxUesdTimes = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// 是否有效
        /// </summary>
        private int _isValid;
        [SugarColumn(ColumnName = "IsValid", Length = 255)]
        public int IsValid
        {
            get => _isValid;
            set
            {
                if (_isValid != value)
                {
                    _isValid = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// 创建人
        /// </summary>
        private string _creator;
        [SugarColumn(ColumnName = "Creator", Length = 255)]
        public string Creator
        {
            get => _creator;
            set
            {
                if (_creator != value)
                {
                    _creator = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// 创建人
        /// </summary>
        private DateTime _creatime;
        [SugarColumn(ColumnName = "Creatime", Length = 255)]
        public DateTime Creatime
        {
            get => _creatime;
            set
            {
                if (_creatime != value)
                {
                    _creatime = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// 创建人
        /// </summary>
        private string _editor;
        [SugarColumn(ColumnName = "Editor", Length = 255)]
        public string Editor
        {
            get => _editor;
            set
            {
                if (_editor != value)
                {
                    _editor = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// 创建人
        /// </summary>
        private DateTime _editime;
        [SugarColumn(ColumnName = "Editime", Length = 255)]
        public DateTime Editime
        {
            get => _editime;
            set
            {
                if (_editime != value)
                {
                    _editime = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// 创建人
        /// </summary>
        private string _remark;
        [SugarColumn(ColumnName = "Remark", Length = 255)]
        public string Remark
        {
            get => _remark;
            set
            {
                if (_remark != value)
                {
                    _remark = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }


        // 状态跟踪字段（不映射到数据库）
        [SugarColumn(IsIgnore = true)]
        public bool IsModified { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
