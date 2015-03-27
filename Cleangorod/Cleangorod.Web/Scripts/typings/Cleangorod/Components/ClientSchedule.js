define(["require", "exports", "knockout", "Utils", "api/ClientApi"], function (require, exports, ko, Utils, ClientApi) {
    var CollectionTimeRangeViewModel = (function () {
        function CollectionTimeRangeViewModel(id, startHour, endHour, selected) {
            this.id = id;
            this.startHour = startHour;
            this.endHour = endHour;
            this.selected = ko.observable(false);
            this.selected(selected);
        }
        CollectionTimeRangeViewModel.prototype.formatHour = function (value) {
            return this.padZero(value) + ":00";
        };
        CollectionTimeRangeViewModel.prototype.padZero = function (value) {
            return (value < 10 ? "0" : "") + value;
        };
        return CollectionTimeRangeViewModel;
    })();
    var ClientScheduleDayViewModel = (function () {
        function ClientScheduleDayViewModel(date) {
            this.date = date;
            this.timeRanges = ko.observableArray([]);
            this.dateFormatter = new Intl.DateTimeFormat('ru-RU', { day: 'numeric', month: 'long', year: 'numeric', hour12: false });
        }
        ClientScheduleDayViewModel.prototype.formatDate = function (value) {
            var dayMonth = value.toLocaleString('ru-RU', { day: 'numeric', month: 'long', hour12: false });
            var weekday = value.toLocaleString('ru-RU', { weekday: 'long', hour12: false });
            return "" + dayMonth + " (" + weekday.toLowerCase() + ")";
        };
        return ClientScheduleDayViewModel;
    })();
    var ClientScheduleViewModel = (function () {
        function ClientScheduleViewModel() {
            var _this = this;
            this.visits = ko.observableArray([]);
            this.note = ko.observable("");
            this.saveMessage = Utils.resettableObservable({ value: null }, 1500, 1000);
            this.actions = Utils.observeExec({
                save: function () {
                    var rangeIds = _this.visits().map(function (x) { return x.timeRanges().filter(function (tr) { return tr.selected(); }).map(function (tr) { return tr.id; }); });
                    return ClientApi.postSchedule({
                        scheduleId: _this.scheduleId,
                        weekStart: _this.weekStart,
                        note: _this.note(),
                        rangeIds: Array.prototype.concat.apply([], rangeIds),
                    }).done(function () { return _this.saveMessage({ value: "Изменения отправлены", isSuccess: true }); }).fail(function (x) { return _this.saveMessage({ value: "Ошибка при отправлении", isError: true }); });
                },
                load: function () {
                    return ClientApi.getSchedule().done(function (schedule) {
                        _this.scheduleId = schedule.id;
                        _this.weekStart = new Date(schedule.weekStart);
                        _this.note(schedule.note);
                        var dateSet = {};
                        schedule.ranges.forEach(function (r) {
                            dateSet[r.date] = true;
                            r.date = new Date(r.date);
                        });
                        Object.keys(dateSet).forEach(function (key) {
                            var date = new Date(key);
                            var visit = new ClientScheduleDayViewModel(date);
                            var rangeViewModels = schedule.ranges.filter(function (r) { return date.getTime() === r.date.getTime(); }).map(function (r) { return new CollectionTimeRangeViewModel(r.id, r.startHour, r.endHour, r.selected); }).sort(function (a, b) { return a.startHour - b.startHour; });
                            ko.utils.arrayPushAll(visit.timeRanges, rangeViewModels);
                            _this.visits.push(visit);
                        });
                    });
                }
            });
            this.actions.load();
        }
        return ClientScheduleViewModel;
    })();
    return ClientScheduleViewModel;
});
//# sourceMappingURL=ClientSchedule.js.map