using Microsoft.AspNetCore.Mvc.ApplicationModels;
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

        public int StoreOutID { get; set; }

        public string StoreOutDate { get; set; }

        /// <summary>
        /// 選択された倉庫ID
        /// </summary>
        public int SelectedDepoID { get; set; }

        /// <summary>
        /// 選択された会社ID
        /// </summary>
        public int SelectedCompanyID { get; set; }

        public List<string> LstErrorMsg { set; get; }//エラーメッセージリスト
        public string Message { set; get; }

        public IPagedList<D_StoreInModel> LstD_StoreOut { set; get; }//一覧画面のlistを取得と設定

        // <summary>
        /// 初期値設定
        /// </summary>
        public D_StoreInModel()
        {
            var now = DateTime.Today.ToString("yyyy/MM/dd"); //　現在日
            var oneMonthAgo = DateTime.Today.AddMonths(-1).ToString("yyyy/MM/dd");  //　一月前
            DateSearchStart = oneMonthAgo;
            DateSearchEnd = now;
        }
    }
}
