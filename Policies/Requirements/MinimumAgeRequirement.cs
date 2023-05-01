using Microsoft.AspNetCore.Authorization;

namespace DemoIdentity.Policies.Requirements;

// requirement là tham số đầu vào của policy
// policy có thể có nhiều tham số đầu vào
public class MinimumAgeRequirement : IAuthorizationRequirement
{
    public MinimumAgeRequirement(int minimumAge) =>
        MinimumAge = minimumAge;

    public int MinimumAge { get; }
}