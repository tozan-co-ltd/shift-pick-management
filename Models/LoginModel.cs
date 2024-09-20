using System.ComponentModel.DataAnnotations;

namespace  ai_truck_load_measurement.Models
{
    /// <summary>
    /// ログイン時に使うModel
    /// </summary>
    public class LoginModel
    {
        /// <summary>
        /// ログインID
        /// </summary>
        [Display(Name = "ログインID")]
        [Required]
        [RegularExpression(@"[a-zA-Z0-9]+")]
        public string? LoginId { get; set; }

        /// <summary>
        /// パスワード
        /// </summary>
        [Display(Name = "パスワード")]
        [DataType(DataType.Password)]
        [Required]
        [RegularExpression(@"[a-zA-Z0-9]{4,10}")]
        public string? Password { get; set; }
    }
}
