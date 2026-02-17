# SSL Certificate Tracker

A robust .NET 8 MVC application designed to track SSL certificate expiration dates and send automated email notifications to ensure your domains stay secure.

## 🚀 Overview

The **SSL Certificate Tracker** is a management tool for IT administrators and developers to keep track of multiple SSL certificates. It features a background processing engine (Hangfire) that checks for expiring certificates daily and notifies users via email before their certificates lapse.

### Key Features

- **Windows Authentication**: Seamless, zero-password login using organizational Windows accounts.
- **Strict Access Control**: Only users with an **Active** status and a valid **IDG Number** in the system can access the application.
- **User Management Portal**: Dedicated administrative interface to manage system users, toggle access, and assign admin roles.
- **Certificate Management**: Comprehensive tracking of SSL domains, including issuance and expiry dates.
- **Automated Notifications**: Background engine checks daily for expiring certificates and alerts assigned users.
- **Hangfire Dashboard**: Secure monitoring of background tasks (Admin only).

---

## 🔐 Authentication & Access Control

The system uses a hybrid authentication model:

1.  **Identity**: Windows Authentication identifies the user (DOMAIN\Username).
2.  **Authorization**: The system extracts the **IDG Number** (alphanumeric ID) and looks it up in the internal database.
3.  **Status Check**: Even if a user is found, they must be marked as **Active** to enter. Inactive users are redirected to an "Access Denied" page.

### Admin Privileges

Users marked as **Admin** in the database have access to:

- **User Management**: Add, edit, or disable any user in the system.
- **Hangfire Dashboard**: View and manually trigger certificate check jobs.

---

## 💻 Setup & Management

### 1. Initial Admin Provisioning

To get started, the first user must be manually added to the database.

1.  Identify your Windows Username and IDG Number (usually your alphanumeric login).
2.  Use the provided `insert_user.sql` script or run the following:
    ```sql
    INSERT INTO Users (Id, IdgNumber, Name, UserName, Email, IsActive, IsAdmin, CreatedAt)
    VALUES (NEWID(), 'YOUR_IDG', 'Your Name', 'DOMAIN\User', 'email@org.com', 1, 1, GETUTCDATE());
    ```

### 2. User Management Portal

Once the first admin is in, they can navigate to **"Manage Users"** in the top navigation bar to:

- **Invite New Users**: Register them by their IDG Number.
- **Suspend Access**: Toggle a user to "Inactive" to block their access immediately without deleting their data.
- **Promote Admins**: Grant administrative rights to other team members.

### 3. Background Jobs (Hangfire)

The system checks for expiring certificates once a day.

- **Monitoring**: Admins can visit `/hangfire` to see job history.
- **Infrastructure**: For production (IIS), ensure the Application Pool "Start Mode" is set to `AlwaysRunning` to prevent the background timer from pausing.

---

## 🏠 Local Configuration

### Update `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=...;Database=SslCertificateTracker;Trusted_Connection=True;"
},
"EmailSettings": {
  "SmtpServer": "smtp.googlemail.com",
  "SmtpPort": 465,
  "SenderEmail": "tjamrasa8@gmail.com",
  "SmtpUsername": "tjamrasa8@gmail.com",
  "SmtpPassword": "your-app-password"
}
```

---

## 📋 Developer Notes

- **Resilient Lookup**: The login filter checks for users using multiple variations (IDG Number, Full Windows Name, etc.) to ensure reliable access.
- **Notifications**: Email alerts are sent based on the "Days Before Expiry" setting in each user's profile (default 30 days).
- **SMTP**: Currently configured for Gmail. For corporate deployment, update the `SmtpServer` to your organization's internal relay.
