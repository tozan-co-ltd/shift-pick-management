using shift_pick_management.Models;
using shift_pick_management.ConnectControllers;
using shift_pick_management.Commons;

namespace shift_pick_management.Models
{
    /// <summary>
    /// デポモデル
    /// </summary>
    public class M_DepoModel
    {
        // デポID
        public int DepoID {  get; set; }
        // デポ名
        public string Name { get; set; }
        // 作成日時
        public DateTime CreatedAt { get; set; }
        // 作成者
        public string CreatedBy {  get; set; }
        // 更新日時
        public DateTime UpdatedAt { get; set; }
        // 更新者
        public string UpdatedBy { get; set; }
    }
}
