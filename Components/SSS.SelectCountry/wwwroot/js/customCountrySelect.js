function initialCountryPicker() {
    $("#country_selector").countrySelect({
        responsiveDropdown: true
    });
}

function getSelectCountryData() {
    const myData = $("#country_selector").countrySelect("getSelectedCountryData");
    return JSON.stringify(myData);
}

function setSelectedCountry(countryName) {
    $("#country_selector").countrySelect("setCountry", countryName);
}