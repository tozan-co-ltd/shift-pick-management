using ai_truck_load_measurement.Properties;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace ai_truck_load_measurement.Models
{
    /// <summary>
    /// ユーザーマスターのModel
    /// </summary>
    public class M_UserModel: CommonModel
    {
        /// <summary>
        /// ユーザーマスターリスト
        /// </summary>
        public IPagedList<M_UserModel>? M_UserList { get; set; }

        /// <summary>
        /// 倉庫リスト
        /// </summary>
        public List<M_DepoModel> M_DepoList { get; set; } = new List<M_DepoModel> { };

        /// <summary>
        /// ハンディメニューリスト
        /// </summary>
        public List<M_HandyMenuModel> M_HandyMenuList { get; set; } = new List<M_HandyMenuModel> { };

        /// <summary>
        /// 選択倉庫リスト
        /// </summary>
        public List<SelectListItem> DepoSelectList { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// 選択ハンディメニューリスト
        /// </summary>
        public List<SelectListItem> HandyMenuSelectList { get; set; } = new List<SelectListItem>();

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
        [RegularExpression(@"[a-zA-Z0-9]+", ErrorMessageResourceName = "E1003", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [StringLength(12, MinimumLength = 4, ErrorMessageResourceName = "E1023", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string LoginID { get; set; }

        /// <summary>
        /// ユーザー名
        /// </summary>
        [Display(Name = "ユーザー名")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [MaxLength(50, ErrorMessageResourceName = "E1008", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string UserName { get; set; }

        /// <summary>
        /// ロール
        /// </summary>
        [Display(Name = "ロール")]
        public int Role { get; set; }

        /// <summary>
        /// メイン倉庫ID
        /// </summary>
        public int MainDepoID { get; set; }

        /// <summary>
        /// メイン倉庫名
        /// </summary>
        [Display(Name = "メイン倉庫名")]
        public string? MainDepoName { get; set; }

        /// <summary>
        /// 選択されたメイン倉庫ID
        /// </summary>
        [Display(Name = "メイン倉庫名")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int SelectedMainDepoID { get; set; }

        /// <summary>
        /// 管理権限区分
        /// </summary>
        [Display(Name = "管理権限区分")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int AuthorizedKubun { get; set; }

        /// <summary>
        /// 管理権限区分名
        /// </summary>
        public string? AuthorizedKubunName { get; set; }

        /// <summary>
        /// パスワード
        /// </summary>
        [Display(Name = "パスワード")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [RegularExpression(@"[a-zA-Z0-9]+", ErrorMessageResourceName = "E1003", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [StringLength(12, MinimumLength = 4, ErrorMessageResourceName = "E1023", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string Password { get; set; }

        /// <summary>
        /// ソルト
        /// </summary>
        [Display(Name = "ソルト")]
        public string? Salt { get; set; }

        /// <summary>
        /// 未使用フラグ
        /// </summary>
        [Display(Name = "未使用フラグ")]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 最終ログイン日時
        /// </summary>
        [Display(Name = "最終ログイン日時")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
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
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 作成者
        /// </summary>
        [Display(Name = "作成者")]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        [Display(Name = "更新日時")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 更新者
        /// </summary>
        [Display(Name = "更新者")]
        public string? UpdatedBy { get; set; }
    }
}
