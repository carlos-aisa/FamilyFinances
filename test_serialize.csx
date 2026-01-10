using System.Text.Json;
using FamilyFinances.Application.Ledger.Payees.Requests;

var req = new RenamePayeeRequest(\"AWS\");
var json = JsonSerializer.Serialize(req);
Console.WriteLine(json);
