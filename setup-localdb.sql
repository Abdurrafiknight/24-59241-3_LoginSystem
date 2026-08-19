-- Run this against (localdb)\MSSQLLocalDB to set up the database
-- used by the Login and Register app.

IF DB_ID('db_users') IS NULL
BEGIN
    CREATE DATABASE db_users;
END
GO

USE db_users;
GO

IF OBJECT_ID('dbo.tbl_users', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[tbl_users]
    (
        [username] NVARCHAR(50) NOT NULL PRIMARY KEY,
        [password] NVARCHAR(50) NULL
    );
END
GO
