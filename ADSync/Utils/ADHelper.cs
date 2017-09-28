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
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.DirectoryServices;
    using Afcas.Objects;
    using Objects;

    /// <summary>
    /// Helper class for Active directory access
    /// </summary>
    internal static class ADHelper {
        #region active directory user property names

        private const string DESCRIPTION = "description";
        private const string DISPLAY_NAME = "displayName";

        private const string EMAIL_ADDRESS = "mail";
        private const string GROUPS = "memberOf";
        private const string PRINCIPAL_NAME = "sAMAccountName";

        #endregion

        [ SuppressMessage( "Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes" ) ]
        public static IList< ADPrincipal > FetchADPrincipals( StringCollection ldapPathList ) {
            IList< ADPrincipal > ret = new List< ADPrincipal >( );

            foreach( string path in ldapPathList ) {
                using( DirectoryEntry root = LoadDirectoryEntry( path ) ) {
                    if( root == null ) {
                        Console.WriteLine( "Skipping DirectoryEntry {0} as it cannot be load", path );
                        continue;
                    }
                    using( DirectorySearcher src = new DirectorySearcher( root ) ) {
                        src.Filter = "(|(&(objectCategory=person)(objectClass=user))(objectCategory=group))";
                        src.SecurityMasks = SecurityMasks.None;
                        src.PageSize = 16;
                        using( SearchResultCollection res = src.FindAll( ) ) {
                            foreach( SearchResult sr in res ) {
                                using( DirectoryEntry de = sr.GetDirectoryEntry( ) ) {
                                    try {
                                        ret.Add( ConstructADPrincipal( de ) );
                                    } catch( Exception ex ) {
                                        Console.WriteLine( "ERROR: while reading data from Active Directory, current item: " );
                                        Console.WriteLine( de.Path );
                                        Console.WriteLine( Environment.NewLine );
                                        Console.WriteLine( ex.Message );
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return ret;
        }

        private static ADPrincipal ConstructADPrincipal( DirectoryEntry de ) {
            string id = de.Guid.ToString();
            string name = ( string )de.Properties[ PRINCIPAL_NAME ].Value;
            string adPath = de.Path.Replace( "LDAP://", "" );
            PrincipalType type = de.SchemaClassName.Equals( "group", StringComparison.OrdinalIgnoreCase )
                                         ? PrincipalType.Group
                                         : PrincipalType.User;
            string email = ( string )de.Properties[ EMAIL_ADDRESS ].Value ?? "";
            string displayName = ( string )de.Properties[ DISPLAY_NAME ].Value ?? "";
            string description = ( string )de.Properties[ DESCRIPTION ].Value ?? "";

            IList grpl = de.Properties[ GROUPS ];
            string[ ] groups;
            if( grpl != null ) {
                groups = new string[grpl.Count];
                grpl.CopyTo( groups, 0 );
            } else {
                groups = new string[0];
            }
            return new ADPrincipal( id, name, adPath, type, email, displayName, description, groups );
        }

        private static DirectoryEntry LoadDirectoryEntry( string path ) {
            DirectoryEntry res = new DirectoryEntry( path );

            try {
                return null == res.NativeGuid ? null : res;
                // will cause an exception if not found
            } catch( DirectoryServicesCOMException ex ) {
                if( ex.ExtendedError != 8333 ) {
                    throw;
                }
                return null;
            }
        }
    }
}