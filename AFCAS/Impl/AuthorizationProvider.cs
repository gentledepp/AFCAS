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
    using System.Text;
    using System.Web;
    using System.Web.Caching;
    using Base;
    using Objects;
    using Utils;

    internal class AuthorizationProvider: IAuthorizationProvider {
        private const string _CacheKeyPrefix = "AuthorizationProvider.";
        private readonly uint _CacheDurationInSeconds;


        public AuthorizationProvider( uint cacheDurationInSeconds ) {
            _CacheDurationInSeconds = cacheDurationInSeconds;
        }


        public uint CacheDurationInSeconds {
            get {
                return _CacheDurationInSeconds;
            }
        }

        #region IAuthorizationProvider Members

        public bool IsAuthorized( string principalId, string operationId, ResourceHandle resource ) {
            if( string.IsNullOrEmpty( principalId ) ) {
                throw new ArgumentNullException( "principalId" );
            }

            if( string.IsNullOrEmpty( operationId ) ) {
                throw new ArgumentNullException( "operationId" );
            }

            if( resource == null ) {
                throw new ArgumentNullException( "resource" );
            }

            return ( bool )ExecuteMethodWithCacheLookUp( "IsAuthorized",
                                                         delegate( string[ ] parameters ) {
                                                             return DBHelper.ExecuteScalar( "IsAuthorized", parameters );
                                                         },
                                                         principalId,
                                                         operationId,
                                                         resource.AfcasKey );
        }

        public bool IsMemberOf( string groupId, string memberId ) {
            if( string.IsNullOrEmpty( groupId ) ) {
                throw new ArgumentNullException( "groupId" );
            }

            if( string.IsNullOrEmpty( memberId ) ) {
                throw new ArgumentNullException( "memberId" );
            }

            return ( bool )ExecuteMethodWithCacheLookUp( "EdgeExists",
                                                         delegate( string[ ] parameters ) {
                                                             return DBHelper.ExecuteScalar( "EdgeExists", parameters );
                                                         },
                                                         memberId,
                                                         groupId,
                                                         EdgeSource.Principal );
        }

        public bool IsSubOperation( string operationId, string subOperationId ) {
            if( string.IsNullOrEmpty( operationId ) ) {
                throw new ArgumentNullException( "operationId" );
            }

            if( string.IsNullOrEmpty( subOperationId ) ) {
                throw new ArgumentNullException( "subOperationId" );
            }

            return ( bool )ExecuteMethodWithCacheLookUp( "EdgeExists",
                                                         delegate( string[ ] parameters ) {
                                                             return DBHelper.ExecuteScalar( "EdgeExists", parameters );
                                                         },
                                                         subOperationId,
                                                         operationId,
                                                         EdgeSource.Operation );
        }

        public bool IsSubResource( ResourceHandle resource, ResourceHandle subResource ) {
            if( resource == null ) {
                throw new ArgumentNullException( "resource" );
            }

            if( subResource == null ) {
                throw new ArgumentNullException( "subResource" );
            }

            return ( bool )ExecuteMethodWithCacheLookUp( "EdgeExists",
                                                         delegate( string[ ] parameters ) {
                                                             return DBHelper.ExecuteScalar( "EdgeExists", parameters );
                                                         },
                                                         resource.AfcasKey,
                                                         subResource.AfcasKey,
                                                         EdgeSource.Resource );
        }

        public IList< ResourceAccessPredicate > GetAuthorizationDigest( string principalId ) {
            if( string.IsNullOrEmpty( principalId ) ) {
                throw new ArgumentNullException( "principalId" );
            }

            return
                    ( IList< ResourceAccessPredicate > )
                    ExecuteMethodWithCacheLookUp( "GetAuthorizationDigest",
                                                  delegate( string[ ] parameters ) {
                                                      DataSet ds = DBHelper.ExecuteDataSet( "GetAuthorizationDigest", parameters );
                                                      if( ds.Tables.Count == 0 || ds.Tables[ 0 ].Rows.Count == 0 ) {
                                                          return new ResourceAccessPredicate[0];
                                                      }
                                                      DataTable tb = ds.Tables[ 0 ];
                                                      List< ResourceAccessPredicate > res =
                                                              new List< ResourceAccessPredicate >( tb.Rows.Count );
                                                      for( int ii = 0; ii < tb.Rows.Count; ii++ ) {
                                                          DataRow row = tb.Rows[ ii ];
                                                          res.Add( new ResourceAccessPredicate( parameters[ 0 ],
                                                                                                ( string )row[ 0 ],
                                                                                                ResourceHandle.GetResourceHandle(
                                                                                                        ( string )row[ 1 ] ),
                                                                                                ( ResourceAccessPredicateType )
                                                                                                ( ( int )row[ 2 ] ) ) );
                                                      }
                                                      return res;
                                                  },
                                                  principalId );
        }

        public IList< ResourceHandle > GetAuthorizedResources( string principalId, string operationId ) {
            if( string.IsNullOrEmpty( principalId ) ) {
                throw new ArgumentNullException( "principalId" );
            }

            if( string.IsNullOrEmpty( operationId ) ) {
                throw new ArgumentNullException( "operationId" );
            }

            return
                    ( IList< ResourceHandle > )
                    ExecuteMethodWithCacheLookUp( "GetAuthorizedResources",
                                                  delegate( string[ ] parameters ) {
                                                      DataSet ds = DBHelper.ExecuteDataSet( "GetAuthorizedResources", parameters );
                                                      if( ds.Tables.Count == 0 || ds.Tables[ 0 ].Rows.Count == 0 ) {
                                                          return new string[0];
                                                      }

                                                      DataTable tb = ds.Tables[ 0 ];
                                                      List< ResourceHandle > res = new List< ResourceHandle >( tb.Rows.Count );
                                                      for( int ii = 0; ii < tb.Rows.Count; ii++ ) {
                                                          DataRow row = tb.Rows[ ii ];
                                                          res.Add( ResourceHandle.GetResourceHandle( ( string )row[ 0 ] ) );
                                                      }
                                                      return res;
                                                  },
                                                  principalId,
                                                  operationId );
        }

        public IList< Operation > GetAuthorizedOperations( string principalId, ResourceHandle resource ) {
            if( string.IsNullOrEmpty( principalId ) ) {
                throw new ArgumentNullException( "principalId" );
            }

            if( resource == null ) {
                throw new ArgumentNullException( "resource" );
            }
            ObjectCache cache = ObjectCache.Current;
            return ( IList< Operation > )ExecuteMethodWithCacheLookUp( "GetAuthorizedOperations",
                                                                       delegate( string[ ] parameters ) {
                                                                           DataSet ds =
                                                                                   DBHelper.ExecuteDataSet( "GetAuthorizedOperations",
                                                                                                            parameters );
                                                                           if( ds.Tables.Count == 0 || ds.Tables[ 0 ].Rows.Count == 0 ) {
                                                                               return new Operation[0];
                                                                           }
                                                                           DataTable tb = ds.Tables[ 0 ];
                                                                           List< Operation > res = new List< Operation >( tb.Rows.Count );
                                                                           for( int ii = 0; ii < tb.Rows.Count; ii++ ) {
                                                                               DataRow row = tb.Rows[ ii ];
                                                                               res.Add( cache.Get< Operation >( ( string )row[ 0 ] ) );
                                                                           }
                                                                           return res;
                                                                       },
                                                                       principalId,
                                                                       resource.AfcasKey );
        }

        #endregion

        private static string BuildCacheKey( string methodName, params string[ ] parameters ) {
            int len = _CacheKeyPrefix.Length + methodName.Length + parameters.Length + 1;
            for( int ii = 0; ii < parameters.Length; ii++ ) {
                len += parameters[ ii ].Length;
            }
            StringBuilder sb = new StringBuilder( len );
            sb.Append( _CacheKeyPrefix );
            sb.Append( methodName );
            sb.Append( '\0' );
            for( int ii = 0; ii < parameters.Length; ii++ ) {
                string pr = parameters[ ii ];
                sb.Append( pr );
                sb.Append( '\0' );
            }
            return sb.ToString( );
        }

        private void AddResultToCache( string key, object result ) {
            if( _CacheDurationInSeconds == 0 ) {
                return;
            }

            HttpRuntime.Cache.Add( key,
                                   result,
                                   null,
                                   DateTime.Now.AddSeconds( _CacheDurationInSeconds ),
                                   Cache.NoSlidingExpiration,
                                   CacheItemPriority.Default,
                                   null );
        }

        private object GetResultFromCache( string key ) {
            return _CacheDurationInSeconds == 0 ? null : HttpRuntime.Cache.Get( key );
        }

        private object ExecuteMethodWithCacheLookUp( string methodName, CacheableMethodCall method, params string[ ] parameters ) {
            string key = BuildCacheKey( methodName, parameters );

            object res = GetResultFromCache( key );
            if( res == null ) {
                res = method( parameters );
                AddResultToCache( key, res );
            }
            return res;
        }

        #region Nested type: CacheableMethodCall

        private delegate object CacheableMethodCall( string[ ] parameters );

        #endregion
    }
}