24-59241-3_LoginSystem
Student ID: 24-59241-3
Student Name: Rafi
Course: Lab 1 - Login, Registration & Logout with C# and SQL Server
Table of Contents
Environment
Database Setup
How the Application Works
Password Hashing
SQL Injection Demo
Bonus Tasks
Screenshots
Problems Faced & Solutions
Sample Project Bugs Analysis
Environment
Table
Component	Version
IDE	Visual Studio 2022 Community
.NET Framework	4.7.2
SQL Server	SQL Server 2022 Express / LocalDB
Language	C#
Connection String Format
The connection string is stored in App.config (NOT hard-coded in any form):
xml
<connectionStrings>
  <add name="LoginDB" 
       connectionString="Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=24-59241-3_LoginDB;Integrated Security=True;Connect Timeout=30" 
       providerName="System.Data.SqlClient"/>
</connectionStrings>
Note: Integrated Security=True means Windows Authentication is used — no password is stored in the connection string.
Database Setup
How I Created the Database
Opened SQL Server Object Explorer in Visual Studio (View → SQL Server Object Explorer).
Connected to my local SQL Server instance: (localdb)\MSSQLLocalDB.
Right-clicked Databases → Add New Database → named it 24-59241-3_LoginDB.
Opened a New Query window and executed the script from Schema.sql.
Schema.sql Explanation
The script creates two tables:
dbo.Users
Table
Column	Type	Constraints
UserID	INT IDENTITY(1,1)	PRIMARY KEY
Username	NVARCHAR(50)	NOT NULL, UNIQUE
PasswordHash	NVARCHAR(200)	NOT NULL
Email	NVARCHAR(100)	
FullName	NVARCHAR(100)	
CreatedAt	DATETIME	DEFAULT GETDATE()
dbo.LoginHistory (Bonus Task)
Table
Column	Type	Constraints
HistoryID	INT IDENTITY(1,1)	PRIMARY KEY
UserID	INT	FOREIGN KEY → Users(UserID)
LoginTime	DATETIME	DEFAULT GETDATE()
LogoutTime	DATETIME	NULL
The LoginHistory table tracks when users log in and out, with a foreign key linking each record to the Users table.
How the Application Works
Project Structure
plain
24-59241-3_LoginSystem/
├── App.config              # Connection string storage
├── Program.cs              # Application entry point
├── DatabaseHelper.cs       # ALL database operations (Bonus)
├── LoginForm.cs            # Login screen
├── RegistrationForm.cs     # Registration screen
├── HomeForm.cs             # Dashboard after login
└── Schema.sql              # Database creation script
Registration Flow
User clicks Register on the LoginForm.
RegistrationForm opens as a dialog.
User fills: Username, Password, Confirm Password, Email, Full Name.
Validation (all in btnRegister_Click):
No empty fields allowed.
Password must be ≥ 6 characters.
Password and Confirm Password must match.
Email must contain @.
Username uniqueness check: DatabaseHelper.UsernameExists() uses ExecuteScalar() with a COUNT(*) query to check if the username is already taken.
If valid and unique, DatabaseHelper.RegisterUser() inserts the record using a parameterized query with ExecuteNonQuery().
The password is hashed with SHA-256 before storage.
On success: message box → form cleared → return to LoginForm.
Login Flow
User enters username and password on LoginForm.
btnLogin_Click validates that fields are not empty.
DatabaseHelper.ValidateLogin() runs a parameterized query using SqlDataReader.
The stored hash is compared with the hash of the entered password.
If successful:
DatabaseHelper.RecordLogin() inserts a row into LoginHistory and returns the HistoryID.
HomeForm is opened, receiving the UserInfo object and HistoryID.
LoginForm is hidden (not closed, to allow return on logout).
If failed:
Failed attempt counter increments.
After 3 failed attempts, the Login button is disabled.
Friendly message shows remaining attempts.
Logout Flow
User clicks Logout on HomeForm.
DatabaseHelper.RecordLogout() updates the LogoutTime in LoginHistory using the stored HistoryID.
HomeForm closes.
The FormClosed event on HomeForm (set in LoginForm) shows the LoginForm again.
LoginForm.ClearForm() empties all textboxes and resets focus to username.
The application does NOT exit — it returns cleanly to the login screen.
Password Hashing
Why Plain Text is Unacceptable
Storing passwords in plain text is a critical security risk. If the database is compromised, attackers immediately gain access to all user credentials. Since many users reuse passwords across sites, this can lead to widespread account breaches.
How I Hashed Passwords
I used SHA-256 (Secure Hash Algorithm 256-bit) via System.Security.Cryptography.SHA256:
csharp
public static string HashPassword(string password)
{
    using (SHA256 sha256 = SHA256.Create())
    {
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2"));
        }
        return builder.ToString();
    }
}
How It Works
The plain-text password is converted to a byte array using UTF-8 encoding.
SHA-256 computes a fixed 256-bit (32-byte) hash value.
The hash bytes are converted to a hexadecimal string (64 characters).
Only the hash is stored in the database. The real password is never saved.
At login, the entered password is hashed again and compared to the stored hash.
Note: While SHA-256 is the minimum required for this lab, production systems should use slower algorithms like bcrypt, PBKDF2, or Argon2 with salting to resist brute-force attacks.
SQL Injection Demo
The Vulnerable Code (String Concatenation)
csharp
// DANGEROUS - DO NOT USE
string query = "SELECT * FROM Users WHERE Username = '" + txtUsername.Text + 
               "' AND PasswordHash = '" + txtPassword.Text + "'";
SqlCommand cmd = new SqlCommand(query, con);
The Exploit Input
Enter the following in the password field:
plain
' OR '1'='1
What Happens
The concatenated SQL becomes:
sql
SELECT * FROM Users WHERE Username = 'admin' AND PasswordHash = '' OR '1'='1'
Because '1'='1' is always true, the WHERE clause evaluates to true for every row in the table. The query returns all users, the code checks reader.Read() and finds rows, so the attacker is logged in without knowing any password.
The Fixed Code (Parameterized Query)
csharp
// SAFE - Uses parameters
string query = "SELECT UserID, Username, PasswordHash, FullName FROM Users WHERE Username = @Username";
using (SqlCommand cmd = new SqlCommand(query, con))
{
    cmd.Parameters.AddWithValue("@Username", username);
    con.Open();
    using (SqlDataReader reader = cmd.ExecuteReader())
    {
        // ... compare hashes
    }
}
Why Parameters Stop the Attack
With parameterized queries, the value travels to the SQL Server separately from the SQL command text. The input ' OR '1'='1 is treated as a literal string value for the @Username parameter, not as executable SQL code. The server never parses the input as part of the query structure. Therefore, the malicious input is simply compared against usernames in the database — and since no user has that name, login fails safely.
Bonus Tasks
I attempted TWO bonus tasks:
1. LoginHistory Table with Foreign Key
Created dbo.LoginHistory with HistoryID, UserID (FK), LoginTime, and LogoutTime.
On successful login, DatabaseHelper.RecordLogin() inserts a row and returns the HistoryID.
On logout, DatabaseHelper.RecordLogout() updates the LogoutTime for that specific record.
The HomeForm displays the current user's login history in a DataGridView.
2. DatabaseHelper Class
All database code was moved out of the forms into DatabaseHelper.cs.
Forms never create SqlConnection or SqlCommand directly.
DatabaseHelper provides static methods: TestConnection(), RegisterUser(), ValidateLogin(), GetAllUsers(), RecordLogin(), RecordLogout(), GetLoginHistory(), ChangePassword().
This makes the code cleaner, reusable, and easier to maintain.
3. Change Password Screen (Extra)
Added a Change Password section on HomeForm.
Verifies the old password first by comparing hashes.
Validates new password length and confirmation match.
Updates the hash in the database using a parameterized query.
Screenshots
The following screenshots should be captured and included in your Report.pdf:
Table Design: SSMS / SQL Server Object Explorer showing dbo.Users columns.
Registration Form: Filled with sample data before clicking Register.
Successful Registration: Message box confirming success.
Successful Login: Message box showing "Welcome, [FullName]!"
Failed Login: Error message after wrong credentials.
Home Screen: Showing lblWelcome, dgvUsers grid, and dgvLoginHistory.
Logout: LoginForm reappearing with cleared fields.
SQL Injection - Before: Vulnerable code + bypass with ' OR '1'='1.
SQL Injection - After: Same input failing with parameterized query.
Problems Faced & Solutions
Problem 1: "ConfigurationManager does not exist"
Solution: Added a reference to System.Configuration via Project → Add Reference → Assemblies → System.Configuration.
Problem 2: "SqlConnection could not be found"
Solution: Added using System.Data.SqlClient; at the top of the file.
Problem 3: Connection string not found at runtime
Solution: Ensured the connection string name in App.config exactly matches the name used in ConfigurationManager.ConnectionStrings["LoginDB"].
Problem 4: Form not returning to LoginForm after logout
Solution: Used FormClosed event on HomeForm to show LoginForm again, rather than closing the application. Also used this.Hide() instead of this.Close() when opening HomeForm.
Problem 5: LoginHistory LogoutTime not updating
Solution: Stored the HistoryID returned by RecordLogin() in a field on HomeForm, then passed it to RecordLogout() on logout.
Sample Project Bugs Analysis
The sample project Login_System contained the following deliberate bugs. I analyzed and understood each one:
Table
#	Bug	Why It Is Wrong	How I Fixed It
1	SQL Injection — query built by string concatenation	Attackers can inject malicious SQL like ' OR '1'='1	Used parameterized queries with @parameters everywhere
2	Two different connection strings — Initial Catalog differed between methods	One method connects to wrong database, causing runtime errors	Stored one connection string in App.config, read via ConfigurationManager
3	Table name mismatch — code queries LoginMst, but script creates Table	The app crashes with "Invalid object name"	Created dbo.Users and referenced it consistently
4	Connection opened in Form1_Load but never used or closed	Wastes resources and leaks connections	Only open connections inside using blocks where needed
5	No try/catch — app crashes if SQL Server is unreachable	Bad user experience; unhandled exceptions	Wrapped all DB calls in try/catch with friendly error messages
6	con.Close() not in finally/using	If an exception throws, Close() is skipped, leaking connections	Used using statements which call Dispose() automatically
7	Passwords stored and compared in plain text	Anyone with DB access sees all passwords	Stored SHA-256 hashes only; compare hashes at login
8	Missing space in concatenated SQL — produces 'x'and	Syntax error at runtime	Used parameterized queries — no string concatenation
9	On success opens a website instead of app's own home screen	Breaks application flow and user expectation	Opened HomeForm showing welcome message and user grid
10	Controls named button1, textBox1, label3	Unreadable and unmaintainable	Used meaningful names: btnLogin, txtUsername, lblTitle
11	No registration form and no logout	Incomplete functionality despite the project name	Built full RegistrationForm and Logout functionality
How to Run This Project
Clone the repository or extract the files.
Open SQL Server and run Schema.sql to create the database and tables.
Open Visual Studio and create a new Windows Forms App (.NET Framework 4.7.2).
Add all .cs files to the project (right-click Project → Add → Existing Item).
Add Reference to System.Configuration (Project → Add Reference → Assemblies).
Update App.config with your actual SQL Server instance name if different.
Build and Run (F5).
Author
Rafi — Student ID: 24-59241-3
