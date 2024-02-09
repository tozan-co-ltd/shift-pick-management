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
        /// 出庫日
        /// </summary>
        [Display(Name = "出庫日")]
        public string DateSearchStart { set; get; }

        /// <summary>
        /// 出庫日(終了)
        /// </summary>
        public string DateSearchEnd { set; get; }

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
        /// 選択された倉庫ID
        /// </summary>
        public int SelectedDepoID { get; set; }

        /// <summary>
        /// 選択された会社ID
        /// </summary>
        public int SelectedCompanyID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int StoreOutID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string StoreOutDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> LstErrorMsg { set; get; }

        /// <summary>
        /// 
        /// </summary>
        public string Message { set; get; }

        /// <summary>
        /// 
        /// </summary>
        public IPagedList<D_StoreOutModel> LstD_StoreOut { set; get; }

        /// <summary>
        /// 初期値設定
        /// </summary>
        public D_StoreOutModel()
        {
            // 現在日
            var now = DateTime.Today.ToString("yyyy/MM/dd");
            // 1ヶ月前
            var oneMonthAgo = DateTime.Today.AddMonths(-1).ToString("yyyy/MM/dd"); 

            DateSearchStart = oneMonthAgo;
            DateSearchEnd = now;
        }
        
    }
}
