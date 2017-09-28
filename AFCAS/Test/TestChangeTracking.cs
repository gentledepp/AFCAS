using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Afcas.Base;
using Afcas.Objects;
using Afcas.Properties;
using Afcas.Utils;
using Dapper;
using NUnit.Framework;

namespace Afcas.Test
{
    [TestFixture]
    public class TestChangeTracking
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            if (!Afcas.IsHandleFactoryRegistered((new SampleResourceHandleFactory()).ResourceType))
            {
                Afcas.RegisterHandleFactory(new SampleResourceHandleFactory());
            }
        }

        [Test]
        public async Task WhenAddingUserToGroup_TracksChanges()
        {
            DBHelper.ExecuteNonQuery("Test_DeleteAllData");
            IAuthorizationManager manager = Afcas.GetAuthorizationManager();
            ObjectCache.PushCurrent(new ObjectCache());

            Principal G = Principal.CreatePrincipal("G", "G", PrincipalType.Group, "");
            Principal U1 = Principal.CreatePrincipal("U1", "U1", PrincipalType.User, "");

            Operation O = Operation.CreateOperation("O", "O");

            manager.AddOrUpdate(G, "");
            manager.AddOrUpdate(U1, "");
            manager.AddOrUpdate(O);

            ResourceHandleFactory resFac = Afcas.GetHandleFactory("SampleResource");
            ResourceHandle R = resFac.GenerateResourceHandleByKey("R");
            ResourceHandle R1 = resFac.GenerateResourceHandleByKey("R1");
            ResourceHandle R2 = resFac.GenerateResourceHandleByKey("R2");

            manager.AddAccessPredicate(G.Key, O.Key, R, ResourceAccessPredicateType.Grant);

            Assert.IsFalse(manager.IsAuthorized(U1.Key, O.Key, R));
            Assert.IsTrue(manager.IsAuthorized(G.Key, O.Key, R));

            var rows = GetAuthRows();
            //manager.AddSubResource(R, R1);
            //manager.AddSubResource(R, R2);

            var lastChange = rows.Max(r => r.Modified);

            // Act
            manager.AddGroupMember(G, U1);

            var rows2 = GetAuthRows(lastChange).Where(r => r.PrincipalId == "U1").ToList();

            var max2 = rows2.Max(r => r.Modified);

            // Assert
            //Assert.IsTrue(manager.IsAuthorized(U1.Key, O.Key, R));
            Assert.GreaterOrEqual(max2, lastChange, $"{lastChange} should be lower!");
        }

        [Test]
        public async Task WhenAddingUserToGroup_OfGroup_TracksChanges()
        {
            DBHelper.ExecuteNonQuery("Test_DeleteAllData");
            IAuthorizationManager manager = Afcas.GetAuthorizationManager();
            ObjectCache.PushCurrent(new ObjectCache());

            Principal G = Principal.CreatePrincipal("G", "G", PrincipalType.Group, "");
            Principal G1 = Principal.CreatePrincipal("G1", "G1", PrincipalType.Group, "");
            Principal U1 = Principal.CreatePrincipal("U1", "U1", PrincipalType.User, "");

            Operation O = Operation.CreateOperation("O", "O");

            manager.AddOrUpdate(G, "");
            manager.AddOrUpdate(G1, "");
            manager.AddOrUpdate(U1, "");
            manager.AddOrUpdate(O);

            manager.AddGroupMember(G1, U1);

            ResourceHandleFactory resFac = Afcas.GetHandleFactory("SampleResource");
            ResourceHandle R = resFac.GenerateResourceHandleByKey("R");
            ResourceHandle R1 = resFac.GenerateResourceHandleByKey("R1");
            ResourceHandle R2 = resFac.GenerateResourceHandleByKey("R2");

            manager.AddAccessPredicate(G.Key, O.Key, R, ResourceAccessPredicateType.Grant);

            Assert.IsFalse(manager.IsAuthorized(U1.Key, O.Key, R));
            Assert.IsTrue(manager.IsAuthorized(G.Key, O.Key, R));

            var rows = GetAuthRows();
            //manager.AddSubResource(R, R1);
            //manager.AddSubResource(R, R2);

            var lastChange = rows.Max(r => r.Modified);

            // Act
            manager.AddGroupMember(G, G1);

            var rows2 = GetAuthRows(lastChange).Where(r => r.PrincipalId == "U1").ToList();

            var max2 = rows2.Max(r => r.Modified);

            // Assert
            //Assert.IsTrue(manager.IsAuthorized(U1.Key, O.Key, R));
            Assert.GreaterOrEqual(max2, lastChange, $"{lastChange} should be lower!");
        }
        
        [Test]
        public async Task WhenSubResourceAdded_TracksChanges()
        {
            DBHelper.ExecuteNonQuery("Test_DeleteAllData");
            IAuthorizationManager manager = Afcas.GetAuthorizationManager();
            ObjectCache.PushCurrent(new ObjectCache());

            Principal G = Principal.CreatePrincipal("G", "G", PrincipalType.Group, "");
            Principal U1 = Principal.CreatePrincipal("U1", "U1", PrincipalType.User, "");

            Operation O = Operation.CreateOperation("O", "O");

            manager.AddOrUpdate(G, "");
            manager.AddOrUpdate(U1, "");
            manager.AddOrUpdate(O);

            manager.AddGroupMember(G, U1);

            ResourceHandleFactory resFac = Afcas.GetHandleFactory("SampleResource");
            ResourceHandle R = resFac.GenerateResourceHandleByKey("R");
            ResourceHandle R1 = resFac.GenerateResourceHandleByKey("R1");
            ResourceHandle R21 = resFac.GenerateResourceHandleByKey("R2.1");

            manager.AddAccessPredicate(G.Key, O.Key, R, ResourceAccessPredicateType.Grant);

            Assert.IsTrue(manager.IsAuthorized(U1.Key, O.Key, R));
            Assert.IsTrue(manager.IsAuthorized(G.Key, O.Key, R));

            var rows = GetAuthRows();
            var lastChange = rows.Max(r => r.Modified);

            // Act
            await Task.Delay(250);
            manager.AddSubResource(R, R1);
            manager.AddSubResource(R1, R21);

            
            var rows2 = GetAuthRows(lastChange).Where(r => r.PrincipalId == "U1" && r.ResourceId == R21.AfcasKey).ToList();
            var max2 = rows2.Max(r => r.Modified);

            // Assert
            //Assert.IsTrue(manager.IsAuthorized(U1.Key, O.Key, R));
            Assert.GreaterOrEqual(max2, lastChange, $"{lastChange} should be lower!");
        }
        
        [Test]
        public async Task WhenSubResourceDeleted_TracksChanges()
        {
            DBHelper.ExecuteNonQuery("Test_DeleteAllData");
            IAuthorizationManager manager = Afcas.GetAuthorizationManager();
            ObjectCache.PushCurrent(new ObjectCache());

            Principal G = Principal.CreatePrincipal("G", "G", PrincipalType.Group, "");
            Principal U1 = Principal.CreatePrincipal("U1", "U1", PrincipalType.User, "");

            Operation O = Operation.CreateOperation("O", "O");

            manager.AddOrUpdate(G, "");
            manager.AddOrUpdate(U1, "");
            manager.AddOrUpdate(O);

            manager.AddGroupMember(G, U1);

            ResourceHandleFactory resFac = Afcas.GetHandleFactory("SampleResource");
            ResourceHandle R = resFac.GenerateResourceHandleByKey("R");
            ResourceHandle R1 = resFac.GenerateResourceHandleByKey("R1");
            ResourceHandle R21 = resFac.GenerateResourceHandleByKey("R2.1");

            manager.AddSubResource(R, R1);
            manager.AddSubResource(R1, R21);

            manager.AddAccessPredicate(G.Key, O.Key, R, ResourceAccessPredicateType.Grant);

            Assert.IsTrue(manager.IsAuthorized(U1.Key, O.Key, R));
            Assert.IsTrue(manager.IsAuthorized(G.Key, O.Key, R));

            var rows = GetAuthRows();
            var lastChange = rows.Max(r => r.Modified);

            // Act
            await Task.Delay(250);
            manager.RemoveSubResource(R1, R21);

            var rows2 = GetAuthRows().Where(r => r.PrincipalId == "U1" && r.ResourceId == R21.AfcasKey).ToList();
            var deletedRows = GetDeletedAuthRows(lastChange).Where(r => r.PrincipalId == "U1" && r.ResourceId == R21.AfcasKey).ToList();

            // Assert
            Assert.AreEqual(0, rows2.Count, "should have lost permission");
            Assert.AreEqual(1, deletedRows.Count);
            Assert.GreaterOrEqual(deletedRows[0].Deleted, lastChange, "deletion must have happened after last change!");
        }

        [Test]
        public async Task WhenRemovedFromGroup_TracksChanges()
        {
            DBHelper.ExecuteNonQuery("Test_DeleteAllData");
            IAuthorizationManager manager = Afcas.GetAuthorizationManager();
            ObjectCache.PushCurrent(new ObjectCache());

            Principal G = Principal.CreatePrincipal("G", "G", PrincipalType.Group, "");
            Principal U1 = Principal.CreatePrincipal("U1", "U1", PrincipalType.User, "");

            Operation O = Operation.CreateOperation("O", "O");

            manager.AddOrUpdate(G, "");
            manager.AddOrUpdate(U1, "");
            manager.AddOrUpdate(O);

            manager.AddGroupMember(G, U1);

            ResourceHandleFactory resFac = Afcas.GetHandleFactory("SampleResource");
            ResourceHandle R = resFac.GenerateResourceHandleByKey("R");
            ResourceHandle R1 = resFac.GenerateResourceHandleByKey("R1");
            ResourceHandle R21 = resFac.GenerateResourceHandleByKey("R2.1");

            manager.AddSubResource(R, R1);
            manager.AddSubResource(R1, R21);

            manager.AddAccessPredicate(G.Key, O.Key, R, ResourceAccessPredicateType.Grant);

            Assert.IsTrue(manager.IsAuthorized(U1.Key, O.Key, R));
            Assert.IsTrue(manager.IsAuthorized(G.Key, O.Key, R));

            var rows = GetAuthRows();
            var lastChange = rows.Max(r => r.Modified);

            // Act
            await Task.Delay(250);
            manager.RemoveGroupMember(G, U1);

            var rows2 = GetAuthRows().Where(r => r.PrincipalId == "U1" && r.ResourceId == R21.AfcasKey).ToList();
            var deletedRows = GetDeletedAuthRows(lastChange).Where(r => r.PrincipalId == "U1" && r.ResourceId == R21.AfcasKey).ToList();
            var dr = GetDeletedAuthRows();

            // Assert
            Assert.AreEqual(0, rows2.Count, "should have lost permission");
            Assert.AreEqual(1, deletedRows.Count);
            Assert.GreaterOrEqual(deletedRows[0].Deleted, lastChange, "deletion must have happened after last change!");
        }


        [Test]
        public async Task WhenRemovingAccessPredicate_TracksChanges()
        {
            DBHelper.ExecuteNonQuery("Test_DeleteAllData");
            IAuthorizationManager manager = Afcas.GetAuthorizationManager();
            ObjectCache.PushCurrent(new ObjectCache());

            Principal G = Principal.CreatePrincipal("G", "G", PrincipalType.Group, "");
            Principal U1 = Principal.CreatePrincipal("U1", "U1", PrincipalType.User, "");

            Operation O = Operation.CreateOperation("O", "O");

            manager.AddOrUpdate(G, "");
            manager.AddOrUpdate(U1, "");
            manager.AddOrUpdate(O);

            manager.AddGroupMember(G, U1);

            ResourceHandleFactory resFac = Afcas.GetHandleFactory("SampleResource");
            ResourceHandle R = resFac.GenerateResourceHandleByKey("R");
            ResourceHandle R1 = resFac.GenerateResourceHandleByKey("R1");
            ResourceHandle R21 = resFac.GenerateResourceHandleByKey("R2.1");

            manager.AddSubResource(R, R1);
            manager.AddSubResource(R1, R21);

            manager.AddAccessPredicate(G.Key, O.Key, R, ResourceAccessPredicateType.Grant);

            Assert.IsTrue(manager.IsAuthorized(U1.Key, O.Key, R));
            Assert.IsTrue(manager.IsAuthorized(G.Key, O.Key, R));

            var rows = GetAuthRows();
            var lastChange = rows.Max(r => r.Modified);

            // Act
            await Task.Delay(250);
            manager.RemoveAccessPredicate(G.Key, O.Key, R, ResourceAccessPredicateType.Grant);

            var rows2 = GetAuthRows().Where(r => r.PrincipalId == "U1" && r.ResourceId == R21.AfcasKey).ToList();
            var deletedRows = GetDeletedAuthRows(lastChange).Where(r => r.PrincipalId == "U1" && r.ResourceId == R21.AfcasKey).ToList();
            var dr = GetDeletedAuthRows();

            // Assert
            Assert.AreEqual(0, rows2.Count, "should have lost permission");
            Assert.AreEqual(1, deletedRows.Count);
            Assert.GreaterOrEqual(deletedRows[0].Deleted, lastChange, "deletion must have happened after last change!");
        }


        private static IEnumerable<FlatGrantListRow> GetAuthRows()
        {
            var rows = DBHelper.RunInTransaction(conn =>
            {
                return conn.Query<FlatGrantListRow>(@" SELECT DISTINCT
        PL.StartVertex AS PrincipalId
       ,ACL.OperationId
       ,RL.StartVertex AS ResourceId
	   ,case when PL.Modified > RL.Modified then PL.Modified else RL.Modified end as Modified
	   ,case when (PL.Deleted is not null and rl.Deleted is null) or (PL.Deleted > RL.Deleted) then PL.Deleted else RL.Deleted end as Deleted
	FROM ( SELECT E.EndVertex, E.StartVertex, E.Modified, E.Deleted
            FROM Edge E
                INNER JOIN AccessPredicate AP
                   ON E.EndVertex = AP.PrincipalId
                WHERE E.Source = 'Principal' and E.Deleted is null
            UNION 
            SELECT PrincipalId, PrincipalId, Modified, Deleted
            FROM AccessPredicate) PL
            CROSS JOIN (
                SELECT E.EndVertex, E.StartVertex, E.Modified, E.Deleted
                FROM Edge E
                    INNER JOIN AccessPredicate AP
                       ON E.EndVertex = AP.ResourceId
                WHERE E.Source = 'Resource'  and E.Deleted is null
                UNION 
                SELECT ResourceId, ResourceId, Modified, Deleted
                FROM AccessPredicate) RL
            INNER JOIN AccessPredicate ACL 
               ON PL.EndVertex = ACL.PrincipalId
              AND RL.EndVertex = ACL.ResourceId");
            });
            return rows;
        }

        private static IEnumerable<FlatGrantListRow> GetAuthRows(DateTime max)
        {
            var rows2 = DBHelper.RunInTransaction(conn =>
            {
                return conn.Query<FlatGrantListRow>(@" SELECT DISTINCT
        PL.StartVertex AS PrincipalId
       ,ACL.OperationId
       ,RL.StartVertex AS ResourceId
	   ,case when PL.Modified > RL.Modified then PL.Modified else RL.Modified end as Modified
	   ,case when (PL.Deleted is not null and rl.Deleted is null) or (PL.Deleted > RL.Deleted) then PL.Deleted else RL.Deleted end as Deleted
	FROM ( SELECT E.EndVertex, E.StartVertex, E.Modified, E.Deleted
            FROM Edge E
                INNER JOIN AccessPredicate AP
                   ON E.EndVertex = AP.PrincipalId
                WHERE E.Source = 'Principal' and E.Deleted is null
            UNION 
            SELECT PrincipalId, PrincipalId, Modified, Deleted
            FROM AccessPredicate) PL
            CROSS JOIN (
                SELECT E.EndVertex, E.StartVertex, E.Modified, E.Deleted
                FROM Edge E
                    INNER JOIN AccessPredicate AP
                       ON E.EndVertex = AP.ResourceId
                WHERE E.Source = 'Resource'  and E.Deleted is null
                UNION 
                SELECT ResourceId, ResourceId, Modified, Deleted
                FROM AccessPredicate) RL
            INNER JOIN AccessPredicate ACL 
               ON PL.EndVertex = ACL.PrincipalId
              AND RL.EndVertex = ACL.ResourceId
            WHERE PL.Modified > @modified or RL.Modified > @modified
                  or PL.Deleted > @modified or RL.Deleted > @modified", new { modified = max });
            });
            return rows2;
        }

        private static IEnumerable<FlatGrantListRow> GetDeletedAuthRows()
        {
            var rows = DBHelper.RunInTransaction(conn =>
            {
                return conn.Query<FlatGrantListRow>(@" SELECT DISTINCT
        PL.StartVertex AS PrincipalId
       ,ACL.OperationId
       ,RL.StartVertex AS ResourceId
	   ,case when PL.Modified > RL.Modified then PL.Modified else RL.Modified end as Modified
	   ,case when (PL.Deleted is not null and rl.Deleted is null) or (PL.Deleted > RL.Deleted) then PL.Deleted else RL.Deleted end as Deleted
	FROM ( SELECT E.EndVertex, E.StartVertex, E.Modified, E.Deleted
            FROM Edge E
                INNER JOIN AccessPredicate AP
                   ON E.EndVertex = AP.PrincipalId
                WHERE E.Source = 'Principal'
            UNION 
            SELECT PrincipalId, PrincipalId, Modified, Deleted
            FROM AccessPredicate) PL
            CROSS JOIN (
                SELECT E.EndVertex, E.StartVertex, E.Modified, E.Deleted
                FROM Edge E
                    INNER JOIN AccessPredicate AP
                       ON E.EndVertex = AP.ResourceId
                WHERE E.Source = 'Resource'
                UNION 
                SELECT ResourceId, ResourceId, Modified, Deleted
                FROM AccessPredicate) RL
            INNER JOIN AccessPredicate ACL 
               ON PL.EndVertex = ACL.PrincipalId
              AND RL.EndVertex = ACL.ResourceId
                where PL.Deleted is not null or RL.Deleted is not null");
            });
            return rows;
        }

        private static IEnumerable<FlatGrantListRow> GetDeletedAuthRows(DateTime max)
        {
            var rows = DBHelper.RunInTransaction(conn =>
            {
                return conn.Query<FlatGrantListRow>(@" SELECT DISTINCT
        PL.StartVertex AS PrincipalId
       ,ACL.OperationId
       ,RL.StartVertex AS ResourceId
	   ,case when PL.Modified > RL.Modified then PL.Modified else RL.Modified end as Modified
	   ,case when (PL.Deleted is not null and rl.Deleted is null) or (PL.Deleted > RL.Deleted) then PL.Deleted else RL.Deleted end as Deleted
	FROM ( SELECT E.EndVertex, E.StartVertex, E.Modified, E.Deleted
            FROM Edge E
                INNER JOIN AccessPredicate AP
                   ON E.EndVertex = AP.PrincipalId
                WHERE E.Source = 'Principal'
            UNION 
            SELECT PrincipalId, PrincipalId, Modified, Deleted
            FROM AccessPredicate) PL
            CROSS JOIN (
                SELECT E.EndVertex, E.StartVertex, E.Modified, E.Deleted
                FROM Edge E
                    INNER JOIN AccessPredicate AP
                       ON E.EndVertex = AP.ResourceId
                WHERE E.Source = 'Resource'
                UNION 
                SELECT ResourceId, ResourceId, Modified, Deleted
                FROM AccessPredicate) RL
            INNER JOIN AccessPredicate ACL 
               ON PL.EndVertex = ACL.PrincipalId
              AND RL.EndVertex = ACL.ResourceId
            WHERE (PL.Deleted is not null or RL.Deleted is not null)
                  and (PL.Deleted > @modified or RL.Deleted > @modified)", new { modified = max });
            });
            return rows;
        }

        public class FlatGrantListRow
        {
            public string PrincipalId { get; set; }
            public string OperationId { get; set; }
            public string ResourceId { get; set; }
            public DateTime Modified { get; set; }
            public DateTime Deleted { get; set; }
        }

    }
}
