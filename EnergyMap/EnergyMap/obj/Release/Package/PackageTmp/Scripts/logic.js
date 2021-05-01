//отображение карты
var mymap = L.map("mapid").setView([57.505, 45.09], 5);

var geojson;


/* Переменные для функций */


//выпадающий список показателей
var selectIndicator = document.querySelector("#indicators");
//максимальное значение показателя
var maxValue = 0;
//минимальное значение показателя
var minValue = 0;
//текущий показатель
var currentIndicator = $("#indicators option:selected").text();
//легенда
var legend = L.control({ position: 'bottomright' });


/* Блок отображения карты */


//добавление границ
L.tileLayer(
    "https://api.mapbox.com/styles/v1/{id}/tiles/{z}/{x}/{y}?access_token={accessToken}",
    {
        attribution:
            'Map data &copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors, Imagery © <a href="https://www.mapbox.com/">Mapbox</a>',
        maxZoom: 18,
        id: "mapbox/streets-v11",
        tileSize: 512,
        opacity: 0.5,
        zoomOffset: -1,
        accessToken:
            "sk.eyJ1Ijoid29ydGhsZXNza25pZ2h0IiwiYSI6ImNrbjF0ZXpjdzExM3Aydm8zcnVndXRjbzQifQ.612ET2bEsuRe9EqrxkryaA",
    }
).addTo(mymap);

//первичное создание карты
geojson = new L.geoJson(bounds, {
    style: initialStyle,
    onEachFeature: onEachFeature,
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

//добавить на все страны возможность наведения и клика
function onEachFeature(feature, layer) {
    layer.on({
        mouseover: highlightFeature,
        mouseout: resetHighlight,
        click: zoomToFeature,
    });
}


/* Наведение на регион */


//смена цвета при наведении
function highlightFeature(e) {
    //получить наведенную область
    var layer = e.target;

    //установить соответствующий стиль
    layer.setStyle({
        weight: 5,
        color: "#666",
        dashArray: "",
        fillOpacity: 0.7,
        opacity: 0.7,
    });

    //поддержка браузеров
    if (!L.Browser.ie && !L.Browser.opera && !L.Browser.edge) {
        layer.bringToFront();
    }

    //обновить окно с информацией о наведенном регионе
    info.update(layer.feature.properties);
}

//убрать эффект наведения
function resetHighlight(e) {
    //если не выбран никакой показатель
    if (currentIndicator == "Выберите показатель") {
        //установить стиль по умолчанию
        geojson.setStyle(initialStyle);
    }
    //выбран какой-то показатель
    else {
        //получить массив значений показателя
        getIndicatorsArray(translateIndicator(currentIndicator));
        geojson.setStyle(style)
    }

    //обновить окно с информацией о наведенном регионе
    info.update();
}


/* Наведение на регион при нажатии */


//приближение при клике
function zoomToFeature(e) {
    mymap.fitBounds(e.target.getBounds());

    //уже выбран какой-то показатель
    if (currentIndicator != "Выберите показатель") {
        var selectedRegion = e.target.feature.properties.VARNAME_1;
        getHighRegions(selectedRegion);
    }
}


/* Поле с информацией о выбранном регионе */


//переменная для поля с информацией о регионе
var info = L.control();

//создание поля
info.onAdd = function (mymap) {
    this._div = L.DomUtil.create("div", "info");
    this.update();
    return this._div;
};

//возвращает значение, если оно есть, или N/A, если null
function CheckInfo(str) {
    if (str != null)
        return str;
    else return "N/A";
}

//обновление поля в зависимости от текущего региона
info.update = function (props) {
    if (currentIndicator != "Выберите показатель") {
        this._div.innerHTML =
            "<h4>Выбранный регион:</h4>" +
            (props
                ? "<b>" +
                props.VARNAME_1 +
            "</b><br />" +
            currentIndicator + ": " + CheckInfo(props[translateIndicator(currentIndicator)])
                : "Наведите курсор на регион");
    } else {
        this._div.innerHTML =
            "<h4>Выбранный регион:</h4>" +
            (props
                ? "<b>" +
            props.VARNAME_1 +
                "</b><br />"
                : "Наведите курсор на регион");
    }
};

//добавить поле к карте
info.addTo(mymap);

//получение показателя по его русскому названию
function translateIndicator(indicator) {
    if (indicator == "Выберите показатель")
        return "default";
    if (indicator == "Объем выработки (млн кВт*ч)")
        return "production_volume";
    if (indicator == "Себестоимость выработки (млн рублей)")
        return "production_price";
    if (indicator == "Объем потребления (млн кВт*ч)")
        return "consumption_volume";
    if (indicator == "Разница выработки и потребления (млн кВт*ч)")
        return "production_consumption_difference";
}


/* Обработка выбора показателя */


//изменение показателя
selectIndicator.addEventListener("change", (event) => {
    mymap.invalidateSize(true);

    //очистить список ближайших по рейтингу регионов
    clearCloseList();

    //получить текущий индикатор
    currentIndicator = $("#indicators option:selected").text();

    if (currentIndicator == "Выберите показатель") {
        geojson.setStyle(initialStyle);
        clearLists();
    }
    else {
        //получить массив значений показателя
        getIndicatorsArray(translateIndicator(currentIndicator));
        geojson.setStyle(style)
    }

    getHighRegions();

    legend.addTo(mymap);
});

//обнулить списки регионов и их соседей
function clearLists() {
    clearTopList();
    clearCloseList();
}

//очистка списка лидеров
function clearTopList() {
    $('.top-row').remove();
    $("#topRegions").append(
        `<tr class='top-row'><td>n</td><td>-----</td><td>-----</td></tr>`
    );
    for (i = 0; i < 3; i++) {
        $("#topRegions").append(
            `<tr class='top-row'><td>${i + 1}</td><td>-----</td><td>-----</td></tr>`
        );
    }
}

//очистка списка соседей
function clearCloseList() {
    $('.close-row').remove();
    for (i = 0; i < 3; i++) {
        $("#closeRegions").append(
            `<tr class='close-row'><td>-----</td><td>-----</td></tr>`
        );
    }
}


/* Цветовая индикация регионов */


//обновление минимального и максимального значений по выбранному показателю
function getIndicatorsArray(indicator) {
    var array = [];

    const data = bounds.features.map((feature) => {
        var value = feature.properties[indicator];
        array.push(value);
        return value;
    });

    // Максимальное и минимальное значения для нахождения пропорции по цветам
    maxValue = arrayMax(array);

    minValue = arrayMin(array);
}

//найти минимальное значение в массиве
function arrayMin(arr) {
    var len = arr.length,
        min = Infinity;
    while (len--) {
        if (arr[len] != null) {
            if (arr[len] < min) {
                min = arr[len];
            }
        }
    }
    return min;
}

//найти максимальное значение в массиве
function arrayMax(arr) {
    var len = arr.length,
        max = -Infinity;
    while (len--) {
        if (arr[len] > max) {
            max = arr[len];
        }
    }
    return max;
}

//получение цвета
function style(feature) {
    return {
        fillColor: getColor(feature.properties[translateIndicator(currentIndicator)]),
        weight: 2,
        opacity: 1,
        color: "white",
        dashArray: "3",
        fillOpacity: 0.7,
    };
}

//функция подбора цвета в зависимости от относительного значения показателя
function getColor(d) {
    return d === maxValue ? "#800026" :
        d > maxValue * 0.9 ? "#BD0026" :
            d == null ? "#343434" :
                d > maxValue * 0.7 ? "#E31A1C" :
                    d > maxValue * 0.6 ? "#FC4E2A" :
                        d > maxValue * 0.5 ? "#FD8D3C" :
                            d > maxValue * 0.4 ? "#FEB24C" :
                                d > maxValue * 0.1 ? "#FED976" :
                                    "#FFEDA0";
}


/* Легенда */


//получение легенды
legend.onAdd = function (map) {
    var div = L.DomUtil.create("div", "info legend"),
        grades = [minValue, maxValue * 0.1, maxValue * 0.2, maxValue * 0.3, maxValue * 0.4, maxValue * 0.5, maxValue * 0.7, maxValue * 0.9, maxValue],
        labels = [];
    console.log("grades = " + grades);

    grades = refineNumbers(grades);

    // loop through our density intervals and generate a label with a colored square for each interval
    for (var i = 0; i < grades.length; i++) {
        div.innerHTML +=
            '<i style="background:' +
            getColor(grades[i] + 1) +
            '"></i> ' +
            grades[i] +
            (grades[i + 1]
                ? "&ndash;" + grades[i + 1] + "<br>"
                : "" + "+" + "<br>");
    }
    div.innerHTML +=
        '<i style="background:' +
        "#606060" +
        '"></i>' + "N/A";

    return div;
};

//уменьшение размеров чисел легенды
function refineNumbers(numbers) {
    //округление
    for (var i = 0; i < numbers.length; i++) {
        numbers[i] = Math.floor(numbers[i] * 100) / 100;
    }

    return numbers;
}


/* Рейтинги регионов */


function getHighRegions(curRegion) {
    array1 = [];

    var t = -1;
    var idplace = 100;

    //заполнить массив списка регионов и их значений
    bounds.features.forEach(function (item, i, arr) {
        t = t + 1;

        var name = item.properties.VARNAME_1;
        var indicator = "";

        for ([key, value] of Object.entries(item.properties)) {
            if (key == translateIndicator(currentIndicator)) {
                indicator = value;
                array1.push({ name: name, value: indicator });
            } else {
            }
        }
    });

    //отсортировать массив регионов по значению показателя
    array1.sort(function (a, b) {
        return a.value - b.value;
    });

    //очистить массив от повторяющихся значений
    var array2 = [];
    for (i = 0; i < array1.length; i++) {
        if (i == 0)
            array2.push(array1[i]);
        else {

            if (array1[i].name != array2[array2.length - 1].name) {
                array2.push(array1[i]);
            }
        }
    }

    //найти заданный регион (если задан)
    if (curRegion) {
        //название региона
        var str = curRegion;
        //найти в массиве
        for (i = 0; i < array2.length; i++) {
            var regionName = array2[i].name;

            if (regionName == str) {
                idplace = i;
                break;
            }
        }
    }

    //получить топ-3 регионов
    var array3 = array2.slice(array2.length - 3, array2.length);

    //удалить старые строки
    $('.top-row').remove();

    //если не выбран показатель, то вывести плейсхолдеры
    if (currentIndicator == "Выберите показатель") {
        $("#topRegions").append(
            `<tr class='top-row'><td>n</td><td>-----</td><td>-----</td></tr>`
        );
        for (i = 0; i < 3; i++) {
            $("#topRegions").append(
                `<tr class='top-row'><td>${i + 1}</td><td>-----</td><td>-----</td></tr>`
            );
        }
    } else {
        //задан выбранный регион
        if (idplace != 100) {
            $("#topRegions").append(
                `<tr class='top-row'><td><b>${array2.length - idplace}</b></td><td><b>${array2[idplace].name}</b></td><td><b>${array2[idplace].value}</b></td></tr>`
            );
        }
        //вывод остальных регионов топа-3
        for (i = 2; i > -1; i--) {
            $("#topRegions").append(
                `<tr class='top-row'><td>${3 - i}</td><td>${array3[i].name}</td><td>${array3[i].value}</td></tr>`
            );
        }
    }

    if ((idplace != 100) && (curRegion)) {
        getCloseRegions(idplace, curRegion, array2);
    }
}

function getCloseRegions(id, regName, array1) {
    //удалить старые строки
    $('.close-row').remove();

    //не выбран никакой показатель
    if (currentIndicator == "Выберите показатель") {
        for (i = 0; i < 3; i++) {
            $("#closeRegions").append(
                `<tr class='close-row'><td>-----</td><td>-----</td></tr>`
            );
        }
    } else {
        theNearRegions = [];

        //валидация
        if (id != (array1.length - 1)) {
            theNearRegions.push({
                name: array1[id - 1].name,
                value: array1[id - 1].value
            });
            theNearRegions.push({
                name: array1[id].name,
                value: array1[id].value
            });
            theNearRegions.push({
                name: array1[id + 1].name,
                value: array1[id + 1].value
            });
        } else {
            theNearRegions.push({
                name: array1[id - 2].name,
                value: array1[id - 2].value
            });
            theNearRegions.push({
                name: array1[id - 1].name,
                value: array1[id - 1].value
            });
            theNearRegions.push({
                name: array1[id].name,
                value: array1[id].value
            });
        }

        for (i = 2; i > -1; i--) {
            if (theNearRegions[i].name == regName) {
                $("#closeRegions").append(
                    `<tr class='close-row'><td><b>${theNearRegions[i].name}</b></td><td><b>${theNearRegions[i].value}</b></td></tr>`
                );
            } else {
                $("#closeRegions").append(
                    `<tr class='close-row'><td>${theNearRegions[i].name}</td><td>${theNearRegions[i].value}</td></tr>`
                );
            }
        }
    }
}