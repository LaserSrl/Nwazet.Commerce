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
        private IWorkContextAccessor _workContextAccessor;

        public override void Register(ContainerBuilder builder) {

            builder.RegisterType<TerritoriesService>().As<ITerritoriesService>();

            //for TerritoriesService
            builder.RegisterType<ContentDefinitionManagerStub>().As<IContentDefinitionManager>();
            builder.RegisterType<Authorizer>().As<IAuthorizer>();

            //for Authorizer
            builder.RegisterType<StubWorkContextAccessor>().As<IWorkContextAccessor>();
            builder.RegisterInstance(new Mock<INotifier>().Object);
            builder.RegisterType<AuthorizationServiceStub>().As<IAuthorizationService>();
        }

        public override void Init() {
            base.Init();

            _territoriesService = _container.Resolve<ITerritoriesService>();
            _workContextAccessor = _container.Resolve<IWorkContextAccessor>();
        }

        [Test]
        public void HierarchyManagePermissionsAreSameNumberAsHierarchyTypesForAdmin() {

            _workContextAccessor.GetContext().CurrentUser = new FakeUser() { UserName = "admin" };


            Assert.That(_territoriesService.GetHierarchyTypes().Count(), Is.EqualTo(3));
            Assert.That(_territoriesService.ListHierarchyTypePermissions().Count(), Is.EqualTo(3));
        }

        [Test]
        public void TerritoryManagePermissionsAreSameNumberAsTerritoryTypesForAdmin() {
            
            Assert.That(_territoriesService.GetTerritoryTypes().Count(), Is.EqualTo(3));
            Assert.That(_territoriesService.ListTerritoryTypePermissions().Count(), Is.EqualTo(3));
        }

        [Test]
        public void HierarchyManagePermissionsAreNotSameNumberAsHierarchyTypes() {

            var user = new FakeUser() { UserName = "user1" };
            _workContextAccessor.GetContext().CurrentUser = user;

            Assert.That(_territoriesService.GetHierarchyTypes().Count(), Is.EqualTo(3));
            Assert.That(_territoriesService.ListHierarchyTypePermissions().Count(), Is.EqualTo(1));
        }

        [Test]
        public void TerritoryManagePermissionsAreNotSameNumberAsTerritoryTypes() {

            var user = new FakeUser() { UserName = "user1" };
            _workContextAccessor.GetContext().CurrentUser = user;

            Assert.That(_territoriesService.GetTerritoryTypes().Count(), Is.EqualTo(3));
            Assert.That(_territoriesService.ListTerritoryTypePermissions().Count(), Is.EqualTo(1));
        }
    }
}
