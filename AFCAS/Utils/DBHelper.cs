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

namespace Afcas.Utils {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Threading;
    using System.Transactions;
    using Properties;
    using IsolationLevel=System.Transactions.IsolationLevel;

    public static class DBHelper {
        private static readonly Dictionary< string, DbParameterCollection > _ProcParametersCache =
                new Dictionary< string, DbParameterCollection >( );

        private static object _SyncRoot;

        private static object SyncRoot {
            get {
                Interlocked.CompareExchange( ref _SyncRoot, new object( ), null );
                return _SyncRoot;
            }
        }

        private static DbCommand CreateCommand( DbConnection conn, string spName, params object[ ] parameterValues ) {
            DbCommand res = conn.CreateCommand( );
            res.CommandText = spName;
            res.CommandType = CommandType.StoredProcedure;

            SetParameters( res, parameterValues );
            return res;
        }

        private static object ExecuteCommmand( string spName, SqlCommandExecutor executor, params object[ ] parameterValues ) {
            object res;
            using( TransactionScope scope = GetRequiredTransactionScope( ) ) {
                DbConnection conn = ConnectionCache.GetConnection( );
                using( DbCommand cmd = CreateCommand( conn, spName, parameterValues ) ) {
                    res = executor( cmd );
                }
                scope.Complete( );
            }
            return res;
        }

        public static object ExecuteScalar( string spName, params object[ ] parameterValues ) {
            return ExecuteCommmand( spName,
                                    delegate( DbCommand cmd ) {
                                        object res = cmd.ExecuteScalar( );
                                        if( res == null ) {
                                            for( int ii = 0; ii < cmd.Parameters.Count; ii++ ) {
                                                DbParameter param = cmd.Parameters[ ii ];
                                                if( param.Direction != ParameterDirection.ReturnValue ) {
                                                    continue;
                                                }
                                                res = param.Value;
                                                break;
                                            }
                                        }
                                        return res;
                                    },
                                    parameterValues );
        }

        public static int ExecuteNonQuery( string spName, params object[ ] parameterValues ) {
            return ExecuteNonQuery( spName, 30, parameterValues );
        }

        public static int ExecuteNonQuery( string spName, int commandTimeout, params object[ ] parameterValues  ) {
            return ( int )ExecuteCommmand( spName,
                                           delegate( DbCommand cmd ) {
                                               int res = 0;
                                               cmd.CommandTimeout = commandTimeout;
                                               cmd.ExecuteNonQuery( );
                                               for( int ii = 0; ii < cmd.Parameters.Count; ii++ ) {
                                                   DbParameter param = cmd.Parameters[ ii ];
                                                   if( param.Direction != ParameterDirection.ReturnValue ) {
                                                       continue;
                                                   }
                                                   res = ( int )param.Value;
                                                   break;
                                               }
                                               return res;
                                           },
                                           parameterValues );
        }


        public static DataSet ExecuteDataSet( string spName, params object[ ] parameterValues ) {
            return ( DataSet )ExecuteCommmand( spName,
                                               delegate( DbCommand cmd ) {
                                                   DataSet res = new DataSet( );
                                                   res.Locale = CultureInfo.CurrentCulture;
                                                   using( DbDataAdapter da = CreateDataAdapter( ) ) {
                                                       da.SelectCommand = cmd;
                                                       da.Fill( res );
                                                   }
                                                   return res;
                                               },
                                               parameterValues );
        }

        public static void Init( string connectionString ) {
            ConnectionCache.Init( connectionString );
        }

        public static TransactionScope GetRequiredTransactionScope( ) {
            if( Transaction.Current != null ) {
                return new TransactionScope( TransactionScopeOption.Required );
            }
            TransactionOptions tro = new TransactionOptions( );
            tro.IsolationLevel = IsolationLevel.ReadCommitted;
            return new TransactionScope( TransactionScopeOption.Required, tro );
        }

        public static T RunInTransaction<T>(Func<DbConnection, T> func)
        {
            using (TransactionScope scope = GetRequiredTransactionScope())
            {
                DbConnection conn = ConnectionCache.GetConnection();

                var result = func(conn);

                scope.Complete();

                return result;
            }
        }

        public static void RunInTransaction(Action<DbConnection> func)
        {
            using (TransactionScope scope = GetRequiredTransactionScope())
            {
                DbConnection conn = ConnectionCache.GetConnection();

                func(conn);

                scope.Complete();
            }
        }

        #region SQL Server specific code here

        private static SqlConnection CreateConnection( string connectionString ) {
            return new SqlConnection( connectionString );
        }

        private static DbDataAdapter CreateDataAdapter( ) {
            return SqlClientFactory.Instance.CreateDataAdapter( );
        }

        private static void SetParameters( DbCommand cmd, params object[ ] paramValues ) {
            DbParameterCollection pc;

            lock( SyncRoot ) {
                if( !_ProcParametersCache.TryGetValue( cmd.CommandText, out pc ) ) {
                    using( TransactionScope scope = new TransactionScope( TransactionScopeOption.Suppress ) ) {
                        using( SqlConnection conn = CreateConnection( ConnectionCache.ConnectionString ) ) {
                            using( SqlCommand paramCmd = conn.CreateCommand( ) ) {
                                paramCmd.CommandType = CommandType.StoredProcedure;
                                paramCmd.CommandText = cmd.CommandText;
                                conn.Open( );
                                SqlCommandBuilder.DeriveParameters( paramCmd );
                                conn.Close( );
                                pc = paramCmd.Parameters;
                                _ProcParametersCache.Add( cmd.CommandText, pc );
                            }
                        }
                        scope.Complete( );
                    }
                }
            }

            // set the parameter values
            int pidx = 0;
            for( int ii = 0; ii < pc.Count; ii++ ) {
                DbParameter param = ( DbParameter )( ( ICloneable )pc[ ii ] ).Clone( );
                cmd.Parameters.Add( param );
                if( pidx < paramValues.Length ) {
                    if( param.Direction == ParameterDirection.Input || param.Direction == ParameterDirection.InputOutput ) {
                        param.Value = paramValues[ pidx++ ];
                    }
                } else {
                    if( param.Direction == ParameterDirection.Input || param.Direction == ParameterDirection.InputOutput ) {
                        param.Value = DBNull.Value;
                    }
                }
            }
        }

        #endregion

        #region Nested type: ConnectionCache

        private static class ConnectionCache {
            private static readonly IDictionary< Transaction, IDictionary< string, DbConnection > > _ConnectionList =
                    new Dictionary< Transaction, IDictionary< string, DbConnection > >( );

            private static string _ConnectionString;

            public static string ConnectionString {
                get {
                    return _ConnectionString;
                }
            }

            private static DbConnection CreateConnection( ) {
                if( _ConnectionString == null ) {
                    Init( Settings.Default.ConnectionString );
                }
                return DBHelper.CreateConnection( _ConnectionString );
            }

            public static DbConnection GetConnection( ) {
                Transaction trx = Transaction.Current;
                if( trx == null ) {
                    throw new InvalidOperationException( "There is no active transaction" );
                }

                ConnKey key = new ConnKey( trx, _ConnectionString );

                DbConnection res;
                IDictionary< string, DbConnection > cl;
                lock( _ConnectionList ) {
                    if( !_ConnectionList.TryGetValue( trx, out cl ) ) {
                        res = CreateConnection( );
                        cl = new Dictionary< string, DbConnection >( );
                        cl.Add( _ConnectionString, res );
                        _ConnectionList.Add( trx, cl );
                        trx.TransactionCompleted += CleanupTransactionData;
                    } else {
                        if( !cl.TryGetValue( _ConnectionString, out res ) ) {
                            res = CreateConnection( );
                            cl.Add( _ConnectionString, res );
                        }
                    }
                    PrepareConnection( res );
                }
                return res;
            }

            private static void CleanupTransactionData( object sender, TransactionEventArgs e ) {
                try {
                    IDictionary< string, DbConnection > cl;
                    lock( _ConnectionList ) {
                        if( _ConnectionList.TryGetValue( e.Transaction, out cl ) ) {
                            foreach( DbConnection conn in cl.Values ) {
                                if( conn != null && conn.State == ConnectionState.Open ) {
                                    conn.Close( );
                                }
                            }
                        }
                    }
                } catch( DbException ) {
                    //ignore errors here
                } finally {
                    _ConnectionList.Remove( e.Transaction );
                }
            }

            private static void PrepareConnection( IDbConnection res ) {
                if( res.State != ConnectionState.Open ) {
                    if( res.ConnectionString != _ConnectionString ) {
                        res.ConnectionString = _ConnectionString;
                    }
                    res.Open( );
                }
            }

            public static void Init( string connectionString ) {
                _ConnectionString = connectionString;
            }

            #region Nested type: ConnKey

            private struct ConnKey {
                private readonly string _DbConn;
                private readonly Transaction _Tran;

                public ConnKey( Transaction trx, string connectionString ) {
                    _Tran = trx;
                    _DbConn = connectionString;
                }

                public override bool Equals( object obj ) {
                    if( obj == null ) {
                        return false;
                    }
                    if( obj is ConnKey ) {
                        ConnKey other = ( ConnKey )obj;
                        return other._Tran == _Tran && other._DbConn == _DbConn;
                    }
                    return false;
                }

                public override int GetHashCode( ) {
                    int res = 0;
                    if( _Tran != null ) {
                        res = _Tran.GetHashCode( );
                    }
                    unchecked {
                        res = _DbConn.GetHashCode( ) + 17*res;
                    }

                    return res;
                }

                public static bool operator ==( ConnKey left, ConnKey right ) {
                    return left.Equals( right );
                }

                public static bool operator !=( ConnKey left, ConnKey right ) {
                    return !left.Equals( right );
                }
            }

            #endregion
        }

        #endregion

        #region Nested type: SqlCommandExecutor

        private delegate object SqlCommandExecutor( DbCommand cmd );

        #endregion
    }
}