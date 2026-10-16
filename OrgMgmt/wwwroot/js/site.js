

// Turn every [data-datepicker] field into a flatpickr calendar. The native
// <input type="date"> control cannot be driven by WebDriver, so the date of
// birth fields use this widget instead: it is real DOM, so both people and
// Selenium can pick a date from it.
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('[data-datepicker]').forEach(function (input) {
        flatpickr(input, {
            dateFormat: 'Y-m-d',
            allowInput: true,
            monthSelectorType: 'dropdown'
        });
    });
});
