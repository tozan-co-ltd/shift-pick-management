using AutoMapper;
using mar_sumaken_web.Commons;
using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using X.PagedList;

namespace mar_sumaken_web.Controllers
{
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
                    // エラーコード：E2011
                    throw new Exception();
                }

                // ユーザーマスター情報取得SQL作成
                var sql = M_UserConnectController.CreateSQLToSelectMUsers();

                // DB接続
                var userList = M_UserConnectController.ConnectMUsers(sql, user.DatabaseName);
                
                if (userList.Count > 0)
                {
                    // ユーザーマスターの詳細を取得
                    IEnumerable<M_UserModel> query = (IEnumerable<M_UserModel>)M_UserConnectController.GetMUserDetailList(userList, user.DatabaseName);
                    model.M_UserList = query.ToPagedList();
                }

                return View(model);
            }
            catch (Exception)
            {
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
                var depoList = M_DepoConnectController.GetMDepoList(user.DatabaseName);
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
                var menuList = M_HandyMenuConnectController.GetMHandyMenuList(user.DatabaseName);
                foreach (var menu in menuList)
                {
                    SelectListItem menuItem = new()
                    {
                        Text = menu.HandyMenuName,
                        Value = Convert.ToString(menu.HandyMenuID),
                        Selected = false
                    };

                    model.HandyMenuSelectList.Add(menuItem);
                }

                return View(model);
            }
            catch (Exception)
            {
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
                if (user == null)
                {
                    // エラーコード：E2011
                    return NotFound(new { errorMessage = "データが見つかりませんでした。" });
                }

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
                if (!ModelState.IsValid || !isDepoSelected)
                {
                    return NotFound(new { errorMessage = "正しい入力を入れてください。" });
                }

                // 重複ユーザーチェック
                bool isDuplicate = M_UserConnectController.CheckIsDuplicateMUserByLoginId(model.LoginID, user.DatabaseName);
                if (isDuplicate)
                {
                    return NotFound(new { errorMessage = "ログインIDが重複しています。" });
                }

                // saltの作成とパスワードのハッシュ化
                var salt = Hashing.GetRandomSalt();
                var hashedPassword = Hashing.ConvertPlaintextPasswordToHashedPassword(model.Password, salt);
                var stringSalt = Hashing.ConvertByteToString(salt);
                model.Password = hashedPassword;
                model.Salt= stringSalt;

                // ユーザーマスター登録
                bool isInsertMuser = M_UserConnectController.InsertMUser(model, user);

                // 更新件数が0の場合はエラーとする
                if (!isInsertMuser)
                {
                    // エラーコード：E2011
                    return NotFound(new { errorMessage = "登録はできませんでした。" });
                }

                return Ok();
            }
            catch (Exception)
            {
                return NotFound(new { errorMessage = "予期せぬエラーが発⽣しました。" });
            }
        }

        /// <summary>
        /// ユーザーマスター修正画面表示
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Edit(int id)
        {
            M_UserModel editUser = new();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();
                if (user == null)
                {
                    // エラーコード：E2011
                    return NotFound(new { errorMessage = "データが見つかりませんでした。" });
                }

                // IDでユーザーを選択するSQLを作成
                var sql = M_UserConnectController.CreateSQLToSelectMUserByUserId(id);
                // DB接続
                List<M_UserModel> userList = M_UserConnectController.ConnectMUsers(sql, user.DatabaseName);
                if (userList.Count != 1)
                {
                    // エラーコード：E2011
                    return NotFound(new { errorMessage = "見つかった情報は間違っています。" });
                }
                editUser = userList[0];

                // 倉庫マスター情報取得
                var depoList = M_DepoConnectController.GetMDepoList(user.DatabaseName);
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
                var menuList = M_HandyMenuConnectController.GetMHandyMenuList(user.DatabaseName);
                foreach (var menu in menuList)
                {
                    SelectListItem menuItem = new()
                    {
                        Text = menu.HandyMenuName,
                        Value = Convert.ToString(menu.HandyMenuID),
                        Selected = false
                    };

                    editUser.HandyMenuSelectList.Add(menuItem);
                }

                // IDでユーザー倉庫中間リスト取得
                var userDepoList = M_UserConnectController.GetUserDepoByUserId(editUser.UserID, user.DatabaseName);
                if (userDepoList.Count > 0)
                {
                    foreach (var depo in userDepoList)
                    {
                        var checkItem = editUser.DepoSelectList.FirstOrDefault(item => item.Value == Convert.ToString(depo.DepoID));
                        if (checkItem != null)
                        {
                            checkItem.Selected = true;
                        }
                    }
                }

                // IDでユーザー-ハンディメニュー中間リスト取得
                var userMenuList = M_UserConnectController.GetUserMenuByUserId(editUser.UserID, user.DatabaseName);
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

                var config = new MapperConfiguration(cfg => cfg.CreateMap<M_UserModel, M_UserEditModel>());
                IMapper mapper = config.CreateMapper();
                M_UserEditModel editModel = mapper.Map<M_UserEditModel>(editUser);

                return View(editModel);
            }
            catch (Exception)
            {
                return View();
            }
        }

        /// <summary>
        /// ユーザーマスター更新
        /// </summary>
        /// <param name="model">ユーザーマスターの更新情報</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Edit(M_UserModel model)
        {
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();
                if (user == null)
                {
                    // エラーコード：E2011
                    return NotFound(new { errorMessage = "データが見つかりませんでした。" });
                }

                // メイン倉庫IDをチェック
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
                    return NotFound(new { errorMessage = "入力情報が間違っています。" });
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

                // 更新件数が0の場合はエラーとする
                if (!isUpdateMuser)
                {
                    // エラーコード：E2011
                    return NotFound(new { errorMessage = "更新はできませんでした。" });
                }

                return Ok();
            }
            catch (Exception)
            {
                return NotFound(new { errorMessage = "予期せぬエラーが発⽣しました。" });
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

                if (user == null || userId == 0)
                {
                    // エラーコード：E2011
                    return NotFound(new { errorMessage = "データが見つかりませんでした。" });
                }

                // ユーザーマスター削除
                int deleteAffectedRows = M_UserConnectController.DeleteMUser(userId, user.DatabaseName);

                // 更新件数が0の場合はエラーとする
                if (deleteAffectedRows == 0)
                {
                    // エラーコード：E2011
                    return NotFound(new { errorMessage = "データが見つかりませんでした。" });
                }

                return Ok();
            }
            catch (Exception)
            {
                return NotFound(new { errorMessage = "予期せぬエラーが発⽣しました。" });
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
                Utils.ToCSV(mUserDataTable, filePath);
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
        /// ユーザーマスターテーブルを作る
        /// </summary>
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
