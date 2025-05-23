using ai_truck_load_measurement.Models;

namespace ai_truck_load_measurement.ConnectControllers
{
    public class NewsConnectController
    {
        public static string CreateSQLToSelectNews()
        {
            var sql = $@"
            SELECT
                news_id
                ,news_subject
                ,news_content
                ,category_class
                ,created_at
                ,created_by
            FROM t_news
            ";
            return sql;
        }

        public static string CreateSQLToInsertNews(NewsModel model, DateTime createdAt, string createdBy)
        {
            var sql = $@"
                INSERT INTO t_news(
                    news_subject 
                    ,news_content
                    ,category_class
                    ,created_at
                    ,created_by
                )
                VALUES (
                    '{model.NewsSubject}',
                    '{model.NewsContent}',
                    '{model.CategoryClass}',
                    '{createdAt.ToString("yyyy/MM/dd HH:mm:ss")}',
                    '{createdBy}'
                );
            ";
            return sql;
        }
    }
}