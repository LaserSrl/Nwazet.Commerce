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
                $(nodeid).append(response);
                $(progressiveTerritoryIndexElement).val(parseInt($(progressiveTerritoryIndexElement).val()) + $(nodeid).children("ol").find(".ajax-expand-node").length);
            },
            error: function (error) {
                alert();
            }
        });
    } else {
        $(nodeid).children("ol").toggle();
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

        stop: function (event, ui) {
            // update all positions whenever a menu item was moved
            populate(this, "0");
            idNode = ui.item.data("index");
            if (("," + $(updatedTerritoryIdsElement).val() + ",").indexOf("," + idNode + ",") == -1) {
                $(updatedTerritoryIdsElement).val($(updatedTerritoryIdsElement).val() + "," + idNode);
            }
            $('#save-message').show();

            // display a message on leave if changes have been made
            window.onbeforeunload = function (e) {
                return $("<div/>").html(leaveConfirmation).text();
            };

            // cancel leaving message on save
            $('#saveButton').click(function (e) {
                window.onbeforeunload = function () { };
            });
        }
    });



})(jQuery);

