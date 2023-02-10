Blazor Input Mask

Blazor Input Mask (based on https://imask.js.org/)

Use like that:

    <EditForm Model="user"

    <InputMask @ref="maskPhoneNumber" @bind-Value="user.PhoneNumber" class="form-control" data-mask="00.00.00.00.00" placeholder="Phone Number" ReturnRawValue="true" />

    </EditForm>


In your _Host.cshtml or Index.html file:

    <script src="_content/SSS.InputMask/js/main.js"></script><br/>

    <script src="_content/SSS.InputMask/js/IMask.js"></script><br/><br/>
                                               
Usage : (RegEx must start and end with a slash '/')
                                               
    <InputMask @ref="maskPhoneNumber" @bind-Value="user.PhoneNumber" class="form-control" data-mask="/^\d+$/" placeholder="Phone Number" ReturnRawValue="true" />

Get raw value:

@code{

    private InputMask maskPhoneNumber { get; set; }

    private void GetRawValue()
    {
        var rawValue = maskPhoneNumber.ReturnValue;
    }
}
