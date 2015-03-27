/// <reference path="../../knockout/knockout.d.ts" />
import ko = require("knockout");
import $ = require("jquery");
import Utils = require("Utils");
import ClientApi = require("api/ClientApi");

class CollectionTimeRangeViewModel {
    public selected = ko.observable(false);

    public formatHour(value: number) {
        return this.padZero(value) + ":00";
    }

    private padZero(value: number): string {
        return (value < 10 ? "0" : "") + value;
    }

    constructor(public id: number, public startHour: number, public endHour: number, selected: boolean) {
        this.selected(selected);
    }
}

class ClientScheduleDayViewModel {

    public timeRanges = ko.observableArray<CollectionTimeRangeViewModel>([]);

    private dateFormatter = <Intl.DateTimeFormat><any>new Intl.DateTimeFormat('ru-RU', { day: 'numeric', month: 'long', year: 'numeric', hour12: false });

    public formatDate(value: Date) {
        var dayMonth = value.toLocaleString('ru-RU', { day: 'numeric', month: 'long', hour12: false });
        var weekday = value.toLocaleString('ru-RU', { weekday: 'long', hour12: false });
        return `${dayMonth} (${weekday.toLowerCase()})`;
    }

    constructor(public date: Date) {
    }
}

class ClientScheduleViewModel {

    private visits = ko.observableArray<ClientScheduleDayViewModel>([]);

    public note = ko.observable("");

    public scheduleId: number;

    public weekStart: Date;

    public saveMessage = Utils.resettableObservable<{ isError?: boolean; isSuccess?: boolean; value: string }>({ value: null }, 1500, 1000);

    public actions = Utils.observeExec({
        save: () => {
            var rangeIds = this.visits()
                .map(x => x.timeRanges()
                    .filter(tr => tr.selected())
                    .map(tr => tr.id));

            return ClientApi.postSchedule({
                scheduleId: this.scheduleId,
                weekStart: this.weekStart,
                note: this.note(),
                rangeIds: Array.prototype.concat.apply([], rangeIds),
            })
                .done(() => this.saveMessage({ value: "Изменения отправлены", isSuccess: true }))
                .fail(x => this.saveMessage({ value: "Ошибка при отправлении", isError: true }));
        },
        load: () => {
            return ClientApi.getSchedule().done(schedule => {
                this.scheduleId = schedule.id;
                this.weekStart = new Date(schedule.weekStart);

                this.note(schedule.note);

                var dateSet = {};
                schedule.ranges.forEach(r => {
                    dateSet[<any>r.date] = true;
                    r.date = new Date(<any>r.date);
                });

                Object.keys(dateSet).forEach(key => {
                    var date = new Date(key);
                    var visit = new ClientScheduleDayViewModel(date);
                    var rangeViewModels = schedule.ranges
                        .filter(r => date.getTime() === r.date.getTime())
                        .map(r => new CollectionTimeRangeViewModel(r.id, r.startHour, r.endHour, r.selected))
                        .sort((a, b) => a.startHour - b.startHour);

                    ko.utils.arrayPushAll(visit.timeRanges, rangeViewModels); 
                    this.visits.push(visit);
                });
            });
        }
    });

    constructor() {
        this.actions.load();
    }
}

export = ClientScheduleViewModel