using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using synthesis_program.DataBase;
using synthesis_program.Models;

namespace synthesis_program.Service
{
    public class SequenceService :IDisposable
    {
        private readonly DbContext _db;

        public SequenceService() => _db = new DbContext();

        public List<TagsModel> SearchTags(string keyword)
        {
            return _db.Instance.Queryable<TagsModel>()
                .Where(t => t.MaterialId.Contains(keyword) || t.Creater.Contains(keyword))
                .ToList();
        }

        public List<TagsModel> SearchAllTags()
        {
            return _db.Instance.Queryable<TagsModel>().ToList();
        }

        public List<TagsModel> SearchSequence(string keyword)
        {
            return _db.Instance.Queryable<TagsModel>()
                .Where(t => t.BatchNo.Contains(keyword) || t.SequenceNoStart.Contains(keyword))
                .ToList();
        }
        public bool DeleteTag(int id)
        {
            string sql = $@"update sequence set isvalid = 0,EditTime = NOW() where id = @Id";
            var result = _db.Instance.Ado.ExecuteCommand(sql, new { Id = id });
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
                _db.Instance.Ado.CommitTran();
                return result;
            }
            catch
            {
                _db.Instance.Ado.RollbackTran();
                throw;
            }
        }

        public bool InsertSequences(TagsModel tag)
        {
            try
            {
                _db.Instance.Ado.BeginTran();
                var result = _db.Instance.Insertable(tag).ExecuteCommand() > 0;
                _db.Instance.Ado.CommitTran();
                return result;
            }
            catch
            {
                _db.Instance.Ado.RollbackTran();
                throw;
            }
        }
    }
}
