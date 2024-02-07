using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace mar_sumaken_web.Models
{
    /// <summary>
    /// 会社マスターのModel
    /// </summary>
    public class M_CompanyModel
    {

        /// <summary>
        /// 会社区分リスト
        /// </summary>
        public List<SelectListItem> KubunSelectList { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// 会社ID
        /// </summary>
        public int CompanyID { set; get; }

        /// <summary>
        /// 会社コード
        /// </summary>
        [Display(Name = "会社コード")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [RegularExpression(@"[0-9]{1,10}")]
        public int CompanyCode { set; get; }

        /// <summary>
        /// 会社区分
        /// </summary>
        [Display(Name = "会社区分")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int CompanyKubun { set; get; }

        /// <summary>
        /// 会社区分名
        /// </summary>
        public string CompanyKubunName { set; get; } = string.Empty;

        /// <summary>
        /// 会社名
        /// </summary>
        [Display(Name = "会社名")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string CompanyName { set; get; }

        /// <summary>
        /// 取引先名
        /// </summary>
        [Display(Name = "取引先名")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string ClientName { set; get; }

        /// <summary>
        /// 削除フラグ
        /// </summary>
        public bool IsDeleted { set; get; } = false;

        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreatedAt { set; get; }

        /// <summary>
        /// 作成者
        /// </summary>
        public string CreatedBy { set; get; } = string.Empty;

        /// <summary>
        /// 更新日時
        /// </summary>
        public DateTime UpdatedAt { set; get; }

        /// <summary>
        /// 更新者
        /// </summary>
        public string UpdatedBy { set; get; } = string.Empty; 
    }
}
