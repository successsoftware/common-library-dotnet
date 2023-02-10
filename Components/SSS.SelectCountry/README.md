Blazor SelectCountry

@using Newtonsoft.Json

Use like that:

    <SelectCountry OnChange="@OnChange" CountryName="@CountryName"></SelectCountry>


In your _Host.cshtml or Index.html file:

    <script src="_content/SSS.SelectCountry/js/countrySelect.js"></script>

    <script src="_content/SSS.SelectCountry/js/customCountrySelect.js"></script>  


@code{

    public class SelectCountryModel
    {
        public string Name { get; set; }
        public string Iso2 { get; set; }
    }
    
    public string CountryName { get; set; } = "Vietnam (Việt Nam)";

    private void OnChange(string value)
    {
        var data = value;

        var result = JsonConvert.DeserializeObject<SelectCountryModel>(data);

        if (!string.IsNullOrEmpty(result.Name))
        {
            this.CountryName = result.Name;
        }
    }

}