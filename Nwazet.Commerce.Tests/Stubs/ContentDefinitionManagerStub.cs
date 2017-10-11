using Orchard.ContentManagement.MetaData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orchard.ContentManagement.MetaData.Models;
using Nwazet.Commerce.Models;

namespace Nwazet.Commerce.Tests.Stubs {
    public class ContentDefinitionManagerStub : IContentDefinitionManager {

        private List<ContentTypeDefinition> _typeDefinitions;

        public ContentDefinitionManagerStub() {

            _typeDefinitions = new List<ContentTypeDefinition>();
            //generate some dummy definitions for the tests of the Territories feature
            for (int i = 0; i < 3; i++) {
                var typeDefinition = new ContentTypeDefinition(
                    name: "HierarchyType" + i.ToString(),
                    displayName: "HierarchyType" + i.ToString(),
                    parts: new ContentTypePartDefinition[] {
                        new ContentTypePartDefinition(
                            contentPartDefinition: new ContentPartDefinition(TerritoryHierarchyPart.PartName, Enumerable.Empty<ContentPartFieldDefinition>(), null),
                            settings: null
                            )
                    },
                    settings: null
                    );
                _typeDefinitions.Add(typeDefinition);
            }
            for (int i = 0; i < 3; i++) {
                var typeDefinition = new ContentTypeDefinition(
                    name: "TerritoryType" + i.ToString(),
                    displayName: "TerritoryType" + i.ToString(),
                    parts: new ContentTypePartDefinition[] {
                        new ContentTypePartDefinition(
                            contentPartDefinition: new ContentPartDefinition(TerritoryPart.PartName, Enumerable.Empty<ContentPartFieldDefinition>(), null),
                            settings: null
                            )
                    },
                    settings: null
                    );
                _typeDefinitions.Add(typeDefinition);
            }
        }

        public void DeletePartDefinition(string name) {
            throw new NotImplementedException();
        }

        public void DeleteTypeDefinition(string name) {
            throw new NotImplementedException();
        }

        public ContentPartDefinition GetPartDefinition(string name) {
            throw new NotImplementedException();
        }

        public ContentTypeDefinition GetTypeDefinition(string name) {
            throw new NotImplementedException();
        }

        public IEnumerable<ContentFieldDefinition> ListFieldDefinitions() {
            throw new NotImplementedException();
        }

        public IEnumerable<ContentPartDefinition> ListPartDefinitions() {
            throw new NotImplementedException();
        }

        public IEnumerable<ContentTypeDefinition> ListTypeDefinitions() {
            return _typeDefinitions;
        }

        public void StorePartDefinition(ContentPartDefinition contentPartDefinition) {
            throw new NotImplementedException();
        }

        public void StoreTypeDefinition(ContentTypeDefinition contentTypeDefinition) {
            _typeDefinitions.Add(contentTypeDefinition);
        }
    }
}
