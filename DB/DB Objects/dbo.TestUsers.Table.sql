USE [intercepteverywhere]
GO
/****** Object:  Table [dbo].[TestUsers]    Script Date: 12/21/2016 6:01:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TestUsers](
	[TestUserID] [int] IDENTITY(1,1) NOT NULL,
	[UserGUID] [uniqueidentifier] NOT NULL,
	[FaceBookID] [nvarchar](max) NULL,
	[EmailID] [nvarchar](max) NULL,
	[Password] [nvarchar](max) NULL,
	[Name] [nvarchar](max) NULL,
	[IsDeleted] [bit] NOT NULL,
	[IsLinked] [bit] NOT NULL,
	[ModifiedDate] [datetime2](7) NULL,
 CONSTRAINT [PK_TestUsers] PRIMARY KEY CLUSTERED 
(
	[TestUserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
ALTER TABLE [dbo].[TestUsers]  WITH CHECK ADD  CONSTRAINT [FK_Users_TestUsers] FOREIGN KEY([UserGUID])
REFERENCES [dbo].[Users] ([UserGUID])
GO
ALTER TABLE [dbo].[TestUsers] CHECK CONSTRAINT [FK_Users_TestUsers]
GO
