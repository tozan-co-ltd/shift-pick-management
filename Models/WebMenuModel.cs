using Dapper;
using ai_truck_load_measurement.Commons;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace ai_truck_load_measurement.Models
{
    /// <summary>
    /// WEBメニューのModel
    /// </summary>
    public class WebMenuModel : CommonModel
    {
        private readonly List<M_WebMenu> referenceList = new List<M_WebMenu>()
        {
            new M_WebMenu{CategoryID = 1, MenuID = 1, MenuName = "ステーション状況", Controller = "Top", Action="Index"},
            new M_WebMenu{CategoryID = 1, MenuID = 2, MenuName = "便予実進捗", Controller = "LoadProgress", Action = "Index"},
            new M_WebMenu{CategoryID = 2, MenuID = 1, MenuName = "荷量分布", Controller = "LoadDistribution", Action = "Index"},
            new M_WebMenu{CategoryID = 2, MenuID = 2, MenuName = "荷量推移", Controller = "LoadTransition", Action = "Index"},
            new M_WebMenu{CategoryID = 2, MenuID = 3, MenuName = "荷量と運行実績", Controller = "LoadOperationRecord", Action = "Index"},
            new M_WebMenu{CategoryID = 2, MenuID = 4, MenuName = "実績出力", Controller = "LoadOutput", Action = "Index"},
            new M_WebMenu{CategoryID = 2, MenuID = 5, MenuName = "アラート履歴", Controller = "AlertRecord", Action = "Index"},
            new M_WebMenu{CategoryID = 3, MenuID = 1, MenuName = "お知らせ", Controller = "News", Action = "Index"},
            new M_WebMenu{CategoryID = 4, MenuID = 1, MenuName = "車両マスター", Controller = "M_Truck", Action = "Index"},
            new M_WebMenu{CategoryID = 4, MenuID = 2, MenuName = "便マスター", Controller = "M_Trip", Action = "Index"},
            new M_WebMenu{CategoryID = 4, MenuID = 3, MenuName = "便枝番マスター", Controller = "M_TripBranchNumber", Action = "Index"},
            new M_WebMenu{CategoryID = 4, MenuID = 4, MenuName = "ユーザーマスター", Controller = "M_User", Action = "Index"},
            new M_WebMenu{CategoryID = 4, MenuID = 5, MenuName = "通知マスター", Controller = "M_Notification", Action = "Index"},
            new M_WebMenu{CategoryID = 5, MenuID = 1, MenuName = "管理用ポータル画面", Controller = "ManagementPortal", Action = "Index"},
            new M_WebMenu{CategoryID = 5, MenuID = 2, MenuName = "1.出発実績無し件数", Controller = "NonDepartedAtRecord", Action = "Index"},
            new M_WebMenu{CategoryID = 5, MenuID = 3, MenuName = "2.紐づけ切れ - ID有", Controller = "NonTripNameRecord", Action = "Index"},
            new M_WebMenu{CategoryID = 5, MenuID = 4, MenuName = "3.紐づけ切れ - ID無", Controller = "NonIdentifyNumberRecord", Action = "Index"},
            new M_WebMenu{CategoryID = 5, MenuID = 5, MenuName = "4.紐づけ切れ回数", Controller = "CountNonTripNameRecord", Action = "Index"},
            new M_WebMenu{CategoryID = 5, MenuID = 6, MenuName = "5.早着・遅着実績", Controller = "ArrivalTimeDefference", Action = "Index"},
            new M_WebMenu{CategoryID = 5, MenuID = 7, MenuName = "6.荷量アラート実績", Controller = "LoadAverage", Action = "Index"},
            new M_WebMenu{CategoryID = 5, MenuID = 8, MenuName = "7.アラート回数詳細表示", Controller = "CountAlertRecord", Action = "Index"},
        };

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
                    new M_WebMenuCategory{CategoryID = 1, CategoryName = "状況"},
                    new M_WebMenuCategory{CategoryID = 2, CategoryName = "実績" },
                    new M_WebMenuCategory{CategoryID = 3, CategoryName = "お知らせ"},
                    new M_WebMenuCategory{CategoryID = 4, CategoryName = "マスター" },
                    new M_WebMenuCategory{CategoryID = 5, CategoryName = "管理用" },
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

                // WEBメニューリストを取得
                menuList = GetWebMenuList(category);
                return menuList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// WEBメニューリストを取得
        /// <param name="mWebMenuCategory"></param>
        /// </summary>
        /// <returns>WEBメニューリスト</returns>
        public List<M_WebMenu> GetWebMenuList(M_WebMenuCategory mWebMenuCategory)
        {
            try
            {
                List<M_WebMenu> webMenuList = new();

                var categoryID = 0;

                if (mWebMenuCategory != null)
                {
                    categoryID = mWebMenuCategory.CategoryID;
                }

                              
                if(categoryID == 0)
                {
                    webMenuList = referenceList;
                }
                else
                {
                    webMenuList = referenceList.FindAll(x => x.CategoryID == categoryID);
                }

                return webMenuList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// コントローラー名に対応するカテゴリーID取得
        /// </summary>
        /// <param name="currentControler"></param>
        /// <returns></returns>
        public int? GetCategoryIDFromControlerName(string controllerName)
        {
            var currentWebMenu = referenceList.Where(x => x.Controller.Equals(controllerName)).FirstOrDefault();
            if(currentWebMenu == null)
            {
                return null;
            }
            int currentCategoryID = currentWebMenu.CategoryID;
            return currentCategoryID;
        }

        /// <summary>
        /// コントローラー名に対応するカテゴリー名取得
        /// </summary>
        /// <param name="controllerName"></param>
        /// <returns></returns>
        public string? GetCategoryNameFromControllerName(string controllerName)
        {
            var categoryID = GetCategoryIDFromControlerName(controllerName);
            if(categoryID == null)
            {
                return null;
            }
            var currentWebMenu = WebMenuCategoryList().Find(x => x.CategoryID == categoryID);
            if(currentWebMenu == null)
            {
                return null;
            }
            return currentWebMenu.CategoryName;
        }

        public string? GetMenuNameFromControllerName(string controllerName)
        {
            var currentWebMenu = referenceList.Where(x => x.Controller.Equals(controllerName)).FirstOrDefault();
            if (currentWebMenu == null)
            {
                return null;
            }
            string currentMenuName = currentWebMenu.MenuName;
            return currentMenuName;
        }

        public void GetBaseView(ClaimsPrincipal claimsPrincipal, ViewContext viewContext)
        {
            base.GetBaseView(claimsPrincipal, viewContext);
            AuthorizedKubun = claimsPrincipal.Claims.ToList().Where(x => x.Type == "AuthorizedKubun").First().Value;
        }

        public string? AuthorizedKubun { get; set; }
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
