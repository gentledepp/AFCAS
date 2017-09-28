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

namespace ADSync.Test {
    using System;
    using System.Collections.Generic;
    using Afcas;
    using Afcas.Base;
    using Afcas.Objects;
    using Afcas.Properties;
    using Afcas.Utils;
    using NUnit.Framework;
    using Objects;
    using Utils;

    [ TestFixture ]
    public class TestADSync {
        private static IList< ADPrincipal > GetPrincipalList( ) {
            ADPrincipal p1 = new ADPrincipal( Guid.NewGuid( ).ToString( ),
                                              "p1",
                                              "cn=p1,dc=test,dc=com",
                                              PrincipalType.User,
                                              "p1@test.com",
                                              "p1 disp name",
                                              "p1 desc",
                                              new[ ] { "cn=g1,dc=test,dc=com", "cn=g2,dc=test,dc=com" } );

            ADPrincipal p2 = new ADPrincipal( Guid.NewGuid( ).ToString( ),
                                              "p2",
                                              "cn=p2,dc=test,dc=com",
                                              PrincipalType.User,
                                              "p2@test.com",
                                              "p2 disp name",
                                              "p2 desc",
                                              new[ ] { "cn=g1,dc=test,dc=com" } );


            ADPrincipal p3 = new ADPrincipal( Guid.NewGuid( ).ToString( ),
                                              "p3",
                                              "cn=p3,dc=test,dc=com",
                                              PrincipalType.User,
                                              "p3@test.com",
                                              "p3 disp name",
                                              "p3 desc",
                                              new[ ] { "cn=g2,dc=test,dc=com" } );


            ADPrincipal g1 = new ADPrincipal( Guid.NewGuid( ).ToString( ),
                                              "g1",
                                              "cn=g1,dc=test,dc=com",
                                              PrincipalType.User,
                                              "g1@test.com",
                                              "g1 disp name",
                                              "g1 desc",
                                              new[ ] { "cn=g2,dc=test,dc=com" } );

            ADPrincipal g2 = new ADPrincipal( Guid.NewGuid( ).ToString( ),
                                              "g2",
                                              "cn=g2,dc=test,dc=com",
                                              PrincipalType.User,
                                              "g2@test.com",
                                              "g2 disp name",
                                              "g2 desc",
                                              new string[ ] { } );

            return new List< ADPrincipal >( new[ ] { p1, p2, p3, g1, g2 } );
        }

        private static bool IsInList( string name, IList< Principal > pl ) {
            for( int ii = 0; ii < pl.Count; ii++ ) {
                Principal pr = pl[ ii ];
                if( pr.Name == name ) {
                    return true;
                }
            }
            return false;
        }

        [ Test ]
        public void TestSync( ) {
            DBHelper.Init( Settings.Default.ConnectionString );
            DBHelper.ExecuteNonQuery( "Test_DeleteAllData" );

            IList< ADPrincipal > pl = GetPrincipalList( );
            ADSyncHelper.SyncAdToSqlServer( pl, Settings.Default.ConnectionString, "TestAD" );

            IAuthorizationManager manager = Afcas.GetAuthorizationManager( );

            using( new ObjectCacheScope( ) ) {
                Assert.AreEqual( 5, manager.GetPrincipalList( ).Count );

                //removal from sql side should not matter
                manager.RemovePrincipal( "p1" );
                ADSyncHelper.SyncAdToSqlServer( pl, Settings.Default.ConnectionString, "TestAD" );
                Assert.AreEqual( 5, manager.GetPrincipalList( ).Count );
            }

            using( new ObjectCacheScope( ) ) {
                //removal from ad side
                ADPrincipal p1 = pl[ 0 ];
                pl.RemoveAt( 0 );
                ADSyncHelper.SyncAdToSqlServer( pl, Settings.Default.ConnectionString, "TestAD" );

                IList< Principal > dbpl = manager.GetPrincipalList( );
                Assert.AreEqual( 4, dbpl.Count );
                Assert.That( !IsInList( p1.Name, dbpl ) );

                Assert.That( !manager.IsMemberOf( "g1", "p1" ) );
            }
        }
    }
}

#endif