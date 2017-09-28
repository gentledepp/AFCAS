#region copyright

// Copyright (C) 2008 Kemal ERDOGAN
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion


using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Dapper;

#if DEBUG

namespace Afcas.Test {
    using System.Collections.Generic;
    using Base;
    using NUnit.Framework;
    using Objects;
    using Utils;

    [TestFixture]
    public class TestAuthorizationProvider
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            if (!Afcas.IsHandleFactoryRegistered((new SampleResourceHandleFactory()).ResourceType))
            {
                Afcas.RegisterHandleFactory(new SampleResourceHandleFactory());
            }
        }

        private static ObjectCache SetupSimpleData()
        {
            DBHelper.ExecuteNonQuery("Test_DeleteAllData");
            IAuthorizationManager manager = Afcas.GetAuthorizationManager();
            ObjectCache.PushCurrent(new ObjectCache());

            Principal G = Principal.CreatePrincipal("G", "G", PrincipalType.Group, "");
            Principal G1 = Principal.CreatePrincipal("G1", "G1", PrincipalType.Group, "");
            Principal G2 = Principal.CreatePrincipal("G2", "G2", PrincipalType.Group, "");
            Principal U1 = Principal.CreatePrincipal("U1", "U1", PrincipalType.User, "");

            Operation O = Operation.CreateOperation("O", "O");
            Operation O1 = Operation.CreateOperation("O1", "O2");
            Operation O2 = Operation.CreateOperation("O2", "O2");
            Operation O3 = Operation.CreateOperation("O3", "O3");


            manager.AddOrUpdate(G, "");
            manager.AddOrUpdate(G1, "");
            manager.AddOrUpdate(G2, "");
            manager.AddOrUpdate(U1, "");
            manager.AddOrUpdate(O);
            manager.AddOrUpdate(O1);
            manager.AddOrUpdate(O2);
            manager.AddOrUpdate(O3);

            ResourceHandleFactory resFac = Afcas.GetHandleFactory("SampleResource");
            ResourceHandle R = resFac.GenerateResourceHandleByKey("R");
            ResourceHandle R1 = resFac.GenerateResourceHandleByKey("R1");
            ResourceHandle R2 = resFac.GenerateResourceHandleByKey("R2");


            manager.AddAccessPredicate(G.Key, O.Key, R, ResourceAccessPredicateType.Grant);
            manager.AddAccessPredicate(U1.Key, O.Key, R, ResourceAccessPredicateType.Grant);
            manager.AddAccessPredicate(U1.Key, O1.Key, R1, ResourceAccessPredicateType.Grant);

            manager.AddGroupMember(G, G1);
            manager.AddGroupMember(G, G2);
            manager.AddGroupMember(G, U1);

            manager.AddSubOperation(O, O1);
            manager.AddSubOperation(O, O2);
            manager.AddSubOperation(O, O3);


            manager.AddSubResource(R, R1);
            manager.AddSubResource(R, R2);
            return ObjectCache.Current;
        }

        private static ObjectCache SetupComplexData()
        {
            DBHelper.ExecuteNonQuery("Test_DeleteAllData");
            ObjectCache.PushCurrent(new ObjectCache());

            IAuthorizationManager manager = Afcas.GetAuthorizationManager();

            Principal PA = Principal.CreatePrincipal("PA", "PA", PrincipalType.Group, "");
            Principal PB = Principal.CreatePrincipal("PB", "PB", PrincipalType.Group, "");
            Principal PC = Principal.CreatePrincipal("PC", "PC", PrincipalType.Group, "");
            Principal PD = Principal.CreatePrincipal("PD", "PD", PrincipalType.Group, "");
            Principal PE = Principal.CreatePrincipal("PE", "PE", PrincipalType.User, "");
            Principal PF = Principal.CreatePrincipal("PF", "PF", PrincipalType.User, "");
            Principal PG = Principal.CreatePrincipal("PG", "PG", PrincipalType.User, "");
            Principal PH = Principal.CreatePrincipal("PH", "PH", PrincipalType.Group, "");
            Principal PI = Principal.CreatePrincipal("PI", "PI", PrincipalType.Group, "");
            Principal PJ = Principal.CreatePrincipal("PJ", "PJ", PrincipalType.User, "");
            Principal PK = Principal.CreatePrincipal("PK", "PK", PrincipalType.User, "");
            Principal PP = Principal.CreatePrincipal("PP", "PP", PrincipalType.Group, "");
            Principal PQ = Principal.CreatePrincipal("PQ", "PQ", PrincipalType.Group, "");
            Principal PR = Principal.CreatePrincipal("PR", "PR", PrincipalType.Group, "");
            Principal PS = Principal.CreatePrincipal("PS", "PS", PrincipalType.User, "");
            Principal PT = Principal.CreatePrincipal("PT", "PT", PrincipalType.User, "");
            manager.AddOrUpdate(PA, "");
            manager.AddOrUpdate(PB, "");
            manager.AddOrUpdate(PC, "");
            manager.AddOrUpdate(PD, "");
            manager.AddOrUpdate(PE, "");
            manager.AddOrUpdate(PF, "");
            manager.AddOrUpdate(PG, "");
            manager.AddOrUpdate(PH, "");
            manager.AddOrUpdate(PI, "");
            manager.AddOrUpdate(PJ, "");
            manager.AddOrUpdate(PK, "");
            manager.AddOrUpdate(PP, "");
            manager.AddOrUpdate(PQ, "");
            manager.AddOrUpdate(PR, "");
            manager.AddOrUpdate(PS, "");
            manager.AddOrUpdate(PT, "");

            manager.AddGroupMember(PA, PB);
            manager.AddGroupMember(PA, PC);
            manager.AddGroupMember(PA, PD);
            manager.AddGroupMember(PB, PD);
            manager.AddGroupMember(PB, PE);
            manager.AddGroupMember(PC, PE);
            manager.AddGroupMember(PC, PH);
            manager.AddGroupMember(PC, PI);
            manager.AddGroupMember(PD, PF);
            manager.AddGroupMember(PD, PG);
            manager.AddGroupMember(PD, PH);
            manager.AddGroupMember(PH, PP);
            manager.AddGroupMember(PI, PJ);
            manager.AddGroupMember(PI, PK);
            manager.AddGroupMember(PP, PQ);
            manager.AddGroupMember(PP, PR);
            manager.AddGroupMember(PQ, PS);
            manager.AddGroupMember(PQ, PT);
            manager.AddGroupMember(PR, PT);

            Operation OA = Operation.CreateOperation("OA", "OA");
            Operation OB = Operation.CreateOperation("OB", "OB");
            Operation OC = Operation.CreateOperation("OC", "OC");
            Operation OD = Operation.CreateOperation("OD", "OD");
            Operation OE = Operation.CreateOperation("OE", "OE");
            Operation OF = Operation.CreateOperation("OF", "OF");
            Operation OG = Operation.CreateOperation("OG", "OG");
            Operation OH = Operation.CreateOperation("OH", "OH");

            manager.AddSubOperation(OA, OB);
            manager.AddSubOperation(OA, OC);
            manager.AddSubOperation(OA, OD);
            manager.AddSubOperation(OE, OF);
            manager.AddSubOperation(OE, OG);
            manager.AddSubOperation(OH, OA);
            manager.AddSubOperation(OH, OE);

            ResourceHandleFactory resFac = Afcas.GetHandleFactory("SampleResource");

            ResourceHandle RA = resFac.GenerateResourceHandleByKey("RA");
            ResourceHandle RB = resFac.GenerateResourceHandleByKey("RB");
            ResourceHandle RC = resFac.GenerateResourceHandleByKey("RC");
            ResourceHandle RD = resFac.GenerateResourceHandleByKey("RD");
            ResourceHandle RE = resFac.GenerateResourceHandleByKey("RE");
            ResourceHandle RF = resFac.GenerateResourceHandleByKey("RF");
            ResourceHandle RG = resFac.GenerateResourceHandleByKey("RG");
            ResourceHandle RH = resFac.GenerateResourceHandleByKey("RH");
            ResourceHandle RI = resFac.GenerateResourceHandleByKey("RI");
            ResourceHandle RJ = resFac.GenerateResourceHandleByKey("RJ");
            ResourceHandle RK = resFac.GenerateResourceHandleByKey("RK");
            ResourceHandle RL = resFac.GenerateResourceHandleByKey("RL");

            manager.AddSubResource(RA, RB);
            manager.AddSubResource(RA, RC);
            manager.AddSubResource(RB, RD);
            manager.AddSubResource(RB, RE);
            manager.AddSubResource(RC, RE);
            manager.AddSubResource(RC, RF);
            manager.AddSubResource(RC, RG);
            manager.AddSubResource(RH, RI);
            manager.AddSubResource(RH, RJ);
            manager.AddSubResource(RH, RK);
            manager.AddSubResource(RL, RA);
            manager.AddSubResource(RL, RH);

            manager.AddAccessPredicate(PI.Key, OH.Key, RL, ResourceAccessPredicateType.Grant);
            manager.AddAccessPredicate(PD.Key, OA.Key, RA, ResourceAccessPredicateType.Grant);
            manager.AddAccessPredicate(PP.Key, OE.Key, RH, ResourceAccessPredicateType.Grant);

            manager.AddAccessPredicate(PA.Key, OG.Key, RL, ResourceAccessPredicateType.Grant);
            manager.AddAccessPredicate(PQ.Key, OB.Key, RL, ResourceAccessPredicateType.Grant);
            manager.AddAccessPredicate(PF.Key, OC.Key, RB, ResourceAccessPredicateType.Grant);
            return ObjectCache.Current;
        }

        
        [Test]
        public void TestComplexAuthorization()
        {
            SetupComplexData();
            IAuthorizationProvider provider = Afcas.GetAuthorizationProvider();

            Principal PA = ObjectCache.Current.Get<Principal>("PA");
            //Principal PB = ObjectCache.Current.Get< Principal >( "PB" );
            Principal PC = ObjectCache.Current.Get<Principal>("PC");
            Principal PD = ObjectCache.Current.Get<Principal>("PD");
            //Principal PE = ObjectCache.Current.Get< Principal >( "PE" );
            Principal PF = ObjectCache.Current.Get<Principal>("PF");
            //Principal PG = ObjectCache.Current.Get< Principal >( "PG" );
            //Principal PH = ObjectCache.Current.Get< Principal >( "PH" );
            Principal PI = ObjectCache.Current.Get<Principal>("PI");
            Principal PJ = ObjectCache.Current.Get<Principal>("PJ");
            Principal PK = ObjectCache.Current.Get<Principal>("PK");
            Principal PP = ObjectCache.Current.Get<Principal>("PP");
            Principal PQ = ObjectCache.Current.Get<Principal>("PQ");
            //Principal PR = ObjectCache.Current.Get< Principal >( "PR" );
            //Principal PS = ObjectCache.Current.Get< Principal >( "PS" );
            //Principal PT = ObjectCache.Current.Get< Principal >( "PT" );


            Operation OA = ObjectCache.Current.Get<Operation>("OA");
            Operation OB = ObjectCache.Current.Get<Operation>("OB");
            Operation OC = ObjectCache.Current.Get<Operation>("OC");
            Operation OD = ObjectCache.Current.Get<Operation>("OD");
            Operation OE = ObjectCache.Current.Get<Operation>("OE");
            Operation OF = ObjectCache.Current.Get<Operation>("OF");
            Operation OG = ObjectCache.Current.Get<Operation>("OG");
            Operation OH = ObjectCache.Current.Get<Operation>("OH");

            ResourceHandleFactory resFac = Afcas.GetHandleFactory("SampleResource");
            ResourceHandle RA = resFac.GenerateResourceHandleByKey("RA");
            ResourceHandle RB = resFac.GenerateResourceHandleByKey("RB");
            ResourceHandle RC = resFac.GenerateResourceHandleByKey("RC");
            //ResourceHandle RD = resFac.GenerateResourceHandleByKey( "RD" );
            //ResourceHandle RE = resFac.GenerateResourceHandleByKey( "RE" );
            //ResourceHandle RF = resFac.GenerateResourceHandleByKey( "RF" );
            //ResourceHandle RG = resFac.GenerateResourceHandleByKey( "RG" );
            ResourceHandle RH = resFac.GenerateResourceHandleByKey("RH");
            //ResourceHandle RI = resFac.GenerateResourceHandleByKey( "RI" );
            //ResourceHandle RJ = resFac.GenerateResourceHandleByKey( "RJ" );
            ResourceHandle RK = resFac.GenerateResourceHandleByKey("RK");
            ResourceHandle RL = resFac.GenerateResourceHandleByKey("RL");


            // explicit auth. list
            Assert.That(provider.IsAuthorized(PI.Key, OH.Key, RL));
            Assert.That(provider.IsAuthorized(PD.Key, OA.Key, RA));
            Assert.That(provider.IsAuthorized(PP.Key, OE.Key, RH));
            Assert.That(provider.IsAuthorized(PA.Key, OG.Key, RL));
            Assert.That(provider.IsAuthorized(PQ.Key, OB.Key, RL));
            Assert.That(provider.IsAuthorized(PF.Key, OC.Key, RB));


            //implied ones
            Assert.That(provider.IsAuthorized(PI.Key, OA.Key, RA));
            Assert.That(provider.IsAuthorized(PI.Key, OE.Key, RC));
            Assert.That(provider.IsAuthorized(PI.Key, OF.Key, RK));

            Assert.That(provider.IsAuthorized(PK.Key, OF.Key, RA));
            Assert.That(provider.IsAuthorized(PK.Key, OE.Key, RC));
            Assert.That(provider.IsAuthorized(PK.Key, OF.Key, RK));

            Assert.That(provider.IsAuthorized(PK.Key, OH.Key, RL));
            Assert.That(provider.IsAuthorized(PJ.Key, OD.Key, RH));
            Assert.That(provider.IsAuthorized(PP.Key, OB.Key, RA));


            Assert.That(!provider.IsAuthorized(PC.Key, OE.Key, RC));
            Assert.That(!provider.IsAuthorized(PP.Key, OB.Key, RL));
        }

        [Test]
        public void TestComplexHierarchy()
        {
            using (new ObjectCacheScope(SetupComplexData()))
            {
                IAuthorizationProvider provider = Afcas.GetAuthorizationProvider();
                Principal PA = ObjectCache.Current.Get<Principal>("PA");
                Principal PB = ObjectCache.Current.Get<Principal>("PB");
                Principal PI = ObjectCache.Current.Get<Principal>("PI");
                Principal PP = ObjectCache.Current.Get<Principal>("PP");
                Principal PT = ObjectCache.Current.Get<Principal>("PT");

                Assert.That(provider.IsMemberOf(PA.Key, PT.Key));
                Assert.That(provider.IsMemberOf(PB.Key, PP.Key));
                Assert.That(!provider.IsMemberOf(PB.Key, PI.Key));
            }
        }

        [Test]
        public void TestOffline()
        {
            using (new ObjectCacheScope(SetupSimpleData()))
            {
                IAuthorizationManager manager = Afcas.GetAuthorizationManager();
                manager.GetOperationList();

                Principal U1 = ObjectCache.Current.Get<Principal>("U1");
                IList<Operation> ol = manager.GetAuthorizedOperations(U1.Key, NullResource.Instance);
                Assert.AreEqual(0, ol.Count);

                ResourceHandleFactory resFac = Afcas.GetHandleFactory("SampleResource");
                SampleResource R = SampleResource.GetOrCreateSampleResource("R");

                ol = manager.GetAuthorizedOperations(U1.Key, resFac.GenerateResourceHandle(R));
                Assert.AreEqual(4, ol.Count);

                IList<ResourceAccessPredicate> acl = manager.GetAuthorizationDigest(U1.Key);
                Assert.AreEqual(12, acl.Count);
            }
        }

        [Test]
        public void TestSimpleAuthorization()
        {
            using (new ObjectCacheScope(SetupSimpleData()))
            {
                IAuthorizationProvider provider = Afcas.GetAuthorizationProvider();

                Principal G = ObjectCache.Current.Get<Principal>("G");
                Principal G2 = ObjectCache.Current.Get<Principal>("G2");
                Principal U1 = ObjectCache.Current.Get<Principal>("U1");

                Operation O = ObjectCache.Current.Get<Operation>("O");
                Operation O3 = ObjectCache.Current.Get<Operation>("O3");

                SampleResource R = SampleResource.GetOrCreateSampleResource("R");
                SampleResource R2 = SampleResource.GetOrCreateSampleResource("R2");
                ResourceHandleFactory resFac = Afcas.GetHandleFactory("SampleResource");

                Assert.That(provider.IsAuthorized(G.Key, O.Key, resFac.GenerateResourceHandle(R)),
                    "authorization must exist");
                Assert.That(provider.IsAuthorized(G2.Key, O3.Key, resFac.GenerateResourceHandle(R2)),
                    "authorization must exist");
                Assert.That(provider.IsAuthorized(U1.Key, O3.Key, resFac.GenerateResourceHandle(R2)),
                    "authorization must exist");
            }
        }

        [Test]
        public void TestSimpleHierarchy()
        {
            using (new ObjectCacheScope(SetupSimpleData()))
            {
                IAuthorizationProvider provider = Afcas.GetAuthorizationProvider();

                Principal G = ObjectCache.Current.Get<Principal>("G");
                Principal G1 = ObjectCache.Current.Get<Principal>("G1");

                Operation O = ObjectCache.Current.Get<Operation>("O");
                Operation O3 = ObjectCache.Current.Get<Operation>("O3");

                ResourceHandleFactory resFac = Afcas.GetHandleFactory("SampleResource");
                SampleResource R = SampleResource.GetOrCreateSampleResource("R");
                SampleResource R2 = SampleResource.GetOrCreateSampleResource("R2");

                Assert.That(provider.IsMemberOf(G.Key, G1.Key));
                Assert.That(provider.IsSubOperation(O.Key, O3.Key));
                Assert.That(!provider.IsSubOperation(O3.Key, O.Key));
                Assert.That(
                    !provider.IsSubResource(resFac.GenerateResourceHandle(R), resFac.GenerateResourceHandle(R2)));
            }
        }

        /*
-- -> 
        with 50 customers, that have 10000 equipments each, create
        creates 750070 edges 660776 KB in size (0.8 KB per edge!)
SELECT 
t.NAME AS TableName,
s.Name AS SchemaName,
p.rows AS RowCounts,
SUM(a.total_pages) * 8 AS TotalSpaceKB, 
CAST(ROUND(((SUM(a.total_pages) * 8) / 1024.00), 2) AS NUMERIC(36, 2)) AS TotalSpaceMB,
SUM(a.used_pages) * 8 AS UsedSpaceKB, 
CAST(ROUND(((SUM(a.used_pages) * 8) / 1024.00), 2) AS NUMERIC(36, 2)) AS UsedSpaceMB, 
(SUM(a.total_pages) - SUM(a.used_pages)) * 8 AS UnusedSpaceKB,
CAST(ROUND(((SUM(a.total_pages) - SUM(a.used_pages)) * 8) / 1024.00, 2) AS NUMERIC(36, 2)) AS UnusedSpaceMB
FROM 
sys.tables t
INNER JOIN      
sys.indexes i ON t.OBJECT_ID = i.object_id
INNER JOIN 
sys.partitions p ON i.object_id = p.OBJECT_ID AND i.index_id = p.index_id
INNER JOIN 
sys.allocation_units a ON p.partition_id = a.container_id
LEFT OUTER JOIN 
sys.schemas s ON t.schema_id = s.schema_id
WHERE 
t.NAME NOT LIKE 'dt%' 
AND t.is_ms_shipped = 0
AND i.OBJECT_ID > 255 
GROUP BY 
t.Name, s.Name, p.Rows
ORDER BY 
t.Name
         */
        [Test]
        public void TestHugeHierarchy()
        {
            var cache = SetupHugeTestData();
        }

        [Test]
        public void TestLongPath()
        {
            DBHelper.ExecuteNonQuery("Test_DeleteAllData");
            ObjectCache.PushCurrent(new ObjectCache());

            IAuthorizationManager manager = Afcas.GetAuthorizationManager();

            Principal PA = Principal.CreatePrincipal("PA", "PA", PrincipalType.Group, "");
            Principal PB = Principal.CreatePrincipal("PB", "PB", PrincipalType.Group, "");
            Principal PC = Principal.CreatePrincipal("PC", "PC", PrincipalType.Group, "");
            Principal PD = Principal.CreatePrincipal("PD", "PD", PrincipalType.Group, "");
            Principal PE = Principal.CreatePrincipal("PE", "PE", PrincipalType.User, "");
            Principal PF = Principal.CreatePrincipal("PF", "PF", PrincipalType.User, "");
            Principal PG = Principal.CreatePrincipal("PG", "PG", PrincipalType.User, "");
            Principal PH = Principal.CreatePrincipal("PH", "PH", PrincipalType.Group, "");
            Principal PI = Principal.CreatePrincipal("PI", "PI", PrincipalType.Group, "");
            Principal PJ = Principal.CreatePrincipal("PJ", "PJ", PrincipalType.User, "");
            Principal PK = Principal.CreatePrincipal("PK", "PK", PrincipalType.User, "");
            Principal PP = Principal.CreatePrincipal("PP", "PP", PrincipalType.Group, "");
            Principal PQ = Principal.CreatePrincipal("PQ", "PQ", PrincipalType.Group, "");
            Principal PR = Principal.CreatePrincipal("PR", "PR", PrincipalType.Group, "");
            Principal PS = Principal.CreatePrincipal("PS", "PS", PrincipalType.User, "");
            Principal PT = Principal.CreatePrincipal("PT", "PT", PrincipalType.User, "");
            manager.AddOrUpdate(PA, "");
            manager.AddOrUpdate(PB, "");
            manager.AddOrUpdate(PC, "");
            manager.AddOrUpdate(PD, "");
            manager.AddOrUpdate(PE, "");
            manager.AddOrUpdate(PF, "");
            manager.AddOrUpdate(PG, "");
            manager.AddOrUpdate(PH, "");
            manager.AddOrUpdate(PI, "");
            manager.AddOrUpdate(PJ, "");
            manager.AddOrUpdate(PK, "");
            manager.AddOrUpdate(PP, "");
            manager.AddOrUpdate(PQ, "");
            manager.AddOrUpdate(PR, "");
            manager.AddOrUpdate(PS, "");
            manager.AddOrUpdate(PT, "");

            manager.AddGroupMember(PA, PB);
            manager.AddGroupMember(PA, PC);
            manager.AddGroupMember(PA, PD);
            manager.AddGroupMember(PB, PD);
            manager.AddGroupMember(PB, PE);
            manager.AddGroupMember(PC, PE);
            manager.AddGroupMember(PC, PH);
            manager.AddGroupMember(PC, PI);
            manager.AddGroupMember(PD, PF);
            manager.AddGroupMember(PD, PG);
            manager.AddGroupMember(PD, PH);
            manager.AddGroupMember(PH, PP);
            manager.AddGroupMember(PI, PJ);
            manager.AddGroupMember(PI, PK);
            manager.AddGroupMember(PP, PQ);
            manager.AddGroupMember(PP, PR);
            manager.AddGroupMember(PQ, PS);
            manager.AddGroupMember(PQ, PT);
            manager.AddGroupMember(PR, PT);

            Operation OA = Operation.CreateOperation("OA", "OA");
            Operation OB = Operation.CreateOperation("OB", "OB");
            Operation OC = Operation.CreateOperation("OC", "OC");
            Operation OD = Operation.CreateOperation("OD", "OD");
            Operation OE = Operation.CreateOperation("OE", "OE");
            Operation OF = Operation.CreateOperation("OF", "OF");
            Operation OG = Operation.CreateOperation("OG", "OG");
            Operation OH = Operation.CreateOperation("OH", "OH");

            manager.AddSubOperation(OA, OB);
            manager.AddSubOperation(OA, OC);
            manager.AddSubOperation(OA, OD);
            manager.AddSubOperation(OE, OF);
            manager.AddSubOperation(OE, OG);
            manager.AddSubOperation(OH, OA);
            manager.AddSubOperation(OH, OE);

            ResourceHandleFactory resFac = Afcas.GetHandleFactory("SampleResource");
            List<ResourceHandle> devices;
            using (Track("creating devices"))
            {
                devices = Enumerable.Range(0, 500000).Select(i => resFac.GenerateResourceHandleByKey($"device {i + 1}")).ToList();
            }

            using (Track($"Creating long graph "))
            for (int i = 1; i < devices.Count; i++)
            {
                var parent = devices[0];
                var child = devices[i];
                //using (Track($"setting {parent.Key} -> {child.Key}"))
                {
                    manager.AddSubResource(parent, child);
                }
            }

            // six permissions per customer
            Console.WriteLine(" ");
            using (Track($"Adding permissions to customers "))
            {
                var device = devices[0];
                manager.AddAccessPredicate(PI.Key, OH.Key, device, ResourceAccessPredicateType.Grant);
                manager.AddAccessPredicate(PD.Key, OA.Key, device, ResourceAccessPredicateType.Grant);
                manager.AddAccessPredicate(PP.Key, OE.Key, device, ResourceAccessPredicateType.Grant);

                manager.AddAccessPredicate(PA.Key, OG.Key, device, ResourceAccessPredicateType.Grant);
                manager.AddAccessPredicate(PQ.Key, OB.Key, device, ResourceAccessPredicateType.Grant);
                manager.AddAccessPredicate(PF.Key, OC.Key, device, ResourceAccessPredicateType.Grant);
            }
        }


        private ObjectCache SetupHugeTestData()
        {
            DBHelper.ExecuteNonQuery("Test_DeleteAllData");
            ObjectCache.PushCurrent(new ObjectCache());

            IAuthorizationManager manager = Afcas.GetAuthorizationManager();

            Principal PA = Principal.CreatePrincipal("PA", "PA", PrincipalType.Group, "");
            Principal PB = Principal.CreatePrincipal("PB", "PB", PrincipalType.Group, "");
            Principal PC = Principal.CreatePrincipal("PC", "PC", PrincipalType.Group, "");
            Principal PD = Principal.CreatePrincipal("PD", "PD", PrincipalType.Group, "");
            Principal PE = Principal.CreatePrincipal("PE", "PE", PrincipalType.User, "");
            Principal PF = Principal.CreatePrincipal("PF", "PF", PrincipalType.User, "");
            Principal PG = Principal.CreatePrincipal("PG", "PG", PrincipalType.User, "");
            Principal PH = Principal.CreatePrincipal("PH", "PH", PrincipalType.Group, "");
            Principal PI = Principal.CreatePrincipal("PI", "PI", PrincipalType.Group, "");
            Principal PJ = Principal.CreatePrincipal("PJ", "PJ", PrincipalType.User, "");
            Principal PK = Principal.CreatePrincipal("PK", "PK", PrincipalType.User, "");
            Principal PP = Principal.CreatePrincipal("PP", "PP", PrincipalType.Group, "");
            Principal PQ = Principal.CreatePrincipal("PQ", "PQ", PrincipalType.Group, "");
            Principal PR = Principal.CreatePrincipal("PR", "PR", PrincipalType.Group, "");
            Principal PS = Principal.CreatePrincipal("PS", "PS", PrincipalType.User, "");
            Principal PT = Principal.CreatePrincipal("PT", "PT", PrincipalType.User, "");
            manager.AddOrUpdate(PA, "");
            manager.AddOrUpdate(PB, "");
            manager.AddOrUpdate(PC, "");
            manager.AddOrUpdate(PD, "");
            manager.AddOrUpdate(PE, "");
            manager.AddOrUpdate(PF, "");
            manager.AddOrUpdate(PG, "");
            manager.AddOrUpdate(PH, "");
            manager.AddOrUpdate(PI, "");
            manager.AddOrUpdate(PJ, "");
            manager.AddOrUpdate(PK, "");
            manager.AddOrUpdate(PP, "");
            manager.AddOrUpdate(PQ, "");
            manager.AddOrUpdate(PR, "");
            manager.AddOrUpdate(PS, "");
            manager.AddOrUpdate(PT, "");

            manager.AddGroupMember(PA, PB);
            manager.AddGroupMember(PA, PC);
            manager.AddGroupMember(PA, PD);
            manager.AddGroupMember(PB, PD);
            manager.AddGroupMember(PB, PE);
            manager.AddGroupMember(PC, PE);
            manager.AddGroupMember(PC, PH);
            manager.AddGroupMember(PC, PI);
            manager.AddGroupMember(PD, PF);
            manager.AddGroupMember(PD, PG);
            manager.AddGroupMember(PD, PH);
            manager.AddGroupMember(PH, PP);
            manager.AddGroupMember(PI, PJ);
            manager.AddGroupMember(PI, PK);
            manager.AddGroupMember(PP, PQ);
            manager.AddGroupMember(PP, PR);
            manager.AddGroupMember(PQ, PS);
            manager.AddGroupMember(PQ, PT);
            manager.AddGroupMember(PR, PT);

            Operation OA = Operation.CreateOperation("OA", "OA");
            Operation OB = Operation.CreateOperation("OB", "OB");
            Operation OC = Operation.CreateOperation("OC", "OC");
            Operation OD = Operation.CreateOperation("OD", "OD");
            Operation OE = Operation.CreateOperation("OE", "OE");
            Operation OF = Operation.CreateOperation("OF", "OF");
            Operation OG = Operation.CreateOperation("OG", "OG");
            Operation OH = Operation.CreateOperation("OH", "OH");

            manager.AddSubOperation(OA, OB);
            manager.AddSubOperation(OA, OC);
            manager.AddSubOperation(OA, OD);
            manager.AddSubOperation(OE, OF);
            manager.AddSubOperation(OE, OG);
            manager.AddSubOperation(OH, OA);
            manager.AddSubOperation(OH, OE);

            ResourceHandleFactory resFac = Afcas.GetHandleFactory("SampleResource");

            //ResourceHandle RA = resFac.GenerateResourceHandleByKey("RA");
            //ResourceHandle RB = resFac.GenerateResourceHandleByKey("RB");
            //ResourceHandle RC = resFac.GenerateResourceHandleByKey("RC");
            //ResourceHandle RD = resFac.GenerateResourceHandleByKey("RD");
            //ResourceHandle RE = resFac.GenerateResourceHandleByKey("RE");
            //ResourceHandle RF = resFac.GenerateResourceHandleByKey("RF");
            //ResourceHandle RG = resFac.GenerateResourceHandleByKey("RG");
            //ResourceHandle RH = resFac.GenerateResourceHandleByKey("RH");
            //ResourceHandle RI = resFac.GenerateResourceHandleByKey("RI");
            //ResourceHandle RJ = resFac.GenerateResourceHandleByKey("RJ");
            //ResourceHandle RK = resFac.GenerateResourceHandleByKey("RK");
            //ResourceHandle RL = resFac.GenerateResourceHandleByKey("RL");

            //manager.AddSubResource(RA, RB);
            //manager.AddSubResource(RA, RC);
            //manager.AddSubResource(RB, RD);
            //manager.AddSubResource(RB, RE);
            //manager.AddSubResource(RC, RE);
            //manager.AddSubResource(RC, RF);
            //manager.AddSubResource(RC, RG);
            //manager.AddSubResource(RH, RI);
            //manager.AddSubResource(RH, RJ);
            //manager.AddSubResource(RH, RK);
            //manager.AddSubResource(RL, RA);
            //manager.AddSubResource(RL, RH);
            var customers = new List<ResourceHandle>();

            int customerCount = 5;
            int deviceCount = 250000;

            using(Track($"Creating 50 customers "))
            for (int i = 0; i < customerCount; i++)
            {
                var customer = resFac.GenerateResourceHandleByKey($"customer {i+1}");

                    // create equipments per customer
                using (Track($"\tCreating {deviceCount} device for {customer.Key} "))
                {
                    for (int j = 0; j < deviceCount / 3; j++)
                    {
                        var device = resFac.GenerateResourceHandleByKey($"device {j + 1} ({customer.Key})");
                        manager.AddSubResource(customer, device);

                        var subDevice = resFac.GenerateResourceHandleByKey($"subdevice {j + 1} of {device.Key} ({customer.Key})");
                        manager.AddSubResource(device, subDevice);
                            
                        var subSubDevice = resFac.GenerateResourceHandleByKey($"subSubdevice {j + 1} of {subDevice.Key} ({customer.Key})");
                        manager.AddSubResource(subDevice, subSubDevice);
                    }
                }
            }

            // create messmittel
            Console.WriteLine(" ");
            using (Track($"Creating 300 messmittel "))
            for (int i = 0; i < 3000; i++)
            {
                var measurementDevice = resFac.GenerateResourceHandleByKey($"messgerät {i + 1}");

                manager.AddAccessPredicate(PI.Key, OH.Key, measurementDevice, ResourceAccessPredicateType.Grant);
                manager.AddAccessPredicate(PD.Key, OA.Key, measurementDevice, ResourceAccessPredicateType.Grant);
                manager.AddAccessPredicate(PP.Key, OE.Key, measurementDevice, ResourceAccessPredicateType.Grant);

                manager.AddAccessPredicate(PA.Key, OG.Key, measurementDevice, ResourceAccessPredicateType.Grant);
                manager.AddAccessPredicate(PQ.Key, OB.Key, measurementDevice, ResourceAccessPredicateType.Grant);
                manager.AddAccessPredicate(PF.Key, OC.Key, measurementDevice, ResourceAccessPredicateType.Grant);
            }


            // six permissions per customer
            Console.WriteLine(" ");
            using (Track($"Adding permissions to customers "))
            foreach (var customer in customers)
            {
                using (Track($"\tAdding permissions on customer {customer.Key} "))
                {
                    manager.AddAccessPredicate(PI.Key, OH.Key, customer, ResourceAccessPredicateType.Grant);
                    manager.AddAccessPredicate(PD.Key, OA.Key, customer, ResourceAccessPredicateType.Grant);
                    manager.AddAccessPredicate(PP.Key, OE.Key, customer, ResourceAccessPredicateType.Grant);

                    manager.AddAccessPredicate(PA.Key, OG.Key, customer, ResourceAccessPredicateType.Grant);
                    manager.AddAccessPredicate(PQ.Key, OB.Key, customer, ResourceAccessPredicateType.Grant);
                    manager.AddAccessPredicate(PF.Key, OC.Key, customer, ResourceAccessPredicateType.Grant);
                }
            }
            return ObjectCache.Current;
        }

        private IDisposable Track(string name)
        {
            return new Tracker(name);
        }

        private class Tracker : IDisposable
        {
            private Stopwatch _sw;
            private string _name;

            public Tracker(string name)
            {
                _sw = new Stopwatch();
                _sw.Start();
                _name = name;
            }

            public void Dispose()
            {
                _sw.Stop();
                Console.WriteLine($"{_name} took {_sw.Elapsed}");
            }
        }
    }
}

#endif