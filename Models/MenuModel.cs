using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using mar_sumaken_web.Controllers;
using mar_sumaken_web.Commons;
using System.Data.SqlClient;
using Dapper;
using static mar_sumaken_web.Models.M_UserModel;

namespace mar_sumaken_web.Models
{
    public class MenuModel:CommonModel
    {

        /// <summary>
        /// カテゴリー一覧を取得
        /// </summary>
        /// <returns> カテゴリー一覧</returns>
        public List<Category> CategoryList()
        {
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionStringForMaster();
            using (var connection = new SqlConnection(connectionString))
            {
                //open-------------------------------------------------------------
                connection.Open();

                //SQLの準備
                var commandText = "";
                commandText = $@"SELECT
                                                 CategoryID
                                                ,CategoryName
                                            FROM M_WebMenuCategory
                                            ;";
                var selectCategoryList = connection.Query<Category>(commandText).ToList();
                return selectCategoryList;
            }
        }

        /// <summary>
        /// カテゴリーのメニュー一覧を取得
        /// </summary>
        /// <param name="category">Category</param>
        /// <returns>カテゴリーのメニュー一覧</returns>
        public CategoryMenuList GetCategoryMenuList(Category category)
        {
            var categoryMenuList = new CategoryMenuList();

            categoryMenuList.Category = category;
            categoryMenuList.MenuList = this.MenuList(category);

            return categoryMenuList;
        }

        /// <summary>
        /// メニュー一覧を取得
        /// </summary>
        /// <param name="category">Category</param>
        /// <returns>メニュー一覧</returns>
        public List<Menu> MenuList(Category category)
        {
            var menuList = new List<Menu>();

            if (Role != 0)
            {
                var userRole = Role;
                string userRoleName = "Role" + userRole;

                //メニュ一覧を取得
                var selectMenuList = GetListMenu(userRoleName, CompanyID, category);

                menuList = selectMenuList;
            }

            return menuList;
        }

        /// <summary>
        /// メニュ一覧を取得
        /// <param name="userRoleName"></param>
        /// <param name="whereString"></param>
        /// </summary>
        /// <returns>MUsersViewModel</returns>
        public static List<Menu> GetListMenu(string userRoleName, int CompanyID, Category category)
        {
            try
            {
                List<Menu> menuList = new List<Menu>();
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
        public Category Category { get; set; }

        /// <summary>
        /// メニュ一覧
        /// </summary>
        public List<Menu> MenuList { get; set; }
    }

    public class Category
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

    public class Menu
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
