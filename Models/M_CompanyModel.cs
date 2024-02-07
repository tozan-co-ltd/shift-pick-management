namespace mar_sumaken_web.Models
{
    /// <summary>
    /// 会社マスターのModel
    /// </summary>
    public class M_CompanyModel
    {
        /// <summary>
        /// 会社ID
        /// </summary>
        public int CompanyID { set; get; }

        /// <summary>
        /// 会社コード
        /// </summary>
        public int CompanyCode { set; get; }

        /// <summary>
        /// 会社区分
        /// </summary>
        public int CompanyKubun { set; get; }

        /// <summary>
        /// 会社区分名
        /// </summary>
        public string CompanyKubunName { set; get; }

        /// <summary>
        /// 会社名
        /// </summary>
        public string CompanyName { set; get; }

        /// <summary>
        /// 取引先名
        /// </summary>
        public string ClientName { set; get; }

        /// <summary>
        /// 削除フラグ
        /// </summary>
        public bool IsDeleted { set; get; }

        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreatedAt { set; get; }

        /// <summary>
        /// 作成者
        /// </summary>
        public string CreatedBy { set; get; }

        /// <summary>
        /// 更新日時
        /// </summary>
        public DateTime UpdatedAt { set; get; }

        /// <summary>
        /// 更新者
        /// </summary>
        public string UpdatedBy { set; get; }
    }
}
