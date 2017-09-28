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

namespace Afcas.Objects {
    using System;

    [ Serializable ]
    public abstract class ResourceHandle {
        [ NonSerialized ]
        private readonly ResourceHandleFactory _Factory;

        protected ResourceHandle( ResourceHandleFactory factory ) {
            _Factory = factory;
        }

        public abstract string Key { get; }

        internal string AfcasKey {
            get {
                return GetAfcasKey( this );
            }
        }

        public string ResourceType {
            get {
                return _Factory.ResourceType;
            }
        }

        private static string GetAfcasKey( ResourceHandle resource ) {
            return resource.ResourceType + '.' + resource.Key;
        }

        private static string GetResourceType( string afcasKey ) {
            int pos = afcasKey.IndexOf( '.' );
            return afcasKey.Substring( 0, pos );
        }

        internal static ResourceHandle GetResourceHandle( string afcasKey ) {
            string resourceType = GetResourceType( afcasKey );
            return Afcas.GetHandleFactory( resourceType ).GenerateResourceHandleByKey( afcasKey.Substring( resourceType.Length + 1 ) );
        }
    }
}