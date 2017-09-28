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

namespace Afcas.Impl {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Base;
    using Objects;
    using Properties;
    using Utils;

    internal class AuthorizationManager: AuthorizationProvider, IAuthorizationManager {
        internal AuthorizationManager( ): base( Settings.Default.DefaultAuthCacheDurationInSeconds ) {}

        private static IList< Principal > BuildPrincipalList( DataSet ds ) {
            if( ds.Tables.Count == 0 || ds.Tables[ 0 ].Rows.Count == 0 ) {
                return new Principal[0];
            }

            DataTable tbl = ds.Tables[ 0 ];
            List< Principal > res = new List< Principal >( tbl.Rows.Count );
            for( int ii = 0; ii < tbl.Rows.Count; ii++ ) {
                DataRow row = tbl.Rows[ ii ];
                string key = ( string )row[ 0 ];
                Principal pr = ObjectCache.Current.Get< Principal >( key ) ?? ConstructPrincipal( row );
                res.Add( pr );
            }
            return res;
        }

        private static IList< Operation > BuildOperationList( DataSet ds ) {
            if( ds.Tables.Count == 0 || ds.Tables[ 0 ].Rows.Count == 0 ) {
                return new Operation[0];
            }

            DataTable tbl = ds.Tables[ 0 ];
            List< Operation > res = new List< Operation >( tbl.Rows.Count );
            for( int ii = 0; ii < tbl.Rows.Count; ii++ ) {
                DataRow row = tbl.Rows[ ii ];
                string key = ( string )row[ 0 ];
                Operation op = ObjectCache.Current.Get< Operation >( key ) ?? ConstructOperation( row );
                res.Add( op );
            }
            return res;
        }

        private static Principal ConstructPrincipal( DataRow row ) {
            Principal pr = Principal.CreatePrincipal( ( string )row[ 0 ],
                                                      ( string )row[ 1 ],
                                                      ( PrincipalType )( short )row[ 2 ],
                                                      ( string )row[ 3 ] );
            pr.Description = ( string )row[ 4 ];
            return pr;
        }

        private static Operation ConstructOperation( DataRow row ) {
            Operation op = Operation.CreateOperation( ( string )row[ 0 ], ( string )row[ 1 ] );
            op.Description = ( string )row[ 2 ];
            return op;
        }

        #region Implementation of IAuthorizationManager

        public void AddOrUpdate( Principal pr, string source ) {
            ObjectCache.Current.PutOrUpdate( pr );
            DBHelper.ExecuteNonQuery( "AddOrUpdatePrincipal", pr.Key, ( int )pr.PrincipalType, pr.Name, pr.EMail, pr.Description );
        }

        public void RemovePrincipal( string id ) {
            ObjectCache.Current.Remove< Principal >( id );
            DBHelper.ExecuteNonQuery( "RemovePrincipal", id );
        }

        public void AddOrUpdate( Operation op ) {
            ObjectCache.Current.PutOrUpdate( op );
            DBHelper.ExecuteNonQuery( "AddOrUpdateOperation", op.Key, op.Name, op.Description );
        }

        public void RemoveOperation( string id ) {
            ObjectCache.Current.Remove< Operation >( id );
            DBHelper.ExecuteNonQuery( "RemoveOperation", id );
        }

        public void AddAccessPredicate( string principalId,
                                        string operationId,
                                        ResourceHandle resource,
                                        ResourceAccessPredicateType type ) {
            if( type != ResourceAccessPredicateType.Grant ) {
                throw new ArgumentException( "Explicit denials are not supported!");
            }
            DBHelper.ExecuteNonQuery( "AddAccessPredicate", principalId, operationId, resource.AfcasKey, ( int )type );
        }

        public void RemoveAccessPredicate( string principalId,
                                           string operationId,
                                           ResourceHandle resource,
                                           ResourceAccessPredicateType type ) {
            DBHelper.ExecuteNonQuery( "RemoveAccessPredicate", principalId, operationId, resource.AfcasKey, ( int )type );
        }

        public void AddGroupMember( Principal group, Principal member ) {
            if( group.PrincipalType != PrincipalType.Group ) {
                throw new ArgumentException( "Only groups may have members" );
            }

            DBHelper.ExecuteNonQuery( "AddEdgeWithSpaceSavings", member.Key, group.Key, EdgeSource.Principal );
        }

        public void RemoveGroupMember( Principal group, Principal member ) {
            DBHelper.ExecuteNonQuery( "RemoveEdgeWithSpaceSavings", member.Key, group.Key );
        }

        public void AddSubOperation( Operation parent, Operation sub ) {
            DBHelper.ExecuteNonQuery( "AddEdgeWithSpaceSavings", sub.Key, parent.Key, EdgeSource.Operation );
        }

        public void RemoveSubOperation( Operation parent, Operation sub ) {
            DBHelper.ExecuteNonQuery( "RemoveEdgeWithSpaceSavings", sub.Key, parent.Key );
        }

        public void AddSubResource( ResourceHandle resource, ResourceHandle subResource ) {
            DBHelper.ExecuteNonQuery( "AddEdgeWithSpaceSavings", subResource.AfcasKey, resource.AfcasKey, EdgeSource.Resource );
        }

        public void RemoveSubResource( ResourceHandle resource, ResourceHandle subResource ) {
            DBHelper.ExecuteNonQuery( "RemoveEdgeWithSpaceSavings", subResource.AfcasKey, resource.AfcasKey, EdgeSource.Resource );
        }

        public IList< Principal > GetPrincipalList( ) {
            return BuildPrincipalList( DBHelper.ExecuteDataSet( "GetPrincipalList" ) );
        }

        public IList< Principal > GetPrincipalList( PrincipalType type ) {
            return BuildPrincipalList( DBHelper.ExecuteDataSet( "GetPrincipalList", ( int )type ) );
        }

        public IList< Principal > GetMembersList( Principal pr ) {
            return BuildPrincipalList( DBHelper.ExecuteDataSet( "GetMembersList", pr.Key, 0 ) );
        }

        public IList< Principal > GetFlatMembersList( Principal pr ) {
            return BuildPrincipalList( DBHelper.ExecuteDataSet( "GetMembersList", pr.Key, 1 ) );
        }

        public IList< Operation > GetOperationList( ) {
            return BuildOperationList( DBHelper.ExecuteDataSet( "GetOperationList" ) );
        }

        public IList< Operation > GetSubOperationsList( Operation op ) {
            return BuildOperationList( DBHelper.ExecuteDataSet( "GetSubOperationsList", op.Key, 0 ) );
        }

        public IList< Operation > GetFlatSubOperationsList( Operation op ) {
            return BuildOperationList( DBHelper.ExecuteDataSet( "GetSubOperationsList", op.Key, 1 ) );
        }

        #endregion
    }
}