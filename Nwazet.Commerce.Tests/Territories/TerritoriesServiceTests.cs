using Autofac;
using Moq;
using NUnit.Framework;
using Nwazet.Commerce.Exceptions;
using Nwazet.Commerce.Handlers;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Permissions;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.Tests.Stubs;
using Nwazet.Commerce.Tests.Territories.Handlers;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Tests.Territories {
    [TestFixture]
    public class TerritoriesServiceTests : DatabaseEnabledTestsBase {

        private ITerritoriesService _territoriesService;
        private ITerritoriesRepositoryService _territoryRepositoryService;
        private ITerritoriesPermissionProvider _permissionProvider;
        private Mock<IOrchardServices> _mockServices;
        private IContentManager _contentManager;

        public override void Init() {
            _mockServices = new Mock<IOrchardServices>();
            base.Init();

            _mockServices.SetupGet(ms => ms.Authorizer).Returns(_container.Resolve<IAuthorizer>());

            _territoriesService = _container.Resolve<ITerritoriesService>();
            _territoryRepositoryService = _container.Resolve<ITerritoriesRepositoryService>();
            _permissionProvider= _container.Resolve<ITerritoriesPermissionProvider>();
            _contentManager = _container.Resolve<IContentManager>();
        }

        public override void Register(ContainerBuilder builder) {

            builder.RegisterType<TerritoriesService>().As<ITerritoriesService>();
            builder.RegisterType<TerritoryRepositoryService>().As<ITerritoriesRepositoryService>();
            builder.RegisterType<TerritoriesPermissions>().As<ITerritoriesPermissionProvider>();

            //for TerritoriesService
            var mockDefinitionManager = new Mock<IContentDefinitionManager>();
            mockDefinitionManager
                .Setup<IEnumerable<ContentTypeDefinition>>(mdm => mdm.ListTypeDefinitions())
                .Returns(MockTypeDefinitions);
            builder.RegisterInstance(mockDefinitionManager.Object);

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
            builder.RegisterType<DefaultContentQuery>().As<IContentQuery>();

            var _workContext = new Mock<WorkContext>();
            _workContext.Setup(w =>
                w.GetState<IUser>(It.Is<string>(s => s == "CurrentUser"))).Returns(() => { return _currentUser; });

            var _workContextAccessor = new Mock<IWorkContextAccessor>();
            _workContextAccessor.Setup(w => w.GetContext()).Returns(_workContext.Object);
            builder.RegisterInstance(_workContextAccessor.Object).As<IWorkContextAccessor>();

            //Handlers
            builder.RegisterType<TerritoryHierarchyPartHandler>().As<IContentHandler>();
            builder.RegisterType<TerritoryHierachyMockHandler>().As<IContentHandler>();
            builder.RegisterType<TerritoryPartHandler>().As<IContentHandler>();
            builder.RegisterType<TerritoryMockHandler>().As<IContentHandler>();
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
        
        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] {
                    typeof(TerritoryInternalRecord),
                    typeof(ContentItemVersionRecord),
                    typeof(ContentItemRecord),
                    typeof(ContentTypeRecord),
                    typeof(TerritoryHierarchyPartRecord),
                    typeof(TerritoryPartRecord)
                };
            }
        }

        private IUser _currentUser;

        [Test]
        public void HierarchyManagePermissionsAreSameNumberAsHierarchyTypesForAdmin() {

            _currentUser = new FakeUser() { UserName = "admin" };

            Assert.That(_territoriesService.GetHierarchyTypes().Count(), Is.EqualTo(3));
            Assert.That(_permissionProvider.ListHierarchyTypePermissions().Count(), Is.EqualTo(3));
        }

        [Test]
        public void TerritoryManagePermissionsAreSameNumberAsTerritoryTypesForAdmin() {

            _currentUser = new FakeUser() { UserName = "admin" };

            Assert.That(_territoriesService.GetTerritoryTypes().Count(), Is.EqualTo(3));
            Assert.That(_permissionProvider.ListTerritoryTypePermissions().Count(), Is.EqualTo(3));
        }

        [Test]
        public void HierarchyManagePermissionsAreNotSameNumberAsHierarchyTypes() {

            _currentUser = new FakeUser() { UserName = "user1" };

            Assert.That(_territoriesService.GetHierarchyTypes().Count(), Is.EqualTo(1));
            Assert.That(_permissionProvider.ListHierarchyTypePermissions().Count(), Is.EqualTo(3));
        }

        [Test]
        public void TerritoryManagePermissionsAreNotSameNumberAsTerritoryTypes() {

            _currentUser = new FakeUser() { UserName = "user1" };

            Assert.That(_territoriesService.GetTerritoryTypes().Count(), Is.EqualTo(1));
            Assert.That(_permissionProvider.ListTerritoryTypePermissions().Count(), Is.EqualTo(3));
        }
        
        private List<IContent> AddSampleHierarchiesData() {
            var items = new List<IContent> {
                _contentManager.Create<TerritoryHierarchyPart>("HierarchyType1", VersionOptions.Published),
                _contentManager.Create<TerritoryHierarchyPart>("HierarchyType2", VersionOptions.Draft),
                _contentManager.Create<TerritoryHierarchyPart>("HierarchyType0", VersionOptions.Published),
                _contentManager.Create<TerritoryHierarchyPart>("HierarchyType1", VersionOptions.Draft)
            };

            return items;
        }

        [Test]
        public void ParameterlessGetHierarchiesQueryReturnsAllLatestVersions() {
            var created = AddSampleHierarchiesData();
            //verify draft vs published

            var gotten = _territoriesService.GetHierarchiesQuery().List();
            Assert.That(gotten.Count() == created.Count());
            Assert.That(gotten
                .Select(pa => pa.ContentItem)
                .Where(ci => ci.IsPublished())
                .Count(), Is.EqualTo(2));
            Assert.That(gotten
                .Select(pa => pa.ContentItem)
                .Where(ci => !ci.IsPublished())
                .Count(), Is.EqualTo(2));
        }

        [Test]
        public void GetHierarchiesQueryReturnsSpecificVersions() {
            var created = AddSampleHierarchiesData();
            //try published separately from draft

            var gottenDraft = _territoriesService.GetHierarchiesQuery(VersionOptions.Draft).List();
            Assert.That(gottenDraft.Count(), Is.EqualTo(2));

            var gottenPub = _territoriesService.GetHierarchiesQuery(VersionOptions.Published).List();
            Assert.That(gottenPub.Count(), Is.EqualTo(2));
        }

        [Test]
        public void GetHierarchiesQueryDiscriminatesOnContentType() {
            var created = AddSampleHierarchiesData();

            var gotten0 = _territoriesService.GetHierarchiesQuery("HierarchyType0").List();
            Assert.That(gotten0.Count(), Is.EqualTo(1));

            var gotten1 = _territoriesService.GetHierarchiesQuery("HierarchyType1").List();
            Assert.That(gotten1.Count(), Is.EqualTo(2));

            var gotten02 = _territoriesService.GetHierarchiesQuery("HierarchyType0", "HierarchyType2").List();
            Assert.That(gotten02.Count(), Is.EqualTo(2));
        }

        private void AddSampleTerritoriesData() {
            var hierarchies = AddSampleHierarchiesData().ToArray();
            for (int i = 0; i < hierarchies.Length; i++) {
                var currentHierarchy = hierarchies[i].As<TerritoryHierarchyPart>();
                var territoryType = currentHierarchy.TerritoryType;
            }
        }
    }
}
