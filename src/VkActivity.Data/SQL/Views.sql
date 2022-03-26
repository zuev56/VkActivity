-- (!) Script must be reexecutable

--DROP VIEW vk.v_activity_log;

CREATE OR REPLACE VIEW vk.v_activity_log
AS
     SELECT l.activity_log_id,
            (u.first_name::text || ' '::text) || u.last_name::text as user_name,
            u.user_id,
            l.is_online::int as online,
            l.is_online,
            l.is_online_mobile::int as mobile,
            l.is_online_mobile,
            to_char(l.insert_date, 'DD.MM.YYYY HH24:MI:SS') as date,
            to_timestamp(l.last_seen) as last_seen,
            l.insert_date
       FROM vk.activity_log l
  LEFT JOIN vk.users u ON l.user_id = u.user_id;

  GRANT ALL ON vk.v_activity_log TO app;


CREATE OR REPLACE VIEW vk.v_compare_with_prod
 AS
 SELECT sq.source AS src,
    (((('All:'::text || sq.allr) || ';  Online:'::text) || sq.online) || ';  OnlineMob:'::text) || sq.online_mobile AS "values"
   FROM ( WITH data AS (
                 SELECT 1 AS num,
                    'PROD'::text AS source,
                    ( SELECT count(*) AS count
                           FROM vk.activity_log_prod) AS allr,
                    ( SELECT count(*) AS count
                           FROM vk.activity_log_prod
                          WHERE activity_log_prod.is_online = true) AS online,
                    ( SELECT count(*) AS count
                           FROM vk.activity_log_prod
                          WHERE activity_log_prod.is_online_mobile = true) AS online_mobile
                UNION
                 SELECT 2 AS num,
                    'TEST'::text AS source,
                    ( SELECT count(*) AS count
                           FROM vk.activity_log) AS allr,
                    ( SELECT count(*) AS count
                           FROM vk.activity_log
                          WHERE activity_log.is_online = true) AS online,
                    ( SELECT count(*) AS count
                           FROM vk.activity_log
                          WHERE activity_log.is_online_mobile = true) AS online_mobile
                )
         SELECT data.num,
            data.source,
            data.allr,
            data.online,
            data.online_mobile
           FROM data
        UNION
         SELECT 3 AS num,
            'DIFF'::text AS text,
            max(data.allr) - min(data.allr) AS allr,
            max(data.online) - min(data.online) AS online,
            max(data.online_mobile) - min(data.online_mobile) AS online_mobile
           FROM data
  ORDER BY 1) sq;

ALTER TABLE vk.v_compare_with_prod
    OWNER TO postgres;

GRANT ALL ON TABLE vk.v_compare_with_prod TO app;
GRANT ALL ON TABLE vk.v_compare_with_prod TO postgres;

