USE [intercepteverywhere]
GO
/****** Object:  Table [dbo].[Apps]    Script Date: 12/21/2016 6:01:34 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Apps](
	[AppGUID] [uniqueidentifier] NOT NULL,
	[DeviceID] [uniqueidentifier] NOT NULL,
	[AppName] [nvarchar](50) NOT NULL,
	[OS] [nvarchar](50) NOT NULL,
	[Version] [nvarchar](50) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[ModifiedBy] [nvarchar](50) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[RegistrationToken] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_Apps] PRIMARY KEY CLUSTERED 
(
	[AppGUID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
