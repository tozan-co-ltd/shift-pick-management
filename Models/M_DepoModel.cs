using ai_truck_load_measurement.Models;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Commons;

namespace ai_truck_load_measurement.Models
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

        
        public static M_DepoModel GetMainDepo(LoginUserModel user)
        {
            M_DepoModel model = new M_DepoModel();
            var sql = M_DepoConnectController.CreateSQLToSelectDepoFromADName(user.ADName);
            var depoList = ConnectToSQLServer.ExecuteQueryToList<M_DepoModel>(sql);
            if (depoList.Count > 0)
            {
                model = depoList[0];
            }
            return model;
            
        }
    }
}
