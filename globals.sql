--
-- PostgreSQL database cluster dump
--

\restrict F8IkVPKtjSY3A8uo5VOPt8YrOecbdXJoMOhnNdTgScaPWtDewsCSc5t2CSTHneg

SET default_transaction_read_only = off;

SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;

--
-- Roles
--

CREATE ROLE postgres;
ALTER ROLE postgres WITH SUPERUSER INHERIT CREATEROLE CREATEDB LOGIN REPLICATION BYPASSRLS PASSWORD 'SCRAM-SHA-256$4096:u6+TkkUw134dV2O1nRabwg==$bDsYbuRnZeZopdRSRdMFuGV/8Yz5LouJD6r5uY0xXGc=:T5s8MeVexWZiWnnoZEXpYAggIti/3KWKhdleZj2JhIw=';

--
-- User Configurations
--








\unrestrict F8IkVPKtjSY3A8uo5VOPt8YrOecbdXJoMOhnNdTgScaPWtDewsCSc5t2CSTHneg

--
-- PostgreSQL database cluster dump complete
--

