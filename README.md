# SSL Certificate Tracker

A robust .NET 8 MVC application designed to track SSL certificate expiration dates and send automated email notifications to ensure your domains stay secure.

## 🚀 Overview

The **SSL Certificate Tracker** is a management tool for IT administrators and developers to keep track of multiple SSL certificates. It features a background processing engine (Hangfire) that checks for expiring certificates daily and notifies users via email before their certificates lapse.

### Key Features

- **User Authentication**: Secure login and registration using ASP.NET Core Identity.
- **Certificate Management**: Add, view, edit, and delete SSL certificate details (Domain, Issue Date, Expiry Date).
- **Automated Notifications**: Background jobs check for certificates expiring within a user-defined threshold (default 30 days).
- **Email Alerts**: Integration with Gmail/SMTP for reliable distribution of alerts.
- **Hangfire Dashboard**: Real-time monitoring of background tasks and scheduled jobs.

---

## 🛠 Tech Stack

- **Framework**: .NET 8.0 (ASP.NET Core MVC)
- **Database**: Microsoft SQL Server / Azure SQL Database
- **Background Processing**: Hangfire
- **Email Service**: MailKit / MimeKit
- **Authentication**: ASP.NET Core Identity

---

## 💻 Local Configuration

### 1. Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express or LocalDB)
- SMTP Account (e.g., Gmail with App Password)

### 2. Setup

1. Clone the repository.
2. Update `appsettings.json` with your sensitive data:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=...;Database=SslCertificateTracker;Trusted_Connection=True;"
   },
   "EmailSettings": {
     "SmtpServer": "smtp.googlemail.com",
     "SmtpPort": 465,
     "SenderEmail": "your-email@gmail.com",
     "SmtpUsername": "your-email@gmail.com",
     "SmtpPassword": "your-app-password"
   }
   ```
3. Apply Migrations:
   ```bash
   dotnet ef database update
   ```
4. Run the application:
   ```bash
   dotnet run
   ```

---

## ☁️ Azure Deployment Guide

This guide assumes you are deploying to **Azure App Service** and **Azure SQL Database**.

### 1. Database Setup

1. Create an **Azure SQL Database** in the Azure Portal.
2. In the "Networking" settings of the SQL Server, ensure "Allow Azure services and resources to access this server" is enabled.
3. Copy the Connection String.

### 2. App Service Setup

1. Create a new **Web App** (Runtime stack: .NET 8).
2. Go to **Settings > Configuration** (or Environment Variables).
3. Add the following keys (Azure uses `__` instead of `:` for nested keys in Environment Variables):
   - `ConnectionStrings__DefaultConnection`: _Your Azure SQL Connection String_
   - `EmailSettings__SmtpServer`: `smtp.googlemail.com`
   - `EmailSettings__SmtpPort`: `465`
   - `EmailSettings__SenderEmail`: _Your Sender Address_
   - `EmailSettings__SmtpUsername`: _Your SMTP Username_
   - `EmailSettings__SmtpPassword`: _Your App Password_

### 3. Identity & Migrations in Azure

If you are using GitHub Actions for deployment, you can run migrations during the build process. Alternatively, you can use the "Apply Migrations" feature if your CI/CD supports it, or temporarily enable "apply migrations on startup" (not recommended for production).

### 4. Background Jobs (Hangfire)

Hangfire requires the database to be reachable. Once the App Service starts, it will automatically initialize the Hangfire tables in your Azure SQL Database.

> **Note**: For Azure App Service, ensure **"Always On"** is enabled (available in Basic tier and above) to keep the background worker running continuously.

---

## 📊 Usage Guide

1. **Dashboard**: Upon login, view your list of tracked domains and their expiry status.
2. **Add Certificate**: Enter the domain name, issue date, and expiration date.
3. **Profile Settings**: Configure your notification email and set how many days before expiry you wish to be notified.
4. **Hangfire Dashboard**: Visit `/hangfire` (Admin access only) to view past and upcoming automated checks.

---

## 👤 Developer Notes

- **SMTP Issues**: If you encounter `SocketException (11001)`, ensure the `SmtpServer` hostname is correct and reachable. The app is currently configured to use `smtp.googlemail.com`.
- **Contribution**: PRs are welcome! Please ensure you update the `appsettings.Example.json` if you introduce new configuration keys.
