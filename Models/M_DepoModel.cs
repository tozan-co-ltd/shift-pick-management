using mar_sumaken_web.Properties;
using System.ComponentModel.DataAnnotations;
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
        public IPagedList<M_DepoModel>? M_DepoList { get; set; }

        /// <summary>
        /// 倉庫ID
        /// </summary>
        [Display(Name = "ID")]
        public int DepoID { get; set; }

        /// <summary>
        /// 倉庫コード
        /// </summary>
        [Display(Name = "倉庫コード")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [RegularExpression(@"[0-9]+", ErrorMessageResourceName = "E1007", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int DepoCode { get; set; }

        /// <summary>
        /// 倉庫名
        /// </summary>
        [Display(Name = "倉庫名")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [MaxLength(50, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
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
        public string? CreatedBy { get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 更新者
        /// </summary>
        public string? UpdatedBy { get; set; }
    }
}
