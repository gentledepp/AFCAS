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

namespace ADSync.Utils {
    using System;
    using System.Collections.Generic;
    using System.Transactions;
    using Afcas.Objects;
    using Afcas.Utils;
    using Objects;

    internal static class ADSyncHelper {
        public static void SyncAdToSqlServer( IList< ADPrincipal > pl, string connectionString, string dbSourceName ) {
            Dictionary< string, ADPrincipal > pdic = new Dictionary< string, ADPrincipal >( pl.Count );
            for( int ii = 0; ii < pl.Count; ii++ ) {
                ADPrincipal pr = pl[ ii ];
                pdic[ pr.ADPath ] = pr;
            }

            DBHelper.Init( connectionString );
            using( TransactionScope scope = DBHelper.GetRequiredTransactionScope( ) ) {
                DBHelper.ExecuteNonQuery( "Sync_ClearSyncData", dbSourceName, EdgeSource.Principal );

                // update principals
                for( int ii = 0; ii < pl.Count; ii++ ) {
                    ADPrincipal pr = pl[ ii ];
                    DBHelper.ExecuteNonQuery( "Sync_AddPrincipalToSyncList",
                                              new Guid( pr.Id ),
                                              pr.PrincipalType,
                                              pr.Name,
                                              pr.Email,
                                              pr.DisplayName,
                                              pr.Description,
                                              dbSourceName );
                }
                DBHelper.ExecuteNonQuery( "Sync_SyncPrincipal", dbSourceName );

                // update memberships
                for( int ii = 0; ii < pl.Count; ii++ ) {
                    ADPrincipal pr = pl[ ii ];

                    for( int jj = 0; jj < pr.GroupPaths.Length; jj++ ) {
                        string path = pr.GroupPaths[ jj ];
                        ADPrincipal gr;
                        if( !pdic.TryGetValue( path, out gr ) ) {
                            // this group is out of scope from the path list
                            continue;
                        }
                        DBHelper.ExecuteNonQuery( "Sync_AddEdgeToSyncList", pr.Name, gr.Name, EdgeSource.Principal );
                    }
                }
                DBHelper.ExecuteNonQuery( "Sync_SyncEdge", 0, EdgeSource.Principal, dbSourceName );

                scope.Complete( );
            }
        }
    }
}