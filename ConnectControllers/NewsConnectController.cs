using ai_truck_load_measurement.Models;

namespace ai_truck_load_measurement.ConnectControllers
{
    public class NewsConnectController
    {
        /// <summary>
        /// お知らせ情報取得SQL
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// お知らせ情報登録SQL
        /// </summary>
        /// <param name="model"></param>
        /// <param name="createdAt"></param>
        /// <param name="createdBy"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 指定されたお知らせIDの情報取得SQL
        /// </summary>
        /// <param name="newsID"></param>
        /// <returns></returns>
        public static string CreateSQLToSelectNewsFromNewsID(int newsID)
        {
            var sql = $@"
                SELECT
                    news_subject
                    ,news_content
                    ,category_class
                    ,created_at
                FROM t_news
                WHERE news_id = {newsID}
            ";
            return sql;
        }
    }
}