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
    [SugarTable("sequence")]
    public class SequenceModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 主键ID（自增）
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        private string _sequenceNo;

        [SugarColumn(ColumnDescription = "序列号", ColumnName = "SequenceNo")]
        public string SequenceNo
        {
            get => _sequenceNo;
            set
            {
                if (_sequenceNo != value)
                {
                    _sequenceNo = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 更新频率
        /// </summary>
        private string _updateRate;

        [SugarColumn( ColumnName = "UpdateRate")]
        public string UpdateRate
        {
            get => _updateRate;
            set
            {
                if (_updateRate != value)
                {
                    _updateRate = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 自增数字是否在末尾
        /// </summary>
        private int _numberIsEnd;

        [SugarColumn(ColumnName = "NumberIsEnd")]
        public int NumberIsEnd
        {
            get => _numberIsEnd;
            set
            {
                if (_numberIsEnd != value)
                {
                    _numberIsEnd = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 创建人（最大长度50）
        /// </summary>
        private string _creater;
        [SugarColumn(ColumnName = "Creater", Length = 50)]
        public string Creater
        {
            get => _creater;
            set
            {
                if (_creater != value)
                {
                    _creater = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 创建时间（自动记录）
        /// </summary>
        [SugarColumn(ColumnName = "CreateTime")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后修改人
        /// </summary>
        private string _editor;
        [SugarColumn(ColumnName = "Editor", Length = 50)]
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
        /// 最后修改时间（自动更新）
        /// </summary>
        private DateTime _editTime;
        [SugarColumn(ColumnName = "EditTime")]
        public DateTime EditTime
        {
            get => _editTime;
            set
            {
                if (_editTime != value)
                {
                    _editTime = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 是否可用：软删除
        /// </summary>
        public int isValid { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        private string _remark;

        [SugarColumn(ColumnDescription = "备注", ColumnName = "Remark")]
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

        [SugarColumn(ColumnName = "Extend_Value")]
        public string Extend_Value { get; set; }

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
