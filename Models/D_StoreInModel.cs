using mar_sumaken_web.Commons;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace mar_sumaken_web.Models
{
    /// <summary>
    /// 入庫実績テーブルのModel
    /// </summary>
    public class D_StoreInModel: CommonModel
    {
        /// <summary>
        /// 検索条件一覧
        /// </summary>
        public List<SearchConditionModel>? SearchConditions { get; set; }

        /// <summary>
        /// 入庫実績リスト
        /// </summary>
        public IPagedList<D_StoreInModel>? D_StoreInList { get; set; }

        /// <summary>
        /// 検索入庫日(開始)
        /// </summary>
        [Display(Name = "入庫日")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? DateSearchStart { get; set; }

        /// <summary>
        /// 検索入庫日(終了)
        /// </summary>
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? DateSearchEnd { get; set; }

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
        /// 選択された倉庫ID
        /// </summary>
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int SelectedDepoID { get; set; }

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
        /// 入庫実績ID
        /// </summary>
        [Display(Name = "ID")]
        public long StoreInID { set; get; }

        /// <summary>
        /// 倉庫ID
        /// </summary>
        [Display(Name = "倉庫ID")]
        public int DepoID { set; get; }

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
        /// 入庫日
        /// </summary>
        [Display(Name = "入庫日")]
        public DateTime StoreInDate { set; get; }

        /// <summary>
        /// 仕入先品番
        /// </summary>
        [Display(Name = "仕入先品番")]
        public string? SupplierProductNumber { set; get; }

        /// <summary>
        /// ロット番号
        /// </summary>
        [Display(Name = "ロット番号")]
        public string? LotNumber { set; get; }

        /// <summary>
        /// 収容数
        /// </summary>
        [Display(Name = "収容数")]
        public int LotQuantity { set; get; }

        /// <summary>
        /// メインキー
        /// </summary>
        [Display(Name = "メインキー")]
        public string? MainProductKey { set; get; }

        /// <summary>
        /// サブキー1
        /// </summary>
        [Display(Name = "サブキー1")]
        public string? FirstSubProductKey { set; get; }

        /// <summary>
        /// サブキー2
        /// </summary>
        [Display(Name = "サブキー2")]
        public string? SecondSubProductKey { set; get; }

        /// <summary>
        /// 箱数
        /// </summary>
        [Display(Name = "箱数")]
        public int NumberOfBoxes { set; get; }

        /// <summary>
        /// 数量
        /// </summary>
        [Display(Name = "数量")]
        public int Quantity { set; get; }

        /// <summary>
        /// 備考
        /// </summary>
        [Display(Name = "備考")]
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

        // <summary>
        /// 初期値設定
        /// </summary>
        public D_StoreInModel()
        {
            var now = DateTime.Today.ToString("yyyy/MM/dd"); //　現在日
            DateSearchStart = now;
            DateSearchEnd = now;
        }
    }
}
