using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace mar_sumaken_web.Models
{
    /// <summary>
    /// ファイル取込実績テーブルのModel
    /// </summary>
    public class D_FileImportModel : CommonModel
    {
        /// <summary>
        /// ファイル取込実績リスト
        /// </summary>
        public List<D_FileImportModel>? D_FileImportList { get; set; }

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
        /// ファイル取込実績ID
        /// </summary>
        public int FileImportID { get; set; }


        /// <summary>
        /// 倉庫ID
        /// </summary>
        public int DepoID { get; set; }

        /// <summary>
        /// 倉庫名
        /// </summary>
        public string DepoName { get; set; }

        /// <summary>
        /// メニュー名
        /// </summary>
        [Display(Name = "メニュー名")]
        public string MenuName { get; set; }

        /// <summary>
        /// 取込ファイル名
        /// </summary>
        [Display(Name = "取込ファイル名")]
        public string ImportFileName { get; set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 登録者
        /// </summary>
        [Display(Name = "登録者")]
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public string Message { set; get; }
    }
}
