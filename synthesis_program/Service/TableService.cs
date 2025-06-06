using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using synthesis_program.DataBase;
using synthesis_program.Models;
using SqlSugar;
using synthesis_program.ViewModels;
using System.Globalization;

namespace synthesis_program.Service
{
    internal class TableService : IDisposable
    {
        private readonly DbContext _db;

        public TableService() => _db = new DbContext();
        public void Dispose() => _db?.Dispose();

        public async Task<List<Prod_TypeModel>> QueryMachineKind()
        {
            try
            {
                string sql = $@"SELECT t.`code`,t.name FROM hts_pcs.prod_type t WHERE t.CODE > 'A001' ORDER BY t.name";
                var codes = await _db.Instance.Ado.SqlQueryAsync<Prod_TypeModel>(sql).ConfigureAwait(false);

                return codes;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<string> QueryModules(string machineKind)
        {
            try
            {
                string sql = $@"SELECT t.`code` FROM hts_pcs.prod_module t WHERE t.prod_type = @prod_type ORDER BY `code`";
                return _db.Instance.Ado.SqlQuery<string>(sql, new { prod_type = machineKind }).ToList();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<string> QueryProcesses(string code)
        {
            try
            {
                string sql = $@"SELECT t.`code` FROM hts_pcs.prod_model t WHERE t.prod_type = @prod_type  ORDER BY t.`code`";
                return _db.Instance.Ado.SqlQuery<string>(sql, new { prod_type = code }).ToList();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<InterimModel> QueryStations(string machineKind,string module,string process)
        {
            try
            {
                //string sql = $@"SELECT distinct t.prod_station FROM hts_pcs.vw_eq_cfg_stn_distribute t WHERE t.prod_type = @prod_type AND t.prod_module  = @prod_module AND t.prod_model = @prod_model AND next_cond >= 0";
                string sql = $@"SELECT DISTINCT
                                      t.prod_station value1,
                                      s.`name` value2
                                    FROM
                                      hts_pcs.vw_eq_cfg_stn_distribute t,
                                      hts_pcs.prod_station s
                                    WHERE
                                      t.prod_station = s.`code`
                                      AND t.prod_type = @prod_type
                                      AND t.prod_module = @prod_module
                                      AND t.prod_model = @prod_model
                                      AND next_cond >= 0";
                return _db.Instance.Ado.SqlQuery<InterimModel>(sql, new { prod_type = machineKind, prod_module = module, prod_model = process }).ToList();
            }
            catch (Exception)
            {

                throw;
            }
        }


        public async Task<List<string>> QueryAllMoAsync(string dataname,DateTime pickDate)
        {
            try
            {
                string database = "hts_prod_" + dataname;
                var sqlServerConfig = new ConnectionConfig()
                {
                    ConnectionString = $@"Server=10.10.1.80;Port=3306;Database={database};Uid=1023711;Pwd=HtsUsr.1;CharSet=utf8mb4;",
                    DbType = SqlSugar.DbType.MySql,
                    IsAutoCloseConnection = true,
                    InitKeyType = InitKeyType.Attribute
                };

                using (var sqlServerDb = new SqlSugarClient(sqlServerConfig))
                {
                    string sql = @"select distinct t.mo from prod_test_rcds t where DATE_FORMAT(t.finished_stamp,'%Y-%m-%d') = DATE_FORMAT(@FinishedStamp,'%Y-%m-%d')";

                    var dataTable = await sqlServerDb.Ado.GetDataTableAsync(sql,new { FinishedStamp = pickDate }).ConfigureAwait(false);
                    return dataTable.Rows.Cast<DataRow>().Select(row => row["MO"].ToString()).ToList();
                }
                
            }
            catch (Exception)
            {
                // 异常处理（建议记录日志）
                throw;
            }
        }

        public async Task<List<string>> QueryAllTeam()
        {
            try
            {
                var sqlServerConfig = new ConnectionConfig()
                {
                    ConnectionString = ConfigurationManager.ConnectionStrings["TeamSQLServer"].ConnectionString,
                    DbType = SqlSugar.DbType.SqlServer,
                    IsAutoCloseConnection = true,
                    InitKeyType = InitKeyType.Attribute
                };
                using (var sqlServerDb = new SqlSugarClient(sqlServerConfig))
                {
                    string sql = $@"select t.TeamName from MES_PRODUCT_LINE t";
                    var dt = await sqlServerDb.Ado.GetDataTableAsync(sql).ConfigureAwait(false);
                    return dt.Rows.Cast<DataRow>().Select(row => row[0].ToString()).ToList();
                }

                    
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<ProductPassRateViewModel>> QueryPassRate(ProductPassRateModel passRateModel,string prod_type)
        {
            try
            {
                string database = "hts_prod_" + passRateModel.prod_type;
                int CosmeticNoPassCount = 0;    //外观不良数
                int PerformNoPassCount = 0;     //性能不良数
                ProductPassRateViewModel productPassRateViewModel = new ProductPassRateViewModel();
                var sqlServerConfig = new ConnectionConfig()
                {
                    ConnectionString = $@"Server=10.10.1.80;Port=3306;Database={database};Uid=1023711;Pwd=HtsUsr.1;CharSet=utf8mb4;",
                    DbType = SqlSugar.DbType.MySql,
                    IsAutoCloseConnection = true,
                    InitKeyType = InitKeyType.Attribute
                };

                using (var sqlServerDb = new SqlSugarClient(sqlServerConfig))
                {
                    // 构建基础查询条件
                    var conditions = new List<string>();
                    if (!string.IsNullOrEmpty(passRateModel.prod_type))
                        conditions.Add($@"t.prod_type = @Prod_type");
                    if (!string.IsNullOrEmpty(passRateModel.prod_team))
                        conditions.Add($@"t.prod_team = @ProdTeam");
                    if (!string.IsNullOrEmpty(passRateModel.mo))
                        conditions.Add($@"t.mo = @Mo");
                    if (passRateModel.finished_stamp != null)
                        conditions.Add($@"t.finished_stamp BETWEEN @StartTime AND @EndTime");
                    if (!string.IsNullOrEmpty(passRateModel.station_curr))
                        conditions.Add($@"t.station_curr in {passRateModel.station_curr}");

                    string whereClause = conditions.Any() ? "WHERE " + string.Join(" AND ", conditions) : "";

                    // 创建参数化查询
                    var parameters = new List<SugarParameter>
                    {
                        new SugarParameter("@Prod_type",passRateModel.prod_type),
                        new SugarParameter("@ProdTeam", passRateModel.prod_team),
                        new SugarParameter("@Mo", passRateModel.mo),
                        new SugarParameter("@StartTime", passRateModel.finished_stamp.ObjToDate().ToString("yyyy-MM-dd 00:00:00")),
                        new SugarParameter("@EndTime", passRateModel.finished_stamp.ObjToDate().ToString("yyyy-MM-dd 23:59:59"))
                    };
                    /*
                     * 检验数 = 外观合格数 + 外观/性能不良数
                     * 直通率 = 外观合格数 / 检验数
                     */

                    // 查询当天外观合格数
                    string sqlCosmeticPassCount = $@"SELECT
                                                      COUNT(DISTINCT t.sn)
                                                    FROM
                                                      prod_test_rcds t,
                                                      hts_pcs.vw_eq_cfg_stn_distribute s
                                                      {whereClause}
                                                      AND t.station_curr = s.prod_station
                                                      AND s.`S.name` like '%外观%'
                                                      AND t.err_code = '0000'
                                                      AND NOT EXISTS (SELECT 1 FROM prod_test_rcds t2 WHERE t2.sn = t.sn AND t2.station_curr = '00CF098')";
                    int CosmeticPassCount = await sqlServerDb.Ado.GetIntAsync(sqlCosmeticPassCount, parameters.ToArray()).ConfigureAwait(false);
                    //if (CosmeticPassCount > 0)
                    //{
                        //查询外观/性能不良数
                        //TOP1-3不良
                        string sqlError = $@"(SELECT '性能不良' AS value1,
                                                    ec.`name` AS value2,
                                                    COUNT(DISTINCT t.sn) AS value3,
                                                    rc.cause_c value4
                                                FROM 
                                                    prod_test_rcds t
                                                    INNER JOIN hts_pcs.prod_err_code2 ec ON t.err_code = ec.`code` AND t.err_code NOT LIKE 'U1%'
                                                    INNER JOIN prod_repair rep ON t.sn = rep.sn 
                                                    INNER JOIN hts_pcs.prod_rep_code rc ON rep.rep_code = rc.`code`
                                                {whereClause} 
                                                    AND EXISTS (
                                                        SELECT 1 
                                                        FROM prod_test_rcds t2 
                                                        WHERE t2.sn = t.sn 
                                                            AND t2.station_curr = '00CF098'
                                                        LIMIT 1  
                                                    )
                                                    AND NOT EXISTS (
                                                        SELECT 1 
                                                        FROM prod_repair t3 
                                                        WHERE t.sn = t3.sn 
                                                            AND t3.rep_code = 'P-OK'
                                                        LIMIT 1  
                                                    )
                                                GROUP BY 
                                                    ec.`name`, rc.cause_c
                                                ORDER BY 
                                                    value3 DESC
                                                    LIMIT 3
                                                    )
                                                    UNION ALL
                                                    (
                                                    SELECT 
                                                    '外观不良' AS value1,
                                                    ec.`name` AS value2,
                                                    COUNT(DISTINCT t.sn) AS value3,
                                                    rc.cause_c value4
                                                FROM 
                                                    prod_test_rcds t
                                                    INNER JOIN hts_pcs.prod_err_code2 ec ON t.err_code = ec.`code` AND t.err_code LIKE 'U1%'
                                                    INNER JOIN prod_repair rep ON t.sn = rep.sn 
                                                    INNER JOIN hts_pcs.prod_rep_code rc ON rep.rep_code = rc.`code`
                                                {whereClause} 
                                                    AND EXISTS (
                                                        SELECT 1 
                                                        FROM prod_test_rcds t2 
                                                        WHERE t2.sn = t.sn 
                                                            AND t2.station_curr = '00CF098'
                                                        LIMIT 1  
                                                    )
                                                    AND NOT EXISTS (
                                                        SELECT 1 
                                                        FROM prod_repair t3 
                                                        WHERE t.sn = t3.sn 
                                                            AND t3.rep_code = 'P-OK'
                                                        LIMIT 1  
                                                    )
                                                GROUP BY 
                                                    ec.`name`, rc.cause_c
                                                ORDER BY 
                                                    value3 DESC
                                                    LIMIT 3
                                                    )";

                        var errorItem = await sqlServerDb.Ado.SqlQueryAsync<InterimModel>(sqlError, parameters.ToArray()).ConfigureAwait(false);

                        if (errorItem.Count > 0)
                        {
                            int count1 = errorItem.FindAll(p => p.value1 == "性能不良").Count();
                            if (count1 > 0)
                            {
                                productPassRateViewModel.Top1Capcity = errorItem[0].value2;
                                productPassRateViewModel.RepairReason1 = errorItem[0].value4;
                                productPassRateViewModel.Count1 = errorItem[0].value3;
                                PerformNoPassCount += Convert.ToInt32(errorItem[0].value3);
                            }
                            if (count1 > 1)
                            {
                                productPassRateViewModel.Top2Capcity = errorItem[1].value2;
                                productPassRateViewModel.RepairReason2 = errorItem[0].value4;
                                productPassRateViewModel.Count2 = errorItem[1].value3;
                                PerformNoPassCount += Convert.ToInt32(errorItem[1].value3);
                            }
                            if (count1 > 2)
                            {
                                productPassRateViewModel.Top3Capcity = errorItem[2].value2;
                                productPassRateViewModel.RepairReason3 = errorItem[0].value4;
                                productPassRateViewModel.Count3 = errorItem[2].value3;
                                PerformNoPassCount += Convert.ToInt32(errorItem[2].value3);
                            }

                            int count2 = errorItem.FindAll(p => p.value1 == "外观不良").Count();
                            if (count2 > 0)
                            {
                                productPassRateViewModel.Top1Surface = errorItem[count1 + 0].value2;
                                productPassRateViewModel.RepairReason_1 = errorItem[0].value4;
                                productPassRateViewModel.Count_1 = errorItem[count1 + 0].value3;
                                CosmeticNoPassCount += Convert.ToInt32(errorItem[count1 + 0].value3);
                            }
                            if (count2 > 1)
                            {
                                productPassRateViewModel.Top2Surface = errorItem[count1 + 1].value2;
                                productPassRateViewModel.RepairReason_2 = errorItem[0].value4;
                                productPassRateViewModel.Count_2 = errorItem[count1 + 1].value3;
                                CosmeticNoPassCount += Convert.ToInt32(errorItem[count1 + 1].value3);
                            }
                            if (count2 > 2)
                            {
                                productPassRateViewModel.Top3Surface = errorItem[count1 + 2].value2;
                                productPassRateViewModel.RepairReason_3 = errorItem[0].value4;
                                productPassRateViewModel.Count_3 = errorItem[count1 + 2].value3;
                                CosmeticNoPassCount += Convert.ToInt32(errorItem[count1 + 2].value3);
                            }

                        }
                        productPassRateViewModel.CheckCount = CosmeticPassCount + CosmeticNoPassCount + PerformNoPassCount;
                        passRateModel.pass_rate = (CosmeticPassCount * 100.0 / productPassRateViewModel.CheckCount).ToString("0") + '%';
                    //}
                    //else
                    //{
                    //    passRateModel.pass_rate = "0%";
                    //}

                    
                    string sqlPass = $@"";

                    {
                        productPassRateViewModel.Month = ((Month)(passRateModel.finished_stamp.ObjToDate().Month)).ToString();
                        productPassRateViewModel.Week = GetCurrentWeekNumber(passRateModel.finished_stamp.ObjToDate()).ToString();
                        productPassRateViewModel.Date = passRateModel.finished_stamp.ObjToDate().ToString("MM/dd");
                        productPassRateViewModel.Monumber = passRateModel.mo;
                        productPassRateViewModel.MachineKind = prod_type;
                        productPassRateViewModel.pass_rate = passRateModel.pass_rate;
                        productPassRateViewModel.CosmeticPassCount = CosmeticPassCount;
                        productPassRateViewModel.CosmeticErrorCount = CosmeticNoPassCount;
                        productPassRateViewModel.ErrorCount = PerformNoPassCount;
                    }
                    return new List<ProductPassRateViewModel> { productPassRateViewModel };
                }
            }
            catch (Exception ex)
            {
                // 记录日志
                Console.WriteLine($"计算直通率时发生错误: {ex.Message}");
                throw;
            }
        }

        public enum Month
        {
            Jan = 1,
            Feb = 2,
            Mar = 3,
            Apr = 4,
            May = 5,
            Jun = 6,
            Jul = 7,
            Aug = 8,
            Sep = 9,
            Oct = 10,
            Nov = 11,
            Dec = 12
        }

        public int GetCurrentWeekNumber(DateTime date)
        {
            CultureInfo culture = CultureInfo.InvariantCulture; // 使用固定文化避免区域设置影响
            Calendar calendar = culture.Calendar;
            // ISO 8601规则：第一周至少有4天，且一周从周一开始
            int weekNumber = calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return weekNumber;
        }

        public async Task<List<Prod_TypeModel>> QueryNewMachineKind()
        {
            try
            {
                string sql = $@"SELECT CONCAT(t.`code`,'   ' ,t.NAME) value FROM hts_pcs.prod_type t WHERE t.CODE > 'A001' ORDER BY t.code";
                var dataTable = await _db.Instance.Ado.SqlQueryAsync<Prod_TypeModel>(sql).ConfigureAwait(false);
                return dataTable;
            }
            catch (Exception)
            {

                throw;
            }
        }

        //public async Task<string> GetLineByMo(string type,string mo)
        //{
        //    try
        //    {
        //        string sql = $@"SELECT CONCAT(t.`code`,'   ' ,t.NAME) value FROM hts_pcs.prod_type t WHERE t.CODE > 'A001' ORDER BY t.code";
        //        var dataTable = await _db.Instance.Ado.SqlQueryAsync<Prod_TypeModel>(sql).ConfigureAwait(false);
        //        return dataTable;
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}
    }
}
