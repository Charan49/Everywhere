ALTER TABLE UserServices
ADD LongToken Nvarchar(MAX)

ALTER TABLE users ADD MobileNumber NVARCHAR(50)

sp_rename 'users.ConfirmationCode', 'MobileConfirmationCode', 'COLUMN';

ALTER TABLE users ADD EmailVerificationCode NVARCHAR(20)