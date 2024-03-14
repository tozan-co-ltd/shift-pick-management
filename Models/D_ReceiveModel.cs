using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace mar_sumaken_web.Models
{
    /// <summary>
    /// 入荷予定照会テーブルのModel
    /// </summary>
    public class D_ReceiveModel : CommonModel
    {
        /// <summary>
        /// 入荷予定照会リスト
        /// </summary>
        public IPagedList<D_ReceiveModel>? DReceiveModelList { get; set; }

        /// <summary>
        /// 検索入荷日(開始)
        /// </summary>
        [Display(Name = "入荷日")]
        public string? SearchStartDate { get; set; }

        /// <summary>
        /// 検索入荷日(終了)
        /// </summary>
        public string? SearchEndDate { get; set; }

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
        /// 初期値設定
        /// </summary>
        public D_ReceiveModel()
        {
            // 現在日
            var now = DateTime.Today.ToString("yyyy/MM/dd");
            SearchStartDate = now;
            SearchEndDate = now;
        }

        /// <summary>
        /// 倉庫ID
        /// </summary>
        [Display(Name = "倉庫ID")]
        public int DepoID { set; get; }

        /// <summary>
        /// 倉庫名
        /// </summary>
        [Display(Name = "倉庫名")]
        public string? DepoName { set; get; }

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
        /// 入荷実績ID
        /// </summary>
        [Display(Name = "ID")]
        public int ReceiveID { get; set; }

        /// <summary>
        /// 読取実績ID
        /// </summary>
        [Display(Name = "読取実績ID")]
        public int ScanResultID { get; set; }

        /// <summary>
        /// 入荷日
        /// </summary>
        [Display(Name = "入荷日")]
        public string? ReceiveDate { get; set; }

        /// <summary>
        /// 仕入先品番
        /// </summary>
        [Display(Name = "仕入先品番")]
        public string? SupplierProductNumber { get; set; }

        /// <summary>
        /// ロット番号
        /// </summary>
        [Display(Name = "ロット番号")]
        public string? LotNumber { get; set; }

        /// <summary>
        /// 箱数
        /// </summary>
        [Display(Name = "箱数")]
        public int NumberOfBoxes { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        [Display(Name = "数量")]
        public string? Quantity { get; set; }

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
        /// 登録日時
        /// </summary>
        [Display(Name = "登録日時")]
        public DateTime CreatedAt { set; get; }

        /// <summary>
        /// 読取日時
        /// </summary>
        [Display(Name = "読取日時")]
        public string? ScanedAt { get; set; }

        /// <summary>
        /// 登録者
        /// </summary>
        [Display(Name = "登録者")]
        public string? CreatedBy { set; get; }

        /// <summary>
        /// ハンディメニューID
        /// </summary>
        [Display(Name = "ハンディメニューID")]
        public int HandyMenuID { set; get; }

        /// <summary>
        /// ハンディメニュー名
        /// </summary>
        [Display(Name = "ハンディメニュー名")]
        public string? HandyMenuName { set; get; }

        /// <summary>
        /// 仕入先かんばんID
        /// </summary>
        [Display(Name = "仕入先かんばんID")]
        public int SupplierKanbanID { set; get; }

        /// <summary>
        /// 入力箱数
        /// </summary>
        [Display(Name = "入力箱数")]
        public int NumberOfInputBoxes { set; get; }

        /// <summary>
        /// スキャン文字列1
        /// </summary>
        [Display(Name = "スキャン文字列1")]
        public string? FirstScanedString { set; get; }

        /// <summary>
        /// スキャン文字列2
        /// </summary>
        [Display(Name = "スキャン文字列2")]
        public string? SecondScanedString { set; get; }

        /// <summary>
        /// 登録者
        /// </summary>
        [Display(Name = "登録者")]
        public string? ScanCreatedBy { set; get; }

    }
}
