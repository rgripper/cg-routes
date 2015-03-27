interface Schedule {
    id?: number;
    weekStart: string;
    ranges: DateTimeRange[];
    note: string;
}

interface DateTimeRange {
    id?: number;
    date: Date;
    startHour: number;
    endHour: number;
    selected: boolean
}

class ClientApi {
    public static postSchedule(data: { scheduleId: number; weekStart: Date; note: string; rangeIds: number[] }) {
        return $.ajax({
            url: "/api/Client/PostSchedule",
            data: JSON.stringify(data),
            type: "POST",
            contentType: "application/json"
        })
    }

    public static getSchedule(): JQueryPromise<Schedule> {
        return $.get("/api/Client/GetSchedule");
    }
}

export = ClientApi;