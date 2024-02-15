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
        /// 入庫実績リスト
        /// </summary>
        public IPagedList<D_StoreInModel> D_StoreInList { get; set; }

        /// <summary>
        /// 検索入庫日(開始)
        /// </summary>
        [Display(Name = "入庫日")]
        public string DateSearchStart { get; set; }

        /// <summary>
        /// 検索入庫日(終了)
        /// </summary>
        public string DateSearchEnd { get; set; }

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
        public IEnumerable<SelectListItem> SearchCompanyList
        {
            get
            {
                return MCompanyList;
            }
        }

        /// <summary>
        /// 選択された会社ID
        /// </summary>
        public int SelectedCompanyID { get; set; }

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
