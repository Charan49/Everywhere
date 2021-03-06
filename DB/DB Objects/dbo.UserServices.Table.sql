USE [intercepteverywhere]
GO
/****** Object:  Table [dbo].[UserServices]    Script Date: 12/21/2016 6:01:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserServices](
	[ServiceGUID] [uniqueidentifier] NOT NULL,
	[UserGUID] [uniqueidentifier] NOT NULL,
	[AccessID] [uniqueidentifier] NOT NULL,
	[AccessToken] [nvarchar](max) NULL,
	[TokenExpiration] [nvarchar](50) NULL,
	[IsDeleted] [bit] NOT NULL,
	[CreatedBy] [nvarchar](50) NOT NULL,
	[ModifiedBy] [nvarchar](50) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedDate] [datetime2](7) NULL,
	[StreamID] [nvarchar](max) NULL,
	[StreamURL] [nvarchar](max) NULL,
	[StreamKey] [nvarchar](max) NULL,
	[StreamDate] [datetime2](7) NULL,
	[PictureURL] [nvarchar](max) NULL,
	[fbUserID] [nvarchar](100) NULL,
	[LongToken] [nvarchar](max) NULL,
 CONSTRAINT [PK_UserServices] PRIMARY KEY CLUSTERED 
(
	[AccessID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
ALTER TABLE [dbo].[UserServices]  WITH CHECK ADD  CONSTRAINT [FK_Services_UserServices] FOREIGN KEY([ServiceGUID])
REFERENCES [dbo].[Services] ([ServiceGUID])
GO
ALTER TABLE [dbo].[UserServices] CHECK CONSTRAINT [FK_Services_UserServices]
GO
ALTER TABLE [dbo].[UserServices]  WITH CHECK ADD  CONSTRAINT [FK_Users_UserServices] FOREIGN KEY([UserGUID])
REFERENCES [dbo].[Users] ([UserGUID])
GO
ALTER TABLE [dbo].[UserServices] CHECK CONSTRAINT [FK_Users_UserServices]
GO
