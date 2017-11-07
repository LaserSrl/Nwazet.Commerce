using Autofac;
using Moq;
using NUnit.Framework;
using Nwazet.Commerce.Handlers;
using Nwazet.Commerce.Migrations;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.Tests.Territories.Handlers;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.Records;
using Orchard.Data;
using Orchard.Data.Migration;
using Orchard.Data.Migration.Interpreters;
using Orchard.Data.Migration.Records;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Tests;
using Orchard.Tests.Stubs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Tests.Territories {
    [TestFixture]
    public class TerritoriesHierarchyServiceTests : DatabaseEnabledTestsBase {

        private ITerritoriesHierarchyService _territoriesHierarchyService;
        private IContentManager _contentManager;

        private IDataMigrationManager _dataMigrationManager;

        public override void Init() {
            base.Init();

            _territoriesHierarchyService = _container.Resolve<ITerritoriesHierarchyService>();
            _contentManager = _container.Resolve<IContentManager>();

            _dataMigrationManager = _container.Resolve<IDataMigrationManager>();

            _dataMigrationManager.Update("Territories");
        }

        public override void Register(ContainerBuilder builder) {
            builder.RegisterType<TerritoriesHierarchyService>().As<ITerritoriesHierarchyService>();

            builder.RegisterType<TerritoryRepositoryService>().As<ITerritoriesRepositoryService>();
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));
            
            var mockDefinitionManager = new Mock<IContentDefinitionManager>();
            mockDefinitionManager
                .Setup<IEnumerable<ContentTypeDefinition>>(mdm => mdm.ListTypeDefinitions())
                .Returns(MockTypeDefinitions);
            mockDefinitionManager // this is required to create the test items
                .Setup(mdm => mdm.GetTypeDefinition(It.IsAny<string>()))
                .Returns<string>(name => MockTypeDefinitions().FirstOrDefault(ctd => ctd.Name == name));
            builder.RegisterInstance(mockDefinitionManager.Object);
            
            builder.RegisterType<DefaultContentManager>().As<IContentManager>();
            //For DefaultContentManager
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<DefaultContentManagerSession>().As<IContentManagerSession>();
            builder.RegisterInstance(new Mock<IContentDisplay>().Object);
            builder.RegisterType<Signals>().As<ISignals>();
            builder.RegisterType<DefaultContentQuery>().As<IContentQuery>();

            //handlers
            builder.RegisterType<TerritoryHierarchyPartHandler>().As<IContentHandler>();
            builder.RegisterType<TerritoryHierachyMockHandler>().As<IContentHandler>();
            builder.RegisterType<TerritoryPartHandler>().As<IContentHandler>();
            builder.RegisterType<TerritoryMockHandler>().As<IContentHandler>();

            // We need the migrations to have the 1-to-many relationships
            builder.RegisterType<DataMigrationManager>().As<IDataMigrationManager>();
            builder.RegisterType<ExtensionManager>().As<IExtensionManager>();
            var mockInterpreter = new Mock<IDataMigrationInterpreter>();
            mockInterpreter
                .Setup(mi => mi.PrefixTableName(It.IsAny<string>()))
                .Returns<string>(ptn => ptn);
            mockInterpreter
                .Setup(mi => mi.RemovePrefixFromTableName(It.IsAny<string>()))
                .Returns<string>(ptn => ptn);
            builder.RegisterInstance(mockInterpreter.Object);
            builder.RegisterType<StubParallelCacheContext>().As<IParallelCacheContext>();
            builder.RegisterType<StubAsyncTokenProvider>().As<IAsyncTokenProvider>();

            var mockMigration = new Mock<TerritoriesMigrations>() { CallBase = true};
            mockMigration
                .Setup(mi => mi.Feature)
                .Returns(new Feature() { Descriptor = new FeatureDescriptor { Id = "Territories", Extension = new ExtensionDescriptor { Id = "Nwazet.Commerce" } } });
            builder.RegisterInstance(mockMigration.Object).As<IDataMigration>(); //TODO

        }
        

        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] {
                    typeof(TerritoryInternalRecord),
                    typeof(TerritoryHierarchyPartRecord),
                    typeof(TerritoryPartRecord),
                    typeof(ContentItemVersionRecord),
                    typeof(ContentItemRecord),
                    typeof(ContentTypeRecord),
                    typeof(DataMigrationRecord)
                };
            }
        }

        private List<ContentTypeDefinition> MockTypeDefinitions() {
            var typeDefinitions = new List<ContentTypeDefinition>();
            //generate some dummy definitions for the tests of the Territories feature
            for (int i = 0; i < 3; i++) {
                var settingsDictionary = new Dictionary<string, string>();
                settingsDictionary.Add("TerritoryHierarchyPartSettings.TerritoryType",
                    "TerritoryType" + i.ToString());
                settingsDictionary.Add("TerritoryHierarchyPartSettings.MayChangeTerritoryTypeOnItem",
                    false.ToString(CultureInfo.InvariantCulture));
                var typeDefinition = new ContentTypeDefinition(
                    name: "HierarchyType" + i.ToString(),
                    displayName: "HierarchyType" + i.ToString(),
                    parts: new ContentTypePartDefinition[] {
                        new ContentTypePartDefinition(
                            contentPartDefinition: new ContentPartDefinition(TerritoryHierarchyPart.PartName, Enumerable.Empty<ContentPartFieldDefinition>(), null),
                            settings: new SettingsDictionary(settingsDictionary)
                            )
                    },
                    settings: null
                    );
                typeDefinitions.Add(typeDefinition);
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
                typeDefinitions.Add(typeDefinition);
            }

            return typeDefinitions;
        }

        [Test]
        public void AddTerritoryThrowsTheExpectedArgumentNullExceptions() {
            // Check the expected ArgumentNullExceptions for AddTerritory(TerritoryPart, TerritoryHierarchyPart):
            // 1. territory is null
            var territory = (TerritoryPart)null;
            var hierarchy = _contentManager.Create<TerritoryHierarchyPart>("HierarchyType0");
            Assert.Throws<ArgumentNullException>(() =>_territoriesHierarchyService.AddTerritory(territory, hierarchy));
            // 2. territory.Record is null
            territory = _contentManager.Create<TerritoryPart>("TerritoryType0");
            territory.Record = null;
            Assert.Throws<ArgumentNullException>(() => _territoriesHierarchyService.AddTerritory(territory, hierarchy));
            // 3. hierarchy is null
            territory = _contentManager.Create<TerritoryPart>("TerritoryType0");
            hierarchy = null;
            Assert.Throws<ArgumentNullException>(() => _territoriesHierarchyService.AddTerritory(territory, hierarchy));
            // 4. hierarchy.Record is null
            hierarchy = _contentManager.Create<TerritoryHierarchyPart>("HierarchyType0");
            hierarchy.Record = null;
            Assert.Throws<ArgumentNullException>(() => _territoriesHierarchyService.AddTerritory(territory, hierarchy));
            // Additionally, for AddTerritory(TerritoryPart, TerritoryHierarchyPart, TerritoryPart):
            // 5. parent is null
            hierarchy = _contentManager.Create<TerritoryHierarchyPart>("HierarchyType0");
            var parent = (TerritoryPart)null;
            Assert.Throws<ArgumentNullException>(() => _territoriesHierarchyService.AddTerritory(territory, hierarchy, parent));
            // 6. parent.Record is null
            parent = _contentManager.Create<TerritoryPart>("TerritoryType0");
            parent.Record = null;
            Assert.Throws<ArgumentNullException>(() => _territoriesHierarchyService.AddTerritory(territory, hierarchy, parent));

            // Sanity check: the following should succeed
            parent = _contentManager.Create<TerritoryPart>("TerritoryType0");
            _territoriesHierarchyService.AddTerritory(parent, hierarchy);
            Assert.That(parent.Record.Hierarchy.Id, Is.EqualTo(hierarchy.Record.Id));
            _territoriesHierarchyService.AddTerritory(territory, hierarchy, parent);
            Assert.That(territory.Record.Hierarchy.Id, Is.EqualTo(hierarchy.Record.Id));
        }

        [Test]
        public void AddTerritoryFailsOnWrongTerritoryType() {
            // Check the expected ArrayTypeMismatchExceptions
            // 1. the ContentType of territory does not match hierarchy.TerritoryType
            var territory = _contentManager.Create<TerritoryPart>("TerritoryType0");
            var hierarchy1 = _contentManager.Create<TerritoryHierarchyPart>("HierarchyType1");
            Assert.Throws<ArrayTypeMismatchException>(() => _territoriesHierarchyService.AddTerritory(territory, hierarchy1));
            // For AddTerritory(TerritoryPart, TerritoryHierarchyPart, TerritoryPart):
            // 2. the parent belongs to a different hierarchy
            var parent = _contentManager.Create<TerritoryPart>("TerritoryType1");
            _territoriesHierarchyService.AddTerritory(parent, hierarchy1);
            var hierarchy0 = _contentManager.Create<TerritoryHierarchyPart>("HierarchyType0");
            Assert.Throws<ArrayTypeMismatchException>(() => _territoriesHierarchyService.AddTerritory(territory, hierarchy0, parent));
        }

        [Test]
        public void AddTerritoryAddsToFirstLevel() {
            // this tests AddTerritory(TerritoryPart, TerritoryHierarchyPart)
        }

        [Test]
        public void AddTerritoryAlsoMovesChildren() { }

        [Test]
        public void AddTerritoryAlsoAssignsParent() {
            // this tests AddTerritory(TerritoryPart, TerritoryHierarchyPart, TerritoryPart)
            var territory = _contentManager.Create<TerritoryPart>("TerritoryType0");
            var hierarchy = _contentManager.Create<TerritoryHierarchyPart>("HierarchyType0");
            var parent = _contentManager.Create<TerritoryPart>("TerritoryType0");

            _territoriesHierarchyService.AddTerritory(parent, hierarchy);
            Assert.That(parent.Record.Hierarchy.Id, Is.EqualTo(hierarchy.Record.Id));

            _territoriesHierarchyService.AddTerritory(territory, hierarchy, parent);
            Assert.That(territory.Record.Hierarchy.Id, Is.EqualTo(hierarchy.Record.Id));
            Assert.That(territory.Record.ParentTerritory.Id, Is.EqualTo(parent.Record.Id));
            Assert.That(parent.Record.Children.Count, Is.EqualTo(1));
            Assert.That(parent.Record.Children.First().Id, Is.EqualTo(territory.Record.Id));
        }

        [Test]
        public void AssignParentThrowsTheExpectedArgumentNullExceptions() {
            // 1. territory is null
            // 2. territory.Record is null
            // 3. parent is null
            // 4. parent.Record is null
            // 5. territory.Record.Hierarchy is null
            // 6. parent.Record.Hierachy is null
        }

        [Test]
        public void AssignParentThrowsTheExpectedMismatchExceptions() {
            // 1. the ContentType of territory and parent do not match
            // 2. territory and parent belong to different hierarchies
        }

        [Test]
        public void AssignParentMovesTerritoryCorrectly() {
            // try both a territory from the root level and one that had a parent already
        }

        [Test]
        public void AssignRecordByNameThrowsExpectedArgumentNullExceptions() {
            // 1. territory is null
            // 2. territory.Record is null
            // 3. no record was found
        }

        [Test]
        public void AssignRecordByIdThrowsExpectedArgumentNullExceptions() {
            // 1. territory is null
            // 2. territory.Record is null
            // 3. no record was found
        }

        [Test]
        public void AssignRecordByObjectThrowsExpectedArgumentNullExceptions() {
            // 1. territory is null
            // 2. territory.Record is null
            // 3. no record was found
        }

        [Test]
        public void AssignRecordDoesNotAllowDuplicates() {
            // test TerritoryInternalDuplicateException this for all variants of the method
            // check that all variants of the method allow reassigning the same record to the same territory
        }


    }
}
