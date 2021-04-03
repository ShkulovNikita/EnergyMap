//отображение карты
var mymap = L.map("mapid").setView([51.505, -0.09], 5);

//первичное создание карты
geojson = new L.geoJson(bounds, {
    style: initialStyle,
    onEachFeature: onEachFeature,
}).addTo(mymap);