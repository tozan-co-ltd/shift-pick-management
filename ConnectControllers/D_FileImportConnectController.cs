using Dapper;
using System.Data.SqlClient;
using  ai_truck_load_measurement.Models;
using  ai_truck_load_measurement.Commons;

namespace  ai_truck_load_measurement.ConnectControllers
{
    /// <summary>
    /// ファイル取込実績テーブルに関する関数
    /// </summary>
    public static class D_FileImportConnectController
    {
        /// <summary>
        /// ファイル取込実績情報取得
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <param name="databaseName">データベース名</param>
        /// <returns></returns>
        public static List<D_FileImportModel> ConnectDFileImports(string sql, string databaseName)
        {
            // 戻り値
            List<D_FileImportModel> strList = new();

            // DB接続
            try
            {
                // SQLServer接続文字列取得
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionString(databaseName);
                // SQLServer接続
                using (var connection = new SqlConnection())
                {
                    connection.ConnectionString = connectionString;
                    connection.Open();

                    strList = connection.Query<D_FileImportModel>(sql).ToList();
                }
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// ファイル取込実績一覧取得SQL作成
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDFileImports(string menuName)
        {
            var sql = $@"
                        SELECT 
                            FORMAT (CreatedAt, 'yyyy/MM/dd HH:mm:ss') AS CreatedAt,
                            ImportFileName,
                            CreatedBy
                        FROM 
	                        D_FileImport
                        WHERE
                            MenuName = '{menuName}'
                            AND CreatedAt >= DATEADD(MONTH, -1, GETDATE());
            ";

            return sql;
        }

        /// <summary>
        /// ファイル取込実績テーブルINSERTのSQL作成
        /// </summary>
        /// <param name="model">モデル</param>
        /// <param name="createdAt">システム</param>
        /// <param name="createdBy"></param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToInsertDFileImport(D_FileImportModel model, DateTime createdAt, string createdBy)
        {
            var sql = $@"
                    INSERT INTO D_FileImport
                            (DepoID, MenuName, ImportFileName, CreatedAt, CreatedBy)
                    VALUES ({model.DepoID}, '{model.MenuName}', '{model.ImportFileName}', '{createdAt}', '{createdBy}');
            ";
            return sql;
        }
    }
}
