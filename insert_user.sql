-- Example insert for the initial Admin user
-- NOTE: 'IdgNumber' is the alphanumeric ID (e.g., from 'whoami' after the backslash)
-- NOTE: 'UserName' is the full DOMAIN\User string

INSERT INTO Users (Id, IdgNumber, Name, UserName, Email, IsActive, IsAdmin, CreatedAt)
VALUES (
    NEWID(), 
    'user',                     -- IDG Number (alphanumeric ID)
    'Tejiri Amrasa',            -- Full Name
    'desktop-uotjeai\user',      -- Windows Username (from whoami)
    'tjthecreator8@gmail.com',  -- Registration Email
    1,                          -- IsActive (1 = Yes)
    1,                          -- IsAdmin (1 = Yes)
    GETUTCDATE()
);

-- Also insert initial settings
INSERT INTO UserSettings (UserId, NotificationEmail, EnableEmailNotifications, DaysBeforeExpiryToNotify)
SELECT Id, Email, 1, 30 FROM Users WHERE UserName = 'desktop-uotjeai\user';
