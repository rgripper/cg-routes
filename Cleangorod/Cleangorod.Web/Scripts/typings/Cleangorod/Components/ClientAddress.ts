/// <reference path="../../knockout/knockout.d.ts" />
import ko = require("knockout");
import L = require("leaflet");
import $ = require("jquery");
import utils = require("utils");

type INominatimSearchResultItem = { lat: number; lon: number; }
type INominatimSearchResult = INominatimSearchResultItem[]

interface IMarkerWrapper {
    data: INominatimSearchResultItem;
    marker: L.Marker;
}


class ClientAddressViewModel {
    public address = ko.observable("открытое шоссе, 6");

    public markerInfos = ko.observableArray<IMarkerWrapper>([]);
    public isLoading = ko.observable(false);

    private imagePath = "/content/images";

    private createIconOptions(iconUrl: string): L.IconOptions {
        return {
            iconSize: new L.Point(25, 41),
            iconAnchor: new L.Point(12, 41),
            popupAnchor: new L.Point(1, -34),
            shadowSize: new L.Point(41, 41),
            iconUrl: iconUrl,
        }
    }

    private icons = {
        selected: new L.Icon(this.createIconOptions(this.imagePath + "/marker-icon-green.png")),
        default: new L.Icon(this.createIconOptions(this.imagePath + "/marker-icon.png"))
    };

    private selectedMarkerWrapper = ko.observable<IMarkerWrapper>();

    public coords = ko.computed(() => this.selectedMarkerWrapper() ? this.selectedMarkerWrapper().marker.getLatLng() : null);

    public map = ko.observable<L.Map>();

    constructor() {
        this.map.subscribe(newMap => {
            this.initMarkerProperties(newMap);
            this.actions.initLocation(newMap);
        });
    }

    private initMarkerProperties(map: L.Map) {
        this.markerInfos.subscribe<KnockoutArrayChange<IMarkerWrapper>[]>(changes => {
            changes.forEach(change => {
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

        this.selectedMarkerWrapper.subscribeChanged((newValue, oldValue) => {
            if (newValue === oldValue) return;
            if (oldValue) oldValue.marker.setIcon(this.icons.default);
            newValue.marker.setIcon(this.icons.selected);
        });

        this.markerInfos.subscribe(() => {
            var markers = this.markerInfos().map(x => x.marker);
            if (markers.length < 1) return;
            var bounds = new L.FeatureGroup(markers).getBounds();
            map.fitBounds(bounds);
        });
    }

    public actions = utils.observeExec({
        initLocation: (map: L.Map) => {
            var currentCoords: L.LatLng = null;
            var exactLocation = !!currentCoords;

            var moscowCoords = new L.LatLng(55.7451004, 37.6254992);
            var deferred = $.Deferred<L.LatLng>();
            if ("geolocation" in navigator) {
                /* geolocation is available */
                navigator.geolocation.getCurrentPosition(
                    x => deferred.resolve(new L.LatLng(x.coords.latitude, x.coords.longitude)),
                    x => deferred.resolve(moscowCoords));
            }
            else if (currentCoords) {
                deferred.resolve(currentCoords);
            }
            else {
                deferred.resolve(moscowCoords);
            }

            return deferred.done(coords => {
                map.setView(coords, 12);

                // add an OpenStreetMap tile layer
                L.tileLayer('http://{s}.tile.osm.org/{z}/{x}/{y}.png', {
                    attribution: '&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
                }).addTo(map);

                if (exactLocation) {
                    this.markerInfos.push(this.createSelectableMarkerWrapper({ lat: coords.lat, lon: coords.lng }));
                }
            });
        },
        locate: () => {
            this.isLoading(true);
            return $.ajax({
                url: "http://nominatim.openstreetmap.org/search/ru/moscow/",
                data: { q: this.address(), format: "json" }
            }).done((result: INominatimSearchResult) => {
                this.markerInfos.removeAll();
                ko.utils.arrayPushAll(this.markerInfos, result.map(x => this.createSelectableMarkerWrapper(x)));
            }).always(() => this.isLoading(false));
        },
        save: () => {
            var coords = this.coords();
            if (!coords) {
                throw new Error("coords");
            }
            return $.post("/api/Client/ChangeClientAddress", { latitude: coords.lat, longitude: coords.lng, address: this.address() });
        }
    });

    private createSelectableMarkerWrapper(value?: INominatimSearchResultItem): IMarkerWrapper {
        var wrapper = {
            marker: new L.Marker(new L.LatLng(value.lat, value.lon), { icon: this.icons.default, draggable: true }),
            data: value
        };

        wrapper.marker.on('click',() => this.selectedMarkerWrapper(wrapper));
        wrapper.marker.on('dragstart',() => this.selectedMarkerWrapper(wrapper));
        return wrapper;
    }
}

export = ClientAddressViewModel