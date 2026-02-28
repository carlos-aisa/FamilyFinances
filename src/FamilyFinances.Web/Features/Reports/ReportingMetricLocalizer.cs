using FamilyFinances.Application.Reporting.Semantics;
using Microsoft.Extensions.Localization;

namespace FamilyFinances.Web.Features.Reports;

public static class ReportingMetricLocalizer
{
    public static string GetLabel(IStringLocalizer<SharedResource> localizer, ReportingMetricDefinition definition)
    {
        return GetLabel(localizer, definition.Key);
    }

    public static string GetLabel(IStringLocalizer<SharedResource> localizer, ReportingMetricKey key)
    {
        return key switch
        {
            ReportingMetricKey.AssetBalance => localizer["Metric_AssetBalance"],
            ReportingMetricKey.LiabilityBalance => localizer["Metric_LiabilityBalance"],
            ReportingMetricKey.NetWorth => localizer["Metric_NetWorth"],
            ReportingMetricKey.Income => localizer["Metric_Income"],
            ReportingMetricKey.Expense => localizer["Metric_Expense"],
            ReportingMetricKey.PeriodNetResult => localizer["Metric_PeriodNetResult"],
            ReportingMetricKey.TransactionsCount => localizer["Metric_TransactionsCount"],
            ReportingMetricKey.DeltaVsPreviousMonth => localizer["Metric_DeltaVsPreviousMonth"],
            ReportingMetricKey.DeltaVsYearStart => localizer["Metric_DeltaVsYearStart"],
            _ => key.ToString()
        };
    }
}
