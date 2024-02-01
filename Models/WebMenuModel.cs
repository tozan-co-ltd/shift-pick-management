using Dapper;
using mar_sumaken_web.Commons;
using System.Data.SqlClient;

namespace mar_sumaken_web.Models
{
    public class WebMenuModel : CommonModel
    {
        /// <summary>
        /// カテゴリー一覧を取得
        /// </summary>
        /// <returns>カテゴリー一覧</returns>
        public List<M_WebMenuCategory> WebMenuCategoryList()
        {
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionStringForMaster();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var commandText = $@"SELECT
                                    CategoryID
                                    ,CategoryName
                                FROM M_WebMenuCategory
                                ORDER BY CategoryCode ASC;
                                ;";
                var selectCategoryList = connection.Query<M_WebMenuCategory>(commandText).ToList();
                return selectCategoryList;
            }
        }

        /// <summary>
        /// カテゴリーのメニュー一覧を取得
        /// </summary>
        /// <param name="category">M_WebMenuCategory</param>
        /// <returns>カテゴリーのメニュー一覧</returns>
        public CategoryMenuList GetWebMenuCategoryList(M_WebMenuCategory category)
        {
            var categoryMenuList = new CategoryMenuList();

            categoryMenuList.Category = category;
            categoryMenuList.MenuList = this.MenuList(category);

            return categoryMenuList;
        }

        /// <summary>
        /// メニュー一覧を取得
        /// </summary>
        /// <param name="category">M_WebMenuCategory</param>
        /// <returns>メニュー一覧</returns>
        public List<M_WebMenu> MenuList(M_WebMenuCategory category)
        {
            var menuList = new List<M_WebMenu>();

            if (Role != 0)
            {
                var userRole = Role;
                string userRoleName = "Role" + userRole;

                //メニュ一覧を取得
                var selectMenuList = GetWebMenuList(userRoleName, CompanyID, category);

                menuList = selectMenuList;
            }

            return menuList;
        }

        /// <summary>
        /// メニュ一覧を取得
        /// <param name="userRoleName"></param>
        /// <param name="CompanyID"></param>
        /// <param name="category">M_WebMenuCategory</param>
        /// </summary>
        /// <returns>MUsersViewModel</returns>
        public static List<M_WebMenu> GetWebMenuList(string userRoleName, int CompanyID, M_WebMenuCategory category)
        {
            try
            {
                List<M_WebMenu> menuList = new List<M_WebMenu>();
               var categoryID = 0;
                if (category != null)
                {
                    categoryID = category.CategoryID;
                    var sql = MenuConnectController.CreateSQLToSelectMenu(userRoleName, categoryID);
                    // DB接続
                    menuList = MenuConnectController.ConnectMenu(sql, CompanyID, categoryID);
                }
                return menuList;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    public class CategoryMenuList
    {
        /// <summary>
        /// カテゴリー
        /// </summary>
        public M_WebMenuCategory Category { get; set; }

        /// <summary>
        /// メニュ一覧
        /// </summary>
        public List<M_WebMenu> MenuList { get; set; }
    }

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
