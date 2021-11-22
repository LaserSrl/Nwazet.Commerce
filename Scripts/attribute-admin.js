$(function () {

    var i = $("#NwazetCommerceAttribute_AttributeValueRecords > li.option").length; // indexer for mvc model binding new tiers

    $("#NwazetCommerceAttribute_AddAttributeValue").click(function (event) {
        event.preventDefault();
        var valueTemplate = $("#valueTemplate").html();
        $("#NwazetCommerceAttribute_AttributeValueRecords")
            .append(Mustache.render(valueTemplate, { index: i, sort: i + 1 }))
            .last()
            .find(".option-name input[type=text]")
            .focus();
        i++;
    });

    $("#NwazetCommerceAttribute_AttributeValueRecords").on("click", ".nwazet-remove-attribute-value", function (event) {
        event.preventDefault();
        $(this).parents("li").remove();
    });

    $("#NwazetCommerceAttribute_AttributeValueRecords").sortable({
        update: function () {
            $.each($(this).children("li"), function () {
                var $row = $(this);
                $row.find("input[name$=SortOrder]").val($row.index());
            });
        }
    });
});