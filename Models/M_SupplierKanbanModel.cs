using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace ai_truck_load_measurement.Models
{
    /// <summary>
    /// 仕入先かんばんマスターのModel
    /// </summary>
    public class M_SupplierKanbanModel : CommonModel
    {
        /// <summary>
        /// 仕入先かんばんリスト
        /// </summary>
        public IPagedList<M_SupplierKanbanModel>? M_SupplierKanbanList { get; set; }

        /// <summary>
        /// 検索倉庫リスト
        /// </summary>
        public IEnumerable<SelectListItem>? SearchDepoList
        {
            get
            {
                return MDepoList;
            }
        }

        /// <summary>
        /// 検索会社リスト
        /// </summary>
        public List<SelectListItem>? SearchCompanyList { get; set; }

        /// <summary>
        /// 選択仕入先リスト
        /// </summary>
        public List<SelectListItem> SuplierSelectList { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// 選択された仕入先ID
        /// </summary>
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int SelectedSupplierID { get; set; }

        /// <summary>
        /// 選択倉庫リスト
        /// </summary>
        public List<SelectListItem> DepoSelectList { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// 選択された倉庫ID
        /// </summary>
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int SelectedDepoID { get; set; }

        /// <summary>
        /// ハンディメニューリスト
        /// </summary>
        public List<M_HandyMenuModel> M_HandyMenuList { get; set; } = new List<M_HandyMenuModel>();

        /// <summary>
        /// 選択ハンディメニューリスト
        /// </summary>
        public List<SelectListItem> HandyMenuSelectList { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// 仕入先かんばんID
        /// </summary>
        [Display(Name = "ID")]
        public int SupplierKanbanID { get; set; }

        /// <summary>
        /// 倉庫ID
        /// </summary>
        [Display(Name = "倉庫ID")]
        public int DepoID { set; get; }

        /// <summary>
        /// 倉庫名
        /// </summary>
        [Display(Name = "倉庫名")]
        public string? DepoName { get; set; }

        /// <summary>
        /// 会社ID:仕入先の会社ID
        /// </summary>
        [Display(Name = "会社ID")]
        public int SupplierID { set; get; }

        /// <summary>
        /// 仕入先名
        /// </summary>
        [Display(Name = "仕入先名")]
        public string? SupplierName { get; set; }

        /// <summary>
        /// 仕入先かんばん名
        /// </summary>
        [Display(Name = "仕入先かんばん名")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [MaxLength(50, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string SupplierKanbanName { get; set; }

        /// <summary>
        /// 重複許容フラグ
        /// </summary>
        [Display(Name = "重複許容フラグ")]
        public string AllowedDuplicatesFlag { get; set; }

        /// <summary>
        /// 識別文字
        /// </summary>
        [Display(Name = "識別文字")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string IdentifyString { get; set; }

        /// <summary>
        /// 識別文字開始位置
        /// </summary>
        [Display(Name = "識別文字開始位置")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int IdentifyStringStartIndex { get; set; }

        /// <summary>
        /// 品番桁数
        /// </summary>
        [Display(Name = "品番桁数")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int ProductNumberLength { get; set; }

        /// <summary>
        /// 品番開始位置
        /// </summary>
        [Display(Name = "品番開始位置")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int ProductNumberStartIndex { get; set; }

        /// <summary>
        /// 数量桁数
        /// </summary>
        [Display(Name = "数量桁数")]
        [Range(0, int.MaxValue, ErrorMessageResourceName = "E1021", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int QuantityLength { get; set; }

        /// <summary>
        /// 数量開始位置
        /// </summary>
        [Display(Name = "数量開始位置")]
        [Range(0, int.MaxValue, ErrorMessageResourceName = "E1021", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int QuantityStartIndex { get; set; }

        /// <summary>
        /// ロット番号桁数
        /// </summary>
        [Display(Name = "ロット番号桁数")]
        [Range(0, int.MaxValue, ErrorMessageResourceName = "E1021", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int LotLength { get; set; }

        /// <summary>
        /// ロット番号開始位置
        /// </summary>
        [Display(Name = "ロット番号開始位置")]
        [Range(0, int.MaxValue, ErrorMessageResourceName = "E1021", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int LotStartIndex { get; set; }

        /// <summary>
        /// メインキー桁数
        /// </summary>
        [Display(Name = "メインキー桁数")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int MainProductKeyLength { get; set; }

        /// <summary>
        /// メインキー開始位置
        /// </summary>
        [Display(Name = "メインキー開始位置")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int MainProductKeyStartIndex { get; set; }

        /// <summary>
        /// サブキー1桁数
        /// </summary>
        [Display(Name = "サブキー1桁数")]
        [Range(0, int.MaxValue, ErrorMessageResourceName = "E1021", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int FirstSubProductKeyLength { get; set; }

        /// <summary>
        /// サブキー1開始位置
        /// </summary>
        [Display(Name = "サブキー1開始位置")]
        [Range(0, int.MaxValue, ErrorMessageResourceName = "E1021", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int FirstSubProductKeyStartIndex { get; set; }

        /// <summary>
        /// サブキー2桁数
        /// </summary>
        [Display(Name = "サブキー2桁数")]
        [Range(0, int.MaxValue, ErrorMessageResourceName = "E1021", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int SecondSubProductKeyLength { get; set; }

        /// <summary>
        /// サブキー2開始位置
        /// </summary>
        [Display(Name = "サブキー2開始位置")]
        [Range(0, int.MaxValue, ErrorMessageResourceName = "E1021", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int SecondSubProductKeyStartIndex { get; set; }

        /// <summary>
        /// 枝番桁数
        /// </summary>
        [Display(Name = "枝番桁数")]
        [Range(0, int.MaxValue, ErrorMessageResourceName = "E1021", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int ProductBranchNumberLength { get; set; }

        /// <summary>
        /// 枝番開始位置
        /// </summary>
        [Display(Name = "枝番開始位置")]
        [Range(0, int.MaxValue, ErrorMessageResourceName = "E1021", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int ProductBranchNumberStartIndex { get; set; }

        /// <summary>
        /// 注文番号桁数
        /// </summary>
        [Display(Name = "注文番号桁数")]
        [Range(0, int.MaxValue, ErrorMessageResourceName = "E1021", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int OrderNumberLength { get; set; }

        /// <summary>
        /// 注文番号開始位置
        /// </summary>
        [Display(Name = "注文番号開始位置")]
        [Range(0, int.MaxValue, ErrorMessageResourceName = "E1021", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int OrderNumberStartIndex { get; set; }

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
        [Display(Name = "更新日時")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 更新者
        /// </summary>
        [Display(Name = "更新者")]
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// QRコード文字列
        /// </summary>
        [Display(Name = "QRコード文字列")]
        public string? QRCodeString { get; set; }
    }
}
