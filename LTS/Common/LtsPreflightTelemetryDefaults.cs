using Core.Common.Enums;

namespace LTS.Common
{
    public static class LtsPreflightTelemetryDefaults
    {
        public const double FullFuelPercentage = 100.0;
        public const double FullAmmoPercentage = 100.0;

        public static double DefaultAmmoPercentage(UAVType uavType)
        {
            return uavType == UAVType.Armed ? FullAmmoPercentage : 0.0;
        }
    }
}
