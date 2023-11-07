using CMS.DataEngine;
using CMS.Membership;
using Kentico.Community.Portal.Core.Operations;

namespace Kentico.Community.Portal.Web.Features.Members;

public record MemberByIDQuery(int MemberID) : IQuery<MemberInfo?>, ICacheByValueQuery
{
    public string CacheValueKey => MemberID.ToString();
}

public class MemberByIDQueryHandler : DataItemQueryHandler<MemberByIDQuery, MemberInfo?>
{
    private readonly IInfoProvider<MemberInfo> provider;

    public MemberByIDQueryHandler(DataItemQueryTools tools, IInfoProvider<MemberInfo> provider) : base(tools) => this.provider = provider;

    public override async Task<MemberInfo?> Handle(MemberByIDQuery request, CancellationToken cancellationToken)
    {
        var members = await provider.Get()
            .WhereEquals(nameof(MemberInfo.MemberID), request.MemberID)
            .GetEnumerableTypedResultAsync();

        return members.FirstOrDefault();
    }

    protected override ICacheDependencyKeysBuilder AddDependencyKeys(MemberByIDQuery query, MemberInfo result, ICacheDependencyKeysBuilder builder) =>
        builder.Object(MemberInfo.OBJECT_TYPE, query.MemberID);
}
