$(function () {
    $("form[data-form-role='addtocart']")
        .on('nwazet.productunavailable', function () {
            // default behaviour: disable submits and hide the elements
            $(this).find('[type="submit"]').prop('disabled', true);
            // find the element that contains the inputs for quantity and the submit button
            var actionsWrapper = $(this).find('.addtocart-quantity-and-button');
            // if that isn't there, it's up to the theme to manage these conditions
            if (actionsWrapper.length) {
                actionsWrapper.hide();
            }
        })
        .on('nwazet.productavailable', function () {
            // default behaviour: show the elements
            // find the element that contains the inputs for quantity and the submit button
            var actionsWrapper = $(this).find('.addtocart-quantity-and-button');
            // if that isn't there, it's up to the theme to manage these conditions
            if (actionsWrapper.length) {
                actionsWrapper.show();
            }
        });
    $("form[data-form-role='addtocart']").each(function (index, element) {
        // for each form used to add a product to the cart
        // figure out whether the product is marked as unavailable
        var unavailables = $(element).find('[data-product-unavailable]');
        if (unavailables.length) {
            // product marked as unavailable
            $(element).trigger('nwazet.productunavailable');
        } else {
            // nothing told us the product is unavailable
            $(element).trigger('nwazet.productavailable');
        }
    });
});