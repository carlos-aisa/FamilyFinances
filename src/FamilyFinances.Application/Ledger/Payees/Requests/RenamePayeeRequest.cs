using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFinances.Application.Ledger.Payees.Requests
{
    public sealed record RenamePayeeRequest(string Name);
}
