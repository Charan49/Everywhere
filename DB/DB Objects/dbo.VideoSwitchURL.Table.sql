USE [intercepteverywhere]
GO
/****** Object:  Table [dbo].[VideoSwitchURL]    Script Date: 12/21/2016 6:01:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VideoSwitchURL](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[VideoGUID] [uniqueidentifier] NOT NULL,
	[StreamURL] [nvarchar](250) NULL,
	[IsDeleted] [bit] NOT NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[ModifiedBy] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NULL,
	[ModifiedDate] [datetime] NULL,
 CONSTRAINT [PK_VideoSwitchURL] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
