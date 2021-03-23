$(document).ready(function () {
    $("#territories-list").on("click", ".ajax-expand-node", TerritoryAjaxCall);
});

function TerritoryAjaxCall(e) {
    e.preventDefault();
    var currentElement = $(this);
    var nodeid = '#' + currentElement.data("node");
    if ($(nodeid).children("ol").find(".ajax-expand-node").length == 0) {
        $.ajax({
            type: "GET",
            url: $(this).attr('href'),
            data: {
                index: $(progressiveTerritoryIndexElement).val()
            },
            success: function (response) {
                if (!$.trim(response)) {//empty response
                    return;
                }
                $(nodeid).find(".ajax-expand-node").removeClass("glyphicon-plus");
                $(nodeid).find(".ajax-expand-node").addClass("glyphicon-minus");

                $(nodeid).append(response);
                $(progressiveTerritoryIndexElement).val(parseInt($(progressiveTerritoryIndexElement).val()) + $(nodeid).children("ol").find(".ajax-expand-node").length);
            },
            error: function (error) {
                alert();
            }
        });
    } else {
        $(nodeid).children("ol").toggle();
        var expandPanel = $('[data-node="'+currentElement.data("node")+'"]');
        if (expandPanel.hasClass("glyphicon-plus")) {
            expandPanel.removeClass("glyphicon-plus");
            expandPanel.addClass("glyphicon-minus");
        }
        else if (expandPanel.hasClass("glyphicon-minus")) {
            expandPanel.removeClass("glyphicon-minus");
            expandPanel.addClass("glyphicon-plus");
        }
    }
}

(function ($) {

    var populate = function (el, parentId) {

        // direct children
        var children = $(el).children('li').each(function (i, child) {
            child = $(child);

            // apply positions to all siblings
            child.find('.territory-parent > input').attr('value', parentId);

            // recurse position for children
            child.children('ol').each(function (i, item) {
                populate(item, child.attr('data-index'))
            });
        });
    };

    $('.territories-list > ol').nestedSortable({
        disableNesting: 'no-nest',
        forcePlaceholderSize: true,
        handle: 'div',
        helper: 'clone',
        items: 'li',
        maxLevels: 0,
        opacity: 1,
        placeholder: 'territory-placeholder',
        revert: 50,
        tabSize: 30,
        rtl: window.isRTL,
        tolerance: 'pointer',
        toleranceElement: '> div',

        start: function (event, ui) {
            // to check whether the parent has changed, save the parent during its selection
            idNode = ui.item.data("index");
            var parent = $('[data-index="' + idNode + '"]').find('.territory-parent > input');
            ui.item.data('start_parent', parent.val());
        },

        stop: function (event, ui) {
            // update all positions whenever a menu item was moved
            populate(this, "0");
            idNode = ui.item.data("index");

            if (("," + $(updatedTerritoryIdsElement).val() + ",").indexOf("," + idNode + ",") == -1) {
                $(updatedTerritoryIdsElement).val($(updatedTerritoryIdsElement).val() + "," + idNode);
            }

            // after repositioning I check if the father has changed
            var parent = $('[data-index="' + idNode + '"]').find('.territory-parent > input');
            if (ui.item.data("start_parent") !== parent.val()) {
                $('#save-message').show();

                // display a message on leave if changes have been made
                window.onbeforeunload = function (e) {
                    return $("<div/>").html(leaveConfirmation).text();
                };
            }

            // cancel leaving message on save
            $('#saveButton').click(function (e) {
                window.onbeforeunload = function () { };
            });
        }
    });



})(jQuery);

