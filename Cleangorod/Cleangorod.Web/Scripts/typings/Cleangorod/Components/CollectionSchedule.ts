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
        return this.dateFormatter.format(<any>value);
    }

    constructor(public date: Date) {
    }
}

class ClientScheduleViewModel {

    private visits = ko.observableArray<ClientScheduleDayViewModel>([]);

    public note = ko.observable("");

    public scheduleId: number;

    public weekStart: Date;

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
            });
        },
        load: () => {
            return ClientApi.getSchedule().done(schedule => {
                var dateSet = {};
                schedule.ranges.forEach(r => {
                    dateSet[<any>r.date] = true;
                    r.date = new Date(<any>r.date);
                });

                Object.keys(dateSet).forEach(key => {
                    var date = new Date(key);
                    var visit = new ClientScheduleDayViewModel(date);
                    schedule.ranges
                        .filter(r => date.getTime() === r.date.getTime())
                        .forEach(r => visit.timeRanges.push(new CollectionTimeRangeViewModel(r.id, r.startHour, r.endHour, r.selected)));
                    
                    this.visits.push(visit);
                })
            });
        }
    });

    constructor() {
        this.actions.load();
    }
}

export = ClientScheduleViewModel