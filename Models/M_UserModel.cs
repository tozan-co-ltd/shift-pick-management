using X.PagedList;
using System.ComponentModel.DataAnnotations;

namespace mar_sumaken_web.Models
{
    /// <summary>
    /// ユーザーマスターのModel
    /// </summary>
    public class M_UserModel
    {
        public IPagedList<M_User> MUserList { get; set; }

        public class M_User
        {
            /// <summary>
            /// ユーザーID
            /// </summary>
            [Display(Name = "ユーザーID")]
            public int UserID { get; set; }

            /// <summary>
            /// ログインID
            /// </summary>
            [Display(Name = "ログインID")]
            [Required(ErrorMessage = "E1001 値が未入力です。値を入力してください。")]
            [RegularExpression(@"[a-zA-Z0-9]+", ErrorMessage = "ログインIDは半角英数字のみ入力できます")]
            public string LoginID { get; set; }

            /// <summary>
            /// ユーザー名
            /// </summary>
            [Display(Name = "ユーザー名")]
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
            public string DepoName { get; set; }

            /// <summary>
            /// 管理権限区分
            /// </summary>
            [Display(Name = "管理権限区分")]
            public int AuthorizedKubun { get; set; }

            /// <summary>
            /// パスワード
            /// </summary>
            [Display(Name = "パスワード")]
            [Required(ErrorMessage = "パスワードは入力必須項目です")]
            [RegularExpression(@"[a-zA-Z0-9]{4,10}", ErrorMessage = "パスワードは４～10文字入力してください。")]
            public string Password { get; set; }

            /// <summary>
            /// ソルト
            /// </summary>
            [Display(Name = "ソルト")]
            public string Salt { get; set; }

            /// <summary>
            /// 未使用フラグ
            /// </summary>
            [Display(Name = "未使用フラグ")]
            public bool NotUseFlag { get; set; }

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
            public string CreatedBy { get; set; }

            /// <summary>
            /// 更新日時
            /// </summary>
            [Display(Name = "更新日時")]
            public DateTime UpdatedAt { get; set; }

            /// <summary>
            /// 更新者
            /// </summary>
            [Display(Name = "更新者")]
            public string UpdatedBy { get; set; }

        }
    }
}
