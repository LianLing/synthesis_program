using SqlSugar;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace synthesis_program.Models
{
    /// <summary>
    /// 标签数据实体类
    /// </summary>
    [SugarTable("tags")]  // 映射数据库表
    public class TagsModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 主键ID（自增）
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; } = 0;

        /// <summary>
        /// 机型
        /// </summary>
        private string _machineKind;
        [SugarColumn(ColumnName = "MachineKind", Length = 255)]
        public string MachineKind
        {
            get => _machineKind;
            set
            {
                if (_machineKind != value)
                {
                    _machineKind = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 生产批号
        /// </summary>
        private string _batchNo;
        [SugarColumn(ColumnName = "BatchNo", Length = 255)]
        public string BatchNo
        {
            get => _batchNo;
            set
            {
                if (_batchNo != value)
                {
                    _batchNo = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 批量
        /// </summary>
        private string _batchCount;
        [SugarColumn(ColumnName = "BatchCount", Length = 255)]
        public string BatchCount
        {
            get => _batchCount;
            set
            {
                if (_batchCount != value)
                {
                    _batchCount = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 版本
        /// </summary>
        private string _version;
        [SugarColumn(ColumnName = "Version", Length = 255)]
        public string Version
        {
            get => _version;
            set
            {
                if (_version != value)
                {
                    _version = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 料号
        /// </summary>
        private string _materialId;

        [SugarColumn( ColumnName = "MaterialId")]
        public string MaterialId
        {
            get => _materialId;
            set
            {
                if (_materialId != value)
                {
                    _materialId = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 生产编号开始
        /// </summary>
        private string _sequenceNoStart;
        [SugarColumn(ColumnName = "SequenceNoStart", Length = 255)]
        public string SequenceNoStart
        {
            get => _sequenceNoStart;
            set
            {
                if (_sequenceNoStart != value)
                {
                    _sequenceNoStart = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 生产编号结束
        /// </summary>
        private string _sequenceNoEnd;
        [SugarColumn(ColumnName = "SequenceNoEnd", Length = 255)]
        public string SequenceNoEnd
        {
            get => _sequenceNoEnd;
            set
            {
                if (_sequenceNoEnd != value)
                {
                    _sequenceNoEnd = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 模板路径（最大长度50）
        /// </summary>
        private string _modelAddress;
        [SugarColumn(ColumnName = "ModelAddress", Length = 50)]
        public string ModelAddress
        {
            get => _modelAddress;
            set
            {
                if (_modelAddress != value)
                {
                    _modelAddress = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// 生效状态（默认true）
        /// </summary>
        private int _isValid;
        [SugarColumn(ColumnName = "IsValid")]
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
        /// 序列号生成状态
        /// </summary>
        private int _isCreated;
        [SugarColumn(ColumnName = "IsCreated")]
        public int IsCreated
        {
            get => _isCreated;
            set
            {
                if (_isCreated != value)
                {
                    _isCreated = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// 关联机型（最大长度100）
        /// </summary>
        private string _connectMachine;
        [SugarColumn(ColumnName = "ConnectMachine", Length = 100)]
        public string ConnectMachine
        {
            get => _connectMachine;
            set
            {
                if (_connectMachine != value)
                {
                    _connectMachine = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 备注信息（文本类型）
        /// </summary>
        private string _remark;
        [SugarColumn(ColumnName = "Remark")]
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
        /// 扩展字段（预留）
        /// </summary>
        private string _extendValue;
        [SugarColumn(ColumnName = "ExtendValue", Length = 255)]
        public string ExtendValue
        {
            get => _extendValue;
            set
            {
                if (_extendValue != value)
                {
                    _extendValue = value;
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