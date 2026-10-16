namespace OrgMgmt.UITests.Infrastructure;

/// <summary>
/// All UI tests share one hosted application and one database, so they must not run in
/// parallel with each other.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class UiCollection : ICollectionFixture<WebAppFixture>
{
    public const string Name = "OrgMgmt UI";
}
