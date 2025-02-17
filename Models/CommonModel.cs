using System.Data.SqlClient;
using ai_truck_load_measurement.Commons;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Dapper;

namespace ai_truck_load_measurement.Models
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
        /// 車両リスト
        /// </summary>
        public IEnumerable<SelectListItem>? MTruckList { get; set; }

        /// <summary>
        /// デポリスト
        /// </summary>
        public IEnumerable<SelectListItem>? MDepoList { get; set; }

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
            MTruckList = GetMTruckList();
            MDepoList = GetMDepoList();
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

        /// <summary>
        /// 車両リスト取得
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SelectListItem> GetMTruckList()
        {
            var selectListItem = new List<SelectListItem>();

            try
            {
                // SQLServer接続文字列取得
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string commandText = $@"
                        SELECT
                            truck_id as Value,
                            truck_number AS Text
                        FROM m_trucks
                        WHERE (1=1)
                            AND is_deleted = 0
                        ";

                    selectListItem = connection.Query<SelectListItem>(commandText).ToList();
                }
                return selectListItem;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// デポリスト取得
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SelectListItem> GetMDepoList()
        {
            var selectListItem = new List<SelectListItem>();
            try
            {
                // SQLServer接続文字列取得
                var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string commandText = $@"
                        SELECT
                            depo_id as Value,
                            name AS Text
                        FROM m_depos
                        ";

                    selectListItem = connection.Query<SelectListItem>(commandText).ToList();
                }
                var firstItem = new SelectListItem() { Text = "選択してください", Disabled = false, Selected = true };
                selectListItem.Insert(0, firstItem);
                return selectListItem;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
