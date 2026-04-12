function initSelect2Multi(id, placeholder, dotNetRef) {
    // Get Select2 utilities and components
    var Utils = $.fn.select2.amd.require('select2/utils');
    var Dropdown = $.fn.select2.amd.require('select2/dropdown');
    var DropdownSearch = $.fn.select2.amd.require('select2/dropdown/search');
    var CloseOnSelect = $.fn.select2.amd.require('select2/dropdown/closeOnSelect');
    var AttachBody = $.fn.select2.amd.require('select2/dropdown/attachBody');

    // Decorate dropdown with search functionality
    var dropdownAdapter = Utils.Decorate(
        Utils.Decorate(
            Utils.Decorate(Dropdown, DropdownSearch),
            CloseOnSelect
        ),
        AttachBody
    );

    // Initialize Select2
    $('#' + id).select2({
        placeholder: placeholder,
        closeOnSelect: false,
        width: '100%',
        allowClear: false,
        dropdownAdapter: dropdownAdapter,
        minimumResultsForSearch: Infinity,
        matcher: function (params, data) {
            if ($.trim(params.term) === '') {
                return data;
            }
            if (typeof data.text === 'undefined') {
                return null;
            }
            if (data.text.toLowerCase().indexOf(params.term.toLowerCase()) > -1) {
                return data;
            }
            return null;
        },
        templateResult: function (data) {
            if (!data.id) return data.text;

            let isSelected = $('#' + id).val() && $('#' + id).val().includes(data.id);
            let checkbox = $('<input type="checkbox" class="select2-checkbox" data-id="' + data.id + '" style="margin-right: 8px;" />').prop('checked', isSelected);

            return $('<span style="display: flex; align-items: center;">').append(checkbox).append(data.text);
        },
        templateSelection: function () {
            return ''; // We won't show anything inside the input
        }
    });

    // Hide the default rendered area and inline search
    let selectElement = $('#' + id);
    let selectionContainer = selectElement.data('select2').$container.find('.select2-selection');
    selectionContainer.find('.select2-selection__rendered').hide();
    selectionContainer.find('.select2-search--inline').hide();

    // Add custom placeholder outside the input
    selectionContainer.append('<span class="custom-placeholder">' + placeholder + '</span>');

    // Add dropdown arrow
    selectionContainer.append('<span class="select2-selection__arrow" role="presentation" style="position:absolute; right:10px; top:50%; transform:translateY(-50%); width:20px; height:20px; cursor:pointer;">&#9662;</span>');

    // Handle checkbox clicks
    $(document).on('click', '.select2-checkbox', function (e) {
        e.stopPropagation();
        let optionId = $(this).data('id');
        let currentVal = $('#' + id).val() || [];

        if (currentVal.includes(optionId)) {
            currentVal = currentVal.filter(v => v !== optionId);
        } else {
            currentVal.push(optionId);
        }

        $('#' + id).val(currentVal).trigger('change');
    });

    // Prevent dropdown from closing on selection
    $('#' + id).on('select2:select select2:unselect', function (e) {
        e.preventDefault();
        $('#' + id).select2('open');
    });

    // Update checkboxes when selection changes
    $('#' + id).on('select2:select select2:unselect', function () {
        updateCheckboxes(id);
        notifyChanges(id, dotNetRef);
        $(this).select2('open');
    });

    // Focus search on dropdown open
    $('#' + id).on('select2:open', function () {
        setTimeout(function () {
            $('.select2-search__field').focus();
        }, 100);
    });
}

function updateCheckboxes(id) {
    let currentVal = $('#' + id).val() || [];
    $('.select2-checkbox').each(function () {
        let checkboxId = $(this).data('id');
        $(this).prop('checked', currentVal.includes(checkboxId));
    });
}

function notifyChanges(id, dotNetRef) {
    let selectedValues = $('#' + id).val() || [];
    dotNetRef.invokeMethodAsync('NotifySelectionChanged', selectedValues);
}

window.clearAllSelect2Multi = function () {
    // Find all select elements that have Select2 initialized
    $('select').each(function () {
        if ($(this).data('select2')) {
            $(this).val([]).trigger('change'); // Clear selections
        }
    });

    // Also update checkboxes
    $('.select2-checkbox').prop('checked', false);
};