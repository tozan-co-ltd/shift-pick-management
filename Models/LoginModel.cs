using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ai_truck_load_measurement.Models
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
        //[RegularExpression(@"[a-zA-Z0-9]+")]
        public string? LoginId { get; set; }

        /// <summary>
        /// パスワード
        /// </summary>
        [Display(Name = "パスワード")]
        [DataType(DataType.Password)]
        [Required]
        //[RegularExpression(@"[a-zA-Z0-9]")]
        public string? Password { get; set; }

        /// <summary>
        /// システムバージョン
        /// </summary>
        public string SystemVersion { get; set; } = GetSystemVersion();

        /// <summary>
        /// システムバージョン取得
        /// </summary>
        /// <returns>アセンブリバージョン</returns>
        public static string GetSystemVersion()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly().GetName();
                return assembly.Version.ToString(3);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
