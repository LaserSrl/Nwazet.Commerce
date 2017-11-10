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

            //_dataMigrationManager = _container.Resolve<IDataMigrationManager>();

            //_dataMigrationManager.Update("Territories");
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

            //// We need the migrations to have the 1-to-many relationships
            //builder.RegisterType<DataMigrationManager>().As<IDataMigrationManager>();
            //builder.RegisterType<ExtensionManager>().As<IExtensionManager>();
            //var mockInterpreter = new Mock<IDataMigrationInterpreter>();
            //mockInterpreter
            //    .Setup(mi => mi.PrefixTableName(It.IsAny<string>()))
            //    .Returns<string>(ptn => ptn);
            //mockInterpreter
            //    .Setup(mi => mi.RemovePrefixFromTableName(It.IsAny<string>()))
            //    .Returns<string>(ptn => ptn);
            //builder.RegisterInstance(mockInterpreter.Object);
            //builder.RegisterType<StubParallelCacheContext>().As<IParallelCacheContext>();
            //builder.RegisterType<StubAsyncTokenProvider>().As<IAsyncTokenProvider>();

            //var mockMigration = new Mock<TerritoriesMigrations>() { CallBase = true};
            //mockMigration
            //    .Setup(mi => mi.Feature)
            //    .Returns(new Feature() { Descriptor = new FeatureDescriptor { Id = "Territories", Extension = new ExtensionDescriptor { Id = "Nwazet.Commerce" } } });
            //builder.RegisterInstance(mockMigration.Object).As<IDataMigration>(); //TODO

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
                    //typeof(DataMigrationRecord),
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
            var territory = _contentManager.Create<TerritoryPart>("TerritoryType0");
            var hierarchy = _contentManager.Create<TerritoryHierarchyPart>("HierarchyType0");
            var parent = _contentManager.Create<TerritoryPart>("TerritoryType0");

            _territoriesHierarchyService.AddTerritory(parent, hierarchy);
            Assert.That(parent.Record.ParentTerritory, Is.EqualTo(null));
            _territoriesHierarchyService.AddTerritory(territory, hierarchy, parent);
            Assert.That(territory.Record.Hierarchy.Id, Is.EqualTo(hierarchy.Record.Id));
            Assert.That(territory.Record.ParentTerritory.Id, Is.EqualTo(parent.Record.Id));
            
            _territoriesHierarchyService.AddTerritory(territory, hierarchy);
            Assert.That(territory.Record.ParentTerritory, Is.EqualTo(null));
        }

        [Test]
        public void AddTerritoryAlsoMovesChildren() {
            var territory = _contentManager.Create<TerritoryPart>("TerritoryType0");
            var parent = _contentManager.Create<TerritoryPart>("TerritoryType0");
            var hierarchy = _contentManager.Create<TerritoryHierarchyPart>("HierarchyType0");
            var otherHierarchy = _contentManager.Create<TerritoryHierarchyPart>("HierarchyType0");

            _territoriesHierarchyService.AddTerritory(parent, hierarchy);
            _territoriesHierarchyService.AddTerritory(territory, hierarchy, parent);
            Assert.That(territory.Record.Hierarchy.Id, Is.EqualTo(hierarchy.Record.Id));
            
            // This only works if the 1-to-n relationship in the db works also on test environment
            _territoriesHierarchyService.AddTerritory(parent, otherHierarchy);
            Assert.That(parent.Record.Hierarchy.Id, Is.EqualTo(otherHierarchy.Record.Id));
            Assert.That(territory.Record.Hierarchy.Id, Is.EqualTo(otherHierarchy.Record.Id));
            Assert.That(territory.Record.ParentTerritory.Id, Is.EqualTo(parent.Record.Id));
        }

        [Test]
        public void AddTerritoryMovesBetweenHierarchies() {
            // verify that we can move a territory from one hierarchy to another
        }

        [Test]
        public void AddTerritoryInTheSameHierarchyDoesNotFail() {
            // verify that we can call AddTerritory twice for the same territory and hierachy
        }

        [Test]
        public void AddTerritoryMovesBetweenHierarchiesOnlyIfInternalRecordsAllowIt() {
            // when using AddTerritory to move from one hierarchy to another, we must ensure that
            // we do not have duplicate TerritoryInternalRecords.
        }

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
        }

        [Test]
        public void AssignParentThrowsTheExpectedArgumentNullExceptions() {
            // 1. territory is null
            var parent = _contentManager.Create<TerritoryPart>("TerritoryType0");
            Assert.Throws<ArgumentNullException>(() => 
                _territoriesHierarchyService.AssignParent(null, parent));
            // 2. territory.Record is null
            var territory = _contentManager.Create<TerritoryPart>("TerritoryType0");
            territory.Record = null;
            Assert.Throws<ArgumentNullException>(() =>
                _territoriesHierarchyService.AssignParent(territory, parent));
            // 3. parent is null
            territory = _contentManager.Create<TerritoryPart>("TerritoryType0");
            Assert.Throws<ArgumentNullException>(() =>
                _territoriesHierarchyService.AssignParent(territory, null));
            // 4. parent.Record is null
            parent.Record = null;
            Assert.Throws<ArgumentNullException>(() =>
                _territoriesHierarchyService.AssignParent(territory, parent));
            // 5. territory.Record.Hierarchy is null
            var hierarchy = _contentManager.Create<TerritoryHierarchyPart>("HierarchyType0");
            parent = _contentManager.Create<TerritoryPart>("TerritoryType0");
            _territoriesHierarchyService.AddTerritory(parent, hierarchy);
            Assert.Throws<ArgumentNullException>(() =>
                _territoriesHierarchyService.AssignParent(territory, parent));
            // 6. parent.Record.Hierachy is null
            _territoriesHierarchyService.AddTerritory(territory, hierarchy);
            parent = _contentManager.Create<TerritoryPart>("TerritoryType0");
            Assert.Throws<ArgumentNullException>(() =>
                _territoriesHierarchyService.AssignParent(territory, parent));

            // Sanity check
            parent = _contentManager.Create<TerritoryPart>("TerritoryType0");
            territory = _contentManager.Create<TerritoryPart>("TerritoryType0");
            _territoriesHierarchyService.AddTerritory(parent, hierarchy);
            _territoriesHierarchyService.AddTerritory(territory, hierarchy);
            _territoriesHierarchyService.AssignParent(territory, parent);
            Assert.That(territory.Record.ParentTerritory.Id, Is.EqualTo(parent.Record.Id));
        }

        [Test]
        public void AssignParentThrowsTheExpectedMismatchExceptions() {
            // 1. the ContentType of territory and parent do not match
            var parent = _contentManager.Create<TerritoryPart>("TerritoryType0");
            var territory = _contentManager.Create<TerritoryPart>("TerritoryType1");
            Assert.Throws<ArrayTypeMismatchException>(() =>
                _territoriesHierarchyService.AssignParent(territory, parent));
            // 2. territory and parent belong to different hierarchies
            territory = _contentManager.Create<TerritoryPart>("TerritoryType0");
            var hierarchy1 = _contentManager.Create<TerritoryHierarchyPart>("HierarchyType0");
            var hierarchy2 = _contentManager.Create<TerritoryHierarchyPart>("HierarchyType0");
            _territoriesHierarchyService.AddTerritory(parent, hierarchy1);
            _territoriesHierarchyService.AddTerritory(territory, hierarchy2);
            Assert.Throws<ArrayTypeMismatchException>(() =>
                _territoriesHierarchyService.AssignParent(territory, parent));
        }

        [Test]
        public void AssignParentThrowsInvalidOperationExceptionsForEqualTerritories() {
            var hierarchy = _contentManager.Create<TerritoryHierarchyPart>("HierarchyType0");
            // 1. parent == child
            var parent = _contentManager.Create<TerritoryPart>("TerritoryType0");
            _territoriesHierarchyService.AddTerritory(parent, hierarchy);
            Assert.Throws<InvalidOperationException>(() =>
                _territoriesHierarchyService.AssignParent(parent, parent));
        }

        [Test]
        public void AssignParentThrowsInvalidOperationExceptionsForAttemptedRecursion() {
            var hierarchy = _contentManager.Create<TerritoryHierarchyPart>("HierarchyType0");
            // parent is in a branch off the child
            var T0 = _contentManager.Create<TerritoryPart>("TerritoryType0");
            _territoriesHierarchyService.AddTerritory(T0, hierarchy);
            // 1. Immediate child
            var T1 = _contentManager.Create<TerritoryPart>("TerritoryType0");
            _territoriesHierarchyService.AddTerritory(T1, hierarchy, T0);
            Assert.Throws<InvalidOperationException>(() =>
                _territoriesHierarchyService.AssignParent(T0, T1));
            // Add a bunch of levels
            for (int i = 0; i < 5; i++) {
                var TX = _contentManager.Create<TerritoryPart>("TerritoryType0");
                _territoriesHierarchyService.AddTerritory(TX, hierarchy, T1);
                T1 = TX;
            }
            // 2. Parent is deeper down
            Assert.Throws<InvalidOperationException>(() =>
                _territoriesHierarchyService.AssignParent(T0, T1));
        }

        [Test]
        public void AssignParentMovesTerritoryCorrectly() {
            // try both a territory from the root level and one that had a parent already
            var hierarchy = _contentManager.Create<TerritoryHierarchyPart>("HierarchyType0");
            // Initial Hierarchy:
            // - T1
            // - - T2
            // - T3
            var T1 = _contentManager.Create<TerritoryPart>("TerritoryType0");
            var T2 = _contentManager.Create<TerritoryPart>("TerritoryType0");
            var T3 = _contentManager.Create<TerritoryPart>("TerritoryType0");
            _territoriesHierarchyService.AddTerritory(T1, hierarchy);
            _territoriesHierarchyService.AddTerritory(T2, hierarchy, T1);
            _territoriesHierarchyService.AddTerritory(T3, hierarchy);
            // sanity check: verify hierarchy
            var startingConditionOK = T1.Record.Hierarchy.Id == hierarchy.Record.Id &&
                T2.Record.Hierarchy.Id == hierarchy.Record.Id &&
                T3.Record.Hierarchy.Id == hierarchy.Record.Id &&
                T1.Record.ParentTerritory == null &&
                T2.Record.ParentTerritory.Id == T1.Record.Id &&
                T3.Record.ParentTerritory == null;
            Assert.That(startingConditionOK);
            // Move T3 under T1
            _territoriesHierarchyService.AssignParent(T3, T1);
            var verify = T1.Record.ParentTerritory == null && // T1 has not moved
                T2.Record.ParentTerritory.Id == T1.Record.Id && // T2 has not moved
                T3.Record.ParentTerritory.Id == T1.Record.Id;
            Assert.That(verify);
            // Move T2 under T3
            _territoriesHierarchyService.AssignParent(T2, T3);
            verify = T1.Record.ParentTerritory == null && // T1 has not moved
                T2.Record.ParentTerritory.Id == T3.Record.Id && 
                T3.Record.ParentTerritory.Id == T1.Record.Id; // T3 has not moved
            Assert.That(verify);
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
