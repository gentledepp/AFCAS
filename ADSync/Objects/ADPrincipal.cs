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

namespace ADSync.Objects {
    using Afcas.Objects;

    internal struct ADPrincipal {
        //these two are not saved in SQL database
        private readonly string _ADPath;
        private readonly string _Description;
        private readonly string _DisplayName;
        private readonly string _Email;
        private readonly string[ ] _GroupPaths;
        private readonly string _Id;
        private readonly string _Name;
        private readonly PrincipalType _PrincipalType;


        public ADPrincipal( string id,
                            string name,
                            string adPath,
                            PrincipalType principalType,
                            string email,
                            string displayName,
                            string description,
                            string[ ] groupPaths ) {
            _Id = id;
            _Name = name;
            _ADPath = adPath;
            _PrincipalType = principalType;
            _Email = email;
            _DisplayName = displayName;
            _Description = description;
            _GroupPaths = groupPaths;
        }

        public string Id {
            get {
                return _Id;
            }
        }

        public string ADPath {
            get {
                return _ADPath;
            }
        }

        public string Name {
            get {
                return _Name;
            }
        }

        public PrincipalType PrincipalType {
            get {
                return _PrincipalType;
            }
        }

        public string DisplayName {
            get {
                return _DisplayName;
            }
        }

        public string Email {
            get {
                return _Email;
            }
        }

        public string Description {
            get {
                return _Description;
            }
        }

        public string[ ] GroupPaths {
            get {
                return _GroupPaths;
            }
        }
    }
}