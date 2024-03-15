using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace mar_sumaken_web.Models
{
    /// <summary>
    /// 出庫実績テーブルのModel
    /// </summary>
    public class D_StoreOutModel : CommonModel
    {
        /// <summary>
        /// 出庫実績リスト
        /// </summary>
        public IPagedList<D_StoreOutModel>? D_StoreOutList { get; set; }

        /// <summary>
        /// 出庫実績登録リスト
        /// </summary>
        public List<D_StoreOutModel>? RegisterList { get; set; }

        /// <summary>
        /// 便-納品書番号
        /// </summary>
        [Display(Name = "便・納品書番号")]
        public List<SelectListItem>? BinSelectedList { get; set; }

        /// <summary>
        /// 便の選択
        /// </summary>
        public string? SelectedBin { get; set; }

        /// <summary>
        /// 検索出庫日(開始)
        /// </summary>
        [Display(Name = "出庫日")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string SearchStartDate { get; set; }

        /// <summary>
        /// 検索出庫日(終了)
        /// </summary>
        public string SearchEndDate { get; set; }

        /// <summary>
        /// 検索納入指示日
        /// </summary>
        [Display(Name = "納入指示日")]
        public string? SearchDeliveryDate { set; get; }

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
        public IEnumerable<SelectListItem>? SearchCompanyList { get; set; }

        /// <summary>
        /// 選択された会社ID
        /// </summary>
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int SelectedCompanyID { get; set; }

        /// <summary>
        /// 出庫実績ID
        /// </summary>
        [Display(Name = "ID")]
        public long StoreOutID { set; get; }

        /// <summary>
        /// 倉庫ID
        /// </summary>
        [Display(Name = "倉庫ID")]
        public int DepoID { set; get; }

        /// <summary>
        /// 倉庫名
        /// </summary>
        [Display(Name = "倉庫名")]
        public int DepoName { set; get; }

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
        /// 出庫日
        /// </summary>
        [Display(Name = "出庫日")]
        public DateTime StoreOutDate { set; get; }

        /// <summary>
        /// 納入指示日
        /// </summary>
        [Display(Name = "納入指示日")]
        public DateTime DeliveryDate { set; get; }

        /// <summary>
        /// 便
        /// </summary>
        [Display(Name = "便")]
        public int DeliveryTimeClass { set; get; }

        /// <summary>
        /// 納品書番号
        /// </summary>
        [Display(Name = "納品書番号")]
        public string? DeliverySlipNumber { set; get; }

        /// <summary>
        /// 納入先品番
        /// </summary>
        [Display(Name = "納入先品番")]
        [MaxLength(50, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? DeliveryProductNumber { set; get; }

        /// <summary>
        /// 仕入先品番
        /// </summary>
        [Display(Name = "仕入先品番")]
        [MaxLength(50, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? SupplierProductNumber { set; get; }

        /// <summary>
        /// ロット番号
        /// </summary>
        [Display(Name = "ロット番号")]
        [MaxLength(50, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? LotNumber { set; get; }

        /// <summary>
        /// 収容数
        /// </summary>
        [Display(Name = "収容数")]
        [RegularExpression(@"[0-9]{1,10}", ErrorMessageResourceName = "E1007", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int LotQuantity { set; get; }

        /// <summary>
        /// メインキー
        /// </summary>
        [Display(Name = "メインキー")]
        [MaxLength(50, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? MainProductKey { set; get; }

        /// <summary>
        /// サブキー1
        /// </summary>
        [Display(Name = "サブキー1")]
        [MaxLength(50, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? FirstSubProductKey { set; get; }

        /// <summary>
        /// サブキー2
        /// </summary>
        [Display(Name = "サブキー2")]
        [MaxLength(50, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? SecondSubProductKey { set; get; }

        /// <summary>
        /// 箱数
        /// </summary>
        [Display(Name = "箱数")]
        [RegularExpression(@"[0-9]{1,10}", ErrorMessageResourceName = "E1007", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int NumberOfBoxes { set; get; }

        /// <summary>
        /// 数量
        /// </summary>
        [Display(Name = "数量")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int Quantity { set; get; }

        /// <summary>
        /// 備考
        /// </summary>
        [Display(Name = "備考")]
        [MaxLength(1000, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? Remarks { set; get; }

        /// <summary>
        /// 登録日時
        /// </summary>
        [Display(Name = "登録日時")]
        public DateTime CreatedAt { set; get; }

        /// <summary>
        /// 登録者
        /// </summary>
        [Display(Name = "登録者")]
        public string? CreatedBy { set; get; }

        /// <summary>
        /// 更新日時
        /// </summary>
        [Display(Name = "更新日時")]
        public DateTime UpdatedAt { set; get; }

        /// <summary>
        /// 更新者
        /// </summary>
        [Display(Name = "更新者")]
        public string? UpdatedBy { set; get; }

        /// <summary>
        /// 初期値設定
        /// </summary>
        public D_StoreOutModel()
        {
            // 現在日
            var now = DateTime.Today.ToString("yyyy/MM/dd");
            SearchStartDate = now;
            SearchEndDate = now;
            SearchDeliveryDate = now;
        }
        
    }
}
