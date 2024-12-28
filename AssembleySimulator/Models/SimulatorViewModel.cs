public class SimulatorViewModel
{
    // Memory operations
   
     public void MoveToMemory(string register, string address)
    {
        string registerValue = GetRegisterValue(register);
        Memory[address] = registerValue;
    }

    // Optional: Method to get value from memory
    public string GetMemoryValue(string address)
    {
        return Memory.ContainsKey(address) ? Memory[address] : "0000";
    }

    // Register Properties
    
    public string AX { get; set; } = "0000";
    public string BX { get; set; } = "0000";
    public string CX { get; set; } = "0000";
    public string DX { get; set; } = "0000";
    public string SI { get; set; } = "0000";
    public string DI { get; set; } = "0000";
    public string BP { get; set; } = "0000";
    public string SP { get; set; } = "FFFF";
    public string DISP { get; set; } = "0000";

    public string Offset { get; set; } = "0000";
    public string Address { get; set; } = "0000";



    [System.Text.Json.Serialization.JsonIgnore]
    private Dictionary<string, string> Memory { get; set; } = new Dictionary<string, string>();
    
    [System.Text.Json.Serialization.JsonIgnore]
    private Stack<string> Stack { get; set; } = new Stack<string>();



    public SimulatorViewModel()
    {
        Memory = new Dictionary<string, string>();
        Stack = new Stack<string>();
    }

     public bool ValidateHexInput(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        value = value.ToUpper();
        return value.All(c => "0123456789ABCDEF".Contains(c)) && value.Length <= 4;
    }

    public bool ValidateAllInputs()
    {
        var registers = new[] { AX, BX, CX, DX, SI, DI, BP, SP, Offset };
        return registers.All(r => ValidateHexInput(r?.PadLeft(4, '0')));
    }

    

    public void ExecuteMov(string source, string destination)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(destination)) return;
        
        var sourceValue = GetRegisterValue(source);
        SetRegisterValue(destination, sourceValue);
    }

    public void ExecuteXchg(string reg1, string reg2)
    {
        // Get the property values using reflection
        var property1 = GetType().GetProperty(reg1);
        var property2 = GetType().GetProperty(reg2);

        if (property1 != null && property2 != null)
        {
            // Get current values
            string value1 = (property1.GetValue(this) as string) ?? "0000";
            string value2 = (property2.GetValue(this) as string) ?? "0000";

            // Set new values
            property1.SetValue(this, value2.PadLeft(4, '0'));
            property2.SetValue(this, value1.PadLeft(4, '0'));

            Console.WriteLine($"Exchanged {reg1}({value1}) with {reg2}({value2})");
        }
    }

    // PUSH operation
    public void ExecutePush(string register)
    {
        if (string.IsNullOrEmpty(register)) return;
        
        var value = GetRegisterValue(register);
        Stack.Push(value);
        var decrease = int.Parse(value, System.Globalization.NumberStyles.HexNumber);
        
        var spValue = int.Parse(SP, System.Globalization.NumberStyles.HexNumber);
        SP = (spValue - decrease).ToString("X4");
        Console.WriteLine($"Pushed {value}");
    }

    public void ExecutePop(string register)
{
    if (string.IsNullOrEmpty(register)) return;

    var value = GetRegisterValue(register);
        Stack.Push(value);
        var add = int.Parse(value, System.Globalization.NumberStyles.HexNumber);
        var spValue = int.Parse(SP, System.Globalization.NumberStyles.HexNumber);
        SP = (spValue + add).ToString("X4");
        Console.WriteLine($"Pushed {value}");
}



    
    private string GetRegisterValue(string register)
    {
        if (string.IsNullOrEmpty(register)) return "0000";
        
        return register.ToUpper() switch
        {
            "AX" => AX?.PadLeft(4, '0') ?? "0000",
            "BX" => BX?.PadLeft(4, '0') ?? "0000",
            "CX" => CX?.PadLeft(4, '0') ?? "0000",
            "DX" => DX?.PadLeft(4, '0') ?? "0000",
            "SI" => SI?.PadLeft(4, '0') ?? "0000",
            "DI" => DI?.PadLeft(4, '0') ?? "0000",
            "BP" => BP?.PadLeft(4, '0') ?? "0000",
            "Offset" => Offset?.PadLeft(4, '0') ?? "0000",
            "SP" => SP?.PadLeft(4, '0') ?? "FFFF",
            _ => "0000"
        };
    }

    private void SetRegisterValue(string register, string value)
    {
        if (string.IsNullOrEmpty(register) || string.IsNullOrEmpty(value)) return;
        
        value = value.PadLeft(4, '0').ToUpper();
        switch (register.ToUpper())
        {
            case "AX": AX = value; break;
            case "BX": BX = value; break;
            case "CX": CX = value; break;
            case "DX": DX = value; break;
            case "SI": SI = value; break;
            case "DI": DI = value; break;
            case "BP": BP = value; break;
            case "SP": SP = value; break;
            case "Offset": Offset = value; break;
        }

        
    }

     public string CalculateEffectiveAddress(string addressingMode)
    {
        int result = 0;
        
        // Parse the Offset value
        if (!string.IsNullOrEmpty(Offset) && ValidateHexInput(Offset))
        {
            result += int.Parse(Offset, System.Globalization.NumberStyles.HexNumber);
        }
        
        switch (addressingMode?.ToLower())
        {
            case "base":
                if (!string.IsNullOrEmpty(BX) && ValidateHexInput(BX))
                {
                    result += int.Parse(BX, System.Globalization.NumberStyles.HexNumber);
                }
                break;
            case "index":
                if (!string.IsNullOrEmpty(SI) && ValidateHexInput(SI))
                {
                    result += int.Parse(SI, System.Globalization.NumberStyles.HexNumber);
                }
                break;
            case "baseindex":
                if (!string.IsNullOrEmpty(BX) && ValidateHexInput(BX))
                {
                    result += int.Parse(BX, System.Globalization.NumberStyles.HexNumber);
                }
                if (!string.IsNullOrEmpty(SI) && ValidateHexInput(SI))
                {
                    result += int.Parse(SI, System.Globalization.NumberStyles.HexNumber);
                }
                break;
        }
        
        return result.ToString("X4");
    }
}