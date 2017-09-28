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


#if DEBUG

namespace Afcas.Test {
    using System.Collections.Generic;
    using Base;
    using NUnit.Framework;
    using Objects;
    using Utils;

    [ TestFixture ]
    public class TestAuthorizationManager {
        [ TestFixtureSetUp ]
        public void Setup( ) {
            if( !Afcas.IsHandleFactoryRegistered( ( new SampleResourceHandleFactory( ) ).ResourceType ) ) {
                Afcas.RegisterHandleFactory( new SampleResourceHandleFactory( ) );
            }
        }

        private static void CreateTestOperations( ObjectCache cache ) {
            IAuthorizationManager manager = Afcas.GetAuthorizationManager( );
            using( new ObjectCacheScope( cache ) ) {
                Operation op1 = Operation.CreateOperation( "op1", "operation 1" );
                Operation op2 = Operation.CreateOperation( "op2", "operation 2" );

                op1.Description = "op1 desc";
                op2.Description = "op2 desc";

                manager.AddOrUpdate( op1 );
                manager.AddOrUpdate( op2 );
            }
        }

        private static void CreateTestPrincipals( ObjectCache cache ) {
            IAuthorizationManager manager = Afcas.GetAuthorizationManager( );

            using( new ObjectCacheScope( cache ) ) {
                Principal grp1 = Principal.CreatePrincipal( "grp1", "Group 1", PrincipalType.Group, "grp1@test.com" );
                Principal grp2 = Principal.CreatePrincipal( "grp2", "Group 2", PrincipalType.Group, "grp2@test.com" );
                Principal usr1 = Principal.CreatePrincipal( "usr1", "User 1", PrincipalType.User, "usr1@test.com" );
                Principal usr2 = Principal.CreatePrincipal( "usr2", "User 2", PrincipalType.User, "usr2@test.com" );

                grp1.Description = "grp1 desc";
                grp2.Description = "grp2 desc";
                usr1.Description = "usr1 desc";
                usr2.Description = "usr2 desc";

                Assert.That( ObjectCache.Current.GetAll< Principal >( ).Count == 4 );

                manager.AddOrUpdate( grp1, "" );
                manager.AddOrUpdate( grp2, "" );
                manager.AddOrUpdate( usr1, "" );
                manager.AddOrUpdate( usr2, "" );
            }
        }

        [ Test ]
        public void TestCreateAcl( ) {
            DBHelper.ExecuteNonQuery( "Test_DeleteAllData" );
            IAuthorizationManager manager = Afcas.GetAuthorizationManager( );
            ResourceHandleFactory fac = Afcas.GetHandleFactory( "SampleResource" );
            TestCreateDag( );
            using( new ObjectCacheScope( ) ) {
                manager.GetPrincipalList( );
                manager.GetOperationList( );

                Principal grp1 = ObjectCache.Current.Get< Principal >( "grp1" );
                //Principal grp2 = ObjectCache.Current.Get< Principal >( "grp2" );
                Principal usr1 = ObjectCache.Current.Get< Principal >( "usr1" );
                //Principal usr2 = ObjectCache.Current.Get< Principal >( "usr2" );

                Operation op1 = ObjectCache.Current.Get< Operation >( "op1" );
                Operation op2 = ObjectCache.Current.Get< Operation >( "op2" );
                //Operation op3 = ObjectCache.Current.Get< Operation >( "op2" );

                manager.AddAccessPredicate( grp1.Key,
                                            op1.Key,
                                            fac.GenerateResourceHandleByKey( "r1" ),
                                            ResourceAccessPredicateType.Grant );
                manager.AddAccessPredicate( grp1.Key,
                                            op1.Key,
                                            fac.GenerateResourceHandleByKey( "r2" ),
                                            ResourceAccessPredicateType.Grant );
                manager.AddAccessPredicate( grp1.Key, op1.Key, NullResource.Instance, ResourceAccessPredicateType.Grant );

                Assert.That( manager.IsAuthorized( grp1.Key, op1.Key, fac.GenerateResourceHandleByKey( "r1" ) ) );
                Assert.That( manager.IsAuthorized( grp1.Key, op2.Key, fac.GenerateResourceHandleByKey( "r1" ) ) );
                Assert.That( manager.IsAuthorized( usr1.Key, op2.Key, fac.GenerateResourceHandleByKey( "r1" ) ) );

                Assert.That( manager.IsAuthorized( usr1.Key, op2.Key, NullResource.Instance ) );
            }
        }

        [ Test ]
        public void TestCreateDag( ) {
            DBHelper.ExecuteNonQuery( "Test_DeleteAllData" );
            IAuthorizationManager manager = Afcas.GetAuthorizationManager( );
            using( new ObjectCacheScope( ) ) {
                CreateTestPrincipals( ObjectCache.Current );
                CreateTestOperations( ObjectCache.Current );

                Principal grp1 = ObjectCache.Current.Get< Principal >( "grp1" );
                Principal grp2 = ObjectCache.Current.Get< Principal >( "grp2" );
                Principal usr1 = ObjectCache.Current.Get< Principal >( "usr1" );
                Principal usr2 = ObjectCache.Current.Get< Principal >( "usr2" );

                manager.AddGroupMember( grp1, usr1 );
                manager.AddGroupMember( grp1, usr2 );
                manager.AddGroupMember( grp2, usr1 );
                manager.AddGroupMember( grp2, grp1 );

                Assert.That( manager.IsMemberOf( grp1.Key, usr1.Key ) );
                Assert.That( manager.IsMemberOf( grp1.Key, usr2.Key ) );
                Assert.That( manager.IsMemberOf( grp2.Key, usr1.Key ) );
                Assert.That( manager.IsMemberOf( grp2.Key, usr2.Key ) );
                Assert.That( manager.IsMemberOf( grp2.Key, grp1.Key ) );

                Assert.AreEqual( 2, manager.GetMembersList( grp1 ).Count );
                Assert.AreEqual( 2, manager.GetMembersList( grp2 ).Count );
                Assert.AreEqual( 3, manager.GetFlatMembersList( grp2 ).Count );

                Operation op1 = ObjectCache.Current.Get< Operation >( "op1" );
                Operation op2 = ObjectCache.Current.Get< Operation >( "op2" );
                Operation op3 = ObjectCache.Current.Get< Operation >( "op2" );

                manager.AddSubOperation( op1, op2 );
                manager.AddSubOperation( op1, op3 );

                Assert.That( manager.IsSubOperation( op1.Key, op2.Key ) );
                Assert.That( manager.IsSubOperation( op1.Key, op3.Key ) );
            }
        }

        [ Test ]
        public void TestCreateOperation( ) {
            DBHelper.ExecuteNonQuery( "Test_DeleteAllData" );
            IAuthorizationManager manager = Afcas.GetAuthorizationManager( );

            using( new ObjectCacheScope( ) ) {
                CreateTestOperations( ObjectCache.Current );

                IList< Operation > ol = manager.GetOperationList( );
                Assert.That( ol.Count == 2 );
                Operation op1 = ObjectCache.Current.Get< Operation >( "op1" );
                Operation op2 = ObjectCache.Current.Get< Operation >( "op2" );

                Assert.AreEqual( "operation 1", op1.Name );
                Assert.AreEqual( "operation 2", op2.Name );

                Assert.AreEqual( "op1 desc", op1.Description );
                Assert.AreEqual( "op2 desc", op2.Description );
            }
        }

        [ Test ]
        public void TestCreatePrincipal( ) {
            DBHelper.ExecuteNonQuery( "Test_DeleteAllData" );
            IAuthorizationManager manager = Afcas.GetAuthorizationManager( );

            using( new ObjectCacheScope( ) ) {
                CreateTestPrincipals( ObjectCache.Current );

                IList< Principal > pl = manager.GetPrincipalList( );
                Assert.That( pl.Count == 4 );
                Assert.That( ObjectCache.Current.GetAll< Principal >( ).Count == 4 );
            }

            using( new ObjectCacheScope( ) ) {
                IList< Principal > gl = manager.GetPrincipalList( PrincipalType.Group );
                IList< Principal > ul = manager.GetPrincipalList( PrincipalType.User );

                Assert.That( gl.Count == 2 );
                Assert.That( ul.Count == 2 );
                Assert.That( ObjectCache.Current.GetAll< Principal >( ).Count == 4 );


                Principal grp1 = ObjectCache.Current.Get< Principal >( "grp1" );
                Principal grp2 = ObjectCache.Current.Get< Principal >( "grp2" );
                Principal usr1 = ObjectCache.Current.Get< Principal >( "usr1" );
                Principal usr2 = ObjectCache.Current.Get< Principal >( "usr2" );

                Assert.That( grp1.Key == "grp1" );
                Assert.That( grp2.Key == "grp2" );
                Assert.That( usr1.Key == "usr1" );
                Assert.That( usr2.Key == "usr2" );

                Assert.That( grp1.PrincipalType == PrincipalType.Group );
                Assert.That( grp2.PrincipalType == PrincipalType.Group );
                Assert.That( usr1.PrincipalType == PrincipalType.User );
                Assert.That( usr2.PrincipalType == PrincipalType.User );

                Assert.That( grp1.Name == "Group 1" );
                Assert.That( grp2.Name == "Group 2" );
                Assert.That( usr1.Name == "User 1" );
                Assert.That( usr2.Name == "User 2" );

                Assert.That( grp1.EMail == "grp1@test.com" );
                Assert.That( grp2.EMail == "grp2@test.com" );
                Assert.That( usr1.EMail == "usr1@test.com" );
                Assert.That( usr2.EMail == "usr2@test.com" );

                Assert.That( grp1.Description == "grp1 desc" );
                Assert.That( grp2.Description == "grp2 desc" );
                Assert.That( usr1.Description == "usr1 desc" );
                Assert.That( usr2.Description == "usr2 desc" );
            }
        }

        [ Test ]
        public void TestDeleteOperation( ) {
            DBHelper.ExecuteNonQuery( "Test_DeleteAllData" );

            IAuthorizationManager manager = Afcas.GetAuthorizationManager( );
            using( new ObjectCacheScope( ) ) {
                Operation op1 = Operation.CreateOperation( "op1", "operation 1" );
                Operation op2 = Operation.CreateOperation( "op2", "operation 2" );
                Operation op3 = Operation.CreateOperation( "op3", "operation 3" );

                manager.AddOrUpdate( op1 );
                manager.AddOrUpdate( op2 );
                manager.AddOrUpdate( op3 );

                Assert.AreEqual( 3, manager.GetOperationList( ).Count );
                Assert.AreEqual( 3, ObjectCache.Current.GetAll< Operation >( ).Count );

                manager.RemoveOperation( op1.Key );

                Assert.AreEqual( 2, manager.GetOperationList( ).Count );
                Assert.AreEqual( 2, ObjectCache.Current.GetAll< Operation >( ).Count );
            }
        }

        [ Test ]
        public void TestDeletePrincipal( ) {
            DBHelper.ExecuteNonQuery( "Test_DeleteAllData" );

            IAuthorizationManager manager = Afcas.GetAuthorizationManager( );
            using( new ObjectCacheScope( ) ) {
                CreateTestPrincipals( ObjectCache.Current );
                manager.RemovePrincipal( "grp1" );

                Assert.AreEqual( 3, manager.GetPrincipalList( ).Count );
                Assert.AreEqual( 3, ObjectCache.Current.GetAll< Principal >( ).Count );
            }
        }

        [ Test ]
        public void TestUpdateOperation( ) {
            DBHelper.ExecuteNonQuery( "Test_DeleteAllData" );
            IAuthorizationManager manager = Afcas.GetAuthorizationManager( );
            using( new ObjectCacheScope( ) ) {
                CreateTestOperations( ObjectCache.Current );

                Operation op1 = ObjectCache.Current.Get< Operation >( "op1" );

                op1.Description = "new op1 desc";

                manager.GetOperationList( );

                Assert.AreEqual( "new op1 desc", op1.Description );
                manager.AddOrUpdate( op1 );
            }

            using( new ObjectCacheScope( ) ) {
                manager.GetOperationList( );
                Operation op1 = ObjectCache.Current.Get< Operation >( "op1" );
                Assert.AreEqual( "new op1 desc", op1.Description );
            }
        }

        [ Test ]
        public void TestUpdatePrincipal( ) {
            DBHelper.ExecuteNonQuery( "Test_DeleteAllData" );
            IAuthorizationManager manager = Afcas.GetAuthorizationManager( );
            using( new ObjectCacheScope( ) ) {
                CreateTestPrincipals( ObjectCache.Current );
                Principal grp1 = ObjectCache.Current.Get< Principal >( "grp1" );
                grp1.Description = "new grp1 desc";
                grp1.EMail = "newgrp1mail@test.com";

                manager.GetPrincipalList( PrincipalType.Group );
                Assert.AreEqual( 4, ObjectCache.Current.GetAll< Principal >( ).Count );

                Assert.AreEqual( grp1.Description, "new grp1 desc" );
                Assert.AreEqual( grp1.EMail, "newgrp1mail@test.com" );

                manager.AddOrUpdate( grp1, "" );
            }

            using( new ObjectCacheScope( ) ) {
                manager.GetPrincipalList( PrincipalType.Group );
                Principal grp1 = ObjectCache.Current.Get< Principal >( "grp1" );

                Assert.AreEqual( grp1.Description, "new grp1 desc" );
                Assert.AreEqual( grp1.EMail, "newgrp1mail@test.com" );
            }
        }
    }
}

#endif