//отображение карты
var mymap = L.map("mapid").setView([51.505, -0.09], 5);

var geojson;

//добавление границ
L.tileLayer(
    "https://api.mapbox.com/styles/v1/{id}/tiles/{z}/{x}/{y}?access_token={accessToken}",
    {
        attribution:
            'Map data &copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors, Imagery © <a href="https://www.mapbox.com/">Mapbox</a>',
        maxZoom: 18,
        id: "mapbox/streets-v11",
        tileSize: 512,
        zoomOffset: -1,
        accessToken:
            "sk.eyJ1Ijoid29ydGhsZXNza25pZ2h0IiwiYSI6ImNrbjF0ZXpjdzExM3Aydm8zcnVndXRjbzQifQ.612ET2bEsuRe9EqrxkryaA",
    }
).addTo(mymap);

//первичное создание карты
geojson = new L.geoJson(bounds, {
    style: initialStyle,
    //onEachFeature: onEachFeature,
}).addTo(mymap);

//оформление при первичном отображении карты
function initialStyle(feature) {
    return {
        fillColor: 'gray',
        weight: 2,
        opacity: 1,
        color: "white",
        dashArray: "3",
        fillOpacity: 0.7,
    };
}