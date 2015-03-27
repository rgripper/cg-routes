define(["require", "exports", "knockout", "leaflet", "jquery", "utils"], function (require, exports, ko, L, $, utils) {
    var ClientAddressViewModel = (function () {
        function ClientAddressViewModel() {
            var _this = this;
            this.address = ko.observable("открытое шоссе, 6");
            this.markerInfos = ko.observableArray([]);
            this.isLoading = ko.observable(false);
            this.imagePath = "/content/images";
            this.icons = {
                selected: new L.Icon(this.createIconOptions(this.imagePath + "/marker-icon-green.png")),
                default: new L.Icon(this.createIconOptions(this.imagePath + "/marker-icon.png"))
            };
            this.selectedMarkerWrapper = ko.observable();
            this.coords = ko.computed(function () { return _this.selectedMarkerWrapper() ? _this.selectedMarkerWrapper().marker.getLatLng() : null; });
            this.map = ko.observable();
            this.actions = utils.observeExec({
                initLocation: function (map) {
                    var currentCoords = null;
                    var exactLocation = !!currentCoords;
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
                    return deferred.done(function (coords) {
                        map.setView(coords, 12);
                        // add an OpenStreetMap tile layer
                        L.tileLayer('http://{s}.tile.osm.org/{z}/{x}/{y}.png', {
                            attribution: '&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
                        }).addTo(map);
                        if (exactLocation) {
                            _this.markerInfos.push(_this.createSelectableMarkerWrapper({ lat: coords.lat, lon: coords.lng }));
                        }
                    });
                },
                locate: function () {
                    _this.isLoading(true);
                    return $.ajax({
                        url: "http://nominatim.openstreetmap.org/search/ru/moscow/",
                        data: { q: _this.address(), format: "json" }
                    }).done(function (result) {
                        _this.markerInfos.removeAll();
                        ko.utils.arrayPushAll(_this.markerInfos, result.map(function (x) { return _this.createSelectableMarkerWrapper(x); }));
                    }).always(function () { return _this.isLoading(false); });
                },
                save: function () {
                    var coords = _this.coords();
                    if (!coords) {
                        throw new Error("coords");
                    }
                    return $.post("/api/Client/ChangeClientAddress", { latitude: coords.lat, longitude: coords.lng, address: _this.address() });
                }
            });
            this.map.subscribe(function (newMap) {
                _this.initMarkerProperties(newMap);
                _this.actions.initLocation(newMap);
            });
        }
        ClientAddressViewModel.prototype.createIconOptions = function (iconUrl) {
            return {
                iconSize: new L.Point(25, 41),
                iconAnchor: new L.Point(12, 41),
                popupAnchor: new L.Point(1, -34),
                shadowSize: new L.Point(41, 41),
                iconUrl: iconUrl,
            };
        };
        ClientAddressViewModel.prototype.initMarkerProperties = function (map) {
            var _this = this;
            this.markerInfos.subscribe(function (changes) {
                changes.forEach(function (change) {
                    switch (change.status) {
                        case "added":
                            map.addLayer(change.value.marker);
                            return;
                        case "deleted":
                            map.removeLayer(change.value.marker);
                            return;
                    }
                });
            }, null, "arrayChange");
            this.selectedMarkerWrapper.subscribeChanged(function (newValue, oldValue) {
                if (newValue === oldValue)
                    return;
                if (oldValue)
                    oldValue.marker.setIcon(_this.icons.default);
                newValue.marker.setIcon(_this.icons.selected);
            });
            this.markerInfos.subscribe(function () {
                var markers = _this.markerInfos().map(function (x) { return x.marker; });
                if (markers.length < 1)
                    return;
                var bounds = new L.FeatureGroup(markers).getBounds();
                map.fitBounds(bounds);
            });
        };
        ClientAddressViewModel.prototype.createSelectableMarkerWrapper = function (value) {
            var _this = this;
            var wrapper = {
                marker: new L.Marker(new L.LatLng(value.lat, value.lon), { icon: this.icons.default, draggable: true }),
                data: value
            };
            wrapper.marker.on('click', function () { return _this.selectedMarkerWrapper(wrapper); });
            wrapper.marker.on('dragstart', function () { return _this.selectedMarkerWrapper(wrapper); });
            return wrapper;
        };
        return ClientAddressViewModel;
    })();
    return ClientAddressViewModel;
});
//# sourceMappingURL=ClientAddress.js.map