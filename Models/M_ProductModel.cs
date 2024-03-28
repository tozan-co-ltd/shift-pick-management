using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace mar_sumaken_web.Models
{
    /// <summary>
    /// 品番マスターのModel
    /// </summary>
    public class M_ProductModel : CommonModel
    {
        /// <summary>
        /// 品番リスト
        /// </summary>
        public List<M_ProductModel>? MProductList { get; set; }

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
        public string? RDepoProductNames { get; set; }

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
        public string? SupplierName { get; set; } = string.Empty;

        /// <summary>
        /// 仕入先品番
        /// </summary>
        [Display(Name = "仕入先品番")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [MaxLength(50, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string SupplierProductNumber { get; set; }

        /// <summary>
        /// 納入先ID
        /// </summary>
        [Display(Name = "納入先ID")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int DeliveryID { get; set; }

        /// <summary>
        /// 納入先名
        /// </summary>
        [Display(Name = "納入先名")]
        public string? DeliveryName { get; set; }

        /// <summary>
        /// 納入先品番
        /// </summary>
        [Display(Name = "納入先品番")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [MaxLength(50, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? DeliveryProductNumber { get; set; }

        /// <summary>
        /// 品名
        /// </summary>
        [Display(Name = "品名")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [MaxLength(100, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? ProductName { get; set; }

        /// <summary>
        /// 収容数
        /// </summary>
        [Display(Name = "収容数")]
        [DisplayFormat(DataFormatString = "{0:#,#}")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int LotQuantity { get; set; }

        /// <summary>
        /// 削除フラグ
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        [Display(Name = "作成日時")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 作成者
        /// </summary>
        [Display(Name = "作成者")]
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
