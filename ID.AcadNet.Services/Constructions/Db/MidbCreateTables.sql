USE [CNTDB]
GO
/****** Object:  Table [dbo].[Tikra]  ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tikra](
	[Division] [nvarchar](20) NULL,
	[Level] [nvarchar](20) NOT NULL,
	[Eliv] [real] NULL,
	[Namedwg] [nvarchar](20) NULL,
	[typSlab] [nvarchar](20) NULL,
	[proDatCons] [datetime] NULL,
	[proDraw] [datetime] NULL,
	[proFea] [datetime] NULL,
	[proConBeam] [datetime] NULL,
	[proConPlate] [datetime] NULL,
	[basePointX] [real] NULL,
	[BasePointY] [real] NULL,
	[DistancGr] [real] NULL,
	[flTsec] [smallint] NULL,
	[kTors] [real] NULL,
	[nMeshX] [smallint] NULL,
	[nMeshY] [smallint] NULL,
	[par1] [real] NULL,
	[par2] [real] NULL,
	[nCalc] [smallint] NULL,
	[flChan] [smallint] NULL,
	[num] [smallint] NULL,
	[datold] [datetime] NULL,
	[inBetonBem] [smallint] NULL,
	[inBetonPlt] [smallint] NULL,
	[inBetonCol] [smallint] NULL,
	[inBetonWal] [smallint] NULL,
	[inBetonPil] [smallint] NULL,
	[ipar1] [smallint] NULL,
	[ipar2] [smallint] NULL,
	[ipar3] [smallint] NULL,
	[ipar4] [smallint] NULL,
	[ipar5] [smallint] NULL,
	[ipar6] [smallint] NULL,
	[ipar7] [smallint] NULL,
	[ipar8] [smallint] NULL,
	[ipar9] [smallint] NULL,
	[ipar10] [smallint] NULL,
	[par3] [real] NULL,
	[par4] [real] NULL,
	[par5] [real] NULL,
	[par6] [real] NULL,
	[par7] [real] NULL,
	[par8] [real] NULL,
	[par9] [real] NULL,
	[par10] [real] NULL,
	[par11] [real] NULL,
	[par12] [real] NULL,
	[par13] [real] NULL,
	[par14] [real] NULL,
	[par15] [real] NULL,
	[par16] [real] NULL,
	[par17] [real] NULL,
	[par18] [real] NULL,
	[par19] [real] NULL,
	[par20] [real] NULL
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'AllowZeroLength', @value=N'True' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Tikra', @level2type=N'COLUMN',@level2name=N'Division'
GO

