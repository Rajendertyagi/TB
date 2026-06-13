using System.Collections.Generic;

namespace TB.Services.Interfaces;

public interface IFlagService
{
    bool IsFeatureEnabled(string flagId);
    List<string> GetActiveChromiumSwitches();
    object GetFlagsDataPayload();
}
