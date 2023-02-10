Blazor Input Tag

Use like that:

    <ChipWrapper Chips="@Chips" Color="#46978f"/>

In your _Host.cshtml or Index.html file:

    <link href="_content/SSS.InputTag/SSS.InputTag.bundle.scp.css" rel="stylesheet"/>                                           

@Color is the parameter to set outline color of tag

@code{

    public List<string> Chips { get; set; } = new List<string>();    

}
