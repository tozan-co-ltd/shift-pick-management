using mar_sumaken_web.Properties;
using System.ComponentModel.DataAnnotations;

namespace mar_sumaken_web.Models
{
    /// <summary>
    /// ユーザーマスターのModel
    /// </summary>
    public class M_UserModel
    {
        /// <summary>
        /// ユーザーマスターリスト
        /// </summary>
        public List<M_User> M_UserList { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public class M_User
        {
            /// <summary>
            /// 
            /// </summary>
            public List<SelectItem> DepoSelectList { get; set; } = new List<SelectItem>();

            /// <summary>
            /// 
            /// </summary>
            public List<SelectItem> HandyMenuSelectList { get; set; } = new List<SelectItem>();

            /// <summary>
            /// 倉庫マスターリスト
            /// </summary>
            public List<M_DepoModel> M_DepoList { get; set; } = new List<M_DepoModel> { };

            /// <summary>
            /// ハンディメニューリスト
            /// </summary>
            public List<M_HandyMenuModel> M_HandyMenuList { get; set; } = new List<M_HandyMenuModel> { };

            /// <summary>
            /// ユーザーID
            /// </summary>
            [Display(Name = "ID")]
            public int UserID { get; set; }

            /// <summary>
            /// ログインID
            /// </summary>
            [Display(Name = "ログインID")]
            [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
            [RegularExpression(@"[a-zA-Z0-9]+", ErrorMessage = "ログインIDは半角英数字のみ入力できます")]
            public string LoginID { get; set; }

            /// <summary>
            /// ユーザー名
            /// </summary>
            [Display(Name = "ユーザー名")]
            [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
            public string UserName { get; set; }

            /// <summary>
            /// ロール
            /// </summary>
            [Display(Name = "ロール")]
            public int Role { get; set; }

            /// <summary>
            /// メイン倉庫ID
            /// </summary>
            [Display(Name = "メイン倉庫ID")]
            public int DepoID { get; set; }

            /// <summary>
            /// 倉庫名
            /// </summary>
            [Display(Name = "倉庫名")]
            public string DepoName { get; set; } = string.Empty;

            /// <summary>
            /// 管理権限区分
            /// </summary>
            [Display(Name = "管理権限区分")]
            [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
            public int AuthorizedKubun { get; set; }

            /// <summary>
            /// 管理権限区分名
            /// </summary>
            public string AuthorizedKubunName { get; set; } = string.Empty;

            /// <summary>
            /// パスワード
            /// </summary>
            [Display(Name = "パスワード")]
            [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
            [RegularExpression(@"[a-zA-Z0-9]{4,10}", ErrorMessage = "パスワードは４～10文字入力してください。")]
            public string Password { get; set; }

            /// <summary>
            /// ソルト
            /// </summary>
            [Display(Name = "ソルト")]
            public string Salt { get; set; } = string.Empty;

            /// <summary>
            /// 未使用フラグ
            /// </summary>
            [Display(Name = "未使用フラグ")]
            public bool IsDeleted { get; set; }

            /// <summary>
            /// 最終ログイン日時
            /// </summary>
            [Display(Name = "最終ログイン日時")]
            public DateTime LastLoginDatetime { get; set; }

            /// <summary>
            /// ログインフラグ: 1 ログイン , 0 ログアウト
            /// </summary>
            [Display(Name = "ユーザー名")]
            public bool IsLogin { get; set; }

            /// <summary>
            /// 作成日時
            /// </summary>
            [Display(Name = "作成日時")]
            public DateTime CreatedAt { get; set; }

            /// <summary>
            /// 作成者
            /// </summary>
            [Display(Name = "作成者")]
            public string CreatedBy { get; set; } = string.Empty;

            /// <summary>
            /// 更新日時
            /// </summary>
            [Display(Name = "更新日時")]
            public DateTime UpdatedAt { get; set; }

            /// <summary>
            /// 更新者
            /// </summary>
            [Display(Name = "更新者")]
            public string UpdatedBy { get; set; } = string.Empty;

        }
    }
}
