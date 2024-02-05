namespace mar_sumaken_web.Models
{
    /// <summary>
    /// Warehouse会社マスターのModel
    /// </summary>
    public class Warehouse_M_CompanyModel
    {
        /// <summary>
        /// 会社ID
        /// </summary>
        public int CompanyID { set; get; }

        /// <summary>
        /// 会社コード
        /// </summary>
        public string CompanyCode { set; get; }

        /// <summary>
        /// 会社パスワード
        /// </summary>
        public string CompanyPassword { set; get; }

        /// <summary>
        /// 会社URL
        /// </summary>
        public string CompanyWebPath { set; get; }

        /// <summary>
        /// 会社名
        /// </summary>
        public string CompanyName { set; get; }

        /// <summary>
        /// 会社名かな
        /// </summary>
        public string CompanyNameKana { set; get; }

        /// <summary>
        /// 会社データベース名
        /// </summary>
        public string DatabaseName { set; get; }

        /// <summary>
        /// APIのURL
        /// </summary>
        public string HandyApiUrl { set; get; }

        /// <summary>
        /// ハンディアプリの最小バージョン
        /// </summary>
        public decimal HandyAppMinVersion { set; get; }

        /// <summary>
        /// ハンディ管理者パスワード
        /// </summary>
        public string HandyAdminPassword { set; get; }

        /// <summary>
        /// 未使用フラグ
        /// </summary>
        public bool IsDeleted { set; get; }

        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 作成者
        /// </summary>
        public string CreateUserID { get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        public DateTime UpdateDate { get; set; }

        /// <summary>
        /// 更新者
        /// </summary>
        public string UpdateUserID { get; set; }
    }
}
