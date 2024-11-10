using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller 
{
    private const string SessionKey = "SimulatorState";
    
    public IActionResult Index()
    {
        var model = GetOrCreateSimulator();
        return View(model);
    }
    
    [HttpPost]
    public IActionResult Simulate(
        SimulatorViewModel model, 
        string operation, 
        string source, 
        string destination, 
        string xchgSource,
        string xchgDestination,
        string pushSource,
        string popDestination,
        string addressingMode)
    {
        try 
        {
            if (!model.ValidateAllInputs())
            {
                ModelState.AddModelError("", "All values must be valid 4-digit hexadecimal numbers");
                return View("Index", model);
            }
            
            switch (operation?.ToUpper())
            {
                case "MOV":
                    if (!string.IsNullOrEmpty(addressingMode))
                    {
                        var effectiveAddress = model.CalculateEffectiveAddress(addressingMode);
                        model.MoveToMemory(source, effectiveAddress);
                    }
                    else
                    {
                        model.ExecuteMov(source, destination);
                    }
                    break;
                    
                case "XCHG":
                    if (!string.IsNullOrEmpty(xchgSource) && !string.IsNullOrEmpty(xchgDestination))
                    {
                        model.ExecuteXchg(xchgSource, xchgDestination);
                    }
                    break;
                    
                case "PUSH":
                    if (!string.IsNullOrEmpty(pushSource))
                    {
                        model.ExecutePush(pushSource);
                    }
                    break;
                    
                case "POP":
                    if (!string.IsNullOrEmpty(popDestination))
                    {
                        model.ExecutePop(popDestination);
                    }
                    break;

                default:
                    ModelState.AddModelError("", "Invalid operation specified");
                    break;
            }

            SaveSimulatorState(model);
            return View("Index", model);
        }
        catch (Exception ex)
        {
            // Add error handling
            ModelState.AddModelError("", $"An error occurred: {ex.Message}");
            return View("Index", model);
        }
    }

    private SimulatorViewModel GetOrCreateSimulator()
    {
        try
        {
            var jsonData = HttpContext.Session.GetString(SessionKey);
            if (string.IsNullOrEmpty(jsonData))
            {
                return new SimulatorViewModel();
            }
            
            var model = System.Text.Json.JsonSerializer.Deserialize<SimulatorViewModel>(jsonData);
            return model ?? new SimulatorViewModel();
        }
        catch (Exception)
        {
            // If there's any error reading the session, return a new instance
            return new SimulatorViewModel();
        }
    }

    private void SaveSimulatorState(SimulatorViewModel model)
    {
        try
        {
            var jsonData = System.Text.Json.JsonSerializer.Serialize(model);
            HttpContext.Session.SetString(SessionKey, jsonData);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Failed to save simulator state: {ex.Message}");
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

// Error view model class
public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}