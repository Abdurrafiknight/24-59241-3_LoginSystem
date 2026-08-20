-- ============================================================
-- Lab 1: Login, Registration & Logout
-- Student ID: 24-59241-3
-- Student Name: Rafi
-- Database Schema Script
-- ============================================================

-- Create Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'24-59241-3_LoginDB')
BEGIN
    CREATE DATABASE [24-59241-3_LoginDB];
END
GO

USE [24-59241-3_LoginDB];
GO

-- Create Users Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE dbo.Users (
        UserID INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(50) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(200) NOT NULL,
        Email NVARCHAR(100),
        FullName NVARCHAR(100),
        CreatedAt DATETIME DEFAULT GETDATE()
    );
END
GO

-- Create LoginHistory Table (Bonus Task)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LoginHistory')
BEGIN
    CREATE TABLE dbo.LoginHistory (
        HistoryID INT IDENTITY(1,1) PRIMARY KEY,
        UserID INT NOT NULL,
        LoginTime DATETIME DEFAULT GETDATE(),
        LogoutTime DATETIME NULL,
        FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID)
    );
END
GO

-- Insert a test user (optional)
-- Password: Test@123 (hashed value would go here)
-- INSERT INTO dbo.Users (Username, PasswordHash, Email, FullName)
-- VALUES ('testuser', 'hashed_value_here', 'test@email.com', 'Test User');
