using Autofac;
using Moq;
using NUnit.Framework;
using Nwazet.Commerce.Exceptions;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.Tests.Stubs;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.Records;
using Orchard.Core.Settings.Metadata;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.Tests.Modules;
using Orchard.Tests.Stubs;
using Orchard.UI.Notify;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Tests.Territories {
    [TestFixture]
    public class TerritoriesServiceTests : DatabaseEnabledTestsBase {

        private ITerritoriesService _territoriesService;
        private ITerritoriesRepositoryService _territoryRepositoryService;
        private Mock<IOrchardServices> _mockServices;

        public override void Init() {
            _mockServices = new Mock<IOrchardServices>();
            base.Init();

            _mockServices.SetupGet(ms => ms.Authorizer).Returns(_container.Resolve<IAuthorizer>());

            _territoriesService = _container.Resolve<ITerritoriesService>();
            _territoryRepositoryService = _container.Resolve<ITerritoriesRepositoryService>();
        }

        public override void Register(ContainerBuilder builder) {

            builder.RegisterType<TerritoriesService>().As<ITerritoriesService>();
            builder.RegisterType<TerritoryRepositoryService>().As<ITerritoriesRepositoryService>();

            //for TerritoriesService
            builder.RegisterType<ContentDefinitionManagerStub>().As<IContentDefinitionManager>();
            builder.RegisterType<Authorizer>().As<IAuthorizer>();
            builder.RegisterType<DefaultContentManager>().As<IContentManager>();
            builder.RegisterInstance(_mockServices.Object);

            //for TerritoryRepositoryService
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));

            //for Authorizer
            builder.RegisterInstance(new Mock<INotifier>().Object);
            builder.RegisterType<AuthorizationServiceStub>().As<IAuthorizationService>();

            //For DefaultContentManager
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<DefaultContentManagerSession>().As<IContentManagerSession>();
            builder.RegisterInstance(new Mock<IContentDisplay>().Object);
            builder.RegisterType<Signals>().As<ISignals>();

            var _workContext = new Mock<WorkContext>();
            _workContext.Setup(w =>
                w.GetState<IUser>(It.Is<string>(s => s == "CurrentUser"))).Returns(() => { return _currentUser; });

            var _workContextAccessor = new Mock<IWorkContextAccessor>();
            _workContextAccessor.Setup(w => w.GetContext()).Returns(_workContext.Object);
            builder.RegisterInstance(_workContextAccessor.Object).As<IWorkContextAccessor>();
        }
        
        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] {
                    typeof(TerritoryInternalRecord),
                };
            }
        }

        private IUser _currentUser;
        [Test]
        public void HierarchyManagePermissionsAreSameNumberAsHierarchyTypesForAdmin() {

            _currentUser = new FakeUser() { UserName = "admin" };

            Assert.That(_territoriesService.GetHierarchyTypes().Count(), Is.EqualTo(3));
            Assert.That(_territoriesService.ListHierarchyTypePermissions().Count(), Is.EqualTo(3));
        }

        [Test]
        public void TerritoryManagePermissionsAreSameNumberAsTerritoryTypesForAdmin() {

            _currentUser = new FakeUser() { UserName = "admin" };

            Assert.That(_territoriesService.GetTerritoryTypes().Count(), Is.EqualTo(3));
            Assert.That(_territoriesService.ListTerritoryTypePermissions().Count(), Is.EqualTo(3));
        }

        [Test]
        public void HierarchyManagePermissionsAreNotSameNumberAsHierarchyTypes() {

            _currentUser = new FakeUser() { UserName = "user1" };

            Assert.That(_territoriesService.GetHierarchyTypes().Count(), Is.EqualTo(1));
            Assert.That(_territoriesService.ListHierarchyTypePermissions().Count(), Is.EqualTo(3));
        }

        [Test]
        public void TerritoryManagePermissionsAreNotSameNumberAsTerritoryTypes() {

            _currentUser = new FakeUser() { UserName = "user1" };

            Assert.That(_territoriesService.GetTerritoryTypes().Count(), Is.EqualTo(1));
            Assert.That(_territoriesService.ListTerritoryTypePermissions().Count(), Is.EqualTo(3));
        }

        private void PopulateTable(int numberOfRecords, int startId = 0) {
            for (int i = startId; i < startId + numberOfRecords; i++) {
                _territoryRepositoryService.AddTerritory(
                    new TerritoryInternalRecord {
                        Name = "Name" + i.ToString() + " "
                    }
                    );
            }
        }

        [Test]
        public void TerritoryInternalRecordsAreCreatedCorrectly() {
            Assert.That(_territoryRepositoryService.GetTerritoriesCount(), Is.EqualTo(0));

            var tir = new TerritoryInternalRecord { Name = "test" };
            Assert.That(_territoryRepositoryService.GetTerritoriesCount(), Is.EqualTo(0));

            PopulateTable(6);
            Assert.That(_territoryRepositoryService.GetTerritoriesCount(), Is.EqualTo(6));

            var created = _territoryRepositoryService.GetTerritories().ToArray();
            Assert.That(created.Length, Is.EqualTo(6));

            var sameObjects = true;
            for (int i = 0; i < created.Length; i++) {
                sameObjects &= created[i].Name == "Name" + i.ToString(); //this also verifies that Names are trimmed
            }
            Assert.That(sameObjects);
        }

        [Test]
        public void GetTerritoriesCountReturnsTheCorrectNumber() {
            Assert.That(_territoryRepositoryService.GetTerritoriesCount(), Is.EqualTo(0));

            PopulateTable(6);
            Assert.That(_territoryRepositoryService.GetTerritories().Count(), Is.EqualTo(6));
            Assert.That(_territoryRepositoryService.GetTerritoriesCount(), Is.EqualTo(6));

            PopulateTable(6, 6);
            Assert.That(_territoryRepositoryService.GetTerritories().Count(), Is.EqualTo(12));
            Assert.That(_territoryRepositoryService.GetTerritoriesCount(), Is.EqualTo(12));

            for (int i = 1; i < 6; i++) {
                _territoryRepositoryService.Delete(i);
            }
            Assert.That(_territoryRepositoryService.GetTerritoriesCount(), Is.EqualTo(7));
        }

        [Test]
        public void GetTerritorieShouldPaginate() {
            PopulateTable(6);

            var page_2_3 = _territoryRepositoryService.GetTerritories(2, 3);
            Assert.That(page_2_3.Count(), Is.EqualTo(3));
            Assert.That(page_2_3.First().Name, Is.EqualTo("Name2"));

            var page_5_3 = _territoryRepositoryService.GetTerritories(5, 3);
            Assert.That(page_5_3.Count(), Is.EqualTo(1));
            Assert.That(page_5_3.First().Name, Is.EqualTo("Name5"));
        }

        [Test]
        public void GetTerritoriesShouldIgnoreNegativeStartIndex() {
            PopulateTable(6);
            var page = _territoryRepositoryService.GetTerritories(-1);
            Assert.That(page.Count(), Is.EqualTo(6));
            Assert.That(page.First().Name, Is.EqualTo("Name0"));
        }

        [Test]
        public void GetTerritoriesShouldIgnoreNegativePageSize() {
            PopulateTable(6);
            var page = _territoryRepositoryService.GetTerritories(0, -2);
            Assert.That(page.Count(), Is.EqualTo(6));
            page = _territoryRepositoryService.GetTerritories(2, -2);
            Assert.That(page.Count(), Is.EqualTo(4));
        }

        [Test]
        public void UpdateHappensCorrectly() {
            PopulateTable(1);
            var tir = _territoryRepositoryService.GetTerritories().First();
            tir.Name += "x";

            var updated = _territoryRepositoryService.Update(tir);

            Assert.That(updated.Name, Is.EqualTo("Name0x"));
            Assert.That(updated.Id == tir.Id);

            var fromDb = _territoryRepositoryService.GetTerritoryInternal(1);
            Assert.That(fromDb.Id == updated.Id);
            Assert.That(fromDb.Name, Is.EqualTo("Name0x"));

            var updated2 = _territoryRepositoryService.Update(tir); //same name should not throw exception here.
            Assert.That(updated2.Name, Is.EqualTo("Name0x"));
            Assert.That(updated2.Id == tir.Id);
        }

        [Test]
        public void DeleteCorrectTerritory() {
            PopulateTable(10);
            var tirs = _territoryRepositoryService.GetTerritories();

            _territoryRepositoryService.Delete(tirs.First());

            Assert.That(_territoryRepositoryService.GetTerritoriesCount(), Is.EqualTo(9));
            var tirs2 = _territoryRepositoryService.GetTerritories();
            Assert.That(tirs2.First().Name, Is.EqualTo("Name1"));

            _territoryRepositoryService.Delete(tirs.First()); //should do nothing
            Assert.That(_territoryRepositoryService.GetTerritoriesCount(), Is.EqualTo(9));
            tirs2 = _territoryRepositoryService.GetTerritories();
            Assert.That(tirs2.First().Name, Is.EqualTo("Name1"));

            _territoryRepositoryService.Delete(tirs2.First().Id);

            Assert.That(_territoryRepositoryService.GetTerritoriesCount(), Is.EqualTo(8));
            var tirs3 = _territoryRepositoryService.GetTerritories();
            Assert.That(tirs3.First().Name, Is.EqualTo("Name2"));

            _territoryRepositoryService.Delete(18); //should do nothing
            Assert.That(_territoryRepositoryService.GetTerritoriesCount(), Is.EqualTo(8));
            tirs3 = _territoryRepositoryService.GetTerritories();
            Assert.That(tirs3.First().Name, Is.EqualTo("Name2"));
        }

        [Test]
        public void CannotCreateTerritoryWithTheSameName() {
            PopulateTable(1);
            Assert.Throws<TerritoryInternalDuplicateException>(() => PopulateTable(1));
            Assert.That(_territoryRepositoryService.GetTerritoriesCount(), Is.EqualTo(1));
        }

        [Test]
        public void CannotEditTerritoryToHaveSameName() {
            PopulateTable(2);

            var tir = _territoryRepositoryService.GetTerritories().First();

            tir.Name = "Name1";
            Assert.Throws<TerritoryInternalDuplicateException>(() => _territoryRepositoryService.Update(tir));
            Assert.That(_territoryRepositoryService.GetTerritories().Count(t => t.Name == "Name1"), Is.EqualTo(1));
        }

        [Test]
        public void CanUpdateTerritoryToHaveSameNameAsIstself() {
            PopulateTable(2);

            var tir = _territoryRepositoryService.GetTerritories().First();

            var updated = _territoryRepositoryService.Update(tir);
            Assert.That(updated.Name, Is.EqualTo("Name0"));
            Assert.That(updated.Id == tir.Id);
        }

        [Test]
        public void CannotGetNonExistingTerritoryById() {
            Assert.That(_territoryRepositoryService.GetTerritoryInternal(5), Is.EqualTo(null));
            Assert.That(_territoryRepositoryService.GetTerritoryInternal(-5), Is.EqualTo(null));
        }

        [Test]
        public void CannoteGetNonExistingTerritoryByName() {
            Assert.That(_territoryRepositoryService.GetTerritoryInternal("source"), Is.EqualTo(null));
            Assert.That(_territoryRepositoryService.GetTerritoryInternal(""), Is.EqualTo(null));
        }

        [Test]
        public void GetTerritoryInternalIgnoresWhiteSpaceBeforeAndAfterTheName() {
            PopulateTable(1);

            var tir = _territoryRepositoryService.GetTerritoryInternal("Name0");
            Assert.That(tir.Id, Is.EqualTo(1));
            Assert.That(tir.Name, Is.EqualTo("Name0"));

            var tir2 = _territoryRepositoryService.GetTerritoryInternal(" Name0");
            Assert.That(tir2.Id, Is.EqualTo(1));
            Assert.That(tir2.Name, Is.EqualTo("Name0"));

            var tir3 = _territoryRepositoryService.GetTerritoryInternal("Name0 ");
            Assert.That(tir3.Id, Is.EqualTo(1));
            Assert.That(tir3.Name, Is.EqualTo("Name0"));

            var tir4 = _territoryRepositoryService.GetTerritoryInternal(" Name0 ");
            Assert.That(tir4.Id, Is.EqualTo(1));
            Assert.That(tir4.Name, Is.EqualTo("Name0"));
        }
    }
}
