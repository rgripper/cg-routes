/// <reference path="../../knockout/knockout.d.ts" />
import ko = require("knockout");
import $ = require("jquery");
import Utils = require("Utils");
import AdminApi = require("api/AdminApi");

type DayViewModel = { date: Date; selected: KnockoutObservable<boolean>; }
type WeekViewModel = DayViewModel[]

class CollectionCalendarViewModel {

    private weeks = ko.observable<WeekViewModel[]>([]);

    private dateFormatter = <Intl.DateTimeFormat><any>new Intl.DateTimeFormat('ru-RU', { day: 'numeric', month: 'long', year: 'numeric', hour12: false });

    public formatDate(value: Date) {
        return this.dateFormatter.format(<any>value);
    }

    public formatMonth(value: Date) {
        if (!value) {
            return null;
        }
        return value.toLocaleString('ru-RU', { month: 'long', hour12: false });
    }

    public firstDate = ko.computed(() => {
        var weeks = this.weeks();
        if (weeks.length === 0) {
            return null;
        }
        var lastWeek = weeks[0];
        return lastWeek[0].date;
    });

    public lastDate = ko.computed(() => {
        var weeks = this.weeks();
        if (weeks.length === 0)
        {
            return null;
        }
        var lastWeek = weeks[weeks.length - 1];
        return lastWeek[lastWeek.length - 1].date;
    });

    constructor(params: { daysCount: number }) {
        var now = new Date();
        var startDate = new Date(Date.UTC(now.getFullYear(), now.getMonth(), now.getDate()));

        var endDate = this.addDays(startDate, params.daysCount);
        var endDateTime = endDate.getTime();

        AdminApi.getSelectedDates({ startDate: startDate, endDate: endDate }).done(selectedDatesStrings => {
            var selectedTimes = selectedDatesStrings.map(x => Date.parse(x));
            this.weeks(this.createWeeks(startDate, endDate, selectedTimes));
        });
    }

    private getDateOnlyUTC(value: Date): Date {
        return new Date(new Date(value.getFullYear(), value.getMonth(), value.getDate()).getTime() - value.getTimezoneOffset() * 60 * 1000);
    }

    private createWeeks(startDate: Date, endDate: Date, selectedTimes: number[]): WeekViewModel[]{
        var days = this.getDays(startDate, endDate);
        var weeks = this.chunk(days, 7);

        var now = new Date();
        var todayTime = this.getDateOnlyUTC(now).getTime();

        return weeks.map(x => x.map((d, i) => ({
            date: d,
            selected: ko.observable(selectedTimes.indexOf(d.getTime()) > -1),
            today: d.getTime() === todayTime,
            past: d.getTime() < todayTime,
            thisMonth: d.getMonth() === now.getMonth(),
            toggle: Utils.observeExec(function () {
                var newState = !this.selected();
                return AdminApi.postDateSelection({ date: this.date, selected: newState }).done(() => this.selected(newState));
            })
        })));
    }

    private getDays(startDate: Date, endDate: Date): Date[] {
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
    }

    private chunk <T>(array: T[], chunkSize): T[][] {
        return Array.prototype.concat.apply([], array.map((elem, i) => i % chunkSize ? [] : [array.slice(i, i + chunkSize)]));
    }

    private getMonday(value: Date): Date {
        var day = value.getDay(),
            diff = value.getDate() - day + (day == 0 ? -6 : 1); // adjust when day is sunday
        return new Date(value.setDate(diff));
    }

    private addDays(date, days): Date {
        var result = new Date(date);
        result.setDate(date.getDate() + days);
        return result;
    }

}

export = CollectionCalendarViewModel