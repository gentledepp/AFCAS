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

namespace ADSync {
    using System;
    using System.Collections.Generic;
    using Objects;
    using Properties;
    using Utils;

    internal class Program {
        private static void Main( string[ ] args ) {
            try {
                IList< ADPrincipal > pl = ADHelper.FetchADPrincipals( Settings.Default.LdapPathList );
                ADSyncHelper.SyncAdToSqlServer( pl,
                                                Afcas.Properties.Settings.Default.ConnectionString,
                                                Settings.Default.PrincipalSourceName );
            } catch(Exception ex) {
                Console.WriteLine( "{0}: Error while running ADSync: {1}", DateTime.Now.ToString( "F"), ex.Message );
                Environment.Exit( 1 );
            }
        }
    }
}