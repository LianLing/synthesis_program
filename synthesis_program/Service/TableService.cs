using DsBoardInterface;
using HtsCommon.DBMySql8;
using SqlSugar;
using synthesis_program.DataBase;
using synthesis_program.Models;
using synthesis_program.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static HsLibs.Utils.WinApi.HsWinApi;
using static HtsCommon.DBMySql8.HtsDB;
using static synthesis_program.Views.DirectRatePage;

namespace synthesis_program.Service
{
    internal class TableService : IDisposable
    {
        private readonly DbContext _db;

        public TableService() => _db = new DbContext();
        public void Dispose() => _db?.Dispose();
        string currentConnection = $@"Server={HtsDB.DBSrvIP};Port={HtsDB.Servers.nDBSrvPort};Uid=htsusr;Pwd=HtsUsr.1;CharSet=utf8mb4;";

        //CServers servers = new CServers();

        public void GetNewConn(string prod_type)
        {
            _db.Instance.Ado.Connection.Close();
            _db.Instance.Ado.Connection.ConnectionString = currentConnection + $"Database=hts_prod_{prod_type};";
            _db.Instance.Ado.Connection.Open();
        }

        public void GetNewConnByPcs(string str)
        {
            _db.Instance.Ado.Connection.Close();
            _db.Instance.Ado.Connection.ConnectionString = currentConnection + $"Database=hts_{str};";
            _db.Instance.Ado.Connection.Open();
        }

        public List<Prod_TypeModel> QueryMachineKind()
        {
            try
            {
                _db.Instance.Ado.Connection.ConnectionString = currentConnection + $"Database=hts_pcs;";
                string sql = $@"SELECT t.`code`,t.name FROM hts_pcs.prod_type t WHERE t.CODE > 'A001' ORDER BY t.name";
                var codes = _db.Instance.Ado.SqlQuery<Prod_TypeModel>(sql);

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
                GetNewConn(machineKind);
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
                GetNewConn(code);
                string sql = $@"SELECT t.`code` FROM hts_pcs.prod_model t WHERE t.prod_type = @prod_type  ORDER BY t.`code`";
                return _db.Instance.Ado.SqlQuery<string>(sql, new { prod_type = code }).ToList();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<InterimModel>> QueryStations(string machineKind, string module, string process)
        {
            try
            {
                GetNewConn(machineKind);
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
                return await _db.Instance.Ado.SqlQueryAsync<InterimModel>(sql, new { prod_type = machineKind, prod_module = module, prod_model = process });
            }
            catch (Exception)
            {

                throw;
            }
        }

        //获取该机型下工单
        public async Task<List<string>> QueryAllMoAsync(string prod_type, DateTime pickDate)
        {
            try
            {
                GetNewConn(prod_type);
                string sql = $@"select distinct t.mo from prod_test_rcds t where t.finished_stamp >= '{pickDate.Date}' and t.finished_stamp < '{pickDate.AddDays(1)}'";
                var dataTable = await _db.Instance.Ado.GetDataTableAsync(sql).ConfigureAwait(false);
                return dataTable.Rows.Cast<DataRow>().Select(row => row["MO"].ToString()).ToList();


            }
            catch (Exception)
            {
                // 异常处理（建议记录日志）
                throw;
            }
        }

        //获取该机型下线别
        public async Task<List<string>> QueryAllLineAsync(string prod_type, DateTime pickDate)
        {
            try
            {
                GetNewConn(prod_type);
                string sql = $@"select distinct t.line_id from prod_test_rcds t where t.finished_stamp >= '{pickDate.Date}' and t.finished_stamp < '{pickDate.AddDays(1)}'";
                var dataTable = await _db.Instance.Ado.GetDataTableAsync(sql).ConfigureAwait(false);
                return dataTable.Rows.Cast<DataRow>().Select(row => row["line_id"].ToString()).ToList();

            }
            catch (Exception)
            {
                // 异常处理（建议记录日志）
                throw;
            }
        }

        public List<string> QueryAllTeam()
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
                    var dt = sqlServerDb.Ado.GetDataTable(sql);
                    return dt.Rows.Cast<DataRow>().Select(row => row[0].ToString()).ToList();
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<ProductPassRateViewModel>> QueryPassRate(ProductPassRateModel passRateModel, string prod_type,string machineKind)
        {
            try
            {
                string database = "hts_prod_" + passRateModel.prod_type;
                int CosmeticNoPassCount = 0;    //外观不良数
                int PerformNoPassCount = 0;     //性能不良数
                string NGcode = "code";     //dtKanBan列code
                string defect_count = "defect_count"; //dtKanBan列defect_count
                string timeType = "timeType"; //dtKanBan列timeType
                string lineId = "prod_line";  //dtKanBan列prod_line
                string extralCondition = string.Empty;  //适用GetKanBanInfo的额外条件
                DataTable dataTable;  //存放看板数据
                ///看板不良:白班数量混合计算，夜班单独计算

                ProductPassRateViewModel productPassRateViewModel = new ProductPassRateViewModel();
                ProductPassRateViewModel productPassRateViewModelWhite = new ProductPassRateViewModel();
                ProductPassRateViewModel productPassRateViewModelBlack = new ProductPassRateViewModel();
                List<ProductPassRateViewModel> passrateModelList = new List<ProductPassRateViewModel>();

                var sqlServerConfig = new ConnectionConfig()
                {
                    ConnectionString = $@"Server={HtsDB.DBSrvIP};Port={HtsDB.Servers.nDBSrvPort};Database={database};Uid=htsusr;Pwd=HtsUsr.1;CharSet=utf8mb4;",
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
                    //线别
                    if (!string.IsNullOrEmpty(passRateModel.line_id))
                        conditions.Add($@"t.line_id = @Line_id");
                    if (!string.IsNullOrEmpty(passRateModel.station_curr))
                        conditions.Add($@"t.station_curr in {passRateModel.station_curr}");

                    var kanBanConditions = new List<string>();

                    string whereClause = string.Empty;
                    //conditions.Any() ? "WHERE " + string.Join(" AND ", conditions) : "";

                    // 创建参数化查询
                    var parameters = new List<SugarParameter>
                    {
                        new SugarParameter("@Prod_type",passRateModel.prod_type),
                        new SugarParameter("@ProdTeam", passRateModel.prod_team),
                        new SugarParameter("@Mo", passRateModel.mo),
                        new SugarParameter("@Line_id",passRateModel.line_id)
                    };

                    /*
                     * 检验数 = 外观合格数 + 外观/性能不良数
                     * 直通率 = 外观合格数 / 外观合格数 + 性能/外观不良数
                     */
                    DataTable dtKanBan = await GetKanNG(sqlServerDb, passRateModel);

                    // 查询当天外观合格数 = 合格数 + 不合格但二次流水正常的数量
                    if (prod_type != "A05F" && prod_type != "V05F")
                    {
                        if (passRateModel.finished_stamp != null)
                            conditions.Add($@"t.finished_stamp BETWEEN '{passRateModel.finished_stamp.ObjToDate().ToString("yyyy-MM-dd 8:30:00")}' AND '{passRateModel.finished_stamp.ObjToDate().ToString("yyyy-MM-dd 20:30:00")}'");
                        productPassRateViewModel.Shift = "白班";
                        productPassRateViewModel.MachineKind = machineKind;
                        whereClause = conditions.Any() ? "WHERE " + string.Join(" AND ", conditions) : "";
                        productPassRateViewModel = await GetProductPassRate(sqlServerDb, whereClause, parameters, productPassRateViewModel, passRateModel, CosmeticNoPassCount, PerformNoPassCount, prod_type, dtKanBan).ConfigureAwait(false);
                        passrateModelList.Add(productPassRateViewModel);
                    }
                    else                //A05F/A0CE白夜班区分
                    {
                        if (dtKanBan.Rows.Count > 0)
                        {

                            if (dtKanBan.AsEnumerable().Any(row => row.Field<string>(timeType).Contains("白班")))
                            {
                                foreach (DataRow row in dtKanBan.Rows)
                                {
                                    if (row[timeType].ToString().Contains("白班"))
                                    {
                                        if (row[NGcode].ToString().Substring(2, 1) == "D" && passRateModel.line_id == row[lineId].ToString())      //选线别的外观不良
                                        {
                                            CosmeticNoPassCount += row[defect_count].ObjToInt();
                                        }
                                        else if (row[NGcode].ToString().Substring(2, 1) == "D" && passRateModel.line_id == "")                     //不选线别的外观不良
                                        {
                                            CosmeticNoPassCount += row[defect_count].ObjToInt();
                                        }
                                        else if (row[NGcode].ToString().Substring(2, 1) != "D" && passRateModel.line_id == row[lineId].ToString())         //选线别的性能不良
                                        {
                                            PerformNoPassCount += row[defect_count].ObjToInt();
                                        }
                                        else if (row[NGcode].ToString().Substring(2, 1) != "D" && passRateModel.line_id == "")                      //不选线别的性能不良
                                        {
                                            PerformNoPassCount += row[defect_count].ObjToInt();
                                        }
                                    }
                                }

                                if (passRateModel.finished_stamp != null)
                                    conditions.Add($@"t.finished_stamp BETWEEN '{passRateModel.finished_stamp.ObjToDate().ToString("yyyy-MM-dd 8:30:00")}' AND '{passRateModel.finished_stamp.ObjToDate().ToString("yyyy-MM-dd 20:30:00")}'");
                                productPassRateViewModelWhite.Shift = "白班";
                                productPassRateViewModelWhite.MachineKind = machineKind;
                                whereClause = conditions.Any() ? "WHERE " + string.Join(" AND ", conditions) : "";
                                extralCondition = $" and t.working_type <= 2 ";
                                dataTable = await GetKanBanInfo(sqlServerDb, passRateModel, extralCondition);
                                //dataTable = dtKanBan.AsEnumerable().Where(row => row.Field<string>(timeType).Contains("白班")).CopyToDataTable();
                                productPassRateViewModel = await GetProductPassRate(sqlServerDb, whereClause, parameters, productPassRateViewModelWhite, passRateModel, CosmeticNoPassCount, PerformNoPassCount, prod_type, dataTable).ConfigureAwait(false);
                                passrateModelList.Add(productPassRateViewModel);


                            }
                            if (dtKanBan.AsEnumerable().Any(row => row.Field<string>(timeType).Contains("夜班")))
                            {
                                CosmeticNoPassCount = 0;
                                PerformNoPassCount = 0;
                                foreach (DataRow row in dtKanBan.Rows)
                                {
                                    if (row[timeType].ToString().Contains("夜班"))
                                    {
                                        if (row[NGcode].ToString().Substring(2, 1) == "D" && passRateModel.line_id == row[lineId].ToString())      //选线别的外观不良
                                        {
                                            CosmeticNoPassCount += row[defect_count].ObjToInt();
                                        }
                                        else if (row[NGcode].ToString().Substring(2, 1) == "D" && passRateModel.line_id == "")                     //不选线别的外观不良
                                        {
                                            CosmeticNoPassCount += row[defect_count].ObjToInt();
                                        }
                                        else if (row[NGcode].ToString().Substring(2, 1) != "D" && passRateModel.line_id == row[lineId].ToString())         //选线别的性能不良
                                        {
                                            PerformNoPassCount += row[defect_count].ObjToInt();
                                        }
                                        else if (row[NGcode].ToString().Substring(2, 1) != "D" && passRateModel.line_id == "")                      //不选线别的性能不良
                                        {
                                            PerformNoPassCount += row[defect_count].ObjToInt();
                                        }
                                    }
                                }
                                if (passRateModel.finished_stamp != null && dtKanBan.AsEnumerable().Any(row => row.Field<string>(timeType).Contains("白班")))
                                {
                                    conditions.Remove($@"t.finished_stamp BETWEEN '{passRateModel.finished_stamp.ObjToDate().ToString("yyyy-MM-dd 8:30:00")}' AND '{passRateModel.finished_stamp.ObjToDate().ToString("yyyy-MM-dd 20:30:00")}'");
                                    conditions.Add($@"t.finished_stamp BETWEEN '{passRateModel.finished_stamp.ObjToDate().ToString("yyyy-MM-dd 20:30:00")}' AND '{passRateModel.finished_stamp.ObjToDate().AddDays(1).ToString("yyyy-MM-dd 8:30:00")}'");
                                }
                                else
                                {
                                    conditions.Add($@"t.finished_stamp BETWEEN '{passRateModel.finished_stamp.ObjToDate().ToString("yyyy-MM-dd 20:30:00")}' AND '{passRateModel.finished_stamp.ObjToDate().AddDays(1).ToString("yyyy-MM-dd 8:30:00")}'");
                                }

                                productPassRateViewModelBlack.Shift = "夜班";
                                productPassRateViewModelBlack.MachineKind = machineKind;
                                whereClause = conditions.Any() ? "WHERE " + string.Join(" AND ", conditions) : "";
                                //dataTable = dtKanBan.AsEnumerable().Where(row => row.Field<string>(timeType).Contains("夜班")).CopyToDataTable();
                                extralCondition = $" and t.working_type > 2 ";
                                dataTable = await GetKanBanInfo(sqlServerDb, passRateModel, extralCondition);
                                productPassRateViewModel = await GetProductPassRate(sqlServerDb, whereClause, parameters, productPassRateViewModelBlack, passRateModel, CosmeticNoPassCount, PerformNoPassCount, prod_type, dataTable).ConfigureAwait(false);
                                passrateModelList.Add(productPassRateViewModel);
                            }
                        }
                    }


                    return passrateModelList;
                }
            }
            catch (Exception ex)
            {
                // 记录日志
                Console.WriteLine($"计算直通率时发生错误: {ex.Message}");
                throw;
            }
        }


        public async Task<DataTable> GetKanBanInfo(SqlSugarClient sqlServerDb, ProductPassRateModel passRateModel, string extralCondition)
        {
            string condition = string.Empty;
            string ng = "NG";       //dtKanBan列的NG字段
            if (!string.IsNullOrEmpty(passRateModel.prod_type))
            {
                condition += $" and t.prod_type = '{passRateModel.prod_type}'";
            }
            if (!string.IsNullOrEmpty(passRateModel.line_id))
            {
                condition += $" and t.prod_line = '{passRateModel.line_id}'";
            }
            if (passRateModel.finished_stamp != null)
            {
                condition += $" and t.date = '{passRateModel.finished_stamp}'";
            }
            condition += extralCondition;
            string kanbansql = $@"(SELECT
                                    '性能不良' NG,
                                    s.`name`,
                                    SUM(t.defect_count) count1
                                  FROM
                                    hts_ds.ds_cfg_offline_defect t
                                    INNER JOIN hts_pcs.prod_err_code2 s ON t.`code` = s.`code`
                                  WHERE
                                    1 = 1
                                    {condition}
                                    AND SUBSTR(t.`code` FROM 3 FOR 1) <> 'D'
                                      GROUP BY
                                        s.`name`
                                      ORDER BY
                                        count1 DESC
                                        LIMIT 3
                                    ) UNION ALL
                                    (
                                      SELECT
                                        '外观不良' NG,
                                        s.`name`,
                                        SUM(t.defect_count) count1
                                      FROM
                                        hts_ds.ds_cfg_offline_defect t
                                        INNER JOIN hts_pcs.prod_err_code2 s ON t.`code` = s.`code`
                                      WHERE
                                        1 = 1
                                            {condition}     
                                        AND SUBSTR(t.`code` FROM 3 FOR 1) = 'D'
                                          GROUP BY
                                            s.`name`
                                          ORDER BY
                                            count1 DESC
                                            LIMIT 3
                                        )";
            return await sqlServerDb.Ado.GetDataTableAsync(kanbansql).ConfigureAwait(false);

        }

        public async Task<DataTable> GetKanNG(SqlSugarClient sqlServerDb, ProductPassRateModel passRateModel)
        {
            string condition = string.Empty;
            if (!string.IsNullOrEmpty(passRateModel.prod_type))
            {
                condition += $" and t.prod_type = '{passRateModel.prod_type}'";
            }
            if (!string.IsNullOrEmpty(passRateModel.line_id))
            {
                condition += $" and t.prod_line = '{passRateModel.line_id}'";
            }
            if (passRateModel.finished_stamp != null)
            {
                condition += $" and t.date = '{passRateModel.finished_stamp}'";
            }
            string sql = $@"SELECT
                          t.prod_line,
                          t.`code`,
                          t.defect_count,
                          t.date,
                          CASE
                            t.working_type
                            WHEN 1 THEN
                              '纯白班'
                            WHEN 2 THEN
                              '24小时-白班'
                            WHEN 3 THEN
                              '24小时-夜班'
                          END timeType,
                          s.`name`
                        FROM
                          hts_ds.ds_cfg_offline_defect t
                          LEFT JOIN hts_pcs.prod_err_code2 s ON t.`code` = s.`code`
                          where 1=1 
                            {condition}";
            return await sqlServerDb.Ado.GetDataTableAsync(sql).ConfigureAwait(false);

        }

        public async Task<ProductPassRateViewModel> GetProductPassRate(SqlSugarClient sqlServerDb, string whereClause, List<SugarParameter> parameters, ProductPassRateViewModel productPassRateViewModel, ProductPassRateModel passRateModel, int CosmeticNoPassCount, int PerformNoPassCount, string prod_type, DataTable dataTable)
        {

            // 查询当天外观合格数 = 合格数 + 不合格但二次流水正常的数量
            string sqlCosmeticPassCount = $@"SELECT
                                                      COUNT(DISTINCT t.sn)
                                                    FROM
                                                      prod_test_rcds t,
                                                      hts_pcs.prod_station s
                                                      {whereClause}
                                                      AND t.station_curr = s.`code`
                                                      AND s.`name` like '%外观%'
                                                      AND t.err_code = '0000'
                                                      ";
            //##AND NOT EXISTS (SELECT 1 FROM prod_test_rcds t2 WHERE t2.sn = t.sn AND t2.station_curr = '00CF098')
            int CosmeticPassCount = await sqlServerDb.Ado.GetIntAsync(sqlCosmeticPassCount, parameters.ToArray()).ConfigureAwait(false);

            //查询外观/性能不良数
            //TOP1-3不良
            string sqlError = $@"(SELECT '性能不良' AS value1,
                                                    ec.`name` AS value2,
                                                    COUNT(DISTINCT t.sn) AS value3,
                                                    rc.cause_c value4,
                                                    t.err_code value5
                                                FROM 
                                                    prod_test_rcds t
                                                    INNER JOIN hts_pcs.prod_err_code2 ec ON t.err_code = ec.`code` AND t.err_code NOT LIKE 'U1%'
                                                    INNER JOIN prod_repair rep ON t.sn = rep.sn 
                                                    INNER JOIN hts_pcs.prod_rep_code rc ON rep.rep_code = rc.`code` and rc.cause_c not like '%误测%'
                                                {whereClause} 
                                                    AND EXISTS (
                                                        SELECT 1 
                                                        FROM prod_test_rcds t2 
                                                        WHERE t2.sn = t.sn 
                                                            AND t2.station_curr = '00CF098'
                                                        LIMIT 1  
                                                    )
                                                    
                                                GROUP BY 
                                                    ec.`name`, rc.cause_c,t.err_code
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
                                                    rc.cause_c value4,
                                                    t.err_code value5
                                                FROM 
                                                    prod_test_rcds t
                                                    INNER JOIN hts_pcs.prod_err_code2 ec ON t.err_code = ec.`code` AND t.err_code LIKE 'U1%'
                                                    INNER JOIN prod_repair rep ON t.sn = rep.sn 
                                                    INNER JOIN hts_pcs.prod_rep_code rc ON rep.rep_code = rc.`code` and rc.cause_c not like '%误测%'
                                                {whereClause} 
                                                    AND EXISTS (
                                                        SELECT 1 
                                                        FROM prod_test_rcds t2 
                                                        WHERE t2.sn = t.sn 
                                                            AND t2.station_curr = '00CF098'
                                                        LIMIT 1  
                                                    )
                                                    
                                                GROUP BY 
                                                    ec.`name`, rc.cause_c,t.err_code
                                                ORDER BY 
                                                    value3 DESC
                                                    LIMIT 3
                                                    )";

            var errorItem = await sqlServerDb.Ado.SqlQueryAsync<InterimModel>(sqlError, parameters.ToArray()).ConfigureAwait(false);

            ///区分A05F和其他机型
            if (passRateModel.prod_type.ToUpper() != "A05F" && passRateModel.prod_type.ToUpper() != "V05F")
            {
                string timeType = "timeType"; //dtKanBan列timeType
                string NGcode = "code";     //dtKanBan列code
                string defect_count = "defect_count"; //dtKanBan列defect_count

                int count1 = errorItem.FindAll(p => p.value1 == "性能不良").Count();
                if (count1 > 0)
                {
                    productPassRateViewModel.Top1Capcity = errorItem.Count > 0 ? errorItem[0].value2 : "";
                    productPassRateViewModel.RepairReason1 = errorItem.Count > 0 ? errorItem[0].value4 : "";
                    productPassRateViewModel.Count1 = errorItem.Count > 0 ? errorItem[0].value3 : "";
                    PerformNoPassCount += Convert.ToInt32(errorItem.Count > 0 ? errorItem[0].value3 : "0");
                }
                if (count1 > 1)
                {
                    productPassRateViewModel.Top2Capcity = errorItem.Count > 0 ? errorItem[1].value2 : "";
                    productPassRateViewModel.RepairReason2 = errorItem.Count > 0 ? errorItem[1].value4 : "";
                    productPassRateViewModel.Count2 = errorItem.Count > 0 ? errorItem[1].value3 : "";
                    PerformNoPassCount += Convert.ToInt32(errorItem.Count > 0 ? errorItem[1].value3 : "");
                }
                if (count1 > 2)
                {
                    productPassRateViewModel.Top3Capcity = errorItem.Count > 0 ? errorItem[2].value2 : "";
                    productPassRateViewModel.RepairReason3 = errorItem.Count > 0 ? errorItem[2].value4 : "";
                    productPassRateViewModel.Count3 = errorItem.Count > 0 ? errorItem[2].value3 : "";
                    PerformNoPassCount += Convert.ToInt32(errorItem.Count > 0 ? errorItem[2].value3 : "");
                }

                int count2 = errorItem.FindAll(p => p.value1 == "外观不良").Count();
                if (count2 > 0)
                {
                    productPassRateViewModel.Top1Surface = errorItem.Count > (count1 + 0) ? errorItem[count1 + 0].value2 : "";
                    productPassRateViewModel.RepairReason_1 = errorItem.Count > (count1 + 0) ? errorItem[count1 + 0].value4 : "";
                    productPassRateViewModel.Count_1 = errorItem.Count > (count1 + 0) ? errorItem[count1 + 0].value3 : "";
                    CosmeticNoPassCount += Convert.ToInt32(errorItem.Count > (count1 + 0) ? errorItem[count1 + 0].value3 : "");
                }
                if (count2 > 1)
                {
                    productPassRateViewModel.Top2Surface = errorItem.Count > (count1 + 1) ? errorItem[count1 + 1].value2 : "";
                    productPassRateViewModel.RepairReason_2 = errorItem.Count > (count1 + 1) ? errorItem[count1 + 1].value4 : "";
                    productPassRateViewModel.Count_2 = errorItem.Count > (count1 + 1) ? errorItem[count1 + 1].value3 : "";
                    CosmeticNoPassCount += Convert.ToInt32(errorItem.Count > (count1 + 1) ? errorItem[count1 + 1].value3 : "");
                }
                if (count2 > 2)
                {
                    productPassRateViewModel.Top3Surface = errorItem.Count > (count1 + 2) ? errorItem[count1 + 2].value2 : "";
                    productPassRateViewModel.RepairReason_3 = errorItem.Count > (count1 + 2) ? errorItem[count1 + 2].value4 : "";
                    productPassRateViewModel.Count_3 = errorItem.Count > (count1 + 2) ? errorItem[count1 + 2].value3 : "";
                    CosmeticNoPassCount += Convert.ToInt32(errorItem.Count > (count1 + 2) ? errorItem[count1 + 2].value3 : "");
                }

            }
            else
            {
                int count1 = errorItem.FindAll(p => p.value1 == "性能不良").Count();
                int countAllDt = dataTable.Rows.Count;
                if ((countAllDt - 1) >= 0 && count1 <= dataTable.Rows[0]["count1"].ObjToInt() && dataTable.Rows[0]["NG"].ToString() == "性能不良")                //性能不良TOP数量小于看板不良数量时，取看板不良数量为准
                {
                    productPassRateViewModel.Top1Capcity = dataTable.Rows[0]["name"].ToString();
                    productPassRateViewModel.RepairReason1 = "线外不良";
                    productPassRateViewModel.Count1 = dataTable.Rows[0]["count1"].ToString();
                }
                else
                {
                    productPassRateViewModel.Top1Capcity = count1 > 0 ? errorItem[0].value2 : "";
                    productPassRateViewModel.RepairReason1 = count1 > 0 ? errorItem[0].value4 : "";
                    productPassRateViewModel.Count1 = count1 > 0 ? errorItem[0].value3 : "";
                }

                if ((countAllDt - 1) >= 1 && count1 <= dataTable.Rows[1]["count1"].ObjToInt() && dataTable.Rows[1]["NG"].ToString() == "性能不良")
                {
                    productPassRateViewModel.Top2Capcity = dataTable.Rows[1]["name"].ToString();
                    productPassRateViewModel.RepairReason2 = "线外不良";
                    productPassRateViewModel.Count2 = dataTable.Rows[1]["count1"].ToString();
                }
                else
                {
                    productPassRateViewModel.Top2Capcity = count1 > 1 ? errorItem[1].value2 : "";
                    productPassRateViewModel.RepairReason2 = count1 > 1 ? errorItem[1].value4 : "";
                    productPassRateViewModel.Count2 = count1 > 1 ? errorItem[1].value3 : "";
                }

                if ((countAllDt - 1) >= 2 && count1 <= dataTable.Rows[2]["count1"].ObjToInt() && dataTable.Rows[2]["NG"].ToString() == "性能不良")
                {
                    productPassRateViewModel.Top2Capcity = dataTable.Rows[2]["name"].ToString();
                    productPassRateViewModel.RepairReason2 = "线外不良";
                    productPassRateViewModel.Count2 = dataTable.Rows[2]["count1"].ToString();
                }
                else
                {
                    productPassRateViewModel.Top3Capcity = count1 > 2 ? errorItem[2].value2 : "";
                    productPassRateViewModel.RepairReason3 = count1 > 2 ? errorItem[2].value4 : "";
                    productPassRateViewModel.Count3 = count1 > 2 ? errorItem[2].value3 : "";
                }

                int count2 = errorItem.FindAll(p => p.value1 == "外观不良").Count();
                int count3 = Convert.ToInt32(dataTable.Compute("COUNT(NG)", "NG = '性能不良'"));        //获取性能不良数
                //int count3 = dataTable.AsEnumerable().Count(row => row.Field<string>("NG") == "性能不良");        //获取性能不良数:使用LINQ列名查询
                //int count3 = dataTable.AsEnumerable().Count(row => row[0]?.ToString() == "性能不良");        //获取性能不良数:使用LINQ索引查询

                if ((countAllDt - 1) >= count3 && count2 <= dataTable.Rows[count3 + 0]["count1"].ObjToInt() && dataTable.Rows[count3 + 0]["NG"].ToString() == "外观不良")
                {
                    productPassRateViewModel.Top1Surface = dataTable.Rows[count3]["name"].ToString();
                    productPassRateViewModel.RepairReason_1 = "线外不良";
                    productPassRateViewModel.Count_1 = dataTable.Rows[count3]["count1"].ToString();
                }
                else
                {
                    productPassRateViewModel.Top1Surface = count2 > 0 ? errorItem[count3 + 0].value2 : "";
                    productPassRateViewModel.RepairReason_1 = count2 > 0 ? errorItem[count3 + 0].value4 : "";
                    productPassRateViewModel.Count_1 = count2 > 0 ? errorItem[count3 + 0].value3 : "";
                }

                if ((countAllDt - 1) >= (count3 + 1) && count2 <= dataTable.Rows[count3 + 1]["count1"].ObjToInt() && dataTable.Rows[count3 + 1]["NG"].ToString() == "外观不良")
                {
                    productPassRateViewModel.Top2Surface = dataTable.Rows[count3 + 1]["name"].ToString();
                    productPassRateViewModel.RepairReason_2 = "线外不良";
                    productPassRateViewModel.Count_2 = dataTable.Rows[count3 + 1]["count1"].ToString();
                }
                else
                {
                    productPassRateViewModel.Top2Surface = count2 > 1 ? errorItem[count3 + 1].value2 : "";
                    productPassRateViewModel.RepairReason_2 = count2 > 1 ? errorItem[count3 + 1].value4 : "";
                    productPassRateViewModel.Count_2 = count2 > 1 ? errorItem[count3 + 1].value3 : "";
                }

                if ((countAllDt - 1) >= (count3 + 2) && count2 <= dataTable.Rows[count3 + 2]["count1"].ObjToInt() && dataTable.Rows[count3 + 2]["NG"].ToString() == "外观不良")
                {
                    productPassRateViewModel.Top3Surface = dataTable.Rows[count3 + 2]["name"].ToString();
                    productPassRateViewModel.RepairReason_3 = "线外不良";
                    productPassRateViewModel.Count_3 = dataTable.Rows[count3 + 2]["count1"].ToString();
                }
                else
                {
                    productPassRateViewModel.Top3Surface = count2 > 2 ? errorItem[count3 + 2].value2 : "";
                    productPassRateViewModel.RepairReason_3 = count2 > 2 ? errorItem[count3 + 2].value4 : "";
                    productPassRateViewModel.Count_3 = count2 > 2 ? errorItem[count3 + 2].value3 : "";
                }
            }
            productPassRateViewModel.CheckCount = CosmeticPassCount + CosmeticNoPassCount + PerformNoPassCount;
            passRateModel.pass_rate = (CosmeticPassCount * 100.0 / productPassRateViewModel.CheckCount).ToString("F2") + '%';


            {
                productPassRateViewModel.Month = ((Month)(passRateModel.finished_stamp.ObjToDate().Month)).ToString();
                productPassRateViewModel.Week = GetCurrentWeekNumber(passRateModel.finished_stamp.ObjToDate()).ToString();
                productPassRateViewModel.Date = passRateModel.finished_stamp.ObjToDate().ToString("MM/dd");
                productPassRateViewModel.Line = passRateModel.line_id != "" ? passRateModel.line_id : "全部";
                productPassRateViewModel.Monumber = passRateModel.mo;
                productPassRateViewModel.pass_rate = passRateModel.pass_rate;
                productPassRateViewModel.CosmeticPassCount = CosmeticPassCount;
                productPassRateViewModel.CosmeticErrorCount = CosmeticNoPassCount;
                productPassRateViewModel.ErrorCount = PerformNoPassCount;
            }
            return productPassRateViewModel;
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



        public async Task<List<ProductRecords>> QueryProductInfo(string code, string prod_type, string order_no)
        {
            try
            {
                string database = "hts_prod_" + code;
                ProductRecords product = new ProductRecords();
                List<ProductRecords> productList = new List<ProductRecords>();
                List<ProductRecords> resultList = new List<ProductRecords>();
                string sql = string.Empty;
                var sqlServerConfig = new ConnectionConfig()
                {
                    ConnectionString = ConfigurationManager.ConnectionStrings["ErpSQLServer"].ConnectionString,
                    DbType = SqlSugar.DbType.SqlServer,
                    IsAutoCloseConnection = true,
                    InitKeyType = InitKeyType.Attribute
                };
                var mysqlConfig = new ConnectionConfig()
                {
                    ConnectionString = $@"Server=10.10.1.80;Port=3306;Database={database};Uid=1023711;Pwd=HtsUsr.1;CharSet=utf8mb4;",
                    DbType = SqlSugar.DbType.MySql,
                    IsAutoCloseConnection = true,
                    InitKeyType = InitKeyType.Attribute
                };

                string sql0 = $@"WITH t1 AS (
                                    SELECT DISTINCT
                                        m.mo,
                                        n.csn 
                                    FROM prod_snapshot m
                                    INNER JOIN prod_var n  
                                        ON m.sn = n.sn
                                        AND n.csn is not null
                                    WHERE m.mo = @Mo  
                                ),
                                t2 AS (
                                    SELECT 
                                        t.mo,
                                        t.finished_stamp,
                                        t.line_id
                                    FROM prod_test_rcds t 
                                    WHERE t.mo = @Mo 
                                    ORDER BY t.finished_stamp ASC 
                                    LIMIT 1
                                )
                                SELECT 
                                    DATE_FORMAT(t2.finished_stamp, '%Y-%m-%d %H:%i:%s') Date,t2.line_id LineId,@Model MODEL,@OrderNo BatchNo,@Completed_Qty COMPLETED_QTY,@Version Version,@PartNo PartNo,t1.csn ProductCode
                                FROM t1
                                INNER JOIN t2  
                                    ON t1.mo = t2.mo";

                using (var sqlServerDb = new SqlSugarClient(sqlServerConfig))
                {
                    if (!string.IsNullOrEmpty(order_no))        //单个结果与MySQL联查
                    {
                        sql = $@"select t.MO Mo,t.ORDER_NO BatchNo,t.COMPLETED_QTY,t.MODEL Version,t.PART_NO PartNo from HS_MO t where t.MODEL like '%{prod_type}%' and SUBSTRING(t.PART_NO, 1, 2) = '00' and t.ORDER_NO = '{order_no}'";
                        product = await sqlServerDb.Ado.SqlQuerySingleAsync<ProductRecords>(sql).ConfigureAwait(false);
                        using (var mySqlDb = new SqlSugarClient(mysqlConfig))
                        {
                            var result = await mySqlDb.Ado.SqlQueryAsync<ProductRecords>(sql0, new { Mo = product.Mo, Model = prod_type, OrderNo = product.BatchNo, Completed_Qty = product.COMPLETED_QTY, Version = product.Version, PartNo = product.PartNo }).ConfigureAwait(false);
                            return result;
                        }
                    }
                    else                //在erp查出结果集，遍历结果集，与MySQL联查
                    {
                        sql = $@"select t.MO Mo,t.ORDER_NO BatchNo,t.COMPLETED_QTY,t.MODEL,t.PART_NO PartNo from HS_MO t where t.MODEL like '%{prod_type}%' and SUBSTRING(t.PART_NO, 1, 2) = '00'";
                        productList = await sqlServerDb.Ado.SqlQueryAsync<ProductRecords>(sql).ConfigureAwait(false);
                        foreach (var p in productList)
                        {
                            using (var mySqlDb = new SqlSugarClient(mysqlConfig))
                            {
                                var result = await mySqlDb.Ado.SqlQueryAsync<ProductRecords>(sql0, new { Mo = p.Mo, Model = prod_type, OrderNo = p.BatchNo, Completed_Qty = p.COMPLETED_QTY, Version = p.Version, PartNo = p.PartNo }).ConfigureAwait(false);
                                resultList.AddRange(result);
                            }
                        }
                        return resultList.OrderBy(r => r.Date).ToList();
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<string>> GetOrderNo(string prod_type)
        {
            try
            {
                var sqlServerConfig = new ConnectionConfig()
                {
                    ConnectionString = ConfigurationManager.ConnectionStrings["ErpSQLServer"].ConnectionString,
                    DbType = SqlSugar.DbType.SqlServer,
                    IsAutoCloseConnection = true,
                    InitKeyType = InitKeyType.Attribute
                };
                using (var sqlServerDb = new SqlSugarClient(sqlServerConfig))
                {
                    string sql = $@"select t.ORDER_NO from HS_MO t where t.MODEL like '%{prod_type}%' and SUBSTRING(t.PART_NO, 1, 2) = '00'";
                    return await sqlServerDb.Ado.SqlQueryAsync<string>(sql).ConfigureAwait(false);
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<int> UpdateStationStatus(string prod_type, List<CheckBoxItem> boxItems)
        {
            try
            {
                _db.Instance.CurrentConnectionConfig.ConnectionString = currentConnection + $"Database=hts_prod_{prod_type};";
                string stationStr = string.Empty;
                if (boxItems.Count > 0)
                {
                    foreach (var item in boxItems)
                    {
                        stationStr += item.Code + ",";
                    }
                }

                stationStr = stationStr.TrimEnd(',');
                //先查询是否已存在数据，如果不存在则插入
                string checkSql = $@"SELECT COUNT(1) FROM TagsManage.station WHERE prod_type = '{prod_type}' and status = 1";
                var count = await _db.Instance.Ado.GetIntAsync(checkSql).ConfigureAwait(false);
                if (count == 0)
                {
                    string sql = $@"insert into TagsManage.station (code,status,prod_type) VALUES ('{stationStr}',1,'{prod_type}')";
                    var result = await _db.Instance.Ado.ExecuteCommandAsync(sql).ConfigureAwait(false);
                    return result;
                }
                else        //如果该机型下存在数据，则更新站点
                {
                    string sql = $@"UPDATE TagsManage.station SET status = 1,code = '{stationStr}' WHERE prod_type = '{prod_type}'";
                    var result = await _db.Instance.Ado.ExecuteCommandAsync(sql).ConfigureAwait(false);
                    return result;
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> QueryCheckedStations(string prod_type)
        {
            try
            {
                _db.Instance.CurrentConnectionConfig.ConnectionString = currentConnection + $"Database=hts_prod_{prod_type};";
                string sql = $@"SELECT code FROM TagsManage.station WHERE prod_type = '{prod_type}' and status = 1";
                var result = await _db.Instance.Ado.SqlQuerySingleAsync<string>(sql).ConfigureAwait(false);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> QueryStationsByProdType(string prod_type)
        {
            try
            {
                _db.Instance.CurrentConnectionConfig.ConnectionString = currentConnection + $"Database=hts_prod_{prod_type};";
                string sql = $@"select t.`code` from TagsManage.station t where t.prod_type = '{prod_type}'";
                return await _db.Instance.Ado.SqlQuerySingleAsync<string>(sql, new { prod_type }).ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 获取初始化机型信息
        /// </summary>
        /// <returns></returns>
        public async Task<List<Prod_TypeModel>> QueryProdType()
        {
            try
            {
                string sql = $@"SELECT t.`code`,t.`name` FROM hts_pcs.prod_type t WHERE t.CODE > 'A001' ORDER BY t.name";
                var codes = await _db.Instance.Ado.SqlQueryAsync<Prod_TypeModel>(sql).ConfigureAwait(false);

                return codes;
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// 根据输入框检索
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public List<ProdLineManageModel> SearchLines(string keyword)
        {
            GetNewConnByPcs("misc");
            return _db.Instance.Queryable<ProdLineManageModel>()
                .Where(t => t.PartNo.Contains(keyword) || t.Name.Contains(keyword)).Where(p => p.IsValid == 1)
                .ToList();
        }


        public bool DeleteLines(int id,string user)
        {
            //GetNewConnByPcs("misc");
            string sql = $@"update hts_misc.Prod_line_Material set isvalid = 0,editime = NOW(),editor = @editor where id = @Id";
            var result = _db.Instance.Ado.ExecuteCommand(sql, new { Id = id, editor = user });
            if (result > 0)
                return true;
            else
                return false;
        }

        public async Task<List<ProdStationModel>> GetStationsByProdType(string prod_type)
        {
            try
            {
                //string sql = $@"select CONCAT(t.`name`,'+',t.`code`) `name`,t.`code` from hts_pcs.prod_station t where t.prod_type = @ProdType;";
                string sql = $@"select t.`name`,t.`code` from hts_pcs.prod_station t where t.prod_type = @ProdType";
                var stations = await _db.Instance.Ado.SqlQueryAsync<ProdStationModel>(sql, new { ProdType = prod_type }).ConfigureAwait(false);
                return stations;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<ProdLineManageModel>> QueryLinesAsync(string prodType,string station)
        {
            try
            {
                string condition = string.Empty;
                if (!string.IsNullOrEmpty(station))
                {
                    condition += $" and t.station_code = '{station}'";
                }
                string sql = $@"SELECT t.* FROM hts_misc.Prod_line_Material t WHERE t.prod_type = @prod_type {condition} and t.isvalid = 1 order by t.id";
                var lines = await _db.Instance.Ado.SqlQueryAsync<ProdLineManageModel>(sql,new { prod_type = prodType}).ConfigureAwait(false);
                return lines;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// 检查料号是否已存在
        /// </summary>
        /// <param name="partNo"></param>
        /// <returns></returns>
        public bool PartNoExists(string partNo)
        {
            string sql = $@"select 1 from hts_misc.Prod_line_Material t where t.PartNo = @PartNo and t.isvalid = 1 limit 1";
            var result = _db.Instance.Ado.SqlQuerySingle<int>(sql, new { PartNo = partNo });
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


        public bool InsertTag(ProdLineManageModel tag)
        {
            try
            {
                GetNewConnByPcs("misc");
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

        /// <summary>
        /// 检查料号是否重复(编辑时使用)
        /// </summary>
        /// <param name="sequenceNoStart"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool CheckRepeatPartNoStart(string partNo, int id)
        {
            string sql = $@"select 1 from hts_misc.Prod_line_Material t where t.PartNo = @PartNo and t.ID = @Id limit 1";
            var result = _db.Instance.Ado.SqlQuerySingle<int>(sql, new { PartNo = partNo, Id = id });
            if (result > 0)     //PartNo未修改
                return true;
            else
            {
                string sql1 = $@"select 1 from hts_misc.Prod_line_Material t where t.PartNo <> @PartNo and t.ID = @Id limit 1";
                var result1 = _db.Instance.Ado.SqlQuerySingle<int>(sql1, new { PartNo = partNo, Id = id });
                if (result1 > 0)    //PartNo修改了
                {
                    string sql2 = $@"select 1 from hts_misc.Prod_line_Material t where t.PartNo = @PartNo and t.ID <> @Id limit 1";
                    var result2 = _db.Instance.Ado.SqlQuerySingle<int>(sql2, new { PartNo = partNo, Id = id });
                    if (result2 > 0)  //PartNo修改了,并且重复
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                    return true;

            }

        }

        public bool UpdateTag(ProdLineManageModel tag)
        {
            GetNewConnByPcs("misc");
            return _db.Instance.Updateable(tag).ExecuteCommand() > 0;
        }

        //public ProdLineManageModel GetLatestData(ProdLineManageModel prodLineManageModel)
        //{
        //    if (!string.IsNullOrEmpty(tagsModel.MachineKind))
        //    {
        //        string sql = $@"select t.* from tags t where t.MachineKind = @machineKind order by t.createtime desc limit 1";
        //        return _db.Instance.Ado.SqlQuerySingle<TagsModel>(sql, new { machineKind = prodLineManageModel.MachineKind });
        //    }
        //    else
        //    {
        //        return null;
        //    }

        //}
    }

}
