interface DateTimeRange {
    id?: number;
    date: Date;
    startHour: number;
    endHour: number;
    selected: boolean
}

interface RouteClient {
    ranges: DateTimeRange[];
    address: string;
    latitude: number;
    longitude: number;
}

class AdminApi {
    public static postDateSelection(data: { date: Date; selected: boolean; }) {
        return $.ajax({
            url: "/api/Admin/PostDateSelection",
            data: JSON.stringify(data),
            type: "POST",
            contentType: "application/json"
        })
    }

    public static getSelectedDates(data: { startDate: Date; endDate: Date; }): JQueryPromise<string[]> {
        return $.get("/api/Admin/GetSelectedDates", { startDate: data.startDate.toISOString(), endDate: data.endDate.toISOString() });
    }

    public static getRouteClientsForDate(data: { date: Date }): JQueryPromise<RouteClient[]> {
        return $.get("/api/Admin/GetRouteClientsForDate", { date: data.date.toISOString() });
    }

}

export = AdminApi;