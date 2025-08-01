USE [IntelliDesktop]
GO
/****** Object:  Table [dbo].[Configs]    Script Date: 06/06/2015 14:37:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Configs](
	[ConfigSetName] [varchar](50) NOT NULL,
	[ParameterName] [varchar](50) NOT NULL,
	[Int1] [int] NULL,
	[Int2] [int] NULL,
	[Int3] [int] NULL,
	[Int4] [int] NULL,
	[Float1] [float] NULL,
	[Float2] [float] NULL,
	[Float3] [float] NULL,
	[Float4] [float] NULL,
	[Date1] [datetime] NULL,
	[Date2] [datetime] NULL,
	[Date3] [datetime] NULL,
	[Date4] [datetime] NULL,
	[Str1] [varchar](500) NULL,
	[Str2] [varchar](500) NULL,
	[Str3] [varchar](500) NULL,
	[Str4] [varchar](500) NULL,
	[LongStr] [varchar](1000) NULL,
 CONSTRAINT [PK_Configs] PRIMARY KEY CLUSTERED 
(
	[ConfigSetName] ASC,
	[ParameterName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Users]    Script Date: 06/06/2015 14:37:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[UserSettingId] [int] NOT NULL,
	[Email] [nvarchar](max) NOT NULL,
	[Settings_Data] [nvarchar](max) NOT NULL,
	[Drive] [nvarchar](max) NOT NULL,
	[IsBlocked] [bit] NOT NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[UserSettingId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Pages]    Script Date: 06/06/2015 14:37:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Pages](
	[ID_Page] [int] NOT NULL,
	[Title] [nvarchar](50) NOT NULL,
	[FullName] [nvarchar](50) NOT NULL,
	[URL] [varchar](50) NOT NULL,
	[AttributeURL] [varchar](50) NULL,
	[Tooltip] [nvarchar](50) NULL,
	[isActive] [bit] NOT NULL,
	[isPrinted] [bit] NOT NULL,
	[Ordered] [int] NULL,
	[LevelAgent] [int] NOT NULL,
	[ID_System] [varchar](2) NOT NULL,
 CONSTRAINT [PK_Pages] PRIMARY KEY CLUSTERED 
(
	[ID_Page] ASC,
	[Title] ASC,
	[FullName] ASC,
	[URL] ASC,
	[isActive] ASC,
	[isPrinted] ASC,
	[LevelAgent] ASC,
	[ID_System] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Layouts]    Script Date: 06/06/2015 14:37:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Layouts](
	[LayoutID] [decimal](6, 0) IDENTITY(1,1) NOT NULL,
	[LayoutName] [varchar](500) NULL,
	[LayoutType] [varchar](25) NULL,
	[LayoutContents] [varchar](500) NULL,
	[AccessType] [varchar](25) NULL,
	[LayoutVersion] [varchar](25) NULL,
	[Comment] [varchar](500) NULL,
	[SiteName] [varchar](25) NULL,
	[BuildingLevels] [varchar](1000) NULL,
	[ProcessName1] [varchar](25) NULL,
	[ProcessName2] [varchar](25) NULL,
	[ProcessName3] [varchar](25) NULL,
	[ProcessName4] [varchar](25) NULL,
	[CADFileName] [varchar](500) NULL,
	[CreatedBy] [int] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[ModifiedBy] [int] NOT NULL,
	[DateModified] [datetime] NULL,
	[LayoutState] [smallint] NULL,
	[FSA] [bit] NOT NULL,
	[ConfigSetName] [varchar](50) NULL,
	[Visible] [bit] NOT NULL,
	[TABFileName] [nvarchar](max) NULL,
	[Param1] [nchar](25) NULL,
	[Param2] [nchar](25) NULL,
	[Param3] [nchar](25) NULL,
	[Param4] [nchar](25) NULL,
 CONSTRAINT [PK_Layouts] PRIMARY KEY CLUSTERED 
(
	[LayoutID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[UserSettings]    Script Date: 06/06/2015 14:37:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserSettings](
	[UserSettingId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NULL,
	[ConfigSetName] [nvarchar](max) NOT NULL,
	[ChainDistance] [real] NOT NULL,
	[DateStarted] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsColorMode] [bit] NOT NULL,
	[LayoutExplorerRowSplitterPosition] [smallint] NOT NULL,
	[LayoutExplorerPGridColumnSplitterPosition] [smallint] NOT NULL,
	[Percent] [smallint] NOT NULL,
	[ProjectStatus] [nvarchar](max) NOT NULL,
	[ToggleLayoutDataTemplateSelector] [smallint] NOT NULL,
	[MinWidth] [int] NOT NULL,
	[LayoutId] [decimal](6, 0) NULL,
	[Drive] [nvarchar](max) NOT NULL,
	[ColorIndex] [int] NOT NULL,
	[Pos_X] [float] NOT NULL,
	[Pos_Y] [float] NOT NULL,
	[Pos_Z] [float] NOT NULL,
	[GeoPos] [geography] NOT NULL,
	[UserSettingUser_UserSetting_UserId] [int] NOT NULL,
	[UserSettingUser_UserSetting_UserSettingId] [int] NOT NULL,
	[Layout_LayoutID] [decimal](6, 0) NOT NULL,
 CONSTRAINT [PK_UserSettings] PRIMARY KEY CLUSTERED 
(
	[UserSettingId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Filters]    Script Date: 06/06/2015 14:37:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Filters](
	[FilterId] [int] IDENTITY(1,1) NOT NULL,
	[Active] [bit] NULL,
	[AccessType] [nvarchar](max) NOT NULL,
	[FilterName] [nvarchar](max) NOT NULL,
	[FSA] [bit] NOT NULL,
	[LayoutName] [nvarchar](max) NOT NULL,
	[LayoutType] [nvarchar](max) NOT NULL,
	[LayoutContents] [nvarchar](max) NOT NULL,
	[LayoutVersion] [nvarchar](max) NOT NULL,
	[Comment] [nvarchar](max) NOT NULL,
	[SiteName] [nvarchar](max) NOT NULL,
	[BuildingLevels] [nvarchar](max) NOT NULL,
	[CADFileName] [nvarchar](max) NOT NULL,
	[CreatedBy] [nvarchar](max) NOT NULL,
	[DateCreated] [datetime] NULL,
	[ModifiedBy] [nvarchar](max) NOT NULL,
	[DateModified] [tinyint] NULL,
	[LayoutState] [smallint] NULL,
	[LayoutId] [int] NOT NULL,
	[Layout_LayoutID] [decimal](6, 0) NOT NULL,
 CONSTRAINT [PK_Filters] PRIMARY KEY CLUSTERED 
(
	[FilterId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Bays]    Script Date: 06/06/2015 14:37:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Bays](
	[LayoutID] [decimal](6, 0) NOT NULL,
	[BayName] [varchar](50) NOT NULL,
	[BaySide] [varchar](50) NOT NULL,
	[BayPart] [varchar](50) NOT NULL,
	[Xmin] [float] NOT NULL,
	[Ymin] [float] NOT NULL,
	[Xmax] [float] NOT NULL,
	[Ymax] [float] NOT NULL,
	[BayId] [int] IDENTITY(1,1) NOT NULL,
	[Layout_LayoutID] [decimal](6, 0) NOT NULL,
 CONSTRAINT [PK_Bays] PRIMARY KEY CLUSTERED 
(
	[LayoutID] ASC,
	[BayId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Items]    Script Date: 06/06/2015 14:37:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Items](
	[ItemID] [decimal](15, 0) NULL,
	[ItemName] [varchar](100) NULL,
	[LayoutID] [decimal](6, 0) NOT NULL,
	[ItemIndex] [decimal](6, 0) NOT NULL,
	[ItemAttributeIndex] [decimal](6, 0) NOT NULL,
	[ItemAttributeName] [varchar](100) NULL,
	[ItemAttributeID] [decimal](22, 0) NULL,
	[BlockID] [decimal](15, 0) NULL,
	[BlockIndex] [decimal](6, 0) NOT NULL,
	[BlockName] [varchar](500) NOT NULL,
	[XrefName] [varchar](500) NULL,
	[LayerName] [varchar](500) NULL,
	[Xpos] [float] NULL,
	[Ypos] [float] NULL,
	[Zpos] [float] NULL,
	[Xscale] [float] NULL,
	[Yscale] [float] NULL,
	[Zscale] [float] NULL,
	[Rotation] [float] NULL,
	[ItemHandle] [varchar](25) NULL,
	[Layout_LayoutID] [decimal](6, 0) NOT NULL,
 CONSTRAINT [PK_Items] PRIMARY KEY CLUSTERED 
(
	[LayoutID] ASC,
	[ItemIndex] ASC,
	[ItemAttributeIndex] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[States]    Script Date: 06/06/2015 14:37:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[States](
	[As_made] [nvarchar](20) NOT NULL,
	[Latitude] [real] NOT NULL,
	[Longitude] [real] NOT NULL,
	[FileName] [nvarchar](50) NULL,
	[DateCreated] [datetime] NULL,
	[LayoutId] [nvarchar](max) NOT NULL,
	[StateId] [uniqueidentifier] NOT NULL,
	[Supplier] [nvarchar](max) NOT NULL,
	[CoordX] [real] NOT NULL,
	[CoordY] [real] NOT NULL,
	[Data_Search] [nvarchar](max) NOT NULL,
	[Status] [nvarchar](max) NOT NULL,
	[Layout_LayoutID] [decimal](6, 0) NOT NULL,
 CONSTRAINT [PK_States] PRIMARY KEY CLUSTERED 
(
	[As_made] ASC,
	[StateId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Rules]    Script Date: 06/06/2015 14:37:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Rules](
	[RuleId] [int] NOT NULL,
	[AttributePatternOn_Data] [nvarchar](max) NULL,
	[ColorIndex] [int] NOT NULL,
	[FilterBlockAttributesOn_Data] [nvarchar](max) NULL,
	[IncludeNested] [bit] NOT NULL,
	[isTypeFilterParent] [bit] NOT NULL,
	[LayerDestination] [nvarchar](max) NOT NULL,
	[LayerPatternOn_Data] [nvarchar](max) NULL,
	[TypeFilterOn_Data] [nvarchar](max) NULL,
	[LineType] [nvarchar](max) NOT NULL,
	[Comment] [nvarchar](max) NOT NULL,
	[LayerPatternOff_Data] [nvarchar](max) NULL,
	[LayoutId] [decimal](6, 0) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Position_X] [float] NOT NULL,
	[Position_Y] [float] NOT NULL,
	[Position_Z] [float] NOT NULL,
	[LayoutCatalogSite_Data] [nvarchar](max) NULL,
	[LayoutCatalogOptions_Data] [nvarchar](max) NULL,
	[TooNameAttributes_Data] [nvarchar](max) NULL,
	[Layout_LayoutID] [decimal](6, 0) NOT NULL,
 CONSTRAINT [PK_Rules] PRIMARY KEY CLUSTERED 
(
	[RuleId] ASC,
	[LayoutId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Blocks]    Script Date: 06/06/2015 14:37:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Blocks](
	[BlockID] [decimal](15, 0) NULL,
	[LayoutID] [decimal](6, 0) NOT NULL,
	[BlockIndex] [decimal](6, 0) NOT NULL,
	[BlockName] [varchar](500) NULL,
	[BlockXrefName] [varchar](500) NULL,
	[BlockHandle] [varchar](25) NULL,
	[Layout_LayoutID] [decimal](6, 0) NOT NULL,
 CONSTRAINT [PK_Blocks] PRIMARY KEY CLUSTERED 
(
	[LayoutID] ASC,
	[BlockIndex] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[BlockAttributes]    Script Date: 06/06/2015 14:37:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[BlockAttributes](
	[BlockID] [decimal](15, 0) NULL,
	[BlockAttributeID] [decimal](22, 0) NULL,
	[LayoutID] [decimal](6, 0) NOT NULL,
	[BlockIndex] [decimal](6, 0) NOT NULL,
	[BlockAttributeIndex] [decimal](6, 0) NOT NULL,
	[BlockAttributeName] [varchar](100) NULL,
	[BlockAttributeValue] [varchar](1000) NULL,
 CONSTRAINT [PK_BlockAttributes] PRIMARY KEY CLUSTERED 
(
	[LayoutID] ASC,
	[BlockIndex] ASC,
	[BlockAttributeIndex] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ItemAttributes]    Script Date: 06/06/2015 14:37:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ItemAttributes](
	[ItemID] [decimal](15, 0) NULL,
	[ItemAttributeID] [decimal](22, 0) NULL,
	[LayoutID] [decimal](6, 0) NOT NULL,
	[ItemIndex] [decimal](6, 0) NOT NULL,
	[ItemAttributeIndex] [decimal](6, 0) NOT NULL,
	[ItemAttributeName] [varchar](100) NOT NULL,
	[ItemAttributeValue] [varchar](1000) NULL,
	[Item_LayoutID] [decimal](6, 0) NOT NULL,
	[Item_ItemIndex] [decimal](6, 0) NOT NULL,
	[Item_ItemAttributeIndex] [decimal](6, 0) NOT NULL,
 CONSTRAINT [PK_ItemAttributes] PRIMARY KEY CLUSTERED 
(
	[LayoutID] ASC,
	[ItemIndex] ASC,
	[ItemAttributeIndex] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Frames]    Script Date: 06/06/2015 14:37:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Frames](
	[BlockID] [decimal](15, 0) NULL,
	[FrameID] [decimal](22, 0) NULL,
	[LayoutID] [decimal](6, 0) NOT NULL,
	[BlockIndex] [decimal](6, 0) NOT NULL,
	[FrameIndex] [decimal](6, 0) NOT NULL,
	[FrameTypeID] [smallint] NOT NULL,
	[Xmin] [float] NOT NULL,
	[Ymin] [float] NOT NULL,
	[Xmax] [float] NOT NULL,
	[Ymax] [float] NOT NULL,
 CONSTRAINT [PK_Frames] PRIMARY KEY CLUSTERED 
(
	[LayoutID] ASC,
	[BlockIndex] ASC,
	[FrameIndex] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Default [DF_Users_IsBlocked]    Script Date: 06/06/2015 14:37:22 ******/
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_IsBlocked]  DEFAULT ((0)) FOR [IsBlocked]
GO
/****** Object:  ForeignKey [FK_LayoutBay]    Script Date: 06/06/2015 14:37:22 ******/
ALTER TABLE [dbo].[Bays]  WITH CHECK ADD  CONSTRAINT [FK_LayoutBay] FOREIGN KEY([Layout_LayoutID])
REFERENCES [dbo].[Layouts] ([LayoutID])
GO
ALTER TABLE [dbo].[Bays] CHECK CONSTRAINT [FK_LayoutBay]
GO
/****** Object:  ForeignKey [FK_BlockAttributes_Blocks]    Script Date: 06/06/2015 14:37:22 ******/
ALTER TABLE [dbo].[BlockAttributes]  WITH CHECK ADD  CONSTRAINT [FK_BlockAttributes_Blocks] FOREIGN KEY([LayoutID], [BlockIndex])
REFERENCES [dbo].[Blocks] ([LayoutID], [BlockIndex])
GO
ALTER TABLE [dbo].[BlockAttributes] CHECK CONSTRAINT [FK_BlockAttributes_Blocks]
GO
/****** Object:  ForeignKey [FK_LayoutBlock]    Script Date: 06/06/2015 14:37:22 ******/
ALTER TABLE [dbo].[Blocks]  WITH CHECK ADD  CONSTRAINT [FK_LayoutBlock] FOREIGN KEY([Layout_LayoutID])
REFERENCES [dbo].[Layouts] ([LayoutID])
GO
ALTER TABLE [dbo].[Blocks] CHECK CONSTRAINT [FK_LayoutBlock]
GO
/****** Object:  ForeignKey [FK_LayoutFilter]    Script Date: 06/06/2015 14:37:22 ******/
ALTER TABLE [dbo].[Filters]  WITH CHECK ADD  CONSTRAINT [FK_LayoutFilter] FOREIGN KEY([Layout_LayoutID])
REFERENCES [dbo].[Layouts] ([LayoutID])
GO
ALTER TABLE [dbo].[Filters] CHECK CONSTRAINT [FK_LayoutFilter]
GO
/****** Object:  ForeignKey [FK_Frames_Blocks]    Script Date: 06/06/2015 14:37:22 ******/
ALTER TABLE [dbo].[Frames]  WITH CHECK ADD  CONSTRAINT [FK_Frames_Blocks] FOREIGN KEY([LayoutID], [BlockIndex])
REFERENCES [dbo].[Blocks] ([LayoutID], [BlockIndex])
GO
ALTER TABLE [dbo].[Frames] CHECK CONSTRAINT [FK_Frames_Blocks]
GO
/****** Object:  ForeignKey [FK_ItemItemAttribute]    Script Date: 06/06/2015 14:37:22 ******/
ALTER TABLE [dbo].[ItemAttributes]  WITH CHECK ADD  CONSTRAINT [FK_ItemItemAttribute] FOREIGN KEY([Item_LayoutID], [Item_ItemIndex], [Item_ItemAttributeIndex])
REFERENCES [dbo].[Items] ([LayoutID], [ItemIndex], [ItemAttributeIndex])
GO
ALTER TABLE [dbo].[ItemAttributes] CHECK CONSTRAINT [FK_ItemItemAttribute]
GO
/****** Object:  ForeignKey [FK_LayoutItem]    Script Date: 06/06/2015 14:37:22 ******/
ALTER TABLE [dbo].[Items]  WITH CHECK ADD  CONSTRAINT [FK_LayoutItem] FOREIGN KEY([Layout_LayoutID])
REFERENCES [dbo].[Layouts] ([LayoutID])
GO
ALTER TABLE [dbo].[Items] CHECK CONSTRAINT [FK_LayoutItem]
GO
/****** Object:  ForeignKey [FK_LayoutRule]    Script Date: 06/06/2015 14:37:22 ******/
ALTER TABLE [dbo].[Rules]  WITH CHECK ADD  CONSTRAINT [FK_LayoutRule] FOREIGN KEY([Layout_LayoutID])
REFERENCES [dbo].[Layouts] ([LayoutID])
GO
ALTER TABLE [dbo].[Rules] CHECK CONSTRAINT [FK_LayoutRule]
GO
/****** Object:  ForeignKey [FK_LayoutState]    Script Date: 06/06/2015 14:37:22 ******/
ALTER TABLE [dbo].[States]  WITH CHECK ADD  CONSTRAINT [FK_LayoutState] FOREIGN KEY([Layout_LayoutID])
REFERENCES [dbo].[Layouts] ([LayoutID])
GO
ALTER TABLE [dbo].[States] CHECK CONSTRAINT [FK_LayoutState]
GO
/****** Object:  ForeignKey [FK_LayoutUserSetting]    Script Date: 06/06/2015 14:37:22 ******/
ALTER TABLE [dbo].[UserSettings]  WITH CHECK ADD  CONSTRAINT [FK_LayoutUserSetting] FOREIGN KEY([Layout_LayoutID])
REFERENCES [dbo].[Layouts] ([LayoutID])
GO
ALTER TABLE [dbo].[UserSettings] CHECK CONSTRAINT [FK_LayoutUserSetting]
GO
/****** Object:  ForeignKey [FK_UserSettingUser]    Script Date: 06/06/2015 14:37:22 ******/
ALTER TABLE [dbo].[UserSettings]  WITH CHECK ADD  CONSTRAINT [FK_UserSettingUser] FOREIGN KEY([UserSettingUser_UserSetting_UserId], [UserSettingUser_UserSetting_UserSettingId])
REFERENCES [dbo].[Users] ([UserId], [UserSettingId])
GO
ALTER TABLE [dbo].[UserSettings] CHECK CONSTRAINT [FK_UserSettingUser]
GO
