using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace mar_sumaken_web.Models
{
    /// <summary>
    /// 品番マスターのModel
    /// </summary>
    public class M_ProductModel : CommonModel
    {
        /// <summary>
        /// 仕入先リスト
        /// </summary>
        public List<SelectListItem> SuplierSelectList { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// 納入先リスト
        /// </summary>
        public List<SelectListItem> DeliverySelectList { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// 登録使用倉庫リスト
        /// </summary>
        [Display(Name = "使用倉庫名")]
        public List<SelectListItem> RDepoProductsRegister { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// 使用倉庫リスト
        /// </summary>
        public List<M_DepoModel> RDepoProducts { get; set; } = new List<M_DepoModel>();

        /// <summary>
        /// 使用倉庫名
        /// </summary>
        [Display(Name = "使用倉庫名")]
        public string RDepoProductNames { get; set; } = string.Empty;

        /// <summary>
        /// 品番ID
        /// </summary>
        [Display(Name = "ID")]
        public int ProductID { get; set; }

        /// <summary>
        /// 仕入先ID
        /// </summary>
        [Display(Name = "仕入先ID")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int SupplierID { get; set; }

        /// <summary>
        /// 仕入先名
        /// </summary>
        [Display(Name = "仕入先名")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string SupplierName { get; set; } = string.Empty;

        /// <summary>
        /// 仕入先品番
        /// </summary>
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string SupplierProductNumber { get; set; } = string.Empty;

        /// <summary>
        /// 納入先ID
        /// </summary>
        public int DeliveryID { get; set; }

        /// <summary>
        /// 納入先名
        /// </summary>
        [Display(Name = "納入先名")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string DeliveryName { get; set; } = string.Empty;

        /// <summary>
        /// 納入先品番
        /// </summary>
        [Display(Name = "納入先品番")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string DeliveryProductNumber { get; set; } = string.Empty;

        /// <summary>
        /// 品名
        /// </summary>
        [Display(Name = "品名")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// 収容数
        /// </summary>
        [Display(Name = "収容数")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int LotQuantity { get; set; }

        /// <summary>
        /// 未使用フラグ
        /// </summary>
        [Display(Name = "未使用フラグ")]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        [Display(Name = "作成日時")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 作成者
        /// </summary>
        [Display(Name = "作成者")]
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// 更新日時
        /// </summary>
        [Display(Name = "更新日時")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 更新者
        /// </summary>
        [Display(Name = "更新者")]
        public string UpdatedBy { get; set; } = string.Empty;
    }
}
