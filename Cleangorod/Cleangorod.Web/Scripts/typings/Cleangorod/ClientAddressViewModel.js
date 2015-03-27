define(["require", "exports", "knockout"], function (require, exports, ko) {
    var ClientAddressViewModel = (function () {
        function ClientAddressViewModel(map) {
            this.map = map;
            this.address = ko.observable("");
            this.latitude = ko.observable();
            this.longitude = ko.observable();
            this.markerInfos = ko.observableArray();
            this.markerInfos.subscribe(function (change) {
                switch (change.status) {
                    case "added":
                        map.addLayer(change.value.marker);
                        return;
                    case "deleted":
                        map.removeLayer(change.value.marker);
                        return;
                }
            }, null, "arrayChange");
        }
        ClientAddressViewModel.prototype.locate = function () {
            var _this = this;
            $.ajax({
                url: "http://nominatim.openstreetmap.org/search/ru/moscow/",
                data: { q: this.address() }
            }).done(function (result) {
                _this.markerInfos.removeAll();
                result.forEach(function (x) { return _this.markerInfos.push({
                    data: x,
                    marker: new L.Marker(new L.LatLng(x.lat, x.lon))
                }); });
            });
        };
        return ClientAddressViewModel;
    })();
    exports.ClientAddressViewModel = ClientAddressViewModel;
    var currentCoords = null;
    var moscowCoords = new L.LatLng(55.7451004, 37.6254992);
    var deferred = $.Deferred();
    if ("geolocation" in navigator) {
        /* geolocation is available */
        navigator.geolocation.getCurrentPosition(function (x) { return deferred.resolve(new L.LatLng(x.coords.latitude, x.coords.longitude)); }, function (x) { return deferred.resolve(moscowCoords); });
    }
    else if (currentCoords) {
        deferred.resolve(currentCoords);
    }
    else {
        deferred.resolve(moscowCoords);
    }
    var viewModel = new ClientAddressViewModel(L.map('map'));
    deferred.done(function (coords) {
        var map = viewModel.map.setView(coords, 12);
        // add an OpenStreetMap tile layer
        L.tileLayer('http://{s}.tile.osm.org/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
        }).addTo(map);
        viewModel.markerInfos.push({ data: null, marker: L.marker(coords) });
    });
    ko.applyBindings(viewModel);
});
//# sourceMappingURL=ClientAddressViewModel.js.map