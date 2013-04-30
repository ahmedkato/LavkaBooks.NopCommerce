USE [aquiladev_lavkabooks_23.03.13]
GO

/****** Object:  Table [dbo].[Free]    Script Date: 01.05.2013 1:25:02 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Free](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProductId] [int] NOT NULL,
	[Title] [nvarchar](100) NOT NULL,
	[Descriptor] [nvarchar](max) NULL,
	[StartOnUtc] [datetime] NOT NULL,
	[EndOnUtc] [datetime] NOT NULL,
	[Amount] [int] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

