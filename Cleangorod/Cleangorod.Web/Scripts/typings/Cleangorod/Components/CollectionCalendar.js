define(["require", "exports", "knockout", "Utils", "api/AdminApi"], function (require, exports, ko, Utils, AdminApi) {
    var CollectionCalendarViewModel = (function () {
        function CollectionCalendarViewModel(params) {
            var _this = this;
            this.weeks = ko.observable([]);
            this.dateFormatter = new Intl.DateTimeFormat('ru-RU', { day: 'numeric', month: 'long', year: 'numeric', hour12: false });
            this.firstDate = ko.computed(function () {
                var weeks = _this.weeks();
                if (weeks.length === 0) {
                    return null;
                }
                var lastWeek = weeks[0];
                return lastWeek[0].date;
            });
            this.lastDate = ko.computed(function () {
                var weeks = _this.weeks();
                if (weeks.length === 0) {
                    return null;
                }
                var lastWeek = weeks[weeks.length - 1];
                return lastWeek[lastWeek.length - 1].date;
            });
            var now = new Date();
            var startDate = new Date(Date.UTC(now.getFullYear(), now.getMonth(), now.getDate()));
            var endDate = this.addDays(startDate, params.daysCount);
            var endDateTime = endDate.getTime();
            AdminApi.getSelectedDates({ startDate: startDate, endDate: endDate }).done(function (selectedDatesStrings) {
                var selectedTimes = selectedDatesStrings.map(function (x) { return Date.parse(x); });
                _this.weeks(_this.createWeeks(startDate, endDate, selectedTimes));
            });
        }
        CollectionCalendarViewModel.prototype.formatDate = function (value) {
            return this.dateFormatter.format(value);
        };
        CollectionCalendarViewModel.prototype.formatMonth = function (value) {
            if (!value) {
                return null;
            }
            return value.toLocaleString('ru-RU', { month: 'long', hour12: false });
        };
        CollectionCalendarViewModel.prototype.getDateOnlyUTC = function (value) {
            return new Date(new Date(value.getFullYear(), value.getMonth(), value.getDate()).getTime() - value.getTimezoneOffset() * 60 * 1000);
        };
        CollectionCalendarViewModel.prototype.createWeeks = function (startDate, endDate, selectedTimes) {
            var days = this.getDays(startDate, endDate);
            var weeks = this.chunk(days, 7);
            var now = new Date();
            var todayTime = this.getDateOnlyUTC(now).getTime();
            return weeks.map(function (x) { return x.map(function (d, i) { return ({
                date: d,
                selected: ko.observable(selectedTimes.indexOf(d.getTime()) > -1),
                today: d.getTime() === todayTime,
                past: d.getTime() < todayTime,
                thisMonth: d.getMonth() === now.getMonth(),
                toggle: Utils.observeExec(function () {
                    var _this = this;
                    var newState = !this.selected();
                    return AdminApi.postDateSelection({ date: this.date, selected: newState }).done(function () { return _this.selected(newState); });
                })
            }); }); });
        };
        CollectionCalendarViewModel.prototype.getDays = function (startDate, endDate) {
            var lastSunday = this.addDays(this.getMonday(endDate), 6);
            var lastSundayTime = lastSunday.getTime();
            var date = this.getMonday(startDate);
            var dates = [];
            while (date.getTime() < lastSundayTime) {
                dates.push(date);
                date = this.addDays(date, 1);
            }
            dates.push(lastSunday);
            return dates;
        };
        CollectionCalendarViewModel.prototype.chunk = function (array, chunkSize) {
            return Array.prototype.concat.apply([], array.map(function (elem, i) { return i % chunkSize ? [] : [array.slice(i, i + chunkSize)]; }));
        };
        CollectionCalendarViewModel.prototype.getMonday = function (value) {
            var day = value.getDay(), diff = value.getDate() - day + (day == 0 ? -6 : 1); // adjust when day is sunday
            return new Date(value.setDate(diff));
        };
        CollectionCalendarViewModel.prototype.addDays = function (date, days) {
            var result = new Date(date);
            result.setDate(date.getDate() + days);
            return result;
        };
        return CollectionCalendarViewModel;
    })();
    return CollectionCalendarViewModel;
});
//# sourceMappingURL=CollectionCalendar.js.map