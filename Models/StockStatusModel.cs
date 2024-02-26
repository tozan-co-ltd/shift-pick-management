using Microsoft.AspNetCore.Mvc.ApplicationModels;
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
        public IPagedList<StockStatusModel> StockStatusList { get; set; }

        /// <summary>
        /// 年月日
        /// </summary>
        [Display(Name = "年月日")]
        public string DateSearchStart { get; set; }

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
        /// 初期値設定
        /// </summary>
        public StockStatusModel()
        {
            // 現在日
            var now = DateTime.Today.ToString("yyyy/MM/dd");
            DateSearchStart = now;
        }
    }
}
