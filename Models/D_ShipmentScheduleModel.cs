using mar_sumaken_web.Commons;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace mar_sumaken_web.Models
{
    /// <summary>
    /// 出荷指示テーブルのModel
    /// </summary>
    public class D_ShipmentScheduleModel : CommonModel
    {
        /// <summary>
        /// 出荷指示リスト
        /// </summary>
        public IPagedList<D_ShipmentScheduleModel> D_ShipmentScheduleList { get; set; }

        /// <summary>
        /// 検索納入指示日(開始)
        /// </summary>
        [Display(Name = "納入指示日")]
        public string SearchStartDate { get; set; }

        /// <summary>
        /// 検索納入指示日(終了)
        /// </summary>
        public string SearchEndDate { get; set; }

        /// <summary>
        /// 検索倉庫リスト
        /// </summary>
        public IEnumerable<SelectListItem> SearchDepoList
        {
            get
            {
                return MDepoList;
            }
        }

        /// <summary>
        /// 選択された倉庫ID
        /// </summary>
        public int SelectedDepoID { get; set; }

        /// <summary>
        /// 検索会社リスト
        /// </summary>
        public IEnumerable<SelectListItem> SearchCompanyList { get; set; }

        /// <summary>
        /// 選択された会社ID
        /// </summary>
        public int SelectedCompanyID { get; set; }

        /// <summary>
        /// 取込ファイル名
        /// </summary>
        public string? ImportFileName { get; set; }

        /// <summary>
        /// 出荷指示実績ID
        /// </summary>
        public int ShipmentScheduleID { get; set; }

        /// <summary>
        /// 会社ID:仕入先の会社ID
        /// </summary>
        [Display(Name = "会社ID")]
        public int SupplierID { set; get; }

        /// <summary>
        /// 仕入先名
        /// </summary>
        [Display(Name = "仕入先名")]
        public string? SupplierName { set; get; }

        /// <summary>
        /// 発注元
        /// </summary>
        [Display(Name = "発注元")]
        [MaxLength(10, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? OrdererCode { get; set; }

        /// <summary>
        /// 発注元工区
        /// </summary>
        [Display(Name = "発注元工区")]
        [MaxLength(5, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? OrdererFactoryKubun { get; set; }

        /// <summary>
        /// 発注元名称
        /// </summary>
        [Display(Name = "発注元名称")]
        [MaxLength(50, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? OrdererName { get; set; }

        /// <summary>
        /// 発注元工場名
        /// </summary>
        [Display(Name = "発注元工場名")]
        [MaxLength(50, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? OrdererFactoryName { get; set; }

        /// <summary>
        /// 出荷元
        /// </summary>
        [Display(Name = "出荷元")]
        [MaxLength(10, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? ShipperCode { get; set; }

        /// <summary>
        /// 出荷元工区
        /// </summary>
        [Display(Name = "出荷元工区")]
        [MaxLength(5, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? ShipperFactoryKubun { get; set; }

        /// <summary>
        /// 出荷元名称
        /// </summary>
        [Display(Name = "出荷元名称")]
        [MaxLength(50, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? ShipperName { get; set; }

        /// <summary>
        /// 納入先
        /// </summary>
        [Display(Name = "納入先")]
        [MaxLength(10, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? DeliveryCode { get; set; }

        /// <summary>
        /// 納入先工区
        /// </summary>
        [Display(Name = "納入先工区")]
        [MaxLength(5, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? DeliveryFactoryKubun { get; set; }

        /// <summary>
        /// 納入場所
        /// </summary>
        [Display(Name = "納入場所")]
        [MaxLength(5, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? DeliveryLocation { get; set; }


        /// <summary>
        /// 納入先名称
        /// </summary>
        [Display(Name = "納入先名称")]
        [MaxLength(50, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? DeliveryName { get; set; }

        /// <summary>
        /// 納入先工場名
        /// </summary>
        [Display(Name = "納入先工場名")]
        [MaxLength(50, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? DeliveryFactoryName { get; set; }

        /// <summary>
        /// 定期／不定期区分名称
        /// </summary>
        [Display(Name = "定期／不定期区分名称")]
        [MaxLength(50, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? RegularKubun { get; set; }

        /// <summary>
        /// 発行日
        /// </summary>
        [Display(Name = "発行日")]
        [RegularExpression(Utils.DateTimeNoneSlashRegex, ErrorMessageResourceName = "E1005", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? IssuedDate { get; set; }

        /// <summary>
        /// 納入指示日
        /// </summary>
        [Display(Name = "納入指示日")]
        [RegularExpression(Utils.DateTimeNoneSlashRegex, ErrorMessageResourceName = "E1005", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? DeliveryDate { get; set; }

        /// <summary>
        /// 納入指示時刻
        /// </summary>
        [Display(Name = "納入指示時刻")]
        [RegularExpression(@"^(?:[01]\d|2[0-3])[0-5]\d$", ErrorMessageResourceName = "E1006", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? DeliveryTime { get; set; }

        /// <summary>
        /// 便
        /// </summary>
        [Display(Name = "便")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [RegularExpression(Utils.NumberOnlyRegex, ErrorMessageResourceName = "E1007", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? DeliveryTimeClass { get; set; }

        /// <summary>
        /// 輸送識別
        /// </summary>
        [Display(Name = "輸送識別")]
        [MaxLength(50, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? TranspotationIdentify { get; set; }

        /// <summary>
        /// 納品書番号
        /// </summary>
        [Display(Name = "納品書番号")]
        [MaxLength(50, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? DeliverySlipNumber { get; set; }

        /// <summary>
        /// ページ数
        /// </summary>
        [Display(Name = "ページ数")]
        [RegularExpression(Utils.NumberOnlyRegex, ErrorMessageResourceName = "E1007", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? DeliverySlipPageNumber { get; set; }

        /// <summary>
        /// 行No
        /// </summary>
        [Display(Name = "行No")]
        [RegularExpression(Utils.NumberOnlyRegex, ErrorMessageResourceName = "E1007", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? DeliverySlipRowNumber { get; set; }

        /// <summary>
        /// 納入先品番(表示用品番)
        /// </summary>
        [Display(Name = "表示用品番")]
        [MaxLength(50, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? DeliveryProductNumber { get; set; }

        /// <summary>
        /// 背番号
        /// </summary>
        [Display(Name = "背番号")]
        [MaxLength(50, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? DeliveryProductAbbreviation { get; set; }

        /// <summary>
        /// 品名
        /// </summary>
        [Display(Name = "品名")]
        [MaxLength(50, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? DeliveryProductName { get; set; }

        /// <summary>
        /// 収容数
        /// </summary>
        [Display(Name = "収容数")]
        [RegularExpression(Utils.NumberOnlyRegex, ErrorMessageResourceName = "E1007", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? LotQuantity { get; set; }

        /// <summary>
        /// 枝番
        /// </summary>
        [Display(Name = "枝番")]
        [RegularExpression(Utils.NumberOnlyRegex, ErrorMessageResourceName = "E1007", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? BranchNumber { get; set; }

        /// <summary>
        /// 納入指示数
        /// </summary>
        [Display(Name = "納入指示数")]
        [RegularExpression(Utils.NumberOnlyRegex, ErrorMessageResourceName = "E1007", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? Quantity { get; set; }

        /// <summary>
        /// 出庫数量 
        /// </summary>
        [Display(Name = "出庫数量 ")]
        public int StoreOutQuantity { get; set; }

        /// <summary>
        /// 仕入先品番
        /// </summary>
        [Display(Name = "仕入先品番")]
        public string? SupplierProductNumber { get; set; }

        /// <summary>
        /// 箱数
        /// </summary>
        [Display(Name = "箱数")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int NumberOfBoxes { get; set; }

        /// <summary>
        /// 出庫箱数
        /// </summary>
        [Display(Name = "出庫箱数")]
        public int StoreOutNumberOfBoxes { get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 更新者
        /// </summary>
        public string UpdatedBy { get; set; } = string.Empty;

        /// <summary>
        /// 初期値設定
        /// </summary>
        public D_ShipmentScheduleModel()
        {
            // 現在日
            var now = DateTime.Today.AddDays(+1).ToString("yyyy/MM/dd");
            SearchStartDate = now;
            SearchEndDate = now;
        }
    }
}
