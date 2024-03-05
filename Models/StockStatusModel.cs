using mar_sumaken_web.Commons;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace mar_sumaken_web.Models
{
    /// <summary>
    /// 在庫照会のModel
    /// </summary>
    public class StockStatusModel : CommonModel
    {
        /// <summary>
        /// 在庫リスト
        /// </summary>
        public IPagedList<StockStatusModel>? StockStatusList { get; set; }

        /// <summary>
        /// 年月日
        /// </summary>
        [Display(Name = "年月日")]
        [RegularExpression(Utils.DateTimeSlashRegex, ErrorMessageResourceName = "E1004", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? DateSearchStart { get; set; }

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
        public StockStatusModel()
        {
            // 現在日
            var now = DateTime.Today.ToString("yyyy/MM/dd");
            DateSearchStart = now;
        }

        /// <summary>
        /// 品番ID
        /// </summary>
        public int ProductID { get; set; }

        /// <summary>
        ///倉庫ID
        /// </summary>
        public int DepoID { get; set; }

        /// <summary>
        ///倉庫名
        /// </summary>
        public string? DepoName { get; set; }

        /// <summary>
        /// 仕入先ID
        /// </summary>
        public int SupplierID { get; set; }

        /// <summary>
        /// 仕入先名
        /// </summary>
        public string? SupplierName { get; set; }

        /// <summary>
        /// 仕入先品番
        /// </summary>
        public string? SupplierProductNumber { get; set; }

        /// <summary>
        /// 収容数
        /// </summary>
        public int LotQuantity { get; set; }

        /// <summary>
        /// 月初在庫数
        /// </summary>
        public int StockQuantityAtBeginningMonth { get; set; }

        /// <summary>
        /// 入庫箱数
        /// </summary>
        public int StoreInNumberOfBoxes { get; set; }

        /// <summary>
        /// 入庫数
        /// </summary>
        public int StoreInQuantity { get; set; }

        /// <summary>
        /// 出庫箱数
        /// </summary>
        public int StoreOutNumberOfBoxes { get; set; }

        /// <summary>
        /// 出庫数
        /// </summary>
        public int StoreOutQuantity { get; set; }

        /// <summary>
        /// 在庫数
        /// </summary>
        public int StockRemainQuantity { get; set; }


    }
}
