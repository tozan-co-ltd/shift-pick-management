namespace ai_truck_load_measurement.Models
{
    /// <summary>
    /// CSVファイル取込のModel
    /// </summary>
    public class CsvFileInputModel
    {
        /// <summary>
        /// インポートファイル
        /// </summary>
        public IFormFile ImportFile { get; set; }

        /// <summary>
        /// ファイル名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 取得する合計ヘッダー列
        /// </summary>
        public int HeaderColumnCount { get; set; }

        /// <summary>
        /// 列とヘッダー名
        /// </summary>
        public Dictionary<int, string> HeaderSettings { get; set; }
    }
}
