/*
 Copyright (C) 2008 Kemal ERDOGAN
 
 This program is free software: you can redistribute it and/or modify
 it under the terms of the GNU Lesser General Public License as published by
 the Free Software Foundation, version 3 of the License.
 
 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU Lesser General Public License for more details.
 
 You should have received a copy of the GNU Lesser General Public License
 along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[EdgeExists]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROC [EdgeExists] (
    @StartVertexId varchar(100),
    @EndVertexId varchar(100),
    @Source varchar(150) )
AS
BEGIN
    IF EXISTS (
        SELECT Hops
            FROM Edge
            WHERE StartVertex = @StartVertexId
              AND EndVertex = @EndVertexId
              AND Source = @Source )
    BEGIN
        SELECT CONVERT(BIT, 1) AS Result
        RETURN 1
    END ELSE
    BEGIN
        SELECT CONVERT(BIT, 0) AS Result
        RETURN 0
    END
END
' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[AccessPredicate]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [AccessPredicate](
	[PrincipalId] [varchar](256) NOT NULL,
	[OperationId] [varchar](10) NOT NULL,
	[ResourceId] [varchar](256) NOT NULL,
	[PredicateType] [tinyint] NOT NULL,
 CONSTRAINT [PK_AccessPredicate_1] PRIMARY KEY CLUSTERED 
(
	[PrincipalId] ASC,
	[OperationId] ASC,
	[ResourceId] ASC,
	[PredicateType] ASC
)WITH FILLFACTOR = 90 ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING ON
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[Principal]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [Principal](
	[ObjectId] [uniqueidentifier] NOT NULL,
	[PrincipalType] [smallint] NOT NULL,
	[Name] [varchar](256) NOT NULL,
	[Email] [varchar](500) NOT NULL,
	[DisplayName] [varchar](500) NOT NULL,
	[Description] [varchar](500) NOT NULL,
	[DataSource] [varchar](100) NOT NULL,
 CONSTRAINT [PK_Principal_1] PRIMARY KEY CLUSTERED 
(
	[ObjectId] ASC
)WITH FILLFACTOR = 90 ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysindexes WHERE id = OBJECT_ID(N'[Principal]') AND name = N'IX_Principal_1')
CREATE UNIQUE NONCLUSTERED INDEX [IX_Principal_1] ON [Principal] 
(
	[Name] ASC
)WITH FILLFACTOR = 90 ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[Sync_Edge]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [Sync_Edge](
	[StartVertex] [varchar](256) NOT NULL,
	[EndVertex] [varchar](256) NOT NULL,
	[Source] [varchar](150) NOT NULL,
 CONSTRAINT [PK_EdgeSyncList] PRIMARY KEY CLUSTERED 
(
	[StartVertex] ASC,
	[EndVertex] ASC
)WITH FILLFACTOR = 90 ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING ON
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[Sync_Principal]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [Sync_Principal](
	[ObjectId] [uniqueidentifier] NOT NULL,
	[PrincipalType] [smallint] NOT NULL,
	[Name] [varchar](256) NOT NULL,
	[Email] [varchar](500) NOT NULL,
	[DisplayName] [varchar](500) NOT NULL,
	[Description] [varchar](500) NOT NULL,
	[DataSource] [varchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ObjectId] ASC
)WITH FILLFACTOR = 90 ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING ON
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[Operation]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [Operation](
	[Id] [varchar](10) NOT NULL,
	[Name] [varchar](250) NOT NULL,
	[Description] [varchar](500) NOT NULL,
 CONSTRAINT [PK_Operation_1] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH FILLFACTOR = 90 ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING ON
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[AddEdgeWithSpaceSavings]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'
CREATE PROC [AddEdgeWithSpaceSavings]
   @StartVertexId varchar(256),
   @EndVertexId varchar(256),
   @Source varchar(150)
AS
BEGIN
   SET NOCOUNT ON

   IF EXISTS(SELECT Hops 
                   FROM Edge 
                   WHERE StartVertex = @StartVertexId 
                     AND EndVertex = @EndVertexId 
                     AND Source = @Source
                     AND Hops = 0)
   BEGIN
      RETURN 0 -- DO NOTHING!!!
   END

   IF @StartVertexId = @EndVertexId 
      OR EXISTS (SELECT Hops 
                     FROM Edge 
                     WHERE StartVertex = @EndVertexId 
                       AND EndVertex = @StartVertexId
                       AND Source = @Source)
   BEGIN
      RAISERROR (''Attempt to create a circular relation detected!'', 16, 1)
      RETURN 0
   END

   CREATE TABLE #Candidates ( 
       StartVertex varchar(256),
       EndVertex varchar(256) )

   INSERT INTO #Candidates
          SELECT StartVertex 
               , @EndVertexId
              FROM Edge
              WHERE EndVertex = @StartVertexId
                AND Source = @Source
          UNION
          SELECT @StartVertexId
               , EndVertex
              FROM Edge
              WHERE StartVertex = @EndVertexId
              AND Source = @Source
          UNION
          SELECT A.StartVertex 
               , B.EndVertex
              FROM Edge A
                 CROSS JOIN Edge B
              WHERE A.EndVertex = @StartVertexId
                AND B.StartVertex = @EndVertexId
                AND A.Source = @Source
                AND B.Source = @Source

   INSERT INTO Edge (
         StartVertex,
         EndVertex,
         Hops,
         Source)
     VALUES ( 
         @StartVertexId
       , @EndVertexId
       , 0
       , @Source )
         
   INSERT INTO Edge (
         StartVertex,
         EndVertex,
         Hops,
         Source)
      SELECT StartVertex,
             EndVertex,
             1,
             @Source
         FROM #Candidates C
         WHERE NOT EXISTS (
             SELECT Hops
                FROM Edge E
                   WHERE E.StartVertex = C.StartVertex
                     AND E.EndVertex = C.EndVertex 
                     AND E.Hops = 1)

END ' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[Edge]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [Edge](
	[StartVertex] [varchar](256) NOT NULL,
	[EndVertex] [varchar](256) NOT NULL,
	[Hops] [int] NOT NULL,
	[Source] [varchar](150) NOT NULL,
	[DelMark] [bit] NOT NULL
) ON [PRIMARY]
END
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysindexes WHERE id = OBJECT_ID(N'[Edge]') AND name = N'IX_Edge_1')
CREATE CLUSTERED INDEX [IX_Edge_1] ON [Edge] 
(
	[StartVertex] ASC,
	[EndVertex] ASC
)WITH FILLFACTOR = 90 ON [PRIMARY]
GO
IF NOT EXISTS (SELECT * FROM dbo.sysindexes WHERE id = OBJECT_ID(N'[Edge]') AND name = N'IX_Edge_2')
CREATE NONCLUSTERED INDEX [IX_Edge_2] ON [Edge] 
(
	[EndVertex] ASC,
	[StartVertex] ASC
)WITH FILLFACTOR = 90 ON [PRIMARY]
GO
IF NOT EXISTS (SELECT * FROM dbo.sysindexes WHERE id = OBJECT_ID(N'[Edge]') AND name = N'IX_Edge_3')
CREATE NONCLUSTERED INDEX [IX_Edge_3] ON [Edge] 
(
	[Hops] ASC
)WITH FILLFACTOR = 90 ON [PRIMARY]
GO
IF NOT EXISTS (SELECT * FROM dbo.sysindexes WHERE id = OBJECT_ID(N'[Edge]') AND name = N'IX_Edge_4')
CREATE NONCLUSTERED INDEX [IX_Edge_4] ON [Edge] 
(
	[Source] ASC
)WITH FILLFACTOR = 90 ON [PRIMARY]
GO
IF NOT EXISTS (SELECT * FROM dbo.sysindexes WHERE id = OBJECT_ID(N'[Edge]') AND name = N'IX_Edge_5')
CREATE NONCLUSTERED INDEX [IX_Edge_5] ON [Edge] 
(
	[DelMark] ASC
)WITH FILLFACTOR = 90 ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[Resource]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [Resource](
	[Id] [varchar](36) NOT NULL,
	[Name] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH FILLFACTOR = 90 ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_PADDING ON
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[TEST_EDGE_DATA]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [TEST_EDGE_DATA](
	[StartVertex] [varchar](256) NOT NULL,
	[EndVertex] [varchar](256) NOT NULL,
	[Hops] [int] NOT NULL,
	[Source] [varchar](150) NOT NULL,
	[DelMark] [bit] NOT NULL
) ON [PRIMARY]
END
GO
SET ANSI_PADDING ON
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[TEST_EDGE_DEL_DATA]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [TEST_EDGE_DEL_DATA](
	[StartVertex] [varchar](256) NOT NULL,
	[EndVertex] [varchar](256) NOT NULL,
	[Source] [varchar](150) NOT NULL
) ON [PRIMARY]
END
GO
SET ANSI_PADDING ON
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[FlatGrantList]') AND OBJECTPROPERTY(id, N'IsView') = 1)
EXEC dbo.sp_executesql @statement = N'    CREATE VIEW [FlatGrantList] 
    AS
    SELECT DISTINCT
        PL.StartVertex AS PrincipalId
       ,OL.StartVertex AS OperationId
       ,RL.StartVertex AS ResourceId
     FROM ( SELECT E.EndVertex, E.StartVertex 
            FROM Edge E
                INNER JOIN AccessPredicate AP
                   ON E.EndVertex = AP.PrincipalId
                WHERE E.Source = ''Principal''
            UNION 
            SELECT PrincipalId, PrincipalId 
            FROM AccessPredicate) PL
            CROSS JOIN (
                SELECT E.EndVertex, E.StartVertex 
                FROM Edge E
                    INNER JOIN AccessPredicate AP
                       ON E.EndVertex = AP.OperationId
                WHERE E.Source = ''Operation''
                UNION 
                SELECT OperationId, OperationId 
                FROM AccessPredicate) OL
            CROSS JOIN (
                SELECT E.EndVertex, E.StartVertex 
                FROM Edge E
                    INNER JOIN AccessPredicate AP
                       ON E.EndVertex = AP.ResourceId
                WHERE E.Source = ''Resource''
                UNION 
                SELECT ResourceId, ResourceId 
                FROM AccessPredicate) RL
            INNER JOIN AccessPredicate ACL 
               ON PL.EndVertex = ACL.PrincipalId
              AND RL.EndVertex = ACL.ResourceId
              AND OL.EndVertex = ACL.OperationId
'
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[FlatGrantListWithFlatResources]') AND OBJECTPROPERTY(id, N'IsView') = 1)
EXEC dbo.sp_executesql @statement = N'    CREATE VIEW [FlatGrantListWithFlatResources] 
    AS
    SELECT 
        PL.StartVertex AS PrincipalId
       ,OL.StartVertex AS OperationId
       ,ACL.ResourceId
     FROM ( SELECT E.EndVertex, E.StartVertex 
            FROM Edge E
                INNER JOIN AccessPredicate AP
                   ON E.EndVertex = AP.PrincipalId
                WHERE E.Source = ''Principal''
            UNION 
            SELECT PrincipalId, PrincipalId 
            FROM AccessPredicate) PL
            CROSS JOIN (
                SELECT E.EndVertex, E.StartVertex 
                FROM Edge E
                    INNER JOIN AccessPredicate AP
                       ON E.EndVertex = AP.OperationId
                WHERE E.Source = ''Operation''
                UNION 
                SELECT OperationId, OperationId 
                FROM AccessPredicate) OL
            INNER JOIN AccessPredicate ACL 
               ON PL.EndVertex = ACL.PrincipalId
              AND OL.EndVertex = ACL.OperationId
'
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[GetAuthorizedResources]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROC [GetAuthorizedResources] (
    @PrincipalId varchar(36),
    @OperationId varchar(10)
)
AS
BEGIN
    SELECT DISTINCT
        ResourceId
      FROM FlatGrantList
      WHERE PrincipalId = @PrincipalId
        AND OperationId = @OperationId
END
' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[FlatGrantListWithFlatResourcesAndOperations]') AND OBJECTPROPERTY(id, N'IsView') = 1)
EXEC dbo.sp_executesql @statement = N'    CREATE VIEW [FlatGrantListWithFlatResourcesAndOperations] 
    AS
    SELECT 
        PL.StartVertex AS PrincipalId
       ,ACL.OperationId
       ,ACL.ResourceId
     FROM ( SELECT E.EndVertex, E.StartVertex 
            FROM Edge E
                INNER JOIN AccessPredicate AP
                   ON E.EndVertex = AP.PrincipalId
                WHERE E.Source = ''Principal''
            UNION 
            SELECT PrincipalId, PrincipalId 
            FROM AccessPredicate) PL
        INNER JOIN AccessPredicate ACL 
           ON PL.EndVertex = ACL.PrincipalId'
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[GetAuthorizedOperations]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROC [GetAuthorizedOperations] (
    @PrincipalId varchar(36),
    @ResourceId varchar(100)
)
AS
BEGIN
    SELECT DISTINCT
        OperationId
      FROM FlatGrantList
      WHERE PrincipalId = @PrincipalId
        AND ResourceId = @ResourceId
END
' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[GetAuthorizationDigest]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROC [GetAuthorizationDigest] (
    @PrincipalId varchar(36)
)
AS
BEGIN
    SELECT DISTINCT
        OperationId,
        ResourceId,
        CONVERT(INT, 0) as PredicateType
      FROM FlatGrantList
      WHERE PrincipalId = @PrincipalId
END
' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[IsAuthorized]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROC [IsAuthorized] (
    @PrincipalId varchar(36),
    @OperationId varchar(10),
    @ResourceId varchar(100)
)
AS
BEGIN
    IF @ResourceId IS NULL
        SELECT @ResourceId = ''''
        
    IF EXISTS (
            SELECT PL.PrincipalId, OL.OperationId, RL.ResourceId
     FROM ( SELECT EndVertex as PrincipalId -- groups of @PrincipalId
                FROM Edge  
                WHERE Source = ''Principal''
                  AND StartVertex = @PrincipalId
            UNION 
            SELECT @PrincipalId as PrincipalId  -- @PrincipalId itself
            )  PL -- Part 1
            CROSS JOIN (
                SELECT EndVertex as OperationId -- parent operations of @OperationId
                    FROM Edge 
                    WHERE Source = ''Operation''
                      AND StartVertex = @OperationId
                UNION 
                SELECT @OperationId as OperationId -- @OperationId itself 
            )  OL -- Part 2
            CROSS JOIN (
                SELECT EndVertex as ResourceId -- parent resources of @ResourceId
                    FROM Edge 
                    WHERE Source = ''Resource''
                      AND StartVertex = @ResourceId
                UNION 
                SELECT @ResourceId as ResourceId  -- @ResourceId itself 
            ) RL -- Part 3  
	        INNER JOIN AccessPredicate ACL 
	           ON PL.PrincipalId = ACL.PrincipalId
	          AND OL.OperationId = ACL.OperationId
	          AND RL.ResourceId = ACL.ResourceId )
    BEGIN
        SELECT CONVERT(BIT, 1) AS Result
        RETURN 1
    END ELSE
    BEGIN
        SELECT CONVERT(BIT, 0) AS Result
        RETURN 0
    END
END' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[IsMemberOf]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROC [IsMemberOf] (
   @GroupId varchar(36),
   @MemberId varchar(36) 
)
AS
BEGIN
    
    IF EXISTS(
        SELECT EndVertex
            FROM Edge  
            WHERE Source = ''Principal''
              AND StartVertex = @MemberId
              AND EndVertex =  @GroupId )
    BEGIN
        SELECT CONVERT(BIT, 1) AS Result
        RETURN 1
    END ELSE
    BEGIN
        SELECT CONVERT(BIT, 0) AS Result
        RETURN 0
    END    
END
' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[AddOrUpdateOperation]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROC [AddOrUpdateOperation] (
  @Id varchar(10),
  @Name varchar(250),
  @Description varchar(500) )
AS
BEGIN
    UPDATE Operation
        SET Name = ISNULL(@Name, @Id)
           ,Description = ISNULL(@Description, '''')
    WHERE Id = @Id
    
    IF @@ROWCOUNT = 0
    BEGIN
        INSERT INTO Operation
                   (Id
                   ,Name
                   ,Description)
             VALUES
                   (@Id
                   ,ISNULL(@Name, @Id)
                   ,ISNULL(@Description, ''''))
    END

END  
' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[GetOperationList]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROC [GetOperationList]
AS
BEGIN
    SELECT Id
          ,Name
          ,Description
      FROM Operation
END
' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[GetSubOperationsList]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROC [GetSubOperationsList] (
    @OperationId varchar(10),
    @IsFlat int
)   
AS
BEGIN
    SELECT DISTINCT
           O.Id
          ,O.Name
          ,O.Description
      FROM Operation O
          INNER JOIN Edge E  
             ON O.Id = E.StartVertex
      WHERE E.EndVertex = @OperationId
        AND E.Source = ''Operation''
        AND ( @IsFlat = 1 OR E.Hops = @IsFlat )
       
END
    
' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[Test_DeleteAllData]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROC [Test_DeleteAllData]
AS
BEGIN
    truncate table Principal
    truncate table Operation
    truncate table Edge
    truncate table AccessPredicate
    truncate table Sync_Principal
    truncate table Sync_Edge
END

' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[Test_Performance_AddEdge_CreateBigGraph]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROC [Test_Performance_AddEdge_CreateBigGraph] (
    @MaxNodes int,
    @MaxTries int
) AS
BEGIN
    TRUNCATE TABLE Edge
    IF @MaxNodes > 1E4
    BEGIN
      RAISERROR (''Supports up to 10,000 vertices''
                 ,16
                 ,1
                  )
      RETURN
    END
    
    DECLARE @idx int
    DECLARE @jdx int
    DECLARE @StartVertex int
    DECLARE @EndVertex int
    DECLARE @totalTime bigint
    DECLARE @start datetime
    DECLARE @cnt int
    
    
    -- BOTH RANDOM
    SELECT @idx = 1, @jdx = 1, @totalTime = 0, @cnt = 0
    WHILE @idx <= @MaxTries
    BEGIN
        SELECT @StartVertex = CONVERT(int, RAND() * 1E6) % @MaxNodes + 1
              ,@EndVertex =   CONVERT(int, RAND() * 1E6) % @MaxNodes + 1
    
        IF @StartVertex <> @EndVertex
            AND NOT EXISTS (SELECT Hops FROM Edge WHERE EndVertex = @StartVertex AND StartVertex = @EndVertex)
        BEGIN
            SELECT @start = GETDATE()
            EXEC AddEdgeWithSpaceSavings  @StartVertex, @EndVertex, ''BigGraph''
            SELECT @TotalTime = @TotalTime + DATEDIFF(ms, @start, GETDATE())
            SELECT @cnt = @cnt + 1
        END
        SELECT @jdx = @jdx + 1
        SELECT @idx = @idx + 1
    END

    -- FIXED START WITH RANDOM END
    SELECT @idx = 1, @jdx = 1
    WHILE @idx <= @MaxNodes
    BEGIN
        WHILE @jdx <= @MaxTries
        BEGIN 
            SELECT @StartVertex = @idx
                  ,@EndVertex =   CONVERT(int, RAND() * 1E6) % @MaxNodes + 1
        
            IF @StartVertex <> @EndVertex
                AND NOT EXISTS (SELECT Hops FROM Edge WHERE EndVertex = @StartVertex AND StartVertex = @EndVertex)
            BEGIN
                SELECT @start = GETDATE()
                EXEC AddEdgeWithSpaceSavings  @StartVertex, @EndVertex, ''BigGraph''
                SELECT @TotalTime = @TotalTime + DATEDIFF(ms, @start, GETDATE())
                SELECT @cnt = @cnt + 1
            END
            SELECT @jdx = @jdx + 1
        END
        SELECT @idx = @idx + 1
    END

    -- FIXED END WITH RANDOM START
    SELECT @idx = 1, @jdx = 1
    WHILE @idx <= @MaxNodes
    BEGIN
        WHILE @jdx <= @MaxTries
        BEGIN 
            SELECT @StartVertex = CONVERT(int, RAND() * 1E6) % @MaxNodes + 1
                  ,@EndVertex = @idx
        
            IF @StartVertex <> @EndVertex
                AND NOT EXISTS (SELECT Hops FROM Edge WHERE EndVertex = @StartVertex AND StartVertex = @EndVertex)
            BEGIN
                SELECT @start = GETDATE()
                EXEC AddEdgeWithSpaceSavings  @StartVertex, @EndVertex, ''BigGraph''
                SELECT @TotalTime = @TotalTime + DATEDIFF(ms, @start, GETDATE())
                SELECT @cnt = @cnt + 1
            END
            SELECT @jdx = @jdx + 1
        END
        SELECT @idx = @idx + 1
    END


    -- COMBINATIONS
    SELECT @idx = 1, @jdx = @MaxNodes
    WHILE @idx <= @MaxNodes
    BEGIN
        WHILE @jdx >= 1
        BEGIN
            SELECT @StartVertex = @idx
                  ,@EndVertex = @jdx
        
            IF @StartVertex <> @EndVertex
                AND NOT EXISTS (SELECT Hops FROM Edge WHERE EndVertex = @StartVertex AND StartVertex = @EndVertex)
            BEGIN
                SELECT @start = GETDATE()
                EXEC AddEdgeWithSpaceSavings  @StartVertex, @EndVertex, ''BigGraph''
                SELECT @TotalTime = @TotalTime + DATEDIFF(ms, @start, GETDATE())
                SELECT @cnt = @cnt + 1
            END
            SELECT @jdx = @jdx - 1
        END
        SELECT @idx = @idx + 1
    END
 
    PRINT ''AVG INSERTION TIME (milliseconds) = '' + CONVERT( varchar(10), @totalTime/@cnt )
END
' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[Test_Performance_AddEdge]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROC [Test_Performance_AddEdge]
AS
BEGIN
    DECLARE   @StartVertexId varchar(256)
    DECLARE   @EndVertexId varchar(256)
    DECLARE   @Source varchar(150)
    DECLARE   @start datetime
    DECLARE   @totalTime bigint
    DECLARE   @testCnt int

    DECLARE EdgeCursor CURSOR FOR
	    SELECT StartVertex, EndVertex, Source
	    FROM TEST_EDGE_DATA loc
	    WHERE Hops = 0   

    TRUNCATE TABLE Edge
    OPEN EdgeCursor

    FETCH NEXT FROM EdgeCursor INTO @StartVertexId, @EndVertexId, @Source
    IF @@FETCH_STATUS = 0
    BEGIN
        SELECT @testCnt = 0, @totalTime = 0
        WHILE @@FETCH_STATUS = 0
        BEGIN
            SELECT @start = GETDATE()
	        EXEC AddEdgeWithSpaceSavings @StartVertexId, @EndVertexId, @Source
            SELECT @totalTime = @totalTime + DATEDIFF(ms, @start, GETDATE())
    	    
	        FETCH NEXT FROM EdgeCursor INTO @StartVertexId, @EndVertexId, @Source
	        SELECT @testCnt = @testCnt + 1
        END
        PRINT ''AVG INSERTION TIME (milliseconds) = '' + CONVERT( varchar(10), @totalTime/@testCnt )
    END ELSE
    BEGIN
        PRINT ''NO DATA FOUND FOR PERFORMANCE TESTING!''
    END
    CLOSE EdgeCursor
    DEALLOCATE EdgeCursor
END
' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[AddAccessPredicate]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROC [AddAccessPredicate] (
  @PrincipalId varchar(256),
  @OperationId varchar(10),
  @ResourceId varchar(256),
  @PredicateType int  )
AS
BEGIN
    INSERT INTO AccessPredicate
               (PrincipalId
               ,OperationId
               ,ResourceId
               ,PredicateType)
         VALUES
               (@PrincipalId
               ,@OperationId
               ,@ResourceId
               ,@PredicateType)
END  
' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[FlatGrantListWithFlatOperations]') AND OBJECTPROPERTY(id, N'IsView') = 1)
EXEC dbo.sp_executesql @statement = N'    CREATE VIEW [FlatGrantListWithFlatOperations] 
    AS
    SELECT DISTINCT
        PL.StartVertex AS PrincipalId
       ,ACL.OperationId
       ,RL.StartVertex AS ResourceId
     FROM ( SELECT E.EndVertex, E.StartVertex 
            FROM Edge E
                INNER JOIN AccessPredicate AP
                   ON E.EndVertex = AP.PrincipalId
                WHERE E.Source = ''Principal''
            UNION 
            SELECT PrincipalId, PrincipalId 
            FROM AccessPredicate) PL
            CROSS JOIN (
                SELECT E.EndVertex, E.StartVertex 
                FROM Edge E
                    INNER JOIN AccessPredicate AP
                       ON E.EndVertex = AP.ResourceId
                WHERE E.Source = ''Resource''
                UNION 
                SELECT ResourceId, ResourceId 
                FROM AccessPredicate) RL
            INNER JOIN AccessPredicate ACL 
               ON PL.EndVertex = ACL.PrincipalId
              AND RL.EndVertex = ACL.ResourceId
'
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[GetMembersList]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROC [GetMembersList] (
    @GroupName varchar(256),
    @IsFlat int
)   
AS
BEGIN
    SELECT DISTINCT
           P.Name
          ,P.DisplayName
          ,P.PrincipalType
          ,P.Email
          ,P.Description
      FROM Principal P
          INNER JOIN Edge E  
             ON P.Name = E.StartVertex
      WHERE E.EndVertex = @GroupName
        AND E.Source = ''Principal''
        AND ( @IsFlat = 1 OR E.Hops = @IsFlat )
        
END
    
' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[GetPrincipalList]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROC [GetPrincipalList] (
    @PrincipalType int = NULL
) AS
BEGIN
    SELECT Name
          ,DisplayName
          ,PrincipalType
          ,Email
          ,Description
      FROM Principal
      WHERE @PrincipalType IS NULL 
         OR @PrincipalType = PrincipalType
END    ' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[AddOrUpdatePrincipal]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROC [AddOrUpdatePrincipal] (
  @Name varchar(256),
  @PrincipalType int,
  @DisplayName varchar(500),
  @Email varchar(500),
  @Description varchar(500),
  @Source varchar(150) )
AS
BEGIN

    UPDATE Principal
        SET DisplayName = ISNULL(@DisplayName, @Name)
           ,PrincipalType = @PrincipalType
           ,Email = ISNULL(@Email, '''')
           ,Description = ISNULL(@Description, '''')
           ,DataSource = ISNULL(@Source, '''')
        WHERE Name = @Name

    IF @@ROWCOUNT = 0
    BEGIN
        INSERT INTO Principal
                   (Name
                   ,DisplayName
                   ,PrincipalType
                   ,Email
                   ,Description
                   ,DataSource)
             VALUES
                   (@Name
                   ,ISNULL(@DisplayName, @Name)
                   ,@PrincipalType
                   ,ISNULL(@Email, '''')
                   ,ISNULL(@Description, '''')
                   ,ISNULL(@Source, ''''))
                   
    END
END  
' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[Sync_AddEdgeToSyncList]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROC [Sync_AddEdgeToSyncList] (
    @StartVertex varchar(256),
    @EndVertex varchar(256),
    @EdgeSource varchar(150))
AS
BEGIN
    INSERT INTO Sync_Edge (
        StartVertex, 
        EndVertex, 
        Source )
     VALUES (
        @StartVertex, 
        @EndVertex, 
        @EdgeSource )
END' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[Sync_ClearSyncData]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROC [Sync_ClearSyncData] (
    @PrincipalSource varchar(100),
    @EdgeSource varchar(100)
) AS
BEGIN
    DELETE Sync_Principal WHERE DataSource = @PrincipalSource
    DELETE Sync_Edge WHERE Source = @EdgeSource
END' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[Sync_AddPrincipalToSyncList]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROC [Sync_AddPrincipalToSyncList] (
  @ObjectId uniqueidentifier,
  @PrincipalType int,
  @Name varchar(256),
  @Email varchar(500),
  @DisplayName varchar(500),
  @Description varchar(500),
  @DataSource varchar(100) )
AS
BEGIN
    INSERT INTO Sync_Principal
               (ObjectId
               ,PrincipalType
               ,Name
               ,Email
               ,DisplayName
               ,Description
               ,DataSource)
         VALUES
               (@ObjectId
               ,@PrincipalType
               ,@Name
               ,@Email
               ,ISNULL(@DisplayName, @Name)
               ,ISNULL(@Description, '''')
               ,@DataSource)
END' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[RemoveEdgeWithSpaceSavings]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROC [RemoveEdgeWithSpaceSavings]
   @StartVertexId varchar(256),
   @EndVertexId varchar(256),
   @Source varchar(150)
AS
BEGIN
   SET NOCOUNT ON
   DELETE Edge
       WHERE Hops = 0
         AND StartVertex = @StartVertexId
         AND EndVertex = @EndVertexId

   IF @@ROWCOUNT = 0
   BEGIN
      RETURN -- NOTHING TO DELETE
   END

   --UPDATE Edge SET DelMark = 0

   UPDATE Edge
        SET DelMark = 1
        FROM Edge 
            INNER JOIN (
                SELECT StartVertex
                     , @EndVertexId AS EndVertex
                      FROM Edge
                      WHERE EndVertex = @StartVertexId
                  UNION
                  SELECT @StartVertexId
                       , EndVertex AS EndVertex
                      FROM Edge
                      WHERE StartVertex = @EndVertexId
                  UNION
                  SELECT A.StartVertex 
                       , B.EndVertex
                      FROM Edge A
                         CROSS JOIN Edge B
                      WHERE A.EndVertex = @StartVertexId
                        AND B.StartVertex = @EndVertexId
                    ) AS C
               ON C.StartVertex = Edge.StartVertex
              AND C.EndVertex = Edge.EndVertex 
        WHERE Hops > 0;
        
   WITH SafeRows AS
        ( SELECT StartVertex, EndVertex FROM Edge WHERE DelMark = 0 )
   UPDATE Edge
        SET DelMark = 0
      FROM Edge
           INNER JOIN SafeRows S1
              ON S1.StartVertex = Edge.StartVertex
           INNER JOIN SafeRows S2 
              ON S1.EndVertex = S2.StartVertex
             AND S2.EndVertex = Edge.EndVertex
      WHERE DelMark = 1;

   WITH SafeRows AS
        ( SELECT StartVertex, EndVertex FROM Edge WHERE DelMark = 0 )
   UPDATE Edge
        SET DelMark = 0
      FROM Edge 
            INNER JOIN SafeRows	S1
               ON S1.StartVertex = Edge.StartVertex
            INNER JOIN SafeRows S2 
               ON S1.EndVertex = S2.StartVertex
            INNER JOIN SafeRows S3    
               ON S2.EndVertex = S3.StartVertex
              AND S3.EndVertex = Edge.EndVertex
      WHERE DelMark = 1
          
   DELETE Edge 
        WHERE DelMark = 1

END
' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[Test_TestClosureWithSpaceSavings]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROC [Test_TestClosureWithSpaceSavings]
AS
    SET NOCOUNT ON

    DELETE Edge WHERE Source = ''TestClosure''

	EXEC AddEdgeWithSpaceSavings ''A'', ''C'', ''TestClosure''
    EXEC AddEdgeWithSpaceSavings ''A'', ''D'', ''TestClosure''
    EXEC AddEdgeWithSpaceSavings ''B'', ''D'', ''TestClosure''
    EXEC AddEdgeWithSpaceSavings ''B'', ''M'', ''TestClosure''
    EXEC AddEdgeWithSpaceSavings ''C'', ''E'', ''TestClosure''
    EXEC AddEdgeWithSpaceSavings ''C'', ''H'', ''TestClosure''
    EXEC AddEdgeWithSpaceSavings ''C'', ''I'', ''TestClosure''
    EXEC AddEdgeWithSpaceSavings ''D'', ''F'', ''TestClosure''
    EXEC AddEdgeWithSpaceSavings ''D'', ''G'', ''TestClosure''
    EXEC AddEdgeWithSpaceSavings ''D'', ''H'', ''TestClosure''
    EXEC AddEdgeWithSpaceSavings ''I'', ''K'', ''TestClosure''
    EXEC AddEdgeWithSpaceSavings ''I'', ''J'', ''TestClosure''
    EXEC AddEdgeWithSpaceSavings ''A'', ''B'', ''TestClosure''
    EXEC AddEdgeWithSpaceSavings ''B'', ''E'', ''TestClosure''

    IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''F'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 1''
    IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''E'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 2''
    IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''C'' and EndVertex = ''K'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 3''
    IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''D'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 4''
    IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''F'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 5''
    IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''F'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 6''
    IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''F'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 7''
    IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''M'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 8''
    IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''B'' and EndVertex = ''H'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 9''

    EXEC RemoveEdgeWithSpaceSavings ''A'', ''B'', ''TestClosure''
    IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''E'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 10''
    IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''D'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 11''
    IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''H'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 12''
    IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''F'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 13''
    IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''G'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 14''
    IF     EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''M'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 15''

    EXEC RemoveEdgeWithSpaceSavings ''A'', ''D'', ''TestClosure''
    IF     EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''F'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 16''
    IF     EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''G'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 17''
    IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''H'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 18''


    EXEC RemoveEdgeWithSpaceSavings ''A'', ''C'', ''TestClosure''
    IF EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''E'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 19''
    IF EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''I'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 20''
    IF EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''K'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 21''


    EXEC AddEdgeWithSpaceSavings ''A'', ''C'', ''TestClosure''
    IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''I'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 22''
    IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''J'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 23''
    IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''K'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 24''
    IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''H'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 25''
    IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''E'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 26''

    EXEC AddEdgeWithSpaceSavings ''P'', ''Q'', ''TestClosure''
    EXEC AddEdgeWithSpaceSavings ''P'', ''R'', ''TestClosure''
    EXEC AddEdgeWithSpaceSavings ''Q'', ''S'', ''TestClosure''
    EXEC AddEdgeWithSpaceSavings ''Q'', ''T'', ''TestClosure''
    EXEC AddEdgeWithSpaceSavings ''R'', ''T'', ''TestClosure''
    EXEC AddEdgeWithSpaceSavings ''R'', ''T'', ''TestClosure''
    EXEC AddEdgeWithSpaceSavings ''H'', ''P'', ''TestClosure''
    IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''D'' and EndVertex = ''T'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 27''
    IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''S'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 28''
    IF     EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''I'' and EndVertex = ''Q'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 29''
    IF     EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''F'' and EndVertex = ''P'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 30''

    EXEC RemoveEdgeWithSpaceSavings ''Q'', ''T'', ''TestClosure''
    IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''T'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 31''
    IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''H'' and EndVertex = ''T'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 32''
    IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''P'' and EndVertex = ''T'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 33''

    
    -- RESTORE TEST HIERARCHY
    EXEC AddEdgeWithSpaceSavings ''Q'', ''T'', ''TestClosure''
    EXEC AddEdgeWithSpaceSavings ''A'', ''B'', ''TestClosure''
    EXEC AddEdgeWithSpaceSavings ''A'', ''C'', ''TestClosure''
    EXEC AddEdgeWithSpaceSavings ''A'', ''D'', ''TestClosure''

    EXEC RemoveEdgeWithSpaceSavings ''A'', ''D'', ''TestClosure''
	EXEC RemoveEdgeWithSpaceSavings ''B'', ''D'', ''TestClosure''
    IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''H'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 33A''

	EXEC RemoveEdgeWithSpaceSavings ''A'', ''C'', ''TestClosure''
    IF EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''H'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 33B''


    EXEC AddEdgeWithSpaceSavings ''A'', ''D'', ''TestClosure''
	EXEC AddEdgeWithSpaceSavings ''B'', ''D'', ''TestClosure''
	EXEC AddEdgeWithSpaceSavings ''A'', ''C'', ''TestClosure''


    EXEC RemoveEdgeWithSpaceSavings ''I'', ''K'', ''TestClosure''
    IF EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''K'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 34''


    EXEC AddEdgeWithSpaceSavings ''I'', ''K'', ''TestClosure''
    IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''K'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 35''

    EXEC RemoveEdgeWithSpaceSavings ''H'', ''P'', ''TestClosure''
    IF EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''Q'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 37''
    IF EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''B'' and EndVertex = ''Q'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 38''
    IF EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''C'' and EndVertex = ''Q'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 39''
    IF EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''D'' and EndVertex = ''Q'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 40''
    IF EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''T'')
	BEGIN
		 PRINT ''PROBLEM 41''
	END
	IF EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''Q'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 42''


    EXEC AddEdgeWithSpaceSavings ''H'', ''P'', ''TestClosure''
    IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''Q'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 43''
    IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''A'' and EndVertex = ''S'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 44''
	IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''B'' and EndVertex = ''T'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 45''
	IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''C'' and EndVertex = ''R'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 46''

    EXEC AddEdgeWithSpaceSavings ''X'', ''Y'', ''TestClosure''
    EXEC AddEdgeWithSpaceSavings ''X'', ''Z'', ''TestClosure''
    EXEC AddEdgeWithSpaceSavings ''Y'', ''Z'', ''TestClosure''
    EXEC RemoveEdgeWithSpaceSavings ''X'', ''Z'', ''TestClosure''
	IF NOT EXISTS (SELECT Hops FROM Edge WHERE StartVertex = ''X'' and EndVertex = ''Z'' AND Source = ''TestClosure'' ) PRINT''PROBLEM 47''


    PRINT ''TEST RUN COMPLETED...''' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[Sync_SyncEdge]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROC [Sync_SyncEdge] (
    @EdgeSource varchar(150),
    @PrincipalSource varchar(150)
)
AS
BEGIN
    -- new edges
    DECLARE NewEdgeList INSENSITIVE CURSOR FOR
        SELECT N.StartVertex, N.EndVertex
            FROM Sync_Edge N
                LEFT JOIN Edge O
                  ON N.StartVertex = O.StartVertex
                 AND N.EndVertex = O.EndVertex
                 AND O.Source = N.Source
             WHERE O.StartVertex IS NULL
               AND N.Source = @EdgeSource
             
    DECLARE @StartVertex varchar(256)
    DECLARE @EndVertex varchar(256)

    OPEN NewEdgeList
    FETCH NEXT FROM NewEdgeList INTO @StartVertex, @EndVertex
    WHILE @@FETCH_STATUS = 0 
    BEGIN
        EXEC AddEdgeWithSpaceSavings @StartVertex, @EndVertex, @EdgeSource
        FETCH NEXT FROM NewEdgeList INTO @StartVertex, @EndVertex
    END
    CLOSE NewEdgeList
    DEALLOCATE NewEdgeList
    
    -- removed edges
    DECLARE RemovedEdgeList INSENSITIVE CURSOR FOR
        SELECT O.StartVertex, O.EndVertex
            FROM Edge O
                LEFT JOIN Sync_Edge N
                  ON N.StartVertex = O.StartVertex
                 AND N.EndVertex = O.EndVertex
             WHERE N.StartVertex IS NULL
               AND O.Source = @EdgeSource
               AND O.StartVertex IN (
                                SELECT Name
                                    FROM Principal
                                    WHERE DataSource = @PrincipalSource)
               AND O.EndVertex IN (
                                SELECT Name
                                    FROM Principal
                                    WHERE DataSource = @PrincipalSource)                        

    OPEN RemovedEdgeList
    FETCH NEXT FROM RemovedEdgeList INTO @StartVertex, @EndVertex
    WHILE @@FETCH_STATUS = 0 
    BEGIN
        EXEC RemoveEdgeWithSpaceSavings @StartVertex, @EndVertex, @EdgeSource
        FETCH NEXT FROM RemovedEdgeList INTO @StartVertex, @EndVertex
    END
    CLOSE RemovedEdgeList
    DEALLOCATE RemovedEdgeList
END' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[RemoveRelatedEdges]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROC [RemoveRelatedEdges] (
    @Id varchar(256),
    @Source varchar(150)
)
AS
BEGIN
    DECLARE EdgesToBeRemoved CURSOR FOR
        SELECT StartVertex
              ,EndVertex
              ,Source
           FROM Edge
           WHERE Hops = 0
             AND Source = @Source
             AND (   StartVertex = @Id
                  OR EndVertex = @Id    ) 
                  
    DECLARE @StartVertex varchar(256)
    DECLARE @EndVertex varchar(256)
    
    OPEN EdgesToBeRemoved
    FETCH NEXT FROM EdgesToBeRemoved INTO @StartVertex, @Endvertex, @Source

    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC RemoveEdgeWithSpaceSavings @StartVertex, @EndVertex, @Source
        FETCH NEXT FROM EdgesToBeRemoved INTO @StartVertex, @Endvertex, @Source
    END
    CLOSE EdgesToBeRemoved
    DEALLOCATE EdgesToBeRemoved
END
    ' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[Test_Performance_RemoveEdge_RandomVertices]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROC [Test_Performance_RemoveEdge_RandomVertices] (
    @testCnt int )
AS
BEGIN
    DECLARE   @ii int
    DECLARE   @StartVertexId varchar(256)
    DECLARE   @EndVertexId varchar(256)
    DECLARE   @Source varchar(150)
    DECLARE   @rc int
    DECLARE   @totalTime bigint
    DECLARE   @avgTime int
    DECLARE   @start datetime

    SELECT @rc = count(*), @totalTime = 0 FROM Edge WHERE hops = 0


    SELECT @ii = 0
    WHILE @ii < @testCnt
    BEGIN
	    WITH EdgeWithId AS
	      ( select *, ROW_NUMBER() OVER (ORDER BY StartVertex) AS ROWID from edge where hops = 0 )
	    SELECT @StartVertexId = StartVertex,
			    @EndVertexId = EndVertex,
			    @Source = Source
	     FROM  EdgeWithId WHERE ROWID = convert(int, (rand()*100000))%@rc + 1
	     SELECT @start = GETDATE()
	     EXEC RemoveEdgeWithSpaceSavings @StartVertexId, @EndVertexId, @Source
	     SELECT @totalTime = @totalTime + DATEDIFF(ms, @start, GETDATE())
	     SELECT @ii = @ii + 1
    END

    PRINT ''AVG DELETION TIME (milliseconds) = '' + CONVERT( varchar(10), @totalTime/@testCnt )
END
' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[Test_Performance_RemoveEdge]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROC [Test_Performance_RemoveEdge] 
AS
BEGIN
    DECLARE   @StartVertexId varchar(256)
    DECLARE   @EndVertexId varchar(256)
    DECLARE   @Source varchar(150)
    DECLARE   @totalTime bigint
    DECLARE   @testCnt int
    DECLARE   @avgTime int
    DECLARE   @start datetime

    TRUNCATE TABLE Edge
    INSERT INTO Edge
        SELECT * FROM 
            TEST_EDGE_DATA
            
    DECLARE DelList CURSOR FOR
        SELECT
            StartVertex, EndVertex, Source
            FROM TEST_EDGE_DEL_DATA

    SELECT @totalTime = 0, @testCnt = COUNT(*) FROM TEST_EDGE_DEL_DATA

    OPEN DelList
    FETCH NEXT FROM DelList INTO @StartVertexId, @EndVertexId, @Source
    WHILE @@FETCH_STATUS = 0
    BEGIN
	     SELECT @start = GETDATE()
	     EXEC RemoveEdgeWithSpaceSavings @StartVertexId, @EndVertexId, @Source
	     SELECT @totalTime = @totalTime + DATEDIFF(ms, @start, GETDATE())
         FETCH NEXT FROM DelList INTO @StartVertexId, @EndVertexId, @Source
    END
    
    CLOSE DelList
    DEALLOCATE DelList

    PRINT ''AVG DELETION TIME (milliseconds) = '' + CONVERT( varchar(10), @totalTime/@testCnt )
END
' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[RemoveOperation]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROC [RemoveOperation] (
    @Id VARCHAR(10) )
AS
BEGIN
    DELETE Operation WHERE Id = @Id
    IF @@ROWCOUNT > 0
    BEGIN
        EXEC RemoveRelatedEdges @Id, ''Operation''
    END
END
' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[RemovePrincipal]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROC [RemovePrincipal] (
    @Name VARCHAR(256) )
AS
BEGIN
    DELETE Principal WHERE Name = @Name
    IF @@ROWCOUNT > 0
    BEGIN
        EXEC RemoveRelatedEdges @Name, ''Principal''
    END
END
    ' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[Sync_SyncPrincipal]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROC [Sync_SyncPrincipal] (
    @DataSource varchar(100)
)    
AS
BEGIN
    -- name changes
    CREATE TABLE #NameChanges (
        Id uniqueidentifier,
        OldName varchar(255),
        NewName varchar(255) )
        
    INSERT INTO #NameChanges
        SELECT O.ObjectId,
               O.Name,
               N.Name
            FROM Sync_Principal N
                INNER JOIN Principal O
                   ON N.ObjectId = O.ObjectId	    
            WHERE N.Name <> O.Name       
            
    UPDATE Principal
       SET PrincipalType = N.PrincipalType
          ,Name = N.Name
          ,EMail = N.Email
          ,DisplayName = N.DisplayName
          ,Description = N.Description
          ,DataSource = N.DataSource
        FROM Principal O
            INNER JOIN Sync_Principal N
               ON O.ObjectId = N.ObjectId
        WHERE O.PrincipalType <> N.PrincipalType
           OR O.Name <> N.Name
           OR O.EMail <> N.Email
           OR O.DisplayName <> N.DisplayName
           OR O.Description <> N.Description
           OR O.DataSource <> N.DataSource

    INSERT INTO Principal
           (ObjectId
           ,PrincipalType
           ,Name
           ,Email
           ,DisplayName
           ,Description
           ,DataSource )
       SELECT N.ObjectId
             ,N.PrincipalType
             ,N.Name
             ,N.Email
             ,N.DisplayName
             ,N.Description
             ,N.DataSource
        FROM Sync_Principal N
            LEFT JOIN Principal O
               ON N.ObjectId = O.ObjectId
        WHERE O.ObjectId IS NULL
        
     -- now handle name changes for Edge table
     DECLARE NameChanges CURSOR FOR
        SELECT Id, 
               OldName, 
               NewName   
            FROM #NameChanges

     DECLARE @Id uniqueidentifier
     DECLARE @OldName varchar(256)
     DECLARE @NewName varchar(256)
     
     OPEN NameChanges
     
     FETCH NEXT FROM NameChanges INTO @Id, @OldName, @NewName
     WHILE @@FETCH_STATUS = 0
     BEGIN
        UPDATE Edge 
            SET StartVertex = CASE WHEN StartVertex = @OldName THEN @NewName  ELSE StartVertex END
               ,EndVertex = CASE WHEN EndVertex = @OldName THEN @NewName  ELSE EndVertex END
            WHERE Source = ''Principal''
              AND @OldName IN ( EndVertex, StartVertex )
        FETCH NEXT FROM NameChanges INTO @Id, @OldName, @NewName
     END
     CLOSE NameChanges
     DEALLOCATE NameChanges
     DROP TABLE #NameChanges
     
     CREATE TABLE #Deleted (
        ObjectId uniqueidentifier,
        Name varchar(256) )
        
     
    -- now deleted Principals 
    INSERT INTO #Deleted ( Name )
    SELECT O.Name
    FROM Principal O
        LEFT JOIN Sync_Principal N
           ON N.ObjectId = O.ObjectId
    WHERE N.ObjectId IS NULL     

    DECLARE DeletedPrincipal CURSOR FOR
    SELECT Name
    FROM #Deleted   

    OPEN DeletedPrincipal

    FETCH NEXT FROM DeletedPrincipal INTO @OldName
    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC RemovePrincipal @OldName 
        FETCH NEXT FROM DeletedPrincipal INTO @OldName
    END
    CLOSE DeletedPrincipal
    DEALLOCATE DeletedPrincipal
    DROP TABLE #Deleted
END' 
END
GO
IF Not EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_AccessPredicate_ResourceId]') AND type = 'D')
BEGIN
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_AccessPredicate_ResourceId]') AND type = 'D')
BEGIN
ALTER TABLE [AccessPredicate] ADD  CONSTRAINT [DF_AccessPredicate_ResourceId]  DEFAULT ('') FOR [ResourceId]
END


END
GO
IF Not EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_AccessPredicate_PredicateType]') AND type = 'D')
BEGIN
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_AccessPredicate_PredicateType]') AND type = 'D')
BEGIN
ALTER TABLE [AccessPredicate] ADD  CONSTRAINT [DF_AccessPredicate_PredicateType]  DEFAULT ((0)) FOR [PredicateType]
END


END
GO
IF Not EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Edge_EdgeType]') AND type = 'D')
BEGIN
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Edge_EdgeType]') AND type = 'D')
BEGIN
ALTER TABLE [Edge] ADD  CONSTRAINT [DF_Edge_EdgeType]  DEFAULT ((0)) FOR [Hops]
END


END
GO
IF Not EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Edge_Source]') AND type = 'D')
BEGIN
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Edge_Source]') AND type = 'D')
BEGIN
ALTER TABLE [Edge] ADD  CONSTRAINT [DF_Edge_Source]  DEFAULT ('') FOR [Source]
END


END
GO
IF Not EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Edge_DelMark]') AND type = 'D')
BEGIN
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Edge_DelMark]') AND type = 'D')
BEGIN
ALTER TABLE [Edge] ADD  CONSTRAINT [DF_Edge_DelMark]  DEFAULT ((0)) FOR [DelMark]
END


END
GO
IF Not EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Operation_Name]') AND type = 'D')
BEGIN
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Operation_Name]') AND type = 'D')
BEGIN
ALTER TABLE [Operation] ADD  CONSTRAINT [DF_Operation_Name]  DEFAULT ('') FOR [Name]
END


END
GO
IF Not EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Operation_Description]') AND type = 'D')
BEGIN
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Operation_Description]') AND type = 'D')
BEGIN
ALTER TABLE [Operation] ADD  CONSTRAINT [DF_Operation_Description]  DEFAULT ('') FOR [Description]
END


END
GO
IF Not EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Principal_ObjectId]') AND type = 'D')
BEGIN
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Principal_ObjectId]') AND type = 'D')
BEGIN
ALTER TABLE [Principal] ADD  CONSTRAINT [DF_Principal_ObjectId]  DEFAULT (newid()) FOR [ObjectId]
END


END
GO
IF Not EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Principal_PrincipalType]') AND type = 'D')
BEGIN
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Principal_PrincipalType]') AND type = 'D')
BEGIN
ALTER TABLE [Principal] ADD  CONSTRAINT [DF_Principal_PrincipalType]  DEFAULT ((0)) FOR [PrincipalType]
END


END
GO
IF Not EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Principal_Name]') AND type = 'D')
BEGIN
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Principal_Name]') AND type = 'D')
BEGIN
ALTER TABLE [Principal] ADD  CONSTRAINT [DF_Principal_Name]  DEFAULT ('') FOR [DisplayName]
END


END
GO
IF Not EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Principal_Description]') AND type = 'D')
BEGIN
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Principal_Description]') AND type = 'D')
BEGIN
ALTER TABLE [Principal] ADD  CONSTRAINT [DF_Principal_Description]  DEFAULT ('') FOR [Description]
END


END
GO
IF Not EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Principal_DataSource]') AND type = 'D')
BEGIN
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Principal_DataSource]') AND type = 'D')
BEGIN
ALTER TABLE [Principal] ADD  CONSTRAINT [DF_Principal_DataSource]  DEFAULT ('') FOR [DataSource]
END


END
GO
IF Not EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__EdgeSyncL__Sourc__36F11965]') AND type = 'D')
BEGIN
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__EdgeSyncL__Sourc__36F11965]') AND type = 'D')
BEGIN
ALTER TABLE [Sync_Edge] ADD  DEFAULT ('') FOR [Source]
END


END
GO
IF Not EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__Principal__Princ__3D9E16F4]') AND type = 'D')
BEGIN
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__Principal__Princ__3D9E16F4]') AND type = 'D')
BEGIN
ALTER TABLE [Sync_Principal] ADD  DEFAULT ((0)) FOR [PrincipalType]
END


END
GO
IF Not EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__Principal__Displ__3E923B2D]') AND type = 'D')
BEGIN
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__Principal__Displ__3E923B2D]') AND type = 'D')
BEGIN
ALTER TABLE [Sync_Principal] ADD  DEFAULT ('') FOR [DisplayName]
END


END
GO
IF Not EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__Principal__Descr__3F865F66]') AND type = 'D')
BEGIN
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__Principal__Descr__3F865F66]') AND type = 'D')
BEGIN
ALTER TABLE [Sync_Principal] ADD  DEFAULT ('') FOR [Description]
END


END
GO
IF Not EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__Principal__DataS__407A839F]') AND type = 'D')
BEGIN
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__Principal__DataS__407A839F]') AND type = 'D')
BEGIN
ALTER TABLE [Sync_Principal] ADD  DEFAULT ('') FOR [DataSource]
END


END
GO
