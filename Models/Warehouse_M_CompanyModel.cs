namespace ai_truck_load_measurement.Models
{
    /// <summary>
    /// Warehouse会社マスターのModel
    /// </summary>
    public class Warehouse_M_CompanyModel
    {
        /// <summary>
        /// 会社ID
        /// </summary>
        public int CompanyID { get; set; }

        /// <summary>
        /// 会社コード
        /// </summary>
        public string CompanyCode { get; set; }

        /// <summary>
        /// 会社パスワード
        /// </summary>
        public string CompanyPassword { get; set; }

        /// <summary>
        /// 会社URL
        /// </summary>
        public string CompanyWebPath { get; set; }

        /// <summary>
        /// 会社名
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// 会社名カナ
        /// </summary>
        public string CompanyNameKana { get; set; }

        /// <summary>
        /// 会社データベース名
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// ハンディアプリの最小バージョン
        /// </summary>
        public decimal HandyAppMinVersion { get; set; }
    }
}
