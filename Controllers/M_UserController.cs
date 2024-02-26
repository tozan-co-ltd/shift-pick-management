using AutoMapper;
using mar_sumaken_web.Commons;
using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Data.SqlClient;
using X.PagedList;

namespace mar_sumaken_web.Controllers
{
    /// <summary>
    /// ユーザーマスター画面
    /// </summary>
    public class M_UserController : BaseController
    {
        private readonly ILogger<M_UserController> _logger;

        public M_UserController(ILogger<M_UserController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// ユーザーマスター画面表示
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IActionResult Index(M_UserModel model)
        {
            if (model == null)
                model = new M_UserModel();

            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 管理権限区分が1(管理者)でない場合はエラーとする
                if (user == null || user.AuthorizedKubun != 1)
                {
                    throw new Exception();
                }

                // ユーザーマスター情報取得SQL作成
                var sql = M_UserConnectController.CreateSQLToSelectMUsers();

                // DB接続
                var userList = M_UserConnectController.ConnectMUsers(sql, user.DatabaseName);
                
                if (userList.Count > 0)
                {
                    // ユーザーマスターの詳細を取得
                    IEnumerable<M_UserModel> mUserList = M_UserConnectController.GetMUserDetailList(userList, user.DatabaseName);
                    model.M_UserList = mUserList.ToPagedList();
                }

                return View(model);
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = "E9999: " + ErrorMessagesResources.E9999;
                return View();
            }
        }

        /// <summary>
        /// ユーザーマスター登録画面表示
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Register()
        {
            M_UserModel model = new();

            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 倉庫マスター情報取得
                // SQL作成
                var depoListSql = M_DepoConnectController.CreateSQLToSelectMDepos();
                // DB接続
                List<M_DepoModel> depoList = M_DepoConnectController.ConnectMDepos(depoListSql, user.DatabaseName);
                foreach (var depo in depoList)
                {
                    SelectListItem depoItem = new()
                    {
                        Text = depo.DepoName,
                        Value = Convert.ToString(depo.DepoID),
                        Selected = false
                    };

                    model.DepoSelectList.Add(depoItem);
                }

                // ハンディメニューマスター情報取得
                // SQL作成
                var handyMenuListSql = M_HandyMenuConnectController.CreateSQLToGetMHandyMenuList();
                // DB接続
                List<M_HandyMenuModel> handyMenuList = M_HandyMenuConnectController.ConnectMHandyMenus(handyMenuListSql, user.DatabaseName);
                foreach (var handyMenu in handyMenuList)
                {
                    SelectListItem menuItem = new()
                    {
                        Text = handyMenu.HandyMenuName,
                        Value = Convert.ToString(handyMenu.HandyMenuID),
                        Selected = false
                    };

                    model.HandyMenuSelectList.Add(menuItem);
                }

                return View(model);
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = "E9999: " + ErrorMessagesResources.E9999;
                return View();
            }
        }

        /// <summary>
        /// ユーザーマスター登録
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Register(M_UserModel model)
        {
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // メイン倉庫IDをチェック
                bool isDepoSelected = false;
                foreach (SelectListItem item in model.DepoSelectList)
                {
                    if (item.Selected)
                    {
                        isDepoSelected = true;
                    }
                }

                // 入力規則チェック
                if (!ModelState.IsValid)
                {
                    var errorMessages = ModelState.SelectMany(x => x.Value.Errors.Select(z => z.ErrorMessage));
                    return NotFound(new { errorMessage = errorMessages });
                }

                // ログインID重複チェック
                var sql = M_UserConnectController.CreateSQLToSelectDuplicateMUser(model.LoginID);
                bool isExisted = ConnectToSQLServer.IsExistedSameRecord(sql, user.DatabaseName);
                if (isExisted)
                {
                    return NotFound(new { errorMessage = "E1009: " + string.Format(ErrorMessagesResources.E1009, Utils.GetDisplayName<M_UserModel>("LoginID")) });
                }

                // saltの作成とパスワードのハッシュ化
                var salt = Hashing.GetRandomSalt();
                var hashedPassword = Hashing.ConvertPlaintextPasswordToHashedPassword(model.Password, salt);
                var stringSalt = Hashing.ConvertByteToString(salt);
                model.Password = hashedPassword;
                model.Salt= stringSalt;

                // ユーザーマスター登録
                bool isInsertMuser = M_UserConnectController.InsertMUser(model, user);

                return Ok();
            }
            catch (SqlException)
            {
                return NotFound(new { errorMessage = "E3004: " + ErrorMessagesResources.E3004 });
            }
            catch (Exception)
            {
                return NotFound(new { errorMessage = "E9999: " + ErrorMessagesResources.E9999 });
            }
        }

        /// <summary>
        /// ユーザーマスター修正画面表示
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Edit(int userId)
        {
            M_UserEditModel editModel = new();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // IDが一致するユーザー情報取得
                // SQL作成
                var userListSql = M_UserConnectController.CreateSQLToSelectMUserByUserId(userId);
                // DB接続
                List<M_UserModel> userList = M_UserConnectController.ConnectMUsers(userListSql, user.DatabaseName);
                if (userList.Count != 1)
                {
                    ViewData["ErrorMessage"] = "E3004: " + ErrorMessagesResources.E3004;
                    return View(editModel);
                }
                M_UserModel editUser = userList[0];

                // 倉庫マスター情報取得
                // SQL作成
                var depoListSql = M_DepoConnectController.CreateSQLToSelectMDepos();
                // DB接続
                List<M_DepoModel> depoList = M_DepoConnectController.ConnectMDepos(depoListSql, user.DatabaseName);
                foreach (var depo in depoList)
                {
                    SelectListItem depoItem = new()
                    {
                        Text = depo.DepoName,
                        Value = Convert.ToString(depo.DepoID),
                        Selected = false
                    };

                    editUser.DepoSelectList.Add(depoItem);
                }

                // ハンディメニューマスター情報取得
                // SQL作成
                var handyMenuListSql = M_HandyMenuConnectController.CreateSQLToGetMHandyMenuList();
                // DB接続
                List<M_HandyMenuModel> handyMenuList = M_HandyMenuConnectController.ConnectMHandyMenus(handyMenuListSql, user.DatabaseName);
                foreach (var handyMenu in handyMenuList)
                {
                    SelectListItem menuItem = new()
                    {
                        Text = handyMenu.HandyMenuName,
                        Value = Convert.ToString(handyMenu.HandyMenuID),
                        Selected = false
                    };

                    editUser.HandyMenuSelectList.Add(menuItem);
                }

                // IDが一致するユーザー倉庫中間リスト取得
                var userDepoListSql = M_UserConnectController.CreateSQLToGetRUserDepoList(editUser.UserID);
                var userDepoList = M_DepoConnectController.ConnectMDepos(userDepoListSql, user.DatabaseName);
                if (userDepoList.Count > 0)
                {
                    foreach (var userDepo in userDepoList)
                    {
                        var checkItem = editUser.DepoSelectList.FirstOrDefault(item => item.Value == Convert.ToString(userDepo.DepoID));
                        if (checkItem != null)
                        {
                            checkItem.Selected = true;
                        }
                    }
                }

                // IDが一致するユーザー-ハンディメニュー中間リスト取得
                var userHandyMenuSql = M_UserConnectController.CreateSQLToGetRUserHandyMenuList(userId);
                var userMenuList = M_HandyMenuConnectController.ConnectMHandyMenus(userHandyMenuSql, user.DatabaseName);
                if (userMenuList.Count > 0)
                {
                    foreach (var menu in userMenuList)
                    {
                        var checkItem = editUser.HandyMenuSelectList.FirstOrDefault(item => item.Value == Convert.ToString(menu.HandyMenuID));
                        if (checkItem != null)
                        {
                            checkItem.Selected = true;
                        }
                    }
                }

                var config = new MapperConfiguration(
                    cfg => cfg.CreateMap<M_UserModel, M_UserEditModel>()
                            .ForMember(dest => dest.M_UserList, opt => opt.Ignore())
                );
                IMapper mapper = config.CreateMapper();
                editModel = mapper.Map<M_UserEditModel>(editUser);

                return View(editModel);
            }
            catch (Exception)
            {
                ViewData["ErrorMessage"] = "E9999: " + ErrorMessagesResources.E9999;
                return View(editModel);
            }
        }

        /// <summary>
        /// ユーザーマスター更新
        /// </summary>
        /// <param name="model">更新情報</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Edit(M_UserModel model)
        {
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // メイン倉庫IDチェック
                bool isDepoSelected = false;
                foreach (SelectListItem item in model.DepoSelectList)
                {
                    if (item.Selected)
                    {
                        isDepoSelected = true;
                    }
                }

                // 更新情報をチェック
                bool isNotChangePassword = string.IsNullOrWhiteSpace(model.Password);
                if (isNotChangePassword)
                {
                    ModelState.Remove("Password");
                }
                if (!ModelState.IsValid || !isDepoSelected)
                {
                    var errorMessages = ModelState.SelectMany(x => x.Value.Errors.Select(z => z.ErrorMessage));
                    return NotFound(new { errorMessage = errorMessages });
                }

                if (!isNotChangePassword)
                {
                    // saltの作成とパスワードのハッシュ化
                    var salt = Hashing.GetRandomSalt();
                    var hashedPassword = Hashing.ConvertPlaintextPasswordToHashedPassword(model.Password, salt);
                    var stringSalt = Hashing.ConvertByteToString(salt);
                    model.Password = hashedPassword;
                    model.Salt = stringSalt;
                }

                // ユーザーマスター更新
                bool isUpdateMuser = await M_UserConnectController.UpdateMUser(model, user);

                return Ok();
            }

            catch (SqlException)
            {
                return NotFound(new { errorMessage = "E3004: " + ErrorMessagesResources.E3004 });
            }
            catch (Exception)
            {
                return NotFound(new { errorMessage = "E9999: " + ErrorMessagesResources.E9999 });
            }
        }

        /// <summary>
        /// ユーザーマスター削除
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IActionResult Delete(int userId)
        {
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // ユーザーマスター削除
                int deleteAffectedRows = M_UserConnectController.DeleteMUser(userId, user);

                return Ok();
            }
            catch (SqlException)
            {
                return NotFound(new { errorMessage = "E3004: " + ErrorMessagesResources.E3004 });
            }
            catch (Exception)
            {
                return NotFound(new { errorMessage = "E9999: " + ErrorMessagesResources.E9999 });
            }
        }

        /// <summary>
        /// ファイル出力
        /// </summary>
        public JsonResult ExportFile()
        {
            try
            {
                // log取得
                _logger.LogInformation($"Excel出力開始");

                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 管理権限区分が1(管理者)でない場合はエラーとする
                if (user == null || user.AuthorizedKubun != 1)
                {
                    throw new Exception();
                }

                // テーブルデータ取得
                DataTable mUserDataTable = CreateDataTable();

                // ユーザーマスター情報取得SQL作成
                var sql = M_UserConnectController.CreateSQLToSelectMUsers();
                // DB接続
                List<M_UserModel> userList = M_UserConnectController.ConnectMUsers(sql, user.DatabaseName);
                if (userList.Count > 0)
                {
                    foreach (M_UserModel userItem in userList)
                    {
                        DataRow newRow = mUserDataTable.NewRow();
                        newRow[Utils.GetDisplayName<M_UserModel>("UserID")] = userItem.UserID.ToString();
                        newRow[Utils.GetDisplayName<M_UserModel>("LoginID")] = userItem.LoginID.ToString();
                        newRow[Utils.GetDisplayName<M_UserModel>("UserName")] = userItem.UserName;
                        newRow[Utils.GetDisplayName<M_UserModel>("DepoName")] = userItem.DepoName;
                        newRow[Utils.GetDisplayName<M_UserModel>("AuthorizedKubun")] = userItem.AuthorizedKubun;
                        newRow[Utils.GetDisplayName<M_UserModel>("UpdatedAt")] = userItem.UpdatedAt.ToString();
                        newRow[Utils.GetDisplayName<M_UserModel>("UpdatedBy")] = userItem.UpdatedBy;

                        mUserDataTable.Rows.Add(newRow);
                    }
                }

                // ファイル名
                var tmpFilename = "ユーザーマスター.csv";
                // CSVファイルへのパスを作成する
                string filePath = Path.Combine(Path.GetTempPath(), tmpFilename);
                // DataTableをCSVに変換する
                CreateFile.ConvertDataTableToCsv(mUserDataTable, filePath);
                // ファイルの作成
                var file = System.IO.File.ReadAllBytes(filePath);

                return Json(new { data = File(file, System.Net.Mime.MediaTypeNames.Application.Octet, tmpFilename) });
            }
            catch (Exception)
            {
                return Json(new { res = "NG", error = "予期せぬエラーが発⽣しました。" });
            }
        }

        /// <summary>
        /// データテーブル作成
        /// </summary>
        /// <returns></returns>
        private static DataTable CreateDataTable()
        {
            var table = new DataTable();

            table.Columns.Add(Utils.GetDisplayName<M_UserModel>("UserID"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_UserModel>("LoginID"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_UserModel>("UserName"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_UserModel>("DepoName"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_UserModel>("AuthorizedKubun"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_UserModel>("UpdatedAt"), typeof(string));
            table.Columns.Add(Utils.GetDisplayName<M_UserModel>("UpdatedBy"), typeof(string));

            return table;
        }

    }
}
