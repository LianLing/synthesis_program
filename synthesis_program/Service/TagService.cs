using synthesis_program.DataBase;
using synthesis_program.Models;
using System;
using System.Collections.Generic;

namespace synthesis_program.Service
{
    public class TagService:IDisposable
    {

        private readonly DbContext _db;

        public TagService() => _db = new DbContext();

        public List<TagsModel> SearchTags(string keyword)
        {
            return _db.Instance.Queryable<TagsModel>()
                .Where(t => t.MachineKind.Contains(keyword) || t.SequenceNoStart.Contains(keyword) || t.MaterialId.Contains(keyword)).Where(p => p.IsValid == 1)
                .ToList();
        }

        public List<TagsModel> SearchSequence(string keyword)
        {
            return _db.Instance.Queryable<TagsModel>()
                .Where(t => t.MachineKind.Contains(keyword) || t.SequenceNoStart.Contains(keyword) || t.MaterialId.Contains(keyword))
                .ToList();
        }

        public List<TagsModel> SearchAllTags()
        {
            return _db.Instance.Queryable<TagsModel>().Where( p => p.IsValid == 1).ToList();
        }

        public bool DeleteTag(int id)
        {
            string sql = $@"update tags set isvalid = 0,EditTime = NOW() where id = @Id";
            var result = _db.Instance.Ado.ExecuteCommand(sql, new {Id = id});
            if (result > 0)
                return true;
            else 
                return false;
        }

        public bool UpdateTag(TagsModel tag)
        {
            return _db.Instance.Updateable(tag).ExecuteCommand() > 0;
        }
        public void Dispose() => _db?.Dispose();

        public bool InsertTag(TagsModel tag)
        {
            try
            {
                _db.Instance.Ado.BeginTran();
                var result = _db.Instance.Insertable(tag).ExecuteCommand() > 0;
               // _db.Instance.Insertable(sequence).ExecuteCommand();
                _db.Instance.Ado.CommitTran();
                return result;
            }
            catch
            {
                _db.Instance.Ado.RollbackTran();
                throw;
            }
        }

        

        // 检查料号是否存在的专用方法
        //public bool MaterialIdExists(string materialId)
        //{
        //    return _db.Instance.Queryable<TagsModel>()
        //        .Where(t => t.MaterialId == materialId)
        //        .Any();
        //}

        /// <summary>
        /// 检查序列号是否已存在
        /// </summary>
        /// <param name="sequenceNo"></param>
        /// <returns></returns>
        public bool SequenceExists(string sequenceNo)
        {
            string sql = $@"select 1 from tags t where t.SequenceNoStart = @SequenceNoStart and t.isvalid = 1 limit 1";
            var result = _db.Instance.Ado.SqlQuerySingle<int>(sql,new { SequenceNoStart = sequenceNo });
            if (result > 0)
                return true;
            else 
                return false;
        }

        /// <summary>
        /// 检查批号是否已存在
        /// </summary>
        /// <param name="batchNo"></param>
        /// <returns></returns>
        public bool BatchNoExists(string batchNo)
        {
            string sql = $@"select 1 from tags t where t.BatchNo = @BatchNo and t.isvalid = 1 limit 1";
            var result = _db.Instance.Ado.SqlQuerySingle<int>(sql, new { BatchNo = batchNo });
            if (result > 0)
                return true;
            else
                return false;
        }

        public bool CheckRepeatSequenceNoStart(string sequenceNoStart, int id)
        {
            string sql = $@"select 1 from tags t where t.SequenceNoStart = @SequenceNoStart and id = @Id limit 1";
            var result = _db.Instance.Ado.SqlQuerySingle<int>(sql, new { SequenceNoStart = sequenceNoStart, Id = id });
            if (result > 0)     //SequenceNoStart未修改
                return true;
            else
            {
                string sql1 = $@"select 1 from tags t where t.SequenceNoStart <> @SequenceNoStart and id = @Id limit 1";
                var result1 = _db.Instance.Ado.SqlQuerySingle<int>(sql1, new { SequenceNoStart = sequenceNoStart, Id = id });
                if (result1 > 0)    //SequenceNoStart修改了
                {
                    string sql2 = $@"select 1 from tags t where t.SequenceNoStart = @SequenceNoStart and id <> @Id limit 1";
                    var result2 = _db.Instance.Ado.SqlQuerySingle<int>(sql2, new { SequenceNoStart = sequenceNoStart, Id = id });
                    if (result2>0)  //SequenceNoStart修改了,并且重复
                    {
                        return false;
                    }else
                    {
                        return true;
                    }
                }
                else
                    return true;
                
            }
              
        }

        public TagsModel GetLatestData(TagsModel tagsModel)
        {
            if (!string.IsNullOrEmpty(tagsModel.MachineKind))
            {
                string sql = $@"select t.* from tags t where t.MachineKind = @machineKind order by t.createtime desc limit 1";
                return _db.Instance.Ado.SqlQuerySingle<TagsModel>(sql, new { machineKind = tagsModel.MachineKind });
            }
            else
            {
                return null;
            }
            
        }

    }
}