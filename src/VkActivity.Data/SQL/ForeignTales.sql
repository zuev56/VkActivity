CREATE EXTENSION IF NOT EXISTS postgres_fdw WITH SCHEMA public;
CREATE SERVER IF NOT EXISTS ActiveVkDb FOREIGN DATA WRAPPER postgres_fdw OPTIONS (host 'srv-1', dbname 'Home', port '5632');
CREATE USER MAPPING IF NOT EXISTS FOR postgres SERVER ActiveVkDb OPTIONS(user 'postgres', password 'postgres');

CREATE FOREIGN TABLE IF NOT EXISTS vk.users_prod (
    user_id     serial      NOT NULL,
    first_name  varchar(50)     NULL,
    last_name   varchar(50)     NULL,
    raw_data    json        NOT NULL,
    update_date timestamptz NOT NULL DEFAULT now(),
    insert_date timestamptz NOT NULL DEFAULT now()
)
SERVER ActiveVkDb OPTIONS(schema_name 'vk', table_name 'users');

CREATE FOREIGN TABLE IF NOT EXISTS vk.activity_log_prod (
    activity_log_id  serial       NOT NULL,
    user_id          int          NOT NULL,
    is_online        bool             NULL,
	is_online_mobile bool         NOT NULL,
	online_app       int              NULL,
	last_seen        int              NULL,
    insert_date      timestamptz  NOT NULL DEFAULT now()
)
SERVER ActiveVkDb OPTIONS(schema_name 'vk', table_name 'activity_log');
	