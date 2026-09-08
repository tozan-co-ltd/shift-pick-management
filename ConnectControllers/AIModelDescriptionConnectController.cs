namespace shift_pick_management.ConnectControllers
{
    public class AIModelDescriptionConnectController
    {
        public static string CreateSQLToSelectAIModelDescriptions()
        {
            var sql = $@"
                SELECT *
                FROM m_ai_models
                ORDER BY start_date DESC
            ";
            return sql;
        }
    }
}
