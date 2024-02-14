using Microsoft.AspNetCore.Mvc.ApplicationModels;
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
        /// 入荷日
        /// </summary>
        [Display(Name = "入荷日")]
        public string DateSearchStart { set; get; }

        /// <summary>
        /// 入庫日(終了)
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
        public List<string> LstErrorMsg { set; get; }

        /// <summary>
        /// 
        /// </summary>
        public string Message { set; get; }

        /// <summary>
        /// 
        /// </summary>
        public IPagedList<D_ReceiveModel> LstD_ReceiveModel { set; get; }

        /// <summary>
        /// 初期値設定
        /// </summary>
        public D_ReceiveModel()
        {
            // 現在日
            var now = DateTime.Today.ToString("yyyy/MM/dd");
            DateSearchStart = now;
            DateSearchEnd = now;
        }
    }
}
