SELECT 'CREATE DATABASE cmc_dev'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'cmc_dev')\gexec

DO $$
BEGIN
    GRANT ALL PRIVILEGES ON DATABASE cmc_dev TO postgres;

    UPDATE pg_database SET datallowconn = true WHERE datname = 'cmc_dev';

    RAISE NOTICE 'Database cmc_dev initialized successfully';
END $$;
