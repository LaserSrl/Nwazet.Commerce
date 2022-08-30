$(function () {
    // We need to configure things so that when a specific product variant/combination
    // is selected by the user, all information displayed is updated accordingly.

    // Find the "add to cart" form for the product
    function findAddToCartForm(productId) {
        // the html elements for the selection of the combination should be inside that form
        return $('form[data-form-role="addtocart"][data-productid="' + productId + '"]');
    }
    // 
    function updateProductInformation(productId, combinationId) {
        var $form = findAddToCartForm(productId);
        // set the correct value of the id to add the product to the cart.
        var $prodId = $form.find('[name="id"]');
        $prodId.val(combinationId);
        // Replace "accessory" information based on the stuff from the
        // newly selected combination.
        // Get the "target" elements in which we'll replace information.
        var allInfoElements =
            $('body [data-commerce-product="' + productId + '"]');
        // Get the "source" elements with the information for the combination.
        var newInfoElements =
            $('head script[data-for-product="' + productId + '"][data-for-combination="' + combinationId + '"]');
        // Replace target elements by matching the data-commerce-role attribute.
        allInfoElements.each(function (index, element) {
            var $element = $(element);
            var roleKey = $element.data('commerce-role');
            if (roleKey) {
                var $newElement = newInfoElements.filter('[data-commerce-role="' + roleKey + '"]');
                if ($newElement && $newElement.length) {
                    $element.html($newElement.html());
                    var eventToTrigger = $element.data('commerce-event');
                    if (eventToTrigger) {
                        $element.trigger(eventToTrigger);
                    }
                }
            }
        });
    }

    $('[data-input-role="combination_id"][data-for-product]').on('change', function () {
        // This depends on the 'change' event being triggered.
        // That event may have to be triggered explicitly on elements if their
        // value is being changed by code.
        var prodId = $(this).data('for-product');
        var combId = $(this).val();
        updateProductInformation(prodId, combId);
        //goToCombinationSlides(prodId, combId);
    });
    // Use the default combination as the "selected" product
    $('[data-input-role="combination_id"][data-for-product]').each(function (index, element) {
        var prodId = $(element).data('for-product');
        var combId = $(element).val();
        updateProductInformation(prodId, combId);
    });
});