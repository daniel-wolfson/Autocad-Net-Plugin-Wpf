var map = L.map('map', {
    zoom: 15,
    minZoom: 14,
    center: new L.latLng([41.898699, 12.472915]),
    layers: L.tileLayer('http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png')
}),
		loader = L.DomUtil.get('loader');

L.layerJSON({
    url: 'http://overpass-api.de/api/interpreter?data=[out:json];node({lat1},{lon1},{lat2},{lon2})[amenity=bar];out;',
    propertyItems: 'elements',
    propertyTitle: 'tags.name',
    propertyLoc: ['lat', 'lon'],
    buildIcon: function (data, title) {

        return new L.Icon({
            iconUrl: 'bar.png',
            iconSize: new L.Point(32, 37),
            iconAnchor: new L.Point(18, 37),
            popupAnchor: new L.Point(0, -37)
        });
    },
    buildPopup: function (data, marker) {
        return data.tags.name || null;
    }
})
.on('dataloading', function (e) {
    loader.style.display = 'block';
})
.on('dataloaded', function (e) {
    loader.style.display = 'none';
})
.addTo(map);