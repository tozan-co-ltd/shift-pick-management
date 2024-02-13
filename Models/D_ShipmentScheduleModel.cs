using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

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
        public List<D_ShipmentScheduleModel> D_ShipmentScheduleList { get; set; }

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
        /// 検索会社リスト
        /// </summary>
        public IEnumerable<SelectListItem> SearchCompanyList
        {
            get
            {
                return MCompanyList;
            }
        }

        /// <summary>
        /// 取込ファイル名
        /// </summary>
        public string? ImportFileName { get; set; }

        /// <summary>
        /// 選択された倉庫ID
        /// </summary>
        public int SelectedDepoID { get; set; }

        /// <summary>
        /// 選択された倉庫名
        /// </summary>
        public string? SelectedDepoName { get; set; }

        /// <summary>
        /// 選択された会社ID
        /// </summary>
        public int SelectedCompanyID { get; set; }

        /// <summary>
        /// 選択された会社名
        /// </summary>
        [Display(Name = "納入先名")]
        public string SelectedCompanyName { get; set; } = string.Empty;

        /// <summary>
        /// 出荷指示実績ID
        /// </summary>
        public int ShipmentScheduleID { get; set; }

        /// <summary>
        /// 発注元
        /// </summary>
        [DisplayName("発注元")]
        [MaxLength(20)]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? OrdererCode { get; set; }

        /// <summary>
        /// 発注元工区
        /// </summary>
        [DisplayName("発注元工区")]
        [MaxLength(10)]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? OrdererFactoryKubun { get; set; }

        /// <summary>
        /// 発注元名称
        /// </summary>
        [DisplayName("発注元名称")]
        [MaxLength(100)]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? OrdererName { get; set; }

        /// <summary>
        /// 発注元工場名
        /// </summary>
        [DisplayName("発注元工場名")]
        [MaxLength(100)]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? OrdererFactoryName { get; set; }

        /// <summary>
        /// 出荷元
        /// </summary>
        [DisplayName("出荷元")]
        [MaxLength(20)]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? ShipperCode { get; set; }

        /// <summary>
        /// 出荷元工区
        /// </summary>
        [DisplayName("出荷元工区")]
        [MaxLength(10)]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? ShipperFactoryKubun { get; set; }

        /// <summary>
        /// 出荷元名称
        /// </summary>
        [DisplayName("出荷元名称")]
        [MaxLength(100)]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? ShipperName { get; set; }

        /// <summary>
        /// 納入先
        /// </summary>
        [DisplayName("納入先")]
        [MaxLength(20)]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? DeliveryCode { get; set; }

        /// <summary>
        /// 納入先工区
        /// </summary>
        [DisplayName("納入先工区")]
        [MaxLength(10)]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? DeliveryFactoryKubun { get; set; }

        /// <summary>
        /// 納入場所
        /// </summary>
        [DisplayName("納入場所")]
        [MaxLength(10)]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? DeliveryLocation { get; set; }

        /// <summary>
        /// 納入先名称
        /// </summary>
        [DisplayName("納入先名称")]
        [MaxLength(100)]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? DeliveryName { get; set; }

        /// <summary>
        /// 納入先工場名
        /// </summary>
        [DisplayName("納入先工場名")]
        [MaxLength(100)]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? DeliveryFactoryName { get; set; }

        /// <summary>
        /// 定期／不定期区分名称
        /// </summary>
        [DisplayName("定期／不定期区分名称")]
        [MaxLength(100)]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? RegularKubun { get; set; }

        /// <summary>
        /// 発行日
        /// </summary>
        [DisplayName("発行日")]
        [DataType(DataType.Date)]
        public DateTime IssuedDate { get; set; }

        /// <summary>
        /// 納入指示日
        /// </summary>
        [DisplayName("納入指示日")]
        public DateTime DeliveryDate { get; set; }

        /// <summary>
        /// 納入指示時刻
        /// </summary>
        [DisplayName("納入指示時刻")]
        [MaxLength(8)]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? DeliveryTime { get; set; }

        /// <summary>
        /// 便
        /// </summary>
        [DisplayName("便")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int DeliveryTimeClass { get; set; }

        /// <summary>
        /// 輸送識別
        /// </summary>
        [DisplayName("輸送識別")]
        [MaxLength(100)]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? TranspotationIdentify { get; set; }

        /// <summary>
        /// 納品書番号
        /// </summary>
        [DisplayName("納品書番号")]
        [MaxLength(100)]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? DeliverySlipNumber { get; set; }

        /// <summary>
        /// ページ数
        /// </summary>
        [DisplayName("ページ数")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int DeliverySlipPageNumber { get; set; }

        /// <summary>
        /// 行No
        /// </summary>
        [DisplayName("行No")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int DeliverySlipRowNumber { get; set; }

        /// <summary>
        /// 納入先品番(表示用品番)
        /// </summary>
        [DisplayName("表示用品番")]
        [MaxLength(100)]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? DeliveryProductNumber { get; set; }

        /// <summary>
        /// 背番号
        /// </summary>
        [DisplayName("背番号")]
        [MaxLength(100)]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string DeliveryProductAbbreviation { get; set; } = string.Empty;

        /// <summary>
        /// 品名
        /// </summary>
        [DisplayName("品名")]
        [MaxLength(100)]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string DeliveryProductName { get; set; } = string.Empty;

        /// <summary>
        /// 収容数
        /// </summary>
        [DisplayName("収容数")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int LotQuantity { get; set; }

        /// <summary>
        /// 枝番
        /// </summary>
        [DisplayName("枝番")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int BranchNumber { get; set; }

        /// <summary>
        /// 納入指示数
        /// </summary>
        [DisplayName("納入指示数")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int Quantity { get; set; }

        /// <summary>
        /// 仕入先品番
        /// </summary>
        [DisplayName("仕入先品番")]
        [MaxLength(100)]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string SupplierProductNumber { get; set; } = string.Empty;

        /// <summary>
        /// 箱数
        /// </summary>
        [DisplayName("箱数")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int NumberOfBoxes { get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 更新者
        /// </summary>
        public string UpdatedBy { get; set; } = string.Empty;
    }
}
