using mar_sumaken_web.Commons;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace mar_sumaken_web.Models
{
    /// <summary>
    /// 入荷予定テーブルのModel
    /// </summary>
    public class D_ReceiveScheduleModel : CommonModel
    {
        /// <summary>
        /// 入荷予定リスト
        /// </summary>
        public IPagedList<D_ReceiveScheduleModel>? D_ReceiveScheduleList { set; get; }

        /// <summary>
        /// 検索入荷予定日(開始)
        /// </summary>
        [Display(Name = "入荷予定日")]
        public string SearchStartDate { get; set; }

        /// <summary>
        /// 検索入荷予定日(終了)
        /// </summary>
        public string SearchEndDate { get; set; }

        /// <summary>
        /// 実績数不一致のみ
        /// </summary>
        public bool DiffenceCountCheck { get; set; }

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
        public int SelectedCompanyID { get; set; }

        /// <summary>
        /// 取込ファイル名
        /// </summary>
        public string? ImportFileName { get; set; }

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
        /// 会社コード
        /// </summary>
        [Display(Name = "会社コード")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [RegularExpression(Utils.NumberOnlyRegex, ErrorMessageResourceName = "E1007", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? CompanyCode { get; set; }

        /// <summary>
        /// 入荷予定ID
        /// </summary>
        [Display(Name = "ID")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int ReceiveScheduleID { get; set; }

        /// <summary>
        /// 入荷予定日
        /// </summary>
        [Display(Name = "入荷予定日")]
        [RegularExpression(Utils.DateTimeSlashRegex, ErrorMessageResourceName = "E1004", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string ReceiveScheduleDate { get; set; }

        /// <summary>
        /// 仕入先品番
        /// </summary>
        [Display(Name = "仕入先品番")]
        [MaxLength(50, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? SupplierProductNumber { get; set; }

        /// <summary>
        /// ロット番号
        /// </summary>
        [Display(Name = "ロット番号")]
        [MaxLength(50, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? LotNumber { get; set; }

        /// <summary>
        /// 予定箱数
        /// </summary>
        [Display(Name = "予定箱数")]
        public int NumberOfBoxes { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        [Display(Name = "数量")]
        [RegularExpression(Utils.NumberOnlyRegex, ErrorMessageResourceName = "E1007", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string Quantity { get; set; }

        /// <summary>
        /// 入庫箱数
        /// </summary>
        [Display(Name = "入庫箱数")]
        public int StoreInNumberOfBox { get; set; }

        /// <summary>
        /// 入庫数量
        /// </summary>
        [Display(Name = "入庫数量")]
        public int StoreInQuantity { get; set; }

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
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 更新者
        /// </summary>
        [Display(Name = "更新者")]
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// 初期値設定
        /// </summary>
        public D_ReceiveScheduleModel()
        {
            // 現在日
            var now = DateTime.Today.ToString("yyyy/MM/dd");
            // 1ヶ月前
            var oneWeeklater = DateTime.Today.AddDays(+7).ToString("yyyy/MM/dd");

            SearchStartDate = now;
            SearchEndDate = oneWeeklater;
        }
    }
}
