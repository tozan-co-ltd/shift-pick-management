using AutoMapper;
using mar_sumaken_web.Commons;
using mar_sumaken_web.ConnectControllers;
using mar_sumaken_web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Runtime.CompilerServices;
using static mar_sumaken_web.Models.M_UserModel;

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
                List<M_User> userList = M_UserConnectController.ConnectMUsers(sql, user.DatabaseName);
                
                if (userList.Count > 0)
                {
                    // ユーザーマスターの詳細を取得する
                    userList = M_UserConnectController.GetMUserDetailList(userList, user.DatabaseName);
                    model.M_UserList = userList;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                //// エラーメッセージ取得
                //// 「SQLServerでエラーが発生しました。」
                //errorMessage = ErrorHandling.CreateErrorMessage("E4002");

                //// log取得
                //var exceptionMessage = ex.Message;
                //_logger.LogError($"{exceptionMessage} {errorMessage}");

                //var shippingImportErrorModel = new HandyErrorMessageModel
                //{
                //    Message = errorMessage + exceptionMessage
                //};
                //return View(shippingImportErrorModel);
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
            M_User model = new M_User();
            try
            {
                // ログイン中ユーザー情報取得
                var user = ClaimsLoginUserData();

                // 倉庫マスター情報取得
                var depoList = M_DepoConnectController.GetMDepoList(user.DatabaseName);
                foreach (var depo in depoList)
                {
                    SelectItem depoItem = new SelectItem();
                    depoItem.Name = depo.DepoName;
                    depoItem.Value = depo.DepoID;
                    depoItem.IsSelected = false;

                    model.DepoSelectList.Add(depoItem);
                }

                // ハンディメニューマスター情報取得
                var menuList = M_HandyMenuConnectController.GetMHandyMenuList(user.DatabaseName);
                foreach (var menu in menuList)
                {
                    SelectItem menuItem = new SelectItem();
                    menuItem.Name = menu.HandyMenuName;
                    menuItem.Value = menu.HandyMenuID;
                    menuItem.IsSelected = false;

                    model.HandyMenuSelectList.Add(menuItem);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                //// エラーメッセージ取得
                //// 「SQLServerでエラーが発生しました。」
                //errorMessage = ErrorHandling.CreateErrorMessage("E4002");

                //// log取得
                //var exceptionMessage = ex.Message;
                //_logger.LogError($"{exceptionMessage} {errorMessage}");

                //var shippingImportErrorModel = new HandyErrorMessageModel
                //{
                //    Message = errorMessage + exceptionMessage
                //};
                //return View(shippingImportErrorModel);
                return View();
            }
        }

        /// <summary>
        /// ユーザーマスター登録
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Register(M_User model)
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
                foreach (SelectItem item in model.DepoSelectList)
                {
                    if (item.IsSelected)
                    {
                        isDepoSelected = true;
                    }
                }
                // 登録情報をチェック
                if (!ModelState.IsValid || !isDepoSelected)
                {
                    return NotFound();
                }

                // 重複ユーザー情報取をチェック
                bool isDuplicate = M_UserConnectController.CheckIsDuplicateMUserByLoginId(model.LoginID, user.DatabaseName);
                if (isDuplicate)
                {
                    // エラーを作成
                    // エラーコード：E2011
                    //throw new Exception();
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
            catch (Exception ex)
            {
                // エラーメッセージ取得
                // 「予期せぬエラーが発⽣しました。」
                //errorMessage = ErrorHandling.CreateErrorMessage("E9999");

                // log取得
                //var exceptionMessage = ex.Message;
                //_logger.LogError($"{exceptionMessage} {errorMessage}");
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
            M_User editUser = new M_User();
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
                List<M_User> userList = M_UserConnectController.ConnectMUsers(sql, user.DatabaseName);
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
                    SelectItem depoItem = new SelectItem();
                    depoItem.Name = depo.DepoName;
                    depoItem.Value = depo.DepoID;
                    depoItem.IsSelected = false;

                    editUser.DepoSelectList.Add(depoItem);
                }
                // ハンディメニューマスター情報取得
                var menuList = M_HandyMenuConnectController.GetMHandyMenuList(user.DatabaseName);
                foreach (var menu in menuList)
                {
                    SelectItem menuItem = new SelectItem();
                    menuItem.Name = menu.HandyMenuName;
                    menuItem.Value = menu.HandyMenuID;
                    menuItem.IsSelected = false;

                    editUser.HandyMenuSelectList.Add(menuItem);
                }

                // IDでユーザー倉庫中間リスト取得
                var userDepoList = M_UserConnectController.GetUserDepoByUserId(editUser.UserID, user.DatabaseName);
                if (userDepoList.Count > 0)
                {
                    foreach (var depo in userDepoList)
                    {
                        var checkItem = editUser.DepoSelectList.FirstOrDefault(item => item.Value == depo.DepoID);
                        if (checkItem != null)
                        {
                            checkItem.IsSelected = true;
                        }
                    }
                }

                // IDでユーザー-ハンディメニュー中間リスト取得
                var userMenuList = M_UserConnectController.GetUserMenuByUserId(editUser.UserID, user.DatabaseName);
                if (userMenuList.Count > 0)
                {
                    foreach (var menu in userMenuList)
                    {
                        var checkItem = editUser.HandyMenuSelectList.FirstOrDefault(item => item.Value == menu.HandyMenuID);
                        if (checkItem != null)
                        {
                            checkItem.IsSelected = true;
                        }
                    }
                }


                var config = new MapperConfiguration(cfg => cfg.CreateMap<M_User, M_UserEditModel>());
                IMapper mapper = config.CreateMapper();
                M_UserEditModel editModel = mapper.Map<M_UserEditModel>(editUser);

                return View(editModel);
            }
            catch (Exception ex)
            {
                //// エラーメッセージ取得
                //// 「SQLServerでエラーが発生しました。」
                //errorMessage = ErrorHandling.CreateErrorMessage("E4002");

                //// log取得
                //var exceptionMessage = ex.Message;
                //_logger.LogError($"{exceptionMessage} {errorMessage}");

                //var shippingImportErrorModel = new HandyErrorMessageModel
                //{
                //    Message = errorMessage + exceptionMessage
                //};
                //return View(shippingImportErrorModel);
                return View();
            }
        }

        /// <summary>
        /// ユーザーマスター更新
        /// </summary>
        /// <param name="model">ユーザーマスターの更新情報</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Edit(M_User model)
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
                foreach (SelectItem item in model.DepoSelectList)
                {
                    if (item.IsSelected)
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
                    // エラーを作成
                    // エラーコード：E2011
                    //throw new Exception();
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
            catch (Exception ex)
            {
                // エラーメッセージ取得
                // 「予期せぬエラーが発⽣しました。」
                //errorMessage = ErrorHandling.CreateErrorMessage("E9999");

                // log取得
                //var exceptionMessage = ex.Message;
                //_logger.LogError($"{exceptionMessage} {errorMessage}");
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
            catch (Exception ex)
            {
                // エラーメッセージ取得
                // 「予期せぬエラーが発⽣しました。」
                //errorMessage = ErrorHandling.CreateErrorMessage("E9999");

                // log取得
                //var exceptionMessage = ex.Message;
                //_logger.LogError($"{exceptionMessage} {errorMessage}");
                return NotFound(new { errorMessage = "予期せぬエラーが発⽣しました。" });
            }
        }

        /// <summary>
        /// フォール出力
        /// </summary>
        public JsonResult ExportFile()
        {
            string? errorMessage;
            try
            {
                // log取得
                _logger.LogInformation($"Excel出力開始");

                // テーブルデータ取得
                DataTable dt = CreateDataTable();

                // ファイル名
                var tmpFilename = "ユーザーマスター.csv";
                // CSVファイルへのパスを作成する
                string filePath = Path.Combine(Path.GetTempPath(), tmpFilename);
                // DataTableをCSVに変換する
                Utils.ToCSV(dt, filePath);
                // ファイルの作成
                var file = System.IO.File.ReadAllBytes(filePath);

                return Json(new { data = File(file, System.Net.Mime.MediaTypeNames.Application.Octet, tmpFilename) });
            }
            catch (Exception ex)
            {
                // エラーメッセージ取得
                // 「予期せぬエラーが発⽣しました。」
                //errorMessage = ErrorHandling.CreateErrorMessage("E9999");

                // log取得
                //var exceptionMessage = ex.Message;
                //_logger.LogInformation($"{exceptionMessage} {errorMessage}");
                return Json(new { res = "NG", error = "予期せぬエラーが発⽣しました。" });
            }
        }

        private DataTable CreateDataTable()
        {
            var table = new DataTable();
            table.Columns.Add("ID", typeof(string));
            table.Columns.Add("ログインID", typeof(string));
            table.Columns.Add("ユーザー名 ", typeof(string));
            table.Columns.Add("メイン倉庫名", typeof(string));
            table.Columns.Add("管理権限区分", typeof(string));
            table.Columns.Add("更新日時", typeof(string));
            table.Columns.Add("更新者", typeof(string));
            return table;
        }

    }
}
