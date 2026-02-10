INSERT INTO Users (Id, UserName, FullName, Email, CreatedAt)
VALUES (
    NEWID(), 
    'desktop-uotjeai\user', 
    'Tejiri Amrasa', 
    'tjthecreator8@gmail.com', 
    GETUTCDATE()
);

-- Note: The username must match exactly what Windows reports.
-- In your environment, 'whoami' reported: desktop-uotjeai\user
-- Ensure your existing record matches this casing and format exactly.
