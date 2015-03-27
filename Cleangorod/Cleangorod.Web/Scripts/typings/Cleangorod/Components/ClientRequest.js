define(["require", "exports"], function (require, exports) {
    var CollectionScheduleViewModel = (function () {
        function CollectionScheduleViewModel() {
            this.dateFormatter = new Intl.DateTimeFormat('ru-RU', { hour: 'numeric', minute: 'numeric', hour12: false });
        }
        CollectionScheduleViewModel.prototype.formatDate = function (value) {
            return this.dateFormatter.format(value);
        };
        return CollectionScheduleViewModel;
    })();
    return CollectionScheduleViewModel;
});
//# sourceMappingURL=ClientRequest.js.map