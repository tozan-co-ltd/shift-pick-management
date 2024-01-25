using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using mar_sumaken_web.Models;
using mar_sumaken_web.Controllers;

namespace mar_sumaken_web.Commons
{
    /// <summary>
    /// エラー処理に関する関数
    /// </summary>
    public class ErrorHandling
    {
        private static readonly ILogger<ErrorHandling> _logger;

        static ErrorHandling()
        {
            _logger = LoggerFactory.Create(builder =>
            {
                builder.AddConsole(); // Hoặc các loại logger khác tùy theo yêu cầu
            }).CreateLogger<ErrorHandling>();
        }

        /// <summary>
        /// エラーメッセージ作成
        /// </summary>
        /// <param name="errorCode">エラーコード</param>
        /// <remarks>エラーコードから表示用エラーメッセージを作成する</remarks>
        /// <returns>エラーメッセージ</returns>
        public static string CreateErrorMessage(string errorCode)
        {
            try
            {
                // 戻り値
                string errorMessage;

                MErrorMessagesModel mErrorMessagesModel = new()
                {
                    ErrorCode = errorCode,
                };

                // エラーメッセージ取得
                var getErrorMessages = GetErrorMessage(mErrorMessagesModel);

                // 表示用エラーメッセージ作成
                if (getErrorMessages.Count == 0)
                {
                    errorMessage = "E4002 SQLServerでエラーが発生しました。";
                    // log取得
                    var errorCause = "該当エラーコードなし";
                    _logger.LogError($"{errorMessage} {errorCause} 引数:{errorCode}");
                }
                else
                {
                    var getErrorMessage = getErrorMessages[0];
                    errorMessage = getErrorMessage.ErrorCode + " " + getErrorMessage.ErrorMessage;
                }
                return errorMessage;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// エラーメッセージ取得
        /// </summary>
        /// <param name="model">MErrorMessagesModel</param>
        /// <returns>エラーメッセージリスト</returns>
        public static List<MErrorMessagesModel> GetErrorMessage(MErrorMessagesModel model)
        {
            try
            {
                // SQL作成
                var sql = MErrorMessagesConnectController.CreateSQLToGetErrorMessage(model.ErrorCode);
                // DB接続
                List<MErrorMessagesModel> strList = MErrorMessagesConnectController.ConnectMErrorMessages(sql);
                return strList;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
