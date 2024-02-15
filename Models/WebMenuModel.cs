using Dapper;
using mar_sumaken_web.Commons;
using System.Data.SqlClient;

namespace mar_sumaken_web.Models
{
    /// <summary>
    /// WEBメニューのModel
    /// </summary>
    public class WebMenuModel : CommonModel
    {
        /// <summary>
        /// WEBカテゴリーリスト取得
        /// </summary>
        /// <returns>WEBカテゴリーリスト</returns>
        public List<M_WebMenuCategory> WebMenuCategoryList()
        {
            try
            {
                // SQL作成
                var sql = WebMenuConnectController.CreateSQLToSelectMWebMenuCategory();
                // DB接続
                var selectCategoryList = WebMenuConnectController.ConnectMWebMenuCategory(sql);

                return selectCategoryList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// カテゴリーのメニュー一覧を取得
        /// </summary>
        /// <param name="category">M_WebMenuCategory</param>
        /// <returns>カテゴリーのメニューリスト</returns>
        public WebCategoryMenuList GetWebMenuCategoryList(M_WebMenuCategory category)
            {
                try
                {
                    var categoryMenuList = new WebCategoryMenuList
                    {
                        M_WebMenuCategoryModel = category,
                        WebMenuList = this.MenuList(category)
                    };

                    return categoryMenuList;
                }
                catch (Exception)
                {
                    throw;
                }
            }

        /// <summary>
        /// WEBメニューカテゴリーリストを取得
        /// </summary>
        /// <param name="category">M_WebMenuCategory</param>
        /// <returns>WEBメニューカテゴリーリスト</returns>
        public List<M_WebMenu> MenuList(M_WebMenuCategory category)
        {
            try
            {
                var menuList = new List<M_WebMenu>();

                if (Role != 0)
                {
                    var userRole = Role;
                    string userRoleName = "Role" + userRole;

                    // WEBメニューリストを取得
                    var selectMenuList = GetWebMenuList(userRoleName, CompanyID, category);

                    menuList = selectMenuList;
                }
                return menuList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// WEBメニューリストを取得
        /// <param name="userRoleName"></param>
        /// <param name="companyID"></param>
        /// <param name="mCategory"></param>
        /// </summary>
        /// <returns>WEBメニューリスト</returns>
        public static List<M_WebMenu> GetWebMenuList(string userRoleName, int companyID, M_WebMenuCategory mCategory)
        {
            try
            {
                List<M_WebMenu> webMenuList = new();

               var categoryID = 0;

                if (mCategory != null)
                {
                    categoryID = mCategory.CategoryID;

                    // SQL作成
                    var sql = WebMenuConnectController.CreateSQLToSelectMWebMenu(companyID, userRoleName, categoryID);
                    // DB接続
                    webMenuList = WebMenuConnectController.ConnectMWebMenu(sql);
                }
                return webMenuList;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    /// <summary>
    /// WEBメニューカテゴリーModelとWEBメニューリストのModel
    /// </summary>
    public class WebCategoryMenuList
    {
        /// <summary>
        /// WEBメニューカテゴリーModel
        /// </summary>
        public M_WebMenuCategory M_WebMenuCategoryModel { get; set; }

        /// <summary>
        /// WEBメニューリスト
        /// </summary>
        public List<M_WebMenu> WebMenuList { get; set; }
    }

    /// <summary>
    /// WEBメニューカテゴリーマスター
    /// </summary>
    public class M_WebMenuCategory
    {
        /// <summary>
        /// カテゴリーID
        /// </summary>
        public int CategoryID { get; set; }

        /// <summary>
        /// カテゴリー名
        /// </summary>
        public string CategoryName { get; set; }
    }

    /// <summary>
    /// WEBメニューマスター
    /// </summary>
    public class M_WebMenu
    {
        /// <summary>
        /// カテゴリーID
        /// </summary>
        public int CategoryID { get; set; }

        /// <summary>
        /// メニュ一ID
        /// </summary>
        public int MenuID { get; set; }

        /// <summary>
        /// メニュ一名
        /// </summary>
        public string MenuName { get; set; }

        /// <summary>
        /// コントローラ
        /// </summary>
        public string Controller { get; set; }

        /// <summary>
        /// アクション
        /// </summary>
        public string Action { get; set; }
    }
}
