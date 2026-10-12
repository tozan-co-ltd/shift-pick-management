using System.Data.SqlClient;
using shift_pick_management.Commons;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Dapper;

namespace shift_pick_management.Models
{
    /// <summary>
    /// 共通Model
    /// </summary>
    /// <remarks>画面表示に必要な情報(ログイン中ユーザーのClaim,メニュー名,検索用倉庫名・会社名)を取得</remarks>
    public class CommonModel
    {
        /// <summary>
        /// Controller名
        /// </summary>
        public string? ControllerName { get; set; }

        /// <summary>
        /// ビュータイトル
        /// </summary>
        public string? ViewTitle { get; set; }

        /// <summary>
        /// カテゴリータイトル
        /// </summary>
        public string? CategoryTitle { get; set; }

        /// <summary>
        /// ベースビュー作成
        /// </summary>
        /// <remarks>コントロール名、ビュータイトルを設定</remarks>
        /// <param name="claimsPrincipal">ClaimsPrincipal</param>
        /// <param name="viewContext">ViewContext</param>
        public void GetBaseView(ClaimsPrincipal claimsPrincipal, ViewContext viewContext)
        {
            ControllerName = viewContext.RouteData.Values["controller"].ToString();
            CategoryTitle = GetCategoryTitle();
            ViewTitle = GetViewTitle();
        }

        /// <summary>
        /// ページタイトル(カテゴリー名)取得
        /// </summary>
        /// <returns>カテゴリー名</returns>
        public string GetCategoryTitle()
        {
            try
            {
                WebMenuModel model = new WebMenuModel();
                var categoryTitle = model.GetCategoryNameFromControllerName(ControllerName);
                return categoryTitle;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// ページタイトル(WEBメニュー名)取得
        /// </summary>
        /// <returns>WEBメニュー名</returns>
        public string GetViewTitle()
        {
            try
            {
                WebMenuModel model = new WebMenuModel();
                var categoryTitle = model.GetMenuNameFromControllerName(ControllerName);
                return categoryTitle;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    // バリデーションチェック時のモデル
    public class ValidCheckModel
    {
        public bool IsValid { get; set; }

        public string ErrorMessage { get; set; }
    }
}
