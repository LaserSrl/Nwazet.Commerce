using Nwazet.Commerce.Services;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Nwazet.Commerce.Forms {
    public class OrderStatusForm : IFormProvider {
        private readonly IOrderService _orderService;
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public OrderStatusForm(
            IOrderService orderService,
            IShapeFactory shapeFactory) {
            _orderService = orderService;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, dynamic> form =
                shape => {
                    var f = Shape.Form(
                        Id: "AnyOfOrderStatus",
                        _Status: Shape.SelectList(
                            Id: "orderstatus",
                            Name: "OrderStatus",
                            Title: T("Order Status"),
                            Description: T("Select some order status"),
                            Size: 10,
                            Multiple: true
                        )
                    );

                    f._Status.Add(new SelectListItem { Value = "", Text = T("Any").Text });

                    var statusLabels = ((Dictionary<OrderStatus, LocalizedString>)_orderService.StatusLabels)
                        .ToDictionary(s => s.Key.StatusName, s => s.Value);
                    foreach (var k in statusLabels.Keys) {
                        f._Status.Add(new SelectListItem { Value = k, Text = statusLabels[k].Text });
                    }

                    return f;
                };

            context.Form("SelectOrderStatus", form);
        }
    }
}
