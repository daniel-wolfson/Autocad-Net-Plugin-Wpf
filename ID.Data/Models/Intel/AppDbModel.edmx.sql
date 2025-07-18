
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 06/05/2015 22:06:58
-- Generated from EDMX file: C:\Projects\ID\ID.AcadNet.Data\Models\AcadNetModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [AcadNet];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_BlockAttributes_Blocks]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[BlockAttributes] DROP CONSTRAINT [FK_BlockAttributes_Blocks];
GO
IF OBJECT_ID(N'[dbo].[FK_Frames_Blocks]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Frames] DROP CONSTRAINT [FK_Frames_Blocks];
GO
IF OBJECT_ID(N'[dbo].[FK_LayoutState]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[States] DROP CONSTRAINT [FK_LayoutState];
GO
IF OBJECT_ID(N'[dbo].[FK_LayoutBlock]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Blocks] DROP CONSTRAINT [FK_LayoutBlock];
GO
IF OBJECT_ID(N'[dbo].[FK_LayoutItem]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Items] DROP CONSTRAINT [FK_LayoutItem];
GO
IF OBJECT_ID(N'[dbo].[FK_ItemItemAttribute]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ItemAttributes] DROP CONSTRAINT [FK_ItemItemAttribute];
GO
IF OBJECT_ID(N'[dbo].[FK_LayoutFilter]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Filters] DROP CONSTRAINT [FK_LayoutFilter];
GO
IF OBJECT_ID(N'[dbo].[FK_LayoutBay]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Bays] DROP CONSTRAINT [FK_LayoutBay];
GO
IF OBJECT_ID(N'[dbo].[FK_UserSettingUser]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserSettings] DROP CONSTRAINT [FK_UserSettingUser];
GO
IF OBJECT_ID(N'[dbo].[FK_LayoutUserSetting]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserSettings] DROP CONSTRAINT [FK_LayoutUserSetting];
GO
IF OBJECT_ID(N'[dbo].[FK_LayoutRule]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Rules] DROP CONSTRAINT [FK_LayoutRule];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[States]', 'U') IS NOT NULL
    DROP TABLE [dbo].[States];
GO
IF OBJECT_ID(N'[dbo].[Configs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Configs];
GO
IF OBJECT_ID(N'[dbo].[Bays]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Bays];
GO
IF OBJECT_ID(N'[dbo].[BlockAttributes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[BlockAttributes];
GO
IF OBJECT_ID(N'[dbo].[Blocks]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Blocks];
GO
IF OBJECT_ID(N'[dbo].[Frames]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Frames];
GO
IF OBJECT_ID(N'[dbo].[ItemAttributes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ItemAttributes];
GO
IF OBJECT_ID(N'[dbo].[Items]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Items];
GO
IF OBJECT_ID(N'[dbo].[Layouts]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Layouts];
GO
IF OBJECT_ID(N'[dbo].[Users]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Users];
GO
IF OBJECT_ID(N'[dbo].[Rules]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Rules];
GO
IF OBJECT_ID(N'[dbo].[UserSettings]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserSettings];
GO
IF OBJECT_ID(N'[dbo].[Filters]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Filters];
GO
IF OBJECT_ID(N'[dbo].[Pages]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Pages];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'States'
CREATE TABLE [dbo].[States] (
    [As_made] nvarchar(20)  NOT NULL,
    [Latitude] real  NOT NULL,
    [Longitude] real  NOT NULL,
    [FileName] nvarchar(50)  NULL,
    [DateCreated] datetime  NULL,
    [LayoutId] nvarchar(max)  NOT NULL,
    [StateId] uniqueidentifier  NOT NULL,
    [Supplier] nvarchar(max)  NOT NULL,
    [CoordX] real  NOT NULL,
    [CoordY] real  NOT NULL,
    [Data_Search] nvarchar(max)  NOT NULL,
    [Status] nvarchar(max)  NOT NULL,
    [Layout_LayoutID] decimal(6,0)  NOT NULL
);
GO

-- Creating table 'Configs'
CREATE TABLE [dbo].[Configs] (
    [ConfigSetName] varchar(50)  NOT NULL,
    [ParameterName] varchar(50)  NOT NULL,
    [Int1] int  NULL,
    [Int2] int  NULL,
    [Int3] int  NULL,
    [Int4] int  NULL,
    [Float1] float  NULL,
    [Float2] float  NULL,
    [Float3] float  NULL,
    [Float4] float  NULL,
    [Date1] datetime  NULL,
    [Date2] datetime  NULL,
    [Date3] datetime  NULL,
    [Date4] datetime  NULL,
    [Str1] varchar(500)  NULL,
    [Str2] varchar(500)  NULL,
    [Str3] varchar(500)  NULL,
    [Str4] varchar(500)  NULL,
    [LongStr] varchar(1000)  NULL
);
GO

-- Creating table 'Bays'
CREATE TABLE [dbo].[Bays] (
    [LayoutID] decimal(6,0)  NOT NULL,
    [BayName] varchar(50)  NOT NULL,
    [BaySide] varchar(50)  NOT NULL,
    [BayPart] varchar(50)  NOT NULL,
    [Xmin] float  NOT NULL,
    [Ymin] float  NOT NULL,
    [Xmax] float  NOT NULL,
    [Ymax] float  NOT NULL,
    [BayId] int IDENTITY(1,1) NOT NULL,
    [Layout_LayoutID] decimal(6,0)  NOT NULL
);
GO

-- Creating table 'BlockAttributes'
CREATE TABLE [dbo].[BlockAttributes] (
    [BlockID] decimal(15,0)  NULL,
    [BlockAttributeID] decimal(22,0)  NULL,
    [LayoutID] decimal(6,0)  NOT NULL,
    [BlockIndex] decimal(6,0)  NOT NULL,
    [BlockAttributeIndex] decimal(6,0)  NOT NULL,
    [BlockAttributeName] varchar(100)  NULL,
    [BlockAttributeValue] varchar(1000)  NULL
);
GO

-- Creating table 'Blocks'
CREATE TABLE [dbo].[Blocks] (
    [BlockID] decimal(15,0)  NULL,
    [LayoutID] decimal(6,0)  NOT NULL,
    [BlockIndex] decimal(6,0)  NOT NULL,
    [BlockName] varchar(500)  NULL,
    [BlockXrefName] varchar(500)  NULL,
    [BlockHandle] varchar(25)  NULL,
    [Layout_LayoutID] decimal(6,0)  NOT NULL
);
GO

-- Creating table 'Frames'
CREATE TABLE [dbo].[Frames] (
    [BlockID] decimal(15,0)  NULL,
    [FrameID] decimal(22,0)  NULL,
    [LayoutID] decimal(6,0)  NOT NULL,
    [BlockIndex] decimal(6,0)  NOT NULL,
    [FrameIndex] decimal(6,0)  NOT NULL,
    [FrameTypeID] smallint  NOT NULL,
    [Xmin] float  NOT NULL,
    [Ymin] float  NOT NULL,
    [Xmax] float  NOT NULL,
    [Ymax] float  NOT NULL
);
GO

-- Creating table 'ItemAttributes'
CREATE TABLE [dbo].[ItemAttributes] (
    [ItemID] decimal(15,0)  NULL,
    [ItemAttributeID] decimal(22,0)  NULL,
    [LayoutID] decimal(6,0)  NOT NULL,
    [ItemIndex] decimal(6,0)  NOT NULL,
    [ItemAttributeIndex] decimal(6,0)  NOT NULL,
    [ItemAttributeName] varchar(100)  NOT NULL,
    [ItemAttributeValue] varchar(1000)  NULL,
    [Item_LayoutID] decimal(6,0)  NOT NULL,
    [Item_ItemIndex] decimal(6,0)  NOT NULL,
    [Item_ItemAttributeIndex] decimal(6,0)  NOT NULL
);
GO

-- Creating table 'Items'
CREATE TABLE [dbo].[Items] (
    [ItemID] decimal(15,0)  NULL,
    [ItemName] varchar(100)  NULL,
    [LayoutID] decimal(6,0)  NOT NULL,
    [ItemIndex] decimal(6,0)  NOT NULL,
    [ItemAttributeIndex] decimal(6,0)  NOT NULL,
    [ItemAttributeName] varchar(100)  NULL,
    [ItemAttributeID] decimal(22,0)  NULL,
    [BlockID] decimal(15,0)  NULL,
    [BlockIndex] decimal(6,0)  NOT NULL,
    [BlockName] varchar(500)  NOT NULL,
    [XrefName] varchar(500)  NULL,
    [LayerName] varchar(500)  NULL,
    [Xpos] float  NULL,
    [Ypos] float  NULL,
    [Zpos] float  NULL,
    [Xscale] float  NULL,
    [Yscale] float  NULL,
    [Zscale] float  NULL,
    [Rotation] float  NULL,
    [ItemHandle] varchar(25)  NULL,
    [Layout_LayoutID] decimal(6,0)  NOT NULL
);
GO

-- Creating table 'Layouts'
CREATE TABLE [dbo].[Layouts] (
    [LayoutID] decimal(6,0) IDENTITY(1,1) NOT NULL,
    [LayoutName] varchar(500)  NULL,
    [LayoutType] varchar(25)  NULL,
    [LayoutContents] varchar(500)  NULL,
    [AccessType] varchar(25)  NULL,
    [LayoutVersion] varchar(25)  NULL,
    [Comment] varchar(500)  NULL,
    [SiteName] varchar(25)  NULL,
    [BuildingLevels] varchar(1000)  NULL,
    [ProcessName1] varchar(25)  NULL,
    [ProcessName2] varchar(25)  NULL,
    [ProcessName3] varchar(25)  NULL,
    [ProcessName4] varchar(25)  NULL,
    [CADFileName] varchar(500)  NULL,
    [CreatedBy] int  NOT NULL,
    [DateCreated] datetime  NOT NULL,
    [ModifiedBy] int  NOT NULL,
    [DateModified] datetime  NULL,
    [LayoutState] smallint  NULL,
    [FSA] bit  NOT NULL,
    [ConfigSetName] varchar(50)  NULL,
    [Visible] bit  NOT NULL,
    [TABFileName] nvarchar(max)  NOT NULL,
    [Param1] nchar(25)  NULL,
    [Param2] nchar(25)  NULL,
    [Param3] nchar(25)  NULL,
    [Param4] nchar(25)  NULL
);
GO

-- Creating table 'Users'
CREATE TABLE [dbo].[Users] (
    [UserId] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [UserSettingId] int  NOT NULL,
    [email] nvarchar(max)  NOT NULL,
    [Settings_Data] nvarchar(max)  NOT NULL,
    [Drive] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'Rules'
CREATE TABLE [dbo].[Rules] (
    [RuleId] int  NOT NULL,
    [AttributePatternOn_Data] nvarchar(max)  NULL,
    [ColorIndex] int  NOT NULL,
    [FilterBlockAttributesOn_Data] nvarchar(max)  NULL,
    [IncludeNested] bit  NOT NULL,
    [isTypeFilterParent] bit  NOT NULL,
    [LayerDestination] nvarchar(max)  NOT NULL,
    [LayerPatternOn_Data] nvarchar(max)  NULL,
    [TypeFilterOn_Data] nvarchar(max)  NULL,
    [LineType] nvarchar(max)  NOT NULL,
    [Comment] nvarchar(max)  NOT NULL,
    [LayerPatternOff_Data] nvarchar(max)  NULL,
    [LayoutId] decimal(6,0)  NOT NULL,
    [Name] nvarchar(50)  NULL,
    [Position_X] float  NOT NULL,
    [Position_Y] float  NOT NULL,
    [Position_Z] float  NOT NULL,
    [LayoutCatalogSite_Data] nvarchar(max)  NULL,
    [LayoutCatalogOptions_Data] nvarchar(max)  NULL,
    [TooNameAttributes_Data] nvarchar(max)  NULL,
    [Layout_LayoutID] decimal(6,0)  NOT NULL
);
GO

-- Creating table 'UserSettings'
CREATE TABLE [dbo].[UserSettings] (
    [UserSettingId] int IDENTITY(1,1) NOT NULL,
    [UserId] int  NULL,
    [ConfigSetName] nvarchar(max)  NOT NULL,
    [ChainDistance] real  NOT NULL,
    [DateStarted] datetime  NOT NULL,
    [IsActive] bit  NOT NULL,
    [IsColorMode] bit  NOT NULL,
    [ProjectExplorerRowSplitterPosition] smallint  NOT NULL,
    [ProjectExplorerPGridColumnSplitterPosition] smallint  NOT NULL,
    [Percent] smallint  NOT NULL,
    [ProjectStatus] nvarchar(max)  NOT NULL,
    [ToggleLayoutDataTemplateSelector] smallint  NOT NULL,
    [MinWidth] int  NOT NULL,
    [LayoutId] decimal(6,0)  NULL,
    [Drive] nvarchar(max)  NOT NULL,
    [ColorIndex] int  NOT NULL,
    [Pos_X] float  NOT NULL,
    [Pos_Y] float  NOT NULL,
    [Pos_Z] float  NOT NULL,
    [GeoPos] geography  NOT NULL,
    [UserSettingUser_UserSetting_UserId] int  NOT NULL,
    [UserSettingUser_UserSetting_UserSettingId] int  NOT NULL,
    [Layout_LayoutID] decimal(6,0)  NOT NULL
);
GO

-- Creating table 'Filters'
CREATE TABLE [dbo].[Filters] (
    [FilterId] int IDENTITY(1,1) NOT NULL,
    [Active] bit  NULL,
    [AccessType] nvarchar(max)  NOT NULL,
    [FilterName] nvarchar(max)  NOT NULL,
    [FSA] bit  NOT NULL,
    [LayoutName] nvarchar(max)  NOT NULL,
    [LayoutType] nvarchar(max)  NOT NULL,
    [LayoutContents] nvarchar(max)  NOT NULL,
    [LayoutVersion] nvarchar(max)  NOT NULL,
    [Comment] nvarchar(max)  NOT NULL,
    [SiteName] nvarchar(max)  NOT NULL,
    [BuildingLevels] nvarchar(max)  NOT NULL,
    [CADFileName] nvarchar(max)  NOT NULL,
    [CreatedBy] nvarchar(max)  NOT NULL,
    [DateCreated] datetime  NULL,
    [ModifiedBy] nvarchar(max)  NOT NULL,
    [DateModified] tinyint  NULL,
    [LayoutState] smallint  NULL,
    [LayoutId] int  NOT NULL,
    [Layout_LayoutID] decimal(6,0)  NOT NULL
);
GO

-- Creating table 'Pages'
CREATE TABLE [dbo].[Pages] (
    [ID_Page] int  NOT NULL,
    [Title] nvarchar(50)  NOT NULL,
    [FullName] nvarchar(50)  NOT NULL,
    [URL] varchar(50)  NOT NULL,
    [AttributeURL] varchar(50)  NULL,
    [Tooltip] nvarchar(50)  NULL,
    [isActive] bit  NOT NULL,
    [isPrinted] bit  NOT NULL,
    [Ordered] int  NULL,
    [LevelAgent] int  NOT NULL,
    [ID_System] varchar(2)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [As_made], [StateId] in table 'States'
ALTER TABLE [dbo].[States]
ADD CONSTRAINT [PK_States]
    PRIMARY KEY CLUSTERED ([As_made], [StateId] ASC);
GO

-- Creating primary key on [ConfigSetName], [ParameterName] in table 'Configs'
ALTER TABLE [dbo].[Configs]
ADD CONSTRAINT [PK_Configs]
    PRIMARY KEY CLUSTERED ([ConfigSetName], [ParameterName] ASC);
GO

-- Creating primary key on [LayoutID], [BayId] in table 'Bays'
ALTER TABLE [dbo].[Bays]
ADD CONSTRAINT [PK_Bays]
    PRIMARY KEY CLUSTERED ([LayoutID], [BayId] ASC);
GO

-- Creating primary key on [LayoutID], [BlockIndex], [BlockAttributeIndex] in table 'BlockAttributes'
ALTER TABLE [dbo].[BlockAttributes]
ADD CONSTRAINT [PK_BlockAttributes]
    PRIMARY KEY CLUSTERED ([LayoutID], [BlockIndex], [BlockAttributeIndex] ASC);
GO

-- Creating primary key on [LayoutID], [BlockIndex] in table 'Blocks'
ALTER TABLE [dbo].[Blocks]
ADD CONSTRAINT [PK_Blocks]
    PRIMARY KEY CLUSTERED ([LayoutID], [BlockIndex] ASC);
GO

-- Creating primary key on [LayoutID], [BlockIndex], [FrameIndex] in table 'Frames'
ALTER TABLE [dbo].[Frames]
ADD CONSTRAINT [PK_Frames]
    PRIMARY KEY CLUSTERED ([LayoutID], [BlockIndex], [FrameIndex] ASC);
GO

-- Creating primary key on [LayoutID], [ItemIndex], [ItemAttributeIndex] in table 'ItemAttributes'
ALTER TABLE [dbo].[ItemAttributes]
ADD CONSTRAINT [PK_ItemAttributes]
    PRIMARY KEY CLUSTERED ([LayoutID], [ItemIndex], [ItemAttributeIndex] ASC);
GO

-- Creating primary key on [LayoutID], [ItemIndex], [ItemAttributeIndex] in table 'Items'
ALTER TABLE [dbo].[Items]
ADD CONSTRAINT [PK_Items]
    PRIMARY KEY CLUSTERED ([LayoutID], [ItemIndex], [ItemAttributeIndex] ASC);
GO

-- Creating primary key on [LayoutID] in table 'Layouts'
ALTER TABLE [dbo].[Layouts]
ADD CONSTRAINT [PK_Layouts]
    PRIMARY KEY CLUSTERED ([LayoutID] ASC);
GO

-- Creating primary key on [UserId], [UserSettingId] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [PK_Users]
    PRIMARY KEY CLUSTERED ([UserId], [UserSettingId] ASC);
GO

-- Creating primary key on [RuleId], [LayoutId] in table 'Rules'
ALTER TABLE [dbo].[Rules]
ADD CONSTRAINT [PK_Rules]
    PRIMARY KEY CLUSTERED ([RuleId], [LayoutId] ASC);
GO

-- Creating primary key on [UserSettingId] in table 'UserSettings'
ALTER TABLE [dbo].[UserSettings]
ADD CONSTRAINT [PK_UserSettings]
    PRIMARY KEY CLUSTERED ([UserSettingId] ASC);
GO

-- Creating primary key on [FilterId] in table 'Filters'
ALTER TABLE [dbo].[Filters]
ADD CONSTRAINT [PK_Filters]
    PRIMARY KEY CLUSTERED ([FilterId] ASC);
GO

-- Creating primary key on [ID_Page], [Title], [FullName], [URL], [isActive], [isPrinted], [LevelAgent], [ID_System] in table 'Pages'
ALTER TABLE [dbo].[Pages]
ADD CONSTRAINT [PK_Pages]
    PRIMARY KEY CLUSTERED ([ID_Page], [Title], [FullName], [URL], [isActive], [isPrinted], [LevelAgent], [ID_System] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [LayoutID], [BlockIndex] in table 'BlockAttributes'
ALTER TABLE [dbo].[BlockAttributes]
ADD CONSTRAINT [FK_BlockAttributes_Blocks]
    FOREIGN KEY ([LayoutID], [BlockIndex])
    REFERENCES [dbo].[Blocks]
        ([LayoutID], [BlockIndex])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [LayoutID], [BlockIndex] in table 'Frames'
ALTER TABLE [dbo].[Frames]
ADD CONSTRAINT [FK_Frames_Blocks]
    FOREIGN KEY ([LayoutID], [BlockIndex])
    REFERENCES [dbo].[Blocks]
        ([LayoutID], [BlockIndex])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Layout_LayoutID] in table 'States'
ALTER TABLE [dbo].[States]
ADD CONSTRAINT [FK_LayoutState]
    FOREIGN KEY ([Layout_LayoutID])
    REFERENCES [dbo].[Layouts]
        ([LayoutID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_LayoutState'
CREATE INDEX [IX_FK_LayoutState]
ON [dbo].[States]
    ([Layout_LayoutID]);
GO

-- Creating foreign key on [Layout_LayoutID] in table 'Blocks'
ALTER TABLE [dbo].[Blocks]
ADD CONSTRAINT [FK_LayoutBlock]
    FOREIGN KEY ([Layout_LayoutID])
    REFERENCES [dbo].[Layouts]
        ([LayoutID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_LayoutBlock'
CREATE INDEX [IX_FK_LayoutBlock]
ON [dbo].[Blocks]
    ([Layout_LayoutID]);
GO

-- Creating foreign key on [Layout_LayoutID] in table 'Items'
ALTER TABLE [dbo].[Items]
ADD CONSTRAINT [FK_LayoutItem]
    FOREIGN KEY ([Layout_LayoutID])
    REFERENCES [dbo].[Layouts]
        ([LayoutID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_LayoutItem'
CREATE INDEX [IX_FK_LayoutItem]
ON [dbo].[Items]
    ([Layout_LayoutID]);
GO

-- Creating foreign key on [Item_LayoutID], [Item_ItemIndex], [Item_ItemAttributeIndex] in table 'ItemAttributes'
ALTER TABLE [dbo].[ItemAttributes]
ADD CONSTRAINT [FK_ItemItemAttribute]
    FOREIGN KEY ([Item_LayoutID], [Item_ItemIndex], [Item_ItemAttributeIndex])
    REFERENCES [dbo].[Items]
        ([LayoutID], [ItemIndex], [ItemAttributeIndex])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ItemItemAttribute'
CREATE INDEX [IX_FK_ItemItemAttribute]
ON [dbo].[ItemAttributes]
    ([Item_LayoutID], [Item_ItemIndex], [Item_ItemAttributeIndex]);
GO

-- Creating foreign key on [Layout_LayoutID] in table 'Filters'
ALTER TABLE [dbo].[Filters]
ADD CONSTRAINT [FK_LayoutFilter]
    FOREIGN KEY ([Layout_LayoutID])
    REFERENCES [dbo].[Layouts]
        ([LayoutID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_LayoutFilter'
CREATE INDEX [IX_FK_LayoutFilter]
ON [dbo].[Filters]
    ([Layout_LayoutID]);
GO

-- Creating foreign key on [Layout_LayoutID] in table 'Bays'
ALTER TABLE [dbo].[Bays]
ADD CONSTRAINT [FK_LayoutBay]
    FOREIGN KEY ([Layout_LayoutID])
    REFERENCES [dbo].[Layouts]
        ([LayoutID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_LayoutBay'
CREATE INDEX [IX_FK_LayoutBay]
ON [dbo].[Bays]
    ([Layout_LayoutID]);
GO

-- Creating foreign key on [UserSettingUser_UserSetting_UserId], [UserSettingUser_UserSetting_UserSettingId] in table 'UserSettings'
ALTER TABLE [dbo].[UserSettings]
ADD CONSTRAINT [FK_UserSettingUser]
    FOREIGN KEY ([UserSettingUser_UserSetting_UserId], [UserSettingUser_UserSetting_UserSettingId])
    REFERENCES [dbo].[Users]
        ([UserId], [UserSettingId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserSettingUser'
CREATE INDEX [IX_FK_UserSettingUser]
ON [dbo].[UserSettings]
    ([UserSettingUser_UserSetting_UserId], [UserSettingUser_UserSetting_UserSettingId]);
GO

-- Creating foreign key on [Layout_LayoutID] in table 'UserSettings'
ALTER TABLE [dbo].[UserSettings]
ADD CONSTRAINT [FK_LayoutUserSetting]
    FOREIGN KEY ([Layout_LayoutID])
    REFERENCES [dbo].[Layouts]
        ([LayoutID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_LayoutUserSetting'
CREATE INDEX [IX_FK_LayoutUserSetting]
ON [dbo].[UserSettings]
    ([Layout_LayoutID]);
GO

-- Creating foreign key on [Layout_LayoutID] in table 'Rules'
ALTER TABLE [dbo].[Rules]
ADD CONSTRAINT [FK_LayoutRule]
    FOREIGN KEY ([Layout_LayoutID])
    REFERENCES [dbo].[Layouts]
        ([LayoutID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_LayoutRule'
CREATE INDEX [IX_FK_LayoutRule]
ON [dbo].[Rules]
    ([Layout_LayoutID]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------