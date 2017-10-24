using Autofac;
using NUnit.Framework;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.Data;
using Orchard.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Tests.Territories {
    [TestFixture]
    public class TerritoriesHierarchyServiceTests : DatabaseEnabledTestsBase {

        private ITerritoriesHierarchyService _territoriesHierarchyService;

        public override void Init() {
            base.Init();

            _territoriesHierarchyService = _container.Resolve<ITerritoriesHierarchyService>();
        }

        public override void Register(ContainerBuilder builder) {
            builder.RegisterType<TerritoriesHierarchyService>().As<ITerritoriesHierarchyService>();

            builder.RegisterType<TerritoryRepositoryService>().As<ITerritoriesRepositoryService>();
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));
        }

        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] {
                    typeof(TerritoryInternalRecord),
                    typeof(TerritoryHierarchyPartRecord),
                    typeof(TerritoryPartRecord)
                };
            }
        }
        
        [Test]
        public void AddTerritoryThrowsTheExpectedArgnumentNullExceptions() {
            // Check the expected ArgumentNullExceptions for AddTerritory(TerritoryPart, TerritoryHierarchyPart):
            // 1. territory is null
            // 2. territory.Record is null
            // 3. hierarchy is null
            // 4. hierarchy.Record is null
            // Additionally, for AddTerritory(TerritoryPart, TerritoryHierarchyPart, TerritoryPart):
            // 5. parent is null
            // 6. parent.Record is null
        }

        [Test]
        public void AddTerritoryFailsOnWrongTerritoryType() {
            // Check the expected ArrayTypeMismatchExceptions
            // 1. the ContentType of territory does not match hierarchy.TerritoryType
            // For AddTerritory(TerritoryPart, TerritoryHierarchyPart, TerritoryPart):
            // 2. the parent belongs to a different hierarchy
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
