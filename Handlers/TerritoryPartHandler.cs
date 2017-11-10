﻿using Nwazet.Commerce.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Handlers {
    [OrchardFeature("Territories")]
    public class TerritoryPartHandler : ContentHandler {

        private readonly IContentManager _contentManager;

        public TerritoryPartHandler(
            IRepository<TerritoryPartRecord> repository,
            IContentManager contentManager) {

            Filters.Add(StorageFilter.For(repository));

            ////Lazyfield setters
            //OnInitializing<TerritoryPart>(PropertySetHandlers);
            //OnInitializing<TerritoryPart>(LazyLoadHandlers);

            //Handle the presence of child territories: may need to run asynchronously
            //OnRemoved<TerritoryPart>(RemoveChildren);
        }

        //static void PropertySetHandlers(
        //    InitializingContentContext context, TerritoryPart part) {

        //    part.ChildrenField.Setter(value => {
        //        var actualItems = value
        //            .Where(ci => ci.As<TerritoryPart>() != null);
        //        part.Record.Children = actualItems.Any() ?
        //            actualItems.Select(ci => ci.As<TerritoryPart>().Record).ToList() :
        //            new List<TerritoryPartRecord>();
        //        return actualItems;
        //    });

        //    part.HierarchyField.Setter(hierarchy => {
        //        part.Record.Hierarchy = 
        //    });

        //    part.ParentField.Setter(value => { });


        //    part.TerritoriesField.Setter(value => {
        //        var actualItems = value.Where(ci => ci.As<TerritoryPart>() != null);
        //        part.Record.Territories = actualItems.Any() ?
        //            actualItems.Select(ci => ci.As<TerritoryPart>().Record).ToList() :
        //            new List<TerritoryPartRecord>();
        //        return actualItems;
        //    });

        //    //call the setters in case a value had already been set
        //    if (part.ChildrenField.Value != null) {
        //        part.ChildrenField.Value = part.ChildrenField.Value;
        //    }
        //    if (part.HierarchyField.Value != null) {
        //        part.HierarchyField.Value = part.HierarchyField.Value;
        //    }
        //    if (part.ParentField.Value != null) {
        //        part.ParentField.Value = part.ParentField.Value;
        //    }
        //}

        //void LazyLoadHandlers(
        //    InitializingContentContext context, TerritoryPart part) {

        //    part.ChildrenField.Loader(() => {
        //        if (part.Record.Children != null && part.Record.Children.Any()) {
        //            return _contentManager
        //                .GetMany<ContentItem>(part.Record.Children.Select(tpr => tpr.ContentItemRecord.Id),
        //                    VersionOptions.Latest, null);
        //        } else {
        //            return Enumerable.Empty<ContentItem>();
        //        }

        //    });
        //}

        //void RemoveChildren(RemoveContentContext context, TerritoryPart part) {
        //    // Only remove first level of children, because they will remove their children
        //    //foreach (var item in part.FirstLevel) {
        //    //    _contentManager.Remove(item);
        //    //}
        //}
    }
}
