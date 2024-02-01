namespace mar_sumaken_web.Models
{
    public class M_DepoModel
    {
        /// <summary>
        /// 倉庫ID
        /// </summary>
        public int DepoID { set; get; }

        /// <summary>
        /// 倉庫コード
        /// </summary>
        public int DepoCode { set; get; }

        /// <summary>
        /// 倉庫名
        /// </summary>
        public string DepoName { set; get; }

        /// <summary>
        /// 未使用フラグ
        /// </summary>
        public bool NotUseFlag { set; get; }

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
