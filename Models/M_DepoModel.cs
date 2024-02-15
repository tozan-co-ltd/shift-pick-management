using X.PagedList;

namespace mar_sumaken_web.Models
{
    /// <summary>
    /// 倉庫マスターのModel
    /// </summary>
    public class M_DepoModel : CommonModel
    {
        /// <summary>
        /// 倉庫リスト
        /// </summary>
        public IPagedList<M_DepoModel> M_DepoList { get; set; }

        /// <summary>
        /// 倉庫ID
        /// </summary>
        public int DepoID { get; set; }

        /// <summary>
        /// 倉庫コード
        /// </summary>
        public int DepoCode { get; set; }

        /// <summary>
        /// 倉庫名
        /// </summary>
        public string DepoName { get; set; }

        /// <summary>
        /// 未使用フラグ
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 作成者
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 更新者
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; }
    }
}
