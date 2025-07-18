
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 10/24/2014 15:12:46
-- Generated from EDMX file: C:\Projects\AcadNet\AcadNetData\Models\GisModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [C:\PROJECTS\ACADNET\ACADNETDATA\APP_DATA\MAPINFO.MDF];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_zHouseNumberMapa_zCityMapa]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[zHouseNumberMapa] DROP CONSTRAINT [FK_zHouseNumberMapa_zCityMapa];
GO
IF OBJECT_ID(N'[dbo].[FK_zHouseNumberMapa_zStreetMapa1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[zHouseNumberMapa] DROP CONSTRAINT [FK_zHouseNumberMapa_zStreetMapa1];
GO
IF OBJECT_ID(N'[dbo].[FK_zHouseNumberMapi_zCityMapi]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[zHouseNumberMapi] DROP CONSTRAINT [FK_zHouseNumberMapi_zCityMapi];
GO
IF OBJECT_ID(N'[dbo].[FK_zHouseNumberMapi_zStreetMapi]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[zHouseNumberMapi] DROP CONSTRAINT [FK_zHouseNumberMapi_zStreetMapi];
GO
IF OBJECT_ID(N'[dbo].[FK_zModule_zModule]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[zModule] DROP CONSTRAINT [FK_zModule_zModule];
GO
IF OBJECT_ID(N'[dbo].[FK_zStreetMapa_zCityMapa1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[zStreetMapa] DROP CONSTRAINT [FK_zStreetMapa_zCityMapa1];
GO
IF OBJECT_ID(N'[dbo].[FK_zStreetMapi_zCityMapi]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[zStreetMapi] DROP CONSTRAINT [FK_zStreetMapi_zCityMapi];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[MapinfoModelStoreContainer].[City]', 'U') IS NOT NULL
    DROP TABLE [MapinfoModelStoreContainer].[City];
GO
IF OBJECT_ID(N'[dbo].[ColorTypes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ColorTypes];
GO
IF OBJECT_ID(N'[MapinfoModelStoreContainer].[HouseNum]', 'U') IS NOT NULL
    DROP TABLE [MapinfoModelStoreContainer].[HouseNum];
GO
IF OBJECT_ID(N'[dbo].[Layers]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Layers];
GO
IF OBJECT_ID(N'[dbo].[State]', 'U') IS NOT NULL
    DROP TABLE [dbo].[State];
GO
IF OBJECT_ID(N'[MapinfoModelStoreContainer].[Street]', 'U') IS NOT NULL
    DROP TABLE [MapinfoModelStoreContainer].[Street];
GO
IF OBJECT_ID(N'[dbo].[sysdiagrams]', 'U') IS NOT NULL
    DROP TABLE [dbo].[sysdiagrams];
GO
IF OBJECT_ID(N'[dbo].[zCityMapa]', 'U') IS NOT NULL
    DROP TABLE [dbo].[zCityMapa];
GO
IF OBJECT_ID(N'[dbo].[zCityMapi]', 'U') IS NOT NULL
    DROP TABLE [dbo].[zCityMapi];
GO
IF OBJECT_ID(N'[dbo].[zHouseNumberMapa]', 'U') IS NOT NULL
    DROP TABLE [dbo].[zHouseNumberMapa];
GO
IF OBJECT_ID(N'[dbo].[zHouseNumberMapi]', 'U') IS NOT NULL
    DROP TABLE [dbo].[zHouseNumberMapi];
GO
IF OBJECT_ID(N'[dbo].[zLayersByModules]', 'U') IS NOT NULL
    DROP TABLE [dbo].[zLayersByModules];
GO
IF OBJECT_ID(N'[dbo].[zModule]', 'U') IS NOT NULL
    DROP TABLE [dbo].[zModule];
GO
IF OBJECT_ID(N'[dbo].[zStreetMapa]', 'U') IS NOT NULL
    DROP TABLE [dbo].[zStreetMapa];
GO
IF OBJECT_ID(N'[dbo].[zStreetMapi]', 'U') IS NOT NULL
    DROP TABLE [dbo].[zStreetMapi];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'City'
CREATE TABLE [dbo].[City] (
    [CityId] int IDENTITY(1,1) NOT NULL,
    [CityName] nvarchar(50)  NULL,
    [X] float  NULL,
    [Y] float  NULL
);
GO

-- Creating table 'ColorTypes'
CREATE TABLE [dbo].[ColorTypes] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Type] nvarchar(10)  NULL,
    [Color] nvarchar(25)  NULL,
    [Name] nvarchar(50)  NULL
);
GO

-- Creating table 'HouseNum'
CREATE TABLE [dbo].[HouseNum] (
    [HouseNumberId] int  NOT NULL,
    [City] int  NULL,
    [Street] int  NULL,
    [HouseNumberName] nvarchar(max)  NULL,
    [X] float  NULL,
    [Y] float  NULL
);
GO

-- Creating table 'Layers'
CREATE TABLE [dbo].[Layers] (
    [LayerId] int IDENTITY(1,1) NOT NULL,
    [LayerName] nvarchar(50)  NULL,
    [DisplayNameHeb] nvarchar(50)  NULL,
    [BaseLayer] bit  NULL,
    [Opacity] float  NULL,
    [Place] int  NULL,
    [Sort] int  NULL
);
GO

-- Creating table 'Street'
CREATE TABLE [dbo].[Street] (
    [StreetId] int IDENTITY(1,1) NOT NULL,
    [City] int  NULL,
    [StreetName] nvarchar(200)  NULL,
    [X] float  NULL,
    [Y] float  NULL
);
GO

-- Creating table 'sysdiagrams'
CREATE TABLE [dbo].[sysdiagrams] (
    [name] nvarchar(128)  NOT NULL,
    [principal_id] int  NOT NULL,
    [diagram_id] int IDENTITY(1,1) NOT NULL,
    [version] int  NULL,
    [definition] varbinary(max)  NULL
);
GO

-- Creating table 'zCityMapa'
CREATE TABLE [dbo].[zCityMapa] (
    [CityId] int IDENTITY(1,1) NOT NULL,
    [CityName] nvarchar(50)  NOT NULL,
    [X] float  NULL,
    [Y] float  NULL
);
GO

-- Creating table 'zCityMapi'
CREATE TABLE [dbo].[zCityMapi] (
    [CityId] int  NOT NULL,
    [CityName] nvarchar(50)  NOT NULL,
    [X] float  NULL,
    [Y] float  NULL
);
GO

-- Creating table 'zHouseNumberMapa'
CREATE TABLE [dbo].[zHouseNumberMapa] (
    [HouseNumberId] int IDENTITY(1,1) NOT NULL,
    [City] int  NULL,
    [Street] int  NOT NULL,
    [HouseNumberName] nvarchar(10)  NOT NULL,
    [X] float  NULL,
    [Y] float  NULL
);
GO

-- Creating table 'zHouseNumberMapi'
CREATE TABLE [dbo].[zHouseNumberMapi] (
    [HouseNumberId] int  NOT NULL,
    [City] int  NULL,
    [Street] int  NOT NULL,
    [HouseNumberName] nvarchar(10)  NOT NULL,
    [X] float  NULL,
    [Y] float  NULL
);
GO

-- Creating table 'zLayersByModules'
CREATE TABLE [dbo].[zLayersByModules] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [LayerId] int  NULL,
    [ModuleId] int  NULL
);
GO

-- Creating table 'zModule'
CREATE TABLE [dbo].[zModule] (
    [ModuleID] int  NOT NULL,
    [ModuleName] nvarchar(20)  NULL
);
GO

-- Creating table 'zStreetMapa'
CREATE TABLE [dbo].[zStreetMapa] (
    [StreetId] int IDENTITY(1,1) NOT NULL,
    [City] int  NOT NULL,
    [StreetName] nvarchar(200)  NOT NULL,
    [X] float  NULL,
    [Y] float  NULL
);
GO

-- Creating table 'zStreetMapi'
CREATE TABLE [dbo].[zStreetMapi] (
    [StreetId] int  NOT NULL,
    [City] int  NOT NULL,
    [StreetName] nvarchar(200)  NOT NULL,
    [X] float  NULL,
    [Y] float  NULL
);
GO

-- Creating table 'State'
CREATE TABLE [dbo].[State] (
    [AsMade] nvarchar(20)  NOT NULL,
    [Lat] float  NOT NULL,
    [Lon] float  NOT NULL,
    [FileName] nvarchar(50)  NULL,
    [DateCreated] datetime  NULL
);
GO

CREATE TABLE [dbo].[UserProfile] (
    [UserId]   INT            IDENTITY (1, 1) NOT NULL,
    [UserName] NVARCHAR (MAX) NULL,
	[Preference] XML NULL,
    PRIMARY KEY CLUSTERED ([UserId] ASC)
);
GO

CREATE TABLE [dbo].[webpages_Membership] (
    [UserId]                                  INT            NOT NULL,
    [CreateDate]                              DATETIME       NULL,
    [ConfirmationToken]                       NVARCHAR (128) NULL,
    [IsConfirmed]                             BIT            DEFAULT ((0)) NULL,
    [LastPasswordFailureDate]                 DATETIME       NULL,
    [PasswordFailuresSinceLastSuccess]        INT            DEFAULT ((0)) NOT NULL,
    [Password]                                NVARCHAR (128) NOT NULL,
    [PasswordChangedDate]                     DATETIME       NULL,
    [PasswordSalt]                            NVARCHAR (128) NOT NULL,
    [PasswordVerificationToken]               NVARCHAR (128) NULL,
    [PasswordVerificationTokenExpirationDate] DATETIME       NULL,
    PRIMARY KEY CLUSTERED ([UserId] ASC)
);
GO

CREATE TABLE [dbo].[webpages_OAuthMembership] (
    [Provider]       NVARCHAR (30)  NOT NULL,
    [ProviderUserId] NVARCHAR (100) NOT NULL,
    [UserId]         INT            NOT NULL,
    PRIMARY KEY CLUSTERED ([Provider] ASC, [ProviderUserId] ASC)
);
GO

CREATE TABLE [dbo].[webpages_Roles] (
    [RoleId]   INT            IDENTITY (1, 1) NOT NULL,
    [RoleName] NVARCHAR (256) NOT NULL,
    PRIMARY KEY CLUSTERED ([RoleId] ASC),
    UNIQUE NONCLUSTERED ([RoleName] ASC)
);
GO

CREATE TABLE [dbo].[webpages_UsersInRoles] (
    [UserId] INT NOT NULL,
    [RoleId] INT NOT NULL,
    PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC),
    CONSTRAINT [fk_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[UserProfile] ([UserId]),
    CONSTRAINT [fk_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[webpages_Roles] ([RoleId])
);
GO
-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [CityId] in table 'City'
ALTER TABLE [dbo].[City]
ADD CONSTRAINT [PK_City]
    PRIMARY KEY CLUSTERED ([CityId] ASC);
GO

-- Creating primary key on [ID] in table 'ColorTypes'
ALTER TABLE [dbo].[ColorTypes]
ADD CONSTRAINT [PK_ColorTypes]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [HouseNumberId] in table 'HouseNum'
ALTER TABLE [dbo].[HouseNum]
ADD CONSTRAINT [PK_HouseNum]
    PRIMARY KEY CLUSTERED ([HouseNumberId] ASC);
GO

-- Creating primary key on [LayerId] in table 'Layers'
ALTER TABLE [dbo].[Layers]
ADD CONSTRAINT [PK_Layers]
    PRIMARY KEY CLUSTERED ([LayerId] ASC);
GO

-- Creating primary key on [StreetId] in table 'Street'
ALTER TABLE [dbo].[Street]
ADD CONSTRAINT [PK_Street]
    PRIMARY KEY CLUSTERED ([StreetId] ASC);
GO

-- Creating primary key on [diagram_id] in table 'sysdiagrams'
ALTER TABLE [dbo].[sysdiagrams]
ADD CONSTRAINT [PK_sysdiagrams]
    PRIMARY KEY CLUSTERED ([diagram_id] ASC);
GO

-- Creating primary key on [CityId] in table 'zCityMapa'
ALTER TABLE [dbo].[zCityMapa]
ADD CONSTRAINT [PK_zCityMapa]
    PRIMARY KEY CLUSTERED ([CityId] ASC);
GO

-- Creating primary key on [CityId] in table 'zCityMapi'
ALTER TABLE [dbo].[zCityMapi]
ADD CONSTRAINT [PK_zCityMapi]
    PRIMARY KEY CLUSTERED ([CityId] ASC);
GO

-- Creating primary key on [HouseNumberId] in table 'zHouseNumberMapa'
ALTER TABLE [dbo].[zHouseNumberMapa]
ADD CONSTRAINT [PK_zHouseNumberMapa]
    PRIMARY KEY CLUSTERED ([HouseNumberId] ASC);
GO

-- Creating primary key on [HouseNumberId] in table 'zHouseNumberMapi'
ALTER TABLE [dbo].[zHouseNumberMapi]
ADD CONSTRAINT [PK_zHouseNumberMapi]
    PRIMARY KEY CLUSTERED ([HouseNumberId] ASC);
GO

-- Creating primary key on [Id] in table 'zLayersByModules'
ALTER TABLE [dbo].[zLayersByModules]
ADD CONSTRAINT [PK_zLayersByModules]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [ModuleID] in table 'zModule'
ALTER TABLE [dbo].[zModule]
ADD CONSTRAINT [PK_zModule]
    PRIMARY KEY CLUSTERED ([ModuleID] ASC);
GO

-- Creating primary key on [StreetId] in table 'zStreetMapa'
ALTER TABLE [dbo].[zStreetMapa]
ADD CONSTRAINT [PK_zStreetMapa]
    PRIMARY KEY CLUSTERED ([StreetId] ASC);
GO

-- Creating primary key on [StreetId] in table 'zStreetMapi'
ALTER TABLE [dbo].[zStreetMapi]
ADD CONSTRAINT [PK_zStreetMapi]
    PRIMARY KEY CLUSTERED ([StreetId] ASC);
GO

-- Creating primary key on [AsMade] in table 'State'
ALTER TABLE [dbo].[State]
ADD CONSTRAINT [PK_State]
    PRIMARY KEY CLUSTERED ([AsMade] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [City] in table 'zHouseNumberMapa'
ALTER TABLE [dbo].[zHouseNumberMapa]
ADD CONSTRAINT [FK_zHouseNumberMapa_zCityMapa]
    FOREIGN KEY ([City])
    REFERENCES [dbo].[zCityMapa]
        ([CityId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_zHouseNumberMapa_zCityMapa'
CREATE INDEX [IX_FK_zHouseNumberMapa_zCityMapa]
ON [dbo].[zHouseNumberMapa]
    ([City]);
GO

-- Creating foreign key on [City] in table 'zStreetMapa'
ALTER TABLE [dbo].[zStreetMapa]
ADD CONSTRAINT [FK_zStreetMapa_zCityMapa1]
    FOREIGN KEY ([City])
    REFERENCES [dbo].[zCityMapa]
        ([CityId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_zStreetMapa_zCityMapa1'
CREATE INDEX [IX_FK_zStreetMapa_zCityMapa1]
ON [dbo].[zStreetMapa]
    ([City]);
GO

-- Creating foreign key on [City] in table 'zHouseNumberMapi'
ALTER TABLE [dbo].[zHouseNumberMapi]
ADD CONSTRAINT [FK_zHouseNumberMapi_zCityMapi]
    FOREIGN KEY ([City])
    REFERENCES [dbo].[zCityMapi]
        ([CityId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_zHouseNumberMapi_zCityMapi'
CREATE INDEX [IX_FK_zHouseNumberMapi_zCityMapi]
ON [dbo].[zHouseNumberMapi]
    ([City]);
GO

-- Creating foreign key on [City] in table 'zStreetMapi'
ALTER TABLE [dbo].[zStreetMapi]
ADD CONSTRAINT [FK_zStreetMapi_zCityMapi]
    FOREIGN KEY ([City])
    REFERENCES [dbo].[zCityMapi]
        ([CityId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_zStreetMapi_zCityMapi'
CREATE INDEX [IX_FK_zStreetMapi_zCityMapi]
ON [dbo].[zStreetMapi]
    ([City]);
GO

-- Creating foreign key on [Street] in table 'zHouseNumberMapa'
ALTER TABLE [dbo].[zHouseNumberMapa]
ADD CONSTRAINT [FK_zHouseNumberMapa_zStreetMapa1]
    FOREIGN KEY ([Street])
    REFERENCES [dbo].[zStreetMapa]
        ([StreetId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_zHouseNumberMapa_zStreetMapa1'
CREATE INDEX [IX_FK_zHouseNumberMapa_zStreetMapa1]
ON [dbo].[zHouseNumberMapa]
    ([Street]);
GO

-- Creating foreign key on [Street] in table 'zHouseNumberMapi'
ALTER TABLE [dbo].[zHouseNumberMapi]
ADD CONSTRAINT [FK_zHouseNumberMapi_zStreetMapi]
    FOREIGN KEY ([Street])
    REFERENCES [dbo].[zStreetMapi]
        ([StreetId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_zHouseNumberMapi_zStreetMapi'
CREATE INDEX [IX_FK_zHouseNumberMapi_zStreetMapi]
ON [dbo].[zHouseNumberMapi]
    ([Street]);
GO

-- Creating foreign key on [ModuleID] in table 'zModule'
ALTER TABLE [dbo].[zModule]
ADD CONSTRAINT [FK_zModule_zModule]
    FOREIGN KEY ([ModuleID])
    REFERENCES [dbo].[zModule]
        ([ModuleID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------