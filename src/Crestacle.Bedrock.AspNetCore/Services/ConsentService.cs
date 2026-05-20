using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Crestacle.Bedrock.Core.Interfaces.Services;

namespace Crestacle.Bedrock.AspNetCore.Services;

internal sealed class ConsentService : IConsentService
{
    private readonly IConsentRepository _consentRepo;
    private readonly IBedrockUnitOfWork _unitOfWork;

    public ConsentService(IConsentRepository consentRepo, IBedrockUnitOfWork unitOfWork)
    {
        _consentRepo = consentRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task RecordConsentAsync(
        Guid userId,
        string policyType,
        string policyVersion,
        string ipAddress,
        string userAgent,
        string? tenantId = null,
        CancellationToken ct = default)
    {
        var record = ConsentRecord.Create(userId, policyType, policyVersion, ipAddress, userAgent, tenantId);
        await _consentRepo.AddAsync(record, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public Task<IReadOnlyList<ConsentRecord>> GetConsentHistoryAsync(Guid userId, CancellationToken ct = default)
        => _consentRepo.GetForUserAsync(userId, ct);
}
