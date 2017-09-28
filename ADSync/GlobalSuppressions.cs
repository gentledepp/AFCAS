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

using System.Diagnostics.CodeAnalysis;

[ assembly: SuppressMessage( "Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames" ) ]
[ assembly: SuppressMessage( "Microsoft.Design", "CA1014:MarkAssembliesWithClsCompliant" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member",
                Target = "ADSync.Test.TestADSync.#TestSync()" ) ]
[ assembly:
        SuppressMessage( "Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "args", Scope = "member",
                Target = "ADSync.Program.#Main(System.String[])" ) ]