using Dapper;
using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.Models;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace ai_truck_load_measurement.ConnectControllers
{
    public class M_UserConnectController 
    {
        /// <summary>
        /// ユーザー情報取得用SQL
        /// </summary>
        /// <returns></returns>
        public static string CreateSQLToSelectMUsers()
        {
            var sql = $@"
                SELECT 
                   Users.user_id,
                   Users.ad_name,
                   Users.depo_id,
                   Depos.name AS depo_name,
                   Users.authorized_kubun,
                   Users.is_required_mail,
                   Users.mail_address,
                   Users.created_at,
                   Users.created_by,
                   Users.updated_at,
                   Users.updated_by
                FROM 
                    m_users AS Users
                INNER JOIN
	                m_depos AS Depos
                ON
	                Users.depo_id = Depos.depo_id
                WHERE 
                    Users.is_deleted = 0
            ";
            return sql;
        }

        /// <summary>
        /// メール受け取りユーザー情報取得用SQL
        /// </summary>
        /// <returns></returns>
        public static string CreateSQLToSelectMUsersIsRequiredMail()
        {
            var sql = $@"
                SELECT 
                   Users.user_id,
                   Users.ad_name,
                   Users.depo_id,
                   Depos.name AS depo_name,
                   Users.authorized_kubun,
                   Users.created_at,
                   Users.created_by,
                   Users.updated_at,
                   Users.updated_by
                FROM 
                    m_users AS Users
                INNER JOIN
	                m_depos AS Depos
                ON
	                Users.depo_id = Depos.depo_id
                WHERE 
                    Users.is_deleted = 0
                AND
                    Users.is_required_mail = 1
            ";
            return sql;
        }

        /// <summary>
        /// DataTable用のユーザーマスター情報取得SQL作成
        /// </summary>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectMUsersForDataTable()
        {
            var sql = $@"
                SELECT 
                    Users.user_id,
                    Users.ad_name,
                    Depos.name AS depo_name,
                    Users.authorized_kubun,
                    Users.is_required_mail,
                    Users.mail_address,
                    Users.is_deleted,
                    FORMAT (Users.created_at, 'yyyy/MM/dd HH:mm:ss'),
                    Users.created_by,
                    FORMAT (Users.updated_at, 'yyyy/MM/dd HH:mm:ss'),
                    Users.updated_by
                FROM 
                    m_users AS Users
                INNER JOIN
	                m_depos AS Depos
                ON
	                Users.depo_id = Depos.depo_id
                WHERE 
                    Users.is_deleted = 0
            ";
            return sql;
        }

        /// <summary>
        /// ユーザーマスター登録SQL作成
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <param name="createdAt">システムタイム</param>
        /// <param name="createdBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToInsertMUser(M_UserModel model, DateTime createdAt, string createdBy)
        {
            string formatCreatedAt = createdAt.ToString("yyyy/MM/dd HH:mm:ss");

            var sql = $@"
                INSERT INTO m_users(
                    ad_name, 
                    depo_id,
                    authorized_kubun,
                    is_required_mail,
                    mail_address,
                    created_at,
                    created_by,
                    updated_at,
                    updated_by
                )
                VALUES (
                    '{model.ADName}',
                    '{model.DepoID}',
                    '{model.AuthorizedKubun}',
                    '{model.IsRequiredMail}',
                    '{model.MailAddress}',
                    '{formatCreatedAt}',
                    '{createdBy}',
                    '{formatCreatedAt}',
                    '{createdBy}'
                );
            ";
            return sql;
        }

        /// <summary>
        /// ユーザーマスター再登録SQL作成
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <param name="updatedAt">システムタイム</param>
        /// <param name="updatedBy">登録者名</param>
        /// <returns></returns>
        public static string CreateSQLToReInsertMUser(M_UserModel model, DateTime updatedAt, string updatedBy)
        {
            var sql = $@"
                UPDATE m_users
                SET 
                    depo_id = {model.DepoID},
                    authorized_kubun = '{model.AuthorizedKubun}',
                    is_required_mail = '{model.IsRequiredMail}',
                    mail_address = '{model.MailAddress}',
                    is_deleted = 0,
                    updated_at = '{updatedAt}',
                    updated_by = '{updatedBy}'
                WHERE 
                    ad_name = '{model.ADName}'
            ;";
            return sql;
        }

        /// <summary>
        /// ユーザーマスター更新SQL作成
        /// </summary>
        /// <param name="model">更新情報</param>
        /// <param name="updatedAt">システムタイム</param>
        /// <param name="updatedBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToUpdateMUser(M_UserModel model, DateTime updatedAt, string updatedBy)
        {
            var sql = $@"
                UPDATE m_users
                SET 
                    ad_name = '{model.ADName}',
                    depo_id = '{model.DepoID}',
                    authorized_kubun = '{model.AuthorizedKubun}',
                    is_required_mail = '{model.IsRequiredMail}',
                    mail_address = '{model.MailAddress}',
                    updated_at = '{updatedAt}',
                    updated_by = '{updatedBy}'
                WHERE
                    user_id = {model.UserID}
                    and is_deleted = 0
            ";
            return sql;
        }

        /// <summary>
        /// ユーザーマスター削除SQL作成
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <param name="updatedAt">システムタイム</param>
        /// <param name="updatedBy">ユーザー名</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToDeleteMUser(int userId, DateTime updatedAt, string updatedBy)
        {
            var sql = $@"
                UPDATE m_users
                SET 
                    is_deleted = 1,
                    updated_at = '{updatedAt}',
                    updated_by = '{updatedBy}'
                WHERE 
                    user_id = {userId}
            ;";
            return sql;
        }

        /// <summary>
        /// 異なるユーザーIDで重複AD名情報取得SQL作成
        /// </summary>
        /// <param name="model">登録情報</param>
        /// <returns>SQL文</returns>
        public static string CreateSQLToSelectDuplicateADName(M_UserModel model)
        {
            if(string.IsNullOrEmpty(model.UserID.ToString()))
                model.UserID = 0;

            var sql = $@"
                SELECT
                    COUNT(*)                      
                FROM 
                    m_users
                WHERE
                    ad_name = '{model.ADName}'
                    AND user_id <> {model.UserID}
                    and is_deleted = 0
            ";

            return sql;
        }

        /// <summary>
        /// 削除済みの重複AD名情報取得SQL作成
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string CreateSQLToDuplicateDeletedADName(string adName)
        {
            var sql = $@"
                SELECT
                    COUNT(*)                      
                FROM 
                    m_users
                WHERE
                    ad_name = '{adName}'
                    and is_deleted = 1
            ";

            return sql;
        }

        /// <summary>
        /// AD名から新規通知有無取得SQL作成
        /// </summary>
        /// <param name="adName"></param>
        /// <returns></returns>
        public static string CreateSQLToSelectHasNewsNotificationFromADName(string adName)
        {
            var sql = $@"
                SELECT has_news_notification
                FROM m_users
                WHERE ad_name = '{adName}'
            ";
            return sql;
        }

        /// <summary>
        /// 新規通知情報更新SQL作成
        /// </summary>
        /// <param name="hasNewsNotification"></param>
        /// <param name="adName"></param>
        /// <returns></returns>
        public static string CreateSQLToUpdateHasNewsNotification(bool hasNewsNotification, string adName)
        {
            var hasNewsNotificationBit = 0;
            if (hasNewsNotification)
                hasNewsNotificationBit = 1;

            var sql = $@"
                UPDATE m_users
                SET 
                    has_news_notification = {hasNewsNotificationBit}
                WHERE
                    is_deleted = 0
            ";
            if (!hasNewsNotification)
            {
                sql += $@"
                    AND ad_name = '{adName}'
                ";
            }
            return sql;
        }
    }
}
