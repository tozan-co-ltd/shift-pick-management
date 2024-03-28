using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace mar_sumaken_web.Models
{
    /// <summary>
    /// 会社マスターのModel
    /// </summary>
    public class M_CompanyModel : CommonModel
    {
        /// <summary>
        /// 会社マスターリスト
        /// </summary>
        public IPagedList<M_CompanyModel>? M_CompanyList { get; set; }

        /// <summary>
        /// 会社区分リスト
        /// </summary>
        public List<SelectListItem> KubunSelectList { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// 会社ID
        /// </summary>
        [Display(Name = "ID")]
        public int CompanyID { get; set; }

        /// <summary>
        /// 会社コード
        /// </summary>
        [Display(Name = "会社コード")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [RegularExpression(@"[0-9]+", ErrorMessageResourceName = "E1007", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int CompanyCode { get; set; }

        /// <summary>
        /// 会社区分
        /// </summary>
        [Display(Name = "会社区分")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int CompanyKubun { get; set; }

        /// <summary>
        /// 会社区分名
        /// </summary>
        public string? CompanyKubunName { get; set; }

        /// <summary>
        /// 会社名
        /// </summary>
        [Display(Name = "会社名")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [MaxLength(50, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? CompanyName { get; set; }

        /// <summary>
        /// 取引先名
        /// </summary>
        [Display(Name = "取引先名")]
        public string? ClientName { get; set; }

        /// <summary>
        /// 削除フラグ
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// 作成日時
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 作成者
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        [Display(Name = "更新日時")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 更新者
        /// </summary>
        [Display(Name = "更新者")]
        public string? UpdatedBy { get; set; }
    }
}
