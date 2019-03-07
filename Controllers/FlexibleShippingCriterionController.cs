using Nwazet.Commerce.Models;
using Nwazet.Commerce.Permissions;
using Nwazet.Commerce.Services;
using Nwazet.Commerce.ViewModels;
using Orchard.ContentManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Admin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Nwazet.Commerce.Controllers {
    [ValidateInput(false), Admin]
    public class FlexibleShippingCriterionController : Controller {
        private readonly IAuthorizer _authorizer;
        private readonly IFlexibleShippingManager _flexibleShippingManager;
        private readonly IFormManager _formManager;
        private readonly IContentManager _contentManager;

        public FlexibleShippingCriterionController(
            IAuthorizer authorizer,
            IFlexibleShippingManager flexibleShippingManager,
            IFormManager formManager,
            IContentManager contentManager) {

            _authorizer = authorizer;
            _flexibleShippingManager = flexibleShippingManager;
            _formManager = formManager;
            _contentManager = contentManager;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Add(int id) {
            if (!_authorizer.Authorize(
                CommercePermissions.ManageShipping, 
                T("Not authorized to manage shipping methods"))) {
                return new HttpUnauthorizedResult();
            }
            var viewModel = new CriteriaAddViewModel {
                Id = id,
                Criteria = _flexibleShippingManager
                    .DescribeCriteria()
            };
            return View(viewModel);
        }

        public ActionResult Edit(int id, string category, string type, int criterionId = -1) {
            if (!_authorizer.Authorize(
                CommercePermissions.ManageShipping,
                T("Not authorized to manage shipping methods"))) {
                return new HttpUnauthorizedResult();
            }

            // Get the descriptor for the criterion we are editing
            var criterion = _flexibleShippingManager
                .GetCriteria(category, type);
            if (criterion == null) {
                return HttpNotFound();
            }
            // build the form, and let external components alter it
            var form = criterion.Form == null 
                ? null 
                : _formManager.Build(criterion.Form);

            string description = "";
            // bind form with existing values.
            if (criterionId != -1) {
                // get the shipping method
                var part = _contentManager.Get<FlexibleShippingMethodPart>(id);
                if (part == null) {
                    // weird error condition
                    return HttpNotFound();
                }
                var critRecord = part
                    .ApplicabilityCriteria
                    .FirstOrDefault(ac => ac.Id == criterionId);
                if (critRecord != null) {
                    description = critRecord.Description;
                    var parameters = FormParametersHelper.FromString(critRecord.State);
                    _formManager.Bind(form, 
                        new DictionaryValueProvider<string>(parameters, CultureInfo.InvariantCulture));
                }
            }

            var viewModel = new CriterionEditViewModel {
                Id = id,
                Description = description,
                Criterion = criterion,
                Form = form };
            return View(viewModel);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(
            int id, 
            string category, 
            string type, 
            [DefaultValue(-1)] int criterionId, 
            FormCollection formCollection) {

            if (!_authorizer.Authorize(
                CommercePermissions.ManageShipping,
                T("Not authorized to manage shipping methods"))) {
                return new HttpUnauthorizedResult();
            }

            // get the shipping method
            var ci = _contentManager.Get(id);
            var part = ci.As<FlexibleShippingMethodPart>();
            if (part == null) {
                // weird error condition
                return HttpNotFound();
            }

            // Get the criterion
            var criterion = _flexibleShippingManager
                .GetCriteria(category, type);

            var model = new CriterionEditViewModel();
            TryUpdateModel(model);

            // validating form values
            _formManager.Validate(new ValidatingContext {
                FormName = criterion.Form,
                ModelState = ModelState,
                ValueProvider = ValueProvider });

            if (ModelState.IsValid) {
                var criterionRecord = part.ApplicabilityCriteria
                    .Where(f => f.Id == criterionId)
                    .FirstOrDefault();

                // add new filter record if it's a newly created filter
                if (criterionRecord == null) {
                    criterionRecord = new ApplicabilityCriterionRecord {
                        Category = category,
                        Type = type
                    };
                    part.ApplicabilityCriteria.Add(criterionRecord);
                }

                var dictionary = formCollection.AllKeys
                    .ToDictionary(key => key, formCollection.Get);

                // save form parameters
                criterionRecord.State = FormParametersHelper.ToString(dictionary);
                criterionRecord.Description = model.Description;

                return RedirectToAction("Edit", _contentManager.GetItemMetadata(ci).EditorRouteValues);
            }

            // model is invalid, display it again
            var form = _formManager.Build(criterion.Form);

            _formManager.Bind(form, formCollection);
            var viewModel = new CriterionEditViewModel {
                Id = id,
                Description = model.Description,
                Criterion = criterion,
                Form = form };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Delete(int id, int criterionId) {
            if (!_authorizer.Authorize(
                CommercePermissions.ManageShipping,
                T("Not authorized to manage shipping methods"))) {
                return new HttpUnauthorizedResult();
            }

            // get the shipping method
            var ci = _contentManager.Get(id);
            var part = ci.As<FlexibleShippingMethodPart>();
            if (part == null) {
                // weird error condition
                return HttpNotFound();
            }
            if (criterionId != -1) {
                var critRecord = part
                    .ApplicabilityCriteria
                    .FirstOrDefault(ac => ac.Id == criterionId);
                if (critRecord == null) {
                    // weird error condition
                    return HttpNotFound();
                }
                // actually delete
                _flexibleShippingManager.DeleteCriterion(criterionId);
            }

            return RedirectToAction("Edit", _contentManager.GetItemMetadata(ci).EditorRouteValues);
        }
    }
}
