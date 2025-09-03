--
-- PostgreSQL database cluster dump
--

SET default_transaction_read_only = off;

SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;

--
-- Roles
--

CREATE ROLE postgres;
ALTER ROLE postgres WITH SUPERUSER INHERIT CREATEROLE CREATEDB LOGIN REPLICATION BYPASSRLS PASSWORD 'SCRAM-SHA-256$4096:ttcSYinXtGiSzYyyggJnkg==$fLODcArep+lEQX9bqkRv1uQLkjTt8KDbwaQR9NQ9cvg=:3pctumf9p3ZMadx+0m1Q3AKwoLwAUv8PlbeiAEhB+Go=';

--
-- User Configurations
--








--
-- PostgreSQL database cluster dump complete
--

