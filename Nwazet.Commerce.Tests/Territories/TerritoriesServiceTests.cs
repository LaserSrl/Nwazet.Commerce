using Autofac;
using Moq;
using NUnit.Framework;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.Tests.Stubs;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.Records;
using Orchard.Core.Settings.Metadata;
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

        public override void Register(ContainerBuilder builder) {

            builder.RegisterType<TerritoriesService>().As<ITerritoriesService>();

            //for TerritoriesService
            builder.RegisterType<ContentDefinitionManagerStub>().As<IContentDefinitionManager>();
            builder.RegisterType<Authorizer>().As<IAuthorizer>();

            //for Authorizer
            builder.RegisterInstance(new Mock<INotifier>().Object);
            builder.RegisterType<AuthorizationServiceStub>().As<IAuthorizationService>();

            var _workContext = new Mock<WorkContext>();
            _workContext.Setup(w => 
                w.GetState<IUser>(It.Is<string>(s => s == "CurrentUser"))).Returns(() => { return _currentUser; });

            var _workContextAccessor = new Mock<IWorkContextAccessor>();
            _workContextAccessor.Setup(w => w.GetContext()).Returns(_workContext.Object);
            builder.RegisterInstance(_workContextAccessor.Object).As<IWorkContextAccessor>();
        }

        public override void Init() {
            base.Init();

            _territoriesService = _container.Resolve<ITerritoriesService>();
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
    }
}
