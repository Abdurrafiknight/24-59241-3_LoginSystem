# 24-59241-3 Login System

**Student ID:** 24-59241-3
**Student Name:** Rafi
**Course:** Lab 1 – Login, Registration & Logout with C# and SQL Server

## Project Overview

This project is a **C# Windows Forms Login and Registration System** connected to **SQL Server LocalDB**.

The system allows users to:

* Register a new account
* Log in securely
* Log out
* Change their password
* View user information
* View login history
* Prevent login after 3 failed attempts

Passwords are stored as **SHA-256 hashes**, and database operations use **parameterized SQL queries** to prevent SQL injection.

## Technologies Used

* **Language:** C#
* **Framework:** .NET Framework 4.7.2
* **IDE:** Visual Studio 2022 Community
* **Database:** SQL Server 2022 Express / LocalDB
* **UI:** Windows Forms

## Main Features

### Registration

* Username and password validation
* Password confirmation
* Email and full name
* Username uniqueness check
* Secure password hashing

### Login

* Username and password verification
* SHA-256 password comparison
* Maximum 3 failed login attempts
* Welcome screen after successful login

### Logout

* Records logout time
* Returns the user to the login screen
* Clears the login form

### Login History

The system records:

* User ID
* Login time
* Logout time

### Change Password

Users can change their password after verifying their current password.

## Database

The project uses two main tables:

### `Users`

| Column       | Type          | Description           |
| ------------ | ------------- | --------------------- |
| UserID       | INT           | Primary Key           |
| Username     | NVARCHAR(50)  | Unique username       |
| PasswordHash | NVARCHAR(200) | Hashed password       |
| Email        | NVARCHAR(100) | User email            |
| FullName     | NVARCHAR(100) | User's full name      |
| CreatedAt    | DATETIME      | Account creation time |

### `LoginHistory`

| Column     | Type     | Description          |
| ---------- | -------- | -------------------- |
| HistoryID  | INT      | Primary Key          |
| UserID     | INT      | Foreign Key to Users |
| LoginTime  | DATETIME | Login time           |
| LogoutTime | DATETIME | Logout time          |

The database creation scripts are included in the repository.

## Project Structure

```text
Login and Register/
├── App.config
├── DatabaseHelper.cs
├── frmLogin.cs
├── frmLogin.Designer.cs
├── frmLogin.resx
├── frmRegister.cs
├── frmRegister.Designer.cs
├── frmRegister.resx
├── frmDashboard.cs
├── frmDashboard.Designer.cs
├── frmDashboard.resx
├── Program.cs
└── Login and Register.csproj
```

## Security

The project demonstrates two important security practices:

**1. Password Hashing**

Passwords are never stored as plain text. SHA-256 is used to create a hash before storing the password.

**2. SQL Injection Prevention**

All database queries use **parameterized SQL commands** instead of directly joining user input with SQL statements.

## How to Run

1. Open the project in **Visual Studio 2022**.
2. Make sure **SQL Server LocalDB** is installed.
3. Create the database `24-59241-3_LoginDB`.
4. Run the provided SQL script to create the required tables.
5. Check the connection string in `App.config`.
6. Build and run the project using **F5**.

## Bonus Features

The project includes:

* Login history with foreign key relationship
* Separate `DatabaseHelper` class for database operations
* Change password functionality
* SQL injection demonstration and prevention

## Author

**Rafi**
**Student ID:** 24-59241-3
