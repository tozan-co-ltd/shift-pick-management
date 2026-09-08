using System.ComponentModel.DataAnnotations;
using shift_pick_management.Properties;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;

namespace shift_pick_management.Models
{
    public class LoadOutputModel : LoadRecordModel
    {
       
    }

    /// <summary>
    /// 検索後の便実績リストのhtmlと要素数を保存するクラス
    /// </summary>
    public class SearchedTripRecordListModel{
        // 便実績リストのhtml
        public string? searchedTripRecordHTML {  get; set; }
        // 便実績リストの要素数
        public int searchedTripRecordLength {  get; set; }
    }
}
