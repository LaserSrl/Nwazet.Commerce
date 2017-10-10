using Autofac;
using Moq;
using NUnit.Framework;
using Nwazet.Commerce.Services;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Settings.Metadata;
using Orchard.Security;
using Orchard.Tests.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Tests.Territories {
    [TestFixture]
    public class TerritoriesServiceTests : DatabaseEnabledTestsBase {

        private ITerritoriesService _territoriesService;
        private Mock<IAuthorizer> _authorizer;

        public override void Register(ContainerBuilder builder) {
            builder.RegisterType<ContentDefinitionManager>().As<IContentDefinitionManager>();
            builder.RegisterType<TerritoriesService>().As<ITerritoriesService>();

            _authorizer = new Mock<IAuthorizer>();
            builder.RegisterInstance(_authorizer.Object);

            _authorizer.Setup(x => x.Authorize(It.IsAny<Permission>(), It.IsAny<LocalizedString>())).Returns(true);
        }

        [Test]
        public void HierarchyManagePermissionsAreSameNumberAsHierarchyTypes() { }

        [Test]
        public void TerritoryManagePermissionsAreSameNumberAsTerritoryTypes() { }
    }
}
