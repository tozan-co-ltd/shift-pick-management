using Dapper;
using ai_truck_load_measurement.Commons;
using System.Data.SqlClient;

namespace ai_truck_load_measurement.Models
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
                //// SQL作成
                //var sql = WebMenuConnectController.CreateSQLToSelectMWebMenuCategory();
                //// DB接続
                //var selectCategoryList = WebMenuConnectController.ConnectMWebMenuCategory(sql);

                var selectCategoryList = new List<M_WebMenuCategory>()
                {
                    new M_WebMenuCategory{CategoryID = 11, CategoryName = "実績"},
                    new M_WebMenuCategory{CategoryID = 12, CategoryName = "マスター" },
                    new M_WebMenuCategory{CategoryID = 1, CategoryName ="入出荷"},
                    new M_WebMenuCategory{CategoryID = 5, CategoryName ="入荷"},
                    new M_WebMenuCategory{CategoryID = 10, CategoryName ="その他"},
                    new M_WebMenuCategory{CategoryID = 6, CategoryName ="出荷"},
                    new M_WebMenuCategory{CategoryID = 2, CategoryName ="納入支持"},
                    new M_WebMenuCategory{CategoryID = 3, CategoryName ="在庫"},
                    new M_WebMenuCategory{CategoryID = 9, CategoryName ="棚卸"},
                    new M_WebMenuCategory{CategoryID = 4, CategoryName ="管理"},
                    new M_WebMenuCategory{CategoryID = 7, CategoryName ="マスター情報"},
                    new M_WebMenuCategory{CategoryID = 8, CategoryName ="AGF管理"}
                };

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
                    menuList = GetWebMenuList(userRoleName, category);
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
        /// <param name="mWebMenuCategory"></param>
        /// </summary>
        /// <returns>WEBメニューリスト</returns>
        public List<M_WebMenu> GetWebMenuList(string userRoleName, M_WebMenuCategory mWebMenuCategory)
        {
            try
            {
                List<M_WebMenu> webMenuList = new();

                var categoryID = 0;

                if (mWebMenuCategory != null)
                {
                    categoryID = mWebMenuCategory.CategoryID;
                }

                //// SQL作成
                //var sql = WebMenuConnectController.CreateSQLToSelectMWebMenu(CompanyID, userRoleName, categoryID);
                //// DB接続
                //webMenuList = WebMenuConnectController.ConnectMWebMenu(sql, CompanyID, categoryID);

                List<M_WebMenu> referenceList = new List<M_WebMenu>()
                {
                    new M_WebMenu{CategoryID = 5, MenuID = 1, MenuName = "入荷予定取込", Controller = "ImportReceiveSchedule", Action = "Index"},
                    new M_WebMenu{CategoryID = 5, MenuID = 2, MenuName = "入荷予定照会", Controller = "D_ReceiveSchedule", Action = "Index"},
                    new M_WebMenu{CategoryID = 5, MenuID = 3, MenuName = "入荷実績照会", Controller = "D_Receive", Action = "Index"},
                    new M_WebMenu{CategoryID = 6, MenuID = 1, MenuName = "出荷指示取込", Controller = "ImportShipmentSchedule", Action = "Index"},
                    new M_WebMenu{CategoryID = 6, MenuID = 2, MenuName = "出荷指示照会", Controller = "D_ShipmentSchedule", Action = "Index"},
                    new M_WebMenu{CategoryID = 3, MenuID = 4, MenuName = "在庫照会", Controller = "StockStatus", Action = "Index"},
                    new M_WebMenu{CategoryID = 3, MenuID = 8, MenuName = "入庫実績照会・修正", Controller = "D_StoreIn", Action = "Index"},
                    new M_WebMenu{CategoryID = 3, MenuID = 3, MenuName = "出庫実績照会・修正", Controller = "D_StoreOut", Action = "Index"},
                    new M_WebMenu{CategoryID = 7, MenuID = 1, MenuName = "倉庫マスター", Controller = "M_Depo", Action = "Index"},
                    new M_WebMenu{CategoryID = 7, MenuID = 2, MenuName = "ユーザーマスター", Controller = "M_User", Action = "Index"},
                    new M_WebMenu{CategoryID = 7, MenuID = 3, MenuName = "会社マスター", Controller = "M_Company", Action = "Index"},
                    new M_WebMenu{CategoryID = 7, MenuID = 4, MenuName = "品番マスター", Controller = "M_Product", Action = "Index"},
                    new M_WebMenu{CategoryID = 7, MenuID = 5, MenuName = "仕入先かんばんマスター", Controller = "M_SupplierKanban", Action = "Index"},
                    new M_WebMenu{CategoryID = 11, MenuID = 1, MenuName = "荷量分布", Controller = "ImportReceiveSchedule", Action = "Index"},
                    new M_WebMenu{CategoryID = 11, MenuID = 2, MenuName = "荷量推移", Controller = "ImportReceiveSchedule", Action = "Index"},
                    new M_WebMenu{CategoryID = 11, MenuID = 3, MenuName = "荷量と運行実績", Controller = "ImportReceiveSchedule", Action = "Index"},
                    new M_WebMenu{CategoryID = 11, MenuID = 4, MenuName = "実績出力", Controller = "ImportReceiveSchedule", Action = "Index"},
                    new M_WebMenu{CategoryID = 12, MenuID = 1, MenuName = "車両マスター", Controller = "ImportReceiveSchedule", Action = "Index"},
                    new M_WebMenu{CategoryID = 12, MenuID = 2, MenuName = "便マスター", Controller = "ImportReceiveSchedule", Action = "Index"},
                };
                webMenuList = referenceList.FindAll(x => x.CategoryID == categoryID);

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
